using sqoClassLibraryAI0502VariaveisSistema;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using sqoClassLibraryAI0502Biblio;
using sqoClassLibraryAI1151FilaProducao.Estrutura;
using sqoClassLibraryAI0502Message;
using AI1627Common20.TemplateDebugging;
using sqoClassLibraryAI1151FilaProducao.Process;
using sqoClassLibraryAI1151FilaProducao;
using System.Xml.Serialization;
using TemplatesStara.CommonStara;
using AI1627CommonInterface;
using System.Linq;

namespace TemplateStara.Expedicao.BloqueioEstoque
{
    [TemplateDebug("sqoExpedicaoBloqueioEstoque")]
    class sqoExpedicaoBloqueioEstoque : sqoClassProcessMovimentacao
    {
        private sqoClassSetMessageDefaults oClassSetMessageDefaults;
        private sqoClassBloqueioEstoque oClassBloqueioEstoque;

        private int nQtdErros = 0;
        private String sMessage = "Falha na validação de dados";
        private String sDescription = String.Empty;

        private String sUsuario = String.Empty;

        enum Action { Invalid = -1, block, free }

        private Action currentAction = Action.Invalid;

        public override sqoClassMessage Executar(string sAction, string sXmlDados, string sXmlType, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao,
            List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sNivel, string sUsuario, object oObjAux)
        {
            using (sqoClassDbConnection oDBConnection = new sqoClassDbConnection())
            {
                this.Init(sXmlDados, sAction, sUsuario);

                this.Validate();

                this.ProcessBusinessLogic(oDBConnection);
            }

            return oClassSetMessageDefaults.Message;
        }

        private void Init(String sXmlDados, String sAction, String sUsuario)
        {
            this.oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());

            this.oClassBloqueioEstoque = new sqoClassBloqueioEstoque();

            this.oClassBloqueioEstoque = sqoClassBiblioSerDes.DeserializeObject<sqoClassBloqueioEstoque>(sXmlDados);

            Enum.TryParse(sAction, out currentAction);

            this.sUsuario = sUsuario;
        }

        private void Validate()
        {
            if (currentAction.Equals(Action.block))
            {
                if (oClassBloqueioEstoque.StatusMovimentacao != 1)
                {
                    this.nQtdErros++;

                    this.sDescription += this.nQtdErros.ToString() + " - É permitido bloquer somente estoque de materiais com status 1!"
                        + Environment.NewLine;
                }
                if (String.IsNullOrEmpty(this.oClassBloqueioEstoque.Observacao))
                {
                    this.nQtdErros++;

                    this.sDescription += this.nQtdErros.ToString() + " - Na função bloquear o campo observação deve ser preenchido!" + Environment.NewLine;
                }
            }

            else if (this.currentAction.Equals(Action.free))
            {
                if (this.oClassBloqueioEstoque.StatusMovimentacao != 3)
                {
                    this.nQtdErros++;

                    this.sDescription += this.nQtdErros.ToString() + " - É permitido desbloquer somente estoque de materiais com status 3!"
                        + Environment.NewLine;
                }
            }

            this.ValidateMessage();
        }

        private void ValidateMessage()
        {
            if (!String.IsNullOrEmpty(sDescription))
            {
                String sMessageDescription = nQtdErros > 1 ? ("Encontrados " + nQtdErros + " erros!")
                    : ("Encontrado " + nQtdErros + " erro!");

                String sMessageBody = sMessageDescription + Environment.NewLine + sDescription;

                CommonStara.MessageBox(false, this.sMessage, sMessageBody, sqoClassMessage.MessageTypeEnum.ERROR, oClassSetMessageDefaults);

                throw new sqoClassMessageUserException(oClassSetMessageDefaults.Message);
            }
        }

        private void ProcessBusinessLogic(sqoClassDbConnection oDBConnection)
        {
            String sOrigemLancamento = this.currentAction.Equals(Action.free) ? "Liberar Estoque" : "Bloquear Estoque";
            String sMovBloquear = "344";
            String sMovDesbloquear = "343";

            try
            {
                oDBConnection.BeginTransaction();

                List<sqoClassLESExpedicaoVolumePersistence> Volumes = new List<sqoClassLESExpedicaoVolumePersistence>();

                int nStatusNovo = 0;
                String sTipoMovimento = String.Empty;
                int nStatusNovoLes;

                nStatusNovo = (int)(currentAction.Equals(Action.block) ? AI1627CommonInterface.LESStatus.STATUS_MOVIMENTACAO.BLOQUEADO_NO_SAP : AI1627CommonInterface.LESStatus.STATUS_MOVIMENTACAO.LIBERADO);

                sTipoMovimento = currentAction.Equals(Action.block) ? sMovBloquear : sMovDesbloquear;



                long nIdMov = this.ExecuteStoreMovEstoqueUpdateStatus(
                    nStatusNovo
                    , sOrigemLancamento
                    , this.oClassBloqueioEstoque.Local
                    , this.oClassBloqueioEstoque.CodigoProduto
                    , this.oClassBloqueioEstoque.QtdAtual
                    , this.oClassBloqueioEstoque.CodigoRastreabilidade
                    , this.oClassBloqueioEstoque.LocalOrigemMov
                    , this.oClassBloqueioEstoque.StatusMovimentacao
                    );

                if (!String.IsNullOrEmpty(this.oClassBloqueioEstoque.CodigoRastreabilidade))
                {
                    Volumes = sqoClassLESExpedicaoVolumeControlerDB.GetExpedicaoVolumesByOrdemProducao(this.oClassBloqueioEstoque.CodigoRastreabilidade);

                    if (Volumes.Any())
                    {
                        nStatusNovoLes = (int)(currentAction.Equals(Action.block) ? AI1627CommonInterface.LESStatus.STATUS_MOVIMENTACAO.BLOQUEADO_NO_LES : AI1627CommonInterface.LESStatus.STATUS_MOVIMENTACAO.LIBERADO_LES);

                        foreach (var oVolume in Volumes)
                        {
                            if (oVolume.TipoVolume.Equals(EXPEDICAO_VOLUME_TIPO.VOLUME_MATERIAL))
                            {
                                List<sqoClassLESEstoqueAtualPersistence> Estoques =
                                sqoClassLESEstoqueAtualControlerDB.GetLESEstoqueAtual(
                                    null
                                    , null
                                    , oVolume.CodigoVolume
                                    , oVolume.CodigoRastreabilidade
                                    , currentAction.Equals(Action.block) ? AI1627CommonInterface.LESStatus.STATUS_MOVIMENTACAO.LIBERADO_LES : AI1627CommonInterface.LESStatus.STATUS_MOVIMENTACAO.BLOQUEADO_NO_LES
                                    , "> 0"
                                    );

                                foreach (var oEstoque in Estoques)
                                {
                                    this.ExecuteStoreMovEstoqueUpdateStatus(
                                        nStatusNovoLes
                                        , sOrigemLancamento
                                        , oEstoque.Local
                                        , oEstoque.CodigoProduto
                                        , oEstoque.Quantidade
                                        , oEstoque.CodigoRastreabilidade
                                        , oEstoque.LocalOrigemMov
                                        , (int)oEstoque.StatusMovimentacao
                                        );
                                }
                            }
                        }
                    }
                }

                this.GetDataFillPersistence();

                this.ExecuteMMEstoqueSapEnvio(nStatusNovo, nIdMov, sTipoMovimento);


                //else if (this.currentAction.Equals(Action.free))
                //{
                //    String sMensagemStatusMov = "Estoque liberado para utilização";

                //    long nIdMov = this.ExecuteStoreMovEstoqueUpdateStatus(nLiberado, sOrigemLancamento, sMensagemStatusMov);

                //    this.GetDataFillPersistence();

                //    this.ExecuteMMEstoqueSapEnvio(nLiberado, nIdMov, sMovDesbloquear);
                //}

                this.oClassSetMessageDefaults.SetarOk();

                //teste
                ///oDBConnection.Rollback();

                oDBConnection.Commit();

            }
            catch (Exception ex)
            {
                sqoClassMessageUserException oClassMessageUserException =
                   new sqoClassMessageUserException("Error" + Environment.NewLine + ex.Message, ex.InnerException);
                oDBConnection.Rollback();

                throw oClassMessageUserException;
            }
        }

        private long ExecuteStoreMovEstoqueUpdateStatus(
            int nStatusNovo
            , String sOrigemLancamento
            , String sLocal
            , String sCodigoProduto
            , double nQtdAtual
            , String sCodigoRastreabilidade
            , String sLocalOrigemMov
            , int nStatusAntigo)
        {
            using (var oCommand = new sqoCommand(CommandType.StoredProcedure))
            {
                oCommand
                    .SetCommandText("WSQOLSSTOREMOVESTOQUEUPDATESTATUS")
                    .Add("@LOCAL", sLocal, OleDbType.VarChar, 50)
                    .Add("@CODIGO_PRODUTO", sCodigoProduto, OleDbType.VarChar, 50)
                    .Add("@DATA_MOVIMENTACAO", DBNull.Value, OleDbType.DBTimeStamp)
                    .Add("@QUANTIDADE_LANCADA", nQtdAtual, OleDbType.Double)
                    .Add("@USUARIO", this.sUsuario, OleDbType.VarChar, 50)
                    .Add("@CODIGO_RASTREABILIDADE", sCodigoRastreabilidade, OleDbType.VarChar, 50)
                    .Add("@PROCESSAR_VALIDACOES", 1, OleDbType.Boolean)
                    .Add("@ID_MOV_VINCULADA", DBNull.Value, OleDbType.BigInt)
                    .Add("@STATUS_MOVIMENTACAO", nStatusNovo, OleDbType.Integer)
                    .Add("@MENSAGEM_STATUS_MOVIMENTACAO", String.Empty, OleDbType.VarChar, 100)
                    .Add("@LOCAL_ORIGEM", sLocalOrigemMov, OleDbType.VarChar, 50)
                    .Add("@OK_OUT", 1, OleDbType.Boolean)
                    .Add("@MESSAGE_OUT", String.Empty, OleDbType.VarChar, 500)
                    .Add("@ID_MOV_OUT", DBNull.Value, OleDbType.BigInt)
                    .Add("@ORIGEM_LANCAMENTO", sOrigemLancamento, OleDbType.VarChar, 50)
                    .Add("@INFO_ORIGEM_LANCAMENTO", nStatusAntigo, OleDbType.VarChar, 50)
                    .Add("@INFO_ORIGEM_LANCAMENTO_2", nStatusNovo, OleDbType.VarChar, 50)
                    .Add("@OBSERVACAO", this.oClassBloqueioEstoque.Observacao, OleDbType.VarChar, 500)
                    .Add("@RETURN_CURSOR", 1, OleDbType.Boolean)
                    .Add("@LOCAL_FILTRO", String.Empty, OleDbType.VarChar, 50)
                    .Add("@TIPO_LOCAL_FILTRO", DBNull.Value, OleDbType.Integer)
                    .Add("@PROCESSAR_INTEGRACAO_ERP", DBNull.Value, OleDbType.Boolean)
                    .Add("@STATUS_MOVIMENTACAO_OUT", DBNull.Value, OleDbType.Integer)
                    .Add("@STATUS_MOVIMENTACAO_ORIGEM", DBNull.Value, OleDbType.Integer)
                    .Add("@PRINTER", String.Empty, OleDbType.VarChar, 50)
                    .Add("@MODULE_VERSION", String.Empty, OleDbType.VarChar, 50)
                    ;

                oCommand.Command.Parameters["@OK_OUT"].Direction = ParameterDirection.Output;
                oCommand.Command.Parameters["@MESSAGE_OUT"].Direction = ParameterDirection.Output;
                oCommand.Command.Parameters["@ID_MOV_OUT"].Direction = ParameterDirection.Output;

                try
                {
                    oCommand.Execute();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Proc: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }

                if ((Boolean)oCommand.Command.Parameters["@OK_OUT"].Value)
                    return (long)oCommand.Command.Parameters["@ID_MOV_OUT"].Value;
                else
                    throw new Exception(oCommand.Command.Parameters["@MESSAGE_OUT"].Value.ToString());
            }
        }

        private void ExecuteMMEstoqueSapEnvio(int nStatusNovo, long nIdMov, String sTipoMovimento)
        {
            using (var oCommand = new sqoCommand(CommandType.StoredProcedure))
            {
                oCommand
                    .SetCommandText("WSQOSAPPROCMMMOVESTOQUESAPENVIO")
                    .Add("@LOCAL", this.oClassBloqueioEstoque.Local, OleDbType.VarChar, 50)
                    .Add("@CODIGO_PRODUTO", this.oClassBloqueioEstoque.CodigoProduto, OleDbType.VarChar, 50)
                    .Add("@ACAO", DBNull.Value, OleDbType.Integer)
                    .Add("@USER_LES", this.sUsuario, OleDbType.VarChar, 50)
                    .Add("@DATA_MOVIMENTACAO", DateTime.Now, OleDbType.DBTimeStamp)
                    .Add("@UNIDADE_NEGOCIO", this.oClassBloqueioEstoque.UnidadeNegocio, OleDbType.VarChar, 50)
                    .Add("@LOCAL_INTEGRACAO", this.oClassBloqueioEstoque.LocalIntegracao, OleDbType.VarChar, 50)
                    .Add("@STATUS_MOVIMENTACAO", nStatusNovo, OleDbType.Integer)
                    .Add("@QUANTIDADE_LANCADA", this.oClassBloqueioEstoque.QtdAtual, OleDbType.Double)
                    .Add("@CENTRO_CUSTOS", String.Empty, OleDbType.VarChar, 50)
                    .Add("@TIPO_INTEGRACAO_ERP", 0, OleDbType.Integer)
                    .Add("@LOCAL_ORIGEM", this.oClassBloqueioEstoque.LocalOrigemMov, OleDbType.VarChar, 50)
                    .Add("@STATUS_MOVIMENTACAO_ORIGEM", this.oClassBloqueioEstoque.StatusMovimentacao, OleDbType.Integer)
                    .Add("@CODIGO_RASTREABILIDADE", this.oClassBloqueioEstoque.CodigoRastreabilidade, OleDbType.VarChar, 50)
                    .Add("@CARREGAR_DADOS_LOCAL", 0, OleDbType.Boolean)
                    .Add("@PROCESSED", 1, OleDbType.Boolean)
                    .Add("@MESSAGE", String.Empty, OleDbType.VarChar, 500)
                    .Add("@UNIDADE_NEGOCIO_ORIGEM", this.oClassBloqueioEstoque.UnidadeNegocio, OleDbType.VarChar, 50)
                    .Add("@LOCAL_ORIGEM_INTEGRACAO", this.oClassBloqueioEstoque.LocalIntegracao, OleDbType.VarChar, 50)
                    .Add("@ID_MOV", nIdMov, OleDbType.BigInt)
                    .Add("@SOMENTE_REGISTRO_AUXILIAR", 0, OleDbType.Boolean)
                    .Add("@REGISTRO_AUXILIAR_TIPO_FLUTUANTE", DBNull.Value, OleDbType.Integer)
                    .Add("@ID_MOV_OUT", DBNull.Value, OleDbType.BigInt)
                    .Add("@QUANTIDADE_ATUAL", DBNull.Value, OleDbType.Double)
                    .Add("@USUARIO", String.Empty, OleDbType.VarChar, 50)
                    .Add("@MENSAGEM_STATUS_MOVIMENTACAO", String.Empty, OleDbType.VarChar, 100)
                    .Add("@ID_MOV_VINCULADA", DBNull.Value, OleDbType.BigInt)
                    .Add("@ORIGEM_LANCAMENTO", String.Empty, OleDbType.VarChar, 50)
                    .Add("@INFO_ORIGEM_LANCAMENTO", String.Empty, OleDbType.VarChar, 50)
                    .Add("@INFO_ORIGEM_LANCAMENTO_2", String.Empty, OleDbType.VarChar, 50)
                    .Add("@OBSERVACAO", String.Empty, OleDbType.VarChar, 500)
                    .Add("@VERSAO_MODULO_ULTIMA_MOVIMENTACAO", String.Empty, OleDbType.VarChar, 50)
                    .Add("@LOCAL_ORIGEM_MOV", String.Empty, OleDbType.VarChar, 50)
                    .Add("@NIVEL", DBNull.Value, OleDbType.Integer)
                    .Add("@ID_WSQOLESTOQUEATUAL", DBNull.Value, OleDbType.BigInt)
                    .Add("@SVC_PROCESS_TIME", DBNull.Value, OleDbType.DBTime)
                    .Add("@SVC_PROCESS_KEY", DBNull.Value, OleDbType.Guid)
                    .Add("@MACHINE_NAME", String.Empty, OleDbType.VarChar, 50)
                    .Add("@OS_USER_NAME", String.Empty, OleDbType.VarChar, 50)
                    .Add("@DOC_REFERENCIA", String.Empty, OleDbType.VarChar, 50)
                    .Add("@LOCAL_ORIGEM_DESTINO", String.Empty, OleDbType.VarChar, 50)
                    .Add("@LOCAL_ORIGEM_NAO_FLUTUANTE", String.Empty, OleDbType.VarChar, 50)
                    .Add("@CODIGO_RASTREABILIDADE_REGISTRO_AUXILIAR", String.Empty, OleDbType.VarChar, 50)
                    .Add("@REGISTRO_AUXILIAR_TIPO_CREDITO", DBNull.Value, OleDbType.Integer)
                    .Add("@REGISTRO_AUXILIAR_TIPO_DEBITO", DBNull.Value, OleDbType.Integer)
                    .Add("@GERAR_REGISTRO_AUXILIAR", 0, OleDbType.Integer)
                    .Add("@CODIGO_MOV", sTipoMovimento, OleDbType.VarChar, 50)
                    ;

                //oCommand.Command.Parameters["@PROCESSED"].Direction = ParameterDirection.Output;
                //oCommand.Command.Parameters["@MESSAGE"].Direction = ParameterDirection.Output;

                try
                {
                    oCommand.Execute();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Proc: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }


            }
        }

        private void GetDataFillPersistence()
        {
            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@UNIDADE_NEGOCIO", String.Empty, OleDbType.VarChar, 50)
                    .Add("@LOCAL_INTEGRACAO", String.Empty, OleDbType.VarChar, 50)
                    .Add("@CODIGO", this.oClassBloqueioEstoque.LocalOrigemMov, OleDbType.VarChar, 50)
                    ;

                String sQuery = @"SELECT 
                                    @UNIDADE_NEGOCIO = UNIDADE_NEGOCIO 
                                	,@LOCAL_INTEGRACAO = LOCAL_INTEGRACAO 
                                FROM 
                                	WSQOLLOCAIS
                                WHERE CODIGO = @CODIGO";

                oCommand.Command.Parameters["@UNIDADE_NEGOCIO"].Direction = ParameterDirection.Output;
                oCommand.Command.Parameters["@LOCAL_INTEGRACAO"].Direction = ParameterDirection.Output;

                try
                {
                    oCommand.SetCommandText(sQuery);

                    oCommand.Execute();

                    this.oClassBloqueioEstoque.UnidadeNegocio = oCommand.Command.Parameters["@UNIDADE_NEGOCIO"].Value.ToString();
                    this.oClassBloqueioEstoque.LocalIntegracao = oCommand.Command.Parameters["@LOCAL_INTEGRACAO"].Value.ToString();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }
            }
        }

    }

    /// <summary>
    /// Classe para instanciar o modelo do criteria da tela.
    /// </summary>
    [XmlRoot("ItemFilaProducao")]
    public class sqoClassBloqueioEstoque
    {
        [XmlElement("LOCAL_ORIGEM_MOV")]
        public string LocalOrigemMov { get; set; }

        [XmlElement("LOCAL")]
        public string Local { get; set; }

        [XmlElement("CODIGO_RASTREABILIDADE")]
        public string CodigoRastreabilidade { get; set; }

        [XmlElement("CODIGO_PRODUTO")]
        public string CodigoProduto { get; set; }

        [XmlElement("ORDEM_CLIENTE")]
        public string OrdemCliente { get; set; }

        [XmlElement("QUANTIDADE_ATUAL")]
        public double QtdAtual { get; set; }

        [XmlElement("STATUS_MOVIMENTACAO")]
        public int StatusMovimentacao { get; set; }

        [XmlElement("OBSERVACAO")]
        public String Observacao { get; set; }

        public String LocalIntegracao { get; set; }

        public String UnidadeNegocio { get; set; }
    }


}
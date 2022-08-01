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
using TemplatesStara.CommonStara;
using AI1627CommonInterface;
using System.Linq;
using Common.Stara.MES.OrdemProducao.DataModel;
using Common.Stara.MES.OrdemProducao.Business;

namespace TemplateStara.Expedicao.GeracaoVolume
{
    [TemplateDebug("sqoExpedicaoGeracaoVolume")]
    class sqoExpedicaoGeracaoVolume : sqoClassProcessMovimentacao
    {
        private sqoClassSetMessageDefaults oClassSetMessageDefaults;
        private sqoClassGeracaoVolume oClassGeracaoVolume;

        private List<sqoClassLESExpedicaoVolumePersistence> oListVolume;
        private List<sqoClassPcp2PecaVolumePersistence> oListVolumeCad;
        private List<sqoClassLESExpedicaoVolumePersistence> oListVol;
        //private sqoClassLESExpedicaoVolumePersistence oVolume;

        private sqoClassParametrosEstrutura oImpressora;
        private sqoClassParametrosEstrutura oOrdemProducao;
        private sqoClassParametrosEstrutura oTipoExpedicao;
        private sqoClassParametrosEstrutura oDeposito;
        private sqoClassParametrosEstrutura oFilePath;

        //private List<sqoClassVolume> oClassVolume;

        enum Action { Invalid = -1, reverse, print, generate, DownloadPDF }
        private Action currentAction = Action.Invalid;

        private int nQtdErros = 0;
        private String sMessage = "Falha na validação de dados";
        private String sDescription = String.Empty;
        private String sUsuario = String.Empty;
        private int nQtdVolGerados = 0;
        private int nQtdVolCad = 0;
        //private int nQtdVolCadastro = 0;

        public override sqoClassMessage Executar(string sAction, string sXmlDados, string sXmlType, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao,
            List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sNivel, string sUsuario, object oObjAux)
        {
            using (sqoClassDbConnection oDBConnection = new sqoClassDbConnection())
            {
                this.Init(oListaParametrosListagem, oListaParametrosMovimentacao, sXmlDados, sAction, sUsuario, oDBConnection);

                this.Validate();

                this.ProcessBusinessLogic(oDBConnection);
            }

            return oClassSetMessageDefaults.Message;
        }

        private void Init(List<sqoClassParametrosEstrutura> oListaParametrosListagem, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao,
            String sXmlDados, String sAction, String sUsuario, sqoClassDbConnection oDBConnection)
        {
            this.oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());

            this.oClassGeracaoVolume = new sqoClassGeracaoVolume();

            this.oClassGeracaoVolume = sqoClassBiblioSerDes.DeserializeObject<sqoClassGeracaoVolume>(sXmlDados);

            Enum.TryParse(sAction, out this.currentAction);

            this.oImpressora = oListaParametrosListagem.Find(x => x.Campo == ("IMPRESSORA"));
            this.oOrdemProducao = oListaParametrosListagem.Find(x => x.Campo == ("ORDEM_PRODUCAO"));
            this.oTipoExpedicao = oListaParametrosListagem.Find(x => x.Campo == ("TIPO_EXPEDICAO"));
            this.oDeposito = oListaParametrosListagem.Find(x => x.Campo == ("DEPOSITO"));

            this.oFilePath = oListaParametrosMovimentacao.Find(x => x.Campo == ("FilePath"));

            this.oClassGeracaoVolume.LocalMacro = sqoClassLESLocaisControlerDB.GetLocalMacro(this.oDeposito.Valor);

            this.oListVolume = sqoClassLESExpedicaoVolumeControlerDB.GetExpedicaoVolumesByOrdemProducao(this.oOrdemProducao.Valor, oDBConnection);

            this.oListVolumeCad = new List<sqoClassPcp2PecaVolumePersistence>();

            this.oListVolumeCad = sqoClassLESExpedicaoVolumeCadastroControlerDB.GetPcp2PecaVolumeByCodigoPaiAndTipoVolume
                (this.oClassGeracaoVolume.CodigoPai, this.oTipoExpedicao.Valor, null);

            //nQtdVolCadastro = this.oListVolumeCad.Count();

            nQtdVolGerados = this.oListVolume.Count();

            //if (nQtdVolGerados > 1)
            //    this.oListVolume = this.oListVolume.OrderBy(x => x.CodigoRastreabilidade).ToList();


            //this.GetVolume();

            this.sUsuario = sUsuario;

            //this.nQtdVolCad = this.oListVolumeCad.Count();
            foreach (var oItem in oListVolumeCad)
                for (int i = 0; i < oItem.Quantidade; i++)
                    this.nQtdVolCad++;
        }

        private void Validate()
        {
            if (this.oClassGeracaoVolume.ImprimirEtiqueta.Equals(true) || this.currentAction.Equals(Action.print))
            {
                if (String.IsNullOrEmpty(this.oImpressora.Valor))
                {
                    this.nQtdErros++;

                    this.sDescription += this.nQtdErros.ToString() + " - Parâmetro Impressora é obrigatório, favor preencher!"
                        + Environment.NewLine;
                }

                if (this.currentAction.Equals(Action.print))
                {
                    if (String.IsNullOrEmpty(this.oClassGeracaoVolume.CodigoRastreabilidade))
                    {
                        this.nQtdErros++;

                        this.sDescription += this.nQtdErros.ToString() + " - Não código de rastreabilidade gerado para o volume "
                            + this.oClassGeracaoVolume.CodigoVolume + "!" + Environment.NewLine;
                    }
                }
            }

            if (this.currentAction.Equals(Action.generate))
            {
                if (String.IsNullOrEmpty(this.oOrdemProducao.Valor))
                {
                    this.nQtdErros++;

                    this.sDescription += this.nQtdErros.ToString() + " - Parâmetro Ordem de Produção é obrigatório! Favor preencher!" + Environment.NewLine;
                }
                else
                {
                    List<SeqProducao> OrdensProducaoList = SeqProducaoBusinesss.GetSeqProducaoByOrdemProducao(this.oOrdemProducao.Valor);

                    if (OrdensProducaoList.Any())
                    {
                        foreach (var oOrdemProducao in OrdensProducaoList)
                        {
                            if (this.oClassGeracaoVolume.CodigoPai != oOrdemProducao.CodigoProduto)
                            {
                                this.nQtdErros++;

                                this.sDescription += this.nQtdErros.ToString() + " - Código Pai (" + this.oClassGeracaoVolume.CodigoPai + ") listado no criteria não pertence a Ordem de Producao ("
                                    + this.oOrdemProducao.Valor + ")!" + Environment.NewLine;
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Lista de ordens de produção vazia OrdensProducaoList");
                    }
                }

                if (nQtdVolGerados > 0)
                {
                    int nQtdContador = 0;

                    foreach (var oItem in oListVolume)
                    {
                        if ((this.oListVolumeCad.FindAll(x => x.CodigoVolume == oItem.CodigoVolume).Count() > 0)
                            && oItem.Status != AI1627CommonInterface.LESStatus.STATUS_EXPEDICAO_VOLUME.ESTORNADO)
                        {
                            nQtdContador++;

                            if (nQtdContador.Equals(nQtdVolCad))
                            {
                                this.nQtdErros++;

                                this.sDescription += this.nQtdErros.ToString() + " - Todos os volumes cadastrados para o Material "
                                    + this.oClassGeracaoVolume.CodigoPai + " e vinculados a Ordem de Produção " + this.oOrdemProducao.Valor
                                    + " já foram gerados!" + Environment.NewLine;
                            }
                        }

                        else if (oListVolumeCad == null || oListVolumeCad.Count == 0)
                        {
                            this.nQtdErros++;
                            //cadasto nao encontrado
                            this.sDescription += this.nQtdErros.ToString() + " - Cadasto nao encontrado!"
                                  + Environment.NewLine;
                        }

                    }

                }
                if (this.nQtdVolGerados == 0)
                {
                    if (this.oListVolumeCad.FindAll(x => x.CodigoPai == x.CodigoVolume).Count() == 0)
                    {
                        this.nQtdErros++;

                        this.sDescription += this.nQtdErros.ToString() + " - Código volume: " + this.oClassGeracaoVolume.CodigoPai
                            + " ainda não gerado e não foi encontrado na lista de geração!" + Environment.NewLine;
                    }
                }
            }

            this.ValidateMessage();
        }

        private void ProcessBusinessLogic(sqoClassDbConnection oDBConnection)
        {

            //TemplatesStara.

            try
            {
                oDBConnection.BeginTransaction();

                bool libStatus;
                String sLocal = String.Empty;

                if (currentAction.Equals(Action.generate))
                {
                    int nQtdEtiqueta = nQtdVolGerados;

                    int nSeqRastreabilidade = 0;

                    if (nQtdVolGerados > 1)
                    {
                        nSeqRastreabilidade = nQtdVolGerados - 1; //TOTAL DA LISTA MENOS A MAQUINA BASE
                    }

                    List<sqoClassLESEstoqueAtualPersistence> oEstoques =
                        sqoClassLESEstoqueAtualControlerDB.GetLESEstoqueAtual(
                            null,
                            null,
                            this.oClassGeracaoVolume.CodigoPai,
                            this.oOrdemProducao.Valor,
                            null,
                            " > 0");

                    if (oEstoques.Any())
                    {
                        libStatus = true;

                        sLocal = oEstoques[0].Local;
                    }

                    else
                    {
                        libStatus = false;

                        sLocal = this.oClassGeracaoVolume.LocalMacro;
                    }

                    foreach (var oItem in oListVolumeCad)
                    {
                        int qtd = this.oListVolume.FindAll(x => x.CodigoVolume == oItem.CodigoVolume).Count();

                        //Verificar se existem volumes gerados conforme o cadastro
                        if (!(qtd >= oItem.Quantidade))
                        {
                            for (int i = qtd; i < oItem.Quantidade; i++)
                            {
                                String sCodigoRastreabilidade = String.Empty;

                                if (oItem.CodigoPai.Equals(oItem.CodigoVolume))
                                    sCodigoRastreabilidade = this.oOrdemProducao.Valor;
                                else
                                {
                                    nSeqRastreabilidade++;

                                    sCodigoRastreabilidade = String.Concat("&", this.oOrdemProducao.Valor, ".", nSeqRastreabilidade);
                                }

                                nQtdEtiqueta++;

                                this.InsertVolume
                                    (oItem.CodigoVolume
                                    , sCodigoRastreabilidade
                                    , oItem.CodigoPai.Equals(oItem.CodigoVolume) ? EXPEDICAO_VOLUME_TIPO.MATERIAL : EXPEDICAO_VOLUME_TIPO.VOLUME_MATERIAL
                                    , nQtdEtiqueta
                                    , oItem.TipoExpedicao
                                    );


                                if (oItem.CodigoPai != oItem.CodigoVolume)
                                    this.CreditoEstoqueVolume(oItem.CodigoPai, oItem.CodigoVolume, sCodigoRastreabilidade, libStatus, sLocal);


                                if (oClassGeracaoVolume.ImprimirEtiqueta)
                                {
                                    this.Print(oItem.CodigoPai, oItem.CodigoVolume, oItem.DescricaoPai, nQtdEtiqueta.ToString(), sCodigoRastreabilidade,
                                            oItem.DescricaoVolume);
                                }
                            }
                        }
                        else
                        {
                            sqoClassLESExpedicaoVolumePersistence oListResult = new sqoClassLESExpedicaoVolumePersistence();

                            oListResult = this.oListVolume.Find(x => x.CodigoVolume == oItem.CodigoVolume);

                            if (oListResult.Status == AI1627CommonInterface.LESStatus.STATUS_EXPEDICAO_VOLUME.ESTORNADO)
                            {
                                sqoClassDefaultPersistence oResult =
                                AI1627CommonInterface.sqoClassLESExpedicaoVolumeControlerDB.ExecuteWsqolUpdateExpedicaoVolume
                                    (oListResult.Id
                                    , oListResult.CodigoVolume
                                    , AI1627CommonInterface.LESStatus.STATUS_EXPEDICAO_VOLUME.CRIADO
                                    );

                                if (!oResult.Ok)
                                    throw new Exception("Erro na atualização de status " + oResult.Message);
                                //oListResult.Status = AI1627CommonInterface.LESStatus.STATUS_EXPEDICAO_VOLUME.CRIADO;

                                //oListResult.Update();

                                //List<sqoClassLESEstoqueAtualPersistence> oEstoque = sqoClassLESEstoqueAtualControlerDB.GetLESEstoqueAtual
                                //    (null
                                //    , null
                                //    , oListResult.CodigoVolume
                                //    , oListResult.CodigoRastreabilidade
                                //    , null
                                //    , " > 0 "
                                //    , null
                                //    , true
                                //    , oDBConnection
                                //    );

                                if (!(oEstoques.Any()))
                                {
                                    this.CreditoEstoqueVolume(oItem.CodigoPai, oListResult.CodigoVolume, oListResult.CodigoRastreabilidade, libStatus, sLocal);
                                }

                                if (oClassGeracaoVolume.ImprimirEtiqueta)
                                {
                                    this.Print(oItem.CodigoPai, oListResult.CodigoVolume, oItem.DescricaoPai, nQtdEtiqueta.ToString(), oListResult.CodigoRastreabilidade,
                                            oItem.DescricaoVolume);
                                }

                            }
                        }
                    }
                }

                else if (currentAction.Equals(Action.print))
                {
                    this.oListVol = sqoClassLESExpedicaoVolumeControlerDB.GetExpedicaoVolumesByCodigoRastreabilidade(this.oClassGeracaoVolume.CodigoRastreabilidade, oDBConnection);

                    foreach (var oVol in oListVol)
                    {

                        this.Print(
                            this.oClassGeracaoVolume.CodigoPai
                            , oVol.CodigoVolume
                            , this.oClassGeracaoVolume.DescricaoPai
                            , oVol.Contador.ToString()
                            , oVol.CodigoRastreabilidade
                            , this.oClassGeracaoVolume.DescricaoVolume);
                    }

                }

                this.oClassSetMessageDefaults.SetarOk();

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

        private void Print(String sCodigoPai, String sCodigoVolume, String sDescricaoPai, String sContador, String sCodigoRastreabilidade,
            String sDescricaoVolume)
        {
            this.GetDataEtiqueta(this.oOrdemProducao.Valor);

            using (var oCommand = new sqoCommand(CommandType.StoredProcedure))
            {
                oCommand
                    .SetCommandText("WSQOLEXPEDICAOPRINTVOLUME")
                    .Add("@ID_PRINTER", long.Parse(oImpressora.Valor), OleDbType.BigInt)
                    .Add("@MODULO", sCodigoPai.Equals(sCodigoVolume) ? MODULO.ExpedicaoPA : MODULO.ExpedicaoVolume, OleDbType.VarChar, 50)
                    .Add("@MATERIAL", sCodigoPai, OleDbType.VarChar, 50)
                    .Add("@DESCRICAO_MATERIAL", sDescricaoPai, OleDbType.VarChar, 100)
                    .Add("@N_SERIE", this.oClassGeracaoVolume.SerialNumber, OleDbType.VarChar, 50)
                    .Add("@ORDEM_VENDA", this.oClassGeracaoVolume.OrdemVenda, OleDbType.VarChar, 50)
                    .Add("@TIPO_PRODUCAO", this.oClassGeracaoVolume.TipoProducao, OleDbType.VarChar, 20)
                    .Add("@USUARIO", this.sUsuario, OleDbType.VarChar, 50)
                    .Add("@CONTADOR", sContador, OleDbType.VarChar, 10)
                    .Add("@COD_RAST_PA", sCodigoPai.Equals(sCodigoVolume) ? sCodigoRastreabilidade : String.Empty, OleDbType.VarChar, 50)
                    .Add("@COD_RAST_VOL_PA", sCodigoPai.Equals(sCodigoVolume) ? String.Empty : sCodigoRastreabilidade, OleDbType.VarChar, 50)
                    .Add("@VOLUME", sCodigoPai.Equals(sCodigoVolume) ? String.Empty : sCodigoVolume, OleDbType.VarChar, 50)
                    .Add("@DESCRICAO_VOLUME", sCodigoPai.Equals(sCodigoVolume) ? String.Empty : sDescricaoVolume, OleDbType.VarChar, 100)
                    ;

                try
                {
                    oCommand.Execute();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }
            }
        }

        private void InsertVolume(String sCodigoVolume, String sCodigoRastreabilidade, EXPEDICAO_VOLUME_TIPO TipoVolume, int nQtdEtiqueta, String sTipoExpedicao)
        {
            sqoClassLESExpedicaoVolumePersistence oVolume = new sqoClassLESExpedicaoVolumePersistence();

            oVolume.IdRemessa = 0;
            oVolume.IdDocumentoTransporte = 0;
            oVolume.CodigoRemessa = String.Empty;
            oVolume.CodigoVolume = sCodigoVolume;
            oVolume.CodigoRastreabilidade = sCodigoRastreabilidade;
            oVolume.OrdemProducao = this.oOrdemProducao.Valor;
            oVolume.TipoVolume = TipoVolume;
            oVolume.PesoTotalCalculado = 0;
            oVolume.PesoTotalVolume = 0;
            oVolume.ChaveExpedicaoPlanejamento = String.Empty;
            oVolume.LocalEntrega = String.Empty;
            oVolume.TipoPlanejamentoLes = EXPEDICAO_TIPO_PLANEJAMENTO_LES.NAO_PLANEJADO;
            oVolume.DataSeparacaoPlanejamento = null;
            oVolume.DataTransportePlanejamento = null;
            oVolume.ObrigarSequenciaPlanejamento = 0;
            oVolume.DocTransporteAuxPlanejamento = String.Empty;
            oVolume.TamCaminhaoPlanejamento = String.Empty;
            oVolume.DetalhePlanejamento = String.Empty;
            oVolume.AgrupamentoLogistico = Guid.Empty;
            oVolume.AgrupamentoCarregamento = Guid.Empty;
            oVolume.Modulo = "WEB";
            oVolume.Status = AI1627CommonInterface.LESStatus.STATUS_EXPEDICAO_VOLUME.CRIADO;
            oVolume.UsuarioUltimaMovimentacao = this.sUsuario;
            oVolume.DataUltimaMovimentacao = DateTime.Now;
            oVolume.Observacao = "VOLUME GERADO VIA WEB";
            oVolume.Contador = nQtdEtiqueta;
            oVolume.TipoExpedicao = sTipoExpedicao;

            oVolume.Insert();
        }

        private void CreditoEstoqueVolume(String sCodigoPai, String sCodigoVolume, String sCodigoRastreabilidade, bool libStatus, String sLocal)
        {
            sqoClassDefaultPersistence oResult = this.ExecStoreMovEstoqueCredito(
                sLocal
                , sCodigoVolume
                , sCodigoRastreabilidade
                , (libStatus == true) ? ESTOQUE_STATUS.LiberadoLES : ESTOQUE_STATUS.BloqueadoLES
                , sCodigoPai);

            if (oResult.Ok.Equals(false))
            {
                throw new Exception(oResult.Message);
            }
        }

        private void ValidateMessage()
        {
            if (!String.IsNullOrEmpty(sDescription))
            {
                String sMessageDescription = nQtdErros > 1 ? ("Encontrados " + nQtdErros + " erros!")
                    : ("Encontrado " + nQtdErros + " erro!");

                String sMessageBody = sMessageDescription + Environment.NewLine + sDescription;

                TemplatesStara.CommonStara.CommonStara.MessageBox(false, this.sMessage, sMessageBody, sqoClassMessage.MessageTypeEnum.ERROR, oClassSetMessageDefaults);

                throw new sqoClassMessageUserException(oClassSetMessageDefaults.Message);
            }
        }

        //SE NECESSÁRIO ALTERAR ESTE METÓDO USAR O PADRÃO DA COMMON PARA MOVIMENTAÇÃO
        private sqoClassDefaultPersistence ExecStoreMovEstoqueCredito(String sLocal, String sCodigoProduto, String sCodigoRastreabilidade, int nStatus,
            String sCodigoPai)
        {
            String nResult = String.Empty;
            sqoClassDefaultPersistence oResult = new sqoClassDefaultPersistence();

            using (var oCommand = new sqoCommand(CommandType.StoredProcedure))
            {
                oCommand
                    .SetCommandText("WSQOLSSTOREMOVESTOQUECREDITO")
                    .Add("@LOCAL", sLocal, OleDbType.VarChar, 50)
                    .Add("@CODIGO_PRODUTO", sCodigoProduto, OleDbType.VarChar, 50)
                    .Add("@DATA_MOVIMENTACAO", DBNull.Value, OleDbType.DBTimeStamp)
                    .Add("@QUANTIDADE_LANCADA", 1, OleDbType.Double)
                    .Add("@USUARIO", this.sUsuario, OleDbType.VarChar, 50)
                    .Add("@CODIGO_RASTREABILIDADE", sCodigoRastreabilidade)
                    .Add("@PROCESSAR_VALIDACOES", 1, OleDbType.Boolean)
                    .Add("@ID_MOV_VINCULADA", DBNull.Value, OleDbType.BigInt)
                    .Add("@STATUS_MOVIMENTACAO", nStatus, OleDbType.Integer)
                    .Add("@MENSAGEM_STATUS_MOVIMENTACAO", String.Empty, OleDbType.VarChar, 100)
                    .Add("@LOCAL_ORIGEM", String.Empty, OleDbType.VarChar, 50)
                    .Add("@OK_OUT", 1, OleDbType.Boolean)
                    .Add("@MESSAGE_OUT", String.Empty, OleDbType.VarChar, 500)
                    .Add("@ID_MOV_OUT", 0, OleDbType.BigInt)
                    .Add("@ORIGEM_LANCAMENTO", this.oOrdemProducao.Valor, OleDbType.VarChar, 50)
                    .Add("@INFO_ORIGEM_LANCAMENTO", sCodigoPai, OleDbType.VarChar, 50)
                    .Add("@INFO_ORIGEM_LANCAMENTO_2", sCodigoProduto, OleDbType.VarChar, 50)
                    .Add("@OBSERVACAO", "Geração volume Web - " + sCodigoPai + " / " + sCodigoProduto)
                    .Add("@RETURN_CURSOR", 1, OleDbType.Boolean)
                    .Add("@LOCAL_FILTRO", String.Empty, OleDbType.VarChar, 50)
                    .Add("@TIPO_LOCAL_FILTRO", 0, OleDbType.Integer)
                    .Add("@PROCESSAR_INTEGRACAO_ERP", 1, OleDbType.Boolean)
                    .Add("@STATUS_MOVIMENTACAO_OUT", 1, OleDbType.Integer)
                    .Add("@STATUS_MOVIMENTACAO_ORIGEM", 1, OleDbType.Integer)
                    .Add("@PRINTER", String.Empty, OleDbType.VarChar, 50)
                    .Add("@MODULE_VERSION", String.Empty, OleDbType.VarChar, 50)
                    .Add("@DOC_REFERENCIA", String.Empty, OleDbType.VarChar, 50)
                    .Add("@REST_GENERIC_PARAM", String.Empty, OleDbType.VarChar, 200)
                    .Add("@GERAR_REGISTRO_AUXILIAR", 1, OleDbType.Integer)
                    ;

                oCommand.Command.Parameters["@OK_OUT"].Direction = ParameterDirection.Output;
                oCommand.Command.Parameters["@MESSAGE_OUT"].Direction = ParameterDirection.Output;
                oCommand.Command.Parameters["@ID_MOV_OUT"].Direction = ParameterDirection.Output;
                //oCommand.Command.Parameters["@STATUS_MOVIMENTACAO_OUT"].Direction = ParameterDirection.Output;

                try
                {
                    oCommand.Execute();

                    //oResult = oCommand.GetResultado<sqoClassDefaultPersistence>();

                    //if ((Boolean)oCommand.Command.Parameters["@OK_OUT"].Value)
                    //{
                    //nResult = (long)oCommand.Command.Parameters["@ID_MOV_OUT"].Value;
                    oResult.Ok = (Boolean)oCommand.Command.Parameters["@OK_OUT"].Value;
                    oResult.Message = oCommand.Command.Parameters["@MESSAGE_OUT"].Value.ToString();
                    //}

                    AI1627CommonInterface.Controller.oLogFile.LogVerboseQueryResult(oCommand, oResult.Ok.ToString() + " - " + oResult.Message.ToString());
                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Proc: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }

                return oResult;
            }
        }

        private void GetDataEtiqueta(String sOrdemProducao)
        {
            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@NUMERO_SERIE", String.Empty, OleDbType.VarChar, 50)
                    .Add("@ORDEM_VENDA", String.Empty, OleDbType.VarChar, 50)
                    .Add("@TIPO_PRODUCAO", String.Empty, OleDbType.VarChar, 10)
                    .Add("@ORDEM_PRODUCAO", sOrdemProducao, OleDbType.VarChar, 50)
                    ;

                String sQuery = @"DECLARE @REGRA VARCHAR(50) = '7003'
                                SELECT
                                    TOP 1
                                	@NUMERO_SERIE = ISNULL(NUMERO_SERIE_ERP,'')
                                	,@ORDEM_VENDA = ISNULL(ATRIBUTO,'')  
                                	,@TIPO_PRODUCAO =  CASE
                                		WHEN ATRIBUTO <> '' THEN 'MTO'
                                		ELSE 'MTS' END
                                FROM
                                	WSQOPCP2SEQPRODUCAO AS SEQ
                                INNER JOIN
                                	WSQOPCP2SEQPRODUCAOREGRAS AS SEQ_REG
                                ON
                                	SEQ.ID = SEQ_REG.ID_SEQ
                                WHERE
                                	ORDEM_PRODUCAO = @ORDEM_PRODUCAO 
                                AND
                                	REGRA = @REGRA";

                oCommand.Command.Parameters["@NUMERO_SERIE"].Direction = ParameterDirection.Output;
                oCommand.Command.Parameters["@ORDEM_VENDA"].Direction = ParameterDirection.Output;
                oCommand.Command.Parameters["@TIPO_PRODUCAO"].Direction = ParameterDirection.Output;

                try
                {
                    oCommand.SetCommandText(sQuery);

                    oCommand.Execute();

                    this.oClassGeracaoVolume.SerialNumber = oCommand.Command.Parameters["@NUMERO_SERIE"].Value.ToString();
                    this.oClassGeracaoVolume.OrdemVenda = oCommand.Command.Parameters["@ORDEM_VENDA"].Value.ToString();
                    this.oClassGeracaoVolume.TipoProducao = oCommand.Command.Parameters["@TIPO_PRODUCAO"].Value.ToString();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }
            }
        }
    }
}
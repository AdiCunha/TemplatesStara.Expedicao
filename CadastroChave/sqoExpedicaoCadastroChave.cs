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

namespace sqoTraceabilityStation
{
    [TemplateDebug("sqoExpedicaoCadastroChave")]
   public class sqoExpedicaoCadastroChave : sqoClassProcessMovimentacao
    {
        private String sUser;
        private Action currentAction = Action.Invalid;
        private sqoExpedicaoChave oCadastroChave;
        private sqoClassSetMessageDefaults oClassSetMessageDefaults;
        private sqoClassChaveExpedicao oChave;
        private int nTipoExpedicao = 0;
        private int nQtdErros = 0;
        private String sMessage = "Falha na validação de dados";
        private String sDescription = String.Empty;

        enum Action { Invalid = -1, Insert, Update, Duplicate, Delivery, Delete }

        public override sqoClassMessage Executar(string sAction, string sXmlDados, string sXmlType, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao,
            List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sNivel, string sUsuario, object oObjAux)
        {
            using (sqoClassDbConnection oDBConnection = new sqoClassDbConnection())
            {
                this.Init(sXmlDados, sUsuario, oListaParametrosListagem, sAction);

                this.Validate();

                this.ProcessBussinessLogic(oDBConnection, oClassSetMessageDefaults);
            }

            return oClassSetMessageDefaults.Message;
        }

        private void Init(string sXmlDados, string sUsuario, List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sAction)
        {
            this.oCadastroChave = new sqoExpedicaoChave();

            this.oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());

            this.oCadastroChave = sqoClassBiblioSerDes.DeserializeObject<sqoExpedicaoChave>(sXmlDados);

            this.sUser = sUsuario;

            Enum.TryParse(sAction, out currentAction);

            nTipoExpedicao = this.SumTipoExpedicaoCodigo(nTipoExpedicao);

            if (this.currentAction.Equals(Action.Insert))
            {
                this.FillPersistence();
            }

            UpperCaseData();
        }

        private int SumTipoExpedicaoCodigo(int nTipoExpedicao)
        {

            if (oCadastroChave.Separacao)
            {
                nTipoExpedicao += 1;
            }

            if (oCadastroChave.Entrega)
            {
                nTipoExpedicao += 2;
            }

            if (oCadastroChave.Carregamento)
            {
                nTipoExpedicao += 4;
            }

            return nTipoExpedicao;
        }

        private void FillPersistence()
        {
            oCadastroChave.Chave = oCadastroChave.ChaveInsert;
            oCadastroChave.Descricao = oCadastroChave.DescricaoInsert;
            oCadastroChave.Deposito = oCadastroChave.DepositoInsert;
            oCadastroChave.TipoExpedicao = nTipoExpedicao;
            oCadastroChave.Equipe = oCadastroChave.EquipeInsert;
            oCadastroChave.LeituraCodigoVolume = oCadastroChave.LeituraCodigoVolumeInsert;
            oCadastroChave.LeituraLocalDestino = oCadastroChave.LeituraLocalDestinoInsert;
            oCadastroChave.LeituraChaveNotaFiscal = oCadastroChave.LeituraChaveNotaFiscalInsert;
            oCadastroChave.LeituraRastreabilidadeComponente = oCadastroChave.LeituraRastreabilidadeComponenteInsert;
            oCadastroChave.AgrupamentoDocTransporte = oCadastroChave.AgrupamentoDocTransporteInsert;
            oCadastroChave.NotificacaoCliente = oCadastroChave.NotificacaoClienteInsert;
            oCadastroChave.EtiquetaSequenciaCarregamento = oCadastroChave.EtiquetaSequenciaCarregamentoInsert;
            oCadastroChave.QuestionarioExpedicao = oCadastroChave.QuestionarioExpedicaoInsert;

        }

        private void UpperCaseData()
        {
            oCadastroChave.Chave = oCadastroChave.Chave.ToUpper();
            oCadastroChave.Descricao = oCadastroChave.Descricao.ToUpper();
        }

        private void Validate()
        {
            ValidateForm();

            if (String.IsNullOrEmpty(sDescription))
            {
                if (currentAction.Equals(Action.Insert) || currentAction.Equals(Action.Duplicate))
                    ValidateInsert();

                else if (currentAction.Equals(Action.Update))
                    ValidateUpdate();
            }

            ValidateMessage();
        }

        private void ValidateForm()
        {
            if (String.IsNullOrEmpty(oCadastroChave.Chave))
            {
                nQtdErros++;

                sDescription += nQtdErros.ToString() + " - Campo \"Chave\" é obrigatório, favor preencher!" + Environment.NewLine;
            }

            if (String.IsNullOrEmpty(oCadastroChave.Descricao))
            {
                nQtdErros++;

                sDescription += nQtdErros.ToString() + " - Campo \"Descrição\" é obrigatório, favor preencher!" + Environment.NewLine;
            }

            if (String.IsNullOrEmpty(oCadastroChave.Deposito))
            {
                nQtdErros++;

                sDescription += nQtdErros.ToString() + " - Campo \"Depósito\" é obrigatório, favor preencher!" + Environment.NewLine;
            }

        }

        private void ValidateInsert()
        {
            if (ExistChave(oCadastroChave.Chave))
            {
                nQtdErros++;

                sDescription += nQtdErros.ToString() + " - Cadastro inválido, Chave: " + oCadastroChave.Chave + " já cadastrado na base de dados!" + Environment.NewLine;
            }

        }

        private void ValidateUpdate()
        {

            var oChaveExpedicaoPercistence = new List<sqoClassChaveExpedicao>();

            var oListChaveExpedicao = this.GetFillChave(oCadastroChave.Id);

            if (this.EqualsChave(oListChaveExpedicao))
            {
                nQtdErros++;

                sDescription += nQtdErros.ToString() + " - Alteração inválida, pelo menos 1 campo deve ser alterado" + Environment.NewLine;
            }

            oChave = null;

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

        private bool ExistChave(String sChave)
        {
            bool Result = true;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@CHAVE", sChave, OleDbType.VarChar, 50)
                    ;

                String sQuery = @"SELECT 1 FROM WSQOLEXPEDICAOCHAVE WHERE CHAVE = @CHAVE";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.GetResultado();

                    if (oResult == null)
                        Result = false;
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }

            return Result;
        }

        internal sqoClassChaveExpedicao GetFillChave(long nIdChave)
        {
            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .SetCommandText(@"SELECT  
                                          ID
                                        , CHAVE
                                        , DESCRICAO
                                        , TIPO_EXPEDICAO
                                        , QUESTIONARIO_EXPEDICAO
                                        , EQUIPE
                                        , LEITURA_CODIGO_VOLUME
                                        , LEITURA_LOCAL_DESTINO
                                        , LEITURA_CHAVE_NOTA_FISCAL
                                        , LEITURA_RASTREABILIDADE_COMPONENTE
                                        , AGRUPAMENTO_DOC_TRANSPORTE
                                        , NOTIFICACAO_CLIENTE
                                        , DEPOSITO
                                        , ETIQUETA_SEQUENCIA_CARREGAMENTO
                                        , OBSERVACAO
                                        , ATIVO

                                    FROM
                                        [WSQOLEXPEDICAOCHAVE]
                                    WHERE
                                        ID = @ID_CHAVE")

                        .Add("@ID_CHAVE", nIdChave, OleDbType.Integer)
                        ;

                try
                {

                    var oResult = oCommand.GetResultado<sqoClassChaveExpedicao>();

                    return oResult;
                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }
            }
        }

        private sqoClassMessage ProcessBussinessLogic(sqoClassDbConnection oDBConnection, sqoClassSetMessageDefaults oClassSetMessageDefaults)
        {
            String sMessage = String.Empty;

            try
            {
                oDBConnection.BeginTransaction();

                if (currentAction.Equals(Action.Insert) || currentAction.Equals(Action.Duplicate))
                {
                    SaveInsert();

                    sMessage = "Dados Inseridos com Sucesso";
                }

                else if (currentAction.Equals(Action.Update))
                {
                    SaveUpdate();

                    sMessage = "Dados Alterados com Sucesso";
                }

                CommonStara.MessageBox(true, sMessage, "", sqoClassMessage.MessageTypeEnum.OK, oClassSetMessageDefaults);

                oDBConnection.Commit();
            }
            catch (Exception ex)
            {
                sqoClassMessageUserException oClassMessageUserException =
                   new sqoClassMessageUserException("Error" + Environment.NewLine + ex.Message, ex.InnerException);
                oDBConnection.Rollback();

                throw oClassMessageUserException;
            }

            return oClassSetMessageDefaults.Message;
        }

        private void SaveInsert()
        {
            long nIdChave = SaveChave();
        }

        private void SaveUpdate()
        {
            UpdateChave();
        }

        private long SaveChave()
        {
            long nResult = -1;
            int nAtivo = 1;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {

                DateTime dDataHoraAtual = DateTime.Now;
                oCommand
                    .Add("@CHAVE", oCadastroChave.Chave, OleDbType.VarChar, 50)
                    .Add("@DESCRICAO", oCadastroChave.Descricao, OleDbType.VarChar, 50)
                    .Add("@TIPO_EXPEDICAO", nTipoExpedicao, OleDbType.Integer)
                    .Add("@QUESTIONARIO_EXPEDICAO", oCadastroChave.QuestionarioExpedicao, OleDbType.Integer)
                    .Add("@EQUIPE", oCadastroChave.Equipe, OleDbType.Integer)
                    .Add("@LEITURA_CODIGO_VOLUME", oCadastroChave.LeituraCodigoVolume, OleDbType.Integer)
                    .Add("@LEITURA_LOCAL_DESTINO", oCadastroChave.LeituraLocalDestino, OleDbType.Integer)
                    .Add("@LEITURA_CHAVE_NOTA_FISCAL", oCadastroChave.LeituraChaveNotaFiscal, OleDbType.Integer)
                    .Add("@LEITURA_RASTREABILIDADE_COMPONENTE", oCadastroChave.LeituraRastreabilidadeComponente, OleDbType.Integer)
                    .Add("@AGRUPAMENTO_DOC_TRANSPORTE", oCadastroChave.AgrupamentoDocTransporte, OleDbType.Integer)
                    .Add("@NOTIFICACAO_CLIENTE", oCadastroChave.NotificacaoCliente, OleDbType.Integer)
                    .Add("@DEPOSITO", oCadastroChave.Deposito, OleDbType.VarChar, 50)
                    .Add("@ETIQUETA_SEQUENCIA_CARREGAMENTO", oCadastroChave.EtiquetaSequenciaCarregamento, OleDbType.Integer)
                    .Add("@USUARIO", sUser, OleDbType.VarChar, 50)
                    .Add("@DATA_OCORRENCIA", dDataHoraAtual, OleDbType.DBTimeStamp)
                    .Add("@OBSERVACAO", oCadastroChave.Observacao, OleDbType.VarChar, 250)
                    .Add("@ATIVO", nAtivo, OleDbType.Boolean)
                    ;

                String sQuery = @"DECLARE @ID_CHAVE AS BIGINT = -1

                                INSERT INTO [WSQOLEXPEDICAOCHAVE]
                                           ([CHAVE]
                                           ,[DESCRICAO]
                                           ,[TIPO_EXPEDICAO]
                                           ,[QUESTIONARIO_EXPEDICAO]
                                           ,[EQUIPE]
                                           ,[LEITURA_CODIGO_VOLUME]
                                           ,[LEITURA_LOCAL_DESTINO]
                                           ,[LEITURA_CHAVE_NOTA_FISCAL]
                                           ,[LEITURA_RASTREABILIDADE_COMPONENTE]
                                           ,[AGRUPAMENTO_DOC_TRANSPORTE]
                                           ,[NOTIFICACAO_CLIENTE]
                                           ,[DEPOSITO]
                                           ,[ETIQUETA_SEQUENCIA_CARREGAMENTO]
                                           ,[USUARIO]
                                           ,[DATA_OCORRENCIA]
                                           ,[OBSERVACAO]
                                           ,[ATIVO])
                                     VALUES
                                           (@CHAVE
                                           ,@DESCRICAO
                                           ,@TIPO_EXPEDICAO
                                           ,@QUESTIONARIO_EXPEDICAO
                                           ,@EQUIPE
                                           ,@LEITURA_CODIGO_VOLUME
                                           ,@LEITURA_LOCAL_DESTINO
                                           ,@LEITURA_CHAVE_NOTA_FISCAL
                                           ,@LEITURA_RASTREABILIDADE_COMPONENTE
                                           ,@AGRUPAMENTO_DOC_TRANSPORTE
                                           ,@NOTIFICACAO_CLIENTE
                                           ,@DEPOSITO
                                           ,@ETIQUETA_SEQUENCIA_CARREGAMENTO
                                           ,@USUARIO
                                           ,@DATA_OCORRENCIA
                                           ,@OBSERVACAO
                                           ,@ATIVO);
                                
                                SET @ID_CHAVE = SCOPE_IDENTITY();
                                
                                SELECT @ID_CHAVE;
                                ";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.GetResultado();

                    nResult = (long)oResult;

                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }

                return nResult;
            }
        }

        private void UpdateChave()
        {

            DateTime dDataHoraAtual = DateTime.Now;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@DESCRICAO", oCadastroChave.Descricao, OleDbType.VarChar, 100)
                    .Add("@TIPO_EXPEDICAO", nTipoExpedicao, OleDbType.Integer)
                    .Add("@QUESTIONARIO_EXPEDICAO", oCadastroChave.QuestionarioExpedicao, OleDbType.Integer)
                    .Add("@EQUIPE", oCadastroChave.Equipe, OleDbType.Integer)
                    .Add("@LEITURA_CODIGO_VOLUME", oCadastroChave.LeituraCodigoVolume, OleDbType.Integer)
                    .Add("@LEITURA_LOCAL_DESTINO", oCadastroChave.LeituraLocalDestino, OleDbType.Integer)
                    .Add("@LEITURA_CHAVE_NOTA_FISCAL", oCadastroChave.LeituraChaveNotaFiscal, OleDbType.Integer)
                    .Add("@LEITURA_RASTREABILIDADE_COMPONENTE", oCadastroChave.LeituraRastreabilidadeComponente, OleDbType.Integer)
                    .Add("@AGRUPAMENTO_DOC_TRANSPORTE", oCadastroChave.AgrupamentoDocTransporte, OleDbType.Integer)
                    .Add("@NOTIFICACAO_CLIENTE", oCadastroChave.NotificacaoCliente, OleDbType.Integer)
                    .Add("@DEPOSITO", oCadastroChave.Deposito, OleDbType.VarChar, 50)
                    .Add("@ETIQUETA_SEQUENCIA_CARREGAMENTO", oCadastroChave.EtiquetaSequenciaCarregamento, OleDbType.Integer)
                    .Add("@USUARIO", sUser, OleDbType.VarChar, 50)
                    .Add("@DATA_OCORRENCIA", dDataHoraAtual, OleDbType.DBTimeStamp)
                    .Add("@ATIVO", oCadastroChave.Ativo, OleDbType.Boolean)
                    .Add("@OBSERVACAO", oCadastroChave.Observacao, OleDbType.VarChar, 250)
                    .Add("@ID", oCadastroChave.Id, OleDbType.BigInt)

                    ;

                String sQuery = @"UPDATE [WSQOLEXPEDICAOCHAVE]
                                    SET [DESCRICAO] = @DESCRICAO
                                       ,[TIPO_EXPEDICAO] = @TIPO_EXPEDICAO
                                       ,[QUESTIONARIO_EXPEDICAO] = @QUESTIONARIO_EXPEDICAO
                                       ,[EQUIPE] = @EQUIPE
                                       ,[LEITURA_CODIGO_VOLUME] = @LEITURA_CODIGO_VOLUME
                                       ,[LEITURA_LOCAL_DESTINO] = @LEITURA_LOCAL_DESTINO
                                       ,[LEITURA_CHAVE_NOTA_FISCAL] = @LEITURA_CHAVE_NOTA_FISCAL
                                       ,[LEITURA_RASTREABILIDADE_COMPONENTE] = @LEITURA_RASTREABILIDADE_COMPONENTE
                                       ,[AGRUPAMENTO_DOC_TRANSPORTE] = @AGRUPAMENTO_DOC_TRANSPORTE
                                       ,[NOTIFICACAO_CLIENTE] = @NOTIFICACAO_CLIENTE
                                       ,[DEPOSITO] = @DEPOSITO
                                       ,[ETIQUETA_SEQUENCIA_CARREGAMENTO] = @ETIQUETA_SEQUENCIA_CARREGAMENTO
                                       ,[USUARIO] = @USUARIO
                                       ,[DATA_OCORRENCIA] = @DATA_OCORRENCIA
                                       ,[ATIVO] = @ATIVO
                                       ,[OBSERVACAO] = @OBSERVACAO

	                                WHERE ID = @ID
                                    ";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.GetResultado();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }
        }

        public bool EqualsChave(sqoClassChaveExpedicao oChaveExpedicaoPercistence)
        {
            return (oChaveExpedicaoPercistence.Deposito == oCadastroChave.Deposito
                    && oChaveExpedicaoPercistence.TipoExpedicao == nTipoExpedicao
                    && oChaveExpedicaoPercistence.Equipe == oCadastroChave.Equipe
                    && oChaveExpedicaoPercistence.LeituraCodigoVolume == oCadastroChave.LeituraCodigoVolume
                    && oChaveExpedicaoPercistence.LeituraLocalDestino == oCadastroChave.LeituraLocalDestino
                    && oChaveExpedicaoPercistence.LeituraChaveNotaFiscal == oCadastroChave.LeituraChaveNotaFiscal
                    && oChaveExpedicaoPercistence.LeituraRastreabilidadeComponente == oCadastroChave.LeituraRastreabilidadeComponente
                    && oChaveExpedicaoPercistence.AgrupamentoDocTransporte == oCadastroChave.AgrupamentoDocTransporte
                    && oChaveExpedicaoPercistence.NotificacaoCliente == oCadastroChave.NotificacaoCliente
                    && oChaveExpedicaoPercistence.EtiquetaSequenciaCarregamento == oCadastroChave.EtiquetaSequenciaCarregamento
                    && oChaveExpedicaoPercistence.QuestionarioExpedicao == oCadastroChave.QuestionarioExpedicao
                    && oChaveExpedicaoPercistence.Ativo == oCadastroChave.Ativo
                    && oChaveExpedicaoPercistence.Observacao == oCadastroChave.Observacao);
        }

    }

    [AutoPersistencia]
    public class sqoClassChaveExpedicao
    {
        public long Id { get; set; }

        public string Chave { get; set; }

        public string Descricao { get; set; }

        public string Deposito { get; set; }

        public int TipoExpedicao { get; set; }

        public int Equipe { get; set; }

        public int LeituraCodigoVolume { get; set; }

        public int LeituraLocalDestino { get; set; }

        public int LeituraChaveNotaFiscal { get; set; }

        public int LeituraRastreabilidadeComponente { get; set; }

        public int AgrupamentoDocTransporte { get; set; }

        public int NotificacaoCliente { get; set; }

        public int EtiquetaSequenciaCarregamento { get; set; }

        public int QuestionarioExpedicao { get; set; }

        public bool Ativo { get; set; }

        public string Observacao { get; set; }

        public bool TipoExpedicaoSeparacao { get; set; }

        public bool TipoExpedicaoEntrega { get; set; }

        public bool TipoExpedicaoCarregamento { get; set; }

    }
}

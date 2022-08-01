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
using sqoClassLibraryAI1151FilaProducao.Persistencia;
using sqoClassLibraryAI1151FilaProducao;
using TemplatesStara.CommonStara;
using AI1627Common20.Log;


namespace sqoTraceabilityStation
{
    [TemplateDebug("sqoExpedicaoCadastroChaveTipoExpedicaoMovimentacao")]
    public class sqoExpedicaoCadastroChaveTipoExpedicaoMovimentacao : IProcessMovimentacao
    {
        private sqoClassSetMessageDefaults oClassSetMessageDefaults;
        private DataValidationAddRem oDataValidationAddRem;
        private sqoExpedicaoChave oCadastroChave;
        private int nTipoExpedicaoChave = 0;
        private string sUser;
        private long nIdLocal = -1;

        private int nQtdErros = 0;
        private string sMessage = "Falha na validação de dados";
        private string sDescription = string.Empty;
        private int nTipoExpedicao = 0;
        private ChaveValues oChaveValues;

        enum Action { Invalid = -1, Insert, Update, Duplicate, Delivery }
        private Action currentAction = Action.Invalid;

        public sqoClassMessage Executar(string sAction, string sXmlDados, string sXmlType, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao, List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sNivel, string sUsuario, object oObjAux)
        {
            using (sqoClassDbConnection oDBConnection = new sqoClassDbConnection())
            {
                this.Init(oListaParametrosMovimentacao, sAction, sXmlDados, sUsuario);

                this.nTipoExpedicao = this.SumTipoExpedicaoCodigo();

                this.ProcessBussinessLogic(oDBConnection, oClassSetMessageDefaults, oListaParametrosMovimentacao);
            }

            return this.oClassSetMessageDefaults.Message;
        }

        private void Init(List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao, String sAction, String sXmlDados, String sUsuario)
        {
            this.oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());

            this.oDataValidationAddRem = Tools.ConverterParamToDataValidationAddRem(oListaParametrosMovimentacao);

            this.oCadastroChave = new sqoExpedicaoChave();

            this.oCadastroChave = sqoClassBiblioSerDes.DeserializeObject<sqoExpedicaoChave>(sXmlDados);

            Enum.TryParse(sAction, out this.currentAction);

            this.ValidarSelecaoLinha();

            this.ValidarAcao();

            this.ValidateMessage();

            this.LoadValuesUpdateChave();

            UpperCaseData();

            this.sUser = sUsuario;
        }

        private void UpperCaseData()
        {
            this.oDataValidationAddRem.GetValueAcao(VINCULAR_LOCAL_FIELDS.CODIGO_LOCAL).ToUpper();
        }

        private void Validate()
        {
            if (this.oDataValidationAddRem.Acao.Equals(Acao.Add))
            {
                this.ValidateMessage();
            }
        }

        private ChaveValues LoadValuesUpdateChave()
        {
            oChaveValues = new ChaveValues()
            {
                Id = Convert.ToInt32(oDataValidationAddRem.GetValueAcao(ALTERAR_TIPO_EXPEDICAO.ID)),
                Chave = oDataValidationAddRem.GetValueAcao(ALTERAR_TIPO_EXPEDICAO.CHAVE),
                Entrega = Convert.ToBoolean(oDataValidationAddRem.GetValueAcao(ALTERAR_TIPO_EXPEDICAO.ENTREGA)),
                Separacao = Convert.ToBoolean(oDataValidationAddRem.GetValueAcao(ALTERAR_TIPO_EXPEDICAO.SEPARACAO)),
                Carregamento = Convert.ToBoolean(oDataValidationAddRem.GetValueAcao(ALTERAR_TIPO_EXPEDICAO.CARREGAMENTO)),
            };

            return oChaveValues;
        }


        private bool GetChaveLocal()
        {
            bool Result = false;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@ID_CHAVE", this.oCadastroChave.Id)
                    .Add("@CODIGO_LOCAL", this.oDataValidationAddRem.GetValueAcao(VINCULAR_LOCAL_FIELDS.CODIGO_LOCAL))
                    ;

                string sQuery = @"SELECT 1 
                                  FROM 
                                    WSQOLEXPEDICAOCHAVELOCALENTREGA 
                                  WHERE
                                    ID_CHAVE = @ID_CHAVE
                                  AND
                                    CODIGO_LOCAL = @CODIGO_LOCAL"
                                  ;

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.GetResultado();

                    if (oResult != null)
                        Result = true;
                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }

            return Result;
        }

        private long GetLocal()
        {
            long nResult = -1;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@CODIGO_LOCAL", this.oDataValidationAddRem.GetValueAcao(VINCULAR_LOCAL_FIELDS.CODIGO_LOCAL))
                    ;

                string sQuery = @"SELECT TOP 1 
                                    ID_LOCAL
                                  FROM WSQOLLOCAIS
                                  WHERE
                                    CODIGO = @CODIGO_LOCAL
                                  AND
                                    STATUS_LOCAL = 1";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.GetResultado();

                    if (oResult != null)
                    {
                        nResult = (long)oResult;

                        this.nIdLocal = nResult;
                    }

                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }

            return nResult;
        }

        private void ValidateMessage()
        {
            if (!String.IsNullOrEmpty(sDescription))
            {
                string sMessageDescription = nQtdErros > 1 ? ("Encontrados " + nQtdErros + " erros!")
                    : ("Encontrado " + nQtdErros + " erro!");

                string sMessageBody = sMessageDescription + Environment.NewLine + sDescription;

                CommonStara.MessageBox(false, this.sMessage, sMessageBody, sqoClassMessage.MessageTypeEnum.ERROR, oClassSetMessageDefaults);

                throw new sqoClassMessageUserException(oClassSetMessageDefaults.Message);
            }
        }

        private string ValidarSelecaoLinha()
        {
            if (String.IsNullOrEmpty(oDataValidationAddRem.GetValueAcao(ALTERAR_TIPO_EXPEDICAO.ID)))
            {
                this.nQtdErros++;

                this.sDescription += nQtdErros.ToString() + " - Obrigatório selecionar a linha!";
            }

            return sDescription;
        }

        private string ValidarAcao()
        {
            if (this.oDataValidationAddRem.Acao.Equals(Acao.Remove))
            {
                this.nQtdErros++;

                this.sDescription += nQtdErros.ToString() + " - Função não disponível, para alterar os dados clique em 'adiciona'!";

                this.ValidateMessage();
            }

            return sDescription;
        }

        private void ProcessBussinessLogic(sqoClassDbConnection oDBConnection, sqoClassSetMessageDefaults oClassSetMessageDefaults, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao)
        {
            sqoClassMessage oClassMessage = new sqoClassMessage();

            try
            {
                oDBConnection.BeginTransaction();


                if (this.oDataValidationAddRem.Acao.Equals(Acao.Add))
                {
                    this.UpdateTipoExpedicao(nTipoExpedicao);
                }

                this.FillDataGrid();

                Tools.SincronizarDataValidationAddRemToParam(oListaParametrosMovimentacao, oDataValidationAddRem);

                oClassSetMessageDefaults.SetarOk();

                oClassSetMessageDefaults.Message.Dado = oListaParametrosMovimentacao;

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

        private int SumTipoExpedicaoCodigo()
        {
            if (oChaveValues.Separacao)
            {
                nTipoExpedicaoChave += 1;
            }

            if (oChaveValues.Entrega)
            {
                nTipoExpedicaoChave += 2;
            }

            if (oChaveValues.Carregamento)
            {
                nTipoExpedicaoChave += 4;
            }

            return nTipoExpedicaoChave;
        }

        private void UpdateTipoExpedicao(int nTipoExpedicao)
        {

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand.SetCommandText(@"UPDATE WSQOLEXPEDICAOCHAVE 
	                                        SET TIPO_EXPEDICAO = ? 
	                                        WHERE ID = ?")

                  .Add("@TIPO_EXPEDICAO", nTipoExpedicao, OleDbType.Integer)
                  .Add("@ID_CHAVE", this.oCadastroChave.Id, OleDbType.BigInt);

                try
                {
                    PrintLog.Verbose(oCommand.QueryToString()).Log();
                    oCommand.Execute();
                }

                catch (Exception ex)
                {

                    PrintLog.Error("Query: " + oCommand.QueryToString(), ex).Log();
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Insert: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }

        }

        private void FillDataGrid()
        {
            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@ID", DBNull.Value, OleDbType.BigInt)
                    .Add("@CHAVE", DBNull.Value, OleDbType.VarChar, 50)
                    .Add("@SEPARACAO", DBNull.Value, OleDbType.Integer)
                    .Add("@ENTREGA", DBNull.Value, OleDbType.Integer)
                    .Add("@CARREGAMENTO", DBNull.Value, OleDbType.Integer)
                    .Add("@ID_CHAVE", this.oCadastroChave.Id, OleDbType.BigInt)
                    ;

                string sQuery = @"SELECT
 	                                 ID 
	                                ,CHAVE	
                                    ,CAST(CASE WHEN((TIPO_EXPEDICAO & 1) = 1) THEN 1 ELSE 0 END AS BIT) SEPARACAO
                                    ,CAST(CASE WHEN((TIPO_EXPEDICAO & 2) = 2) THEN 1 ELSE 0 END AS BIT) ENTREGA
                                    ,CAST(CASE WHEN((TIPO_EXPEDICAO & 4) = 4) THEN 1 ELSE 0 END AS BIT) CARREGAMENTO
                                    FROM
	                                WSQOLEXPEDICAOCHAVE
                                 WHERE
	                                ID = @ID_CHAVE";

                oCommand.Command.Parameters["@ID"].Direction = ParameterDirection.Output;
                oCommand.Command.Parameters["@CHAVE"].Direction = ParameterDirection.Output;
                oCommand.Command.Parameters["@SEPARACAO"].Direction = ParameterDirection.Output;
                oCommand.Command.Parameters["@ENTREGA"].Direction = ParameterDirection.Output;
                oCommand.Command.Parameters["@CARREGAMENTO"].Direction = ParameterDirection.Output;

                try
                {
                    oCommand.SetCommandText(sQuery);
                    oDataValidationAddRem.SetValueAcao(this.oDataValidationAddRem.GetValueAcao(ALTERAR_TIPO_EXPEDICAO.ID), oCommand.Command.Parameters["@ID"].Value.ToString());
                    oDataValidationAddRem.SetValueAcao(this.oDataValidationAddRem.GetValueAcao(ALTERAR_TIPO_EXPEDICAO.CHAVE), oCommand.Command.Parameters["@CHAVE"].Value.ToString());
                    oDataValidationAddRem.SetValueAcao(this.oDataValidationAddRem.GetValueAcao(ALTERAR_TIPO_EXPEDICAO.SEPARACAO), oCommand.Command.Parameters["@SEPARACAO"].Value.ToString());
                    oDataValidationAddRem.SetValueAcao(this.oDataValidationAddRem.GetValueAcao(ALTERAR_TIPO_EXPEDICAO.ENTREGA), oCommand.Command.Parameters["@ENTREGA"].Value.ToString());
                    oDataValidationAddRem.SetValueAcao(this.oDataValidationAddRem.GetValueAcao(ALTERAR_TIPO_EXPEDICAO.CARREGAMENTO), oCommand.Command.Parameters["@CARREGAMENTO"].Value.ToString());
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Insert: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }

            }
        }
    }

    public class ALTERAR_TIPO_EXPEDICAO
    {
        public const string ID = "Id";
        public const string CHAVE = "Chave";    
        public const string SEPARACAO = "Separacao";
        public const string ENTREGA = "Entrega";
        public const string CARREGAMENTO = "Carregamento";
    }

    [AutoPersistencia]
    public class ChaveValues
    {
        public int Id { get; set; }

        public string Chave { get; set; }

        public bool Separacao { get; set; }

        public bool Entrega { get; set; }

        public bool Carregamento { get; set; }

    }
}

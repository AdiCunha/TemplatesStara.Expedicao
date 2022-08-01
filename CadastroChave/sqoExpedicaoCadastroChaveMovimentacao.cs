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
using System.Globalization;
using System.Xml.Serialization;
using TemplatesStara.CommonStara;

namespace sqoTraceabilityStation
{
    [TemplateDebug("sqoExpedicaoCadastroChaveMovimentacao")]
    public class sqoExpedicaoCadastroChaveMovimentacao : IProcessMovimentacao
    {
        private sqoClassSetMessageDefaults oClassSetMessageDefaults;
        private DataValidationAddRem oDataValidationAddRem;
        private sqoExpedicaoChave oCadastroChave;

        private string sUser;
        private long nIdLocal = -1;
        private int nQtdErros = 0;
        private string sMessage = "Falha na validação de dados";
        private string sDescription = string.Empty;
        string CodLocal = string.Empty;

        enum Action { Invalid = -1, Insert, Update, Duplicate, Delivery }
        private Action currentAction = Action.Invalid;

        public sqoClassMessage Executar(string sAction, string sXmlDados, string sXmlType, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao, List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sNivel, string sUsuario, object oObjAux)
        {
            using (sqoClassDbConnection oDBConnection = new sqoClassDbConnection())
            {
                this.Init(oListaParametrosMovimentacao, sAction, sXmlDados, sUsuario);

                this.Validate();

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

            UpperCaseData();

            this.sUser = sUsuario;
        }

        private void UpperCaseData()
        {
           this.CodLocal =  this.oDataValidationAddRem.GetValueAcao(VINCULAR_LOCAL_FIELDS.CODIGO_LOCAL).ToUpper();
        }

        private void Validate()
        {
            if (this.oDataValidationAddRem.Acao.Equals(Acao.Add))
            {
                this.ValidateAdd();

                this.ValidateMessage();
            }
        }

        private void ValidateAdd()
        {
            string sDeposito = "";

            if (String.IsNullOrEmpty(this.CodLocal))
            {
                this.nQtdErros++;

                this.sDescription += this.nQtdErros.ToString() + " - Campo Código Local é obrigátorio, favor preencher!" + Environment.NewLine;
            }

            if (!String.IsNullOrEmpty(this.CodLocal))
            {
                if (GetChaveLocal())
                {
                    this.nQtdErros++;

                    this.sDescription += this.nQtdErros.ToString() + " - Cadastro inválido, Local: " + this.oDataValidationAddRem.GetValueAcao(VINCULAR_LOCAL_FIELDS.CODIGO_LOCAL) +
                        " já vinculado a chave: " + this.oCadastroChave.Chave + "!" + Environment.NewLine;
                }

                //else if (String.IsNullOrEmpty(this.GetLocalEntrega(CodLocal)))
                //{
                //    this.nQtdErros++;

                //    this.sDescription += this.nQtdErros.ToString() + " - Cadastro inválido, Local: " + this.oDataValidationAddRem.GetValueAcao(VINCULAR_LOCAL_FIELDS.CODIGO_LOCAL) +
                //        " inativo ou não existe na base de dados!" + Environment.NewLine;
                //}

            }

            sDeposito = this.DepositoExist();

            if (!String.IsNullOrEmpty(sDeposito))
            {
                this.nQtdErros++;

                this.sDescription
                    += this.nQtdErros.ToString()
                    + " - Local do deposito: "
                    + sDeposito
                    + " já cadastrado!"
                    + " Permitido cadastrar somente um local por depósito. "
                    + Environment.NewLine;
            }

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
                                    CODIGO_LOCAL = @CODIGO_LOCAL";

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

        private long GetIdLocalEntrega()
        {
            long nIdLocalEntrega = 0;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@CODIGO_LOCAL", this.oDataValidationAddRem.GetValueAcao(VINCULAR_LOCAL_FIELDS.CODIGO_LOCAL));

                string sQuery = @"SELECT 
		                            ID_LOCAL 
	                              FROM
		                            WSQOLLOCAIS
	                              WHERE
		                            CODIGO = ?";

                oCommand.SetCommandText(sQuery);

                var oResult = oCommand.GetResultado();

                nIdLocalEntrega = (long)oResult;
            }

            return nIdLocalEntrega;
        }

        private string GetLocalEntrega(string CodLocal)
        {
            string sLocalEntrega = "";

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@CODIGO_LOCAL", CodLocal);

                string sQuery = @"SELECT
                                    CODIGO_LOCAL
                                  FROM
                                    WSQOLEXPEDICAOCHAVELOCALENTREGA
                                  WHERE
                                    CODIGO_LOCAL = ?";

                oCommand.SetCommandText(sQuery);

                var oResult = oCommand.GetResultado();

                sLocalEntrega = (string)oResult;
            }

            return sLocalEntrega;
        }

        private string DepositoExist()
        {
            string sDeposito = "";

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@CHAVE", this.oDataValidationAddRem.GetValueAcao(VINCULAR_LOCAL_FIELDS.CHAVE))
                    .Add("@CODIGO_LOCAL", this.oDataValidationAddRem.GetValueAcao(VINCULAR_LOCAL_FIELDS.CODIGO_LOCAL))
                    ;

                string sQuery = @"SELECT
                                      L.LOCAL_INTEGRACAO
                                  FROM
                                      WSQOLLOCAIS AS L
                                  INNER JOIN
                                      (SELECT
                                          DISTINCT LOC.LOCAL_INTEGRACAO
                                       FROM 
                                          WSQOLEXPEDICAOCHAVELOCALENTREGA AS ENTREGA
                                       INNER JOIN
	                                      WSQOLEXPEDICAOCHAVE AS CHAVE
                                       ON 
	                                      CHAVE.ID = ENTREGA.ID_CHAVE
                                       AND 
	                                     CHAVE.CHAVE = ?
                                       INNER JOIN
                                          WSQOLLOCAIS AS LOC
                                       ON
                                          ENTREGA.ID_LOCAL = LOC.ID_LOCAL
                                       ) AS LOC
                                  ON
                                      L.LOCAL_INTEGRACAO = LOC.LOCAL_INTEGRACAO
                                  WHERE
                                      L.CODIGO = ?";

                oCommand.SetCommandText(sQuery);

                var oResult = oCommand.GetResultado();

                sDeposito = (string)oResult;
            }

            return sDeposito;
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

        private void ProcessBussinessLogic(sqoClassDbConnection oDBConnection, sqoClassSetMessageDefaults oClassSetMessageDefaults, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao)
        {
            sqoClassMessage oClassMessage = new sqoClassMessage();

            try
            {
                oDBConnection.BeginTransaction();

                long nIdChaveLocal = -1;

                nIdLocal = this.GetIdLocalEntrega();

                if (this.oDataValidationAddRem.Acao.Equals(Acao.Add))
                {
                    nIdChaveLocal = SaveChaveLocal();

                    this.SaveChaveLocalHist(nIdChaveLocal);
                }
                else if (this.oDataValidationAddRem.Acao.Equals(Acao.Remove))
                {
                    this.SaveChaveLocalHist(long.Parse(this.oDataValidationAddRem.GetValueAcao(VINCULAR_LOCAL_FIELDS.ID)));

                    DeleteChaveLocal();
                }

                this.FillDataGrid(nIdChaveLocal);

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

        private void ValidateDepositoChave()
        {
            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@ID_CHAVE", this.oCadastroChave.Id)
                    .Add("@DEPOSITO", this.oCadastroChave.Deposito)
                    ;

                string sQuery = @"SELECT 
                                    CHAVE.ID 
                                   ,CHAVE.DEPOSITO
                                  FROM WSQOLEXPEDICAOCHAVE AS CHAVE
                                  WHERE
                                    CHAVE.ID = @ID_CHAVE
                                  AND
                                    DEPOSITO = @DEPOSITO";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.GetResultado();

                    if (oResult != null)
                    {
                        this.nQtdErros++;

                        this.sDescription += this.nQtdErros.ToString() + " - Depósito: " +
                        this.oCadastroChave.DepositoInsert + " já cadastrado para a chave: " +
                        this.oCadastroChave.Chave + Environment.NewLine;
                    }

                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }

        }

        private long SaveChaveLocal()
        {
            long nIdChaveLocal = -1;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@ID_CHAVE", this.oCadastroChave.Id)
                    .Add("@ID_LOCAL", this.nIdLocal)
                    .Add("@CODIGO_LOCAL", this.oDataValidationAddRem.GetValueAcao(VINCULAR_LOCAL_FIELDS.CODIGO_LOCAL).ToUpper());

                string sQuery = @"DECLARE @SCOPE_ID AS BIGINT

                                    INSERT INTO [WSQOLEXPEDICAOCHAVELOCALENTREGA]
                                               ([ID_CHAVE]
                                               ,[ID_LOCAL]
                                               ,[CODIGO_LOCAL])
                                         VALUES
                                               (@ID_CHAVE
                                               ,@ID_LOCAL
                                               ,@CODIGO_LOCAL)
		   
                                    SET @SCOPE_ID = SCOPE_IDENTITY()

                                    SELECT @SCOPE_ID";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.GetResultado();

                    if (oResult != null)
                    {
                        nIdChaveLocal = (long)oResult;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Insert: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }

            return nIdChaveLocal;
        }

        private void DeleteChaveLocal()
        {
            long nIdChaveLocal = -1;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@ID", this.oDataValidationAddRem.GetValueAcao(VINCULAR_LOCAL_FIELDS.ID))
                    ;

                string sQuery = @"DELETE FROM [WSQOLEXPEDICAOCHAVELOCALENTREGA]
                                    WHERE ID = @ID";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.GetResultado();

                    nIdChaveLocal = long.Parse(this.oDataValidationAddRem.GetValueAcao(VINCULAR_LOCAL_FIELDS.ID));
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Insert: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }
        }

        private void SaveChaveLocalHist(long nIdChaveLocal)
        {
            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@USUARIO", this.sUser)
                    .Add("@DATA_OCORRENCIA", DateTime.Now)
                    .Add("@OBSERVACAO", this.oDataValidationAddRem.Acao.Equals(Acao.Add) ? "Insert" : "Delete")
                    .Add("@ID", nIdChaveLocal)
                    ;

                string sQuery = @"INSERT INTO [WSQOLEXPEDICAOCHAVELOCALENTREGAHIST]
                                               ([ID_CHAVE_LOCAL]
                                               ,[ID_CHAVE]
                                               ,[ID_LOCAL]
                                               ,[CODIGO_LOCAL]
                                               ,[USUARIO]
                                               ,[DATA_OCORRENCIA]
                                               ,[OBSERVACAO])
                                         SELECT
                                               CHAVE_LOC.ID
                                               ,CHAVE_LOC.ID_CHAVE
                                               ,CHAVE_LOC.ID_LOCAL
                                               ,CHAVE_LOC.CODIGO_LOCAL
                                               ,@USUARIO
                                               ,@DATA_OCORRENCIA
                                               ,@OBSERVACAO
                                    	FROM	
                                    		WSQOLEXPEDICAOCHAVELOCALENTREGA AS CHAVE_LOC
                                    	WHERE
                                    		CHAVE_LOC.ID = @ID";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.GetResultado();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Insert: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }
        }

        private void FillDataGrid(long nIdChaveLocal)
        {
            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@ID", DBNull.Value, OleDbType.BigInt)
                    .Add("@ID_CHAVE", DBNull.Value, OleDbType.BigInt)
                    .Add("@CHAVE", DBNull.Value, OleDbType.VarChar, 50)
                    .Add("@ID_LOCAL", DBNull.Value, OleDbType.BigInt)
                    .Add("@CODIGO_LOCAL", DBNull.Value, OleDbType.VarChar, 50)
                    .Add("@DEPOSITO", DBNull.Value, OleDbType.VarChar, 50)
                    .Add("@ID_CHAVE_LOCAL", nIdChaveLocal, OleDbType.BigInt)
                    ;

                string sQuery = @"SELECT
                                	 @ID = ENTREGA.ID 
                                	,@ID_CHAVE = ENTREGA.ID_CHAVE
                                	,@CHAVE = CHAVE.CHAVE
                                	,@ID_LOCAL = ENTREGA.ID_LOCAL
                                	,@CODIGO_LOCAL = ENTREGA.CODIGO_LOCAL
                                	,@DEPOSITO = LOC.LOCAL_INTEGRACAO
                                FROM 
                                	WSQOLEXPEDICAOCHAVELOCALENTREGA AS ENTREGA
                                INNER JOIN
                                	WSQOLEXPEDICAOCHAVE AS CHAVE
                                ON
                                	ENTREGA.ID_CHAVE = CHAVE.ID
                                INNER JOIN
                                	WSQOLLOCAIS AS LOC
                                ON
                                	ENTREGA.ID_LOCAL = LOC.ID_LOCAL
                                WHERE 
                                    ENTREGA.ID = @ID_CHAVE_LOCAL";

                oCommand.Command.Parameters["@ID"].Direction = ParameterDirection.Output;
                oCommand.Command.Parameters["@ID_CHAVE"].Direction = ParameterDirection.Output;
                oCommand.Command.Parameters["@CHAVE"].Direction = ParameterDirection.Output;
                oCommand.Command.Parameters["@ID_LOCAL"].Direction = ParameterDirection.Output;
                oCommand.Command.Parameters["@CODIGO_LOCAL"].Direction = ParameterDirection.Output;
                oCommand.Command.Parameters["@DEPOSITO"].Direction = ParameterDirection.Output;

                try
                {
                    oCommand.SetCommandText(sQuery);

                    oCommand.Execute();

                    oDataValidationAddRem.SetValueAcao(VINCULAR_LOCAL_FIELDS.ID, oCommand.Command.Parameters["@ID"].Value.ToString());
                    oDataValidationAddRem.SetValueAcao(VINCULAR_LOCAL_FIELDS.ID_CHAVE, oCommand.Command.Parameters["@ID_CHAVE"].Value.ToString());
                    oDataValidationAddRem.SetValueAcao(VINCULAR_LOCAL_FIELDS.CHAVE, oCommand.Command.Parameters["@CHAVE"].Value.ToString());
                    oDataValidationAddRem.SetValueAcao(VINCULAR_LOCAL_FIELDS.ID_LOCAL, oCommand.Command.Parameters["@ID_LOCAL"].Value.ToString());
                    oDataValidationAddRem.SetValueAcao(VINCULAR_LOCAL_FIELDS.CODIGO_LOCAL, oCommand.Command.Parameters["@CODIGO_LOCAL"].Value.ToString());
                    oDataValidationAddRem.SetValueAcao(VINCULAR_LOCAL_FIELDS.DEPOSITO, oCommand.Command.Parameters["@DEPOSITO"].Value.ToString());
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Insert: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }

            }
        }
    }
}
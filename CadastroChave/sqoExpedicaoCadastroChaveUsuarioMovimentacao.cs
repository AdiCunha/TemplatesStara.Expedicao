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

namespace sqoTraceabilityStation
{
    [TemplateDebug("sqoExpedicaoCadastroChaveUsuarioMovimentacao")]
    public class sqoExpedicaoCadastroChaveUsuarioMovimentacao : sqoClassProcessMovimentacao
    {

        private sqoClassSetMessageDefaults oClassSetMessageDefaults;
        private DataValidationAddRem oDataValidationAddRem;
        private sqoExpedicaoChave oCadastroChave;
        private sqoTipoExpedicao oSqoTipoExpedicao;

        private int nQtdErros = 0;
        private string sMessage = "Falha na validação de dados";
        private string sDescription = String.Empty;
        private string sUser = String.Empty;
        private int nTipoExpedicao = 0;
        private string sTipoExpedicao = "";
        private ChaveUsuarioValuesMov oChaveUsuarioValuesMov;

        public override sqoClassMessage Executar(string sAction, string sXmlDados, string sXmlType, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao, List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sNivel, string sUsuario, object oObjAux)
        {
            using (sqoClassDbConnection oDBConnection = new sqoClassDbConnection())
            {
                this.Init(oListaParametrosMovimentacao, sUsuario, sXmlDados);

                this.Validate();

                this.ProcessBussinessLogic(oDBConnection, oListaParametrosMovimentacao);
            }

            return this.oClassSetMessageDefaults.Message;
        }

        private void Init(List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao, String sUsuario, String sXmlDados)
        {
            this.oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());

            this.oDataValidationAddRem = Tools.ConverterParamToDataValidationAddRem(oListaParametrosMovimentacao);

            this.oCadastroChave = new sqoExpedicaoChave();

            this.oCadastroChave = sqoClassBiblioSerDes.DeserializeObject<sqoExpedicaoChave>(sXmlDados);

            this.sUser = sUsuario;
        }

        private void Validate()
        {
            if (this.oDataValidationAddRem.Acao.Equals(Acao.Add))
            {
                this.ValidateAdd();
            }

            else
            {
                this.ValidadeRemove();
            }

            this.ValidateMessage();
        }

        private void ValidateAdd()
        {

            if (String.IsNullOrEmpty(oDataValidationAddRem.GetValueAcao(VINCULAR_USUARIO_FIELDS.ID)))
            {
                this.nQtdErros++;

                this.sDescription += nQtdErros.ToString() + " - Obrigatório selecionar um item da lista " + Environment.NewLine;
            }

            else
            {
                this.LoadValuesUpdateChave();

                this.nTipoExpedicao = this.SumTipoExpedicaoCodigo(nTipoExpedicao);

                this.oSqoTipoExpedicao = this.GetAcessoChave();

                if (String.IsNullOrEmpty(oChaveUsuarioValuesMov.Usuario))
                {
                    this.nQtdErros++;

                    this.sDescription += nQtdErros.ToString() + " - Campo Usuário é obrigatório, favor preencher!" + Environment.NewLine;
                }

                if (this.GetChaveUsuario())
                {
                    this.nQtdErros++;

                    this.sDescription += nQtdErros.ToString() + " - Cadastro inválido, Usuário: " + oChaveUsuarioValuesMov.Usuario
                        + " já vinculado a Chave: " + this.oCadastroChave.Chave + "!" + Environment.NewLine;
                }

                if (!String.IsNullOrEmpty(this.ValidaAcesso()))
                {
                    this.nQtdErros++;

                    this.sDescription += nQtdErros.ToString() + " - Cadastro inválido, Tipo Expedição: " + sTipoExpedicao + "não cadastrado(s) na chave: " + this.oCadastroChave.Chave;
                }

                else if (!this.GetUsuario())
                {
                    this.nQtdErros++;

                    this.sDescription += this.nQtdErros.ToString() + " - Cadastro inválido, Usuário: " + oChaveUsuarioValuesMov.Usuario
                        + " inativo ou não existe na base de dados!" + Environment.NewLine;
                }

            }

        }

        private void ValidadeRemove()
        {

            if (String.IsNullOrEmpty(oDataValidationAddRem.GetValueAcao(VINCULAR_USUARIO_FIELDS.ID)))
            {
                this.nQtdErros++;

                this.sDescription += nQtdErros.ToString() + " - Obrigatório selecionar um item da lista " + Environment.NewLine;
            }

            else
                this.LoadValuesUpdateChave();
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

        private bool GetChaveUsuario()
        {
            bool Result = false;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@ID_CHAVE", this.oCadastroChave.Id, OleDbType.BigInt)
                    .Add("@USUARIO", oChaveUsuarioValuesMov.Usuario, OleDbType.VarChar, 50)
                    ;

                String sQuery = @"SELECT 
                                	1 
                                FROM 
                                	WSQOLEXPEDICAOCHAVEUSUARIO
                                WHERE
                                	ID_CHAVE = @ID_CHAVE
                                AND	USUARIO = @USUARIO";

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

                return Result;
            }
        }

        private int SumTipoExpedicaoCodigo(int nTipoExpedicao)
        {

            if (oChaveUsuarioValuesMov.Separacao)
            {
                nTipoExpedicao += 1;
            }

            if (oChaveUsuarioValuesMov.Entrega)
            {
                nTipoExpedicao += 2;
            }

            if (oChaveUsuarioValuesMov.Carregamento)
            {
                nTipoExpedicao += 4;
            }

            return nTipoExpedicao;
        }

        private string ValidaAcesso()
        {

            if (oChaveUsuarioValuesMov.Separacao && !oSqoTipoExpedicao.Separacao)
            {
                sTipoExpedicao += "SEPARAÇÃO; ";
            }

            if (oChaveUsuarioValuesMov.Entrega && !oSqoTipoExpedicao.Entrega)
            {
                sTipoExpedicao += "ENTREGA; ";
            }

            if (oChaveUsuarioValuesMov.Carregamento && !oSqoTipoExpedicao.Carregamento)
            {
                sTipoExpedicao += "CARREGAMENTO; ";
            }

            return sTipoExpedicao;
        }

        private bool GetUsuario()
        {
            bool Result = false;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@USUARIO", oChaveUsuarioValuesMov.Usuario, OleDbType.VarChar, 50)
                    ;

                String sQuery = @"SELECT 1 FROM WSQOLOGINS WHERE USUARIO = @USUARIO AND HABILITADO = 1";

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

        public sqoTipoExpedicao GetAcessoChave()
        {
            string sQuery = @"SELECT
                                 CAST(CASE WHEN((TIPO_EXPEDICAO & 1) = 1) THEN 1 ELSE 0 END AS BIT) SEPARACAO
                                ,CAST(CASE WHEN((TIPO_EXPEDICAO & 2) = 2) THEN 1 ELSE 0 END AS BIT) ENTREGA
                                ,CAST(CASE WHEN((TIPO_EXPEDICAO & 4) = 4) THEN 1 ELSE 0 END AS BIT) CARREGAMENTO
                                ,CAST(CASE WHEN((TIPO_EXPEDICAO & 8) = 8) THEN 1 ELSE 0 END AS BIT) TRANSPORTE
                            FROM
	                            WSQOLEXPEDICAOCHAVE
                            WHERE
	                            ID = @ID_CHAVE"
                ;

            using (sqoCommand oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                     .Add("@ID_CHAVE", this.oCadastroChave.Id, OleDbType.BigInt)
                     .SetCommandText(sQuery)
                   ;

                var oResult = oCommand.GetResultado<sqoTipoExpedicao>();

                return oResult;
            }
        }

        private void ProcessBussinessLogic(sqoClassDbConnection oDBConnection, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao)
        {
            try
            {
                oDBConnection.BeginTransaction();

                if (this.oDataValidationAddRem.Acao.Equals(Acao.Add))
                {
                    long nIdChaveUsuario = this.SaveChaveUsuario();

                    this.FillDataGrid();
                }

                else if (this.oDataValidationAddRem.Acao.Equals(Acao.Remove))
                {
                    this.RemoveChaveUsuario();
                }

                Tools.SincronizarDataValidationAddRemToParam(oListaParametrosMovimentacao, this.oDataValidationAddRem);

                this.oClassSetMessageDefaults.SetarOk();

                this.oClassSetMessageDefaults.Message.Dado = oListaParametrosMovimentacao;

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

        private long SaveChaveUsuario()
        {
            long nResult = -1;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@ID_CHAVE", this.oCadastroChave.Id, OleDbType.VarChar, 50)
                    .Add("@USUARIO", this.oDataValidationAddRem.GetValueAcao(VINCULAR_USUARIO_FIELDS.USUARIO).ToUpper(), OleDbType.VarChar, 50)
                    .Add("@CODIGO_ACAO", nTipoExpedicao, OleDbType.Integer)
                    .Add("@USUARIO_ULTIMA_ALTERACAO", sUser, OleDbType.VarChar, 50)
                    ;

                String sQuery = @"DECLARE @ID_CHAVE_USUARIO AS BIGINT = -1
                                    INSERT INTO [WSQOLEXPEDICAOCHAVEUSUARIO]
                                               ([ID_CHAVE]
                                               ,[USUARIO]
                                               ,[CODIGO_ACAO]
                                               ,[USUARIO_ULTIMA_ALTERACAO])
                                         VALUES
                                               (@ID_CHAVE
                                               ,@USUARIO
                                               ,@CODIGO_ACAO  
                                               ,@USUARIO_ULTIMA_ALTERACAO)

                                        SET @ID_CHAVE_USUARIO = SCOPE_IDENTITY()
                                        
                                        SELECT @ID_CHAVE_USUARIO";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.GetResultado();

                    if (oResult != null)
                        nResult = (long)oResult;
                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Insert: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }

            return nResult;
        }

        private void RemoveChaveUsuario()
        {
            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@ID", oChaveUsuarioValuesMov.IdUsuario, OleDbType.BigInt)
                    ;

                String sQuery = @"DELETE FROM [WSQOLEXPEDICAOCHAVEUSUARIO]
                                        WHERE ID = @ID";

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

        private ChaveUsuarioValuesMov LoadValuesUpdateChave()
        {
            oChaveUsuarioValuesMov = new ChaveUsuarioValuesMov()
            {
                Id = Convert.ToInt32(oDataValidationAddRem.GetValueAcao(VINCULAR_USUARIO_FIELDS.ID)),
                IdUsuario = Convert.ToInt32(oDataValidationAddRem.GetValueAcao(VINCULAR_USUARIO_FIELDS.ID_USUARIO)),
                Usuario = oDataValidationAddRem.GetValueAcao(VINCULAR_USUARIO_FIELDS.USUARIO),
                Entrega = Convert.ToBoolean(oDataValidationAddRem.GetValueAcao(VINCULAR_USUARIO_FIELDS.ENTREGA)),
                Separacao = Convert.ToBoolean(oDataValidationAddRem.GetValueAcao(VINCULAR_USUARIO_FIELDS.SEPARACAO)),
                Carregamento = Convert.ToBoolean(oDataValidationAddRem.GetValueAcao(VINCULAR_USUARIO_FIELDS.CARREGAMENTO))
            };

            return oChaveUsuarioValuesMov;
        }

        private void FillDataGrid()
        {
            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@ID", DBNull.Value, OleDbType.BigInt)
                    .Add("@ID_USUARIO", DBNull.Value, OleDbType.BigInt)
                    .Add("@USUARIO", DBNull.Value, OleDbType.VarChar, 50)
                    .Add("@SEPARACAO", DBNull.Value, OleDbType.Integer)
                    .Add("@ENTREGA", DBNull.Value, OleDbType.Integer)
                    .Add("@CARREGAMENTO", DBNull.Value, OleDbType.Integer)
                    .Add("@ID", this.oChaveUsuarioValuesMov.IdUsuario, OleDbType.BigInt)
                    ;

                string sQuery = @"SELECT
 	                                ,ID_CHAVE AS ID
                                    ,ID AS ID_USUARIO
	                                ,USUARIO	
                                    ,CAST(CASE WHEN((TIPO_EXPEDICAO & 1) = 1) THEN 1 ELSE 0 END AS BIT) SEPARACAO
                                    ,CAST(CASE WHEN((TIPO_EXPEDICAO & 2) = 2) THEN 1 ELSE 0 END AS BIT) ENTREGA
                                    ,CAST(CASE WHEN((TIPO_EXPEDICAO & 4) = 4) THEN 1 ELSE 0 END AS BIT) CARREGAMENTO
                                    FROM
	                                WSQOLEXPEDICAOCHAVEUSUARIO
                                WHERE
	                                ID_USUARIO = ?";

                oCommand.Command.Parameters["@ID"].Direction = ParameterDirection.Output;
                oCommand.Command.Parameters["@ID_USUARIO"].Direction = ParameterDirection.Output;
                oCommand.Command.Parameters["@USUARIO"].Direction = ParameterDirection.Output;
                oCommand.Command.Parameters["@SEPARACAO"].Direction = ParameterDirection.Output;
                oCommand.Command.Parameters["@ENTREGA"].Direction = ParameterDirection.Output;
                oCommand.Command.Parameters["@CARREGAMENTO"].Direction = ParameterDirection.Output;

                try
                {
                    oCommand.SetCommandText(sQuery);

                    oDataValidationAddRem.SetValueAcao(this.oDataValidationAddRem.GetValueAcao(VINCULAR_USUARIO_FIELDS.ID), oCommand.Command.Parameters["@ID"].Value.ToString());
                    oDataValidationAddRem.SetValueAcao(this.oDataValidationAddRem.GetValueAcao(VINCULAR_USUARIO_FIELDS.ID_USUARIO), oCommand.Command.Parameters["@ID_USUARIO"].Value.ToString());
                    oDataValidationAddRem.SetValueAcao(this.oDataValidationAddRem.GetValueAcao(VINCULAR_USUARIO_FIELDS.USUARIO), oCommand.Command.Parameters["@USUARIO"].Value.ToString());
                    oDataValidationAddRem.SetValueAcao(this.oDataValidationAddRem.GetValueAcao(VINCULAR_USUARIO_FIELDS.SEPARACAO), oCommand.Command.Parameters["@SEPARACAO"].Value.ToString());
                    oDataValidationAddRem.SetValueAcao(this.oDataValidationAddRem.GetValueAcao(VINCULAR_USUARIO_FIELDS.ENTREGA), oCommand.Command.Parameters["@ENTREGA"].Value.ToString());
                    oDataValidationAddRem.SetValueAcao(this.oDataValidationAddRem.GetValueAcao(VINCULAR_USUARIO_FIELDS.CARREGAMENTO), oCommand.Command.Parameters["@CARREGAMENTO"].Value.ToString());
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Insert: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }

            }
        }
    }

    [AutoPersistencia]
    public class ChaveUsuarioValuesMov
    {
        public int Id { get; set; }

        public int IdUsuario { get; set; }

        public string Usuario { get; set; }

        public bool Separacao { get; set; }

        public bool Entrega { get; set; }

        public bool Carregamento { get; set; }

    }

    public class VINCULAR_USUARIO_FIELDS
    {
        public const string ID = "Id";
        public const string ID_USUARIO = "IdUsuario";
        public const string CHAVE = "Chave";
        public const string USUARIO = "Usuario";
        public const string SEPARACAO = "Separacao";
        public const string ENTREGA = "Entrega";
        public const string CARREGAMENTO = "Carregamento";
    }
}
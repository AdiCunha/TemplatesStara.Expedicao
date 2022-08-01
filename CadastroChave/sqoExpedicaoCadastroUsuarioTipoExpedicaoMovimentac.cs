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
using AI1627Common20.Log;
using System.Linq;

namespace sqoTraceabilityStation
{
    [TemplateDebug("sqoExpedicaoCadastroUsuarioTipoExpedicaoMovimentac")]
    public class sqoExpedicaoCadastroUsuarioTipoExpedicaoMovimentac : IProcessMovimentacao
    {
        private sqoClassSetMessageDefaults oClassSetMessageDefaults;
        private DataValidationAddRem oDataValidationAddRem;
        private sqoExpedicaoChave oCadastroChave;
        private sqoTipoExpedicao oSqoTipoExpedicaoChave;
        private ChaveUsuarioValues oChaveUsuarioValues;

        private int nQtdErros = 0;
        private string sMessage = "Falha na validação de dados";
        private string sDescription = string.Empty;
        private string sUser = string.Empty;
        private int nTipoExpedicaoUsuario = 0;
        private string sTipoExpedicaoUsuario;

        public sqoClassMessage Executar(string sAction, string sXmlDados, string sXmlType, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao, List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sNivel, string sUsuario, object oObjAux)
        {
            using (sqoClassDbConnection oDBConnection = new sqoClassDbConnection())
            {
                this.Init(oListaParametrosMovimentacao, sUsuario, sXmlDados);

                this.Validate();

                this.ProcessBussinessLogic(oDBConnection, oClassSetMessageDefaults, oListaParametrosMovimentacao);
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
                this.ValidateAlter();
            }

            else
            {
                this.nQtdErros++;

                this.sDescription += nQtdErros.ToString() + " - Função não disponível, para salvar as alterações clique em 'Adiciona'.";
            }

            this.ValidateMessage();
        }

        private void ValidateAlter()
        {

            if (String.IsNullOrEmpty(oDataValidationAddRem.GetValueAcao(VINCULAR_USUARIO_TIPO_EXPEDICAO_FIELDS.ID_USUARIO)))
            {
                this.nQtdErros++;

                this.sDescription += nQtdErros.ToString() + " - Obrigatório selecionar um item da lista " + Environment.NewLine;
            }
            else
            {
                this.LoadValuesUpdateChave();

                this.nTipoExpedicaoUsuario = this.SumTipoExpedicaoCodigo();

                this.oSqoTipoExpedicaoChave = this.GetAcessoChave();

                if (!String.IsNullOrEmpty(this.ValidaAcessoUsuarioVsChave()))
                {
                    this.nQtdErros++;

                    this.sDescription += nQtdErros.ToString() + " - Cadastro inválido, Tipo Expedição: " + sTipoExpedicaoUsuario + "não cadastrado(s) na chave: " + this.oCadastroChave.Chave;
                }

            }

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

        private ChaveUsuarioValues LoadValuesUpdateChave()
        {
            oChaveUsuarioValues = new ChaveUsuarioValues()
            {
                Id = Convert.ToInt32(oDataValidationAddRem.GetValueAcao(VINCULAR_USUARIO_TIPO_EXPEDICAO_FIELDS.ID_USUARIO)),
                Usuario = oDataValidationAddRem.GetValueAcao(VINCULAR_USUARIO_TIPO_EXPEDICAO_FIELDS.USUARIO),
                Entrega = Convert.ToBoolean(oDataValidationAddRem.GetValueAcao(VINCULAR_USUARIO_TIPO_EXPEDICAO_FIELDS.ENTREGA)),
                Separacao = Convert.ToBoolean(oDataValidationAddRem.GetValueAcao(VINCULAR_USUARIO_TIPO_EXPEDICAO_FIELDS.SEPARACAO)),
                Carregamento = Convert.ToBoolean(oDataValidationAddRem.GetValueAcao(VINCULAR_USUARIO_TIPO_EXPEDICAO_FIELDS.CARREGAMENTO))
            };

            return oChaveUsuarioValues;
        }

        private int SumTipoExpedicaoCodigo()
        {

            if (oChaveUsuarioValues.Separacao)
            {
                nTipoExpedicaoUsuario += 1;
            }

            if (oChaveUsuarioValues.Entrega)
            {
                nTipoExpedicaoUsuario += 2;
            }

            if (oChaveUsuarioValues.Carregamento)
            {
                nTipoExpedicaoUsuario += 4;
            }

            return nTipoExpedicaoUsuario;
        }

        public string ValidaAcessoUsuarioVsChave()
        {

            if (oChaveUsuarioValues.Separacao && !oCadastroChave.Separacao)
            {
                sTipoExpedicaoUsuario += "SEPARAÇÃO; ";
            }

            if (oChaveUsuarioValues.Entrega && !oCadastroChave.Entrega)
            {
                sTipoExpedicaoUsuario += "ENTREGA; ";
            }

            if (oChaveUsuarioValues.Carregamento && !oCadastroChave.Carregamento)
            {
                sTipoExpedicaoUsuario += "CARREGAMENTO; ";
            }

            return sTipoExpedicaoUsuario;
        }

        private void ProcessBussinessLogic(sqoClassDbConnection oDBConnection, sqoClassSetMessageDefaults oClassSetMessageDefaults, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao)
        {

            sqoClassMessage oClassMessage = new sqoClassMessage();

            try
            {
                oDBConnection.BeginTransaction();

                if (this.oDataValidationAddRem.Acao.Equals(Acao.Add))
                {
                    this.UpdateTipoExpedicao(nTipoExpedicaoUsuario);

                    this.FillDataGrid();

                    Tools.SincronizarDataValidationAddRemToParam(oListaParametrosMovimentacao, oDataValidationAddRem);

                    oClassSetMessageDefaults.SetarOk();

                    oClassSetMessageDefaults.Message.Dado = oListaParametrosMovimentacao;

                    oDBConnection.Commit();

                }

                else if (this.oDataValidationAddRem.Acao.Equals(Acao.Remove))
                {
                    this.nQtdErros++;

                    this.sDescription += nQtdErros.ToString() + "Ação Inválida, para validar as alterações clique em 'Adicionar' ";
                }

            }
            catch (Exception ex)
            {
                sqoClassMessageUserException oClassMessageUserException =
                   new sqoClassMessageUserException("Error" + Environment.NewLine + ex.Message, ex.InnerException);
                oDBConnection.Rollback();

                throw oClassMessageUserException;
            }
        }

        private void UpdateTipoExpedicao(int nTipoExpedicaoUsuario)
        {

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand.SetCommandText(@"UPDATE WSQOLEXPEDICAOCHAVEUSUARIO 
	                                        SET CODIGO_ACAO = ? 
	                                        WHERE ID = ?")

                  .Add("@CODIGO_ACAO", nTipoExpedicaoUsuario, OleDbType.Integer)
                  .Add("@ID", oChaveUsuarioValues.Id, OleDbType.BigInt);

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
                    .Add("@USUARIO", DBNull.Value, OleDbType.VarChar, 50)
                    .Add("@SEPARACAO", DBNull.Value, OleDbType.Integer)
                    .Add("@ENTREGA", DBNull.Value, OleDbType.Integer)
                    .Add("@CARREGAMENTO", DBNull.Value, OleDbType.Integer)
                    .Add("@ID", this.oCadastroChave.Id, OleDbType.BigInt)
                    ;

                String sQuery = @"SELECT
 	                                 ID 
	                                ,USUARIO	
                                    ,CAST(CASE WHEN((TIPO_EXPEDICAO & 1) = 1) THEN 1 ELSE 0 END AS BIT) SEPARACAO
                                    ,CAST(CASE WHEN((TIPO_EXPEDICAO & 2) = 2) THEN 1 ELSE 0 END AS BIT) ENTREGA
                                    ,CAST(CASE WHEN((TIPO_EXPEDICAO & 4) = 4) THEN 1 ELSE 0 END AS BIT) CARREGAMENTO
                                    FROM
	                                WSQOLEXPEDICAOCHAVEUSUARIO
                                WHERE
	                                ID = @ID";

                oCommand.Command.Parameters["@ID"].Direction = ParameterDirection.Output;
                oCommand.Command.Parameters["@USUARIO"].Direction = ParameterDirection.Output;
                oCommand.Command.Parameters["@SEPARACAO"].Direction = ParameterDirection.Output;
                oCommand.Command.Parameters["@ENTREGA"].Direction = ParameterDirection.Output;
                oCommand.Command.Parameters["@CARREGAMENTO"].Direction = ParameterDirection.Output;

                try
                {
                    oCommand.SetCommandText(sQuery);

                    oDataValidationAddRem.SetValueAcao(this.oDataValidationAddRem.GetValueAcao(VINCULAR_USUARIO_TIPO_EXPEDICAO_FIELDS.ID_USUARIO), oCommand.Command.Parameters["@ID"].Value.ToString());
                    oDataValidationAddRem.SetValueAcao(this.oDataValidationAddRem.GetValueAcao(VINCULAR_USUARIO_TIPO_EXPEDICAO_FIELDS.USUARIO), oCommand.Command.Parameters["@USUARIO"].Value.ToString());
                    oDataValidationAddRem.SetValueAcao(this.oDataValidationAddRem.GetValueAcao(VINCULAR_USUARIO_TIPO_EXPEDICAO_FIELDS.SEPARACAO), oCommand.Command.Parameters["@SEPARACAO"].Value.ToString());
                    oDataValidationAddRem.SetValueAcao(this.oDataValidationAddRem.GetValueAcao(VINCULAR_USUARIO_TIPO_EXPEDICAO_FIELDS.ENTREGA), oCommand.Command.Parameters["@ENTREGA"].Value.ToString());
                    oDataValidationAddRem.SetValueAcao(this.oDataValidationAddRem.GetValueAcao(VINCULAR_USUARIO_TIPO_EXPEDICAO_FIELDS.CARREGAMENTO), oCommand.Command.Parameters["@CARREGAMENTO"].Value.ToString());
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Insert: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }

            }
        }

    }

    public class VINCULAR_USUARIO_TIPO_EXPEDICAO_FIELDS
    {
        public const string ID_USUARIO = "IdUsuario";
        public const string USUARIO = "Usuario";
        public const string SEPARACAO = "Separacao";
        public const string ENTREGA = "Entrega";
        public const string CARREGAMENTO = "Carregamento";
    }

    [AutoPersistencia]
    public class ChaveUsuarioValues
    {
        public int Id { get; set; }

        public int IdUsuario { get; set; }

        public string Usuario { get; set; }

        public bool Separacao { get; set; }

        public bool Entrega { get; set; }

        public bool Carregamento { get; set; }

    }
}



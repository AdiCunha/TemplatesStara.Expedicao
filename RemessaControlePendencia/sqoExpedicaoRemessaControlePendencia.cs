//Comentar o define quando colar na web!
//#define NAO_COMPILAR

#if !NAO_COMPILAR
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
    [TemplateDebug("sqoExpedicaoRemessaControlePendencia")]
    class sqoExpedicaoRemessaControlePendencia : sqoClassProcessMovimentacao
    {
        private String sUser;
        private String sNivel;
        private String sEmailUser = String.Empty;
        private Action currentAction = Action.Invalid;

        private sqoClassRemessaPendenciaData oClassRemessaPendenciaData;
        private sqoClassSetMessageDefaults oClassSetMessageDefaults;

        private int nQtdErros = 0;
        private String sMessage = "Falha na validação de dados";
        private String sDescription = String.Empty;

        enum Action { Invalid = -1, Insert, Update, Duplicate, SendMail }
        public override sqoClassMessage Executar(string sAction, string sXmlDados, string sXmlType, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao,
            List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sNivel, string sUsuario, object oObjAux)
        {
            using (sqoClassDbConnection oDBConnection = new sqoClassDbConnection())
            {
                this.Init(sXmlDados, sUsuario, sAction, sNivel);

                this.Validate();

                this.ProcessBussinessLogic(oDBConnection);
            }

            return this.oClassSetMessageDefaults.Message;
        }

        private void Init(String sXmlDados, String sUsuario, String sAction, String sNivel)
        {
            this.oClassRemessaPendenciaData = new sqoClassRemessaPendenciaData();

            this.oClassRemessaPendenciaData = sqoClassBiblioSerDes.DeserializeObject<sqoClassRemessaPendenciaData>(sXmlDados);

            this.oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());

            this.sUser = sUsuario;

            this.sNivel = sNivel;

            Enum.TryParse(sAction, out currentAction);

            this.FillData();

            if (this.oClassRemessaPendenciaData.SendUser)
                this.sEmailUser = this.GetMailUser();

            if (this.oClassRemessaPendenciaData.SendUser)
                this.FillDestinatario(sEmailUser);
        }

        private void FillData()
        {
            if (this.oClassRemessaPendenciaData.Disponivel)
                this.oClassRemessaPendenciaData.nDisponivel = 1;
            else
                this.oClassRemessaPendenciaData.nDisponivel = 0;
        }

        private void Validate()
        {
            if (currentAction.Equals(Action.Update))
            {
                if (this.GetPendencia())
                {
                    this.nQtdErros++;

                    this.sDescription += this.nQtdErros.ToString() + " - Alteração inválida, campo \"Disponível\" já cadastro como " + this.oClassRemessaPendenciaData.Disponivel.ToString()
                        + "!" + Environment.NewLine;
                }
            }

            else if (this.currentAction.Equals(Action.SendMail))
            {
                if (String.IsNullOrEmpty(this.oClassRemessaPendenciaData.Destinatario) && !this.oClassRemessaPendenciaData.SendUser)
                {
                    this.nQtdErros++;

                    this.sDescription += this.nQtdErros.ToString() + " - Campo Destinatário é obrigatório! Favor preencher!" + Environment.NewLine;
                }

                if (String.IsNullOrEmpty(this.oClassRemessaPendenciaData.Descricao_Email))
                {
                    this.nQtdErros++;

                    this.sDescription += this.nQtdErros.ToString() + " - Campo Descrição E-mail é obrigatório! Favor preencher!" + Environment.NewLine;
                }

                if (this.oClassRemessaPendenciaData.SendUser && String.IsNullOrEmpty(this.sEmailUser))
                {
                    this.nQtdErros++;

                    this.sDescription += this.nQtdErros.ToString() + " - E-mail não cadastrado na tabela WSQOLOGINS para o usuário "
                        + this.sUser + "! Favor cadastrar e-mail ou  preencher a opção \"Enviar Para Mim\" como \"FALSE\"!" + Environment.NewLine;
                }

                String sValidateMail = this.ValidEmail();
                if (!String.IsNullOrEmpty(sValidateMail))
                {
                    this.nQtdErros++;

                    this.sDescription += this.nQtdErros.ToString() + " - " + sValidateMail;
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

        private bool GetPendencia()
        {
            Boolean Result = false;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@CODIGO_REMESSA", this.oClassRemessaPendenciaData.Remessa, OleDbType.VarChar, 50)
                    .Add("@DISPONIVEL", this.oClassRemessaPendenciaData.nDisponivel, OleDbType.Integer)
                    ;

                String sQuery = @"SELECT 1 FROM WSQOLEXPREMESSA WHERE CODIGO_REMESSA = @CODIGO_REMESSA AND DISPONIVEL = @DISPONIVEL";

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

        private void ProcessBussinessLogic(sqoClassDbConnection oDBConnection)
        {
            String sMessage = String.Empty;

            try
            {
                oDBConnection.BeginTransaction();

                if (this.currentAction.Equals(Action.Update))
                {
                    this.Save();

                    sMessage = "Dados Alterados com Sucesso";
                }

                else if (this.currentAction.Equals(Action.SendMail))
                {

                    //long nIdEmailConfig =  this.GetIDEmailConfig();
                    String sTable = this.FillTable2();

                    this.SendMail(sTable);

                    //this.SendMail2(stable);

                    sMessage = "E-mail enviado com sucesso!";
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
        }

        private void Save()
        {
            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@DISPONIVEL", this.oClassRemessaPendenciaData.nDisponivel, OleDbType.Integer)
                    .Add("@USUARIO_ULTIMA_MOVIMENTACAO", this.sUser, OleDbType.VarChar, 50)
                    .Add("@DATA_ULTIMA_MOVIMENTACAO", DateTime.Now, OleDbType.DBTimeStamp)
                    .Add("@OBSERVACAO", this.oClassRemessaPendenciaData.Observacao, OleDbType.VarChar, 50)
                    .Add("@CODIGO_REMESSA", this.oClassRemessaPendenciaData.Remessa, OleDbType.VarChar, 50)

                    ;

                String sQuery = @"UPDATE WSQOLEXPREMESSA
                                SET DISPONIVEL = @DISPONIVEL
                                    ,USUARIO_ULTIMA_MOVIMENTACAO = @USUARIO_ULTIMA_MOVIMENTACAO
                                    ,DATA_ULTIMA_MOVIMENTACAO = @DATA_ULTIMA_MOVIMENTACAO
                                    ,OBSERVACAO = @OBSERVACAO
                                WHERE
                                	CODIGO_REMESSA = @CODIGO_REMESSA
                                ";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.GetResultado();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        //ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                        ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }
            }
        }

        private void SendMail(String sTable)
        {
            using (var oCommand = new sqoCommand(CommandType.StoredProcedure))
            {
                oCommand
                    .SetCommandText("WSQOPROCSENDEMAIL")
                    .Add("@NOME", this.sNivel, OleDbType.VarChar, 100)
                    .Add("@ID", 0, OleDbType.BigInt)
                    .Add("@PARAM1", this.oClassRemessaPendenciaData.Descricao_Email, OleDbType.VarChar, 4000)
                    .Add("@PARAM2", sTable, OleDbType.VarChar, 4000)
                    .Add("@PARAM3", DBNull.Value, OleDbType.VarChar, 500)
                    .Add("@PARAM4", DBNull.Value, OleDbType.VarChar, 500)
                    .Add("@PARAM5", DBNull.Value, OleDbType.VarChar, 500)
                    .Add("@PARAM6", DBNull.Value, OleDbType.VarChar, 500)
                    .Add("@PARAM7", DBNull.Value, OleDbType.VarChar, 500)
                    .Add("@PARAM8", DBNull.Value, OleDbType.VarChar, 500)
                    .Add("@PARAM9", DBNull.Value, OleDbType.VarChar, 500)
                    .Add("@PARAM10", DBNull.Value, OleDbType.VarChar, 500)
                    .Add("@TO_MAIL_PARAM", this.oClassRemessaPendenciaData.Destinatario, OleDbType.VarChar, 1000)
                    .Add("@IS_BODY_HTML", true, OleDbType.Boolean)
                    ;

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

        private string GetMailUser()
        {
            String sEmailUser = String.Empty;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@EMAIL", DBNull.Value, OleDbType.VarChar, 50)
                    .Add("@USUARIO", this.sUser, OleDbType.VarChar, 50)
                    ;

                String sQuery = @"SELECT @EMAIL = EMAIL FROM WSQOLOGINS WHERE USUARIO = @USUARIO";

                oCommand.Command.Parameters["@EMAIL"].Direction = ParameterDirection.Output;

                try
                {
                    oCommand.SetCommandText(sQuery);

                    oCommand.Execute();

                    sEmailUser = oCommand.Command.Parameters["@EMAIL"].Value.ToString();

                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }
            }

            return sEmailUser;
        }

        private void FillDestinatario(String sEmailUser)
        {
            if (!this.oClassRemessaPendenciaData.Destinatario.Contains(sEmailUser))
                this.oClassRemessaPendenciaData.Destinatario = String.Concat(this.oClassRemessaPendenciaData.Destinatario, "; ", sEmailUser);

        }

        private string ValidEmail()
        {
            String sResult = String.Empty;
            String sValidaMail = String.Empty;

            String[] colunas = this.oClassRemessaPendenciaData.Destinatario.Split(';');

            int nLength = colunas.Length;

            var oValidateEmail = new ValidateEmail();

            for (int i = 0; i < nLength; i++)
            {
                if (!oValidateEmail.IsValidEmail(colunas[i]))
                    sResult += "A cadeia de caracteres especificada não está no formato necessário para um endereço de email " 
                        + colunas[i]  + Environment.NewLine;
            }

            return sResult;
        }

        private string FillTable()
        {
            String sTable = "<table Style = \"border: 1px solid black\">" +
                "<tr>" +

                    "<th Style = \"border: 1px solid black\"> Disponível </th>" +

                    "<th Style = \"border: 1px solid black\"> Ordem de Venda</th>" +

                    "<th Style = \"border: 1px solid black\"> Remessa </th>" +

                    "<th Style = \"border: 1px solid black\"> Chave </th>" +

                    "<th Style = \"border: 1px solid black\"> Status Picking </th >" +

                    "<th Style = \"border: 1px solid black\"> Qtd Itens </th> " +

                    "<th Style = \"border: 1px solid black\"> Status Remessa </th>" +

                    "<th Style = \"border: 1px solid black\"> Código Motivo </th> " +

                    "<th Style = \"border: 1px solid black\"> Cliente </th>" +

                    "<th Style = \"border: 1px solid black\"> Cidade </th>" +

                    "<th Style = \"border: 1px solid black\"> Estado </th>" +

                    "<th Style = \"border: 1px solid black\"> Data Remessa </th>" +

                    "<th Style = \"border: 1px solid black\"> Data Fim Pagamento</th>" +

                "</tr>" +

                "<tr>" +

                    "<td Style = \"border: 1px solid black\" >" + this.oClassRemessaPendenciaData.Disponivel.ToString() + "</td>" +

                    "<td Style = \"border: 1px solid black\" >" + this.oClassRemessaPendenciaData.OrdemVenda + "</td>" +

                    "<td Style = \"border: 1px solid black\" >" + this.oClassRemessaPendenciaData.Remessa + "</td>" +

                    "<td Style = \"border: 1px solid black\" >" + this.oClassRemessaPendenciaData.Chave + "</td>" +

                    "<td Style = \"border: 1px solid black\" >" + this.oClassRemessaPendenciaData.StatusPicking + "</td>" +

                    "<td Style = \"border: 1px solid black\" >" + this.oClassRemessaPendenciaData.Qtditem.ToString() + "</td>" +

                    "<td Style = \"border: 1px solid black\" >" + this.oClassRemessaPendenciaData.StatusRemessa.ToString() + "</td>" +

                    "<td Style = \"border: 1px solid black\" >" + this.oClassRemessaPendenciaData.CodigoMotivo + "</td>" +

                    "<td Style = \"border: 1px solid black\" >" + this.oClassRemessaPendenciaData.Cliente + "</td>" +

                    "<td Style = \"border: 1px solid black\" >" + this.oClassRemessaPendenciaData.Cidade + "</td>" +

                    "<td Style = \"border: 1px solid black\" >" + this.oClassRemessaPendenciaData.Estado + "</td>" +

                    "<td Style = \"border: 1px solid black\" >" + this.oClassRemessaPendenciaData.DataRemessa.ToString() + "</td>" +

                    "<td Style = \"border: 1px solid black\" >" + this.oClassRemessaPendenciaData.DataFimPagamento.ToString() + "</td>" +

                "</tr>" +
            "</table>";

            return sTable;
        }

        private string FillTable2()
        {
            String sTable = @"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"">
                                <HTML><HEAD>
                                <META http-equiv=Content-Type content=""text/html; charset=iso-8859-1"">
                                <META content=""MSHTML 6.00.2900.3020"" name=GENERATOR>
                                <style type=""text/css"">
                                table {
                                    font-family: arial, sans-serif;
                                    border-collapse: collapse;
                                    width: 100%;
                                }
                                
                                td, th {
                                    border: 1px solid #dddddd;
                                    text-align: left;
                                    padding: 8px;
                                }
                                
                                tr:nth-child(even) {
                                    background-color: #dddddd;
                                }
                                	</style>
                                </HEAD>
                                <body>                              
                                <table>
                                    <tr>
                                      <th>Disponível</th>
                                      <th>Ordem de Venda</th>
                                      <th>Remessa</th>
                                	  <th>Chave</th>
                                	  <th>Status Picking</th>
                                	  <th>Qtd Itens</th>
                                	  <th>Status Remessa</th>
                                	  <th>Código Motivo</th>
                                	  <th>Cliente</th>
                                	  <th>Cidade</th>
                                	  <th>Estado</th>
                                	  <th>Data Remessa</th>
                                	  <th>Data Fim Pagamento</th>
                                	  <th>Observação</th>
                                    </tr>
                                    <tr>
                                      <td>@DISPONIVEL</td>
                                      <td>@ORDEM_VENDA</td>
                                      <td>@REMESSA</td>
                                      <td>@CHAVE</td>
                                	  <td>@STATUS_PICKING</td>
                                      <td>@QTD_ITENS</td>
                                      <td>@STATUS_REMESSA</td>
                                      <td>@CODIGO_MOTIVO</td>
                                	  <td>@CLIENTE</td>
                                      <td>@CIDADE</td>
                                      <td>@ESTADO</td>
                                      <td>@DATA_REMESSA</td>
                                	  <td>@DATA_FIM_PAGAMETO</td>
                                	  <td>@OBSERVACAO</td>
                                    </tr>
                                </table>
                                </body>
                                </html>";

            sTable = sTable.Replace("@DISPONIVEL", this.oClassRemessaPendenciaData.Disponivel.ToString());
            sTable = sTable.Replace("@ORDEM_VENDA", this.oClassRemessaPendenciaData.OrdemVenda);
            sTable = sTable.Replace("@REMESSA", this.oClassRemessaPendenciaData.Remessa);
            sTable = sTable.Replace("@CHAVE", this.oClassRemessaPendenciaData.Chave);
            sTable = sTable.Replace("@STATUS_PICKING", this.oClassRemessaPendenciaData.StatusPicking);
            sTable = sTable.Replace("@QTD_ITENS", this.oClassRemessaPendenciaData.Qtditem.ToString());
            sTable = sTable.Replace("@STATUS_REMESSA", this.oClassRemessaPendenciaData.StatusRemessa.ToString());
            sTable = sTable.Replace("@CODIGO_MOTIVO", this.oClassRemessaPendenciaData.CodigoMotivo);
            sTable = sTable.Replace("@CLIENTE", this.oClassRemessaPendenciaData.Cliente);
            sTable = sTable.Replace("@CIDADE", this.oClassRemessaPendenciaData.Cidade);
            sTable = sTable.Replace("@ESTADO", this.oClassRemessaPendenciaData.Estado);
            sTable = sTable.Replace("@DATA_REMESSA", this.oClassRemessaPendenciaData.DataRemessa);
            sTable = sTable.Replace("@DATA_FIM_PAGAMETO", this.oClassRemessaPendenciaData.DataFimPagamento);
            sTable = sTable.Replace("@OBSERVACAO", this.oClassRemessaPendenciaData.Observacao);


            return sTable;
        }

        //private void SendMail2 (String sTabel)
        //{
        //    using (var oCommand = new sqoCommand)
        //    {

        //    }
        //}
    }

    /// <summary>
    /// Classe para instanciar o modelo do criteria da tela.
    /// </summary>
    [XmlRoot("ItemFilaProducao")]
    public class sqoClassRemessaPendenciaData
    {
        [XmlElement("DISPONIVEL")]
        public Boolean Disponivel { get; set; }

        [XmlElement("REMESSA")]
        public string Remessa { get; set; }

        [XmlElement("OBSERVACAO")]
        public string Observacao { get; set; }

        [XmlElement("DESTINATARIO")]
        public string Destinatario { get; set; }

        [XmlElement("DESCRICAO_EMAIL")]
        public string Descricao_Email { get; set; }

        [XmlElement("SEND_USER")]
        public bool SendUser { get; set; }

        [XmlElement("ORDEM_VENDA")]
        public string OrdemVenda { get; set; }

        [XmlElement("CHAVE")]
        public string Chave { get; set; }

        [XmlElement("STATUS_PICKING")]
        public string StatusPicking { get; set; }

        [XmlElement("QTD_ITEM")]
        public double Qtditem { get; set; }

        [XmlElement("STATUS_REMESSA")]
        public int StatusRemessa { get; set; }

        [XmlElement("CODIGO_MOTIVO")]
        public string CodigoMotivo { get; set; }

        [XmlElement("CLIENTE")]
        public string Cliente { get; set; }

        [XmlElement("CIDADE")]
        public string Cidade { get; set; }

        [XmlElement("ESTADO")]
        public string Estado { get; set; }

        [XmlElement("DATA_REMESSA")]
        public string DataRemessa { get; set; }

        [XmlElement("DATA_FIM_PAGAMENTO")]
        public string DataFimPagamento { get; set; }

        public int nDisponivel { get; set; }
    }
}
#endif
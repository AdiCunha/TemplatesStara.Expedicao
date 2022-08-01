using AI1627Common20.TemplateDebugging;
using sqoClassLibraryAI0502Biblio;
using sqoClassLibraryAI0502Message;
using sqoClassLibraryAI0502VariaveisSistema;
using sqoClassLibraryAI1151FilaProducao;
using sqoClassLibraryAI1151FilaProducao.Estrutura;
using sqoClassLibraryAI1151FilaProducao.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TemplatesStara.CommonStara;
using TemplateStara.Expedicao.TransicaoStatus.DataModel;

namespace TemplateStara.Expedicao.TransicaoStatus.Business
{
    [TemplateDebug("WebProcessTransicaoStatusInserir")]
    public class WebProcessTransicaoStatusInserir : IProcessMovimentacao
    {
        private sqoClassSetMessageDefaults oClassSetMessageDefaults;
        private StatusTransitionsInsert oStatusTransitionsInsert;
        private string sDescription = string.Empty;
        private string sMessage = "Falha na validação de dados";


        public sqoClassMessage Executar(string sAction
            , string sXmlDados
            , string sXmlType
            , List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao
            , List<sqoClassParametrosEstrutura> oListaParametrosListagem
            , string sNivel
            , string sUsuario
            , object oObjAux)

        {
            using (sqoClassDbConnection oDBConnection = new sqoClassDbConnection())
            {
                this.Init(oListaParametrosMovimentacao, sUsuario, sXmlDados);

                this.ValidarCampos();

                this.ProcessBussinessLogic(oDBConnection, oListaParametrosMovimentacao);

            }

            return this.oClassSetMessageDefaults.Message;
        }

        private void Init(List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao, string sUsuario, string sXmlDados)
        {
            this.oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());

            this.oStatusTransitionsInsert = new StatusTransitionsInsert();

            this.oStatusTransitionsInsert = sqoClassBiblioSerDes.DeserializeObject<StatusTransitionsInsert>(sXmlDados);

        }

        private string CheckRegra()
        {
            string Regra = "";

            if (oStatusTransitionsInsert.Permite)
            {
                Regra = "ALLOW";
            }
            else
            {
                Regra = "DENY";
            }

            return Regra;
        }

        private void ValidarCampos()
        {
            WebProcessTransicaoStatusInserirValidacoes oWebProcessTransicaoStatusInserirValidacoes = new WebProcessTransicaoStatusInserirValidacoes();

            sDescription = oWebProcessTransicaoStatusInserirValidacoes.ValidarPreenchimentoCampos(oStatusTransitionsInsert.CurrentStatus
                                                                                                , oStatusTransitionsInsert.NextStatus
                                                                                                , oStatusTransitionsInsert.Modulo
                                                                                                , oStatusTransitionsInsert.Permite
                                                                                                , oStatusTransitionsInsert.Mensagem);
            this.ValidateMessage();
        }

        private void ProcessBussinessLogic(sqoClassDbConnection oDBConnection, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao)
        {
            try
            {
                oDBConnection.BeginTransaction();

                WebProcessVerificarModuloInsert oWebProcessVerificarModuloInsert = new WebProcessVerificarModuloInsert();

                oWebProcessVerificarModuloInsert.InserirStatusExpedicao(oStatusTransitionsInsert, this.CheckRegra());

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

        public void ValidateMessage()
        {
            if (!string.IsNullOrEmpty(sDescription))
            {
                this.oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());

                string sMessageBody = Environment.NewLine + sDescription;

                CommonStara.MessageBox(false, this.sMessage, sMessageBody, sqoClassMessage.MessageTypeEnum.ERROR, oClassSetMessageDefaults);

                throw new sqoClassMessageUserException(oClassSetMessageDefaults.Message);
            }
        }
    }
}

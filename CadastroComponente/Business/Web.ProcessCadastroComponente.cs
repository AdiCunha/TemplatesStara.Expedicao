using sqoClassLibraryAI0502VariaveisSistema;
using System;
using System.Collections.Generic;
using sqoClassLibraryAI0502Biblio;
using sqoClassLibraryAI1151FilaProducao.Estrutura;
using sqoClassLibraryAI0502Message;
using AI1627Common20.TemplateDebugging;
using sqoClassLibraryAI1151FilaProducao.Process;
using sqoClassLibraryAI1151FilaProducao;
using TemplatesStara.CommonStara;
using TemplateStara.Expedicao.CadastroComponente.DataModel;

namespace sqoTraceabilityStation
{
    [TemplateDebug("Web.ProcessCadastroComponente")]
    public class sqoCadastroComponente : IProcessMovimentacao
    {
        private Action currentAction = Action.Invalid;
        private CadastroComponente oCadastroComponente;
        private sqoClassSetMessageDefaults oClassSetMessageDefaults;
        private CadastroComponenteDao oCadastroComponenteDao = new CadastroComponenteDao();
        private ProcessCadCompValidacoes oProcessCadCompValidacoes = new ProcessCadCompValidacoes();

        private int nQtdErros = 0;
        private string sMessage = "Falha na validação de dados";
        private string sMessageErro = string.Empty;

        enum Action { Invalid = -1, Insert, Update, Delete }

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
                this.Init(sXmlDados, sUsuario, oListaParametrosListagem, sAction);

                this.ValidarPreenchimento();

                this.ProcessBussinessLogic(oDBConnection, oClassSetMessageDefaults, sUsuario);
            }

            return oClassSetMessageDefaults.Message;
        }

        private void Init(string sXmlDados, string sUsuario, List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sAction)
        {
            this.oCadastroComponente = new CadastroComponente();

            this.oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());

            this.oCadastroComponente = sqoClassBiblioSerDes.DeserializeObject<CadastroComponente>(sXmlDados);

            Enum.TryParse(sAction, out currentAction);

            if (this.currentAction.Equals(Action.Insert))
                this.FillPersistence();
            else
                UpperDescription();
        }

        private void FillPersistence()
        {
            oCadastroComponente.Material = oCadastroComponente.MaterialInsert;

            oCadastroComponente.DescricaoComponente = oCadastroComponente.DescricaoComponenteInsert;

            oCadastroComponente.Tipo = oCadastroComponente.TipoInsert;

            UpperDescription();
        }

        private void UpperDescription()
        {
            oCadastroComponente.DescricaoComponente = oCadastroComponente.DescricaoComponente.ToUpper();
        }

        private void ValidarPreenchimento()
        {
            if (currentAction.Equals(Action.Insert))
            {
                sMessageErro += oProcessCadCompValidacoes.ValidacaoesCadastro(oCadastroComponente);
            }

            if (currentAction.Equals(Action.Update))
            {
                sMessageErro += oProcessCadCompValidacoes.ValidateUpdate(oCadastroComponente);
            }

            if (!string.IsNullOrEmpty(sMessageErro))
            {
                CommonStara.MessageBox(false, "Falha na validação de dados", sMessageErro, sqoClassMessage.MessageTypeEnum.ERROR, oClassSetMessageDefaults);

                throw new sqoClassMessageUserException(oClassSetMessageDefaults.Message);
            }
        }


        private sqoClassMessage ProcessBussinessLogic(sqoClassDbConnection oDBConnection, sqoClassSetMessageDefaults oClassSetMessageDefaults, string sUsuario)
        {
            try
            {
                 oDBConnection.BeginTransaction();

                if (currentAction.Equals(Action.Insert))
                {
                    oCadastroComponenteDao.SaveComponente(oCadastroComponente, sUsuario);

                    CommonStara.MessageBox(true, "Dados Inseridos com Sucesso", "", sqoClassMessage.MessageTypeEnum.OK, oClassSetMessageDefaults);
                }

                if (currentAction.Equals(Action.Update))
                {
                    oCadastroComponenteDao.UpdateComponente(oCadastroComponente, sUsuario);

                    CommonStara.MessageBox(true, "Dados Alterados com Sucesso", "", sqoClassMessage.MessageTypeEnum.OK, oClassSetMessageDefaults);
                }

                else if (currentAction.Equals(Action.Delete))
                {
                    oCadastroComponenteDao.DeleteComponente(oCadastroComponente);

                    CommonStara.MessageBox(true, "Registro excluído com Sucesso", "", sqoClassMessage.MessageTypeEnum.OK, oClassSetMessageDefaults);
                }

                oDBConnection.Commit();
            }
            catch (Exception ex)
            {
                sqoClassMessageUserException oClassMessageUserException = new sqoClassMessageUserException("Error" + Environment.NewLine + ex.Message, ex.InnerException);

                oDBConnection.Rollback();

                throw oClassMessageUserException;
            }

            return oClassSetMessageDefaults.Message;
        }
    }
}
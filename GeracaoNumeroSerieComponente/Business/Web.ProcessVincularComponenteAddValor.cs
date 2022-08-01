using AI1627Common20.TemplateDebugging;
using sqoClassLibraryAI0502Message;
using sqoClassLibraryAI0502VariaveisSistema;
using sqoClassLibraryAI1151FilaProducao;
using sqoClassLibraryAI1151FilaProducao.Estrutura;
using sqoClassLibraryAI1151FilaProducao.Persistencia;
using sqoClassLibraryAI1151FilaProducao.Process;
using System;
using System.Collections.Generic;
using TemplateStara.Expedicao.GeracaoNumeroSerieComponente.DataModel;

namespace TemplateStara.Expedicao.GeracaoNumeroSerieComponente.Business
{
    [TemplateDebug("Web.ProcessVincularComponenteAddValor")]
    class ProcessVincularComponenteAddValor : IProcessMovimentacao
    {
        private sqoClassSetMessageDefaults oClassSetMessageDefaults;
        private DataValidationAddRem oDataValidationAddRem;
        private string sDescription = string.Empty;
        private List<DocumentoReferenciaListagemList> oDocumentoReferenciaListagemList;
        private string sMessage = "Falha na validação de dados";

        private const string DescricaoComponente = "DescricaoComponente";
        private const string Valor = "Valor";
        private const string Material = "MaterialList";

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
                this.Init(sXmlDados, oListaParametrosMovimentacao, oListaParametrosListagem);

                this.ProcessBusinessLogic(oListaParametrosMovimentacao);
            }

            return oClassSetMessageDefaults.Message;
        }

        private void Init(string sXmlDados, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao, List<sqoClassParametrosEstrutura> oListaParametrosListagem)
        {
            this.oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());

            this.oDataValidationAddRem = Tools.ConverterParamToDataValidationAddRem(oListaParametrosMovimentacao);
        }

        private void ProcessBusinessLogic(List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao)
        {
            try
            {

                string ValorLoaded = this.oDataValidationAddRem.GetValueAcao(Valor);

                this.oDataValidationAddRem.SetValueAcao(Valor, ValorLoaded);

                Tools.SincronizarDataValidationAddRemToParam(oListaParametrosMovimentacao, oDataValidationAddRem);

                this.oClassSetMessageDefaults.SetarOk();

                this.oClassSetMessageDefaults.Message.Dado = oListaParametrosMovimentacao;

            }
            catch (Exception ex)
            {
                sqoClassMessageUserException oClassMessageUserException =
                   new sqoClassMessageUserException("Error" + Environment.NewLine + ex.Message, ex.InnerException);

                throw oClassMessageUserException;
            }
        }
    }
}

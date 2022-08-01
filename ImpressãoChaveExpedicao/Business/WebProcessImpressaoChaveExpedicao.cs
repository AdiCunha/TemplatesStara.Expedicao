using AI1627Common20.TemplateDebugging;
using sqoClassLibraryAI0502Biblio;
using sqoClassLibraryAI0502Message;
using sqoClassLibraryAI0502VariaveisSistema;
using sqoClassLibraryAI1151FilaProducao.Estrutura;
using sqoClassLibraryAI1151FilaProducao.Process;
using System.Collections.Generic;
using TemplateStara.Expedicao.ImpressãoChaveExpedicao.DataModel;

namespace TemplateStara.Expedicao.ImpressãoChaveExpedicao.Business
{
    [TemplateDebug("WebProcessImpressaoChaveExpedicao")]
    public class WebProcessImpressaoChaveExpedicao : IProcessMovimentacao
    {
        private sqoClassSetMessageDefaults oClassSetMessageDefaults;
        private ChaveExpedicao oChaveExpedicao;
        private sqoClassParametrosEstrutura oImpressora;
        private string sUsuario = string.Empty;
        WebProcessBusinessLogic oWebProcessBusinessLogic = new WebProcessBusinessLogic();



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
                this.Init(sXmlDados, oListaParametrosListagem);

                oWebProcessBusinessLogic.ValidateMessage();

                oClassSetMessageDefaults.Message = oWebProcessBusinessLogic.ProcessBusinessLogic(oChaveExpedicao.Chave, oChaveExpedicao.Descricao, oChaveExpedicao.Observao);
            }

            return oClassSetMessageDefaults.Message;
        }

        private void Init(string sXmlDados, List<sqoClassParametrosEstrutura> oListaParametrosListagem)
        {           
            this.oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());

            this.oChaveExpedicao = new ChaveExpedicao();

            this.oChaveExpedicao = sqoClassBiblioSerDes.DeserializeObject<ChaveExpedicao>(sXmlDados);

            oImpressora = oListaParametrosListagem.Find(x => x.Campo == "Impressora");

            oWebProcessBusinessLogic.ValidateImpressora(oImpressora.Valor);
        }
    }
}

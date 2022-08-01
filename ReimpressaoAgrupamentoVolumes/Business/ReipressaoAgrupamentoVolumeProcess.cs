using AI1627Common20.TemplateDebugging;
using sqoClassLibraryAI0502Biblio;
using sqoClassLibraryAI0502Message;
using sqoClassLibraryAI0502VariaveisSistema;
using sqoClassLibraryAI1151FilaProducao.Estrutura;
using sqoClassLibraryAI1151FilaProducao.Process;
using System.Collections.Generic;
using TemplateStara.Expedicao.ReimpressaoAgrupamentoVolumes.Business;
using TemplateStara.Expedicao.ReimpressaoAgrupamentoVolumes.DataModel;

namespace TemplateStara.Expedicao.ReimpressaoAgrupamentoVolumes
{
    [TemplateDebug("ReipressaoAgrupamentoVolumeProcess")]
    public class ReipressaoAgrupamentoVolumeProcess : IProcessMovimentacao
    {

        private sqoClassSetMessageDefaults oClassSetMessageDefaults;
        private VolumeExpedicao oVolumeExpedicao;
        private sqoClassParametrosEstrutura oImpressora;
        private string sUsuario = string.Empty;
        ReipressaoAgrupamentoVolumeBusiness oReipressaoAgrupamentoVolumeBusiness = new ReipressaoAgrupamentoVolumeBusiness();

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

                oReipressaoAgrupamentoVolumeBusiness.ValidateMessage();

                oClassSetMessageDefaults.Message = oReipressaoAgrupamentoVolumeBusiness.ProcessBusinessLogic(oVolumeExpedicao.AgrupamentoLogistico, oVolumeExpedicao.QtdVolume, oVolumeExpedicao.GrupoRemessa, sUsuario);
            }

            return oClassSetMessageDefaults.Message;
        }

        private void Init(string sXmlDados, List<sqoClassParametrosEstrutura> oListaParametrosListagem)
        {
            this.oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());

            this.oVolumeExpedicao = new DataModel.VolumeExpedicao();


            this.oVolumeExpedicao = sqoClassBiblioSerDes.DeserializeObject<VolumeExpedicao>(sXmlDados);

            oImpressora = oListaParametrosListagem.Find(x => x.Campo == "Impressora");

            oReipressaoAgrupamentoVolumeBusiness.ValidateImpressora(oImpressora.Valor);
        }
    }
}

using AI1627Common20.TemplateDebugging;
using sqoClassLibraryAI0502Biblio;
using sqoClassLibraryAI1151FilaProducao.Estrutura;
using sqoClassLibraryAI1151FilaProducao.Process;
using System;
using System.Collections.Generic;
using TemplateStara.Expedicao.TransicaoStatus.Dao;
using TemplateStara.Expedicao.TransicaoStatus.DataModel;

namespace sqoTraceabilityStation
{
    [TemplateDebug("WebProcessTransicaoStatusListagem")]
    public class WebProcessTransicaoStatusListagem : sqoClassProcessListar
    {
        private StatusTransitions oStatusTransitions;
        private DaoStatusRemessa oDaoStatusRemessa;
        private MODULO Modulo = MODULO.Invalid;
        private List<sqoClassStatusTransitions> oClassStatusTransitions;

        public override string Executar(string sAction
            , String sXmlDados
            , string sXmlType
            , List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao
            , List<sqoClassParametrosEstrutura> oListaParametrosListagem
            , string sNivel
            , string sUsuario
            , object oObjAux)
        {
            string sReturn = string.Empty;

            Init(sXmlDados);

            sReturn = this.CadastroStatusCarregar();

            return sReturn;
        }

        private void Init(String sXmlDados)
        {
            oStatusTransitions = new StatusTransitions();

            oStatusTransitions = sqoClassBiblioSerDes.DeserializeObject<StatusTransitions>(sXmlDados);

            Enum.TryParse(oStatusTransitions.Module, out this.Modulo);
           
        }

        private string CadastroStatusCarregar()
        {
            DaoStatusRemessa oDaoStatusRemessa = new DaoStatusRemessa();

            switch (Modulo)
            {
                case MODULO.Remessa:
                    {
                        oClassStatusTransitions = oDaoStatusRemessa.GetStatusRemessa();
                        break;
                    }

                case MODULO.Item:
                    {
                        oClassStatusTransitions = oDaoStatusRemessa.GetStatusRemessaItem();
                        break;
                    }
                case MODULO.Grupo:
                    {
                        oClassStatusTransitions = oDaoStatusRemessa.GetStatusRemessaGrupo();
                        break;
                    }
                case MODULO.Volume:
                    {
                        oClassStatusTransitions = oDaoStatusRemessa.GetStatusRemessaVolume();
                        break;
                    }

                default:
                    {
                        throw new NotImplementedException();
                    }
            }

            return MontarXmlFilaProducao(oClassStatusTransitions);
        }

        private string MontarXmlFilaProducao(List<sqoClassStatusTransitions> oClassStatusTransitions)
        {
            string sXmlResult = "";

            sqoClassDetailsStatus details = new sqoClassDetailsStatus();
            details.Details = new List<sqoClassItemDetailBaseStatus>();

            foreach (sqoClassItemDetailBaseStatus oClassStatuslist in oClassStatusTransitions)
                details.Details.Add(oClassStatuslist);

            sXmlResult = sqoClassBiblioSerDes.SerializeObject(details);

            if (sXmlResult.Length > 0)
                sXmlResult = sXmlResult.Remove(0, 1);

            return sXmlResult;
        }
    }
}

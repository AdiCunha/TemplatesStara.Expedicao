using System;
using TemplateStara.Expedicao.TransicaoStatus.Dao;
using TemplateStara.Expedicao.TransicaoStatus.DataModel;

namespace TemplateStara.Expedicao.TransicaoStatus.Business
{
    public class WebProcessVerificarModulo
    {
        private MODULO Modulo = MODULO.Invalid;

        public void AlterarStatusExpedicao(StatusTransitionsValues oStatusTransitionsValues, string CheckRegra)
        {
            Enum.TryParse(oStatusTransitionsValues.Modulo, out this.Modulo);

            DaoStatusRemessa oDaoStatusRemessa = new DaoStatusRemessa();

            switch (Modulo)
            {
                case MODULO.Remessa:
                    {
                        oDaoStatusRemessa.UpdateStatusRemessa(oStatusTransitionsValues, CheckRegra);
                        break;
                    }
                case MODULO.Item:
                    {
                        oDaoStatusRemessa.UpdateStatusRemessaItem(oStatusTransitionsValues, CheckRegra);
                        break;
                    }
                case MODULO.Grupo:
                    {
                        oDaoStatusRemessa.UpdateStatusRemessaGrupo(oStatusTransitionsValues, CheckRegra);
                        break;
                    }
                case MODULO.Volume:
                    {
                        oDaoStatusRemessa.UpdateStatusRemessaVolume(oStatusTransitionsValues, CheckRegra);
                        break;
                    }
            }
        }
    }
}


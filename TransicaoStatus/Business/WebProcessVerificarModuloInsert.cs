using System;
using TemplateStara.Expedicao.TransicaoStatus.Dao;
using TemplateStara.Expedicao.TransicaoStatus.DataModel;

namespace TemplateStara.Expedicao.TransicaoStatus.Business
{
    public class WebProcessVerificarModuloInsert
    {
        private MODULO Modulo = MODULO.Invalid;

        public void InserirStatusExpedicao(StatusTransitionsInsert oStatusTransitionsInsert, string CheckRegra)
        {
            Enum.TryParse(oStatusTransitionsInsert.Modulo, out this.Modulo);

            DaoStatusRemessa oDaoStatusRemessa = new DaoStatusRemessa();

            switch (Modulo)
            {
                case MODULO.Remessa:
                    {
                        oDaoStatusRemessa.InserirTransicaoStatusRemessa(oStatusTransitionsInsert, CheckRegra);
                        break;
                    }
                case MODULO.Item:
                    {
                        oDaoStatusRemessa.InserirTransicaoStatusItem(oStatusTransitionsInsert, CheckRegra);
                        break;
                    }
                case MODULO.Grupo:
                    {
                        oDaoStatusRemessa.InserirTransicaoStatusGrupo(oStatusTransitionsInsert, CheckRegra);
                        break;
                    }
                case MODULO.Volume:
                    {
                        oDaoStatusRemessa.InserirTransicaoStatusVolume(oStatusTransitionsInsert, CheckRegra);
                        break;
                    }
            }
        }
    }
}

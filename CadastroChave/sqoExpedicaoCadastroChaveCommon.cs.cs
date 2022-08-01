using sqoClassLibraryAI1151FilaProducao.Persistencia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace sqoTraceabilityStation
{
    public class sqoExpedicaoCadastroChaveCommon
    {

        public string ValidaAcessoUsuarioVsChave(ChaveUsuarioValuesMov OChaveUsuarioValues, sqoExpedicaoChave oSqoTipoExpedicaoUsuario, string sTipoExpedicao)
        {

            if (OChaveUsuarioValues.Separacao && !oSqoTipoExpedicaoUsuario.Separacao)
            {
                sTipoExpedicao += "Separação; ";
            }

            if (OChaveUsuarioValues.Entrega && !oSqoTipoExpedicaoUsuario.Entrega)
            {
                sTipoExpedicao += "Entrega; ";
            }

            if (OChaveUsuarioValues.Carregamento && !oSqoTipoExpedicaoUsuario.Carregamento)
            {
                sTipoExpedicao += "Carregamento; ";
            }

            return sTipoExpedicao;
        }


    }
}

using System;
using TemplateStara.Expedicao.CadastroComponente.DataModel;

namespace sqoTraceabilityStation
{
    public class ProcessCadCompValidacoes
    {
        public string ValidacaoesCadastro(CadastroComponente oCadastroComponente)
        {

            CadastroComponenteDao oCadastroComponenteDao = new CadastroComponenteDao();

            string sMessageErro = string.Empty;

            if (string.IsNullOrEmpty(oCadastroComponente.Material))
            {
                sMessageErro += "Campo Material é obrigatório o preenchimento!" + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(oCadastroComponente.DescricaoComponente))
            {
                sMessageErro += "Campo Descrição Componente é obrigatório o preenchimento!" + Environment.NewLine;
            }

            if (oCadastroComponente.Tipo < 0)
            {
                sMessageErro += "Campo Tipo Componente é obrigatório!" + Environment.NewLine;
            }

            if (!oCadastroComponenteDao.GetPeca(oCadastroComponente.Material))
            {
                sMessageErro += "Material: " + oCadastroComponente.Material + " inválido! Favor preencher com um Material existente!" + Environment.NewLine;
            }

            if (!oCadastroComponenteDao.GetTipoComponente(oCadastroComponente.Tipo))
            {
                sMessageErro += "Tipo Componente: " + oCadastroComponente.Tipo + " inválido! Preencher com um tipo Válido!" + Environment.NewLine;
            }

            if (oCadastroComponenteDao.GetComponenteMaterial(oCadastroComponente))
            {
                sMessageErro += "Material: " + oCadastroComponente.Material
                    + " | Descrição Componente: " + oCadastroComponente.DescricaoComponente
                    + " | Tipo: "  + oCadastroComponente.Tipo
                    + " já cadastrado na base de dados." 
                    + Environment.NewLine;
            }

            if (oCadastroComponente.Grupo == null)
            {
                sMessageErro += "Campo Grupo Componente é obrigatório! - Selecione um grupo da lista ou a opção em branco." + Environment.NewLine;
            }

            return sMessageErro;
        }

        public string ValidateUpdate(CadastroComponente oCadastroComponente)
        {
            CadastroComponenteDao oCadastroComponenteDao = new CadastroComponenteDao();

            string sMessageErro = string.Empty;

            if (oCadastroComponenteDao.GetComponenteAlteracao(oCadastroComponente))
            {
                sMessageErro += "Pelo menos 1 item deve ser alterado" + Environment.NewLine;
            }

            return sMessageErro;
        }
    }
}

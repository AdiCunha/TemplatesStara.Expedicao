using sqoTraceabilityStation;
using System;
using TemplateStara.Expedicao.CadastroComponente.Dao;

namespace TemplateStara.Expedicao.CadastroComponente.Business
{
    public class ProcessImportCadCompValidacoes
    {
        ImportCadastroComponenteDao oImportCadastroComponenteDao = new ImportCadastroComponenteDao();

        public string ValidateFillColumns(LinhaPlanilha oLinha)
        {
            string sMessageErro = string.Empty;


            if (oLinha.Operacao.Equals("E"))
            {

                if (!oImportCadastroComponenteDao.GetCadastroMaterialComponente(oLinha.Material, oLinha.DescricaoComponente, oLinha.TipoComponente))
                {
                    sMessageErro += "Linha: " + oLinha.LineNumber + " - Material: " + oLinha.Material + " / Descrição: " + oLinha.DescricaoComponente + " não encontrado no cadastro de componente materiais. " + Environment.NewLine;
                }

                if (string.IsNullOrEmpty(oLinha.Material))
                {
                    sMessageErro += "Linha: " + oLinha.LineNumber + " - Coluna *Material* obrigatório preenchimento: " + Environment.NewLine;
                }

                if (string.IsNullOrEmpty(oLinha.DescricaoComponente))
                {
                    sMessageErro += "Linha: " + oLinha.LineNumber + " - Coluna *Descrição Componente* obrigatório preenchimento: " + Environment.NewLine;
                }

                if (string.IsNullOrEmpty(oLinha.TipoComponente))
                {
                    sMessageErro += "Linha: " + oLinha.LineNumber + " - Coluna *Tipo Componente* obrigatório preenchimento: " + Environment.NewLine;
                }

                if (string.IsNullOrEmpty(oLinha.Operacao))
                {
                    sMessageErro += "Linha: " + oLinha.LineNumber + " - Coluna *Operação* obrigatório preenchimento: " + Environment.NewLine;
                }

                if (!oImportCadastroComponenteDao.GetTipoComponente(Convert.ToInt32(oLinha.TipoComponente)))
                {
                    sMessageErro += "Linha: " + oLinha.LineNumber + " - Tipo inválido: " + oLinha.TipoComponente + Environment.NewLine;
                }

            }

            if (oLinha.Operacao.Equals("I"))
            {
                if (!oImportCadastroComponenteDao.GetCadastroMaterial(oLinha.Material))
                {
                    sMessageErro += "Linha: " + oLinha.LineNumber + " - Coluna *Material* - Código: " + oLinha.Material + " não encontrado no cadastro de materiais. " + Environment.NewLine;
                }

                if (string.IsNullOrEmpty(oLinha.Material))
                {
                    sMessageErro += "Linha: " + oLinha.LineNumber + " - Coluna *Material* obrigatório preenchimento: " + Environment.NewLine;
                }

                if (string.IsNullOrEmpty(oLinha.DescricaoComponente))
                {
                    sMessageErro += "Linha: " + oLinha.LineNumber + " - Coluna *Descrição Componente* obrigatório preenchimento: " + Environment.NewLine;
                }

                if (string.IsNullOrEmpty(oLinha.TipoComponente))
                {
                    sMessageErro += "Linha: " + oLinha.LineNumber + " - Coluna *Tipo Componente* obrigatório preenchimento: " + Environment.NewLine;
                }

                if (string.IsNullOrEmpty(oLinha.Operacao))
                {
                    sMessageErro += "Linha: " + oLinha.LineNumber + " - Coluna *Operação* obrigatório preenchimento: " + Environment.NewLine;
                }

                if (oLinha.Grupo != "" && !oImportCadastroComponenteDao.GetGrupoExpedicao(oLinha.Grupo))
                {
                    sMessageErro += "Linha: " + oLinha.LineNumber + " - Grupo inválido: " + oLinha.Grupo + Environment.NewLine;
                }

                if (!oImportCadastroComponenteDao.GetTipoComponente(Convert.ToInt32(oLinha.TipoComponente)))
                {
                    sMessageErro += "Linha: " + oLinha.LineNumber + " - Tipo inválido: " + oLinha.TipoComponente + Environment.NewLine;
                }

                if (oLinha.Operacao.ToUpper().Equals("I"))
                {
                    if (oImportCadastroComponenteDao.GetComponenteMaterial(oLinha))
                    {
                        sMessageErro += "Linha: " + oLinha.LineNumber + " - Valores já inseridos na base de dados! (Material - Descrição Componente - Tipo Componente), ao menos um destes valores devem ser diferentes do cadastro." + Environment.NewLine;
                    }
                }

            }

            else if (oLinha.Operacao.ToUpper().Equals("A"))
            {
                if (!oImportCadastroComponenteDao.GetComponenteMaterial(oLinha))
                {
                    sMessageErro += "Linha: " + oLinha.LineNumber + " - Valores inexistentes na base de dados." + Environment.NewLine;
                }

                if (oImportCadastroComponenteDao.GetDadosAlteracao(oLinha))
                {
                    sMessageErro += "Nenhum dado alterado na Linha:" + oLinha.LineNumber + Environment.NewLine;
                }
            }
            
            return sMessageErro;
        }
    }
}

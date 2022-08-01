using System;
using TemplateStara.Expedicao.TransicaoStatus.Dao;
using TemplateStara.Expedicao.TransicaoStatus.DataModel;

namespace TemplateStara.Expedicao.TransicaoStatus.Business
{
    public class WebProcessTransicaoStatusInserirValidacoes
    {
        private string RetornoMensagem = string.Empty;
        private DaoStatusRemessa oDaoStatusRemessa;
        private MODULO Modulo = MODULO.Invalid;

        public string ValidarPreenchimentoCampos(int CurrentStatus, int NextStatus, string Modulo, bool Permite, string Mensagem)
        {
            DaoStatusRemessa oDaoStatusRemessa = new DaoStatusRemessa();

            Enum.TryParse(Modulo, out this.Modulo);

            if (CurrentStatus < 0)
            {
                RetornoMensagem += "Campo *Status Atual - Obrigatório preenchimento." + Environment.NewLine;
            }

            if (NextStatus < 0)
            {
                RetornoMensagem += "Campo *Novo Status - Obrigatório preenchimento." + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(Modulo))
            {
                RetornoMensagem += "Campo *Módulo - Obrigatório preenchimento." + Environment.NewLine;
            }

            if (Permite && !string.IsNullOrEmpty(Mensagem))
            {
                RetornoMensagem += "Campo *Mensagem - Deve ser preenchido somente quando o campo *Permitir for FALSE." + Environment.NewLine;
            }

            switch (this.Modulo)
            {
                case MODULO.Remessa:
                    {
                        if (oDaoStatusRemessa.ValidarTransicaoExistenteRemessa(CurrentStatus, NextStatus))
                        {
                            RetornoMensagem += "Modulo: " + Modulo + " - Status Atual: " + CurrentStatus + " para o Novo Status: " + NextStatus + " já cadastrado na base de dados." + Environment.NewLine;
                        }

                        break;
                    }
                case MODULO.Item:
                    {
                        if (oDaoStatusRemessa.ValidarTransicaoExistenteItem(CurrentStatus, NextStatus))
                        {
                            RetornoMensagem += "Modulo: " + Modulo + " - Status Atual: " + CurrentStatus + " para o Novo Status: " + NextStatus + " já cadastrado na base de dados." + Environment.NewLine;
                        }

                        break;
                    }
                case MODULO.Grupo:
                    {
                        if (oDaoStatusRemessa.ValidarTransicaoExistenteGrupo(CurrentStatus, NextStatus))
                        {
                            RetornoMensagem += "Modulo: " + Modulo + " - Status Atual: " + CurrentStatus + " para o Novo Status: " + NextStatus + " já cadastrado na base de dados." + Environment.NewLine;
                        }

                        break;
                    }
                case MODULO.Volume:
                    {
                        if (oDaoStatusRemessa.ValidarTransicaoExistenteVolume(CurrentStatus, NextStatus))
                        {
                            RetornoMensagem += "Modulo: " + Modulo + " - Status Atual: " + CurrentStatus + " para o Novo Status: " + NextStatus + " já cadastrado na base de dados." + Environment.NewLine;
                        }

                        break;
                    }
            }

            return RetornoMensagem;
        }
    }
}

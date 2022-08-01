using AI1627Common20.TemplateDebugging;
using sqoClassLibraryAI0502Biblio;
using sqoClassLibraryAI0502Message;
using sqoClassLibraryAI0502VariaveisSistema;
using sqoClassLibraryAI1151FilaProducao;
using sqoClassLibraryAI1151FilaProducao.Estrutura;
using sqoClassLibraryAI1151FilaProducao.Persistencia;
using sqoClassLibraryAI1151FilaProducao.Process;
using System;
using System.Collections.Generic;
using TemplateStara.Expedicao.TransicaoStatus.Business;
using TemplateStara.Expedicao.TransicaoStatus.Dao;
using TemplateStara.Expedicao.TransicaoStatus.DataModel;

namespace sqoTraceabilityStation
{
    [TemplateDebug("WebProcessTransicaoStatusPersistencia")]
    public class WebProcessTransicaoStatusPersistencia : sqoClassProcessMovimentacao
    {
        private sqoClassSetMessageDefaults oClassSetMessageDefaults;
        private DataValidationAddRem oDataValidationAddRem;
        private StatusTransitions oStatusTransitions;
        private DaoStatusRemessa oDaoStatusRemessa;
        private StatusTransitionsValues oStatusTransitionsValues;

        private int nQtdErros = 0;
        private string sMessage = "Falha na validação de dados";
        private string sDescription = string.Empty;

        public override sqoClassMessage Executar(string sAction, string sXmlDados, string sXmlType, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao, List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sNivel, string sUsuario, object oObjAux)
        {
            using (sqoClassDbConnection oDBConnection = new sqoClassDbConnection())
            {
                this.Init(oListaParametrosMovimentacao, sUsuario, sXmlDados);

                this.ProcessBussinessLogic(oDBConnection, oListaParametrosMovimentacao);
            }

            return this.oClassSetMessageDefaults.Message;
        }

        private void Init(List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao, String sUsuario, String sXmlDados)
        {
            this.oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());

            this.oDataValidationAddRem = Tools.ConverterParamToDataValidationAddRem(oListaParametrosMovimentacao);

            this.oStatusTransitions = new StatusTransitions();

            this.oStatusTransitions = sqoClassBiblioSerDes.DeserializeObject<StatusTransitions>(sXmlDados);

            this.LoadValuesTransicaoStatus();

        }

        private string CheckRegra()
        {
            string Regra = "";

            if (oDataValidationAddRem.GetValueAcao(TRANSICAO_STATUS_FIELD.PERMITE) == "true")
            {
                Regra = "ALLOW";
            }
            else
            {
                Regra = "DENY";
            }

            return Regra;
        }

        private StatusTransitionsValues LoadValuesTransicaoStatus()
        {
            oStatusTransitionsValues = new StatusTransitionsValues()
            {
                CurrentStatus = oDataValidationAddRem.GetValueAcao(TRANSICAO_STATUS_FIELD.CURRENT_STATUS),
                NextStatus = oDataValidationAddRem.GetValueAcao(TRANSICAO_STATUS_FIELD.NEXT_STATUS),
                Permite = Convert.ToString(oDataValidationAddRem.GetValueAcao(TRANSICAO_STATUS_FIELD.PERMITE)),
                Mensagem = oDataValidationAddRem.GetValueAcao(TRANSICAO_STATUS_FIELD.MENSAGEM),
                Modulo = oDataValidationAddRem.GetValueAcao(TRANSICAO_STATUS_FIELD.MODULO)
            };

            return oStatusTransitionsValues;
        }

        private void ProcessBussinessLogic(sqoClassDbConnection oDBConnection, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao)
        {
            try
            {
                oDBConnection.BeginTransaction();

                if (this.oDataValidationAddRem.Acao.Equals(Acao.Add))
                {
                    WebProcessVerificarModulo oWebProcessVerificarModulo = new WebProcessVerificarModulo();

                    oWebProcessVerificarModulo.AlterarStatusExpedicao(oStatusTransitionsValues, this.CheckRegra());
                }

                Tools.SincronizarDataValidationAddRemToParam(oListaParametrosMovimentacao, this.oDataValidationAddRem);

                this.oClassSetMessageDefaults.SetarOk();

                this.oClassSetMessageDefaults.Message.Dado = oListaParametrosMovimentacao;

                oDBConnection.Commit();
            }
            catch (Exception ex)
            {
                sqoClassMessageUserException oClassMessageUserException =
                   new sqoClassMessageUserException("Error" + Environment.NewLine + ex.Message, ex.InnerException);
                oDBConnection.Rollback();

                throw oClassMessageUserException;
            }
        }
    }
}

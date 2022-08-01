using AI1627Common20.TemplateDebugging;
using sqoClassLibraryAI0502Biblio;
using sqoClassLibraryAI0502Message;
using sqoClassLibraryAI0502VariaveisSistema;
using sqoClassLibraryAI1151FilaProducao;
using sqoClassLibraryAI1151FilaProducao.Estrutura;
using sqoClassLibraryAI1151FilaProducao.Process;
using System;
using System.Collections.Generic;
using TemplatesStara.CommonStara;
using TemplateStara.Expedicao.GeracaoNumeroSerieComponente.Dao;
using TemplateStara.Expedicao.GeracaoNumeroSerieComponente.DataModel;

namespace TemplateStara.Expedicao.GeracaoNumeroSerieComponente.Business
{
    [TemplateDebug("Web.ProcessGeracaoNumeroSerie")]
    public class ProcessGeracaoNumeroSerie : IProcessMovimentacao
    {
        private sqoClassSetMessageDefaults oClassSetMessageDefaults;
        private NumeroSerieComponente oNumeroSerieComponente;
        private sqoClassParametrosEstrutura oImpressora;
        private string sUsuario;
        private int nQtdErros = 0;
        private int IdImpressora = 0;
        private string sDescription = string.Empty;
        private string sMessage = "Falha na validação de dados";
        private string sMessageErro = string.Empty;

        public sqoClassMessage Executar(string sAction, string sXmlDados, string sXmlType, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao, List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sNivel, string sUsuario, object oObjAux)
        {
            using (sqoClassDbConnection oDBConnection = new sqoClassDbConnection())
            {
                this.Init(oListaParametrosListagem, sXmlDados, sUsuario);

                this.ValidateMessage();

                this.ProcessBusinessLogic(oDBConnection);
            }

            return oClassSetMessageDefaults.Message;
        }

        private void Init(List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sXmlDados, string sUsuario)
        {
            this.oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());

            this.oNumeroSerieComponente = sqoClassBiblioSerDes.DeserializeObject<NumeroSerieComponente>(sXmlDados);

            this.sUsuario = sUsuario;

        }

        private void ProcessBusinessLogic(sqoClassDbConnection oDBConnection)
        {
            try
            {
                GerarNumeroSerieDao oGerarNumeroSerieDao = new GerarNumeroSerieDao();

                if(oNumeroSerieComponente.IdGeracao == 0)
                {
                    oGerarNumeroSerieDao.InsertNumeroSerie(oNumeroSerieComponente.Material
                                                         , oNumeroSerieComponente.NumeroSerie
                                                         , oNumeroSerieComponente.DocReferencia
                                                         , sUsuario
                                                         , oNumeroSerieComponente.Observacao);

                    this.oClassSetMessageDefaults.SetarOk();
                }

                else
                {
                    oDBConnection.BeginTransaction();

                    oGerarNumeroSerieDao.SetNumeroSerie(oNumeroSerieComponente.NumeroSerie, oNumeroSerieComponente.Observacao, oNumeroSerieComponente.IdGeracao);

                    oDBConnection.Commit();

                    this.oClassSetMessageDefaults.SetarOk();
                }
            }
            catch (Exception ex)
            {
                oDBConnection.Rollback();
                oClassSetMessageDefaults.SetarError(ex);

            }
        }

        private void ValidateMessage()
        {
            if (!string.IsNullOrEmpty(sDescription))
            {
                string sMessageBody = Environment.NewLine + sDescription;

                CommonStara.MessageBox(false, this.sMessage, sMessageBody, sqoClassMessage.MessageTypeEnum.ERROR, oClassSetMessageDefaults);

                throw new sqoClassMessageUserException(oClassSetMessageDefaults.Message);
            }
        }
    }


}
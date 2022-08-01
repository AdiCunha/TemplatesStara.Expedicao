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
    [TemplateDebug("Web.ProcessVincularComponenteListagem")]
    public class ProcessVincularComponenteListagem : sqoClassProcessListar
    {
        private sqoClassSetMessageDefaults oClassSetMessageDefaults;
        private DocumentoReferenciaListagem oDocumentoReferenciaListagem;
        private sqoClassParametrosEstrutura oGrupo;
        private List<DocumentoReferenciaListagemList> oDocumentoReferenciaListagemList;
        private string sDescription = string.Empty;
        private string sUsuario = string.Empty;

        public override string Executar(string sAction
                                    , string sXmlDados
                                    , string sXmlType
                                    , List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao
                                    , List<sqoClassParametrosEstrutura> oListaParametrosListagem
                                    , string sNivel
                                    , string sUsuario
                                    , object oObjAux)
        {
            string sReturn = string.Empty;

            using (sqoClassDbConnection oDBConnection = new sqoClassDbConnection())
            {
                this.Init(sXmlDados, oListaParametrosListagem);

                sReturn = this.ProcessBusinessLogic(oDBConnection);
            }

            return sReturn;
        }

        private void Init(string sXmlDados, List<sqoClassParametrosEstrutura> oListaParametrosListagem)
        {
            this.oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());

            oDocumentoReferenciaListagem = new DocumentoReferenciaListagem();

            oDocumentoReferenciaListagem = sqoClassBiblioSerDes.DeserializeObject<DocumentoReferenciaListagem>(sXmlDados);

        }

        public string ProcessBusinessLogic(sqoClassDbConnection oDBConnection)
        {
            string sReturn = string.Empty;

            oDBConnection.BeginTransaction();

            sReturn = ListDocumentoReferenciaComponente();

            if (string.IsNullOrEmpty(sReturn))
            {
                oDBConnection.Rollback();

                sqoClassMessageUserException oClassMessageUserException = new sqoClassMessageUserException(sDescription);

                throw oClassMessageUserException;
            }

            else
            {
                oDBConnection.Commit();
                return sReturn;
            }
        }

        public string ListDocumentoReferenciaComponente()
        {
            string sReturn = null;

            VincularComponenteListagemDao oVincularComponenteListagemDao = new VincularComponenteListagemDao();

            VincularComponenteSaveDao oVincularComponenteSaveDao = new VincularComponenteSaveDao();

            if (oDocumentoReferenciaListagem.IdGeracao == 0)
            {
                this.sDescription = "Erro: " + Environment.NewLine + "Necessário primeiro gerar o Número de Série do material Pai, se ainda não o tiver em mãos pode ser usado um provisório." + Environment.NewLine;

                return sReturn;
            }

            if (oDocumentoReferenciaListagem.IdGeracao == 0 && oVincularComponenteSaveDao.VerificarQtdOPMaiorQueUm(oDocumentoReferenciaListagem.DocReferencia) == 0)
            {
                oDocumentoReferenciaListagemList = oVincularComponenteListagemDao.GetRastreabilidadeComponenteNaoGerado(oDocumentoReferenciaListagem);

                return MontarXmlFilaProducao(oDocumentoReferenciaListagemList);
            }

            else
            {
                oDocumentoReferenciaListagemList = oVincularComponenteListagemDao.GetRastreabilidadeComponente(oDocumentoReferenciaListagem);

                return MontarXmlFilaProducao(oDocumentoReferenciaListagemList);
            }
        }

        public string MontarXmlFilaProducao(List<DocumentoReferenciaListagemList> oDocumentoReferenciaListagemList)
        {
            string sXmlResult = string.Empty;

            var oDetail = new sqoClassDetails();

            foreach (var oList in oDocumentoReferenciaListagemList)
            {
                oDetail.Add(oList);
            }

            var oXml = oDetail.Serializar();

            return oXml;
        }
    }
}

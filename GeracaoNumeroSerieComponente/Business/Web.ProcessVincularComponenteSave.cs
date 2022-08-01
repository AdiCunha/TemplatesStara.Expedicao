using AI1627Common20.TemplateDebugging;
using sqoClassLibraryAI0502Biblio;
using sqoClassLibraryAI0502Message;
using sqoClassLibraryAI0502VariaveisSistema;
using sqoClassLibraryAI1151FilaProducao.Estrutura;
using sqoClassLibraryAI1151FilaProducao.Process;
using System;
using System.Collections.Generic;
using TemplateStara.Expedicao.GeracaoNumeroSerieComponente.DataModel;
using TemplateStara.Expedicao.GeracaoNumeroSerieComponente.Dao;
using sqoClassLibraryAI1151FilaProducao;
using TemplatesStara.CommonStara;

namespace TemplateStara.Expedicao.GeracaoNumeroSerieComponente.Business
{
    [TemplateDebug("Web.ProcessVincularComponenteSave")]
    public class WProcessVincularComponenteSave : IProcessMovimentacao
    {
        private sqoClassSetMessageDefaults oClassSetMessageDefaults;
        private DocumentoReferenciaListagem oDocumentoReferenciaListagem;
        private VincularComponenteSaveDao oVincularComponenteSaveDao;
        private string sUsuario;
        private string sDescription = string.Empty;
        private string sMessage = "Falha na validação de dados";

        public sqoClassMessage Executar(string sAction
                                        , string sXmlDados
                                        , string sXmlType
                                        , List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao
                                        , List<sqoClassParametrosEstrutura> oListaParametrosListagem
                                        , string sNivel
                                        , string sUsuario
                                        , object oObjAux)
        {
            using (sqoClassDbConnection oDBConnection = new sqoClassDbConnection())
            {
                this.Init(sXmlDados, oListaParametrosListagem, sUsuario);

                this.ProcessBusinessLogic(oDBConnection);

                this.ValidateMessage();
            }

            return oClassSetMessageDefaults.Message;
        }

        private void Init(string sXmlDados, List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sUsuario)
        {
            this.oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());

            this.oDocumentoReferenciaListagem = new DocumentoReferenciaListagem();

            this.oDocumentoReferenciaListagem = sqoClassBiblioSerDes.DeserializeObject<DocumentoReferenciaListagem>(sXmlDados);

            this.sUsuario = sUsuario;
        }

        private void ProcessBusinessLogic(sqoClassDbConnection oDBConnection)
        {
            try
            {
                int ContadorAlteracao = 0;

                foreach (var oItemRegra in oDocumentoReferenciaListagem.ListaRegrasItemFilaProducao)
                {
                    VincularComponenteSaveDao oVincularComponenteSaveDao = new VincularComponenteSaveDao();

                    if (oItemRegra.IdItem > 0)
                    {
                        if (oVincularComponenteSaveDao.GetValorComponente(oItemRegra.IdItem) != oItemRegra.Valor)
                        {
                            oDBConnection.BeginTransaction();
                              
                            oVincularComponenteSaveDao.SetValorVincularComponente(oItemRegra.IdItem, oItemRegra.Valor, oVincularComponenteSaveDao.GetIdHeader(oItemRegra.Valor));

                            oDBConnection.Commit();

                            ContadorAlteracao++;
                        }
                    }

                    else if (!string.IsNullOrEmpty(oItemRegra.Valor))
                    {
                        //int IdNserieGeracao = oVincularComponenteSaveDao.VerificarQtdOPMaiorQueUm(oDocumentoReferenciaListagem.DocReferencia);

                        if(oItemRegra.IdGeracao == 0)
                        {
                            this.sDescription = "Necessário primeiro gerar o Número de Série do material Pai, se ainda não o tiver em mãos pode ser usado um provisório." + Environment.NewLine;
                        }

                        //else if (oItemRegra.IdGeracao == 0 && oVincularComponenteSaveDao.VerificarQtdOPMaiorQueUm(oDocumentoReferenciaListagem.DocReferencia) == 0 && !string.IsNullOrEmpty(oItemRegra.NumeroSerie))
                        //{
                        //    oVincularComponenteSaveDao.InsertDocRefGeracaoComponente(oDocumentoReferenciaListagem.Material, oDocumentoReferenciaListagem.DocReferencia);
                        //}

                        else 
                        {
                            oVincularComponenteSaveDao.InsertValorVincularComponente(oItemRegra.DescricaoComponente, oItemRegra.Valor, sUsuario, oItemRegra.IdGeracao, oVincularComponenteSaveDao.GetIdHeader(oItemRegra.Valor));
                        }

                        ContadorAlteracao++;
                    }
                }

                if (ContadorAlteracao > 0)
                {
                    this.oClassSetMessageDefaults.SetarOk();
                }

                else
                    this.sDescription = "É necessário alterar ao menos um dado para salvar!" + Environment.NewLine;
            }

            catch (Exception ex)
            {
                sqoClassMessageUserException oClassMessageUserException =
                  new sqoClassMessageUserException("Error" + Environment.NewLine + ex.Message, ex.InnerException);
                oDBConnection.Rollback();

                throw oClassMessageUserException;
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
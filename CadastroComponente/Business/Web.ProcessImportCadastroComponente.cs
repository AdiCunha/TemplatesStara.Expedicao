using AI1627Common20.TemplateDebugging;
using sqoClassLibraryAI0502Message;
using sqoClassLibraryAI0502VariaveisSistema;
using sqoClassLibraryAI1151FilaProducao.Estrutura;
using sqoClassLibraryAI1151FilaProducao.Process;
using System;
using System.Collections.Generic;
using TemplatesStara.CommonStara;
using sqoClassLibraryAI1151FilaProducao;
using TemplateStara.Expedicao.CadastroComponente.Dao;

using Common.Stara.Common.Business;
using sqoTraceabilityStation;
using TemplateStara.Expedicao.CadastroComponente.Business;

namespace TemplateStara.Expedicao.GeracaoNumeroSerieComponente
{
    [TemplateDebug("Web.ProcessImportCadastroComponente")]
    public class ImportCadastroComponente : IProcessMovimentacao
    {
        enum Action { Invalid = -1, Insert, Update, Delete, Import, Export, DownloadPdf }
        private Action currentAction = Action.Invalid;
        private ImportCadastroComponenteDao oImportCadastroComponenteDao = new ImportCadastroComponenteDao();
        private ProcessImpCadCompDownloadExcel oProcessImpCadCompDownloadExcel = new ProcessImpCadCompDownloadExcel();
        private ProcessImportCadCompValidacoes oProcessImportCadCompValidacoes = new ProcessImportCadCompValidacoes();

        private string sFileName = string.Empty;
        private string sFilePath = string.Empty;

        public sqoClassMessage Executar(string sAction, string sXmlDados, string sXmlType, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao, List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sNivel, string sUsuario, object oObjAux)
        {
            sqoClassSetMessageDefaults oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());

            Enum.TryParse(sAction, out this.currentAction);

            List<LinhaPlanilha> oLinhaPlanilha = new List<LinhaPlanilha>();

            using (sqoClassDbConnection oDBConnection = new sqoClassDbConnection())
            {
                switch (currentAction)
                {
                    case Action.Import:
                        {
                            foreach (sqoClassParametrosEstrutura item in oListaParametrosMovimentacao)
                            {
                                var tt = item.Campo;
                            }

                            oLinhaPlanilha = SheetBusiness.CarregarPlanilha<LinhaPlanilha>(oListaParametrosMovimentacao);

                            this.ValidarLinhaPlanilha(oLinhaPlanilha, oClassSetMessageDefaults);

                            this.ProcessBusinessLogic(oDBConnection, oLinhaPlanilha, sUsuario, sNivel, sAction);

                            CommonStara.MessageBox(true, "Planilha Importada com sucesso", "Planilha Importada com sucesso", sqoClassMessage.MessageTypeEnum.OK, oClassSetMessageDefaults);

                            break;
                        }

                    case Action.Export:
                        {
                            CommonStara.MessageBox(true, "Planilha Gerada com sucesso", "Planilha Gerada com sucesso", sqoClassMessage.MessageTypeEnum.OK, oClassSetMessageDefaults);

                            oClassSetMessageDefaults.Message.Dado = oProcessImpCadCompDownloadExcel.DownloadExcel();

                            break;
                        }

                    case Action.DownloadPdf:
                        {
                            CarregarParametrosList(oListaParametrosListagem);

                            CommonStara.DownloadPdf(sFilePath, oClassSetMessageDefaults);

                            CommonStara.MessageBox(true, "PDF gerado com sucesso!", "", sqoClassMessage.MessageTypeEnum.OK, oClassSetMessageDefaults);

                            break;
                        }

                    default:
                        break;
                }
            }

            return oClassSetMessageDefaults.Message;
        }

        private void CarregarParametrosList(List<sqoClassParametrosEstrutura> oListaParametrosListagem)
        {
            this.sFilePath = oListaParametrosListagem.Find(x => x.Campo == "FilePath").Valor;
        }

        private void ValidarLinhaPlanilha(List<LinhaPlanilha> Planilha, sqoClassSetMessageDefaults oClassSetMessageDefaults)
        {
            string sMessageErro = "";

            foreach (var oLinha in Planilha)
            {

                var duplicateExists = Planilha.FindAll(x => x.Material == oLinha.Material && x.DescricaoComponente == oLinha.DescricaoComponente && x.TipoComponente == oLinha.TipoComponente).Count > 1;

                if (duplicateExists)
                {
                    sMessageErro += "Linha: " + oLinha.LineNumber + " duplicada!" + Environment.NewLine;
                }

                sMessageErro += oProcessImportCadCompValidacoes.ValidateFillColumns(oLinha);
            }

            if (!string.IsNullOrEmpty(sMessageErro))
            {
                CommonStara.MessageBox(false, "Falha na validação de dados", sMessageErro, sqoClassMessage.MessageTypeEnum.ERROR, oClassSetMessageDefaults);

                throw new sqoClassMessageUserException(oClassSetMessageDefaults.Message);
            }
        }

        private void ProcessBusinessLogic(sqoClassDbConnection oDBConnection, List<LinhaPlanilha> Planilha, string sUsuario, string sNivel, string sAction)
        {
            oDBConnection.BeginTransaction();

            try
            {
                Planilha.ForEach(x => { x.DescricaoComponente = x.DescricaoComponente.ToUpper(); });

                foreach (var oLinha in Planilha)
                {
                    if (oLinha.Operacao.ToUpper().Equals("I"))
                    {
                        oImportCadastroComponenteDao.SaveComponente(oLinha, sUsuario);
                    }

                    if (oLinha.Operacao.ToUpper().Equals("A"))
                    {
                        oImportCadastroComponenteDao.UpdateComponente(oLinha, sUsuario);
                    }

                    else if (oLinha.Operacao.ToUpper().Equals("E"))
                    {
                        oImportCadastroComponenteDao.DeleteComponenteImport(oLinha);
                    }

                    CommonStara.GravarHistorico(oDBConnection, oLinha, sNivel, sFileName, sUsuario, oLinha.Operacao == "I" ? Operation.INSERT : Operation.UPDATE);
                }

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
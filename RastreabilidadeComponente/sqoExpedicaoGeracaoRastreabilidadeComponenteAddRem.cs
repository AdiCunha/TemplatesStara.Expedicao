using System.Data;
using sqoClassLibraryAI0502VariaveisSistema;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using sqoClassLibraryAI0502Biblio;
using sqoClassLibraryAI1151FilaProducao.Estrutura;
using AI1627Common20.TemplateDebugging;
using sqoClassLibraryAI1151FilaProducao.Process;
using System.Xml.Serialization;
using sqoClassLibraryAI1151FilaProducao;
using TemplatesStara.CommonStara;
using sqoClassLibraryAI0502Message;
using sqoClassLibraryAI1151FilaProducao.Persistencia;

namespace sqoTraceabilityStation
{
    [TemplateDebug("sqoExpedicaoGeracaoRastreabilidadeComponenteAddRem")]
    public class sqoExpedicaoGeracaoRastreabilidadeComponenteAddRem : IProcessMovimentacao
    {
        private sqoClassSetMessageDefaults oClassSetMessageDefaults;
        private DataValidationAddRem oDataValidationAddRem;

        private int nQtdErros = 0;
        private String sMessage = "Falha na validação de dados";
        private String sDescription = String.Empty;

        //private sqoClassParametrosEstrutura oQtdEtiqueta;
        //private sqoClassParametrosEstrutura oImpressora;

        private const String sDescricaoComponente = "DescricaoComponente";
        private const String sNumeroSerie = "NumeroSerie";
        private const String sMaterial = "MaterialList";


        public sqoClassMessage Executar(string sAction, string sXmlDados, string sXmlType, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao, List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sNivel, string sUsuario, object oObjAux)
        {
            using (sqoClassDbConnection oDBConnection = new sqoClassDbConnection())
            {
                this.Init(sXmlDados, oListaParametrosMovimentacao, oListaParametrosListagem);

                //this.Validate();

                this.ProcessBusinessLogic(oListaParametrosMovimentacao);
            }

            return oClassSetMessageDefaults.Message;
        }

        private void Init(String sXmlDados, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao, List<sqoClassParametrosEstrutura> oListaParametrosListagem)
        {
            this.oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());

            this.oDataValidationAddRem = Tools.ConverterParamToDataValidationAddRem(oListaParametrosMovimentacao);

        }

        private void Validate()
        {
            if (this.oDataValidationAddRem.Acao == Acao.Edit)
            {
                if (String.IsNullOrEmpty(oDataValidationAddRem.GetValueAcao(sDescricaoComponente)))
                {
                    this.nQtdErros++;

                    this.sDescription += this.nQtdErros.ToString() + " - Obrigatório preencher o número de série de todos os componentes!"
                        + Environment.NewLine;
                }

                if (String.IsNullOrEmpty(this.oDataValidationAddRem.GetValueAcao(sNumeroSerie)))
                {
                    this.nQtdErros++;

                    this.sDescription += this.nQtdErros.ToString() + " - Campo N° Série obrigatório, favor preencher!" + Environment.NewLine;
                }

            }
            this.ValidateMessage();
        }

        private void ValidateMessage()
        {
            if (!String.IsNullOrEmpty(sDescription))
            {
                String sMessageDescription = nQtdErros > 1 ? ("Encontrados " + nQtdErros + " erros!")
                    : ("Encontrado " + nQtdErros + " erro!");

                String sMessageBody = sMessageDescription + Environment.NewLine + sDescription;

                CommonStara.MessageBox(false, this.sMessage, sMessageBody, sqoClassMessage.MessageTypeEnum.ERROR, oClassSetMessageDefaults);

                throw new sqoClassMessageUserException(oClassSetMessageDefaults.Message);
            }
        }

        private void ProcessBusinessLogic(List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao)
        {
            try
            {

                String sNumeroSerieUpper = this.oDataValidationAddRem.GetValueAcao(sNumeroSerie).ToUpper();

                this.oDataValidationAddRem.SetValueAcao(sNumeroSerie, sNumeroSerieUpper);


                Tools.SincronizarDataValidationAddRemToParam(oListaParametrosMovimentacao, oDataValidationAddRem);

                this.oClassSetMessageDefaults.SetarOk();

                this.oClassSetMessageDefaults.Message.Dado = oListaParametrosMovimentacao;

            }
            catch (Exception ex)
            {
                sqoClassMessageUserException oClassMessageUserException =
                   new sqoClassMessageUserException("Error" + Environment.NewLine + ex.Message, ex.InnerException);

                throw oClassMessageUserException;
            }
        }
    }
}
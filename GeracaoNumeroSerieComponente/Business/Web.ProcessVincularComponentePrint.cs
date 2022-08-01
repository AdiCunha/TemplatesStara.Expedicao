using AI1627Common20.TemplateDebugging;
using Newtonsoft.Json;
using sqoClassLibraryAI0502Biblio;
using sqoClassLibraryAI0502Message;
using sqoClassLibraryAI0502VariaveisSistema;
using sqoClassLibraryAI1151FilaProducao;
using sqoClassLibraryAI1151FilaProducao.Estrutura;
using sqoClassLibraryAI1151FilaProducao.Process;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using TemplatesStara.CommonStara;
using TemplateStara.Expedicao.GeracaoNumeroSerieComponente.Dao;
using TemplateStara.Expedicao.GeracaoNumeroSerieComponente.DataModel;


namespace TemplateStara.Expedicao.GeracaoNumeroSerieComponente.Business
{
    [TemplateDebug("Web.ProcessVincularComponentePrint")]
    public class ProcessVincularComponenteSave : IProcessMovimentacao
    {
        private sqoClassSetMessageDefaults oClassSetMessageDefaults;
        private NumeroSerieGeracao oNumeroSerieGeracao;
        private sqoClassParametrosEstrutura oImpressora;
        private string sUsuario = string.Empty;
        private string IdImpressora;
        private Exception ex = null;
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

                this.ValidateMessage();

                this.ProcessBusinessLogic();
            }

            return oClassSetMessageDefaults.Message;
        }

        private void Init(string sXmlDados, List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sUsuario)
        {
            this.oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());

            this.oNumeroSerieGeracao = new NumeroSerieGeracao();

            this.oNumeroSerieGeracao = sqoClassBiblioSerDes.DeserializeObject<NumeroSerieGeracao>(sXmlDados);

            oImpressora = oListaParametrosListagem.Find(x => x.Campo == "Impressora");

            IdImpressora = oImpressora.Valor;

            this.ValidateImpressora(IdImpressora);

            this.ValidateGeracao(oNumeroSerieGeracao);

            this.sUsuario = sUsuario;
        }

        private void ProcessBusinessLogic()
        {
            object oNumeroSerieGeracaoLoad = this.NumeroSerieGeracaoCarregarValores(oNumeroSerieGeracao);

            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://columba/sequor-spool/PrintService.svc/v1/print/label/serial-number-generation");

            httpWebRequest.ContentType = "application/json";

            httpWebRequest.Method = "POST";


            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = JsonConvert.SerializeObject(oNumeroSerieGeracaoLoad);

                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();

                if (result.Contains("successfully printed"))
                {
                    this.oClassSetMessageDefaults.SetarOk();
                }

                else
                    oClassSetMessageDefaults.SetarError(ex);
            }

        }

        public object NumeroSerieGeracaoCarregarValores(NumeroSerieGeracao oNumeroSerieGeracao)
        {
            ImprimirGeracaoNumeroSerieDao oImprimirGeracaoNumeroSerieDao = new ImprimirGeracaoNumeroSerieDao();

            oNumeroSerieGeracao.IdPrinter = Convert.ToInt32(IdImpressora);

            oNumeroSerieGeracao.DescricaoMaterial = oImprimirGeracaoNumeroSerieDao.GetDescricaoMaterial(oNumeroSerieGeracao.Material);

            oNumeroSerieGeracao.Usuario = sUsuario;

            oNumeroSerieGeracao.DateGeracao = oImprimirGeracaoNumeroSerieDao.GetDataGeracao(oNumeroSerieGeracao.Material, oNumeroSerieGeracao.NrSerie);

            oNumeroSerieGeracao.DataImpressao = DateTime.Now.ToString();

            return oNumeroSerieGeracao;
        }

        private void ValidateImpressora(string IdImpressora)
        {
            if (string.IsNullOrEmpty(IdImpressora))
            {
                this.sDescription = "É necessário selecionar uma impressora!" + Environment.NewLine;
            }

            else
                Convert.ToInt32(IdImpressora);
        }

        private void ValidateGeracao(NumeroSerieGeracao oNumeroSerieGeracao)
        {
            ImprimirGeracaoNumeroSerieDao oImprimirGeracaoNumeroSerieDao = new ImprimirGeracaoNumeroSerieDao();

            if (string.IsNullOrEmpty(oNumeroSerieGeracao.NrSerie))
            {
                this.sDescription = "Não é possível imprimir a etiqueta antes de gerar o número de série!" + Environment.NewLine;
            }

            if (!oImprimirGeracaoNumeroSerieDao.PermitirImpressao(oNumeroSerieGeracao.DocReferencia, oNumeroSerieGeracao.NrSerie))
            {
                this.sDescription = "Não é possível imprimir a etiqueta antes de gerar o número de série de todos os componentes!" + Environment.NewLine;
            }

            else
                Convert.ToInt32(IdImpressora);
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

using Newtonsoft.Json;
using sequor.spool.data.datarequest;
using sequor.spool.data.dataResult;
using sqoClassLibraryAI0502Message;
using sqoClassLibraryAI1151FilaProducao;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using TemplatesStara.CommonStara;

namespace TemplateStara.Expedicao.ReimpressaoAgrupamentoVolumes.Business
{
    public class ReipressaoAgrupamentoVolumeBusiness
    {
        private sqoClassSetMessageDefaults oClassSetMessageDefaults;
        private string sDescription = string.Empty;
        private string sMessage = "Falha na validação de dados";
        private int nIdImpressora;
        private string RetornoMensagem = string.Empty;

        public sqoClassMessage ProcessBusinessLogic(Guid AgrupamentoLogistico, string QtdVolume, string GrupoRemessa, string Usuario)
        {
            DateTime DataGeracao = DateTime.Now;

            GenericDataRequest oSpoolGenericRequest = new GenericDataRequest()
            {
                IdPrinter = nIdImpressora,
                Module = "EXPEDICAO_AGRUPAMENTO_VOLUMES",

                Replaces = new List<AI1627Common40.SoapWebService.Model.KeyValue>()
                {
                    new AI1627Common40.SoapWebService.Model.KeyValue()
                    {
                        Key = "<codigo>", Value = Convert.ToString(AgrupamentoLogistico)
                    },

                    new AI1627Common40.SoapWebService.Model.KeyValue()
                    {
                        Key = "<vol>", Value = QtdVolume
                    },

                    new AI1627Common40.SoapWebService.Model.KeyValue()
                    {
                        Key = "<grupo_remessa>", Value = GrupoRemessa
                    },

                      new AI1627Common40.SoapWebService.Model.KeyValue()
                    {
                        Key = "<usuario>", Value = Usuario
                    },

                        new AI1627Common40.SoapWebService.Model.KeyValue()
                    {
                        Key = "<data_geracao>", Value = Convert.ToString(DataGeracao)
                    }
                }
            };

            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://columba/sequor-spool/PrintService.svc/v1/print/label/generic");

            httpWebRequest.ContentType = "application/json";

            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = JsonConvert.SerializeObject(oSpoolGenericRequest);

                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                this.oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());

                var result = streamReader.ReadToEnd();

                DefaultResult deserializedReturn = JsonConvert.DeserializeObject<DefaultResult>(result);

                if (deserializedReturn.Ok)
                {
                    this.oClassSetMessageDefaults.SetarOk();
                }

                else
                {
                    sDescription = deserializedReturn.Message;

                    this.ValidateMessage();
                }
            }

            return oClassSetMessageDefaults.Message;
        }

        public void ValidateImpressora(string IdImpressora)
        {
            if (string.IsNullOrEmpty(IdImpressora))
            {
                this.sDescription = "É necessário selecionar uma impressora!" + Environment.NewLine;
            }

            else
                nIdImpressora = Convert.ToInt32(IdImpressora);
        }

        public void ValidateMessage()
        {
            if (!string.IsNullOrEmpty(sDescription))
            {
                this.oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());

                string sMessageBody = Environment.NewLine + sDescription;

                CommonStara.MessageBox(false, this.sMessage, sMessageBody, sqoClassMessage.MessageTypeEnum.ERROR, oClassSetMessageDefaults);

                throw new sqoClassMessageUserException(oClassSetMessageDefaults.Message);
            }
        }
    }
}

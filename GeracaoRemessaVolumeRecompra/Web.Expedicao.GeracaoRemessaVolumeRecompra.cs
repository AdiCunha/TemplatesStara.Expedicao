using AI1627Common20.TemplateDebugging;
using sqoClassLibraryAI0502Message;
using sqoClassLibraryAI1151FilaProducao.Estrutura;
using sqoClassLibraryAI1151FilaProducao.Process;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;
using sqoClassLibraryAI0502VariaveisSistema;
using sqoClassLibraryAI0502Biblio;
using sqoClassLibraryAI1151FilaProducao;
using AI1627CommonInterface;
using Common.Stara.Common.Business;
using Common.Stara.Common.DataModel;

namespace TelasDinamicas.Expedicao
{
    [TemplateDebug("Web.Expedicao.GeracaoRemessaVolumeRecompra")]
    public class GeracaoRemessaVolumeRecompra : IProcessMovimentacao
    {
        private sqoClassSetMessageDefaults oClassSetMessageDefaults;
        private sqoClassLESEXPRemessaPersistence oRemessa;
        private sqoClassLESEXPRemessaItensPersistence oItem;
        private sqoClassLESEXPRemessaItensNrSeriePersistence oNrSerie;
        private GeracaoRemessaVolumeDataModel oGeracaoRemessaVolumeEstrutura;

        enum Action { Invalid = -1, inserir }
        private Action currentAction = Action.Invalid;

        private String sUsuario = String.Empty;
        private String sOrigemLancamento = String.Empty;
        private String sMessage = String.Empty;
        private int nQtdErros = 0;
        private sqoClassParametrosEstrutura oNrSerieParametro;
        private sqoClassParametrosEstrutura oMaterialParametro;


        public sqoClassMessage Executar(string sAction, string sXmlDados, string sXmlType, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao, List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sNivel, string sUsuario, object oObjAux)
        {
            using (sqoClassDbConnection oDBConnection = new sqoClassDbConnection())
            {



                this.Init(oListaParametrosListagem, oListaParametrosMovimentacao, sXmlDados, sAction, sUsuario);

                this.ValidateParametro(oDBConnection);

                switch (currentAction)
                {
                    case Action.inserir:
                        {
                            this.Validate(oDBConnection);

                            this.FillRemessa(oDBConnection);

                            this.ProcessInsert(oDBConnection);

                            break;
                        }
                }
            }

            this.oClassSetMessageDefaults.SetarOk();

            return oClassSetMessageDefaults.Message;
        }

        private void Init(List<sqoClassParametrosEstrutura> oListaParametrosListagem, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao, String sXmlDados, String sAction, String sUsuario)
        {
            this.oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());

            sXmlDados = sXmlDados.Replace("null", "");
            sXmlDados = sXmlDados.Replace("undefined", "");


            //this.oGeracaoRemessaVolumeEstrutura = sqoClassBiblioSerDes.DeserializeObject<GeracaoRemessaVolumeDataModel>(sXmlDados);

            this.oGeracaoRemessaVolumeEstrutura = SheetBusiness.XmlToObject<GeracaoRemessaVolumeDataModel>(sXmlDados);

            this.oNrSerieParametro = oListaParametrosListagem.Find(x => x.Campo == ("NR_SERIE"));

            this.oMaterialParametro = oListaParametrosListagem.Find(x => x.Campo == ("MATERIAL"));

            this.sUsuario = sUsuario;

            Enum.TryParse(sAction, out this.currentAction);

        }

        private void Validate(sqoClassDbConnection oDBConnection)
        {
            this.ValidateCurrentNrSerie(oDBConnection);

            this.ValidateDocumentoReferencia();

            this.ValidateObservacao();

            this.ValidateDeposito();

            this.ValidateMessage();
        }


        private void ValidateParametro(sqoClassDbConnection oDBConnection)
        {

            this.ValidateONrSerie();

            this.ValidateMaterial();

        }

        public void FillRemessa(sqoClassDbConnection classDbConnection)
        {

            this.oRemessa = new sqoClassLESEXPRemessaPersistence()
            {
                CodigoRemessa = Convert.ToString(oGeracaoRemessaVolumeEstrutura.DocumentoReferencia)
               ,
                Observacao = Convert.ToString(oGeracaoRemessaVolumeEstrutura.Observacao)
               ,
                DataRemessa = DateTime.Now
               ,
                DataUltimaMovimentacao = DateTime.Now
               ,
                DocumentoFaturamento = oGeracaoRemessaVolumeEstrutura.DocumentoReferencia
               ,
                LocalExpedicao = oGeracaoRemessaVolumeEstrutura.Deposito
               ,
                ModificadoEm = DateTime.Now
               ,
                Usuario = sUsuario
               ,
                UsuarioUltimaMovimentacao = sUsuario

            };

            this.oItem = new sqoClassLESEXPRemessaItensPersistence()
            {
                CodigoProduto = oMaterialParametro.Valor
                ,
                DataUltimaMovimentacao = DateTime.Now
                ,
                DocumentoReferencia = oGeracaoRemessaVolumeEstrutura.DocumentoReferencia
                ,
                ItemRemessa = oMaterialParametro.Valor
                ,
                LocalDeposito = oGeracaoRemessaVolumeEstrutura.Deposito
                ,
                NumeroSerie = oNrSerieParametro.Valor
                ,
                Observacao = oGeracaoRemessaVolumeEstrutura.Observacao
                ,
                UsuarioUltimaMovimentacao = sUsuario

            };

            this.oNrSerie = new sqoClassLESEXPRemessaItensNrSeriePersistence()
            {
                NrSerie = oNrSerieParametro.Valor
               ,
                CodigoRemessa = oGeracaoRemessaVolumeEstrutura.DocumentoReferencia
               ,
                DataUltimaMovimentacao = DateTime.Now
               ,
                ItemRemessa = oMaterialParametro.Valor
               ,
                Observacao = oGeracaoRemessaVolumeEstrutura.Observacao
               ,
                UsuarioUltimaMovimentacao = sUsuario

            };



            oItem.LiClassLESEXPRemessaItensNrSeriePersistence.Add(oNrSerie);

            oRemessa.LiClassLESEXPRemessaItensPersistence.Add(oItem);

        }


        private void ValidateCurrentNrSerie(sqoClassDbConnection oDBConnection)
        {
            List<sqoClassLESEXPRemessaItensNrSeriePersistence> oLiCurrentNrSerie =
               AI1627CommonInterface.sqoClassLESEXPRemessaItensNrSerieControlerDB.GetLESExpRemessaItensNrSerieByNrSerie(oNrSerieParametro.Valor, oDBConnection);

            if (oLiCurrentNrSerie != null && oLiCurrentNrSerie.Count > 0 && !String.IsNullOrEmpty(oLiCurrentNrSerie.First().NrSerie))
            {
                this.nQtdErros++;

                this.sMessage += this.nQtdErros.ToString() + " - Nr de série: " + oNrSerieParametro.Valor + " já existe!" + Environment.NewLine;
            }

        }

        private void ValidateDocumentoReferencia()
        {

            if (String.IsNullOrEmpty(this.oGeracaoRemessaVolumeEstrutura.DocumentoReferencia))
            {
                this.nQtdErros++;

                this.sMessage += this.nQtdErros.ToString() + " - Campo Documento de Referência não foi preenchido! " + Environment.NewLine;
            }

        }

        private void ValidateObservacao()
        {

            if (String.IsNullOrEmpty(this.oGeracaoRemessaVolumeEstrutura.Observacao))
            {
                this.nQtdErros++;

                this.sMessage += this.nQtdErros.ToString() + " - Campo Observação não foi preenchido! " + Environment.NewLine;
            }

        }

        private void ValidateONrSerie()
        {

            if ((String.IsNullOrEmpty(this.oNrSerieParametro.Valor)))
            {

                this.nQtdErros++;

                this.sMessage += " - Parâmetro Número de Série não foi preenchido! " + Environment.NewLine;

                this.ValidateMessage();

                throw new sqoClassMessageUserException(sMessage);


                //this.sMessage += this.nQtdErros.ToString() + " - Parâmetro Número de Série não foi preenchido! " + Environment.NewLine;

            }

        }

        private void ValidateMaterial()
        {

            if (String.IsNullOrEmpty(this.oMaterialParametro.Valor))
            {
                this.nQtdErros++;

                this.sMessage += this.nQtdErros.ToString() + " - Parâmetro Material não foi preenchido! " + Environment.NewLine;

                this.ValidateMessage();

                throw new sqoClassMessageUserException(sMessage);

            }

        }


        private void ValidateDeposito()
        {

            if (String.IsNullOrEmpty(this.oGeracaoRemessaVolumeEstrutura.Deposito))
            {
                this.nQtdErros++;

                this.sMessage += this.nQtdErros.ToString() + " - Campo Depósito não foi preenchido! " + Environment.NewLine;
            }

        }

        private void ProcessInsert(sqoClassDbConnection oDBConnection)

        {

            try
            {
                oDBConnection.BeginTransaction();

                this.oRemessa.Insert();

                oDBConnection.Commit();

            }
            catch (Exception ex)
            {
                sqoClassMessageUserException oClassMessageUserException =
                   new sqoClassMessageUserException("Error " + Environment.NewLine + ex.Message, ex.InnerException);
                oDBConnection.Rollback();

                throw oClassMessageUserException;
            }
        }

        private void ValidateMessage()
        {
            if (!String.IsNullOrEmpty(this.sMessage))
            {
                String sMessageHeader = "Falha na validação de dados";

                String sMessageDescription = nQtdErros > 1 ? ("Encontrados " + nQtdErros + " erros!")
                    : ("Encontrado " + nQtdErros + " erro!");

                String sMessageBody = sMessageDescription + Environment.NewLine + this.sMessage;

                TemplatesStara.CommonStara.CommonStara.MessageBox(false, sMessageHeader, sMessageBody, sqoClassMessage.MessageTypeEnum.ERROR, oClassSetMessageDefaults);

                throw new sqoClassMessageUserException(oClassSetMessageDefaults.Message);
            }
        }
    }

    /// <summary>
    /// Classe para instanciar o modelo do criteria da tela.
    /// </summary>
    [PlanilhaInfo("ItemFilaProducao")]
    public class GeracaoRemessaVolumeDataModel
    {
        [PlanilhaColunaInfo(-1, "DOCUMENTO_REFERENCIA")]
        public String DocumentoReferencia { get; set; }

        [PlanilhaColunaInfo(-1, "OBSERVACAO")]
        public String Observacao { get; set; }

        [PlanilhaColunaInfo(-1, "DEPOSITO")]
        public String Deposito { get; set; }
        //public String Deposito { get { return Deposito; } set
        /*public String Deposito { get { return Deposito; } set*/
        //{ Deposito = (value != null && value == "null") ? null : value; } }

    }



}
using sqoClassLibraryAI0502VariaveisSistema;
using System;
using System.Collections.Generic;
using sqoClassLibraryAI0502Biblio;
using sqoClassLibraryAI1151FilaProducao.Estrutura;
using sqoClassLibraryAI0502Message;
using AI1627Common20.TemplateDebugging;
using sqoClassLibraryAI1151FilaProducao.Process;
using sqoClassLibraryAI1151FilaProducao;
using TemplatesStara.CommonStara;
using AI1627CommonInterface;

namespace TemplateStara.Expedicao.GeracaoVolume
{
    [TemplateDebug("sqoExpedicaoGeracaoVolumeEstorno")]
    class sqoExpedicaoGeracaoVolumeEstorno : sqoClassProcessMovimentacao
    {
        private sqoClassSetMessageDefaults oClassSetMessageDefaults;
        private sqoClassGeracaoVolume oClassGeracaoVolume;

        private List<sqoClassLESExpedicaoVolumePersistence> oListVolume;
        private List<sqoClassPcp2PecaVolumePersistence> oListVolumeCad;

        private sqoClassParametrosEstrutura oOrdemProducao;
        private sqoClassParametrosEstrutura oTipoVolume;

        private int nQtdErros = 0;
        private String sMessage = "Falha na validação de dados";
        private String sDescription = String.Empty;
        private String sUsuario = String.Empty;

        public override sqoClassMessage Executar(string sAction, string sXmlDados, string sXmlType, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao,
            List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sNivel, string sUsuario, object oObjAux)
        {
            using (sqoClassDbConnection oDBConnection = new sqoClassDbConnection())
            {
                this.Init(sXmlDados, oListaParametrosListagem, sUsuario, oDBConnection);

                //this.Validate();

                this.ProcessBusinessLogic(oDBConnection);

            }

            return this.oClassSetMessageDefaults.Message;
        }

        private void Init(String sXmlDados, List<sqoClassParametrosEstrutura> oListaParametrosListagem, String sUsuario, sqoClassDbConnection oDBConnection)
        {
            this.oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());

            //this.oClassGeracaoVolume = new sqoClassGeracaoVolume();

            this.oClassGeracaoVolume = sqoClassBiblioSerDes.DeserializeObject<sqoClassGeracaoVolume>(sXmlDados);

            this.oOrdemProducao = oListaParametrosListagem.Find(x => x.Campo == ("ORDEM_PRODUCAO"));

            this.oTipoVolume = oListaParametrosListagem.Find(x => x.Campo == ("TIPO_EXPEDICAO"));

            this.oListVolume = sqoClassLESExpedicaoVolumeControlerDB.GetExpedicaoVolumesByOrdemProducaoAndTipoExpedicao(this.oOrdemProducao.Valor, this.oTipoVolume.Valor,oDBConnection);

            this.oListVolumeCad = sqoClassLESExpedicaoVolumeCadastroControlerDB.GetPcp2PecaVolumeByCodigoPaiAndTipoVolume
                (this.oClassGeracaoVolume.CodigoPai, this.oTipoVolume.Valor, oDBConnection);

            this.sUsuario = sUsuario;

        }

        private void ProcessBusinessLogic(sqoClassDbConnection oDBConnection)
        {
            try
            {
                oDBConnection.BeginTransaction();

                int nQtdVolumes = this.oListVolume.Count;
                int nQtdNaoEstornado = 0;

                string sCodigoVolumeAtual = "";
                int qtdVolumeAtual = 0;

                foreach (var oItem in this.oListVolume)
                {
                    if (sCodigoVolumeAtual == oItem.CodigoVolume)
                    {
                        qtdVolumeAtual++;
                    }
                    else
                    {
                        sCodigoVolumeAtual = oItem.CodigoVolume;
                        qtdVolumeAtual = 1;
                    }

                    var oVolCad = oListVolumeCad.Find(x => x.CodigoVolume == oItem.CodigoVolume
                        && (x.Ativo == true || x.CodigoPai == oItem.CodigoVolume));

                    int qtd = 0;

                    if (oVolCad != null)
                        qtd = oVolCad.Quantidade;

                    if (((this.oListVolumeCad.FindAll(x => x.CodigoVolume == oItem.CodigoVolume).Count == 0)
                        && oItem.Status != AI1627CommonInterface.LESStatus.STATUS_EXPEDICAO_VOLUME.ESTORNADO)
                        || (qtdVolumeAtual > qtd && oItem.Status != AI1627CommonInterface.LESStatus.STATUS_EXPEDICAO_VOLUME.ESTORNADO))
                    {
                        List<sqoClassLESEstoqueAtualPersistence> oEstoque = sqoClassLESEstoqueAtualControlerDB.GetLESEstoqueAtual
                                    (null
                                    , null
                                    , oItem.CodigoVolume
                                    , oItem.CodigoRastreabilidade
                                    , null
                                    , " > 0 "
                                    , null
                                    , true
                                    , oDBConnection
                                    );

                        if (oEstoque.Count == 0)
                        {
                            sqoClassLESExpedicaoVolumePersistence oListResult = new sqoClassLESExpedicaoVolumePersistence();

                            oListResult = oItem;

                            sqoClassDefaultPersistence oResult =
                            AI1627CommonInterface.sqoClassLESExpedicaoVolumeControlerDB.ExecuteWsqolUpdateExpedicaoVolume
                                    ( nId: oListResult.Id
                                    , sCodigoVolume: oListResult.CodigoVolume
                                    , nStatus: AI1627CommonInterface.LESStatus.STATUS_EXPEDICAO_VOLUME.ESTORNADO
                                    , sUsuarioUltimaMovimentacao : sUsuario
                                    );

                            if (!oResult.Ok)
                                throw new Exception("Erro na atualização de status " + oResult.Message);
                        }
                        else
                        {
                            this.nQtdErros++;

                            this.sDescription += this.nQtdErros + " - Não é permitido estornar volume com saldo em estoque Código Volume: "
                                + oItem.CodigoVolume + ", Código Rastreabilidade: " + oItem.CodigoRastreabilidade + " !" + Environment.NewLine;
                        }
                    }
                    else
                    {
                        nQtdNaoEstornado++;

                        if (nQtdVolumes.Equals(nQtdNaoEstornado))
                        {
                            this.nQtdErros++;

                            this.sDescription += this.nQtdErros + " - Nenhum volume diferente do cadastro para estorno!" + Environment.NewLine; ;
                        }
                    }

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

            if (!String.IsNullOrEmpty(this.sDescription))
            {
                this.ValidateMessage();
            }
            else
            {
                this.oClassSetMessageDefaults.SetarOk();
            }

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
    }
}
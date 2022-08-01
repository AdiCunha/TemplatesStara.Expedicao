using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AI1627Common20.TemplateDebugging;
using sqoClassLibraryAI0502Message;
using sqoClassLibraryAI0502VariaveisSistema;
using sqoClassLibraryAI1151FilaProducao.Estrutura;
using sqoClassLibraryAI1151FilaProducao.Process;
using AI1627CommonInterface;
using Common.Stara.LES.Pcp2Peca.DataModel;
using Common.Stara.LES.Pcp2Peca.Business;
using sqoTraceabilityStation.SeqProducaoRoteiroProcessoProgram;
using sqoClassLibraryAI1151FilaProducao;
using Common.Stara.MES.Cadastro.DataModel;
using Common.Stara.MES.Cadastro.Business;
using Common.Stara.LES.Expedicao.Business;
using Common.Stara.MES.OrdemProducao.DataModel;
using sqoClassLibraryAI0502Biblio;
using AI1627CommonInterface.LESStatus;

namespace TelasDinamicas.Expedicao
{
    [TemplateDebug("Web.Expedicao.GeracaoVolumeRetrabalhoPA")]
    public class GeracaoVolumeRetrabalhoPA : IProcessMovimentacao
    {
        private sqoClassSetMessageDefaults oClassSetMessageDefaults;
        private GeracaoVolumeRetrabalho oGeracaoVolumeRetrabalho;
        private List<sqoClassLESExpedicaoVolumePersistence> oListVolume;
        private List<Pcp2PecaVolume> oListPcp2PecaVolume;
        private sqoSeqProducaoRoteiroProcesso oSeqProducaoRoteiroProcesso;
        private Pcp2ParImportacaoFila oPcp2ParImportacaoFila;
        private List<SeqProducao> oListSeqProducao;
        private Pcp2Peca oPcp2PecaCodigoPai;
        private Pcp2Peca oPcp2PecaCodigoVolume;

        enum Action { Invalid = -1, reverse, print, generate }
        private Action currentAction = Action.Invalid;

        private sqoClassParametrosEstrutura oImpressora;
        private sqoClassParametrosEstrutura oNrSerie;
        private sqoClassParametrosEstrutura oTipoVolume;

        private String sUsuario = String.Empty;
        private String sOrigemLancamento = String.Empty;
        private String sInfoOrigemLancamento = String.Empty;
        private String sInfoOrigemLancamento2 = String.Empty;
        private String sObservacao = String.Empty;
        private String sMessage = String.Empty;
        private int nQtdErros = 0;
        private String sCodigoProdutoOP = String.Empty;

        public sqoClassMessage Executar(string sAction, string sXmlDados, string sXmlType, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao, List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sNivel
            , string sUsuario, object oObjAux)
        {
            using (sqoClassDbConnection oDBConnection = new sqoClassDbConnection())
            {
                this.Init(oListaParametrosListagem, oListaParametrosMovimentacao, sXmlDados, sAction, sUsuario);

                switch (currentAction)
                {
                    case Action.generate:
                        {
                            this.InitGenerate();

                            this.ValidateGenerate();

                            this.ProcessBusinessLogicGenerate(oDBConnection);

                            break;
                        }

                    case Action.reverse:
                        {
                            this.ValidateReverse();

                            this.ProcessBusinessLogicReverse(oDBConnection);

                            break;
                        }

                    case Action.print:
                        {
                            this.InitPrint();

                            this.ValidatePrint();

                            this.ProcessBusinessPrint(oDBConnection);

                            break;
                        }
                }

            }

            this.oClassSetMessageDefaults.SetarOk();

            return this.oClassSetMessageDefaults.Message;
        }

        private void Init(List<sqoClassParametrosEstrutura> oListaParametrosListagem, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao,
            String sXmlDados, String sAction, String sUsuario)
        {
            this.oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());

            this.oGeracaoVolumeRetrabalho = new GeracaoVolumeRetrabalho();

            this.oGeracaoVolumeRetrabalho = sqoClassBiblioSerDes.DeserializeObject<GeracaoVolumeRetrabalho>(sXmlDados);

            this.oImpressora = oListaParametrosListagem.Find(x => x.Campo == ("IMPRESSORA"));
            this.oNrSerie = oListaParametrosListagem.Find(x => x.Campo == ("NR_SERIE"));
            this.oTipoVolume = oListaParametrosListagem.Find(x => x.Campo == ("TIPO_VOLUME"));

            this.oListVolume = sqoClassLESExpedicaoVolumeControlerDB.GetExpedicaoVolumesByOrdemProducao(this.oNrSerie.Valor);

            this.sUsuario = sUsuario;

            Enum.TryParse(sAction, out this.currentAction);

        }

        private void InitGenerate()
        {
            this.sCodigoProdutoOP = "RET_" + this.oGeracaoVolumeRetrabalho.OrdemProducao; //código da op de retrabalho

            this.oListPcp2PecaVolume = Pcp2PecaVolumeBusiness.GetPcp2PecaVolumeByCodigoPaiAndTipoVolume(this.oGeracaoVolumeRetrabalho.CodigoPai, this.oTipoVolume.Valor);

            this.FillOperacao();

            sqoSeqProducaoRoteiroProcessoPersistence SeqProducaoRoteiroProcessoPersistence = new sqoSeqProducaoRoteiroProcessoPersistence(null);

            this.oSeqProducaoRoteiroProcesso = SeqProducaoRoteiroProcessoPersistence.GetRoteiroProcesso(this.oGeracaoVolumeRetrabalho.OrdemProducao, this.oGeracaoVolumeRetrabalho.Operacao);

            if (this.oSeqProducaoRoteiroProcesso == null)
                throw new sqoClassMessageUserException("Ordem de Produção " + this.oGeracaoVolumeRetrabalho.OrdemProducao + " e Operação " + this.oGeracaoVolumeRetrabalho.Operacao + " não encontrado na base de dados!");
           
            this.oPcp2ParImportacaoFila = Pcp2ParImportacaoFilaBusiness.GetPcp2ParImportacaoFilaByLinhaMontagemAndEstacao(this.oSeqProducaoRoteiroProcesso.LinhaMontagem, this.oSeqProducaoRoteiroProcesso.Estacao);

            this.oListSeqProducao = Common.Stara.MES.OrdemProducao.Business.SeqProducaoBusinesss.GetSeqProducaoByOrdemProducao(this.oGeracaoVolumeRetrabalho.OrdemProducao);
        }

        private void InitPrint()
        {
            this.oPcp2PecaCodigoPai = Pcp2PecaBusiness.GetPcp2PecaByCodigoPeca(this.oGeracaoVolumeRetrabalho.CodigoPai);

            this.oPcp2PecaCodigoVolume = Pcp2PecaBusiness.GetPcp2PecaByCodigoPeca(this.oGeracaoVolumeRetrabalho.CodigoVolume);
        }

        private void ValidateGenerate()
        {
            if (String.IsNullOrEmpty(this.oGeracaoVolumeRetrabalho.OrdemProducao))
            {
                this.nQtdErros++;

                this.sMessage += this.nQtdErros.ToString() + " - Campo Ordem de Produção obrigatório!" + Environment.NewLine;
            }

            if (String.IsNullOrEmpty(this.oGeracaoVolumeRetrabalho.Operacao))
            {
                this.nQtdErros++;

                this.sMessage += this.nQtdErros.ToString() + " - Campo Operação obrigatório!" + Environment.NewLine;
            }

            else
            {
                if (this.oGeracaoVolumeRetrabalho.Print)
                    this.ValidatePrinter();

                this.ValidateNrSerie(); //por no testes

                this.ValidateVolume();

                if (!this.oListPcp2PecaVolume.Any(x => x.CodigoPai != x.CodigoVolume))
                {
                    this.nQtdErros++;

                    this.sMessage += this.nQtdErros.ToString() + " - Não possui volumes cadastrado para o Código Pai: " + this.oGeracaoVolumeRetrabalho.CodigoPai + " Tipo Expedição: " + this.oTipoVolume.Valor + Environment.NewLine;
                }

                this.ValidateSeqProducao();

                if (this.oSeqProducaoRoteiroProcesso.Equals(null))
                {
                    this.nQtdErros++;

                    this.sMessage += this.nQtdErros.ToString() + " - Não foi possível encontrar Ordem de Produção : " + this.oGeracaoVolumeRetrabalho.OrdemProducao + " e Operação: " + this.oGeracaoVolumeRetrabalho.Operacao + Environment.NewLine;
                }

                if (oPcp2ParImportacaoFila.Equals(null) || String.IsNullOrEmpty(oPcp2ParImportacaoFila.LocalDebitoPadraoLes))
                {
                    this.nQtdErros++;

                    this.sMessage += this.nQtdErros.ToString() + " - Não foi possível encontrar local débito padrão Estação : " + this.oSeqProducaoRoteiroProcesso.Estacao + Environment.NewLine;
                }
            }
            this.ValidateMessage();
        }


        private void ValidateReverse()
        {
            this.ValidateNrSerie();

            this.ValidateVolume();

            this.ValidateMessage();
        }

        private void ValidatePrint()
        {
            this.ValidatePrinter();

            this.ValidateVolume();

            this.ValidateMessage();
        }

        private void ProcessBusinessLogicGenerate(sqoClassDbConnection oDBConnection)
        {
            try
            {
                oDBConnection.BeginTransaction();

                foreach (var oVolume in oListVolume)
                {
                    this.ValidateStockDebit(oVolume.CodigoVolume, oVolume.CodigoRastreabilidade, oPcp2ParImportacaoFila.LocalDebitoPadraoLes);

                    //não deve ocorrer o débito da máquina base
                    if (oVolume.TipoVolume.Equals(EXPEDICAO_VOLUME_TIPO.VOLUME_MATERIAL) && String.IsNullOrEmpty(oVolume.TipoExpedicao))
                    {
                        this.StoreMovEstoqueDebito(oVolume.CodigoVolume, oVolume.CodigoRastreabilidade, oPcp2ParImportacaoFila.LocalDebitoPadraoLes);

                        sqoClassLESExpedicaoVolumeControlerDB.ExecuteWsqolUpdateExpedicaoVolume(oVolume.Id, nStatus: AI1627CommonInterface.LESStatus.STATUS_EXPEDICAO_VOLUME.ESTORNADO, sObservacao: "Retrabalho OP: " + this.oGeracaoVolumeRetrabalho.OrdemProducao);
                    }

                }

                var oPcp2PecaVolumeVolume = this.oListPcp2PecaVolume.FindAll(x => x.CodigoPai != x.CodigoVolume); //obter apenas os volumes, nao retornar máquina base

                //Retirar da lista volumes inativos
                List<Pcp2PecaVolume> oListPcp2PecaVolumeVolume = this.RemovePecaVolume(oPcp2PecaVolumeVolume);

                int nContador = oListVolume.Max(x => x.Contador) + 1;

                int nSeqCodigoRastreabilidade = nContador > 1 ? nContador - 1 : nContador;

                foreach (var oPcp2PecaVol in oListPcp2PecaVolumeVolume)
                {
                    for (int i = 0; i < oPcp2PecaVol.Quantidade; i++)
                    {
                        String sCodigoRastreabilidade = "&" + this.oNrSerie.Valor + "." + nSeqCodigoRastreabilidade.ToString();

                        this.InsertVolume(oPcp2PecaVol.CodigoVolume, sCodigoRastreabilidade, EXPEDICAO_VOLUME_TIPO.VOLUME_MATERIAL, nContador, this.oTipoVolume.Valor);

                        this.StoreMovEstoqueCredito(oPcp2PecaVol.CodigoVolume, sCodigoRastreabilidade);

                        if (this.oGeracaoVolumeRetrabalho.Print)
                        {
                            Pcp2Peca oPcp2PecaPai = Pcp2PecaBusiness.GetPcp2PecaByCodigoPeca(oPcp2PecaVol.CodigoPai);

                            Pcp2Peca oPcp2PecaVolume = Pcp2PecaBusiness.GetPcp2PecaByCodigoPeca(oPcp2PecaVol.CodigoVolume);

                            VolumeBusiness.PrintEtiquetaVolume(
                                long.Parse(this.oImpressora.Valor)
                                , oPcp2PecaVol.CodigoPai
                                , oPcp2PecaPai.Descricao
                                , String.Empty
                                , String.Empty
                                , String.Empty
                                , this.sUsuario
                                , nContador.ToString()
                                , sCodigoRastreabilidade
                                , oPcp2PecaVol.CodigoVolume
                                , oPcp2PecaVolume.Descricao
                                );

                        }

                        nSeqCodigoRastreabilidade++;

                        nContador++;
                    }
                }

                //oDBConnection.Rollback();
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

        private void ProcessBusinessLogicReverse(sqoClassDbConnection oDBConnection)
        {
            sqoClassDefaultPersistence oResult;

            try
            {
                oDBConnection.BeginTransaction();

                oResult = VolumeBusiness.EstornarVolume(this.oNrSerie.Valor, oTipoVolume.Valor, this.oGeracaoVolumeRetrabalho.CodigoPai);
            }
            catch (Exception ex)
            {
                sqoClassMessageUserException oClassMessageUserException =
                   new sqoClassMessageUserException("Error " + Environment.NewLine + ex.Message, ex.InnerException);
                oDBConnection.Rollback();

                throw oClassMessageUserException;
            }

            if (oResult.Ok.Equals(false))
            {
                this.nQtdErros++;

                this.sMessage += this.nQtdErros.ToString() + " - " + oResult.Message;

                this.ValidateMessage();
            }

            //oDBConnection.Rollback();
            oDBConnection.Commit();
        }

        private void ProcessBusinessPrint(sqoClassDbConnection oDBConnection)
        {
            try
            {
                oDBConnection.BeginTransaction();

                EXPEDICAO_VOLUME_TIPO TipoVolume = this.oGeracaoVolumeRetrabalho.CodigoPai.Equals(this.oGeracaoVolumeRetrabalho.CodigoVolume) ? EXPEDICAO_VOLUME_TIPO.MATERIAL : EXPEDICAO_VOLUME_TIPO.VOLUME_MATERIAL;

                if (TipoVolume.Equals(EXPEDICAO_VOLUME_TIPO.MATERIAL))
                {
                    VolumeBusiness.PrintEtiquetaPA(
                        long.Parse(this.oImpressora.Valor)
                        , this.oGeracaoVolumeRetrabalho.CodigoPai
                        , oPcp2PecaCodigoPai.Descricao
                        , String.Empty
                        , String.Empty
                        , String.Empty
                        , this.sUsuario
                        , this.oGeracaoVolumeRetrabalho.Contador.ToString()
                        , oGeracaoVolumeRetrabalho.CodigoRastreabilidade
                        );
                }

                else if (TipoVolume.Equals(EXPEDICAO_VOLUME_TIPO.VOLUME_MATERIAL))
                {
                    VolumeBusiness.PrintEtiquetaVolume(
                        long.Parse(this.oImpressora.Valor)
                        , this.oGeracaoVolumeRetrabalho.CodigoPai
                        , oPcp2PecaCodigoPai.Descricao
                        , String.Empty
                        , String.Empty
                        , String.Empty
                        , this.sUsuario
                        , this.oGeracaoVolumeRetrabalho.Contador.ToString()
                        , oGeracaoVolumeRetrabalho.CodigoRastreabilidade
                        , oGeracaoVolumeRetrabalho.CodigoVolume
                        , oPcp2PecaCodigoVolume.Descricao
                        );
                }

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


        private void StoreMovEstoqueDebito(String sCodigoVolume, String sCodigoRastreabilidade, String sLocal)
        {
            this.FillMovData(sCodigoVolume);

            long nIdMov = sqoClassLESEstoqueAtualControlerDB.ExecuteStoreMovEstoqueDebito(
                        sLocal
                        , sCodigoVolume
                        , 1 //volume sempre quantidade 1
                        , this.sUsuario
                        , sCodigoRastreabilidade
                        , null
                        , AI1627CommonInterface.LESStatus.STATUS_MOVIMENTACAO.LIBERADO_LES
                        , String.Empty
                        , true
                        , String.Empty
                        , sOrigemLancamento
                        , sInfoOrigemLancamento
                        , sInfoOrigemLancamento2
                        , sObservacao
                        );

            if (nIdMov <= 0)
            {
                this.nQtdErros++;

                this.sMessage += this.nQtdErros.ToString() + " - Erro ao executar débito do estoque, Local: " + sLocal + " Código Produto: " + sCodigoVolume + " Código Rastreabilidade: " + sCodigoRastreabilidade + "!" + Environment.NewLine;
            }

        }

        private List<Pcp2PecaVolume> RemovePecaVolume(List<Pcp2PecaVolume> oListPcp2PecaVolume)
        {
            List<Pcp2PecaVolume> oListRemove = new List<Pcp2PecaVolume>();

            foreach (var oPcp2PecaVolume in oListPcp2PecaVolume)
            {
                var oPcp2Peca = Pcp2PecaBusiness.GetPcp2PecaByCodigoPeca(oPcp2PecaVolume.CodigoVolume);

                if (oPcp2Peca.Ativo.Equals(false))
                {
                    oListRemove.Add(oPcp2PecaVolume);
                }
            }

            foreach (var oRemove in oListRemove)
            {
                oListPcp2PecaVolume.Remove(oRemove);
            }

            return oListPcp2PecaVolume;
        }

        private void StoreMovEstoqueCredito(String sCodigoVolume, String sCodigoRastreabilidade)
        {
            this.FillMovData(sCodigoVolume);

            long nIdMov = sqoClassLESEstoqueAtualControlerDB.ExecuteStoreMovEstoqueCredito(
                        this.oPcp2ParImportacaoFila.LocalCreditoLes
                        , sCodigoVolume
                        , 1
                        , this.sUsuario
                        , sCodigoRastreabilidade
                        , null
                        , STATUS_MOVIMENTACAO.BLOQUEADO_NO_LES
                        , String.Empty
                        , true
                        , String.Empty
                        , sOrigemLancamento
                        , sInfoOrigemLancamento
                        , sInfoOrigemLancamento2
                        , sObservacao
                        );

            if (nIdMov <= 0)
                throw new Exception("Erro ao executar crédito no estoque, Local: " + this.oPcp2ParImportacaoFila.LocalCreditoLes + " Código Produto: " + sCodigoVolume + " Código Rastreabilidade: " + sCodigoRastreabilidade + "!");

        }


        private void FillMovData(String sCodigoVolume)
        {
            this.sOrigemLancamento = "Geração volume retrabalho recompra";
            this.sInfoOrigemLancamento = "Código Pai: " + this.oGeracaoVolumeRetrabalho.CodigoPai;
            this.sInfoOrigemLancamento2 = "Código Volume: " + sCodigoVolume;
            this.sObservacao = "Geração volume recompra web - " + this.oGeracaoVolumeRetrabalho.CodigoPai + " / " + sCodigoVolume;
        }

        private void InsertVolume(String sCodigoVolume, String sCodigoRastreabilidade, EXPEDICAO_VOLUME_TIPO TipoVolume, int nQtdEtiqueta, String sTipoExpedicao)
        {
            sqoClassLESExpedicaoVolumePersistence oVolume = new sqoClassLESExpedicaoVolumePersistence();

            oVolume.IdRemessa = 0;
            oVolume.IdDocumentoTransporte = 0;
            oVolume.CodigoRemessa = String.Empty;
            oVolume.CodigoVolume = sCodigoVolume;
            oVolume.CodigoRastreabilidade = sCodigoRastreabilidade;
            oVolume.OrdemProducao = this.oNrSerie.Valor;
            oVolume.TipoVolume = TipoVolume;
            oVolume.PesoTotalCalculado = 0;
            oVolume.PesoTotalVolume = 0;
            oVolume.ChaveExpedicaoPlanejamento = String.Empty;
            oVolume.LocalEntrega = String.Empty;
            oVolume.TipoPlanejamentoLes = EXPEDICAO_TIPO_PLANEJAMENTO_LES.NAO_PLANEJADO;
            oVolume.DataSeparacaoPlanejamento = null;
            oVolume.DataTransportePlanejamento = null;
            oVolume.ObrigarSequenciaPlanejamento = 0;
            oVolume.DocTransporteAuxPlanejamento = String.Empty;
            oVolume.TamCaminhaoPlanejamento = String.Empty;
            oVolume.DetalhePlanejamento = String.Empty;
            oVolume.AgrupamentoLogistico = Guid.Empty;
            oVolume.AgrupamentoCarregamento = Guid.Empty;
            oVolume.Modulo = "WEB";
            oVolume.Status = AI1627CommonInterface.LESStatus.STATUS_EXPEDICAO_VOLUME.CRIADO;
            oVolume.UsuarioUltimaMovimentacao = this.sUsuario;
            oVolume.DataUltimaMovimentacao = DateTime.Now;
            oVolume.Observacao = "Volume gerado via Web - EXPEDICAO_VOLUME_GERACAO_RETRABALHO_PA";
            oVolume.Contador = nQtdEtiqueta;
            oVolume.TipoExpedicao = sTipoExpedicao;

            oVolume.Insert();
        }

        private void ValidateMessage()
        {
            if (!String.IsNullOrEmpty(this.sMessage))
            {
                String sMessageHeader = "Falha na validação de dados";

                String sMessageDescription = nQtdErros > 1 ? ("Encontrados " + nQtdErros + " erros!")
                    : ("Encontrado " + nQtdErros + " erro!");

                String sMessageBody = sMessageDescription + Environment.NewLine + this.sMessage;

                //TemplatesStara.CommonStara.CommonStara.MessageBox(false, sMessageHeader, sMessageBody, sqoClassMessage.MessageTypeEnum.ERROR, this.oClassSetMessageDefaults);

                oClassSetMessageDefaults.Message.Ok = false;
                oClassSetMessageDefaults.Message.Message = sMessageHeader;
                oClassSetMessageDefaults.Message.MessageDescription = sMessageBody;
                oClassSetMessageDefaults.Message.MessageType = sqoClassMessage.MessageTypeEnum.ERROR;

                throw new sqoClassMessageUserException(oClassSetMessageDefaults.Message);
            }
        }

        private void ValidateStockDebit(String sCodigoVolume, String sCodigoRastreabilidade, String sLocal)
        {
            List<sqoClassLESEstoqueAtualPersistence> oListEstoque = sqoClassLESEstoqueAtualControlerDB.GetLESEstoqueAtual(
                    oCodigoProduto: sCodigoVolume
                    , oCodigoRastreabilidade: sCodigoRastreabilidade
                    , oQuantidade: " > 0"
                    , oLocal: sLocal);

            if (!oListEstoque.Any())
            {
                //this.nQtdErros++;

                //this.sMessage += this.nQtdErros.ToString() + " - Não foi possível encontrar estoque disponível no local " + sLocal + ", Código Produto: " + sCodigoVolume + " Código Rastreabilidade: " + sCodigoRastreabilidade
                //    + "!" + Environment.NewLine;

                throw new sqoClassMessageUserException(this.nQtdErros.ToString() + " - Não foi possível encontrar estoque disponível no local " + sLocal + ", Código Produto: " + sCodigoVolume + " Código Rastreabilidade: " + sCodigoRastreabilidade
                    + "!" + Environment.NewLine);

                //this.ValidateMessage();
            }
        }

        private void ValidateNrSerie()
        {
            List<sqoClassLESEXPRemessaItensNrSeriePersistence> oListItensNrSerie =
                sqoClassLESEXPRemessaItensNrSerieControlerDB.GetLESExpRemessaItensNrSerieByNrSerie(this.oNrSerie.Valor);

            if (oListItensNrSerie.Any())
            {
                foreach (var oItensNrSerie in oListItensNrSerie)
                {
                    List<sqoClassLESEXPRemessaItensPersistence> oListItens = sqoClassLESEXPRemessaItensControlerDB.GetLESExpRemessaItensByIdItem(oItensNrSerie.IdRemessaItem);

                    foreach (var oItens in oListItens)
                    {
                        if (!(oItens.CodigoProduto.Equals(this.oGeracaoVolumeRetrabalho.CodigoPai)))
                        {
                            this.nQtdErros++;

                            this.sMessage += this.nQtdErros.ToString() + " - Número de Série " + oItensNrSerie.NrSerie + " pertence ao Material " + oItens.CodigoProduto + " !" + Environment.NewLine;
                        }
                    }
                }
            }
            else
                throw new Exception("Lista \"oListItensNrSerie\" não possui nenhum registro");
        }

        private void FillOperacao()
        {
            while (this.oGeracaoVolumeRetrabalho.Operacao.Length < 4)
            {
                this.oGeracaoVolumeRetrabalho.Operacao = "0" + this.oGeracaoVolumeRetrabalho.Operacao;
            }
        }

        private void ValidateSeqProducao()
        {
            if (!this.oListSeqProducao.Any())
            {
                this.nQtdErros++;

                this.sMessage += this.nQtdErros.ToString() + " - Não foi possível encontrar Ordem de Produção: " + this.oGeracaoVolumeRetrabalho.OrdemProducao + Environment.NewLine;
            }
            else
            {
                if (!this.oListSeqProducao[0].CodigoProduto.Equals(this.sCodigoProdutoOP))
                {
                    this.nQtdErros++;

                    this.sMessage += this.nQtdErros.ToString() + " - Ordem de Producação " + this.oGeracaoVolumeRetrabalho.OrdemProducao + " não é uma OP de retrabalho!" + Environment.NewLine;
                }
            }
        }

        private void ValidatePrinter()
        {
            if (String.IsNullOrEmpty(this.oImpressora.Valor))
            {
                this.nQtdErros++;

                this.sMessage += this.nQtdErros.ToString() + " - Parâmetro impressora obrigatório preenchimento para executar função!" + Environment.NewLine;
            }
        }

        private void ValidateVolume()
        {
            if (!this.oListVolume.Any())
            {
                this.nQtdErros++;

                this.sMessage += this.nQtdErros.ToString() + " - Não possui volumes gerados para o Número de Série: " + this.oNrSerie.Valor + Environment.NewLine;
            }
        }
    }
}

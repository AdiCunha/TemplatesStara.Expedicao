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
using Common.Stara.LES.Expedicao.Business;
using Common.Stara.LES.Pcp2Peca.DataModel;
using Common.Stara.LES.Pcp2Peca.Business;
using TemplatesStara.CommonStara;
using AI1627Common20;

namespace TelasDinamicas.Expedicao
{
    [TemplateDebug("Web.Expedicao.GeracaoVolumeRecompra")]
    public class GeracaoVolumeRecompra : IProcessMovimentacao
    {
        private sqoClassSetMessageDefaults oClassSetMessageDefaults;
        private GeracaoVolumeDataModel oGeracaoVolumeDataModel;
        private List<sqoClassLESEstoqueAtualPersistence> estoqueAtualPersistenceList;
        private List<sqoClassLESExpedicaoVolumePersistence> oListVolume;
        private Pcp2Peca oPcp2PecaCodigoPai;
        private Pcp2Peca oPcp2PecaCodigoVolume;

        private sqoClassParametrosEstrutura oImpressora;
        private sqoClassParametrosEstrutura oDeposito;

        enum Action { Invalid = -1, reverse, print, generate, addVolume }
        private Action currentAction = Action.Invalid;

        private String sUsuario = String.Empty;
        private const String sCodigoVolumeRecompra = "VOLUME-RECOMPRA";
        private const String sTipoExpedicaoRecompra = "RECOMPRA";
        private const String sModuloRecompra = "WEB";
        private const String sOrdemVenda = "N/A";

        private String sOrigemLancamento = String.Empty;
        private String sInfoOrigemLancamento = String.Empty;
        private String sInfoOrigemLancamento2 = String.Empty;
        private String sObservacao = String.Empty;

        private String sMessage = String.Empty;
        private int nQtdErros = 0;

        public sqoClassMessage Executar(string sAction, string sXmlDados, string sXmlType, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao, List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sNivel, string sUsuario, object oObjAux)
        {
            using (sqoClassDbConnection oDBConnection = new sqoClassDbConnection())
            {

                this.Init(oListaParametrosListagem, oListaParametrosMovimentacao, sXmlDados, sAction, sUsuario);

                switch (currentAction)
                {
                    case Action.generate:
                        {
                            this.ValidateGenerate(oDBConnection);

                            this.ProcessBusinessLogicGenerate(oDBConnection);

                            break;
                        }

                    case Action.addVolume:
                        {
                            this.ValidateAddVolume();

                            this.ProcessBusinessLogicAddVolume(oDBConnection);

                            break;
                        }
                    case Action.print:
                        {
                            this.ValidatePrint();

                            this.ProcessBusinessLogicPrint(oDBConnection);

                            break;
                        }

                    case Action.reverse:
                        {
                            this.ValidateReverse();

                            this.ProcessBusinessLogicReverse(oDBConnection);

                            break;
                        }

                }
            }

            this.oClassSetMessageDefaults.SetarOk();

            return oClassSetMessageDefaults.Message;
        }

        private void Init(List<sqoClassParametrosEstrutura> oListaParametrosListagem, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao,
            String sXmlDados, String sAction, String sUsuario)
        {
            this.oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());

            this.oGeracaoVolumeDataModel = new GeracaoVolumeDataModel();

            this.oGeracaoVolumeDataModel = sqoClassBiblioSerDes.DeserializeObject<GeracaoVolumeDataModel>(sXmlDados);

            this.oImpressora = oListaParametrosListagem.Find(x => x.Campo == ("IMPRESSORA"));

            this.oDeposito = oListaParametrosListagem.Find(x => x.Campo == ("DEPOSITO"));

            Enum.TryParse(sAction, out this.currentAction);

            this.sUsuario = sUsuario;

            this.estoqueAtualPersistenceList = this.GetLESEstoqueAtualMaquinaBase();

            this.estoqueAtualPersistenceList.AddRange(sqoClassLESEstoqueAtualControlerDB.GetLESEstoqueAtual(oWhere: "WHERE CODIGO_RASTREABILIDADE LIKE '%" + oGeracaoVolumeDataModel.NrSerie + ".%' AND QUANTIDADE > 0"));

            this.oPcp2PecaCodigoPai = Pcp2PecaBusiness.GetPcp2PecaByCodigoPeca(this.oGeracaoVolumeDataModel.CodigoPai);

            this.oPcp2PecaCodigoVolume = Pcp2PecaBusiness.GetPcp2PecaByCodigoPeca(sCodigoVolumeRecompra);

            this.oListVolume = sqoClassLESExpedicaoVolumeControlerDB.GetExpedicaoVolumesByOrdemProducao(this.oGeracaoVolumeDataModel.NrSerie);
        }

        private List<sqoClassLESEstoqueAtualPersistence> GetLESEstoqueAtualMaquinaBase()
        {
            List<sqoClassLESEstoqueAtualPersistence> oLiEstoqueAtualMaquinaBase = new List<sqoClassLESEstoqueAtualPersistence>();

            string sTipoPeca = "ZFER";

            using (var oCommand = new sqoCommand(System.Data.CommandType.Text))
            {

                string sQuery = @" SELECT   
                                       EST.LOCAL  
                                      ,EST.CODIGO_PRODUTO  
                                      ,EST.CODIGO_RASTREABILIDADE  
                                      ,EST.QUANTIDADE  
                                      ,EST.DATA_INCLUSAO  
                                      ,EST.DATA_ULTIMA_MOVIMENTACAO  
                                      ,EST.USUARIO_ULTIMA_MOVIMENTACAO  
                                      ,EST.STATUS_MOVIMENTACAO  
                                      ,EST.MENSAGEM_STATUS_MOVIMENTACAO  
                                      ,EST.LOCAL_ORIGEM_MOV  
                                      ,EST.ID  
                                      ,EST.NIVEL  
									  ,PECA.TIPO_PECA
                                     FROM   
                                        WSQOLESTOQUEATUAL AS EST
                                    INNER JOIN
                                        WSQOLLOCAIS AS LOC
                                    ON
                                        LOC.CODIGO = EST.LOCAL_ORIGEM_MOV

                                    AND LOC.LOCAL_INTEGRACAO = ?

									AND EST.QUANTIDADE > 0

									INNER JOIN
										WSQOPCP2PECA AS PECA
									ON
										PECA.CODIGO_PECA = EST.CODIGO_PRODUTO

                                    WHERE
                                        EST.CODIGO_PRODUTO = ?
									AND
										PECA.TIPO_PECA = ?
                                        ";

                oCommand.SetCommandText(sQuery);

                oCommand
                    .Add("@DEPOSITO", this.oDeposito.Valor, System.Data.OleDb.OleDbType.VarChar, 50)
                    .Add("@CODIGO_PRODUTO", oGeracaoVolumeDataModel.CodigoPai, System.Data.OleDb.OleDbType.VarChar, 50)
                    .Add("@TIPO_PECA", sTipoPeca, System.Data.OleDb.OleDbType.VarChar, 50)
                    ;

                try
                {
                    oLiEstoqueAtualMaquinaBase = oCommand.GetListaResultado<sqoClassLESEstoqueAtualPersistence>();

                    AI1627Common20.Log.PrintLog.Log.LogVerbose(
                        oLiEstoqueAtualMaquinaBase.Count == 1 ? oLiEstoqueAtualMaquinaBase[0].ToString() : "Result.Count" + oLiEstoqueAtualMaquinaBase.Count(),
                        "Query: " + oCommand.QueryToString());

                    if (oLiEstoqueAtualMaquinaBase.Exists(x => x.CodigoRastreabilidade.Equals("&" + this.oGeracaoVolumeDataModel.NrSerie)))
                        oLiEstoqueAtualMaquinaBase.RemoveAll(x => x.CodigoRastreabilidade != ("&" + this.oGeracaoVolumeDataModel.NrSerie));
                    else
                        oLiEstoqueAtualMaquinaBase.RemoveAll(x => x != oLiEstoqueAtualMaquinaBase.First(y => y.CodigoRastreabilidade == String.Empty));
                }
                catch (Exception ex)
                {
                    throw new sqoClassMessageUserException(ex.Message);
                }
            }

            return oLiEstoqueAtualMaquinaBase;

        }

        private void Validate()
        {
            //this.ValidateNrSerie();

            this.ValidateMessage();
        }

        private void ValidateGenerate(sqoClassDbConnection oDBConnection)
        {

            this.Validate();

            if (this.oGeracaoVolumeDataModel.ImprimirEtiqueta)
            {
                this.ValidatePrinter();
            }

            this.ValidateVolumeQuantity();

            this.ValidateStockGenerate();

            this.ValidateVolumeList();

            this.ValidateMessage();
        }

        private void ValidateVolumeList()
        {

            var oListVol = this.oListVolume.Count(x => x.Status != AI1627CommonInterface.LESStatus.STATUS_EXPEDICAO_VOLUME.FATURADO && x.Status != AI1627CommonInterface.LESStatus.STATUS_EXPEDICAO_VOLUME.ESTORNADO);

            if (oListVol > 0)
            {
                this.nQtdErros++;

                string sMessageError = String.Empty;

                oListVolume.ToList().ForEach(x =>
                {
                    if (x.Status != AI1627CommonInterface.LESStatus.STATUS_EXPEDICAO_VOLUME.FATURADO && x.Status != AI1627CommonInterface.LESStatus.STATUS_EXPEDICAO_VOLUME.ESTORNADO)
                    {
                        sMessageError += x.ToString() + ", Status: " + (int)x.Status + " (" + x.Status.ToString() + ") " + Environment.NewLine;
                    }
                });

                //this.oListVolume.ForEach(x => sMessageError += x.ToString() + ", Status: " + (int)x.Status + " (" + x.Status.ToString() + ") " + Environment.NewLine);

                this.sMessage += this.nQtdErros.ToString() + " - Não é possível re-gerar volumes de recompra com o status diferente de: " +
                    (int)AI1627CommonInterface.LESStatus.STATUS_EXPEDICAO_VOLUME.FATURADO + " (" + AI1627CommonInterface.LESStatus.STATUS_EXPEDICAO_VOLUME.FATURADO.ToString() + ") ou " +
                    (int)AI1627CommonInterface.LESStatus.STATUS_EXPEDICAO_VOLUME.ESTORNADO + " (" + AI1627CommonInterface.LESStatus.STATUS_EXPEDICAO_VOLUME.ESTORNADO.ToString() + ")." + Environment.NewLine +
                    " Lista de volumes: " + Environment.NewLine + sMessageError;
            }
        }

        private void ValidateAddVolume()
        {
            this.Validate();

            if (this.oGeracaoVolumeDataModel.ImprimirEtiqueta)
            {
                this.ValidatePrinter();
            }

            this.ValidateVolumeQuantity();

            this.ValidateStockAddVolume();

            if (!this.oListVolume.Any())
            {
                this.nQtdErros++;

                this.sMessage += this.nQtdErros.ToString() + " - Não é possível utilizar a função \"Adicionar Volumes\", não possui nenhum volume gerado para o Número de Série: " + this.oGeracaoVolumeDataModel.NrSerie
                    + ", se necessário utilizar a função \"Gerar Volumes\"!" + Environment.NewLine;
            }

            this.ValidateMessage();
        }

        private void ValidatePrint()
        {
            this.ValidatePrinter();

            if (!this.oListVolume.Any())
            {
                this.nQtdErros++;

                this.sMessage += this.nQtdErros.ToString() + " - Não possui nenhum volume gerado para impressão de etiqueta!" + Environment.NewLine;
            }

            this.ValidateMessage();
        }

        private void ValidateReverse()
        {
            //this.ValidateStock();

            var oListVol = this.oListVolume.Find(x => x.CodigoRastreabilidade.Equals(this.oGeracaoVolumeDataModel.CodigoRastreabilidade) && x.CodigoVolume.Equals(this.oGeracaoVolumeDataModel.CodigoVolume));

            if (oListVol == null)
            {
                this.nQtdErros++;

                this.sMessage += this.nQtdErros.ToString() + " - Não existe volume gerado para estorno!" + Environment.NewLine;
            }
            else
            {
                List<sqoClassPcp2PecaVolumePersistence> oListVolumeCad = sqoClassLESExpedicaoVolumeCadastroControlerDB.GetPcp2PecaVolumeByCodigoPaiAndTipoVolume(this.oGeracaoVolumeDataModel.CodigoPai, oListVol.TipoExpedicao);

                if (oListVolumeCad.Any())
                {
                    this.nQtdErros++;

                    this.sMessage += this.nQtdErros.ToString() + " - Não é permitido estornar volumes com cadastro ativo!" + Environment.NewLine;
                }
            }

            this.ValidateMessage();
        }

        private void ProcessBusinessLogicGenerate(sqoClassDbConnection oDBConnection)
        {

            try
            {
                oDBConnection.BeginTransaction();

                int nContador = 1;

                string sObservacaoRecompra = "VOLUME RECOMPRA GERADO VIA WEB";

                int nSeqCodigoRastreabilidade = 0;

                var statusMovimentacaoAtual = estoqueAtualPersistenceList.FirstOrDefault().StatusMovimentacao;

                for (int i = 0; i < this.oGeracaoVolumeDataModel.QuantidadeVolume; i++)
                {
                    long nIdMovDebito = 0;
                    long nIdMovCredito = 0;

                    String sCodigoRastreabilidade = "&" + this.oGeracaoVolumeDataModel.NrSerie;

                    EXPEDICAO_VOLUME_TIPO TipoVolume = nContador == 1 ? EXPEDICAO_VOLUME_TIPO.MATERIAL : EXPEDICAO_VOLUME_TIPO.VOLUME_MATERIAL;

                    AI1627CommonInterface.LESStatus.STATUS_MOVIMENTACAO statusMovimentacao = TipoVolume == EXPEDICAO_VOLUME_TIPO.MATERIAL
                        ? AI1627CommonInterface.LESStatus.STATUS_MOVIMENTACAO.LIBERADO
                        : AI1627CommonInterface.LESStatus.STATUS_MOVIMENTACAO.LIBERADO_LES;

                    if (statusMovimentacaoAtual == AI1627CommonInterface.LESStatus.STATUS_MOVIMENTACAO.BLOQUEADO_NO_SAP || statusMovimentacaoAtual == AI1627CommonInterface.LESStatus.STATUS_MOVIMENTACAO.BLOQUEADO_NO_LES)
                    {
                        statusMovimentacao = TipoVolume == EXPEDICAO_VOLUME_TIPO.MATERIAL
                            ? AI1627CommonInterface.LESStatus.STATUS_MOVIMENTACAO.BLOQUEADO_NO_SAP
                            : AI1627CommonInterface.LESStatus.STATUS_MOVIMENTACAO.BLOQUEADO_NO_LES;
                    }

                    if (statusMovimentacaoAtual == AI1627CommonInterface.LESStatus.STATUS_MOVIMENTACAO.BLOQUEADO_PARA_QUALIDADE_SAP || statusMovimentacaoAtual == AI1627CommonInterface.LESStatus.STATUS_MOVIMENTACAO.BLOQUEADO_NO_LES)
                    {
                        statusMovimentacao = TipoVolume == EXPEDICAO_VOLUME_TIPO.MATERIAL
                            ? AI1627CommonInterface.LESStatus.STATUS_MOVIMENTACAO.BLOQUEADO_PARA_QUALIDADE_SAP
                            : AI1627CommonInterface.LESStatus.STATUS_MOVIMENTACAO.BLOQUEADO_NO_LES;
                    }

                    if (TipoVolume.Equals(EXPEDICAO_VOLUME_TIPO.VOLUME_MATERIAL))
                    {
                        nSeqCodigoRastreabilidade++;

                        sCodigoRastreabilidade += "." + nSeqCodigoRastreabilidade.ToString();
                    }

                    String sCodigoVolume = TipoVolume.Equals(EXPEDICAO_VOLUME_TIPO.MATERIAL) ? this.oGeracaoVolumeDataModel.CodigoPai : sCodigoVolumeRecompra;

                    String sCodigoProdutoEstoque = TipoVolume.Equals(EXPEDICAO_VOLUME_TIPO.MATERIAL) ? this.oGeracaoVolumeDataModel.CodigoPai : sCodigoVolumeRecompra;

                    this.FillMovData(sCodigoProdutoEstoque);

                    if (oListVolume.Exists(x => x.CodigoRastreabilidade.Equals(sCodigoRastreabilidade)))
                    {

                        sqoClassLESExpedicaoVolumeControlerDB.ExecuteWsqolUpdateExpedicaoVolume(
                            nId: oListVolume.First(x => x.CodigoRastreabilidade.Equals(sCodigoRastreabilidade)).Id,
                            nStatus: AI1627CommonInterface.LESStatus.STATUS_EXPEDICAO_VOLUME.CRIADO,
                            sCodigoRemessa: string.Empty,
                            nIdDocumentoTransporte: 0,
                            sTipoExpedicao: sTipoExpedicaoRecompra,
                            sOrdemProducao: this.oGeracaoVolumeDataModel.NrSerie,
                            sModulo: sModuloRecompra,
                            sObservacao: sObservacaoRecompra,
                            nPesoTotalCalculado: 0,
                            nPesoTotalVolume: 0,
                            sChaveExpedicaoPlanejamento: string.Empty,
                            sLocalEntrega: string.Empty,
                            nTipoPlanejamentoLes: 0,
                            dDataSeparacaoPlanejamento: null,
                            dDataTransportePlanejamento: null,
                            nObrigarSequenciaPlanejamento: 0,
                            sDocTransporteAuxPlanejamento: string.Empty,
                            sTamCaminhaoPlanejamento: string.Empty,
                            sDetalhePlanejamento: string.Empty,
                            gAgrupamentoLogistico: Guid.Empty,
                            gAgrupamentoCarregamento: Guid.Empty,
                            sUsuarioUltimaMovimentacao: sUsuario

                            );
                    }
                    else
                    {
                        this.InsertVolume(sCodigoVolume, sCodigoRastreabilidade, this.oGeracaoVolumeDataModel.NrSerie, TipoVolume, nContador);
                    }


                    //já possui a máquina base em estoque
                    if (TipoVolume.Equals(EXPEDICAO_VOLUME_TIPO.MATERIAL))
                    {
                        nIdMovDebito = StoreMovEstoqueDebito(
                            statusMovimentacao,
                            this.estoqueAtualPersistenceList[0].Local,
                            String.Empty,
                            sOrigemLancamento,
                            sInfoOrigemLancamento,
                            sInfoOrigemLancamento2,
                            sObservacao);

                        if (nIdMovDebito <= 0)
                            throw new Exception("Erro ao executar débito do estoque");
                    }

                    nIdMovCredito = StoreMovEstoqueCredito(
                        sCodigoProdutoEstoque
                        , sCodigoRastreabilidade
                        , nIdMovDebito
                        , statusMovimentacao
                        , this.sOrigemLancamento
                        , this.sInfoOrigemLancamento
                        , this.sInfoOrigemLancamento2
                        , this.sObservacao
                        );

                    if (nIdMovCredito <= 0)
                        throw new Exception("Erro ao executar crédito no estoque, favor entrar em contato com o Administrador do sistema!");

                    if (this.oGeracaoVolumeDataModel.ImprimirEtiqueta)
                    {
                        if (TipoVolume.Equals(EXPEDICAO_VOLUME_TIPO.MATERIAL))
                        {
                            VolumeBusiness.PrintEtiquetaPA(
                                long.Parse(this.oImpressora.Valor)
                                , this.oGeracaoVolumeDataModel.CodigoPai
                                , oPcp2PecaCodigoPai.Descricao
                                , this.oGeracaoVolumeDataModel.NrSerie
                                , sOrdemVenda
                                , sTipoExpedicaoRecompra
                                , this.sUsuario
                                , nContador.ToString()
                                , sCodigoRastreabilidade
                                );
                        }

                        else if (TipoVolume.Equals(EXPEDICAO_VOLUME_TIPO.VOLUME_MATERIAL))
                        {
                            VolumeBusiness.PrintEtiquetaVolume(
                                long.Parse(this.oImpressora.Valor)
                                , this.oGeracaoVolumeDataModel.CodigoPai
                                , oPcp2PecaCodigoPai.Descricao
                                , this.oGeracaoVolumeDataModel.NrSerie
                                , sOrdemVenda
                                , sTipoExpedicaoRecompra
                                , this.sUsuario
                                , nContador.ToString()
                                , sCodigoRastreabilidade
                                , sCodigoVolumeRecompra
                                , oPcp2PecaCodigoVolume.Descricao
                                );
                        }
                        else
                        {
                            throw new Exception("Não foi possível imprimir etiqueta, favor entrar em contato com o Administrador do sistema!");
                        }
                    }

                    nContador++;
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

        private void ProcessBusinessLogicAddVolume(sqoClassDbConnection oDBConnection)
        {
            try
            {
                oDBConnection.BeginTransaction();

                int nContador = oListVolume.Max(x => x.Contador);

                int nSeqCodigoRastreabilidade = 0;

                for (int i = 0; i < this.oGeracaoVolumeDataModel.QuantidadeVolume; i++)
                {
                    nContador++;

                    nSeqCodigoRastreabilidade = nContador > 1 ? nContador - 1 : nContador;

                    String sCodigoRastreabilidade = "&" + this.oGeracaoVolumeDataModel.NrSerie + "." + nSeqCodigoRastreabilidade.ToString();

                    this.FillMovData(sCodigoVolumeRecompra);

                    this.InsertVolume(sCodigoVolumeRecompra, sCodigoRastreabilidade, this.oGeracaoVolumeDataModel.NrSerie, EXPEDICAO_VOLUME_TIPO.VOLUME_MATERIAL, nContador);

                    var statusMovimentacaoAtual = estoqueAtualPersistenceList.FirstOrDefault().StatusMovimentacao;

                    var statusMovimentacao = AI1627CommonInterface.LESStatus.STATUS_MOVIMENTACAO.LIBERADO_LES;

                    if (statusMovimentacaoAtual == AI1627CommonInterface.LESStatus.STATUS_MOVIMENTACAO.BLOQUEADO_NO_SAP
                        || statusMovimentacaoAtual == AI1627CommonInterface.LESStatus.STATUS_MOVIMENTACAO.BLOQUEADO_PARA_QUALIDADE_SAP
                        || statusMovimentacaoAtual == AI1627CommonInterface.LESStatus.STATUS_MOVIMENTACAO.BLOQUEADO_NO_LES)
                    {
                        statusMovimentacao = AI1627CommonInterface.LESStatus.STATUS_MOVIMENTACAO.BLOQUEADO_NO_LES;
                    }

                    long nIdMovCredito = StoreMovEstoqueCredito(
                        sCodigoVolumeRecompra
                        , sCodigoRastreabilidade
                        , 0
                        , statusMovimentacao
                        , this.sOrigemLancamento
                        , this.sInfoOrigemLancamento
                        , this.sInfoOrigemLancamento2
                        , this.sObservacao
                        );

                    if (nIdMovCredito <= 0)
                        throw new Exception("Erro ao executar crédito no estoque, favor entrar em contato com o Administrador do sistema!");

                    if (this.oGeracaoVolumeDataModel.ImprimirEtiqueta)
                    {

                        VolumeBusiness.PrintEtiquetaVolume(
                            long.Parse(this.oImpressora.Valor)
                            , this.oGeracaoVolumeDataModel.CodigoPai
                            , oPcp2PecaCodigoPai.Descricao
                            , this.oGeracaoVolumeDataModel.NrSerie
                            , sOrdemVenda
                            , sTipoExpedicaoRecompra
                            , this.sUsuario
                            , nContador.ToString()
                            , sCodigoRastreabilidade
                            , sCodigoVolumeRecompra
                            , oPcp2PecaCodigoVolume.Descricao
                            );


                    }
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

        private void ProcessBusinessLogicPrint(sqoClassDbConnection oDBConnection)
        {
            try
            {
                oDBConnection.BeginTransaction();

                EXPEDICAO_VOLUME_TIPO TipoVolume = this.oGeracaoVolumeDataModel.CodigoPai.Equals(this.oGeracaoVolumeDataModel.CodigoVolume) ? EXPEDICAO_VOLUME_TIPO.MATERIAL : EXPEDICAO_VOLUME_TIPO.VOLUME_MATERIAL;

                if (TipoVolume.Equals(EXPEDICAO_VOLUME_TIPO.MATERIAL))
                {
                    VolumeBusiness.PrintEtiquetaPA(
                        long.Parse(this.oImpressora.Valor)
                        , this.oGeracaoVolumeDataModel.CodigoPai
                        , oPcp2PecaCodigoPai.Descricao
                        , this.oGeracaoVolumeDataModel.NrSerie
                        , sOrdemVenda
                        , sTipoExpedicaoRecompra
                        , this.sUsuario
                        , this.oGeracaoVolumeDataModel.Contador.ToString()
                        , oGeracaoVolumeDataModel.CodigoRastreabilidade
                        );
                }

                else if (TipoVolume.Equals(EXPEDICAO_VOLUME_TIPO.VOLUME_MATERIAL))
                {
                    VolumeBusiness.PrintEtiquetaVolume(
                        long.Parse(this.oImpressora.Valor)
                        , this.oGeracaoVolumeDataModel.CodigoPai
                        , oPcp2PecaCodigoPai.Descricao
                        , this.oGeracaoVolumeDataModel.NrSerie
                        , sOrdemVenda
                        , sTipoExpedicaoRecompra
                        , this.sUsuario
                        , this.oGeracaoVolumeDataModel.Contador.ToString()
                        , oGeracaoVolumeDataModel.CodigoRastreabilidade
                        , sCodigoVolumeRecompra
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

        private void ProcessBusinessLogicReverse(sqoClassDbConnection oDBConnection)
        {
            try
            {
                oDBConnection.BeginTransaction();

                this.FillMovData(this.oGeracaoVolumeDataModel.CodigoVolume);

                if (this.estoqueAtualPersistenceList.Any())
                {
                    this.StoreMovEstoqueDebito(this.estoqueAtualPersistenceList[0].StatusMovimentacao, String.Empty, this.estoqueAtualPersistenceList[0].CodigoRastreabilidade, this.sOrigemLancamento, this.sInfoOrigemLancamento, this.sInfoOrigemLancamento2, this.sObservacao);
                }

                var oVolume = this.oListVolume.Find(x => x.CodigoVolume.Equals(this.oGeracaoVolumeDataModel.CodigoVolume) && x.CodigoRastreabilidade.Equals(this.oGeracaoVolumeDataModel.CodigoRastreabilidade));

                sqoClassLESExpedicaoVolumeControlerDB.ExecuteWsqolUpdateExpedicaoVolume(oVolume.Id, nStatus: AI1627CommonInterface.LESStatus.STATUS_EXPEDICAO_VOLUME.ESTORNADO);

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


        private void ProcessBusinessLogicRetornoReRecompra(sqoClassDbConnection oDBConnection)
        {
            try
            {
                oDBConnection.BeginTransaction();

                this.FillMovData(this.oGeracaoVolumeDataModel.CodigoVolume);

                if (this.estoqueAtualPersistenceList.Any())
                {
                    this.StoreMovEstoqueDebito(this.estoqueAtualPersistenceList[0].StatusMovimentacao, String.Empty, this.estoqueAtualPersistenceList[0].CodigoRastreabilidade, this.sOrigemLancamento, this.sInfoOrigemLancamento, this.sInfoOrigemLancamento2, this.sObservacao);
                }

                var oVolume = this.oListVolume.Find(x => x.CodigoVolume.Equals(this.oGeracaoVolumeDataModel.CodigoVolume) && x.CodigoRastreabilidade.Equals(this.oGeracaoVolumeDataModel.CodigoRastreabilidade));

                sqoClassLESExpedicaoVolumeControlerDB.ExecuteWsqolUpdateExpedicaoVolume(oVolume.Id, nStatus: AI1627CommonInterface.LESStatus.STATUS_EXPEDICAO_VOLUME.CRIADO);

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


        private void InsertVolume(String sCodigoVolume, String sCodigoRastreabilidade, String sOrdemProducao, EXPEDICAO_VOLUME_TIPO TipoVolume, int nContador)
        {
            sqoClassLESExpedicaoVolumePersistence oVolume = new sqoClassLESExpedicaoVolumePersistence();

            oVolume.IdRemessa = 0;
            oVolume.IdDocumentoTransporte = 0;
            oVolume.CodigoRemessa = String.Empty;
            oVolume.CodigoVolume = sCodigoVolume;
            oVolume.CodigoRastreabilidade = sCodigoRastreabilidade;
            oVolume.OrdemProducao = sOrdemProducao;
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
            oVolume.Modulo = sModuloRecompra;
            oVolume.Status = AI1627CommonInterface.LESStatus.STATUS_EXPEDICAO_VOLUME.CRIADO;
            oVolume.UsuarioUltimaMovimentacao = this.sUsuario;
            oVolume.DataUltimaMovimentacao = DateTime.Now;
            oVolume.Observacao = "VOLUME RECOMPRA GERADO VIA WEB";
            oVolume.Contador = nContador;
            oVolume.TipoExpedicao = sTipoExpedicaoRecompra;

            long nIdVolume = oVolume.Insert();

            if (nIdVolume <= 0)
                throw new Exception(oVolume.ToString());
        }

        private long StoreMovEstoqueDebito(
            AI1627CommonInterface.LESStatus.STATUS_MOVIMENTACAO sTATUS_MOVIMENTACAO
            , String sLocalDestino
            , String sCodigoRastreabilidade
            , String sOrigemLancamento
            , String sInfoOrigemLancamento
            , String sInfoOrigemLancamento2
            , String sObservacao
            )
        {
            long nIdMov = sqoClassLESEstoqueAtualControlerDB.ExecuteStoreMovEstoqueDebito(
                            this.estoqueAtualPersistenceList[0].Local
                            , this.estoqueAtualPersistenceList[0].CodigoProduto
                            , 1
                            , this.sUsuario
                            , sCodigoRastreabilidade
                            , null
                            , sTATUS_MOVIMENTACAO
                            , sLocalDestino
                            , true
                            , String.Empty
                            , sOrigemLancamento
                            , sInfoOrigemLancamento
                            , sInfoOrigemLancamento2
                            , sObservacao
                            );

            return nIdMov;
        }

        private long StoreMovEstoqueCredito(
            String sCodigoProduto
            , String sCodigoRastreabilidade
            , long nIdMovDebito
            , AI1627CommonInterface.LESStatus.STATUS_MOVIMENTACAO sTATUS_MOVIMENTACAO
            , String sOrigemLancamento
            , String sInfoOrigemLancamento
            , String sInfoOrigemLancamento2
            , String sObservacao
            )
        {
            long nIdMov = sqoClassLESEstoqueAtualControlerDB.ExecuteStoreMovEstoqueCredito(
                        this.estoqueAtualPersistenceList[0].Local
                        , sCodigoProduto
                        , 1
                        , this.sUsuario
                        , sCodigoRastreabilidade
                        , nIdMovDebito
                        , sTATUS_MOVIMENTACAO
                        , String.Empty
                        , true
                        , String.Empty
                        , sOrigemLancamento
                        , sInfoOrigemLancamento
                        , sInfoOrigemLancamento2
                        , sObservacao
                        );

            return nIdMov;
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

        private void ValidateNrSerie()
        {
            List<sqoClassLESEXPRemessaItensNrSeriePersistence> oListItensNrSerie =
                sqoClassLESEXPRemessaItensNrSerieControlerDB.GetLESExpRemessaItensNrSerieByNrSerie(this.oGeracaoVolumeDataModel.NrSerie);

            if (oListItensNrSerie.Any())
            {
                foreach (var oItensNrSerie in oListItensNrSerie)
                {
                    List<sqoClassLESEXPRemessaItensPersistence> oListItens = sqoClassLESEXPRemessaItensControlerDB.GetLESExpRemessaItensByIdItem(oItensNrSerie.IdRemessaItem);

                    foreach (var oItens in oListItens)
                    {
                        if (!(oItens.CodigoProduto.Equals(this.oGeracaoVolumeDataModel.CodigoPai)))
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

        private void ValidatePrinter()
        {
            if (String.IsNullOrEmpty(this.oImpressora.Valor))
            {
                this.nQtdErros++;

                this.sMessage += this.nQtdErros.ToString() + " - Parâmetro Impressora Obrigatório";
            }
        }

        private void ValidateVolumeQuantity()
        {
            //quantidade de volumes maior que zero
            //quantidade de volumes int

            if (this.oGeracaoVolumeDataModel.QuantidadeVolume <= 0)
            {
                this.nQtdErros++;

                this.sMessage += this.nQtdErros.ToString() + " - Campo Quantidade Volume deve ser maior que zero!";
            }
        }

        private void ValidateStockGenerate()
        {
            if (currentAction == Action.generate)
            {

                if (estoqueAtualPersistenceList.Count == 0)
                {
                    this.nQtdErros++;

                    string sMessageError = String.Empty;

                    this.sMessage += this.nQtdErros.ToString() + " - Não é permitido re-gerar volumes de recompra de materiais sem saldo de estoque! ";
                }

                else if (estoqueAtualPersistenceList.Count != 1)
                {
                    this.nQtdErros++;

                    string sMessageError = String.Empty;

                    this.estoqueAtualPersistenceList.ForEach(x => sMessageError += x.ToString() + Environment.NewLine);

                    this.sMessage += this.nQtdErros.ToString() + " - Não é permitido re-gerar volumes de recompra de materiais com saldo de estoque dos volumes! " +
                        " Lista de estoque do material " + oGeracaoVolumeDataModel.CodigoPai + " : " + Environment.NewLine + sMessageError;
                }

                else
                {
                    if (this.estoqueAtualPersistenceList.Exists(x => x.Quantidade != 1 && !String.IsNullOrEmpty(x.CodigoRastreabilidade)))
                    {
                        this.nQtdErros++;

                        this.sMessage += this.nQtdErros.ToString() + " - Não é permitido re-gerar volumes de recompra de materiais com código de rastreabilidade e com saldo de estoque diferente de 1:" +
                           " Saldo do volume: " + this.estoqueAtualPersistenceList[0].Quantidade + Environment.NewLine;
                    }

                    if (!String.IsNullOrEmpty(this.estoqueAtualPersistenceList[0].CodigoRastreabilidade))
                    {
                        this.nQtdErros++;

                        this.sMessage += this.nQtdErros.ToString() + " - Não é permitido re-gerar volumes de recompra de materiais que possuam código de rastreabilidade" +
                           " Código de rastreabilidade do volume: " + this.estoqueAtualPersistenceList[0].CodigoRastreabilidade + Environment.NewLine;
                    }
                }

            }

        }

        private void ValidateStockAddVolume()
        {
            if (currentAction == Action.addVolume)
            {
                if (oListVolume.Count != estoqueAtualPersistenceList.Count)
                {
                    this.nQtdErros++;

                    this.sMessage += this.nQtdErros.ToString() + " - Quantidade de volumes gerados: " + oListVolume.Count + " é diferente da quantidade em estoque: " +
                      estoqueAtualPersistenceList.Count + " !" + Environment.NewLine;
                }


                List<sqoClassLESEstoqueAtualPersistence> estoqueAtualPersistenceListValidate = new List<sqoClassLESEstoqueAtualPersistence>();

                foreach (var oGeracaoList in oListVolume)
                {
                    sqoClassLESEstoqueAtualPersistence oEstoqueAtualPersistence =
                        estoqueAtualPersistenceList.Find(x => x.CodigoRastreabilidade.Equals(this.oGeracaoVolumeDataModel.CodigoRastreabilidade));

                    if (oEstoqueAtualPersistence != null)
                    {
                        estoqueAtualPersistenceListValidate.Add(oEstoqueAtualPersistence);

                        if (oGeracaoVolumeDataModel.CodigoRastreabilidade != oEstoqueAtualPersistence.CodigoRastreabilidade)
                        {
                            this.nQtdErros++;

                            this.sMessage += this.nQtdErros.ToString() + " - Código de rastreabilidade do volume da geração deve ser igual ao do estoque! " + Environment.NewLine;
                        }

                        else if (oEstoqueAtualPersistence.Quantidade != 1)
                        {
                            this.nQtdErros++;

                            this.sMessage += this.nQtdErros.ToString() + " - Quantidade: " + oEstoqueAtualPersistence.Quantidade +
                                " do volume " + oEstoqueAtualPersistence.CodigoRastreabilidade + " inválida. Deve ser igual a 1!" + Environment.NewLine;
                        }

                    }

                    else
                    {
                        this.nQtdErros++;

                        this.sMessage += this.nQtdErros.ToString() + " - Estoque não encontrado para o volume: " +
                            oEstoqueAtualPersistence.CodigoRastreabilidade + Environment.NewLine;
                    }

                }

                foreach (var oEstoqueList in estoqueAtualPersistenceList)
                {
                    sqoClassLESEstoqueAtualPersistence oEstoqueAtualPersistenceList =
                        estoqueAtualPersistenceListValidate.Find(x => x.CodigoRastreabilidade.Equals(oEstoqueList.CodigoRastreabilidade));

                    if (oEstoqueAtualPersistenceList != null)
                    {

                        if (oEstoqueAtualPersistenceList.Quantidade != 1)
                        {
                            this.nQtdErros++;

                            this.sMessage += this.nQtdErros.ToString() + " - Quantidade em estoque: " + oEstoqueAtualPersistenceList.Quantidade +
                                " inválida! Deve ser igual a 1!" + Environment.NewLine;
                        }
                    }
                }

            }


            if (!this.estoqueAtualPersistenceList.Any())
            {
                this.nQtdErros++;

                this.sMessage += this.nQtdErros.ToString() + " - Não foi possível encontrar estoque do Código Pai: " + this.oGeracaoVolumeDataModel.CodigoPai + " !" + Environment.NewLine;
            }
        }

        private void FillMovData(String sCodigoVolume)
        {
            this.sOrigemLancamento = "Geração volume recompra";
            this.sInfoOrigemLancamento = "Código Pai: " + this.oGeracaoVolumeDataModel.CodigoPai;
            this.sInfoOrigemLancamento2 = "Código Volume: " + sCodigoVolume;
            this.sObservacao = "Geração volume recompra web - " + this.oGeracaoVolumeDataModel.CodigoPai + " / " + sCodigoVolume;
        }
    }


    internal class GeracaoVolumePersistence
    {

    }


    /// <summary>
    /// Classe para instanciar o modelo do criteria da tela.
    /// </summary>
    [XmlRoot("ItemFilaProducao")]
    public class GeracaoVolumeDataModel
    {
        [XmlElement("ID")]
        public long Id { get; set; }

        [XmlElement("CODIGO_PAI")]
        public String CodigoPai { get; set; }

        [XmlElement("CODIGO_VOLUME")]
        public String CodigoVolume { get; set; }

        [XmlElement("STATUS_VOLUME")]
        public int StatusVolume { get; set; }

        [XmlElement("DESCRICAO_STATUS_VOLUME")]
        public String DescricaoStatusVolume { get; set; }

        [XmlElement("NR_SERIE")]
        public String NrSerie { get; set; }

        [XmlElement("CODIGO_RASTREABILIDADE")]
        public String CodigoRastreabilidade { get; set; }

        [XmlElement("CONTADOR")]
        public int Contador { get; set; }

        [XmlElement("QUANTIDADE")]
        public Double Quantidade { get; set; }

        [XmlElement("IMPRIMIR_ETIQUETA")]
        public Boolean ImprimirEtiqueta { get; set; }

        [XmlElement("QUANTIDADE_VOLUME")]
        public int QuantidadeVolume { get; set; }
    }



}
using AI1627Common20.TemplateDebugging;
using sqoClassLibraryAI0502Message;
using sqoClassLibraryAI1151FilaProducao.Estrutura;
using sqoClassLibraryAI1151FilaProducao.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using sqoClassLibraryAI0502VariaveisSistema;
using sqoClassLibraryAI0502Biblio;
using sqoClassLibraryAI1151FilaProducao;
using AI1627CommonInterface;
using System.Data;
using System.Data.OleDb;
using TemplatesStara.CommonStara;
using Common.Stara.LES.Expedicao.Business;
using Common.Stara.LES.Expedicao.DataModel;
using AI1627CommonInterface.LESStatus;

namespace TelaDinamica.Expedicao
{
    [TemplateDebug("WebExpedicaoAlterarRastPicking")]
    public class WebExpedicaoAlterarRastPicking : IProcessMovimentacao
    {
        public sqoClassMessage Executar(string sAction, string sXmlDados, string sXmlType, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao, List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sNivel, string sUsuario, object oObjAux)
        {
            var oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());
            oClassSetMessageDefaults.SetarOk();

            var oRastreabilidadePickingPersistencia = sqoClassBiblioSerDes.DeserializeObject<ExpedicaoAlterarRastreabilidadePickingPersistencia>(sXmlDados);

            using (var oDBConnection = new sqoClassDbConnection())
            {
                ValidarAlteracao(oDBConnection, oRastreabilidadePickingPersistencia);

                ProcessBusinessLogic(oDBConnection, oRastreabilidadePickingPersistencia, sNivel, sUsuario);
            }

            return oClassSetMessageDefaults.Message;
        }

        private void ValidarAlteracao(sqoClassDbConnection oDBConnection, ExpedicaoAlterarRastreabilidadePickingPersistencia oRastreabilidadePickingPersistencia)
        {
            string sDescription = "";

            if (WebExpedicaoAlterarRastPickingPersistence.QueryScreenData(oRastreabilidadePickingPersistencia) != 1)
            {
                sDescription += " - Dados do criteria desatualizados, favor atualizar lista!\n";
            }
            else
            {
                if (ValidateSituacaoItemRemessa(oRastreabilidadePickingPersistencia))
                {
                    sDescription += " - Não é permitido fazer alterações em remessas com a situação do item definida no ERP diferente de \"S\"!\n";
                }

                if (!String.IsNullOrEmpty(oRastreabilidadePickingPersistencia.CodigoRastreabilidadeItem))
                {
                    var oEstoque = sqoClassLESEstoqueAtualControlerDB.GetLESEstoqueAtual(null, null, null, oRastreabilidadePickingPersistencia.CodigoRastreabilidadeItem, null, null, "> 0");

                    var oListVolume = sqoClassLESExpedicaoVolumeControlerDB.GetExpedicaoVolumesByOrdemProducao(oRastreabilidadePickingPersistencia.CodigoRastreabilidadeItem, oDBConnection);

                    VerificarVolumeDuplicado(oListVolume);

                    if (oEstoque.Count == 0)
                    {
                        sDescription += " - Código de Rastreabilidade: " + oRastreabilidadePickingPersistencia.CodigoRastreabilidadeItem + " para o item não foi possível encontrar no estoque atual!\n";
                    }
                    else
                    {
                        if (oEstoque.FindAll(x => x.CodigoProduto == oRastreabilidadePickingPersistencia.Material).Count() == 0)
                        {
                            sDescription += " - Código de Rastreabilidade: " + oRastreabilidadePickingPersistencia.CodigoRastreabilidadeItem + " informado não pertence ao Material: " + oRastreabilidadePickingPersistencia.Material + " !\n";
                        }
                        else
                        {
                            if (oEstoque.FindAll(x => x.StatusMovimentacao != STATUS_MOVIMENTACAO.LIBERADO).Count() > 0)
                            {
                                sDescription += " - Não é permitido vincular Código de Rastreabilidade com status em estoque diferente de 1!\n";
                            }

                            string sExistsMTO = GetCodigoRastreabilidadeItemPickingMTO(oRastreabilidadePickingPersistencia.CodigoRastreabilidadeItem);

                            if (sExistsMTO.Equals("1"))
                            {
                                sDescription += " - Não é permitido utilizar Ordem de Produção: " + oRastreabilidadePickingPersistencia.CodigoRastreabilidadeItem + " com cliente vinculado!\n";
                            }
                            else
                            {
                                foreach (var oVolume in oListVolume)
                                {
                                    if (oVolume.Status != STATUS_EXPEDICAO_VOLUME.ESTORNADO)
                                    {
                                        var oListEstoque = sqoClassLESEstoqueAtualControlerDB.GetLESEstoqueAtual(null, null, null, oVolume.CodigoRastreabilidade, null, ">0", null);

                                        if (oListEstoque.Count().Equals(0))
                                        {
                                            sDescription += " - Não é permitido vincular volume sem saldo em estoque Código Rastreabilidade: " + oVolume.CodigoRastreabilidade + " !\n";
                                        }
                                        else
                                        {
                                            sDescription += ValidateDeposito(oVolume, oRastreabilidadePickingPersistencia);
                                        }
                                    }
                                }

                                sDescription += ValidaPickingJaUtilizado(oRastreabilidadePickingPersistencia);

                            }
                        }
                    }
                }

            }

            if (!String.IsNullOrEmpty(sDescription))
                throw new sqoClassMessageUserException(sDescription);

        }

        private void ProcessBusinessLogic(sqoClassDbConnection oDBConnection, ExpedicaoAlterarRastreabilidadePickingPersistencia oRastreabilidadePickingPersistencia, string sNivel, string sUsuario)
        {
            int nHeaderPickingMov = 0;

            long nIdMov = 0;

            try
            {
                oDBConnection.BeginTransaction();

                var oListVolumeNew = sqoClassLESExpedicaoVolumeControlerDB.GetExpedicaoVolumesByOrdemProducao(oRastreabilidadePickingPersistencia.CodigoRastreabilidadeItem, oDBConnection);

                VerificarVolumeDuplicado(oListVolumeNew);

                var oListVolumeOld = new List<sqoClassLESExpedicaoVolumePersistence>();

                if (!String.IsNullOrEmpty(oRastreabilidadePickingPersistencia.CodigoRastreabilidade))
                {
                    oListVolumeOld = sqoClassLESExpedicaoVolumeControlerDB.GetExpedicaoVolumesByOrdemProducao(oRastreabilidadePickingPersistencia.CodigoRastreabilidade, oDBConnection);

                    VerificarVolumeDuplicado(oListVolumeOld);
                }

                var oListPickingItemOld = sqoClassLESPickingControlerDB.GetLESPickingItem(oRastreabilidadePickingPersistencia.Id);

                if (nHeaderPickingMov == 0)
                {
                    nIdMov = InsertLESPickingMov(oRastreabilidadePickingPersistencia.Id, sUsuario, oRastreabilidadePickingPersistencia.Status);

                    nHeaderPickingMov++;
                }

                if (!oListVolumeOld.Any())
                {
                    DeleteLESPickingItemByIdItem(oRastreabilidadePickingPersistencia.Id, oRastreabilidadePickingPersistencia.IdItem);

                    InsertLESPickingMovItens(nIdMov, oRastreabilidadePickingPersistencia.IdItem, oRastreabilidadePickingPersistencia.Material, 6);//AI1627CommonInterface.LESStatus.STATUS_ITEM_PICKING.ESTORNADO
                }
                else
                {
                    foreach (var oVolumeOld in oListVolumeOld)
                    {
                        if (oVolumeOld.Status != STATUS_EXPEDICAO_VOLUME.ESTORNADO)
                        {
                            if (oListPickingItemOld.FindAll(x => x.CodigoRastreabilidade == oVolumeOld.CodigoRastreabilidade).Count() > 0)
                            {
                                DeleteLESPickingItemByCodigoRastreabilidade(oRastreabilidadePickingPersistencia.Id, oVolumeOld.CodigoRastreabilidade);

                                var oItem = oListPickingItemOld.Find(x => x.CodigoRastreabilidade == oVolumeOld.CodigoRastreabilidade);

                                InsertLESPickingMovItens(nIdMov, oItem.Id, oVolumeOld.CodigoVolume, 6);//AI1627CommonInterface.LESStatus.STATUS_ITEM_PICKING.ESTORNADO

                                sqoClassLESExpedicaoVolumeControlerDB.ExecuteWsqolUpdateExpedicaoVolume(
                                    oVolumeOld.Id,
                                    nIdRemessa: 0,
                                    sCodigoRemessa:
                                    String.Empty,
                                    nStatus: STATUS_EXPEDICAO_VOLUME.CRIADO,
                                    sObservacao: sNivel,
                                    sUsuarioUltimaMovimentacao:
                                    sUsuario,
                                    dDataUltimaMovimentacao: DateTime.Now
                                    );

                                ItemRemessaVolumeBusiness.DeleteItemRemessaVolumeByIdExpedicaoVolume(oVolumeOld.Id);
                            }
                        }
                    }
                }

                if (!oListVolumeNew.Any())
                {
                    long nIdSeqItem = InsertLESPickingItem(
                        oRastreabilidadePickingPersistencia.Id,
                        oRastreabilidadePickingPersistencia.Material,
                        oRastreabilidadePickingPersistencia.CodigoRastreabilidadeItem,
                        oRastreabilidadePickingPersistencia.IdItemDoc
                        );

                    InsertLESPickingMovItens(nIdMov, nIdSeqItem, oRastreabilidadePickingPersistencia.Material, (int)STATUS_ITEM_PICKING.NAO_PAGO);
                }

                else
                {
                    foreach (var oVolumeNew in oListVolumeNew)
                    {
                        if (oVolumeNew.Status != STATUS_EXPEDICAO_VOLUME.ESTORNADO)
                        {
                            if (String.IsNullOrEmpty(oVolumeNew.CodigoRastreabilidade))
                                throw new sqoClassExceptionMessageUser("Volume registrado sem Código de Rastreabilidade, ID: " + oVolumeNew.Id + "!");

                            var oItem = oListPickingItemOld.Find(x => x.Item == oRastreabilidadePickingPersistencia.Material);

                            long nIdSeqItem = InsertLESPickingItem(
                                oRastreabilidadePickingPersistencia.Id,
                                oVolumeNew.CodigoVolume,
                                oVolumeNew.CodigoRastreabilidade,
                                oItem.IdItemDoc
                                );

                            InsertLESPickingMovItens(nIdMov, nIdSeqItem, oVolumeNew.CodigoVolume, (int)STATUS_ITEM_PICKING.NAO_PAGO);

                            UpdateVolume(oVolumeNew.Id, oRastreabilidadePickingPersistencia, sUsuario);

                            InsertItemRemessaVolume(oVolumeNew.Id, oRastreabilidadePickingPersistencia);

                        }

                    }
                }

                oDBConnection.Commit();

            }
            catch (Exception ex)
            {
                oDBConnection.Rollback();

                throw new sqoClassMessageUserException("Error\n" + ex.Message, ex.InnerException);
            }
        }

        private void VerificarVolumeDuplicado(List<sqoClassLESExpedicaoVolumePersistence> oListVolume)
        {
            var oClassDefaultPersistence = sqoClassLESExpedicaoVolumeControlerDB.CheckVolumeList(oListVolume);

            if (oClassDefaultPersistence.Ok == false)
                throw new sqoClassExceptionMessageUser(oClassDefaultPersistence.Message);
        }

        private void UpdateVolume(long nIdVolume, ExpedicaoAlterarRastreabilidadePickingPersistencia oRastreabilidadePickingPersistencia, string sUsuario)
        {
            var oRemessa = sqoClassLESEXPRemessaControlerDB.GetLESExpRemessaByCodigoRemessa(oRastreabilidadePickingPersistencia.Picking);

            if (oRemessa.Equals(null))
                throw new sqoClassExceptionMessageUser("Remessa " + oRastreabilidadePickingPersistencia.Picking + " não encontrada na base de dados!");

            sqoClassLESExpedicaoVolumeControlerDB.ExecuteWsqolUpdateExpedicaoVolume(
                nId: nIdVolume,
                nIdRemessa: oRemessa.Id,
                sCodigoRemessa: oRastreabilidadePickingPersistencia.Picking,
                nStatus: STATUS_EXPEDICAO_VOLUME.PICKING_CRIADO,
                sUsuarioUltimaMovimentacao: sUsuario,
                dDataUltimaMovimentacao: DateTime.Now
                );
        }

        private void InsertItemRemessaVolume(long nIdVolume, ExpedicaoAlterarRastreabilidadePickingPersistencia oRastreabilidadePickingPersistencia)
        {
            var oRemessaItem = sqoClassLESEXPRemessaItensControlerDB.GetLESExpRemessaItensByCodigoRemessaAndItemRemessa(oRastreabilidadePickingPersistencia.Picking, oRastreabilidadePickingPersistencia.IdItemDoc);

            if (oRemessaItem.Equals(null))
                throw new sqoClassExceptionMessageUser("Item da remessa encontrado na base de dados , Item Remessa: " + oRastreabilidadePickingPersistencia.IdItemDoc + " Remessa: "
                    + oRastreabilidadePickingPersistencia.Picking + " !");

            var oItemRemessaVolume = new ItemRemessaVolume()
            {
                IdExpedicaoVolume = nIdVolume,
                IdExpRemessaItens = oRemessaItem.Id,
                ItemRemessa = oRastreabilidadePickingPersistencia.IdItemDoc
            };

            var oClassDefaultPersistence = ItemRemessaVolumeBusiness.InsertItemRemessaVolumeBusiness(oItemRemessaVolume);

            if (oClassDefaultPersistence.Ok.Equals(false))
                throw new sqoClassExceptionMessageUser(oClassDefaultPersistence.Message);

        }

        private void DeleteLESPickingItemByCodigoRastreabilidade(long nIdSeq, string sCodigoRastreabilidade)
        {
            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@ID_SEQ", nIdSeq, OleDbType.BigInt)
                    .Add("@CODIGO_RASTREABILIDADE", sCodigoRastreabilidade, OleDbType.VarChar, 50)
                    ;

                string sQuery = @"DELETE FROM WSQOLPICKINGSEQITENS WHERE ID_SEQ = @ID_SEQ AND CODIGO_RASTREABILIDADE = @CODIGO_RASTREABILIDADE";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    oCommand.Execute();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }
            }
        }

        private void DeleteLESPickingItemByIdItem(long nIdSeq, long nIdItem)
        {
            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@ID_SEQ", nIdSeq, OleDbType.BigInt)
                    .Add("@ID_ITEM", nIdItem, OleDbType.VarChar, 50)
                    ;

                string sQuery = @"DELETE FROM WSQOLPICKINGSEQITENS WHERE ID_SEQ = @ID_SEQ AND ID = @ID_ITEM";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    oCommand.Execute();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }
            }
        }

        private long InsertLESPickingItem(long nIdSeq, string sMaterial, string sCodigoRastreabilidade, string nIdItemDoc)
        {
            var oListPickingItemNew = new sqoClassLESPickingItemPersistence
            {
                IdSeq = nIdSeq,
                Item = sMaterial,
                Quantidade = 1,
                QuantidadePago = 0,
                Status = AI1627CommonInterface.LESStatus.STATUS_ITEM_PICKING.NAO_PAGO,
                CodigoRastreabilidade = sCodigoRastreabilidade,
                IdItemDoc = nIdItemDoc
            };

            return oListPickingItemNew.Insert();
        }

        private long InsertLESPickingMov(long nIdSeq, string sUsuario, int nStatus)
        {
            long nResult = 0;

            string sMessage = "Alteração rastreabilidade Item WEB";

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@ID_SEQ", nIdSeq, OleDbType.BigInt)
                    .Add("@DATA_OCORRENCIA_INICIAL", DateTime.Now, OleDbType.DBTimeStamp)
                    .Add("@DATA_OCORRENCIA_FINAL", DateTime.Now, OleDbType.DBTimeStamp)
                    .Add("@TEMPO_CICLO", DBNull.Value, OleDbType.Integer)
                    .Add("@USUARIO_PAGAMENTO", sUsuario, OleDbType.VarChar, 50)
                    .Add("@STATUS", nStatus, OleDbType.Integer)
                    .Add("@VERSAO_TABLET_ULTIMA_MOVIMENTACAO", DBNull.Value, OleDbType.VarChar, 50)
                    .Add("@PRIORIDADE_A", DBNull.Value, OleDbType.VarChar, 50)
                    .Add("@MSG_ERRO", sMessage, OleDbType.VarChar, 500)
                    .Add("@CHAVE", DBNull.Value, OleDbType.VarChar, 50)
                    .Add("@GRUPO", DBNull.Value, OleDbType.VarChar, 50)
                    ;

                string sQuery = @"DECLARE @ID_MOV BIGINT

                                INSERT INTO [dbo].[WSQOLPICKINGMOV]
                                           ([ID_SEQ]
                                           ,[DATA_OCORRENCIA_INICIAL]
                                           ,[DATA_OCORRENCIA_FINAL]
                                           ,[TEMPO_CICLO]
                                           ,[USUARIO_PAGAMENTO]
                                           ,[STATUS]
                                           ,[VERSAO_TABLET_ULTIMA_MOVIMENTACAO]
                                           ,[PRIORIDADE_A]
                                           ,[MSG_ERRO]
                                           ,[CHAVE]
                                           ,[GRUPO])
                                     VALUES
                                           (@ID_SEQ
                                           ,@DATA_OCORRENCIA_INICIAL
                                           ,@DATA_OCORRENCIA_FINAL
                                           ,@TEMPO_CICLO
                                           ,@USUARIO_PAGAMENTO
                                           ,@STATUS
                                           ,@VERSAO_TABLET_ULTIMA_MOVIMENTACAO
                                           ,@PRIORIDADE_A
                                           ,@MSG_ERRO
                                           ,@CHAVE
                                           ,@GRUPO)
                                
                                SET @ID_MOV = SCOPE_IDENTITY()
                                
                                SELECT @ID_MOV";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.GetResultado();

                    if (oResult != null)
                        nResult = (long)oResult;
                    else
                        throw new Exception("Não foi possível identificar ID de retorno da tabela WSQOLPICKINGMOV!");

                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Insert: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }

                return nResult;
            }
        }

        private void InsertLESPickingMovItens(long nIdMov, long nIdSeqItem, string sMaterial, int nStatus)
        {
            string sMessage = "Alteração rastreabilidade Item WEB";

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@ID_MOV", nIdMov, OleDbType.BigInt)
                    .Add("@ITEM", sMaterial, OleDbType.VarChar, 50)
                    .Add("@DATA_OCORRENCIA_INICIAL", DateTime.Now, OleDbType.DBTimeStamp)
                    .Add("@DATA_OCORRENCIA_FINAL", DateTime.Now, OleDbType.DBTimeStamp)
                    .Add("@ID_SEQ_ITENS", nIdSeqItem, OleDbType.BigInt)
                    .Add("@TEMPO_CICLO", DBNull.Value, OleDbType.Integer)
                    .Add("@STATUS", nStatus, OleDbType.Integer)
                    .Add("@QUANTIDADE", 1, OleDbType.Numeric)
                    .Add("@QUANTIDADE_PAGO", 0, OleDbType.Numeric)
                    .Add("@QUANTIDADE_FALTANTE", 0, OleDbType.Numeric)
                    .Add("@LOCAL_DEBITO", String.Empty, OleDbType.VarChar, 50)
                    .Add("@LOCAL_CREDITO", String.Empty, OleDbType.VarChar, 50)
                    .Add("@EMBALAGEM", false, OleDbType.Boolean)
                    .Add("@VERSAO_TABLET_ULTIMA_MOVIMENTACAO", String.Empty, OleDbType.VarChar, 50)
                    .Add("@LOCAL_DEBITO_CODIGO_RASTREABILIDADE", String.Empty, OleDbType.VarChar, 50)
                    .Add("@MSG_ERRO", sMessage, OleDbType.VarChar, 500)
                    .Add("@LOCAL_CREDITO_CODIGO_RASTREABILIDADE", String.Empty, OleDbType.VarChar, 50)
                    .Add("@QUANTIDADE_ENTREGUE", 0, OleDbType.Numeric)
                    .Add("@PICKING_FINISH_KEY", Guid.Empty, OleDbType.Guid)
                    ;

                string sQuery = @"INSERT INTO [WSQOLPICKINGMOVITENS]
                                       ([ID_MOV]
                                       ,[ITEM]
                                       ,[DATA_OCORRENCIA_INICIAL]
                                       ,[DATA_OCORRENCIA_FINAL]
                                       ,[ID_SEQ_ITENS]
                                       ,[TEMPO_CICLO]
                                       ,[STATUS]
                                       ,[QUANTIDADE]
                                       ,[QUANTIDADE_PAGO]
                                       ,[QUANTIDADE_FALTANTE]
                                       ,[LOCAL_DEBITO]
                                       ,[LOCAL_CREDITO]
                                       ,[EMBALAGEM]
                                       ,[VERSAO_TABLET_ULTIMA_MOVIMENTACAO]
                                       ,[LOCAL_DEBITO_CODIGO_RASTREABILIDADE]
                                       ,[MSG_ERRO]
                                       ,[LOCAL_CREDITO_CODIGO_RASTREABILIDADE]
                                       ,[QUANTIDADE_ENTREGUE]
                                       ,[PICKING_FINISH_KEY])
                                 VALUES
                                       (@ID_MOV
                                       , @ITEM
                                       , @DATA_OCORRENCIA_INICIAL
                                       , @DATA_OCORRENCIA_FINAL
                                       , @ID_SEQ_ITENS
                                       , @TEMPO_CICLO
                                       , @STATUS
                                       , @QUANTIDADE
                                       , @QUANTIDADE_PAGO
                                       , @QUANTIDADE_FALTANTE
                                       , @LOCAL_DEBITO
                                       , @LOCAL_CREDITO
                                       , @EMBALAGEM
                                       , @VERSAO_TABLET_ULTIMA_MOVIMENTACAO
                                       , @LOCAL_DEBITO_CODIGO_RASTREABILIDADE
                                       , @MSG_ERRO
                                       , @LOCAL_CREDITO_CODIGO_RASTREABILIDADE
                                       , @QUANTIDADE_ENTREGUE
                                       , @PICKING_FINISH_KEY)"
                                        ;

                try
                {
                    oCommand.SetCommandText(sQuery);
                    oCommand.Execute();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Insert: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }
            }
        }

        private string GetCodigoRastreabilidadeItemPickingMTO(string sOrdemProducao)
        {
            string sQuery = "SELECT 1 FROM " +
                                "WSQOPCP2SEQPRODUCAO AS SEQ " +
                                "INNER JOIN WSQOPCP2SEQPRODUCAOREGRAS REG_7003 " +
                                    "ON SEQ.ID = REG_7003.ID_SEQ " +
                                "WHERE SEQ.ORDEM_PRODUCAO = \'" + sOrdemProducao + "\'" +
                                "AND REG_7003.ATRIBUTO <> '' " +
                                "AND REG_7003.REGRA = '7003'"
                                ;

            Object oResult = AI1627CommonInterface.DB.sqoClassControllerDB.ExecuteScalar(sQuery);

            if (oResult != null)
                return oResult.ToString();
            else
                return String.Empty;
        }

        private bool ValidateSituacaoItemRemessa(ExpedicaoAlterarRastreabilidadePickingPersistencia oRastreabilidadePickingPersistencia)
        {
            bool bResult = false;

            var oRemessaItem = sqoClassLESEXPRemessaItensControlerDB.GetLESExpRemessaItensByCodigoRemessaAndItemRemessa(oRastreabilidadePickingPersistencia.Picking, oRastreabilidadePickingPersistencia.IdItemDoc);

            if (oRemessaItem.Situacao != "S")
                bResult = true;

            return bResult;
        }

        private string ValidaPickingJaUtilizado(ExpedicaoAlterarRastreabilidadePickingPersistencia oRastreabilidadePickingPersistencia)
        {
            string sDescription = "";

            var itens = sqoClassLESPickingControlerDB.GetLESPickingItem(null, "WHERE CODIGO_RASTREABILIDADE = '" + oRastreabilidadePickingPersistencia.CodigoRastreabilidadeItem + "'", true);

            foreach (var oItem in itens)
            {
                var oListPickings = sqoClassLESPickingControlerDB.GetLESPicking(oWhere: "WHERE ID = " + oItem.IdSeq + "AND STATUS <> 5 AND ORIGEM = 'REM'");

                foreach (var oPicking in oListPickings)
                {

                    var oRemessa = sqoClassLESEXPRemessaControlerDB.GetLESExpRemessaByCodigoRemessa(oPicking.CodigoRastreabilidade);

                    if (oRemessa.StatusErp != "D")
                    {
                        sDescription += " - Código de Rastreabilidade: " + oRastreabilidadePickingPersistencia.CodigoRastreabilidadeItem + " já está sendo utilizado no Picking: " + oPicking.CodigoRastreabilidade + "!\n";

                        break;
                    }
                }

            }

            return sDescription;

        }

        private string ValidateDeposito(sqoClassLESExpedicaoVolumePersistence oVolume, ExpedicaoAlterarRastreabilidadePickingPersistencia oRastreabilidadePickingPersistencia)
        {
            string sDescription = "";

            var oItemRemessa = sqoClassLESEXPRemessaItensControlerDB.GetLESExpRemessaItensByCodigoRemessaAndItemRemessa(oRastreabilidadePickingPersistencia.Picking, oRastreabilidadePickingPersistencia.IdItemDoc);

            var oListEstoque = sqoClassLESEstoqueAtualControlerDB.GetLESEstoqueAtual(null, null, null, oVolume.CodigoRastreabilidade, null, null, "> 0");

            var oLocais = new List<sqoClassLESLocal>();

            foreach (var oEstoque in oListEstoque)
            {
                oLocais.Add(sqoClassLESLocaisControlerDB.GetLESLocal(oCodigo: oEstoque.LocalOrigemMov));
            }

            var oDeposito = oLocais.Find(x => x.LocalIntegracao == oItemRemessa.LocalDeposito);

            if (oDeposito == null)
            {
                sDescription += "Volume " + oVolume.CodigoRastreabilidade + " não existe no depósito " + oItemRemessa.LocalDeposito + " do item da remessa!"
                    + Environment.NewLine;
            }

            return sDescription;
        }

    }

    internal class WebExpedicaoAlterarRastPickingPersistence
    {
        public static int QueryScreenData(ExpedicaoAlterarRastreabilidadePickingPersistencia oRastreabilidadePickingPersistencia)
        {
            int nResult = 0;

            string sQuery = @"SELECT
                            	1
                            FROM 
                            	WSQOLPICKINGSEQ AS SEQ
                            INNER JOIN
                            	WSQOLPICKINGSEQSTATUS AS SEQ_STATUS
                            ON
                            	SEQ_STATUS.STATUS = SEQ.STATUS
                            INNER JOIN
                            	WSQOLPICKINGSEQITENS AS ITENS
                            ON
                            	ITENS.ID_SEQ = SEQ.ID
                            INNER JOIN
                            	WSQOPCP2PECA AS PECA
                            ON
                            	ITENS.ITEM = PECA.CODIGO_PECA
                            AND PECA.TIPO_PECA = @ZFER
                            WHERE
                            	SEQ.STATUS = @NAO_INICIADO
                            --COLUNAS
                            AND SEQ.ID = @ID
                            AND ITENS.ID = @ID_ITEM
                            AND SEQ.CHAVE = @CHAVE
                            AND SEQ.CODIGO_RASTREABILIDADE = @PICKING
                            --AND SEQ.DATA_PROCESSO = @DATA_PROCESSO
                            AND SEQ_STATUS.STATUS = @STATUS
                            --AND SEQ_STATUS.DESCRICAO = @DESCRICAO_STATUS
                            AND ITENS.ITEM = @MATERIAL
                            --AND PECA.DESCRICAO = @DESCRICAO
                            AND ITENS.CODIGO_RASTREABILIDADE = @CODIGO_RASTREABILIDADE
                            AND ISNULL(ITENS.ID_ITEM_DOC,'') = @ID_ITEM_DOC";

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                try
                {
                    oCommand
                        .Add("@ZFER", "ZFER", OleDbType.VarChar, 10)
                        .Add("@NAO_INICIADO", 0, OleDbType.Integer)
                        .Add("@ID", oRastreabilidadePickingPersistencia.Id, OleDbType.BigInt)
                        .Add("@ID_ITEM", oRastreabilidadePickingPersistencia.IdItem, OleDbType.BigInt)
                        .Add("@CHAVE", oRastreabilidadePickingPersistencia.Chave, OleDbType.VarChar, 50)
                        .Add("@PICKING", oRastreabilidadePickingPersistencia.Picking, OleDbType.VarChar, 50)
                        .Add("@STATUS", oRastreabilidadePickingPersistencia.Status, OleDbType.Integer)
                        .Add("@MATERIAL", oRastreabilidadePickingPersistencia.Material, OleDbType.VarChar, 50)
                        .Add("@CODIGO_RASTREABILIDADE", oRastreabilidadePickingPersistencia.CodigoRastreabilidade, OleDbType.VarChar, 50)
                        .Add("@ID_ITEM_DOC", oRastreabilidadePickingPersistencia.IdItemDoc, OleDbType.VarChar, 10)
                        ;

                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.GetResultado();

                    if (oResult != null)
                        nResult = (int)oResult;

                    return nResult;
                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }
            }
        }
    }
}
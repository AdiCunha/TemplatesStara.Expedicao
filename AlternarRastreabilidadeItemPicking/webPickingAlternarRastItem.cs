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

namespace TelaDinamica.Expedicao
{
    [TemplateDebug("webPickingAlternarRastItem")]
    public class webPickingAlternarRastItem : IProcessMovimentacao
    {
        private sqoClassSetMessageDefaults oClassSetMessageDefaults;
        private webPickingAlternarRasItemPersistencia oPickingAlternarRasItemPersistencia;

        private List<sqoClassLESPickingPersistence> oListPicking1;
        private List<sqoClassLESPickingPersistence> oListPicking2;

        private List<sqoClassLESPickingItemPersistence> oListPickingItem1;
        private List<sqoClassLESPickingItemPersistence> oListPickingItem2;

        private String sUsuario = String.Empty;
        private String sNivel = String.Empty;
        private int nQtdErros = 0;
        private String sMessage = "Falha na validação de dados";
        private String sDescription = String.Empty;

        public sqoClassMessage Executar(string sAction, string sXmlDados, string sXmlType, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao, List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sNivel, string sUsuario, object oObjAux)
        {
            using (sqoClassDbConnection oDBConnection = new sqoClassDbConnection())
            {
                this.Init(sXmlDados, sUsuario, sNivel);

                this.Validate();

                this.ProcessBusinessLogic(oDBConnection);
            }

            return this.oClassSetMessageDefaults.Message;
        }

        private void Init(String sXmlDados, String sUsuario, String sNivel)
        {
            this.oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());

            this.oPickingAlternarRasItemPersistencia = sqoClassBiblioSerDes.DeserializeObject<webPickingAlternarRasItemPersistencia>(sXmlDados);

            this.sUsuario = sUsuario;

            this.sNivel = sNivel;

            //tamanho padrão do id item doc é de 6 caracteres
            while (this.oPickingAlternarRasItemPersistencia.IdItemDocUpdate.Length < 6)
            {
                this.oPickingAlternarRasItemPersistencia.IdItemDocUpdate = "0" + this.oPickingAlternarRasItemPersistencia.IdItemDocUpdate;
            }

            oListPicking1 = sqoClassLESPickingControlerDB.GetLESPicking(oCodigoRastreabilidade: this.oPickingAlternarRasItemPersistencia.Remessa);

            oListPicking2 = sqoClassLESPickingControlerDB.GetLESPicking(oCodigoRastreabilidade: this.oPickingAlternarRasItemPersistencia.RemessaUpdate);

            this.ValidateListPicking();

            oListPickingItem1 = sqoClassLESPickingControlerDB.GetLESPickingItem(null, "WHERE ID_SEQ = " + this.oListPicking1[0].Id + " AND ID_ITEM_DOC = " + this.oPickingAlternarRasItemPersistencia.IdItemDoc);

            oListPickingItem2 = sqoClassLESPickingControlerDB.GetLESPickingItem(null, "WHERE ID_SEQ = " + this.oListPicking2[0].Id + " AND ID_ITEM_DOC = " + this.oPickingAlternarRasItemPersistencia.IdItemDocUpdate);
        }

        private void Validate()
        {
            if (!oListPickingItem1.Any() || !oListPickingItem2.Any())
            {
                this.nQtdErros++;

                this.sDescription += this.nQtdErros.ToString() + " - Item da remessa informado (" + this.oPickingAlternarRasItemPersistencia.IdItemDocUpdate + ") não existe na base de dados!";
            }

            else if (oListPicking2[0].Origem != "REM")
            {
                this.nQtdErros++;

                this.sDescription += this.nQtdErros.ToString() + " - Picking encontrado com origem diferente de \"REM\"!" + Environment.NewLine;
            }

            else if (this.oPickingAlternarRasItemPersistencia.Remessa.Equals(this.oPickingAlternarRasItemPersistencia.RemessaUpdate))
            {
                this.nQtdErros++;

                this.sDescription += this.nQtdErros.ToString() + " - Remessa preenchida no formulário igual a remessa do parâmetro!" + Environment.NewLine;
            }
            //else if (String.IsNullOrEmpty(this.oPickingAlternarRasItemPersistencia.RemessaUpdate))
            //{
            //    this.nQtdErros++;

            //    this.sDescription += this.nQtdErros.ToString() + " - Campo \"Remessa\" preenchimento obrigatório!" + Environment.NewLine;
            //}

            //else if (String.IsNullOrEmpty(this.oPickingAlternarRasItemPersistencia.IdItemDocUpdate))
            //{
            //    this.nQtdErros++;

            //    this.sDescription += this.nQtdErros.ToString() + " - Campo \"Item Remessa\" preenchimento obrigatório!" + Environment.NewLine;
            //}

            else
            {
                //validações remessa informada pelo usuário
                //if (!oListPickingItem2.Any())
                //{
                //    this.nQtdErros++;

                //    this.sDescription += this.nQtdErros.ToString() + " - Remessa informada (" + this.oPickingAlternarRasItemPersistencia.RemessaUpdate + ") não existe na base de dados!" + Environment.NewLine;
                //}

                this.ValidateSituacaoRemessa();

                foreach (var oPicking in oListPicking2)
                {
                    if (oPicking.Status != AI1627CommonInterface.LESStatus.STATUS_PICKING.NAO_INICIADO)
                    {
                        this.nQtdErros++;

                        this.sDescription += this.nQtdErros.ToString() + " - Não é permitido alternar rastreabilidade item do picking com status diferente de Não Iniciado! Remessa: " + oPicking.CodigoRastreabilidade + " Status: "
                            + oPicking.Status + Environment.NewLine;
                    }
                }

                //necessário fazer o count pois a lista pode não conter o mesmo material
                if (this.oListPickingItem2.FindAll(x => x.Item.Equals(this.oPickingAlternarRasItemPersistencia.Material)).Count() < 1)
                {
                    this.nQtdErros++;

                    this.sDescription += this.nQtdErros.ToString() + " - Remessa " + this.oPickingAlternarRasItemPersistencia.RemessaUpdate + " não possui item com o material " + this.oPickingAlternarRasItemPersistencia.Material + " !"
                        + Environment.NewLine;
                }

            }

            this.ValidateMessage();

        }

        private void ProcessBusinessLogic(sqoClassDbConnection oDBConnection)
        {
            try
            {
                oDBConnection.BeginTransaction();

                String sMessage = "Alternada rastreabilidade Item WEB";

                long nIdMov1 = CommonStaraPickingControlerDB.PickingMovInsert(this.oListPicking1[0].Id, DateTime.Now, DateTime.Now, sUsuarioPagamento: sUsuario, nStatus: (int)oListPickingItem1[0].Status, sMsgErro: sMessage);

                //deletar itens do picking 1
                foreach (var oPickingItem1 in this.oListPickingItem1)
                {
                    CommonStaraPickingControlerDB.PickingItemDeleteById(oPickingItem1.Id);

                    if (!String.IsNullOrEmpty(oPickingItem1.CodigoRastreabilidade))
                    {
                        this.DeleteExpedicaoItemRemessaVolume(oPickingItem1);
                    }

                    long nIdMovItem = CommonStaraPickingControlerDB.PickingMovItemInsert(
                        DateTime.Now
                        , DateTime.Now
                        , nIdMov1
                        , oPickingItem1.Item
                        , oPickingItem1.Id
                        , 0
                        , 6 //status estornado
                        , oPickingItem1.Quantidade
                        , oPickingItem1.QuantidadePago
                        , 0
                        , String.Empty
                        , String.Empty
                        , false
                        , String.Empty
                        , String.Empty
                        , sMessage
                        , String.Empty
                        , oPickingItem1.QuantidadeEntregue
                        , Guid.Empty
                        );

                    if (nIdMovItem < 1)
                        throw new Exception("Falha ao inserir dados na tabela WSQOLPICKINGMOVITENS");
                }


                long nIdMov2 = CommonStaraPickingControlerDB.PickingMovInsert(this.oListPicking2[0].Id, DateTime.Now, DateTime.Now, sUsuarioPagamento: sUsuario, nStatus: (int)oListPickingItem2[0].Status, sMsgErro: sMessage);

                //deletar itens do picking 2
                foreach (var oPickingItem2 in this.oListPickingItem2)
                {
                    CommonStaraPickingControlerDB.PickingItemDeleteById(oPickingItem2.Id);

                    if (!String.IsNullOrEmpty(oPickingItem2.CodigoRastreabilidade))
                    {
                        this.DeleteExpedicaoItemRemessaVolume(oPickingItem2);
                    }

                    long nIdMovItem = CommonStaraPickingControlerDB.PickingMovItemInsert(
                         DateTime.Now
                        , DateTime.Now
                        , nIdMov1
                        , oPickingItem2.Item
                        , oPickingItem2.Id
                        , 0
                        , 6 //status estornado
                        , oPickingItem2.Quantidade
                        , oPickingItem2.QuantidadePago
                        , 0
                        , String.Empty
                        , String.Empty
                        , false
                        , String.Empty
                        , String.Empty
                        , sMessage
                        , String.Empty
                        , oPickingItem2.QuantidadeEntregue
                        , Guid.Empty
                        );

                    if (nIdMovItem < 1)
                        throw new Exception("Falha ao inserir dados na tabela WSQOLPICKINGMOVITENS");
                }


                //inserir intens do picking 2
                foreach (var oPickingItem2 in oListPickingItem2)
                {
                    sqoClassLESPickingItemPersistence oListPickingItemNew = new sqoClassLESPickingItemPersistence
                    {
                        IdSeq = oListPicking1[0].Id
                        ,
                        Item = oPickingItem2.Item
                        ,
                        Quantidade = oPickingItem2.Quantidade
                        ,
                        QuantidadePago = oPickingItem2.QuantidadePago
                        ,
                        Status = oPickingItem2.Status
                        ,
                        Peso = oPickingItem2.Peso
                        ,
                        CodigoLeitura = oPickingItem2.CodigoLeitura
                        ,
                        CodigoRastreabilidade = oPickingItem2.CodigoRastreabilidade
                        ,
                        IntegracaoErp = oPickingItem2.IntegracaoErp
                        ,
                        IdItemDoc = this.oPickingAlternarRasItemPersistencia.IdItemDoc
                        ,
                        QuantidadeEntregue = oPickingItem2.QuantidadeEntregue
                    };

                    long nIdItem = oListPickingItemNew.Insert();

                    if (nIdItem < 1)
                        throw new Exception("Falaha ao inserir item do picking " + oListPickingItemNew.ToString());

                    if (!String.IsNullOrEmpty(oPickingItem2.CodigoRastreabilidade))
                    {
                        this.UpdateVolume(oListPicking1[0].CodigoRastreabilidade, oPickingItem2.CodigoRastreabilidade);

                        this.InsertExpedicaoItemRemessaVolume(this.oPickingAlternarRasItemPersistencia.Remessa, oPickingItem2.CodigoRastreabilidade, this.oPickingAlternarRasItemPersistencia.IdItemDoc);
                    }
                }

                //inserir intens do picking 1
                foreach (var oPickingItem1 in oListPickingItem1)
                {
                    sqoClassLESPickingItemPersistence oListPickingItemNew = new sqoClassLESPickingItemPersistence
                    {
                        IdSeq = oListPicking2[0].Id
                        ,
                        Item = oPickingItem1.Item
                        ,
                        Quantidade = oPickingItem1.Quantidade
                        ,
                        QuantidadePago = oPickingItem1.QuantidadePago
                        ,
                        Status = oPickingItem1.Status
                        ,
                        Peso = oPickingItem1.Peso
                        ,
                        CodigoLeitura = oPickingItem1.CodigoLeitura
                        ,
                        CodigoRastreabilidade = oPickingItem1.CodigoRastreabilidade
                        ,
                        IntegracaoErp = oPickingItem1.IntegracaoErp
                        ,
                        IdItemDoc = this.oPickingAlternarRasItemPersistencia.IdItemDocUpdate
                        ,
                        QuantidadeEntregue = oPickingItem1.QuantidadeEntregue
                    };

                    long nIdItem = oListPickingItemNew.Insert();

                    if (nIdItem < 1)
                        throw new Exception("Falaha ao inserir item do picking " + oListPickingItemNew.ToString());

                    if (!String.IsNullOrEmpty(oPickingItem1.CodigoRastreabilidade))
                    {
                        this.UpdateVolume(oListPicking2[0].CodigoRastreabilidade, oPickingItem1.CodigoRastreabilidade);

                        this.InsertExpedicaoItemRemessaVolume(this.oPickingAlternarRasItemPersistencia.RemessaUpdate, oPickingItem1.CodigoRastreabilidade, this.oPickingAlternarRasItemPersistencia.IdItemDocUpdate);
                    }

                }

                oClassSetMessageDefaults.SetarOk();

                //oDBConnection.Rollback();
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

        private void ValidateListPicking()
        {
            if (!oListPicking1.Any() || !oListPicking2.Any())
            {
                this.nQtdErros++;

                this.sDescription += this.nQtdErros.ToString() + " - Não foi encontrado nenhum picking!" + Environment.NewLine;

            }
            else if (oListPicking1.Count() > 1 || oListPicking2.Count() > 1)
            {
                this.nQtdErros++;

                this.sDescription += this.nQtdErros.ToString() + " - Encontrado mais de um picking!" + Environment.NewLine;
            }

            if (!String.IsNullOrEmpty(this.sDescription))
                this.ValidateMessage();
        }

        private void DeleteExpedicaoItemRemessaVolume(sqoClassLESPickingItemPersistence oLESPickingItemPersistence)
        {
            List<sqoClassLESExpedicaoVolumePersistence> oListVolume = sqoClassLESExpedicaoVolumeControlerDB.GetExpedicaoVolumesByCodigoRastreabilidade(oLESPickingItemPersistence.CodigoRastreabilidade);

            //Verificar se não possui vinculo duplicado
            this.CheckVolume(oListVolume);

            foreach (var oVolume in oListVolume)
            {
                this.DeleteExpedicaoItemRemessaVolumeByIdExpedicaoVolume(oVolume.Id);
            }

        }

        private void DeleteExpedicaoItemRemessaVolumeByIdExpedicaoVolume(long nIdVolume)
        {
            String sQuery = "DELETE FROM [WSQOLEXPEDICAOITEMREMESSAVOLUME] WHERE [ID_EXPEDICAO_VOLUME] = @ID_EXPEDICAO_VOLUME ";

            List<OleDbParameter> oLiParameter = new List<OleDbParameter>();

            oLiParameter.Add(new OleDbParameter("@ID_EXPEDICAO_VOLUME", OleDbType.BigInt) { Value = nIdVolume });

            AI1627CommonInterface.DB.sqoClassControllerDB.ExecuteNonQuery(sQuery, CommandType.Text, oLiParameter);
        }

        private void InsertExpedicaoItemRemessaVolume(String sCodigoRemessa, String sCodigoRastreabilidade, String nIdItemDoc)
        {
            sqoClassLESEXPRemessaItensPersistence oRemessaItem = sqoClassLESEXPRemessaItensControlerDB.GetLESExpRemessaItensByCodigoRemessaAndItemRemessa(sCodigoRemessa, nIdItemDoc);

            if (oRemessaItem == null)
                throw new Exception("Não foi possível obter lista oRemessaItem");

            List<sqoClassLESExpedicaoVolumePersistence> oListVolume = sqoClassLESExpedicaoVolumeControlerDB.GetExpedicaoVolumesByCodigoRastreabilidade(sCodigoRastreabilidade);

            if (oListVolume.Any())
            {
                //Verificar se não possui vinculo duplicado
                this.CheckVolume(oListVolume);

                //throw new Exception("Não foi possível obter lista oListVolume");
                sqoClassLESExpedicaoVolumeControlerDB.InsertExpedicaoItemRemessaVolume(oRemessaItem, oListVolume[0]);
            }
        }

        private void CheckVolume(List<sqoClassLESExpedicaoVolumePersistence> oListVolume)
        {
            sqoClassDefaultPersistence oClassDefaultPersistence = sqoClassLESExpedicaoVolumeControlerDB.CheckVolumeList(oListVolume);

            if (oClassDefaultPersistence.Ok == false)
                throw new sqoClassExceptionMessageUser(oClassDefaultPersistence.Message);
        }

        private void UpdateVolume(String sCodigoRemessa, String sCodigoRastreabilidade)
        {
            List<sqoClassLESExpedicaoVolumePersistence> oListVolume = sqoClassLESExpedicaoVolumeControlerDB.GetExpedicaoVolumesByCodigoRastreabilidade(sCodigoRastreabilidade);

            if (oListVolume.Any())
            {
                //Verificar se não possui vinculo duplicado
                this.CheckVolume(oListVolume);

                //throw new Exception("Não foi possível obter lista oListVolume");
                sqoClassLESExpedicaoVolumeControlerDB.ExecuteWsqolUpdateExpedicaoVolume(
                    oListVolume[0].Id
                    , sCodigoRemessa: sCodigoRemessa
                    , sObservacao: this.sNivel
                    , sUsuarioUltimaMovimentacao: this.sUsuario
                    , dDataUltimaMovimentacao: DateTime.Now);
            }

        }

        private void ValidateSituacaoRemessa()
        {
            //sqoClassLESEXPRemessaPersistence oExpRemessa = sqoClassLESEXPRemessaControlerDB.GetLESExpRemessaByCodigoRemessa(this.oPickingAlternarRasItemPersistencia.RemessaUpdate);

            //if (oExpRemessa == null)
            //throw new Exception("Não foi possivel obter objeto oExpRemessa");

            sqoClassLESEXPRemessaItensPersistence oExpRemessaItens = sqoClassLESEXPRemessaItensControlerDB.GetLESExpRemessaItensByCodigoRemessaAndItemRemessa(
                this.oPickingAlternarRasItemPersistencia.RemessaUpdate
                , this.oPickingAlternarRasItemPersistencia.IdItemDocUpdate
                );

            if (oExpRemessaItens.Situacao != "S")
            {
                this.nQtdErros++;

                this.sDescription += this.nQtdErros.ToString() + " - Permitido alternar rastreabilidade apenas de remessas MTS!" + Environment.NewLine;
            }
        }

    }

}
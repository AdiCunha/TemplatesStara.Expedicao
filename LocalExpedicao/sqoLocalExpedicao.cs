using AI1627Common20.TemplateDebugging;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Sequor.LES.Expedition.Api;
using Sequor.LES.Expedition.Model;
using sqoClassLibraryAI0502Biblio;
using sqoClassLibraryAI0502Message;
using sqoClassLibraryAI0502VariaveisSistema;
using sqoClassLibraryAI1151FilaProducao;
using sqoClassLibraryAI1151FilaProducao.Estrutura;
using sqoClassLibraryAI1151FilaProducao.Process;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;

namespace sqoTraceabilityStation
{
    [TemplateDebug("PCP - Local Expedicao Action")]
    public class TemplateLocalExpedicaoCriteria : sqoClassProcessMovimentacao
    {
        private string sParamListagem_CRT_Texto = "";

        private string basePath => "http://nmtwnseqiisdev.dcstara.com.br:81/Sequor.LES.Expedition";

        public override sqoClassMessage Executar(string sAction
                                                , string sXmlDados
                                                , string sXmlType
                                                , List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao
                                                , List<sqoClassParametrosEstrutura> oListaParametrosListagem
                                                , string sNivel
                                                , string sUsuario
                                                , object oObjAux)
        {
            sqoClassMessage oClassMessage = new sqoClassMessage();

            GetParametrosListagem(oListaParametrosListagem);
            sXmlDados = sXmlDados.Replace("null", "");
            sqoClassPcpDynCriteriaItem oItem = null;

            try
            {
                oItem = sqoClassBiblioSerDes.DeserializeObject<sqoClassPcpDynCriteriaItem>(sXmlDados);
                oClassMessage.Ok = true;

                switch (sAction)
                {
                    case "Inserir":
                        try
                        {
                            if (oItem.Tipo_Expedicao_Codigo == "CARREGAMENTO")

                            this.Inserir(oItem);

                            oClassMessage.Message = "Local de expedição inserido com sucesso - ID: " + oItem.ID;
                            oClassMessage.MessageType = sqoClassMessage.MessageTypeEnum.OK;
                            oClassMessage.MessageDescription = "";
                            oClassMessage.Ok = true;
                        }
                        catch (Exception e)
                        {
                            oClassMessage.Message = "FALHA ao inserir novo local de expedição";
                            oClassMessage.MessageType = sqoClassMessage.MessageTypeEnum.ERROR;
                            oClassMessage.MessageDescription = e.Message;
                            oClassMessage.Ok = false;
                        }
                        break;

                    case "Editar":
                        try
                        {
                            this.Editar(oItem);

                            oClassMessage.Message = "Local de expedição ID: " + oItem.ID + " atualizado com sucesso";
                            oClassMessage.MessageType = sqoClassMessage.MessageTypeEnum.OK;
                            oClassMessage.MessageDescription = "";
                            oClassMessage.Ok = true;
                        }
                        catch (Exception e)
                        {
                            oClassMessage.Message = "FALHA ao editar local de expedição ID: " + oItem.ID;
                            oClassMessage.MessageType = sqoClassMessage.MessageTypeEnum.ERROR;
                            oClassMessage.MessageDescription = e.Message;
                            oClassMessage.Ok = false;
                        }
                        break;

                    case "Duplicar":
                        try
                        {
                            this.Duplicar(oItem);

                            oClassMessage.Message = "Local de expedição ID: " + oItem.ID + " duplicado com sucesso";
                            oClassMessage.MessageType = sqoClassMessage.MessageTypeEnum.OK;
                            oClassMessage.MessageDescription = "";
                            oClassMessage.Ok = true;
                        }
                        catch (Exception e)
                        {
                            oClassMessage.Message = "FALHA ao duplicar local de expedição ID: " + oItem.ID;
                            oClassMessage.MessageType = sqoClassMessage.MessageTypeEnum.ERROR;
                            oClassMessage.MessageDescription = e.Message;
                            oClassMessage.Ok = false;
                        }
                        break;

                    case "Excuir":
                        try
                        {
                            this.Excluir(oItem);

                            oClassMessage.Message = "Local de expedição ID: " + oItem.ID + " removido com sucesso";
                            oClassMessage.MessageType = sqoClassMessage.MessageTypeEnum.OK;
                            oClassMessage.MessageDescription = "";
                            oClassMessage.Ok = true;
                        }
                        catch (Exception e)
                        {
                            oClassMessage.Message = "FALHA ao remover local de expedição ID: " + oItem.ID;
                            oClassMessage.MessageType = sqoClassMessage.MessageTypeEnum.ERROR;
                            oClassMessage.MessageDescription = e.Message;
                            oClassMessage.Ok = false;
                        }
                        break;

                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                oClassMessage.Message = "Erro ao realizar ação!";
                oClassMessage.MessageType = sqoClassMessage.MessageTypeEnum.ERROR;
                oClassMessage.MessageDescription = e.Message;
                oClassMessage.Ok = false;
            }
            finally
            {
                //
            }
            return oClassMessage;
        }

        private void Inserir(sqoClassPcpDynCriteriaItem oPersistencia)
        {
            ShipmentLocationApi api = new ShipmentLocationApi(basePath);

            ShipmentLocationInputModel oPersistenciaMapped = new ShipmentLocationInputModel
            {
                LocalExpedicao = oPersistencia.Local_Expedicao,
                ChaveExpedicao = oPersistencia.Chave_Expedicao,
                Tipo = oPersistencia.Tipo_Expedicao_Codigo,
                Pais = oPersistencia.Pais,
                LastUpdateDate = oPersistencia.Last_Update_Date,
                LastUpdateUser = oPersistencia.Last_Update_User,
                Message = oPersistencia.Message
            };

            api.ApiShipmentLocationCreate(oPersistenciaMapped);
        }

        private void Editar(sqoClassPcpDynCriteriaItem oPersistencia)
        {
            ShipmentLocationApi api = new ShipmentLocationApi(basePath);

            ShipmentLocationModel oPersistenciaMapped = new ShipmentLocationModel
            {
                Id = oPersistencia.ID,
                ChaveExpedicao = oPersistencia.Chave_Expedicao,
                Tipo = oPersistencia.Tipo_Expedicao_Codigo,
                LastUpdateDate = oPersistencia.Last_Update_Date,
                LastUpdateUser = oPersistencia.Last_Update_User,
                LocalExpedicao = oPersistencia.Local_Expedicao,
                Pais = oPersistencia.Pais,
                Message = oPersistencia.Message
            };

            api.ApiShipmentLocationUpdate(oPersistenciaMapped);

        }

        private void Excluir(sqoClassPcpDynCriteriaItem oPersistencia)
        {
            ShipmentLocationApi api = new ShipmentLocationApi(basePath);

            api.ApiShipmentLocationDelete(oPersistencia.ID);
        }

        public void GetParametrosListagem(List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao)
        {
            foreach (sqoClassParametrosEstrutura oClassParametrosEstrutura in oListaParametrosMovimentacao)
            {
                switch (oClassParametrosEstrutura.Campo)
                {
                    case "CRT_Texto":
                        this.sParamListagem_CRT_Texto = oClassParametrosEstrutura.Valor;
                        break;
                }
            }
        }
    }
}
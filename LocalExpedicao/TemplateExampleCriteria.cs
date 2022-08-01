using AI1627Common20.TemplateDebugging;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
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
    public class TemplateExampleCriteria : sqoClassProcessMovimentacao
    {
        private string sParamListagem_CRT_Texto = "";      

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
                            this.Inserir(oItem);

                        oClassMessage.Message = "Local de expedição inserido com sucesso - ID: "+ oItem.ID;
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

                            oClassMessage.Message = "Local de expedição ID: "+ oItem.ID +" atualizado com sucesso";
                            oClassMessage.MessageType = sqoClassMessage.MessageTypeEnum.OK;
                            oClassMessage.MessageDescription = "";
                            oClassMessage.Ok = true;
                        }
                        catch (Exception e)
                        {
                            oClassMessage.Message = "FALHA ao editar local de expedição ID: "+ oItem.ID;
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
                            oClassMessage.Message = "FALHA ao duplicar local de expedição ID: "+ oItem.ID;
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
                            oClassMessage.Message = "FALHA ao remover local de expedição ID: "+ oItem.ID;
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



        #region OLD
        //    try
        //    {
        //        oDBconnection = sqoClassConnectionFactory.GetConnection();

        //        if (sAction == "Importar")
        //        {
        //            List<sqoClassPcpDynCriteriaItem> oListPersistencia = new List<sqoClassPcpDynCriteriaItem>();

        //            oListPersistencia = ProcessXLSX(oListaParametrosMovimentacao, sNivel);

        //            oDBconnection.Open();

        //            int nRegistro = -1;

        //            try
        //            {
        //                sqoClassBiblioDB.BeginTransaction(oDBconnection);

        //                nRegistro = 1;

        //                foreach (sqoClassPcpDynCriteriaItem oItem in oListPersistencia)
        //                {
        //                    if (oItem.Acao == "ADICIONAR")
        //                    {
        //                        this.ValidaPersistencia(oDBconnection, oItem, oItem.Acao);
        //                        this.Adicionar(oDBconnection, oItem, sUsuario);
        //                    }
        //                    else if (oItem.Acao == "EDITAR")
        //                    {
        //                        this.ValidaPersistencia(oDBconnection, oItem, oItem.Acao);

        //                        if (this.Editar(oDBconnection, oItem) == 0)
        //                            throw (new Exception("Registro (" + oItem.ID + ") não existe."));
        //                    }
        //                    else if (oItem.Acao == "REMOVER")
        //                    {
        //                        if (this.Remover(oDBconnection, oItem, sUsuario) == 0)
        //                            throw (new Exception("Registro (" + oItem.ID + ") não existe."));
        //                    }
        //                    else if (!string.IsNullOrWhiteSpace(oItem.Acao))
        //                    {
        //                        throw (new Exception("Ação (" + oItem.Acao + ") não permitida."));
        //                    }

        //                    nRegistro++;
        //                }

        //                sqoClassBiblioDB.Commit(oDBconnection);
        //            }
        //            catch (Exception ex)
        //            {
        //                sqoClassBiblioDB.Rollback(oDBconnection);

        //                if (nRegistro != -1)
        //                    throw (new Exception("Registro " + nRegistro + " com problema!" + System.Environment.NewLine + System.Environment.NewLine + ex.Message));
        //                else
        //                    throw (ex);
        //            }

        //            oClassMessage.Message = "Importação realizada com sucesso.";
        //            oClassMessage.MessageType = sqoClassMessage.MessageTypeEnum.OK;
        //            oClassMessage.MessageDescription = "";
        //            oClassMessage.Ok = true;
        //        }
        //        else
        //        {
        //            oDBconnection.Open();

        //            sXmlDados = sXmlDados.Replace("null", "");
        //            sqoClassPcpDynCriteriaItem oItem = null;

        //            try
        //            {
        //                oItem = sqoClassBiblioSerDes.DeserializeObject<sqoClassPcpDynCriteriaItem>(sXmlDados);

        //                oClassMessage.Ok = true;
        //            }
        //            catch (Exception ex)
        //            {
        //                oClassMessage.Message = "Erro no cadastro!";
        //                oClassMessage.MessageType = sqoClassMessage.MessageTypeEnum.ERROR;
        //                oClassMessage.MessageDescription = "Não foi possível deserializar o objeto enviado para a template.";
        //                oClassMessage.Ok = false;
        //            }

        //            if (oClassMessage.Ok && !String.IsNullOrEmpty(sAction))
        //            {
        //                if (sAction == "Adicionar")
        //                {
        //                    ValidaPersistencia(oDBconnection, oItem, sAction);

        //                    try
        //                    {
        //                        this.Adicionar(oDBconnection, oItem, sUsuario);

        //                        oClassMessage.Message = "Novo registro cadastrado com sucesso.";
        //                        oClassMessage.MessageType = sqoClassMessage.MessageTypeEnum.OK;
        //                        oClassMessage.MessageDescription = "";
        //                        oClassMessage.Ok = true;
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        oClassMessage.Message = "Não foi possível cadastrar novo registro!";
        //                        oClassMessage.MessageType = sqoClassMessage.MessageTypeEnum.ALERT;
        //                        oClassMessage.MessageDescription = ex.Message;
        //                        oClassMessage.Ok = false;
        //                    }
        //                }
        //                else if (sAction == "Duplicar")
        //                {
        //                    ValidaPersistencia(oDBconnection, oItem, sAction);

        //                    try
        //                    {
        //                        if (oItem.File.Length == 0)
        //                            oItem.File = GetFile(oDBconnection, oItem.ID);                                

        //                        this.Adicionar(oDBconnection, oItem, sUsuario);

        //                        oClassMessage.Message = "Novo registro cadastrado com sucesso.";
        //                        oClassMessage.MessageType = sqoClassMessage.MessageTypeEnum.OK;
        //                        oClassMessage.MessageDescription = "";
        //                        oClassMessage.Ok = true;
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        oClassMessage.Message = "Não foi possível cadastrar novo registro!";
        //                        oClassMessage.MessageType = sqoClassMessage.MessageTypeEnum.ALERT;
        //                        oClassMessage.MessageDescription = ex.Message;
        //                        oClassMessage.Ok = false;
        //                    }
        //                }
        //                else if (sAction == "Editar")
        //                {
        //                    ValidaPersistencia(oDBconnection, oItem, sAction);

        //                    try
        //                    {
        //                        if(this.Editar(oDBconnection, oItem) == 0)
        //                            throw (new Exception("Registro não encontrado."));

        //                        oClassMessage.Message = "Registro editado com sucesso.";
        //                        oClassMessage.MessageType = sqoClassMessage.MessageTypeEnum.OK;
        //                        oClassMessage.MessageDescription = "";
        //                        oClassMessage.Ok = true;
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        oClassMessage.Message = "Não foi possível editar o registro!";
        //                        oClassMessage.MessageType = sqoClassMessage.MessageTypeEnum.ERROR;
        //                        oClassMessage.MessageDescription = ex.Message;
        //                        oClassMessage.Ok = false;
        //                    }
        //                }
        //                else if (sAction == "Remover")
        //                {
        //                    try
        //                    {
        //                        if(this.Remover(oDBconnection, oItem, sUsuario) == 0)
        //                            throw (new Exception("Registro não encontrado."));

        //                        oClassMessage.Message = "Registro removido com sucesso.";
        //                        oClassMessage.MessageType = sqoClassMessage.MessageTypeEnum.OK;
        //                        oClassMessage.MessageDescription = "";
        //                        oClassMessage.Ok = true;
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        oClassMessage.Message = "Não foi possível remover o registro!";
        //                        oClassMessage.MessageType = sqoClassMessage.MessageTypeEnum.ERROR;
        //                        oClassMessage.MessageDescription = ex.Message;
        //                        oClassMessage.Ok = false;
        //                    }
        //                }
        //                else if (sAction == "DonwloadImagem")
        //                {
        //                    try
        //                    {
        //                        byte[] oImagem = GetFile(oDBconnection, oItem.ID);

        //                        if (oImagem == null || oImagem.Length == 0)
        //                            throw (new Exception("Imagem não encontrada ou não cadastrada."));

        //                        oClassMessage.Dado = Tools.SaveFileToDownload(oImagem, "png");
        //                        oClassMessage.Message = "Download da imagem realizado com sucesso.";
        //                        oClassMessage.MessageType = sqoClassMessage.MessageTypeEnum.OK;
        //                        oClassMessage.MessageDescription = "";
        //                        oClassMessage.Ok = true;
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        oClassMessage.Message = "Não foi possível fazer download da imagem!";
        //                        oClassMessage.MessageType = sqoClassMessage.MessageTypeEnum.ERROR;
        //                        oClassMessage.MessageDescription = ex.Message;
        //                        oClassMessage.Ok = false;
        //                    }
        //                }
        //                else if (sAction == "Sublist")
        //                {
        //                    if (oListaParametrosMovimentacao != null && oListaParametrosMovimentacao.Count >= 2)
        //                    {
        //                        string subAction = oListaParametrosMovimentacao[0].Valor;

        //                        if(subAction == "Add")
        //                        {
        //                            sqoClassPcpDynCriteriaSubItemDados dados = sqoClassBiblioSerDes.DeserializeObject<sqoClassPcpDynCriteriaSubItemDados>(oListaParametrosMovimentacao[1].Valor);

        //                            ValidaSubPersistencia(oDBconnection, oItem, sAction, subAction, dados);

        //                            this.AdicionarSubItem(oDBconnection, oItem.ID, dados.DadosAcao);

        //                            dados.List.Add(new sqoClassPcpDynCriteriaSubItemDadosList() {Index = dados.List.Count, LISTA = dados.DadosAcao.LISTA, TEXTO = dados.DadosAcao.TEXTO, NUMERO = dados.DadosAcao.NUMERO});

        //                            oListaParametrosMovimentacao[1].Valor = sqoClassBiblioSerDes.SerializeObject(dados).Replace("﻿<?xml version=\"1.0\" encoding=\"utf-8\"?>", "");                                     

        //                            oClassMessage.Message = "Registro inserido com sucesso.";
        //                            oClassMessage.MessageType = sqoClassMessage.MessageTypeEnum.OK;
        //                            oClassMessage.MessageDescription = "";
        //                            oClassMessage.Ok = true;
        //                            oClassMessage.Dado = oListaParametrosMovimentacao;
        //                        }
        //                        else if(oListaParametrosMovimentacao[0].Valor == "Remove")
        //                        {
        //                            int index = int.Parse(oListaParametrosMovimentacao[1].Valor);
        //                            sqoClassPcpDynCriteriaSubItemDados dados = sqoClassBiblioSerDes.DeserializeObject<sqoClassPcpDynCriteriaSubItemDados>(oListaParametrosMovimentacao[2].Valor);

        //                            if (this.RemoverSubItem(oDBconnection, oItem.ID, dados.List[index]) == 0)
        //                                throw (new Exception("Registro não encontrado."));

        //                            dados.List.RemoveAll(x => x.Index == index);

        //                            oListaParametrosMovimentacao[2].Valor = sqoClassBiblioSerDes.SerializeObject(dados).Replace("﻿<?xml version=\"1.0\" encoding=\"utf-8\"?>", "");

        //                            oClassMessage.Message = "Registro removidos com sucesso.";
        //                            oClassMessage.MessageType = sqoClassMessage.MessageTypeEnum.OK;
        //                            oClassMessage.MessageDescription = "";
        //                            oClassMessage.Ok = true;
        //                            oClassMessage.Dado = oListaParametrosMovimentacao;
        //                        }
        //                        //else if (oListaParametrosMovimentacao[0].Valor == "Edit")
        //                        //{
        //                        //
        //                        //}
        //                        else
        //                        {
        //                            oClassMessage.Message = "Ação não suportada!";
        //                            oClassMessage.MessageType = sqoClassMessage.MessageTypeEnum.ERROR;
        //                            oClassMessage.MessageDescription = "";
        //                            oClassMessage.Ok = false;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        oClassMessage.Message = "Erro ao realizar ação!";
        //        oClassMessage.MessageType = sqoClassMessage.MessageTypeEnum.ERROR;
        //        oClassMessage.MessageDescription = ex.Message;
        //        oClassMessage.Ok = false;
        //    }
        //    finally
        //    {
        //        if (oDBconnection != null)
        //        {
        //            if (oDBconnection.State == ConnectionState.Open)
        //                oDBconnection.Close();
        //            oDBconnection.Dispose();
        //        }
        //    }

        //    return oClassMessage;
        //}
        #endregion





        private void Inserir(sqoClassPcpDynCriteriaItem oPersistencia)
        {

        }

        private void Editar(sqoClassPcpDynCriteriaItem oPersistencia)
        {

        }
        
        private void Duplicar(sqoClassPcpDynCriteriaItem oPersistencia)
        {

        }
        
        private void Excluir(sqoClassPcpDynCriteriaItem oPersistencia)
        {

        }

        public void GetParametrosListagem(List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao)
        {
            foreach (sqoClassParametrosEstrutura oClassParametrosEstrutura in oListaParametrosMovimentacao)
            {
                switch (oClassParametrosEstrutura.Campo)
                {
                    case "CRT_Texto": this.sParamListagem_CRT_Texto = oClassParametrosEstrutura.Valor;
                    break;
                }
            }
        }
    }
}
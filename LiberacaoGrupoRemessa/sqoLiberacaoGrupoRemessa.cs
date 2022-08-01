using AI1627Common20.TemplateDebugging;
using AI1627CommonInterface;
using AI1627CommonInterface.LESStatus;
using sequor.expedicao.client;
using sequor.expedicao.client.DataModel;
using sqoClassLibraryAI0502Biblio;
using sqoClassLibraryAI0502Message;
using sqoClassLibraryAI0502VariaveisSistema;
using sqoClassLibraryAI1151FilaProducao.Estrutura;
using sqoClassLibraryAI1151FilaProducao.Process;
using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.Serialization;
using System.Linq;

namespace TemplateStara.Expedicao.sqoLiberacaoGrupoRemessa
{
    [TemplateDebug("sqoLiberacaoGrupoRemessa")]
    public class sqoLiberacaoGrupoRemessa : IProcessMovimentacao
    {
        private List<sqoClassLESEXPRemessaPersistence> oListSqoClassLESEXPRemessaPersistence;
        private sqoClassSetMessageDefaults oClassSetMessageDefaults;
        private GrupoRemessa oGrupoRemessa;
        private ExpedicaoService oExpedicaoService = new ExpedicaoService();
        private HandleStatusRequest oHandleStatusRequest = new HandleStatusRequest();
        private StatusFlagRequest oStatusFlagRequest = new StatusFlagRequest();

        enum Action { Invalid = -1, Release }
        private Action currentAction = Action.Invalid;
        private string sUsuario = string.Empty;
        private string sMensagemErro = string.Empty;

        public sqoClassMessage Executar(string sAction, string sXmlDados, string sXmlType, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao, List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sNivel, string sUsuario, object oObjAux)
        {

            this.Init(sXmlDados, sUsuario, sAction);

            using (sqoClassDbConnection oDBConnection = new sqoClassDbConnection())
            {
                switch (currentAction)
                {
                    case Action.Release:
                        {
                            if (this.Validations())
                                this.ProcessBussinessLogic(oDBConnection, oListSqoClassLESEXPRemessaPersistence);

                            break;
                        }
                }
            }

            return oClassSetMessageDefaults.Message;
        }

        private void Init(string sXmlDados, string sUsuario, string sAction)
        {
            this.oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());

            this.oGrupoRemessa = sqoClassBiblioSerDes.DeserializeObject<GrupoRemessa>(sXmlDados);

            this.sUsuario = sUsuario;

            Enum.TryParse(sAction, out this.currentAction);
        }

        private bool Validations()
        {
            bool bReturn = true;

            this.ValidaItemSelecionado();

            this.GetLESExpRemessa();

            if (!string.IsNullOrEmpty(sMensagemErro))
            {
                bReturn = false;
                string sMessage = "Falha na validação de dados";
                this.oClassSetMessageDefaults.Message.Ok = false;
                this.oClassSetMessageDefaults.Message.Message = sMessage;
                this.oClassSetMessageDefaults.Message.MessageDescription = sMensagemErro;
                this.oClassSetMessageDefaults.Message.MessageType = sqoClassMessage.MessageTypeEnum.ERROR;
            }

            return (bReturn);
        }

        private string ValidaItemSelecionado()
        {
            if (this.oGrupoRemessa == null)

            {
                sMensagemErro = "Necessário selecionar um item da lista!";
            }

            return sMensagemErro;
        }

        private List<sqoClassLESEXPRemessaPersistence> GetLESExpRemessa()
        {
            string sWhere = "";

            sWhere = this.MontarSWhere(sWhere);

            oListSqoClassLESEXPRemessaPersistence = sqoClassLESEXPRemessaControlerDB.GetLESExpRemessa(sWhere, null, null, EXP_REMESSA_GET_TYPE.ITEM);

            if (oListSqoClassLESEXPRemessaPersistence.Count == 0)
            {
                sMensagemErro += "Não existe remessa para o grupo selecionado!";
            }

            return oListSqoClassLESEXPRemessaPersistence;

        }

        private bool VerificarStatusRemessa(long Id)
        {
            bool Result = false;

            int StatusLes = 20;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@STATUS_LES", StatusLes)
                    .Add("@ID", Id)
                    ;

                string sQuery = @"SELECT 
	                                 ID
	                                ,CODIGO_REMESSA
	                                ,STATUS_LES
                                  FROM
	                                WSQOLEXPREMESSA
                                  WHERE
	                                STATUS_LES <> ?
                                  AND
	                                ID = ?";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.GetResultado();

                    if (oResult != null)
                        Result = true;
                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }

            return Result;
        }

        private void ProcessBussinessLogic(sqoClassDbConnection oDBConnection, List<sqoClassLESEXPRemessaPersistence> oListSqoClassLESEXPRemessaPersistence)
        {
            try
            {
                this.Executar(oGrupoRemessa, oListSqoClassLESEXPRemessaPersistence, oDBConnection);

                oClassSetMessageDefaults.SetarOk();
                oClassSetMessageDefaults.Message.MessageType = sqoClassMessage.MessageTypeEnum.OK;
                oClassSetMessageDefaults.Message.Ok = true;
            }
            catch (Exception ex)
            {
                string sMessage = "Falha na validação de dados";
                this.oClassSetMessageDefaults.Message.Ok = false;
                this.oClassSetMessageDefaults.Message.Message = sMessage + ex.Message;
                this.oClassSetMessageDefaults.Message.MessageDescription = sMensagemErro + ex.InnerException;
                this.oClassSetMessageDefaults.Message.MessageType = sqoClassMessage.MessageTypeEnum.ERROR;
            }
        }

        private void Executar(GrupoRemessa oGrupoRemessa, List<sqoClassLESEXPRemessaPersistence> oListSqoClassLESEXPRemessaPersistence, sqoClassDbConnection oDBConnection)
        {
            foreach (var oListaRemessas in oListSqoClassLESEXPRemessaPersistence)
            {

                try
                {
                    oDBConnection.BeginTransaction();

                    foreach (var item in oListaRemessas.LiClassLESEXPRemessaItensPersistence)
                    {

                        if (UpdateStatusRemessaItem(item.Status))
                        {
                            sqoClassLESEXPRemessaItensControlerDB.ExecuteWsqolUpdateExpRemessaItens(
                            nId: item.Id,
                            nStatus: STATUS_EXP_REMESSA_ITEM.PLANEJADO);
                        }
                    }

                    oDBConnection.Commit();
                }
                catch (Exception ex)
                {
                    oDBConnection.Rollback();

                    sMensagemErro += ex.Message;
                }


                oStatusFlagRequest.Ids = new List<long>();

                oStatusFlagRequest.Ids.AddRange(oListaRemessas.LiClassLESEXPRemessaItensPersistence.Select(x => x.Id).ToList());

                oStatusFlagRequest.Status = (int)STATUS_EXP_REMESSA_ITEM.PLANEJADO;

                oStatusFlagRequest.Table = "WSQOLEXPREMESSAITENS";

                oStatusFlagRequest.SetParent = true;

                this.oExpedicaoService.AddStatusFlag(oStatusFlagRequest);

                var sReturnRem = this.AtualizarRemessa(oListaRemessas.Id);

                if (!string.IsNullOrEmpty(sReturnRem))
                {
                    sMensagemErro += sReturnRem;
                }

            }
        }

        private string AtualizarRemessa(long Id)
        {
            string bReturn = "";

            string TableName = "WSQOLEXPREMESSA";

            bool TrueParam = true;

            oHandleStatusRequest.Table = TableName;

            oHandleStatusRequest.Ids = new List<long>() { Id };

            oHandleStatusRequest.ObserveStructure = TrueParam;

            var result = oExpedicaoService.HandlerStatus(oHandleStatusRequest);

            if (!result.Ok)
            {
                bReturn = "Não foi possível atualizar o status da remessa! Detalhes: " + Environment.NewLine + result.Message;
            }

            return bReturn;
        }

        private string MontarSWhere(string sWhere)
        {
            sWhere = "WHERE ID_GRUPO_REMESSA = " + oGrupoRemessa.Id.ToString();

            return sWhere;
        }

        private bool UpdateStatusRemessaItem(STATUS_EXP_REMESSA_ITEM status)
        {
            if (status == STATUS_EXP_REMESSA_ITEM.SEPARADO_PARCIAL
                || status == STATUS_EXP_REMESSA_ITEM.SEPARADO
                || status == STATUS_EXP_REMESSA_ITEM.EM_TRANSITO
                || status == STATUS_EXP_REMESSA_ITEM.ENTREGUE_PARCIAL
                || status == STATUS_EXP_REMESSA_ITEM.ENTREGUE
                || status == STATUS_EXP_REMESSA_ITEM.CARREGADO_PARCIAL
                || status == STATUS_EXP_REMESSA_ITEM.CARREGADO
                || status == STATUS_EXP_REMESSA_ITEM.FATURADO)
                return false;

            return true;
        }
    }

    [XmlRoot("ItemFilaProducao")]
    public class GrupoRemessa
    {
        [XmlElement("ID")]
        public long Id { get; set; }

        [XmlElement("GRUPO")]
        public string Grupo { get; set; }

        [XmlElement("STATUS")]
        public int Status { get; set; }
    }

}
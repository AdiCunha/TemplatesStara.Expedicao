using AI1627Common20.TemplateDebugging;
using AI1627CommonInterface;
using AI1627CommonInterface.LESStatus;
using sequor.expedicao.client;
using sequor.expedicao.client.DataModel;
using sequor_expedicao;
using sqoClassLibraryAI0502Biblio;
using sqoClassLibraryAI0502Message;
using sqoClassLibraryAI0502VariaveisSistema;
using sqoClassLibraryAI1151FilaProducao.Estrutura;
using sqoClassLibraryAI1151FilaProducao.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace TemplateStara.Expedicao.LiberacaoGrupoRemessa
{
    [TemplateDebug("sqoLiberacaoPlanejamentoGrupoRemessa")]
    class sqoLiberacaoPlanejamentoGrupoRemessa : IProcessMovimentacao
    {
        private List<sqoClassLESEXPRemessaPersistence> oListSqoClassLESEXPRemessaPersistence;
        private sqoClassSetMessageDefaults oClassSetMessageDefaults;
        private PlanejamentoGrupoRemessa oPlanejamentoGrupoRemessa;
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

            this.oPlanejamentoGrupoRemessa = sqoClassBiblioSerDes.DeserializeObject<PlanejamentoGrupoRemessa>(sXmlDados);

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
            if (this.oPlanejamentoGrupoRemessa == null)

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

        private void ProcessBussinessLogic(sqoClassDbConnection oDBConnection, List<sqoClassLESEXPRemessaPersistence> oListSqoClassLESEXPRemessaPersistence)
        {
            try
            {


                this.Execute(oPlanejamentoGrupoRemessa, oListSqoClassLESEXPRemessaPersistence, oDBConnection);



                oClassSetMessageDefaults.SetarOk();
                oClassSetMessageDefaults.Message.MessageType = sqoClassMessage.MessageTypeEnum.OK;
                oClassSetMessageDefaults.Message.Ok = true;
            }
            catch (Exception ex)
            {
                oDBConnection.Rollback();
                string sMessage = "Falha na validação de dados";
                this.oClassSetMessageDefaults.Message.Ok = false;
                this.oClassSetMessageDefaults.Message.Message = sMessage;
                this.oClassSetMessageDefaults.Message.MessageDescription = sMensagemErro += ex.Message;
                this.oClassSetMessageDefaults.Message.MessageType = sqoClassMessage.MessageTypeEnum.ERROR;
            }
        }

        private void Execute(PlanejamentoGrupoRemessa oPlanejamentoGrupoRemessa, List<sqoClassLESEXPRemessaPersistence> oListSqoClassLESEXPRemessaPersistence, sqoClassDbConnection oDBConnection)
        {

            foreach (var oListaRemessas in oListSqoClassLESEXPRemessaPersistence)
            {
                oDBConnection.BeginTransaction();

                foreach (var item in oListaRemessas.LiClassLESEXPRemessaItensPersistence)
                {
                    try
                    {

                        sqoClassLESEXPRemessaItensControlerDB.ExecuteWsqolUpdateExpRemessaItens(
                              nId: item.Id,
                              nStatus: STATUS_EXP_REMESSA_ITEM.PLANEJAMENTO_LIBERADO);


                    }
                    catch (Exception ex)
                    {
                        oDBConnection.Rollback();

                        sMensagemErro += ex.Message;

                    }
                }

                oDBConnection.Commit();

                oStatusFlagRequest.Ids = new List<long>();

                oStatusFlagRequest.Ids.AddRange(oListaRemessas.LiClassLESEXPRemessaItensPersistence.Select(x => x.Id).ToList());

                oStatusFlagRequest.Status = (int)STATUS_EXP_REMESSA_ITEM.PLANEJAMENTO_LIBERADO;

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
            sWhere = "WHERE ID_GRUPO_REMESSA = " + oPlanejamentoGrupoRemessa.Id.ToString();

            return sWhere;
        }
    }

    [XmlRoot("ItemFilaProducao")]
    public class PlanejamentoGrupoRemessa
    {
        [XmlElement("ID")]
        public long Id { get; set; }

        [XmlElement("GRUPO")]
        public string Grupo { get; set; }

        [XmlElement("STATUS")]
        public int Status { get; set; }
    }

}
using AI1627Common20.TemplateDebugging;
using sequor.expedicao.client;
using sequor.expedicao.client.DataModel;
using sqoClassLibraryAI0502Biblio;
using sqoClassLibraryAI0502Message;
using sqoClassLibraryAI0502VariaveisSistema;
using sqoClassLibraryAI1151FilaProducao;
using sqoClassLibraryAI1151FilaProducao.Estrutura;
using sqoClassLibraryAI1151FilaProducao.Process;
using System;
using System.Collections.Generic;
using TemplatesStara.CommonStara;
using TemplateStara.Expedicao.AlterarSituacaoItemRemessa.Dao;
using TemplateStara.Expedicao.AlterarSituacaoItemRemessa.DataModel;

namespace TemplateStara.Expedicao.AlterarSituacaoItemRemessa.Business
{
    [TemplateDebug("AlterarSituacaoItemRemessaBusiness")]
    public class AlterarSituacaoItemRemessaBusiness : IProcessMovimentacao
    {
        private sqoClassSetMessageDefaults oClassSetMessageDefaults;
        private SituacaoRemessaItem oSituacaoRemessaItem;
        private ExpedicaoService oExpedicaoService;
        private HandleStatusRequest oHandleStatusRequest;
        sqoClassDbConnection oDBConnection = new sqoClassDbConnection();
        private string sUsuario;
        private string sDescription = string.Empty;
        private string sMessage = "Falha na validação de dados";
        private string sMessageErro = string.Empty;
        private int IdHist = 0;
        enum Action { Invalid = -1, Execute }
        private Action currentAction = Action.Invalid;

        public sqoClassMessage Executar(string sAction
                                        , string sXmlDados
                                        , string sXmlType
                                        , List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao
                                        , List<sqoClassParametrosEstrutura> oListaParametrosListagem
                                        , string sNivel
                                        , string sUsuario
                                        , object oObjAux)
        {
            this.Init(oListaParametrosListagem, sXmlDados, sUsuario, sAction);

            switch (currentAction)
            {
                case Action.Execute:
                    {
                        this.ProcessBusinessLogic();

                        break;
                    }
            }

            return oClassSetMessageDefaults.Message;
        }

        private void Init(List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sXmlDados, string sUsuario, string sAction)
        {
            this.oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());

            this.oSituacaoRemessaItem = sqoClassBiblioSerDes.DeserializeObject<SituacaoRemessaItem>(sXmlDados);

            this.sUsuario = sUsuario;

            Enum.TryParse(sAction, out this.currentAction);

        }

        private void ProcessBusinessLogic()
        {

            AlterarSituacaoItemRemessaDao OAlterarSituacaoItemRemessaDao = new AlterarSituacaoItemRemessaDao();

            string SituacaoE = "E";

            if (oSituacaoRemessaItem.Situacao != SituacaoE)
            {
                oDBConnection.BeginTransaction();

                OAlterarSituacaoItemRemessaDao.SetSituacaoItemRemessa(oSituacaoRemessaItem.Id);

                if (this.AtualizarRemessa())
                {
                    this.AlterarHistorico(OAlterarSituacaoItemRemessaDao);

                    this.oClassSetMessageDefaults.SetarOk();

                    oDBConnection.Commit();
                }

                else
                {
                    ValidateMessage();

                    oDBConnection.Rollback();
                }
            }

            else
            {
                this.sDescription = "Erro: Item da Remessa já está com situação " + SituacaoE + " ! " + Environment.NewLine;

                this.ValidateMessage();
            }

        }

        private bool AtualizarRemessa()
        {
            bool bReturn = true;

            string TableName = "WSQOLEXPREMESSA";

            bool TrueParam = true;

            oHandleStatusRequest = new HandleStatusRequest();

            this.oExpedicaoService = new ExpedicaoService();

            oHandleStatusRequest.Table = TableName;

            oHandleStatusRequest.Ids = new List<long>() { oSituacaoRemessaItem.IdRemessa };

            oHandleStatusRequest.ObserveStructure = TrueParam;

            var result = oExpedicaoService.HandlerStatus(oHandleStatusRequest);

            return bReturn;

        }

        private void AlterarHistorico(AlterarSituacaoItemRemessaDao OAlterarSituacaoItemRemessaDao)
        {
            IdHist = OAlterarSituacaoItemRemessaDao.GetIdHist(oSituacaoRemessaItem.Id);

            OAlterarSituacaoItemRemessaDao.SetSituacaoItemRemessaHist(sUsuario, oSituacaoRemessaItem.Observacao, IdHist);
        }

        private void ValidateMessage()
        {
            if (!string.IsNullOrEmpty(sDescription))
            {
                string sMessageBody = Environment.NewLine + sDescription;

                CommonStara.MessageBox(false, this.sMessage, sMessageBody, sqoClassMessage.MessageTypeEnum.ERROR, oClassSetMessageDefaults);

                throw new sqoClassMessageUserException(oClassSetMessageDefaults.Message);
            }
        }
    }
}

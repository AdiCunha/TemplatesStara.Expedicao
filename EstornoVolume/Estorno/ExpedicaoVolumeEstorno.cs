using AI1627Common20.TemplateDebugging;
using AI1627Common40.Configuracoes.Core;
using AI1627Common40.Configuracoes.DataModel;
using Sequor.LES.Expedition.Api;
using Sequor.LES.Expedition.Client;
using Sequor.LES.Expedition.Model;
using sqoClassLibraryAI0502Biblio;
using sqoClassLibraryAI0502Message;
using sqoClassLibraryAI1151FilaProducao;
using sqoClassLibraryAI1151FilaProducao.Estrutura;
using sqoClassLibraryAI1151FilaProducao.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace TemplatesStara.Expedicao.EstornoVolume.Estorno
{
    [TemplateDebug("ExpedicaoVolumeEstorno")]
    public class ExpedicaoVolumeEstorno : IProcessMovimentacao
    {
        private sqoClassSetMessageDefaults _sqoClassSetMessageDefaults;

        public sqoClassMessage Executar(string sAction, string sXmlDados, string sXmlType, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao, List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sNivel, string sUsuario, object oObjAux)
        {
            try
            {
                _sqoClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());

                _sqoClassSetMessageDefaults.SetarOk();

                var estornarVolumeFormModel = sqoClassBiblioSerDes.DeserializeObject<EstornarVolumeFormModel>(sXmlDados);

                IActionResolver actionResolver = new ActionResolver(Actions.List);

                IAction action = actionResolver.Resolve(sAction);

                action.Execute(estornarVolumeFormModel, sUsuario);

                _sqoClassSetMessageDefaults.Message.Dado = oListaParametrosMovimentacao;

                return _sqoClassSetMessageDefaults.Message;
            }
            catch (Exception ex)
            {
                sqoClassMessageUserException oClassMessageUserException =
                                    new sqoClassMessageUserException("Erro ao executar:" + Environment.NewLine + ex.Message, ex.InnerException);

                throw oClassMessageUserException;
            }
            
        }
    }

    public enum ActionsEnum
    {
        Reverse
    }

    public static class Actions
    {
        public static readonly IDictionary<ActionsEnum, IAction> List;

        static Actions()
        {
            var configurationList = new ContextConfiguration().GetContextConfiguration();

            var externalSupplyApiClient = new ExpeditionApiClient(configurationList.Expedition);

            var actions = new Dictionary<ActionsEnum, IAction>();
            actions.Add(ActionsEnum.Reverse, new ReverseAction(externalSupplyApiClient));

            List = actions;
        }
    }

    public interface IAction
    {
        void Execute(EstornarVolumeFormModel estornarVolumeFormModel, string user);
    }

    public class ReverseAction : IAction
    {
        private readonly IVolumeApi _volumeApi;

        public ReverseAction(ExpeditionApiClient expeditionApiClient)
        {
            _volumeApi = new VolumeApi(expeditionApiClient);
        }

        public void Execute(EstornarVolumeFormModel estornarVolumeFormModel, string user)
        {
            try
            {
                var command = new ReverseVolumeCommand
                {
                    Volume = estornarVolumeFormModel.Volume,
                    User = user
                };

                _volumeApi.ReverseVolume(command);
            }
            catch (ApiException ex)
            {
                if (ex.ErrorCode == 400)
                {
                    var notifications = ex.GetNotifications();

                    var error = string.Join(Environment.NewLine, notifications.Select(k => k.Key + ": " + k.Message).ToArray());

                    throw new Exception(error);
                }

                throw new Exception(ex.ErrorContent.ToString());
            }
           
        }
    }

    public interface IActionResolver
    {
        IAction Resolve(string name);
    }

    public class ActionResolver : IActionResolver
    {
        private readonly IDictionary<ActionsEnum, IAction> actions;

        public ActionResolver(IDictionary<ActionsEnum, IAction> actions)
        {
            this.actions = actions;
        }

        public IAction Resolve(string action)
        {
            try
            {
                var actionEnum = (ActionsEnum)Enum.Parse(typeof(ActionsEnum), action);
                return actions[actionEnum];
            }
            catch
            {
                throw new ArgumentException("Ação inválida!", action);
            }
        }
    }

    internal class ContextConfiguration
    {
        private readonly sqoConfiguracaoBusiness configurationBusiness;

        public ConfigurationList GetContextConfiguration()
        {
            return configurationBusiness.ObterConfiguracoesBinding<ConfigurationList>();
        }

        public ContextConfiguration()
        {
            this.configurationBusiness = sqoConfiguracaoContextoBusiness.ObterContextoFromBinding(typeof(ConfigurationList));
        }
    }

    [ConfiguracaoBinding(Contexto = "SERVICES_EXPEDITION")]
    internal class ConfigurationList : sqoConfiguracoesBindingBase
    {
        public string Expedition { get; set; }
    }

    [XmlRoot("ItemFilaProducao")]
    public class EstornarVolumeFormModel
    {
        [XmlElement("CODIGO_REMESSA")]
        public string Remessa { get; set; }

        [XmlElement("CODIGO_RASTREABILIDADE")]
        public string Volume { get; set; }

        [XmlElement("TIPO_VOLUME")]
        public int TipoVolume { get; set; }

        [XmlElement("STATUS")]
        public int Status { get; set; }
    }
}

using sqoClassLibraryAI0502Biblio;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace TemplateStara.Expedicao.TransicaoStatus.DataModel
{
    [XmlRoot("ItemFilaProducao")]
    public class StatusTransitions
    {
        [XmlElement("MODULE")]
        public string Module { get; set; }

    }

    [XmlRoot("RootDetails")]
    public class sqoClassDetailsStatus
    {
        public List<sqoClassItemDetailBaseStatus> oDetails;

        [XmlArray("Details")]
        [XmlArrayItem("Detail", typeof(sqoClassItemDetailBaseStatus))]
        public List<sqoClassItemDetailBaseStatus> Details
        {
            get { return oDetails; }
            set { oDetails = value; }
        }
    }

    [XmlRoot("Detail")]
    [XmlInclude(typeof(sqoClassItemDetailItemValorStatus))]
    [XmlInclude(typeof(sqoClassStatusTransitions))]
    public abstract class sqoClassItemDetailBaseStatus
    {
    }

    public class sqoClassItemDetailItemValorStatus : sqoClassItemDetailBaseStatus
    {
        public string sItem;
        public string sValor;

        [XmlAttribute]
        public string Item
        {
            get { return sItem; }
            set { sItem = value; }
        }

        [XmlAttribute]
        public string Valor
        {
            get { return sValor; }
            set { sValor = value; }
        }
    }

    [AutoPersistencia]
    public class sqoClassStatusTransitions : sqoClassItemDetailBaseStatus
    {
        public string CurrentStatus { get; set; }

        public string NextStatus { get; set; }

        public bool Permite { get; set; }

        public string Mensagem { get; set; }

        public string Modulo { get; set; }

    }

    [AutoPersistencia]
    public class StatusTransitionsValues
    {
        public string CurrentStatus { get; set; }

        public string NextStatus { get; set; }

        public string Permite { get; set; }

        public string Mensagem { get; set; }

        public string Modulo { get; set; }
    }

    public class TRANSICAO_STATUS_FIELD
    {
        public const string CURRENT_STATUS = "CurrentStatus";
        public const string NEXT_STATUS = "NextStatus";
        public const string PERMITE = "Permite";
        public const string MENSAGEM = "Mensagem";
        public const string MODULO = "Modulo";
    }

    public enum MODULO
    {
        Remessa, Item, Grupo, Volume, Invalid = -1
    }

    [XmlRoot("ItemFilaProducao")]
    public class StatusTransitionsInsert
    {
        [XmlElement("CURRENT_STATUS")]
        public int CurrentStatus { get; set; }

        [XmlElement("NEXT_STATUS")]
        public int NextStatus { get; set; }

        [XmlElement("PERMITE")]
        public bool Permite { get; set; }

        [XmlElement("MENSAGEM")]
        public string Mensagem { get; set; }

        [XmlElement("MODULO")]
        public string Modulo { get; set; }
    }
}

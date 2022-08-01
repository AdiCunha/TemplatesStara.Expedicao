using System.Collections.Generic;
using System.Xml.Serialization;

namespace TemplateStara.Expedicao.AlterarSituacaoItemRemessa.DataModel
{
    [XmlRoot("ItemFilaProducao")]
    public class SituacaoRemessaItem
    {
        [XmlElement("ID")]
        public int Id { get; set; }

        [XmlElement("ID_REMESSA")]
        public int IdRemessa { get; set; }

        [XmlElement("SITUACAO")]
        public string Situacao { get; set; }

        [XmlElement("OBSERVACAO")]
        public string Observacao { get; set; }

    }

    public class SendRemessaData
    {
        public string Table { get; set; }

        public List<long> Ids { get; set; }

        public bool ObserveStructure { get; set; }
    }
}

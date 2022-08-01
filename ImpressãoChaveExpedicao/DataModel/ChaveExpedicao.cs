using System.Xml.Serialization;

namespace TemplateStara.Expedicao.ImpressãoChaveExpedicao.DataModel
{
    [XmlRoot("ItemFilaProducao")]
    public class ChaveExpedicao
    {
        [XmlElement("ID")]
        public long Id { get; set; }

        [XmlElement("CHAVE")]
        public string Chave { get; set; }

        [XmlElement("DESCRICAO")]
        public string Descricao { get; set; }

        [XmlElement("OBSERVACAO")]
        public string Observao { get; set; }

        [XmlElement("ID_PRINTER")]
        public long IdPrinter { get; set; }

        [XmlElement("USUARIO")]
        public string Usuario { get; set; }
    }
}

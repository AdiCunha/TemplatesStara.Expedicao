using sqoClassLibraryAI0502Biblio;
using System.Xml.Serialization;

namespace TemplateStara.Expedicao.GeracaoNumeroSerieComponente.DataModel
{

    [XmlRoot("ItemFilaProducao")]
    public class NumeroSerieComponente
    {
        [XmlElement("ID_GERACAO")]
        public int IdGeracao { get; set; }

        [XmlElement("NUMERO_SERIE")]
        public string NumeroSerie { get; set; }

        [XmlElement("DOC_REFERENCIA")]
        public string DocReferencia { get; set; }

        [XmlElement("MATERIAL")]
        public string Material { get; set; }

        [XmlElement("OBSERVACAO")]
        public string Observacao { get; set; }

    }
}
using System.Xml.Serialization;

namespace TemplateStara.Expedicao.GeracaoNumeroSerieComponente.DataModel
{
    [XmlRoot("ItemFilaProducao")]
    public class NumeroSerieGeracao
    {
        [XmlElement("IDPRINTER")]
        public int IdPrinter { get; set; }

        [XmlElement("MATERIAL")]
        public string Material { get; set; }

        [XmlElement("DESCRICAOMATERIAL")]
        public string DescricaoMaterial { get; set; }

        [XmlElement("DOC_REFERENCIA")]
        public string DocReferencia { get; set; }

        [XmlElement("USUARIO")]
        public string Usuario { get; set; }

        [XmlElement("DATAGERACAO")]
        public string DateGeracao { get; set; }

        [XmlElement("NUMERO_SERIE")]
        public string NrSerie {get; set; }

        [XmlElement("DATAIMPRECAO")]
        public string DataImpressao { get; set; }

    }
}

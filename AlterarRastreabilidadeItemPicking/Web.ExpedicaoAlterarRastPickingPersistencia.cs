using System;
using System.Xml.Serialization;

namespace TelaDinamica.Expedicao
{
    /// <summary>
    /// Classe para instanciar o modelo do criteria da tela.
    /// </summary>
    [XmlRoot("ItemFilaProducao")]
    public class ExpedicaoAlterarRastreabilidadePickingPersistencia
    {
        [XmlElement("ID")]
        public long Id { get; set; }

        [XmlElement("ID_ITEM")]
        public long IdItem { get; set; }

        [XmlElement("CHAVE")]
        public String Chave { get; set; }

        [XmlElement("PICKING")]
        public String Picking { get; set; }

        [XmlElement("DATA_PROCESSO")]
        public String DataProcesso { get; set; }

        [XmlElement("STATUS")]
        public int Status { get; set; }

        [XmlElement("MATERIAL")]
        public String Material { get; set; }

        [XmlElement("DESCRICAO")]
        public String Descricao { get; set; }

        [XmlElement("CODIGO_RASTREABILIDADE")]
        public String CodigoRastreabilidade { get; set; }

        [XmlElement("CODIGO_RASTREABILIDADE_ITEM")]
        public String CodigoRastreabilidadeItem { get; set; }

        [XmlElement("ID_ITEM_DOC")]
        public String IdItemDoc { get; set; }

    }

}
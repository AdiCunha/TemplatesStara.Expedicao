using System.Xml.Serialization;

namespace TemplateStara.Expedicao.CadastroComponente.DataModel
{
    [XmlRoot("ItemFilaProducao")]
    public class CadastroComponente
    {
        [XmlElement("ID")]
        public long Id { get; set; }

        [XmlElement("MATERIAL_INSERT")]
        public string MaterialInsert { get; set; }

        [XmlElement("MATERIAL")]
        public string Material { get; set; }

        [XmlElement("DESCRICAO_COMPONENTE_INSERT")]
        public string DescricaoComponenteInsert { get; set; }

        [XmlElement("DESCRICAO_COMPONENTE")]
        public string DescricaoComponente { get; set; }

        [XmlElement("TIPO_INSERT")]
        public int TipoInsert { get; set; }

        [XmlElement("TIPO")]
        public int Tipo { get; set; }

        [XmlElement("ATIVO")]
        public bool Ativo { get; set; }

        [XmlElement("GRUPO")]
        public string Grupo { get; set; }

        [XmlElement("OBSERVACAO")]
        public string Observacao { get; set; }
    }
}

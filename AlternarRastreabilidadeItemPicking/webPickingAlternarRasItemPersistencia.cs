using System;
using System.Xml.Serialization;

namespace TelaDinamica.Expedicao
{
    /// <summary>
    /// Classe para instanciar o modelo do criteria da tela.
    /// </summary>
    [XmlRoot("ItemFilaProducao")]
    public class webPickingAlternarRasItemPersistencia
    {
        [XmlElement("ID")]
        public long Id { get; set; }

        [XmlElement("ID_ITEM")]
        public long IdItem { get; set; }

        [XmlElement("ID_ITEM_DOC")]
        public String IdItemDoc { get; set; }

        [XmlElement("CHAVE")]
        public String Chave { get; set; }

        [XmlElement("REMESSA")]
        public String Remessa { get; set; }

        [XmlElement("DATA_PROCESSO")]
        public String DataProcesso { get; set; }

        [XmlElement("QUANTIDADE")]
        public Double Quantidade { get; set; }

        [XmlElement("MATERIAL")]
        public String Material { get; set; }

        [XmlElement("ITEM_REMESSA")]
        public String ItemRemessa { get; set; }

        [XmlElement("REMESSA_UPDATE")]
        public String RemessaUpdate { get; set; }

        [XmlElement("ID_ITEM_DOC_UPDATE")]
        public String IdItemDocUpdate { get; set; }

    }

}
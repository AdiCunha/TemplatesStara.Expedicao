using System;
using System.Xml.Serialization;

namespace TemplateStara.Expedicao.ReimpressaoAgrupamentoVolumes.DataModel
{

    [XmlRoot("ItemFilaProducao")]
    public class VolumeExpedicao
    {
        [XmlElement("AGRUPAMENTO_LOGISTICO")]
        public Guid AgrupamentoLogistico { get; set; }

        [XmlElement("QTD_VOLUME")]
        public string QtdVolume { get; set; }

        [XmlElement("GRUPO_REMESSA")]
        public string GrupoRemessa { get; set; }
    }
}
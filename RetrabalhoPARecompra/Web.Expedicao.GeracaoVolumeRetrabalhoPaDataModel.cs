using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TelasDinamicas.Expedicao
{
    /// <summary>
    /// Classe para instanciar o modelo do criteria da tela.
    /// </summary>
    [XmlRoot("ItemFilaProducao")]
    public class GeracaoVolumeRetrabalho
    {
        [XmlElement("CODIGO_PAI")]
        public String CodigoPai { get; set; }

        [XmlElement("DESCRICAO_PAI")]
        public String DescricaoPai { get; set; }

        [XmlElement("CODIGO_VOLUME")]
        public String CodigoVolume { get; set; }

        [XmlElement("DESCRICAO_VOLUME")]
        public String DescricaoVolume { get; set; }

        [XmlElement("STATUS_VOLUME")]
        public Decimal StatusVolume { get; set; }

        [XmlElement("DESCRICAO_STATUS_VOLUME")]
        public String DescricaoStatusVolume { get; set; }

        [XmlElement("TIPO_EXPEDICAO")]
        public String TipoVolume { get; set; }

        [XmlElement("ORDEM_PRODUCAO")]
        public String NrSerie { get; set; }

        [XmlElement("QUANTIDADE_ESTOQUE")]
        public Decimal QuantidadeEstoque { get; set; }

        [XmlElement("CODIGO_RASTREABILIDADE")]
        public String CodigoRastreabilidade { get; set; }

        [XmlElement("STATUS_ESTOQUE")]
        public int StatusEstoque { get; set; }

        [XmlElement("DESCRICAO_STATUS_ESTOQUE")]
        public String DescricaoStatusEstoque { get; set; }

        [XmlElement("QTD_ETIQUETAS")]
        public int QtdEtiquetas { get; set; }

        [XmlElement("PRINT")]
        public bool Print { get; set; }

        [XmlElement("ORDEM_PRODUCAO_RETRABALHO")]
        public String OrdemProducao { get; set; }

        [XmlElement("OPERACAO")]
        public String Operacao { get; set; }

        [XmlElement("CONTADOR")]
        public int Contador { get; set; }
    }
}

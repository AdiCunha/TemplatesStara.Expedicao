using System;
using System.Xml.Serialization;

namespace TemplateStara.Expedicao.GeracaoVolume
{
    /// <summary>
    /// Classe para instanciar o modelo do criteria da tela.
    /// </summary>
    [XmlRoot("ItemFilaProducao")]
    public class sqoClassGeracaoVolume
    {
        [XmlElement("IMPRIMIR_ETIQUETA")]
        public Boolean ImprimirEtiqueta { get; set; }

        [XmlElement("CODIGO_PAI")]
        public String CodigoPai { get; set; }

        [XmlElement("DESCRICAO_PAI")]
        public String DescricaoPai { get; set; }

        [XmlElement("CODIGO_VOLUME")]
        public String CodigoVolume { get; set; }

        [XmlElement("DESCRICAO_VOLUME")]
        public String DescricaoVolume { get; set; }

        [XmlElement("TIPO_EXPEDICAO")]
        public String TipoExpedicao { get; set; }

        [XmlElement("CODIGO_RASTREABILIDADE")]
        public String CodigoRastreabilidade { get; set; }

        [XmlElement("STATUS_VOLUME")]
        public String StatusVolume { get; set; }

        [XmlElement("TIPO_VOLUME")]
        public int TipoVolume { get; set; }

        public String LocalMacro { get; set; }

        public String SerialNumber;

        public String OrdemVenda;

        public String TipoProducao;
    }

    public class ESTOQUE_STATUS
    {
        public const int Liberado = 1;

        public const int BloqueadoLES = 8;

        public const int LiberadoLES = 9;
    }

    public class MODULO
    {
        public const String ExpedicaoPA = "EXPEDICAO_PA";
        public const String ExpedicaoVolume = "EXPEDICAO_VOLUME";
    }
}
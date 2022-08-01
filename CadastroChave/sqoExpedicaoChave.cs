using sqoClassLibraryAI0502Biblio;
using System;
using System.Xml.Serialization;

namespace sqoTraceabilityStation
{
    [XmlRoot("ItemFilaProducao")]
    public class sqoExpedicaoChave
    {
        [XmlElement("ID")]
        public long Id { get; set; }

        [XmlElement("CHAVE")]
        public String Chave { get; set; }

        [XmlElement("CHAVE_INSERT")]
        public String ChaveInsert { get; set; }

        [XmlElement("DESCRICAO")]
        public String Descricao { get; set; }

        [XmlElement("DESCRICAO_INSERT")]
        public String DescricaoInsert { get; set; }

        [XmlElement("DEPOSITO")]
        public String Deposito { get; set; }

        [XmlElement("DEPOSITO_INSERT")]
        public String DepositoInsert { get; set; }

        [XmlElement("TIPO_EXPEDICAO")]
        public int TipoExpedicao { get; set; }

        [XmlElement("SEPARACAO")]
        public bool Separacao { get; set; }

        [XmlElement("ENTREGA")]
        public bool Entrega { get; set; }

        [XmlElement("CARREGAMENTO")]
        public bool Carregamento { get; set; }

        [XmlElement("EQUIPE")]
        public int Equipe { get; set; }

        [XmlElement("EQUIPE_INSERT")]
        public int EquipeInsert { get; set; }

        [XmlElement("LEITURA_CODIGO_VOLUME")]
        public int LeituraCodigoVolume { get; set; }

        [XmlElement("LEITURA_CODIGO_VOLUME_INSERT")]
        public int LeituraCodigoVolumeInsert { get; set; }

        [XmlElement("LEITURA_LOCAL_DESTINO")]
        public int LeituraLocalDestino { get; set; }

        [XmlElement("LEITURA_LOCAL_DESTINO_INSERT")]
        public int LeituraLocalDestinoInsert { get; set; }

        [XmlElement("LEITURA_CHAVE_NOTA_FISCAL")]
        public int LeituraChaveNotaFiscal { get; set; }

        [XmlElement("LEITURA_CHAVE_NOTA_FISCAL_INSERT")]
        public int LeituraChaveNotaFiscalInsert { get; set; }

        [XmlElement("LEITURA_RASTREABILIDADE_COMPONENTE")]
        public int LeituraRastreabilidadeComponente { get; set; }

        [XmlElement("LEITURA_RASTREABILIDADE_COMPONENTE_INSERT")]
        public int LeituraRastreabilidadeComponenteInsert { get; set; }

        [XmlElement("AGRUPAMENTO_DOC_TRANSPORTE")]
        public int AgrupamentoDocTransporte { get; set; }

        [XmlElement("AGRUPAMENTO_DOC_TRANSPORTE_INSERT")]
        public int AgrupamentoDocTransporteInsert { get; set; }

        [XmlElement("NOTIFICACAO_CLIENTE")]
        public int NotificacaoCliente { get; set; }

        [XmlElement("NOTIFICACAO_CLIENTE_INSERT")]
        public int NotificacaoClienteInsert { get; set; }

        [XmlElement("ETIQUETA_SEQUENCIA_CARREGAMENTO")]
        public int EtiquetaSequenciaCarregamento { get; set; }

        [XmlElement("ETIQUETA_SEQUENCIA_CARREGAMENTO_INSERT")]
        public int EtiquetaSequenciaCarregamentoInsert { get; set; }

        [XmlElement("QUESTIONARIO_EXPEDICAO")]
        public int QuestionarioExpedicao { get; set; }

        [XmlElement("QUESTIONARIO_EXPEDICAO_INSERT")]
        public int QuestionarioExpedicaoInsert { get; set; }

        [XmlElement("OBSERVACAO")]
        public string Observacao { get; set; }

        [XmlElement("ATIVO")]
        public bool Ativo { get; set; }

    }

    [XmlRoot("ItemFilaProducao")]
    public class sqoClassCadastroChaveUsuario
    {
        [XmlElement("ID_USUARIO")]
        public long IdUsuario { get; set; }

        [XmlElement("CODIGO_ACAO")]
        public string CodigoAcao { get; set; }

        [XmlElement("USUARIO")]
        public string Usuario { get; set; }
    }

    public class VINCULAR_LOCAL_FIELDS
    {
        public const string CODIGO_LOCAL = "CodigoLocal";
        public const string CODIGO_LOCAL_FORM = "CodigoLocalForm";
        public const string ID = "Id";
        public const string ID_CHAVE = "IdChave";
        public const string CHAVE = "Chave";
        public const string ID_LOCAL = "IdLocal";
        public const string DEPOSITO = "Deposito";

    }

    [AutoPersistencia]
    public class sqoTipoExpedicao
    {
        public bool Separacao { get; set; }

        public bool Entrega { get; set; }

        public bool Carregamento { get; set; }

        public bool Transporte { get; set; }
    }

}

using sqoClassLibraryAI0502Biblio;
using System.Collections.Generic;
using System.Xml.Serialization;
using TemplatesStara.CommonStara;

namespace TemplateStara.Expedicao.GeracaoNumeroSerieComponente.DataModel
{
    [XmlRoot("ItemFilaProducao")]
    public class DocumentoReferenciaListagem
    {
        [XmlElement("ID")]
        public int Id { get; set; }

        [XmlElement("MATERIAL")]
        public string Material { get; set; }

        [XmlElement("DESCRICAO_COMPONENTE")]
        public string DescricaoComponente { get; set; }

        [XmlElement("QTD_COMPONENTE")]
        public string QtdComponente { get; set; }

        [XmlElement("NUMERO_SERIE")]
        public string NumeroSerie { get; set; }

        [XmlElement("DOC_REFERENCIA")]
        public string DocReferencia { get; set; }

        [XmlElement("GRUPO")]
        public string Grupo { get; set; }

        [XmlElement("VALOR")]
        public string Valor { get; set; }

        [XmlElement("ID_GERACAO")]
        public int IdGeracao { get; set; }



        private List<sqoClassRegraItemFilaProducaoEstrutura> oListaRegrasItemFilaProducao = new List<sqoClassRegraItemFilaProducaoEstrutura>();

        [XmlArray("Regras")]
        [XmlArrayItem("Regra", typeof(sqoClassRegraItemFilaProducaoEstrutura))]
        public List<sqoClassRegraItemFilaProducaoEstrutura> ListaRegrasItemFilaProducao
        {
            get { return oListaRegrasItemFilaProducao; }
            set { oListaRegrasItemFilaProducao = value; }
        }
    }

    [XmlRoot("Regra")]
    public class sqoClassRegraItemFilaProducaoEstrutura
    {
        private string sRegra = "";

        public string Regra
        {
            get { return sRegra; }
            set { sRegra = value; }
        }

        public int Id
        { get; set; }

        public string NumeroSerie
        { get; set; }

        public string DescricaoComponente
        { get; set; }

        public string OrdemProducao
        { get; set; }

        public string Valor
        { get; set; }

        public int IdGeracao
        { get; set; }

        public int IdItem
        { get; set; }
    }

    [AutoPersistencia(AutoId = true)]
    public class DocumentoReferenciaListagemList : sqoClassItemDetailBase
    {
        public int Id { get; set; }

        public string MaterialList { get; set; }

        public string DescricaoComponente { get; set; }

        public string DocumentoReferencia { get; set; }

        public string Valor { get; set; }

        public int IdGeracao { get; set; }

        public int IdItem { get; set; }

    }
}

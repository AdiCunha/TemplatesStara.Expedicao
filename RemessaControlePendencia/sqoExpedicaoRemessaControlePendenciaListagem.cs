//Comentar o define quando colar na web!
//#define NAO_COMPILAR

#if !NAO_COMPILAR
using sqoClassLibraryAI1151FilaProducao;
using System.Data;
using sqoClassLibraryAI0502VariaveisSistema;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using sqoClassLibraryAI0502Biblio;
using sqoClassLibraryAI1151FilaProducao.Estrutura;
using AI1627Common20.TemplateDebugging;
using sqoClassLibraryAI1151FilaProducao.Process;
using System.Xml.Serialization;

namespace sqoTraceabilityStation
{
    /// <summary>
    /// Template para listagem itens da requisição
    /// </summary>
    /// 
    [TemplateDebug("sqoExpedicaoRemessaControlePendenciaListagem")]
    class sqoExpedicaoRemessaControlePendenciaListagem : sqoClassProcessListar
    {
        private sqoClassRemessaPendencia oClassRemessaPendencia;
        //private sqoClassSetMessageDefaults oClassSetMessageDefaults;

        public override string Executar(string sAction, string sXmlDados, string sXmlType, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao, List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sNivel, string sUsuario, object oObjAux)
        {
            String sReturn = String.Empty;

            using (sqoClassDbConnection oDBConnection = new sqoClassDbConnection())
            {
                this.Init(sXmlDados);

                sReturn = this.ProcessBussinessLogic(oDBConnection);
            }            

            return sReturn;
        }

        private void Init(String sXmlDados)
        {
            this.oClassRemessaPendencia = new sqoClassRemessaPendencia();

            oClassRemessaPendencia = sqoClassBiblioSerDes.DeserializeObject<sqoClassRemessaPendencia>(sXmlDados);

            //oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());
        }

        private string ProcessBussinessLogic(sqoClassDbConnection oDBConnection)
        {
            String sXml = String.Empty;
            try
            {
                oDBConnection.BeginTransaction();

                sXml =  this.ToView();

                oDBConnection.Commit();
            }
            catch (Exception ex)
            {
                sqoClassMessageUserException oClassMessageUserException =
                  new sqoClassMessageUserException("Error" + Environment.NewLine + ex.Message, ex.InnerException);
                oDBConnection.Rollback();

                throw oClassMessageUserException;
            }

            return sXml;
        }

        private string ToView()
        {
            List<sqoClassRemessaPendenciaList> oDatailList = GetDatailList();

            return MontarXmlFilaProducao(oDatailList);

        }

        private List<sqoClassRemessaPendenciaList> GetDatailList()
        {
            List<sqoClassRemessaPendenciaList> oLiResult = new List<sqoClassRemessaPendenciaList>();

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@REMESSA", this.oClassRemessaPendencia.Remessa, OleDbType.VarChar, 50);

                String sQuery = @"SELECT
                                	SEQ.CODIGO_RASTREABILIDADE AS REMESSA
                                	,SEQ_EMB.LOCAL
                                	,TIPO_REC.DESCRICAO
                                	,ISNULL(ESTOQ.LOCAL_ORIGEM_MOV,'') AS LOCAL_ORIGEM_MOV
                                	,ISNULL(ESTOQ.LOCAL, '') AS LOCAL_ESTOQUE
                                	,CONVERT(FLOAT,ISNULL(TIPO_REC.ALTURA,0)) AS ALTURA
                                	,CONVERT(FLOAT,ISNULL(TIPO_REC.LARGURA,0)) AS LARGURA
                                	,CONVERT(FLOAT,ISNULL(TIPO_REC.COMPRIMENTO,0)) AS COMPRIMENTO
                                	,CONVERT(FLOAT,ISNULL(TIPO_REC.ALTURA * TIPO_REC.LARGURA * TIPO_REC.COMPRIMENTO,0)) AS CUBAGEM_C3
                                	,CONVERT(FLOAT,ISNULL(TIPO_REC.PESO_RECIPIENTE,0)) AS PESO_RECIPIENTE
                                	,CONVERT(FLOAT,ISNULL(SUM(SEQ_ITEM.PESO),0)) AS PESO_MATERIAIS
                                	,CONVERT(FLOAT,ISNULL((CASE
                                		WHEN TIPO_REC.PESO_RECIPIENTE IS NULL THEN 0
                                		ELSE TIPO_REC.PESO_RECIPIENTE
                                		END) + SUM(SEQ_ITEM.PESO),0)) AS PESO_TOTAL
                                FROM
                                	WSQOLPICKINGSEQITEMEMBALAGEM AS SEQ_EMB
                                INNER JOIN
                                	WSQOLPICKINGSEQ AS SEQ
                                ON
                                	SEQ.ID = SEQ_EMB.ID_PICKING_SEQ
                                LEFT JOIN
                                	WSQOLTIPORECIPIENTE AS TIPO_REC
                                ON
                                	SEQ_EMB.TIPO_EMBALAGEM = TIPO_REC.ID_TIPO_RECIPIENTE
                                LEFT JOIN
                                	WSQOLPICKINGSEQITENS AS SEQ_ITEM
                                ON
                                	SEQ_EMB.ID_PICKING_SEQ_ITEM = SEQ_ITEM.ID
                                LEFT JOIN
                                	WSQOLESTOQUEATUAL AS ESTOQ
                                ON
                                	SEQ_EMB.LOCAL = ESTOQ.CODIGO_RASTREABILIDADE
                                AND
                                	ESTOQ.QUANTIDADE > 0
                                
                                WHERE
                                	SEQ.CODIGO_RASTREABILIDADE = @REMESSA
                                GROUP BY
                                	SEQ.CODIGO_RASTREABILIDADE
                                	,SEQ_EMB.LOCAL
                                	,TIPO_REC.DESCRICAO
                                	,TIPO_REC.ALTURA
                                	,TIPO_REC.LARGURA
                                	,TIPO_REC.COMPRIMENTO
                                	,TIPO_REC.PESO_RECIPIENTE
                                	,ESTOQ.LOCAL_ORIGEM_MOV
                                	,ESTOQ.LOCAL ";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    oLiResult = oCommand.GetListaResultado<sqoClassRemessaPendenciaList>();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Query: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }
            }

            return oLiResult;
        }

        private String MontarXmlFilaProducao(List<sqoClassRemessaPendenciaList> oClassRemessaPendencia)
        {
            String sXmlResult = "";

            sqoClassDetailsRemessaPendencia details = new sqoClassDetailsRemessaPendencia();
            details.Details = new List<sqoClassItemDetailBaseRemessaPendencia>();

            foreach (sqoClassItemDetailBaseRemessaPendencia oClassRemessaPendencialist in oClassRemessaPendencia)
                details.Details.Add(oClassRemessaPendencialist);

            sXmlResult = sqoClassBiblioSerDes.SerializeObject(details);

            if (sXmlResult.Length > 0)
                sXmlResult = sXmlResult.Remove(0, 1);

            return sXmlResult;
        }

    }



    [XmlRoot("ItemFilaProducao")]
    public class sqoClassRemessaPendencia
    {
        [XmlElement("REMESSA")]
        public String Remessa { get; set; }

    }

    [AutoPersistencia]
    public class sqoClassRemessaPendenciaList : sqoClassItemDetailBaseRemessaPendencia
    {
        public String Remessa { get; set; }

        public String Local { get; set; }

        public String Descricao { get; set; }

        public Double Altura { get; set; }

        public Double Largura { get; set; }

        public Double Comprimento { get; set; }

        public Double CubagemC3 { get; set; }

        public Double PesoRecipiente { get; set; }

        public Double PesoMateriais { get; set; }

        public Double PesoTotal { get; set; }

        public String LocalOrigemMov { get; set; }

        public String LocalEstoque { get; set; }
    }

    [XmlRoot("RootDetails")]
    public class sqoClassDetailsRemessaPendencia
    {
        private List<sqoClassItemDetailBaseRemessaPendencia> oDetails;

        [XmlArray("Details")]
        [XmlArrayItem("Detail", typeof(sqoClassItemDetailBaseRemessaPendencia))]
        public List<sqoClassItemDetailBaseRemessaPendencia> Details
        {
            get { return oDetails; }
            set { oDetails = value; }
        }
    }

    [XmlRoot("Detail")]
    [XmlInclude(typeof(sqoClassItemDetailItemValorRemessaPendencia))]
    [XmlInclude(typeof(sqoClassRemessaPendenciaList))]
    public abstract class sqoClassItemDetailBaseRemessaPendencia
    {
    }

    public class sqoClassItemDetailItemValorRemessaPendencia : sqoClassItemDetailBaseRemessaPendencia
    {
        private string sItem;
        private string sValor;

        [XmlAttribute]
        public string Item
        {
            get { return sItem; }
            set { sItem = value; }
        }

        [XmlAttribute]
        public string Valor
        {
            get { return sValor; }
            set { sValor = value; }
        }
    }
}
#endif

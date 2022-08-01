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
    [TemplateDebug("sqoExpedicaoCadastroChaveTipoExpedicaoListagem")]
    public class sqoExpedicaoCadastroChaveTipoExpedicaoListagem : sqoClassProcessListar
    {
        private sqoExpedicaoChave oClassCadastroChave;

        public override string Executar(string sAction, string sXmlDados, string sXmlType, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao, List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sNivel, string sUsuario, object oObjAux)
        {
            String sReturn = String.Empty;

            this.Init(sXmlDados);

            sReturn = this.CadastroUsuarioCarregar();

            return sReturn;
        }

        private void Init(String sXmlDados)
        {
            oClassCadastroChave = new sqoExpedicaoChave();

            oClassCadastroChave = sqoClassBiblioSerDes.DeserializeObject<sqoExpedicaoChave>(sXmlDados);
        }

        private string CadastroUsuarioCarregar()
        {
            List<sqoTipoExpedicaoChaveListagem> oTipoExpedicao = this.GetTipoExpedicaoChave();

            return MontarXmlFilaProducao(oTipoExpedicao);
        }

        private List<sqoTipoExpedicaoChaveListagem> GetTipoExpedicaoChave()
        {
            List<sqoTipoExpedicaoChaveListagem> oTipoExpedicao;

            using (var oCommand = new sqoCommand())
            {
                oCommand
                    .Add("@ID_CHAVE", this.oClassCadastroChave.Id, OleDbType.BigInt)
                    ;

                String sQuery = @"SELECT
                                     ID 
	                                ,CHAVE	   
                                    ,CAST(CASE WHEN((TIPO_EXPEDICAO & 1) = 1) THEN 1 ELSE 0 END AS BIT) SEPARACAO
                                    ,CAST(CASE WHEN((TIPO_EXPEDICAO & 2) = 2) THEN 1 ELSE 0 END AS BIT) ENTREGA
                                    ,CAST(CASE WHEN((TIPO_EXPEDICAO & 4) = 4) THEN 1 ELSE 0 END AS BIT) CARREGAMENTO
                                    ,CAST(CASE WHEN((TIPO_EXPEDICAO & 8) = 8) THEN 1 ELSE 0 END AS BIT) TRANSPORTE
                            FROM
	                            WSQOLEXPEDICAOCHAVE
                            WHERE
	                            ID = @ID_CHAVE";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    oTipoExpedicao = oCommand.GetListaResultado<sqoTipoExpedicaoChaveListagem>();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }

            return oTipoExpedicao;
        }


        private string MontarXmlFilaProducao(List<sqoTipoExpedicaoChaveListagem> oClassTipoExpedicao)
        {
            string sXmlResult = "";

            sqoClassDetailsTipoExpedicao details = new sqoClassDetailsTipoExpedicao();
            details.Details = new List<sqoClassItemDetailBaseTipoExpedicao>();

            foreach (sqoClassItemDetailBaseTipoExpedicao oClassChaveUsuariolist in oClassTipoExpedicao)
                details.Details.Add(oClassChaveUsuariolist);

            sXmlResult = sqoClassBiblioSerDes.SerializeObject(details);

            if (sXmlResult.Length > 0)
                sXmlResult = sXmlResult.Remove(0, 1);

            return sXmlResult;
        }

    }

    [XmlRoot("RootDetails")]
    public class sqoClassDetailsTipoExpedicao
    {
        private List<sqoClassItemDetailBaseTipoExpedicao> oDetails;

        [XmlArray("Details")]
        [XmlArrayItem("Detail", typeof(sqoClassItemDetailBaseTipoExpedicao))]
        public List<sqoClassItemDetailBaseTipoExpedicao> Details
        {
            get { return oDetails; }
            set { oDetails = value; }
        }
    }

    [XmlRoot("Detail")]
    [XmlInclude(typeof(sqoClassItemDetailItemValorTipoExpedicao))]
    [XmlInclude(typeof(sqoTipoExpedicaoChaveListagem))]
    public abstract class sqoClassItemDetailBaseTipoExpedicao
    {
    }

    public class sqoClassItemDetailItemValorTipoExpedicao : sqoClassItemDetailBaseTipoExpedicao
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

    [AutoPersistencia]
    public class sqoTipoExpedicaoChaveListagem : sqoClassItemDetailBaseTipoExpedicao
    {
        public long Id { get; set; }

        public string Chave { get; set; }

        public bool Separacao { get; set; }

        public bool Entrega { get; set; }

        public bool Carregamento { get; set; }

    }

}

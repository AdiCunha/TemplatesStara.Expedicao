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
    [TemplateDebug("sqoExpedicaoCadastroChaveListagem")]
   public class sqoExpedicaoCadastroChaveListagem : sqoClassProcessListar
    {
        private sqoExpedicaoChave oClassCadastroChave;
   

        public override string Executar(string sAction, string sXmlDados, string sXmlType, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao, List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sNivel, string sUsuario, object oObjAux)
        {
            String sReturn = String.Empty;

            using (sqoClassDbConnection oDBConnection = new sqoClassDbConnection())
            {
                Init(sXmlDados);

                switch (sAction)
                {
                    case sqoCadastroAction.DELIVERY:
                        sReturn = CadastroLocalCarregar();
                        break;
                }
            }

            return sReturn;

        }

        private void Init(String sXmlDados)
        {
            oClassCadastroChave = new sqoExpedicaoChave();

            oClassCadastroChave = sqoClassBiblioSerDes.DeserializeObject<sqoExpedicaoChave>(sXmlDados);
        }

        private string CadastroLocalCarregar()
        {
            List<sqoClassChaveEntrega> oClassChaveEntrega = ChaveLocalGet(oClassCadastroChave);
            return MontarXmlFilaProducao(oClassChaveEntrega);
        }

        private List<sqoClassChaveEntrega> ChaveLocalGet(sqoExpedicaoChave oClassCadastroChave)
        {
            List<sqoClassChaveEntrega> oClassChaveEntrega;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@ID", oClassCadastroChave.Id, OleDbType.BigInt)
                    ;

                String sQuery = @"SELECT
                                	ENTREGA.ID 
                                	,ENTREGA.ID_CHAVE
                                	,CHAVE.CHAVE
                                	,ENTREGA.ID_LOCAL
                                	,ENTREGA.CODIGO_LOCAL
                                	,LOC.LOCAL_INTEGRACAO AS DEPOSITO
                                FROM 
                                	WSQOLEXPEDICAOCHAVELOCALENTREGA AS ENTREGA
                                INNER JOIN
                                	WSQOLEXPEDICAOCHAVE AS CHAVE
                                ON
                                	ENTREGA.ID_CHAVE = CHAVE.ID
                                INNER JOIN
                                	WSQOLLOCAIS AS LOC
                                ON
                                	ENTREGA.ID_LOCAL = LOC.ID_LOCAL
                                WHERE ENTREGA.ID_CHAVE = @ID
                                ";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    oClassChaveEntrega = oCommand.GetListaResultado<sqoClassChaveEntrega>();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }

            return oClassChaveEntrega;
        }

        private String MontarXmlFilaProducao(List<sqoClassChaveEntrega> oClassChaveEntrega)
        {
            String sXmlResult = "";

            sqoClassDetails details = new sqoClassDetails();
            details.Details = new List<sqoClassItemDetailBase>();

            foreach (sqoClassItemDetailBase oClassChaveEntregalist in oClassChaveEntrega)
                details.Details.Add(oClassChaveEntregalist);

            sXmlResult = sqoClassBiblioSerDes.SerializeObject(details);

            if (sXmlResult.Length > 0)
                sXmlResult = sXmlResult.Remove(0, 1);

            return sXmlResult;
        }

    }

    public class sqoCadastroAction
    {
        public const string DELIVERY = "Delivery";
    }

    public class sqoClassChaveEntrega : sqoClassItemDetailBase
    {
        [ColunaAttribute("Id", "ID", TIPO_COLUNA.tcLong, -1)]
        public long Id { get; set; }

        [ColunaAttribute("IdChave", "ID_CHAVE", TIPO_COLUNA.tcLong, -1)]
        public long IdChave { get; set; }

        [ColunaAttribute("Chave", "CHAVE", TIPO_COLUNA.tcString)]
        public string Chave { get; set; }

        [ColunaAttribute("IdLocal", "ID_LOCAL", TIPO_COLUNA.tcLong, -1)]
        public long IdLocal { get; set; }

        [ColunaAttribute("CodigoLocal", "CODIGO_LOCAL", TIPO_COLUNA.tcString)]
        public string CodigoLocal { get; set; }

        [ColunaAttribute("Deposito", "DEPOSITO", TIPO_COLUNA.tcString)]
        public string Deposito { get; set; }
    }

    [XmlRoot("RootDetails")]
    public class sqoClassDetails
    {
        private List<sqoClassItemDetailBase> oDetails;

        [XmlArray("Details")]
        [XmlArrayItem("Detail", typeof(sqoClassItemDetailBase))]
        public List<sqoClassItemDetailBase> Details
        {
            get { return oDetails; }
            set { oDetails = value; }
        }
    }

    [XmlRoot("Detail")]
    [XmlInclude(typeof(sqoClassItemDetailItemValor))]
    [XmlInclude(typeof(sqoClassChaveEntrega))]
    public abstract class sqoClassItemDetailBase
    {
    }

    public class sqoClassItemDetailItemValor : sqoClassItemDetailBase
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

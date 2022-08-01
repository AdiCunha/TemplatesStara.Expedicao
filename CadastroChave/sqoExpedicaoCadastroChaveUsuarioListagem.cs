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
    [TemplateDebug("sqoExpedicaoCadastroChaveUsuarioListagem")]
   public class sqoExpedicaoCadastroChaveUsuarioListagem : sqoClassProcessListar
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
            List<sqoClassChaveUsuario> oClassChaveUsuario = this.GetChaveUsuario();

            return MontarXmlFilaProducao(oClassChaveUsuario);
        }

        private List<sqoClassChaveUsuario> GetChaveUsuario()
        {
            List<sqoClassChaveUsuario> oClassChaveUsuario;

            using (var oCommand = new sqoCommand())
            {
                oCommand
                    .Add("@ID", this.oClassCadastroChave.Id, OleDbType.BigInt)
                    ;

                String sQuery = @"SELECT 
                                     CHAVE.ID
                                    ,CHAVE_USUARIO.ID AS ID_USUARIO
                                    ,CHAVE.CHAVE
                                    ,CHAVE_USUARIO.USUARIO
                                    ,CHAVE_USUARIO.CODIGO_ACAO
                                    ,CAST(CASE WHEN((CHAVE_USUARIO.CODIGO_ACAO & 1) = 1) THEN 1 ELSE 0 END AS BIT) SEPARACAO
                                    ,CAST(CASE WHEN((CHAVE_USUARIO.CODIGO_ACAO & 2) = 2) THEN 1 ELSE 0 END AS BIT) ENTREGA
                                    ,CAST(CASE WHEN((CHAVE_USUARIO.CODIGO_ACAO & 4) = 4) THEN 1 ELSE 0 END AS BIT) CARREGAMENTO
                                    FROM
                                    WSQOLEXPEDICAOCHAVE AS CHAVE
                                LEFT JOIN
                                    WSQOLEXPEDICAOCHAVEUSUARIO AS CHAVE_USUARIO 
                                ON
                                    CHAVE_USUARIO.ID_CHAVE = CHAVE.ID
                                WHERE
                                    CHAVE.ID = @ID";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    oClassChaveUsuario = oCommand.GetListaResultado<sqoClassChaveUsuario>();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }

            return oClassChaveUsuario;
        }

        private String MontarXmlFilaProducao(List<sqoClassChaveUsuario> oClassChaveUsuario)
        {
            String sXmlResult = "";

            sqoClassDetailsChaveUsuario details = new sqoClassDetailsChaveUsuario();
            details.Details = new List<sqoClassItemDetailBaseChaveUsuario>();

            foreach (sqoClassItemDetailBaseChaveUsuario oClassChaveUsuariolist in oClassChaveUsuario)
                details.Details.Add(oClassChaveUsuariolist);

            sXmlResult = sqoClassBiblioSerDes.SerializeObject(details);

            if (sXmlResult.Length > 0)
                sXmlResult = sXmlResult.Remove(0, 1);

            return sXmlResult;
        }

    }

    [XmlRoot("RootDetails")]
    public class sqoClassDetailsChaveUsuario
    {
        private List<sqoClassItemDetailBaseChaveUsuario> oDetails;

        [XmlArray("Details")]
        [XmlArrayItem("Detail", typeof(sqoClassItemDetailBaseChaveUsuario))]
        public List<sqoClassItemDetailBaseChaveUsuario> Details
        {
            get { return oDetails; }
            set { oDetails = value; }
        }
    }

    [XmlRoot("Detail")]
    [XmlInclude(typeof(sqoClassItemDetailItemValorChaveUsuario))]
    [XmlInclude(typeof(sqoClassChaveUsuario))]
    public abstract class sqoClassItemDetailBaseChaveUsuario
    {
    }

    public class sqoClassItemDetailItemValorChaveUsuario : sqoClassItemDetailBaseChaveUsuario
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
    public class sqoClassChaveUsuario : sqoClassItemDetailBaseChaveUsuario
    {
        public bool Separacao { get; set; }

        public bool Entrega { get; set; }

        public bool Carregamento { get; set; }

        public bool Transporte { get; set; }

        public long Id { get; set; }

        public string Chave { get; set; }

        public long IdUsuario { get; set; }

        public string Usuario { get; set; }

    }
}

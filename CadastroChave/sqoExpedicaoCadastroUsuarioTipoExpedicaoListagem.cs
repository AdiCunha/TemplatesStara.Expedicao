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
    [TemplateDebug("sqoExpedicaoCadastroUsuarioTipoExpedicaoListagem")]
   public class sqoExpedicaoCadastroUsuarioTipoExpedicaoListagem : sqoClassProcessListar
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
            List<sqoTipoExpedicaoUsuario> oTipoExpedicaoUsuario = this.GetTipoExpedicaoUsuario();

            return MontarXmlFilaProducao(oTipoExpedicaoUsuario);
        }

        private List<sqoTipoExpedicaoUsuario> GetTipoExpedicaoUsuario()
        {
            List<sqoTipoExpedicaoUsuario> oTipoExpedicaoUsuario;

            using (var oCommand = new sqoCommand())
            {
                oCommand
                    .Add("@ID", this.oClassCadastroChave.Id, OleDbType.BigInt)
                    ;

                String sQuery = @"SELECT
 	                                 USUARIO.ID AS ID_USUARIO
	                                ,CHAVE.CHAVE
	                                ,USUARIO.USUARIO
                                    ,CAST(CASE WHEN((USUARIO.CODIGO_ACAO & 1) = 1) THEN 1 ELSE 0 END AS BIT) SEPARACAO
                                    ,CAST(CASE WHEN((USUARIO.CODIGO_ACAO & 2) = 2) THEN 1 ELSE 0 END AS BIT) ENTREGA
                                    ,CAST(CASE WHEN((USUARIO.CODIGO_ACAO & 4) = 4) THEN 1 ELSE 0 END AS BIT) CARREGAMENTO
                                FROM
	                                WSQOLEXPEDICAOCHAVEUSUARIO AS USUARIO

                                INNER JOIN
	                                WSQOLEXPEDICAOCHAVE AS CHAVE
                                ON
	                                USUARIO.ID_CHAVE = CHAVE.ID

                                WHERE
	                                USUARIO.ID_CHAVE = @ID";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    oTipoExpedicaoUsuario = oCommand.GetListaResultado<sqoTipoExpedicaoUsuario>();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }

            return oTipoExpedicaoUsuario;
        }

        private string MontarXmlFilaProducao(List<sqoTipoExpedicaoUsuario> oClassTipoExpedicaoUsuario)
        {
            string sXmlResult = "";

            sqoClassDetailsTipoExpedicaoUsuario details = new sqoClassDetailsTipoExpedicaoUsuario();
            details.Details = new List<sqoClassItemDetailBaseTipoExpedicaoUsuario>();

            foreach (sqoClassItemDetailBaseTipoExpedicaoUsuario oClassChaveUsuariolist in oClassTipoExpedicaoUsuario)
                details.Details.Add(oClassChaveUsuariolist);

            sXmlResult = sqoClassBiblioSerDes.SerializeObject(details);

            if (sXmlResult.Length > 0)
                sXmlResult = sXmlResult.Remove(0, 1);

            return sXmlResult;
        }
    }

    [XmlRoot("RootDetails")]
    public class sqoClassDetailsTipoExpedicaoUsuario
    {
        private List<sqoClassItemDetailBaseTipoExpedicaoUsuario> oDetails;

        [XmlArray("Details")]
        [XmlArrayItem("Detail", typeof(sqoClassItemDetailBaseTipoExpedicaoUsuario))]
        public List<sqoClassItemDetailBaseTipoExpedicaoUsuario> Details
        {
            get { return oDetails; }
            set { oDetails = value; }
        }
    }

    [XmlRoot("Detail")]
    [XmlInclude(typeof(sqoClassItemDetailItemValorTipoExpedicaoUsuario))]
    [XmlInclude(typeof(sqoTipoExpedicaoUsuario))]
    public abstract class sqoClassItemDetailBaseTipoExpedicaoUsuario
    {
    }

    public class sqoClassItemDetailItemValorTipoExpedicaoUsuario : sqoClassItemDetailBaseTipoExpedicaoUsuario
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
    public class sqoTipoExpedicaoUsuario : sqoClassItemDetailBaseTipoExpedicaoUsuario
    {
        public bool Separacao { get; set; }

        public bool Entrega { get; set; }

        public bool Carregamento { get; set; }

        public bool Transporte { get; set; }

        public long IdUsuario { get; set; }

        public string Usuario { get; set; }

        public string Chave { get; set; }
    }
}


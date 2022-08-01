//Comentar o define quando colar na web!
//#define NAO_COMPILAR

#if !NAO_COMPILAR
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
using sqoClassLibraryAI1151FilaProducao;
using TemplatesStara.CommonStara;

namespace TemplatesStara.Expedicao.RastreabilidadeComponente
{
    [TemplateDebug("sqoExpedicaoGeracaoRastrComponenteListagem")]
    class sqoExpedicaoGeracaoRastrComponenteListagem1 : sqoClassProcessListar
    {
        private sqoClassGeracaoRastreabilidadeComponente oClassGeracaoRastreabilidadeComponente;

        public override string Executar(string sAction, string sXmlDados, string sXmlType, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao,
            List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sNivel, string sUsuario, object oObjAux)
        {
            String sReturn = String.Empty;

            using (sqoClassDbConnection oDbConnection = new sqoClassDbConnection())
            {
                Init(sXmlDados);

                sReturn = ProcessBusinessLogic(oDbConnection);
            }

            return sReturn;
        }

        private void Init(String sXmlDados)
        {
            oClassGeracaoRastreabilidadeComponente = new sqoClassGeracaoRastreabilidadeComponente();

            oClassGeracaoRastreabilidadeComponente = sqoClassBiblioSerDes.DeserializeObject<sqoClassGeracaoRastreabilidadeComponente>(sXmlDados);

        }

        private string ProcessBusinessLogic(sqoClassDbConnection oDBConnection)
        {
            String sReturn = String.Empty;

            try
            {
                oDBConnection.BeginTransaction();

                sReturn = ListRastreabilidadeComponente();

                oDBConnection.Commit();
            }
            catch (Exception ex)
            {
                sqoClassMessageUserException oClassMessageUserException =
                   new sqoClassMessageUserException("Error" + Environment.NewLine + ex.Message, ex.InnerException);
                oDBConnection.Rollback();

                throw oClassMessageUserException;
            }

            return sReturn;
        }

        private string ListRastreabilidadeComponente()
        {
            List<sqoClassComponenteRastList> oClassComponenteRastList = GetRastreabilidadeComponente();

            return MontarXmlFilaProducao(oClassComponenteRastList);
        }

        private List<sqoClassComponenteRastList> GetRastreabilidadeComponente()
        {
            int TipoRastreabilidade = 10;

            int Ativo = 1;

            List<sqoClassComponenteRastList> oClassComponenteRastList;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@ORDEM_PRODUCAO", oClassGeracaoRastreabilidadeComponente.OrdemProducao, OleDbType.VarChar, 50)
                    .Add("@ORDEM_PRODUCAO", oClassGeracaoRastreabilidadeComponente.OrdemProducao, OleDbType.VarChar, 50)
                    .Add("@ATIVO", Ativo, OleDbType.Integer)
                    .Add("@MATERIAL", oClassGeracaoRastreabilidadeComponente.Material, OleDbType.VarChar, 100)
                    .Add("@TIPO_RASTREABILIDADE", TipoRastreabilidade, OleDbType.Integer)
                    ;

                string sQuery = @"SELECT
                                       C.CODIGO_PECA AS MATERIAL_LIST
                                      ,C.ID AS ID_COMPONENTE
                                      ,C.DESCRICAO_COMPONENTE
                                      ,? AS DOC_REFERENCIA
                                      ,G.ID AS ID_GERACAO
                                      ,G.NUMERO_SERIE
                                 FROM
                                    WSQOLPCP2PECACOMPONENTE AS C
                                 LEFT JOIN
                                    WSQOLEXPEDICAOVOLUME AS V
                                 ON
                                    V.CODIGO_RASTREABILIDADE = ?
                                 LEFT JOIN
                                    WSQOLEXPEDICAOCOMPONENTEGERACAONUMEROSERIE AS G
                                 ON
                                    G.ID_VOLUME = V.ID
                                 AND
                                    G.ID_COMPONENTE = C.ID
                                 WHERE
                                    C.ATIVO = ?
                                 AND
                                    C.CODIGO_PECA = ?
                                 AND
                                    C.TIPO = ?";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var Teste = oCommand.QueryToString();

                    oClassComponenteRastList = oCommand.GetListaResultado<sqoClassComponenteRastList>();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }

                return oClassComponenteRastList;
            }
        }

        private string MontarXmlFilaProducao(List<sqoClassComponenteRastList> oClassComponenteRastList)
        {
            String sXmlResult = String.Empty;

            var oDetail = new sqoClassDetails();

            foreach (var oList in oClassComponenteRastList)
            {

                oDetail.Add(oList);
            }

            var oXml = oDetail.Serializar();

            return oXml;
        }
    }

    /// <summary>
    /// Classe para instanciar o modelo do criteria da tela.
    /// </summary>
    [XmlRoot("ItemFilaProducao")]
    public class sqoClassGeracaoRastreabilidadeComponente
    {
        [XmlElement("MATERIAL")]
        public string Material { get; set; }

        [XmlElement("ORDEM_PRODUCAO")]
        public string OrdemProducao { get; set; }

        [XmlElement("NUMERO_SERIE")]
        public string NumeroSerie { get; set; }
    }

    [AutoPersistencia]
    public class sqoClassComponenteRastList : sqoClassItemDetailBase
    {
        public string MaterialList { get; set; }

        public int IdComponente { get; set; }

        public string DescricaoComponente { get; set; }

        public string DocReferencia { get; set; }

        public string NumeroSerie { get; set; }

        public int IdGeracao { get; set; }
    }

}
#endif
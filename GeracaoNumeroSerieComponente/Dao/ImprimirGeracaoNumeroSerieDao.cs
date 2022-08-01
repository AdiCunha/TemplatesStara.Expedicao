using sqoClassLibraryAI0502VariaveisSistema;
using sqoClassLibraryAI1151FilaProducao;
using System;
using System.Data;


namespace TemplateStara.Expedicao.GeracaoNumeroSerieComponente.Dao
{
    public class ImprimirGeracaoNumeroSerieDao
    {
        public string GetDescricaoMaterial(string Material)
        {

            string sQuery = @"SELECT 
	                             DESCRICAO 
                              FROM 
                                 WSQOPCP2PECA
                              WHERE 
	                             CODIGO_PECA = ? ";

            using (var oCommand = new sqoCommand(CommandType.Text))
            {

                oCommand.SetCommandText(sQuery);

                oCommand.Add("@MATERIAL", Material);

                var oResult = oCommand.GetResultado();

                if (oResult == null)
                    throw new sqoClassMessageUserException("Descrição do Material: " + Material + " não econtrada no cadastro." + Environment.NewLine);

                return oResult.ToString();
            }

        }

        public string GetDataGeracao(string Material, string NumeroSerie)
        {
            string sQuery = @"SELECT 
	                            DATA AS DATA_GERACAO
                              FROM 
	                            WSQOLEXPEDICAOCOMPONENTEGERACAONUMEROSERIE
                              WHERE
	                            CODIGO_PECA = ?
                              AND
	                            NUMERO_SERIE =  ? ";

            using (var oCommand = new sqoCommand(CommandType.Text))
            {

                oCommand.SetCommandText(sQuery);

                oCommand.Add("@MATERIAL", Material)
                        .Add("@NUMERO_SERIE", NumeroSerie);

                var oResult = oCommand.GetResultado();

                if (oResult == null)
                    throw new sqoClassMessageUserException("Data Geração nula ou registro ainda não foi gerado." + Environment.NewLine);

                return oResult.ToString();
            }

        }

        public string GetNumeroSerie(string DocumentoReferencia)
        {
            string sQuery = @"SELECT
		                        NUMERO_SERIE 
	                          FROM
		                        WSQOLEXPEDICAOCOMPONENTEGERACAONUMEROSERIE
	                          WHERE
		                        DOC_REFERENCIA = ? ";

            using (var oCommand = new sqoCommand(CommandType.Text))
            {        
                oCommand.SetCommandText(sQuery);

                oCommand.Add("@DOC_REFERENCIA", DocumentoReferencia);

                var oResult = oCommand.GetResultado();

                if (oResult == null)
                    throw new sqoClassMessageUserException("Data Geração nula ou registro ainda não foi gerado." + Environment.NewLine);

                return oResult.ToString();
            }

        }

        public bool PermitirImpressao(string DocumentoReferencia, string Numero_Serie)
        {
            int Tipo = 20;

            bool Result = true;

            string sQuery = @"SELECT
	                             FIM.ID
	                            ,FIM.MATERIAL
	                            ,FIM.NUMERO_SERIE_MATERIAL
	                            ,FIM.ORDEM_PRODUCAO
	                            ,SUM(FIM.QTD_GERADOS) AS QTD_GERADOS
	                            ,COUNT(FIM.ID) AS QTD_TOTAL

                             FROM
	                             (
		                            SELECT
			                             ISNULL(S.ID, 0) AS ID
			                            ,ISNULL(C.CODIGO_PECA, '') AS MATERIAL
			                            ,ISNULL(S.NUMERO_SERIE, '') AS NUMERO_SERIE_MATERIAL
			                            ,ISNULL(C.DESCRICAO_COMPONENTE, '') AS COMPONENTE
			                            ,ISNULL(I.VALOR, '') AS NUMERO_SERIE_COMPONENTE
			                            ,ISNULL(S.DOC_REFERENCIA, '') AS ORDEM_PRODUCAO
			                            ,CASE WHEN ISNULL(I.VALOR, '') <> '' THEN 1
			                            ELSE 0 END QTD_GERADOS
		                            FROM
			                            WSQOLPCP2PECACOMPONENTE AS C

		                            LEFT JOIN
			                            WSQOLEXPEDICAOCOMPONENTEGERACAONUMEROSERIE AS S
		                            ON
			                            C.CODIGO_PECA = S.CODIGO_PECA

		                            LEFT JOIN
			                            WSQOLEXPEDICAOCOMPONENTEGERACAONUMEROSERIEITEM AS I
		                            ON
			                            I.ID_GERACAO = S.ID
		                            AND
			                            I.DESCRICAO = C.DESCRICAO_COMPONENTE
		                            WHERE
			                            C.TIPO = ?
		                            AND
			                            ISNULL(S.DOC_REFERENCIA, '') <> ''
		                            GROUP BY
			                             S.ID
			                            ,C.CODIGO_PECA
			                            ,S.NUMERO_SERIE
			                            ,DESCRICAO_COMPONENTE
			                            ,I.VALOR
			                            ,S.DOC_REFERENCIA
	                            ) AS FIM

                            WHERE
	                           FIM.ORDEM_PRODUCAO = ?
                            
                            AND
                               FIM.NUMERO_SERIE_MATERIAL = ?

                            GROUP BY
		                         FIM.ID
	                            ,FIM.MATERIAL
	                            ,FIM.NUMERO_SERIE_MATERIAL
	                            ,FIM.ORDEM_PRODUCAO
                            HAVING 
	                            SUM(FIM.QTD_GERADOS) = COUNT(FIM.ID)";

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand.SetCommandText(sQuery);

                oCommand.Add("@TIPO", Tipo);
                oCommand.Add("@DOC_REFERENCIA", DocumentoReferencia);
                oCommand.Add("@NUMERO_SERIE", Numero_Serie);

                var oResult = oCommand.GetResultado();

                if (oResult == null)
                    Result = false;

                return Result;
            }
        }
    }
}

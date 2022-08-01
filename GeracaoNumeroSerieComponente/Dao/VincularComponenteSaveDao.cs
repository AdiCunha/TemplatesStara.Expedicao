using sqoClassLibraryAI0502VariaveisSistema;
using System;
using System.Data;
using sqoClassLibraryAI1151FilaProducao;
using System.Data.OleDb;

namespace TemplateStara.Expedicao.GeracaoNumeroSerieComponente.Dao
{
    public class VincularComponenteSaveDao
    {
        public void SetValorVincularComponente(long Id, string Valor, long IdVinculo)
        {
            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand.SetCommandText(@"UPDATE
                                            WSQOLEXPEDICAOCOMPONENTEGERACAONUMEROSERIEITEM
                                          SET 
                                           VALOR = ?
                                          ,ID_GERACAO_VINCULO = ?
                                          WHERE 
	                                         ID = ?")
                .Add("@VALOR", Valor)
                .Add("@ID_GERACAO_VINCULO", IdVinculo)
                .Add("@ID", Id)
                ;

                try
                {
                    oCommand.Execute();
                }

                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }
            }
        }

        public void InsertValorVincularComponente(string DescricaoComponente, string Valor, string Usuario, long Id, int IdGeracaoVinculo)
        {
            DateTime Data = DateTime.Now;

            //object IdGeracao = 0;

            //if (IdGeracaoVinculo > 0)
            //     IdGeracao = IdGeracaoVinculo;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@ID_GERACAO", Id)
                    .Add("@DESCRICAO", DescricaoComponente)
                    .Add("@VALOR", Valor)
                    .Add("@USUARIO", Usuario)
                    .Add("@DATA", Data)
                    .Add("@ID_GERACAO_VINCULO", IdGeracaoVinculo)
                    ;

                string sQuery = @"INSERT INTO [dbo].[WSQOLEXPEDICAOCOMPONENTEGERACAONUMEROSERIEITEM]
                                       ([ID_GERACAO]
                                       ,[DESCRICAO]
                                       ,[VALOR]
                                       ,[USUARIO]
                                       ,[DATA]
                                       ,[ID_GERACAO_VINCULO])
                                  VALUES
                                       (?, ?, ?, ?, ?, ?)";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.ExecuteGetScopeIdentity();

                    int nResult = 0;
                    bool bParse = false;

                    if (oResult != 0)
                        bParse = int.TryParse(oResult.ToString(), out nResult);

                    if (!bParse)
                        throw new Exception("Erro ao executar Insert");

                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Insert: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }
            }
        }

        public int VerificarQtdOPMaiorQueUm(string DocReferencia)
        {

            int Quantidade = 1;

            string sQuery = @"SELECT
	                            1 AS QTD_COMPONENTES
                              FROM
	                            WSQOPCP2SEQPRODUCAO
                              WHERE
	                            ORDEM_PRODUCAO = ?
                              AND
	                            QUANTIDADE > ?";

            using (var oCommand = new sqoCommand(CommandType.Text))
            {

                oCommand.SetCommandText(sQuery);

                oCommand.Add("@ORDEM_PRODUCAO", DocReferencia);
                oCommand.Add("@QUANTIDADE", Quantidade);

                var oResult = oCommand.GetResultado();

                if (oResult == null)
                    oResult = 0;

                return Convert.ToInt32(oResult);
            }
        }

        public int InsertDocRefGeracaoComponente(string CodigoPeca, string DocReferencia)
        {
            DateTime Date = DateTime.Now;
            int IdGeracaoComponente = 0;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@CODIGO_PECA", CodigoPeca)
                    .Add("@DOC_REFERENCIA", DocReferencia)
                    .Add("@DATA", Date, OleDbType.DBTimeStamp)
                    ;

                string sQuery = @"INSERT INTO [dbo].[WSQOLEXPEDICAOCOMPONENTEGERACAONUMEROSERIE]
                                       ( CODIGO_PECA
                                        ,DOC_REFERENCIA
                                        ,DATA)
                                  VALUES
                                       (?, ?, ?)";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.ExecuteGetScopeIdentity();

                    int nResult = 0;
                    bool bParse = false;

                    if (oResult != 0)
                        bParse = int.TryParse(oResult.ToString(), out nResult);

                    if (!bParse)
                        throw new Exception("Erro ao executar Insert");

                    IdGeracaoComponente = nResult;
                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Insert: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }
            }

            return IdGeracaoComponente;
        }

        public string GetValorComponente(int Id)
        {
            string sQuery = @"SELECT 
	                            VALOR 
                              FROM
	                            WSQOLEXPEDICAOCOMPONENTEGERACAONUMEROSERIEITEM 
                              WHERE 
	                            ID = ? ";

            using (var oCommand = new sqoCommand(CommandType.Text))
            {

                oCommand.SetCommandText(sQuery);

                oCommand.Add("@ID", Id);

                var oResult = oCommand.GetResultado();

                if (oResult == null)
                    oResult = 0;

                return oResult.ToString();
            }

        }

        public int GetIdHeader(string NrSerie)
        {
            string sQuery = @"SELECT
                                 ID
                              FROM
	                             WSQOLEXPEDICAOCOMPONENTEGERACAONUMEROSERIE
                              WHERE
	                             NUMERO_SERIE = ? ";

            using (var oCommand = new sqoCommand(CommandType.Text))
            {

                oCommand.SetCommandText(sQuery);

                oCommand.Add("@NUMERO_SERIE", NrSerie);

                var oResult = oCommand.GetResultado();

                if (oResult == null)
                    oResult = 0;

                return  Convert.ToInt32(oResult);
            }
        }
    }
}

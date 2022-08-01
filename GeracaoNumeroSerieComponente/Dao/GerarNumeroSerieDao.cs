using sqoClassLibraryAI0502VariaveisSistema;
using sqoClassLibraryAI1151FilaProducao;
using System;
using System.Data;

namespace TemplateStara.Expedicao.GeracaoNumeroSerieComponente.Dao
{
    public class GerarNumeroSerieDao
    {
        public bool GetNumeroSerie(string DocumentoReferencia, string Material)
        {

            bool Result = false;

            string sQuery = @"SELECT
                                  1
	                          FROM
		                        WSQOLEXPEDICAOCOMPONENTEGERACAONUMEROSERIE
	                          WHERE
		                        DOC_REFERENCIA = ?
                               AND
                                CODIGO_PECA = ?";

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand.SetCommandText(sQuery);

                oCommand.Add("@DOC_REFERENCIA", DocumentoReferencia);
                oCommand.Add("@CODIGO_PECA", Material);

                var oResult = oCommand.GetResultado();

                if (oResult == null)
                    Result = true; 
                    //    throw new sqoClassMessageUserException("Número de série não encontrado." + Environment.NewLine);

                    return Result;
            }
        }

        public void InsertNumeroSerie(string Material
                                    , string NumeroSerie
                                    , string DocReferencia
                                    , string Usuario
                                    , string Observacao)
        {
            DateTime Data = DateTime.Now;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@MATERIAL", Material)
                    .Add("@NUMERO_SERIE", NumeroSerie)
                    .Add("@DOCREFERENCIA", DocReferencia)
                    .Add("@USUARIO", Usuario)
                    .Add("@DATA", Data)
                    .Add("@OBSERVACAO", Observacao)
                    ;

                string sQuery = @"INSERT INTO [dbo].[WSQOLEXPEDICAOCOMPONENTEGERACAONUMEROSERIE]
                                        ([CODIGO_PECA]
                                        ,[NUMERO_SERIE]
                                        ,[DOC_REFERENCIA]
                                        ,[USUARIO]
                                        ,[DATA]
                                        ,[OBSERVACAO])
                                  VALUES
                                       (?, ?, ?, ?, ?, ?)";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.GetResultado();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Insert: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }
            }
        }

        public void SetNumeroSerie(string NumeroSerie, string Observacao, int Id)
        {
            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand.SetCommandText(@"UPDATE
                                            WSQOLEXPEDICAOCOMPONENTEGERACAONUMEROSERIE
                                          SET 
                                            NUMERO_SERIE = ?
                                           ,OBSERVACAO = ?

                                          WHERE 
	                                         ID = ? ")

                .Add("@NUMERO_SERIE", NumeroSerie)
                .Add("@OBSERVACAO", Observacao)
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

        public bool VerificaNumeroSerieExistente(string NumeroSerie)
        {

            bool Result = true;

            string sQuery = @"SELECT
	                            NUMERO_SERIE
                              FROM 
	                            WSQOLEXPEDICAOCOMPONENTEGERACAONUMEROSERIE
                              WHERE
	                            NUMERO_SERIE = ?";

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand.SetCommandText(sQuery);

                oCommand.Add("@NUMERO_SERIE", NumeroSerie);

                var oResult = oCommand.GetResultado();

                if (oResult == null)
                    Result = false;

                return Result;
            }
        }
    }
}

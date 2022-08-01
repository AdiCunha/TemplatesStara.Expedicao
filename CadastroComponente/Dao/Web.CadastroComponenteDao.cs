using AI1627Common20.Log;
using sqoClassLibraryAI0502VariaveisSistema;
using System;
using System.Data;
using TemplateStara.Expedicao.CadastroComponente.DataModel;

namespace sqoTraceabilityStation
{
    public class CadastroComponenteDao
    {
        public bool GetPeca(string Material)
        {
            bool result = true;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@MATERIAL", Material)
                    ;

                string sQuery = @"SELECT
                                    1 
                                  FROM
                                    WSQOPCP2PECA 
                                  WHERE 
                                    CODIGO_PECA = ?";

                oCommand.SetCommandText(sQuery);

                try
                {
                    var oResult = oCommand.GetResultado();

                    if (oResult == null)
                        result = false;

                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }

                return result;
            }
        }

        public bool GetComponenteMaterial(CadastroComponente oCadastroComponente)
        {
            bool Result = true;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@CODIGO_PECA", oCadastroComponente.Material)
                    .Add("@DESCRICAO_COMPONENTE", oCadastroComponente.DescricaoComponente)
                    .Add("@TIPO", oCadastroComponente.Tipo)
                    ;

                string sQuery = @"SELECT 
                                     1 
                                  FROM 
                                     WSQOLPCP2PECACOMPONENTE 
                                  WHERE 
                                     CODIGO_PECA = ? 
                                  AND
                                     DESCRICAO_COMPONENTE = ? 
                                  AND
                                     TIPO = ?";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.GetResultado();

                    if (oResult == null)
                        Result = false;

                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.GetForLog()
                        + Environment.NewLine, ex.InnerException);
                }
            }

            return Result;
        }

        public long SaveComponente(CadastroComponente oCadastroComponente, string sUsuario)
        {
            long Result = -1;
            int Ativo = 1;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                DateTime Data = DateTime.Now;

                oCommand
                    .Add("@CODIGO_PECA", oCadastroComponente.Material)
                    .Add("@DESCRICAO_COMPONENTE", oCadastroComponente.DescricaoComponente)
                    .Add("@TIPO", oCadastroComponente.Tipo)
                    .Add("@ATIVO", Ativo)
                    .Add("@GRUPO", oCadastroComponente.Grupo)
                    .Add("@USUARIO", sUsuario)
                    .Add("@DATA", Data)
                    .Add("@OBSERVACAO", oCadastroComponente.Observacao)
                    ;

                string sQuery = @"DECLARE @ID_COMPONENTE BIGINT
                                
                                INSERT INTO [WSQOLPCP2PECACOMPONENTE]
                                            ([CODIGO_PECA]
                                            ,[DESCRICAO_COMPONENTE]
                                            ,[TIPO]
                                            ,[ATIVO]
                                            ,[GRUPO]
                                            ,[USUARIO]
                                            ,[DATA]
                                            ,[OBSERVACAO])
                                     VALUES
                                           (?, ?, ?, ?, ?, ?, ?, ?)

                                SET @ID_COMPONENTE = SCOPE_IDENTITY()

                                SELECT @ID_COMPONENTE";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    Result = (long)oCommand.GetResultado();

                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }
            return Result;
        }

        public void UpdateComponente(CadastroComponente oCadastroComponente, string sUsuario)
        {
            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                DateTime Data = DateTime.Now;

                int Ativo = 1;

                oCommand
                    .Add("@DESCRICAO_COMPONENTE", oCadastroComponente.DescricaoComponente)
                    .Add("@TIPO", oCadastroComponente.Tipo)
                    .Add("@ATIVO", Ativo)
                    .Add("@GRUPO", oCadastroComponente.Grupo)
                    .Add("@USUARIO", sUsuario)
                    .Add("@DATA", Data)
                    .Add("@OBSERVACAO", oCadastroComponente.Observacao)
                    .Add("@ID", oCadastroComponente.Id)
                    ;

                string sQuery = @"UPDATE [WSQOLPCP2PECACOMPONENTE]
                                  SET 
                                     [DESCRICAO_COMPONENTE] = ?
                                    ,[TIPO] = ?
                                    ,[ATIVO] = ?
                                    ,[GRUPO] = ?
                                    ,[USUARIO] = ?
                                    ,[DATA] = ?
                                    ,[OBSERVACAO] = ?
                                  WHERE
                                     [ID] = ? ";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.GetResultado();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }
        }

        public bool GetTipoComponente(int TipoComponente)
        {
            bool Result = true;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@TIPO", TipoComponente)
                    ;

                string sQuery = @"SELECT
                                    1
                                 FROM
                                    WSQOLPCP2PECACOMPONENTETIPO 
                                WHERE 
                                    CODIGO = ?";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.GetResultado();

                    if (oResult == null)
                        Result = false;
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }

            return Result;
        }

        public bool GetComponenteAlteracao(CadastroComponente oCadastroComponente)
        {
            bool Result = true;
            int Ativo = 1;


            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@CODIGO_PECA", oCadastroComponente.Material)
                    .Add("@DESCRICAO_COMPONENTE", oCadastroComponente.DescricaoComponente)
                    .Add("@TIPO", oCadastroComponente.Tipo)
                    .Add("@ATIVO", Ativo)
                    .Add("@GRUPO", oCadastroComponente.Grupo)
                    .Add("@OBSERVACAO", oCadastroComponente.Observacao)
                    ;

                string sQuery = @"SELECT 
                                      1 
                                  FROM 
                                      WSQOLPCP2PECACOMPONENTE 
                                  WHERE 
                                      CODIGO_PECA = ?
                                  AND
                                     DESCRICAO_COMPONENTE = ?
                                  AND
                                     TIPO = ?
                                  AND
	                                 ATIVO = ?
                                  AND
	                                 GRUPO = ?
                                  AND
	                                 OBSERVACAO = ?";

                try
                {

                    PrintLog.Verbose(oCommand.QueryToString() + "Query UPDATE: " + sQuery).Log();

                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.GetResultado();

                    if (oResult == null)
                        Result = false;

                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.GetForLog()
                        + Environment.NewLine, ex.InnerException);
                }
            }

            return Result;
        }

        public void DeleteComponente(CadastroComponente oCadastroComponente)
        {
            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                DateTime Data = DateTime.Now;

                oCommand
                    .Add("@ID", oCadastroComponente.Id)
                    ;

                string sQuery = @"DELETE FROM [WSQOLPCP2PECACOMPONENTE] 
                                  WHERE 
                                    CHAVE = ? 
                                  AND 
                                    USUARIO = ?";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.GetResultado();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }
        }
    }
}

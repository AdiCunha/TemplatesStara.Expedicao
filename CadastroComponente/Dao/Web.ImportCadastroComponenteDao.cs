using sqoClassLibraryAI0502VariaveisSistema;
using sqoTraceabilityStation;
using System;
using System.Data;


namespace TemplateStara.Expedicao.CadastroComponente.Dao
{
    public class ImportCadastroComponenteDao
    {
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

        public long GetIdComponente(string Material, string DescricaoComponente, int Tipo)
        {
            long IdComponente = -1;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@ID", IdComponente)
                    .Add("@CODIGO_PECA", Material)
                    .Add("@DESCRICAO_COMPONENTE", DescricaoComponente)
                    .Add("@TIPO", Tipo)
                    ;

                string sQuery = @"SELECT 
                                    ? = ID
                                  FROM 
                                    WSQOLPCP2PECACOMPONENTE 
                                  WHERE
                                    CODIGO_PECA = ? 
                                  AND 
                                    DESCRICAO_COMPONENTE = ? 
                                  AND 
                                    TIPO = ?";

                oCommand.Command.Parameters["@ID"].Direction = ParameterDirection.Output;

                try
                {
                    oCommand.SetCommandText(sQuery);

                    oCommand.Execute();

                    IdComponente = (long)oCommand.Command.Parameters["@ID"].Value;
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }

            return IdComponente;
        }

        public bool GetComponenteMaterial(LinhaPlanilha OLinhaPlanilha)
        {
            bool Result = true;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@CODIGO_PECA", OLinhaPlanilha.Material)
                    .Add("@DESCRICAO_COMPONENTE", OLinhaPlanilha.DescricaoComponente)
                    .Add("@TIPO", OLinhaPlanilha.TipoComponente)
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

        public bool GetDadosAlteracao(LinhaPlanilha OLinhaPlanilha)
        {
            bool Result = true;
            int Ativo = 1;


            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@CODIGO_PECA", OLinhaPlanilha.Material)
                    .Add("@DESCRICAO_COMPONENTE", OLinhaPlanilha.DescricaoComponente)
                    .Add("@TIPO", OLinhaPlanilha.TipoComponente)
                    .Add("@ATIVO", Ativo)
                    .Add("@GRUPO", OLinhaPlanilha.Grupo)
                    .Add("@OBSERVACAO", OLinhaPlanilha.Observacao)
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

        public bool GetGrupoExpedicao(string Grupo)
        {
            bool Result = true;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@GRUPO", Grupo)
                    ;

                string sQuery = @"SELECT
	                                  1 
                                  FROM 
	                                  WSQOLEXPEDICAOCOMPONENTEGRUPO
                                  WHERE 
	                                  NOME = ?";

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

        public void SaveComponente(LinhaPlanilha OLinhaPlanilha, string sUsuario)
        {
            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                DateTime Data = DateTime.Now;
                int Ativo = 1;


                oCommand
                    .Add("@CODIGO_PECA", OLinhaPlanilha.Material)
                    .Add("@DESCRICAO_COMPONENTE", OLinhaPlanilha.DescricaoComponente)
                    .Add("@TIPO", OLinhaPlanilha.TipoComponente)
                    .Add("@ATIVO", Ativo)
                    .Add("@GRUPO", OLinhaPlanilha.Grupo)
                    .Add("@USUARIO", sUsuario)
                    .Add("@DATA", Data)
                    .Add("@OBSERVACAO", OLinhaPlanilha.Observacao)
                    ;

                string sQuery = @"INSERT INTO [dbo].[WSQOLPCP2PECACOMPONENTE]
                                            ([CODIGO_PECA]
                                            ,[DESCRICAO_COMPONENTE]
                                            ,[TIPO]
                                            ,[ATIVO]
                                            ,[GRUPO]
                                            ,[USUARIO]
                                            ,[DATA]
                                            ,[OBSERVACAO])
                                   VALUES
                                        (?, ?, ?, ?, ?, ?, ?, ?)";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    oCommand.Execute();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }
        }
        
        public void UpdateComponente(LinhaPlanilha OLinhaPlanilha, string sUsuario)
        {
            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                DateTime Data = DateTime.Now;
                int Ativo = 1;

                oCommand
                    .Add("@DESCRICAO_COMPONENTE", OLinhaPlanilha.DescricaoComponente)
                    .Add("@TIPO", OLinhaPlanilha.TipoComponente)
                    .Add("@ATIVO", Ativo)
                    .Add("@GRUPO", OLinhaPlanilha.Grupo)
                    .Add("@USUARIO", sUsuario)
                    .Add("@DATA", Data)
                    .Add("@OBSERVACAO", OLinhaPlanilha.Observacao)
                    .Add("@CODIGO_PECA", OLinhaPlanilha.Material)
                    .Add("@DESCRICAO_COMPONENTE", OLinhaPlanilha.DescricaoComponente)
                    .Add("@TIPO", OLinhaPlanilha.TipoComponente)
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
                                     [CODIGO_PECA] = ? 
                                  AND
                                     [DESCRICAO_COMPONENTE] = ?
                                  AND
                                     [TIPO] = ?";

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

        public void DeleteComponenteImport(LinhaPlanilha OLinhaPlanilha)
        {
            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                DateTime Data = DateTime.Now;

                oCommand
                    .Add("@CODIGO_PECA", OLinhaPlanilha.Material)
                    .Add("@DESCRICAO_COMPONENTE", OLinhaPlanilha.DescricaoComponente)
                    .Add("@TIPO", OLinhaPlanilha.TipoComponente)
                    ;

                string sQuery = @"DELETE FROM
	                                WSQOLPCP2PECACOMPONENTE
                                  WHERE
	                                CODIGO_PECA = ?
                                  AND
	                                DESCRICAO_COMPONENTE = ?
                                  AND
	                                TIPO = ? ";

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

        public bool GetCadastroMaterial(string Material)
        {
            bool Result = false;

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

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.GetResultado();

                    if (oResult != null)
                        Result = true;
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }

            return Result;
        }

        public bool GetCadastroMaterialComponente(string Material, string Descricao, string Tipo)
        {
            bool Result = false;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@CODIGO_PECA", Material)
                    .Add("@DESCRICAO_COMPONENTE", Descricao)
                    .Add("@TIPO", Tipo)
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
	                                TIPO = ? ";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.GetResultado();

                    if (oResult != null)
                        Result = true;
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }

            return Result;
        }

    }
}

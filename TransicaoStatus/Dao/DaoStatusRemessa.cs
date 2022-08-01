using sqoClassLibraryAI0502VariaveisSistema;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using TemplateStara.Expedicao.TransicaoStatus.DataModel;

namespace TemplateStara.Expedicao.TransicaoStatus.Dao
{
    public class DaoStatusRemessa
    {
        public List<sqoClassStatusTransitions> GetStatusRemessa()
        {
            List<sqoClassStatusTransitions> oClassStatusTransitions;

            using (var oCommand = new sqoCommand())
            {
                oCommand
                    .Add("@ID", null, OleDbType.BigInt)
                    ;

                string sQuery = @"SELECT 
	                                  CURRENT_STATUS
	                                  ,NEXT_STATUS
	                                  ,CASE 
	                                        WHEN [RULE] = 'ALLOW' THEN 'true'
		                                    WHEN [RULE] = 'DENY'  THEN 'false'
	                                    ELSE 'N/A' END PERMITE
	                                  ,[MESSAGE] AS MENSAGEM
                                      ,[SCOPE] AS MODULO
                                  FROM
                                      WSQOLEXPREMESSASTATUSTRANSITIONS
                                  
                                  ORDER BY
	                                    CURRENT_STATUS
                                       ,NEXT_STATUS";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    oClassStatusTransitions = oCommand.GetListaResultado<sqoClassStatusTransitions>();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }

            return oClassStatusTransitions;
        }

        public List<sqoClassStatusTransitions> GetStatusRemessaItem()
        {
            List<sqoClassStatusTransitions> oClassStatusTransitions;

            using (var oCommand = new sqoCommand())
            {
                oCommand
                    .Add("@ID", null, OleDbType.BigInt)
                    ;

                string sQuery = @"SELECT 
	                                  CURRENT_STATUS
	                                  ,NEXT_STATUS
	                                  ,CASE 
	                                        WHEN [RULE] = 'ALLOW' THEN 'true'
		                                    WHEN [RULE] = 'DENY'  THEN 'false'
	                                    ELSE 'N/A' END PERMITE
	                                  ,[MESSAGE] AS MENSAGEM
                                      ,[SCOPE] AS MODULO
                                  FROM
                                      WSQOLEXPREMESSAITENSSTATUSTRANSITIONS

                                  ORDER BY
	                                  CURRENT_STATUS
                                     ,NEXT_STATUS";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    oClassStatusTransitions = oCommand.GetListaResultado<sqoClassStatusTransitions>();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }

            return oClassStatusTransitions;
        }

        public List<sqoClassStatusTransitions> GetStatusRemessaGrupo()
        {
            List<sqoClassStatusTransitions> oClassStatusTransitions;

            using (var oCommand = new sqoCommand())
            {
                oCommand
                    .Add("@ID", null, OleDbType.BigInt)
                    ;

                string sQuery = @"SELECT 
	                                  CURRENT_STATUS
	                                  ,NEXT_STATUS
	                                  ,CASE 
	                                        WHEN [RULE] = 'ALLOW' THEN 'true'
		                                    WHEN [RULE] = 'DENY'  THEN 'false'
	                                    ELSE 'N/A' END PERMITE
	                                  ,[MESSAGE] AS MENSAGEM
                                      ,[SCOPE] AS MODULO
                                  FROM
                                      WSQOLEXPEDICAOGRUPOREMESSASTATUSTRANSITIONS

                                  ORDER BY
	                                  CURRENT_STATUS
                                     ,NEXT_STATUS";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    oClassStatusTransitions = oCommand.GetListaResultado<sqoClassStatusTransitions>();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }

            return oClassStatusTransitions;
        }

        public List<sqoClassStatusTransitions> GetStatusRemessaVolume()
        {
            List<sqoClassStatusTransitions> oClassStatusTransitions;

            using (var oCommand = new sqoCommand())
            {
                oCommand
                    .Add("@ID", null, OleDbType.BigInt)
                    ;

                string sQuery = @"SELECT 
	                                  CURRENT_STATUS
	                                  ,NEXT_STATUS
	                                  ,CASE 
	                                        WHEN [RULE] = 'ALLOW' THEN 'true'
		                                    WHEN [RULE] = 'DENY'  THEN 'false'
	                                    ELSE 'N/A' END PERMITE
	                                  ,[MESSAGE] AS MENSAGEM
                                      ,[SCOPE] AS MODULO
                                  FROM
                                      WSQOLEXPEDICAOVOLUMESTATUSTRANSITIONS

                                  ORDER BY
	                                  CURRENT_STATUS
                                     ,NEXT_STATUS";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    oClassStatusTransitions = oCommand.GetListaResultado<sqoClassStatusTransitions>();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }

            return oClassStatusTransitions;
        }

        public void UpdateStatusRemessa(StatusTransitionsValues oStatusTransitionsValues, string Regra)
        {
            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@RULE", Regra)
                    .Add("@MESSAGE", oStatusTransitionsValues.Mensagem)
                    .Add("@CURRENT_STATUS", oStatusTransitionsValues.CurrentStatus)
                    .Add("@NEXT_STATUS", oStatusTransitionsValues.NextStatus)
                    ;

                string sQuery = @"UPDATE [WSQOLEXPREMESSASTATUSTRANSITIONS]
                                  SET [RULE] = ?
                                     ,[MESSAGE] = ?
                                      
	                              WHERE
                                      CURRENT_STATUS = ?
                                  AND
                                      NEXT_STATUS = ? ";

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

        public void UpdateStatusRemessaItem(StatusTransitionsValues oStatusTransitionsValues, string Regra)
        {
            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@RULE", Regra)
                    .Add("@MESSAGE", oStatusTransitionsValues.Mensagem)
                    .Add("@CURRENT_STATUS", oStatusTransitionsValues.CurrentStatus)
                    .Add("@NEXT_STATUS", oStatusTransitionsValues.NextStatus)
                    ;

                string sQuery = @"UPDATE [WSQOLEXPREMESSAITENSSTATUSTRANSITIONS]
                                  SET [RULE] = ?
                                     ,[MESSAGE] = ?
                                      
	                              WHERE
                                      CURRENT_STATUS = ?
                                  AND
                                      NEXT_STATUS = ? ";

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

        public void UpdateStatusRemessaGrupo(StatusTransitionsValues oStatusTransitionsValues, string Regra)
        {
            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@RULE", Regra)
                    .Add("@MESSAGE", oStatusTransitionsValues.Mensagem)
                    .Add("@CURRENT_STATUS", oStatusTransitionsValues.CurrentStatus)
                    .Add("@NEXT_STATUS", oStatusTransitionsValues.NextStatus)
                    ;

                string sQuery = @"UPDATE [WSQOLEXPEDICAOGRUPOREMESSASTATUSTRANSITIONS]
                                  SET [RULE] = ?
                                     ,[MESSAGE] = ?
                                      
	                              WHERE
                                      CURRENT_STATUS = ?
                                  AND
                                      NEXT_STATUS = ? ";

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

        public void UpdateStatusRemessaVolume(StatusTransitionsValues oStatusTransitionsValues, string Regra)
        {
            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@RULE", Regra)
                    .Add("@MESSAGE", oStatusTransitionsValues.Mensagem)
                    .Add("@CURRENT_STATUS", oStatusTransitionsValues.CurrentStatus)
                    .Add("@NEXT_STATUS", oStatusTransitionsValues.NextStatus)
                    ;

                string sQuery = @"UPDATE [WSQOLEXPEDICAOVOLUMESTATUSTRANSITIONS]
                                  SET [RULE] = ?
                                     ,[MESSAGE] = ?
                                      
	                              WHERE
                                      CURRENT_STATUS = ?
                                  AND
                                      NEXT_STATUS = ? ";

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

        public void InserirTransicaoStatusRemessa(StatusTransitionsInsert oStatusTransitionsInsert, string Regra)
        {
            if(Regra == "DENY" && string.IsNullOrEmpty(oStatusTransitionsInsert.Mensagem))
            {
                oStatusTransitionsInsert.Mensagem = "Contatar administração do sistema para alterar o cadastro.";
            }

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@CURRENT_STATUS", oStatusTransitionsInsert.CurrentStatus)
                    .Add("@NEXT_STATUS", oStatusTransitionsInsert.NextStatus)
                    .Add("@RULE", Regra)
                    .Add("@MESSAGE", oStatusTransitionsInsert.Mensagem)
                    .Add("@SCOPE", oStatusTransitionsInsert.Modulo)
                    ;

                string sQuery = @"INSERT INTO [WSQOLEXPREMESSASTATUSTRANSITIONS]
                                               ( [CURRENT_STATUS]
                                                ,[NEXT_STATUS]
                                                ,[RULE]
                                                ,[MESSAGE]
                                                ,[SCOPE])
                                  VALUES
                                           (?, ?, ?, ?, ?)";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.GetResultado();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Insert: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }
        }

        public void InserirTransicaoStatusItem(StatusTransitionsInsert oStatusTransitionsInsert, string Regra)
        {
            if (Regra == "DENY" && string.IsNullOrEmpty(oStatusTransitionsInsert.Mensagem))
            {
                oStatusTransitionsInsert.Mensagem = "Contatar administração do sistema para alterar o cadastro.";
            }

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@CURRENT_STATUS", oStatusTransitionsInsert.CurrentStatus)
                    .Add("@NEXT_STATUS", oStatusTransitionsInsert.NextStatus)
                    .Add("@RULE", Regra)
                    .Add("@MESSAGE", oStatusTransitionsInsert.Mensagem)
                    .Add("@SCOPE", oStatusTransitionsInsert.Modulo)
                    ;

                string sQuery = @"INSERT INTO [WSQOLEXPREMESSAITENSSTATUSTRANSITIONS]
                                               ( [CURRENT_STATUS]
                                                ,[NEXT_STATUS]
                                                ,[RULE]
                                                ,[MESSAGE]
                                                ,[SCOPE])
                                  VALUES
                                           (?, ?, ?, ?, ?)";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.GetResultado();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Insert: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }
        }

        public void InserirTransicaoStatusGrupo(StatusTransitionsInsert oStatusTransitionsInsert, string Regra)
        {
            if (Regra == "DENY" && string.IsNullOrEmpty(oStatusTransitionsInsert.Mensagem))
            {
                oStatusTransitionsInsert.Mensagem = "Contatar administração do sistema para alterar o cadastro.";
            }

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@CURRENT_STATUS", oStatusTransitionsInsert.CurrentStatus)
                    .Add("@NEXT_STATUS", oStatusTransitionsInsert.NextStatus)
                    .Add("@RULE", Regra)
                    .Add("@MESSAGE", oStatusTransitionsInsert.Mensagem)
                    .Add("@SCOPE", oStatusTransitionsInsert.Modulo)
                    ;

                string sQuery = @"INSERT INTO [WSQOLEXPEDICAOGRUPOREMESSASTATUSTRANSITIONS]
                                               ( [CURRENT_STATUS]
                                                ,[NEXT_STATUS]
                                                ,[RULE]
                                                ,[MESSAGE]
                                                ,[SCOPE])
                                  VALUES
                                           (?, ?, ?, ?, ?)";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.GetResultado();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Insert: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }
        }

        public void InserirTransicaoStatusVolume(StatusTransitionsInsert oStatusTransitionsInsert, string Regra)
        {
            if (Regra == "DENY" && string.IsNullOrEmpty(oStatusTransitionsInsert.Mensagem))
            {
                oStatusTransitionsInsert.Mensagem = "Contatar administração do sistema para alterar o cadastro.";
            }

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@CURRENT_STATUS", oStatusTransitionsInsert.CurrentStatus)
                    .Add("@NEXT_STATUS", oStatusTransitionsInsert.NextStatus)
                    .Add("@RULE", Regra)
                    .Add("@MESSAGE", oStatusTransitionsInsert.Mensagem)
                    .Add("@SCOPE", oStatusTransitionsInsert.Modulo)
                    ;

                string sQuery = @"INSERT INTO [WSQOLEXPEDICAOVOLUMESTATUSTRANSITIONS]
                                               ( [CURRENT_STATUS]
                                                ,[NEXT_STATUS]
                                                ,[RULE]
                                                ,[MESSAGE]
                                                ,[SCOPE])
                                  VALUES
                                           (?, ?, ?, ?, ?)";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.GetResultado();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Insert: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }
        }

        public bool ValidarTransicaoExistenteRemessa(int CurrentStatus, int NextStatus)
        {
            bool Result = false;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@CURRENT_STATUS", CurrentStatus)
                    .Add("@NEXT_STATUS", NextStatus)
                    ;

                string sQuery = @"SELECT
	                                 1
                                  FROM
                                     WSQOLEXPREMESSASTATUSTRANSITIONS
                                  WHERE
	                                 CURRENT_STATUS = ?
                                  AND
	                                 NEXT_STATUS = ?";

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

        public bool ValidarTransicaoExistenteItem(int CurrentStatus, int NextStatus)
        {
            bool Result = false;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@CURRENT_STATUS", CurrentStatus)
                    .Add("@NEXT_STATUS", NextStatus)
                    ;

                string sQuery = @"SELECT
	                                 1
                                  FROM
                                     WSQOLEXPREMESSAITENSSTATUSTRANSITIONS
                                  WHERE
	                                 CURRENT_STATUS = ?
                                  AND
	                                 NEXT_STATUS = ?";

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

        public bool ValidarTransicaoExistenteGrupo(int CurrentStatus, int NextStatus)
        {
            bool Result = false;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@CURRENT_STATUS", CurrentStatus)
                    .Add("@NEXT_STATUS", NextStatus)
                    ;

                string sQuery = @"SELECT
	                                 1
                                  FROM
                                     WSQOLEXPEDICAOGRUPOREMESSASTATUSTRANSITIONS
                                  WHERE
	                                 CURRENT_STATUS = ?
                                  AND
	                                 NEXT_STATUS = ?";

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

        public bool ValidarTransicaoExistenteVolume(int CurrentStatus, int NextStatus)
        {
            bool Result = false;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@CURRENT_STATUS", CurrentStatus)
                    .Add("@NEXT_STATUS", NextStatus)
                    ;

                string sQuery = @"SELECT
	                                 1
                                  FROM
                                     WSQOLEXPEDICAOVOLUMESTATUSTRANSITIONS
                                  WHERE
	                                 CURRENT_STATUS = ?
                                  AND
	                                 NEXT_STATUS = ?";

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

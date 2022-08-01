using sqoClassLibraryAI0502VariaveisSistema;
using System;
using System.Data;
using System.Data.OleDb;
using AI1627Common20.TemplateDebugging;

namespace sqoTraceabilityStation
{
    [TemplateDebug("sqoCadastroVolumeCommon")]
    class sqoCadastroVolumeCommon
    {
        /// <summary>
        /// Valida se o volume já existe
        /// </summary>
        /// <param name="sMaterial"></param>
        /// <param name="sCodigoVolume"></param>
        /// <returns></returns>
        public static string ValidateVolume(String sMaterial, String sCodigoVolume, String sTipoExpedicao, bool getResult)
        {
            String sMessage = String.Empty;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@CODIGO_VOLUME", sCodigoVolume, OleDbType.VarChar, 50)
                    .Add("@CODIGO_PAI", sMaterial, OleDbType.VarChar, 50)
                    .Add("@TIPO_EXPEDICAO", sTipoExpedicao, OleDbType.VarChar, 50)
                    ;

                String sQuery = @"SELECT 1 FROM WSQOPCP2PECAVOLUME WHERE CODIGO_VOLUME = @CODIGO_VOLUME AND CODIGO_PAI = @CODIGO_PAI
                                    AND TIPO_EXPEDICAO = @TIPO_EXPEDICAO";

                oCommand.SetCommandText(sQuery);

                try
                {
                    var oResult = oCommand.GetResultado();

                    if (oResult != null && getResult == false)
                    {
                        sMessage += " - Não é possível cadastrar volume " + sCodigoVolume + ", dados já cadastrados no sistema!" +
                             " Se necessário utilizar a função Alterar..." + Environment.NewLine;
                    }
                    else if (oResult == null && getResult == true)
                    {
                        sMessage += " - Não é possível alterar volume " + sCodigoVolume + ", dados não cadastrados no sistema!" +
                             " Se necessário utilizar a função Inserir..." + Environment.NewLine;
                    }

                }
                catch (Exception ex)
                {
                    sMessage += ex.Message + Environment.NewLine + oCommand.GetForLog() + Environment.NewLine;
                }
            }

            return sMessage;

        }

        public static string ValidateVolumeType(string CodigoVolume)
        {
            string ErrorMessage = "";
            string TipoPeca = "MVOL";

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@CODIGO_PECA", CodigoVolume)
                    .Add("@TIPO_PECA", TipoPeca)
                    ;

                string sQuery = @"SELECT 
                                    TOP 1 *
                                  FROM
	                                WSQOPCP2PECA AS PEC
                                  WHERE
	                                PEC.CODIGO_PECA = ?
                                  AND
	                                PEC.TIPO_PECA <> ?";

                oCommand.SetCommandText(sQuery);

                try
                {
                    var oResult = oCommand.GetResultado();

                    if (oResult != null)
                    {
                        ErrorMessage += " - Impossível vincular Volume com tipo diferente de: " + TipoPeca + ". " + "Código do volume: " + CodigoVolume + Environment.NewLine;
                    }

                }
                catch (Exception ex)
                {
                    ErrorMessage += ex.Message + Environment.NewLine + oCommand.GetForLog() + Environment.NewLine;
                }
            }


            return ErrorMessage;
        }

        public static string ValidateParentVolume(String sMaterial, String sCodigoVolume, String Action)
        {
            String sMessage = String.Empty;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@CODIGO_PAI", sMaterial, OleDbType.VarChar, 50)
                    .Add("@CODIGO_VOLUME", sMaterial, OleDbType.VarChar, 50)
                    ;

                String sQuery = @"SELECT 1 FROM WSQOPCP2PECAVOLUME WHERE CODIGO_PAI = @CODIGO_PAI AND CODIGO_VOLUME = @CODIGO_VOLUME";

                oCommand.SetCommandText(sQuery);

                try
                {
                    var oResult = oCommand.GetResultado();

                    if (oResult == null || oResult is DBNull)
                    {
                        String sMessageAction = Action.Equals("Link") ? "vincular o volume " + sCodigoVolume +
                            " ao material " + sMaterial : "cadastrar o volume " + sCodigoVolume;

                        sMessage = " - Não é possível " + sMessageAction + ", dados do código pai "
                           + sMaterial + " ainda não cadastrado como volume na base de dados!" + Environment.NewLine;
                    }

                }
                catch (Exception ex)
                {
                    sMessage += ex.Message + Environment.NewLine + oCommand.GetForLog() + Environment.NewLine;
                }
            }

            return sMessage;
        }

        public static bool ExistPeca(String sMaterial)
        {
            bool bResult = false;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@MATERIAL", sMaterial, OleDbType.VarChar, 50);

                String sQuery = @"SELECT 1 FROM WSQOPCP2PECA WHERE CODIGO_PECA = @MATERIAL";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.GetResultado();

                    if (oResult != null)
                        bResult = true;
                }
                catch (Exception ex)
                {
                    throw new Exception(
                    ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }
            }

            return bResult;
        }
    }
}
using sqoClassLibraryAI0502VariaveisSistema;
using System;
using System.Data;

namespace TemplateStara.Expedicao.AlterarSituacaoItemRemessa.Dao
{
    public class AlterarSituacaoItemRemessaDao
    {
        public void SetSituacaoItemRemessa(int Id)
        {
            string SituacaoE = "E";

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand.SetCommandText(@"UPDATE
                                            WSQOLEXPREMESSAITENS
                                          SET 
                                            SITUACAO = ?
                                          WHERE 
	                                         ID = ?")
                .Add("@SITUACAO", SituacaoE)
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

        public void SetSituacaoItemRemessaHist(string Usuario, string Observacao, int IdHist)
        {
            if (string.IsNullOrEmpty(Observacao))
            {
                Observacao = "Alterada situação do item da remessa via tela web";
            }

            DateTime DataAtual = DateTime.Now;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand.SetCommandText(@"UPDATE 
	                                        WSQOLEXPREMESSAITENSHIST
                                          SET
	                                         USUARIO_ULTIMA_MOVIMENTACAO = ?
                                            ,OBSERVACAO = ?
                                            ,DATA_ULTIMA_MOVIMENTACAO = ?
                                          WHERE
	                                        ID = ?")

                .Add("@USUARIO_ULTIMA_MOVIMENTACAO", Usuario)
                .Add("@OBSERVACAO", Observacao)
                .Add("@DATA_ULTIMA_MOVIMENTACAO", DataAtual)
                .Add("@ID", IdHist)
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

        public int GetIdHist(int IdItem)
        {
            string sQuery = @"SELECT
	                            TOP 1
	                            ID
                              FROM 
	                          WSQOLEXPREMESSAITENSHIST
                              WHERE
	                            ID_REMESSA_ITENS = ?
                              ORDER BY
	                            ID DESC";

            using (var oCommand = new sqoCommand(CommandType.Text))
            {

                oCommand.SetCommandText(sQuery);

                oCommand.Add("@ID_REMESSA_ITENS", IdItem);

                var oResult = oCommand.GetResultado();

                if (oResult == null)
                    oResult = 0;

                return Convert.ToInt32(oResult);
            }
        }
    }
}

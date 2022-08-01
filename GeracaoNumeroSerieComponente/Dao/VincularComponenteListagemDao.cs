using AI1627Common20.Log;
using sqoClassLibraryAI0502VariaveisSistema;
using System;
using System.Collections.Generic;
using System.Data;
using TemplateStara.Expedicao.GeracaoNumeroSerieComponente.DataModel;

namespace TemplateStara.Expedicao.GeracaoNumeroSerieComponente.Dao
{
   public class VincularComponenteListagemDao
    {
        public List<DocumentoReferenciaListagemList> GetRastreabilidadeComponente(DocumentoReferenciaListagem oDocumentoReferenciaListagem)
        {
            int TipoRastreabilidade = 20;

            int Ativo = 1;

            string GrupoTodos = oDocumentoReferenciaListagem.Grupo.Replace("TODOS", "");

            oDocumentoReferenciaListagem.Grupo = GrupoTodos;

            List<DocumentoReferenciaListagemList> oDocumentoReferenciaListagemList;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@DOC_REFERENCIA", oDocumentoReferenciaListagem.DocReferencia)
                    .Add("@CODIGO_PECA", oDocumentoReferenciaListagem.Material)
                    .Add("@TIPO", TipoRastreabilidade)
                    .Add("@ATIVO", Ativo)
                    .Add("@GRUPO", oDocumentoReferenciaListagem.Grupo)
                    .Add("@GRUPO", oDocumentoReferenciaListagem.Grupo)
                    .Add("@ID_GERACAO", oDocumentoReferenciaListagem.IdGeracao)
                    ;

                string sQuery = @"SELECT 
                                     ISNULL(C.ID, 0) AS ID
	                                ,C.CODIGO_PECA AS MATERIAL_LIST
                                    ,S.DOC_REFERENCIA AS DOCUMENTO_REFERENCIA
	                                ,C.DESCRICAO_COMPONENTE
	                                ,I.VALOR
	                                ,S.ID AS ID_GERACAO
                                    ,I.ID AS ID_ITEM

                                  FROM
	                                  WSQOLPCP2PECACOMPONENTE AS C

                                  LEFT JOIN
	                                  WSQOLEXPEDICAOCOMPONENTEGERACAONUMEROSERIE AS S
                                  ON
	                                  C.CODIGO_PECA = S.CODIGO_PECA
                                  AND
	                                  S.DOC_REFERENCIA = ?

                                  LEFT JOIN
	                                  WSQOLEXPEDICAOCOMPONENTEGERACAONUMEROSERIEITEM AS I
                                  ON
	                                  I.ID_GERACAO = S.ID
                                  AND
	                                  I.DESCRICAO = C.DESCRICAO_COMPONENTE

                                  WHERE
	                                  C.CODIGO_PECA = ?
                                  AND
	                                  C.TIPO = ?
                                  AND
                                      C.ATIVO = ?
                                  AND 
	                                 (C.GRUPO = CASE WHEN ? = '' THEN C.GRUPO ELSE ? END)
                                  AND
                                     S.ID = ?";

                try
                {
                    var query = oCommand.SetCommandText(sQuery);

                    var Teste = oCommand.QueryToString();

                    PrintLog.Verbose( "Log da Query: " + oCommand.QueryToString()).Log();

                    oDocumentoReferenciaListagemList = oCommand.GetListaResultado<DocumentoReferenciaListagemList>();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }

                return oDocumentoReferenciaListagemList;
            }
        }

        public List<DocumentoReferenciaListagemList> GetRastreabilidadeComponenteTipoDez(DocumentoReferenciaListagem oDocumentoReferenciaListagem)
        {
            int TipoRastreabilidade = 10;

            string GrupoTodos = oDocumentoReferenciaListagem.Grupo.Replace("TODOS", "");

            oDocumentoReferenciaListagem.Grupo = GrupoTodos;

            List<DocumentoReferenciaListagemList> oDocumentoReferenciaListagemList;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@DOC_REFERENCIA", oDocumentoReferenciaListagem.DocReferencia)
                    .Add("@CODIGO_PECA", oDocumentoReferenciaListagem.Material)
                    .Add("@TIPO", TipoRastreabilidade)
                    .Add("@GRUPO", oDocumentoReferenciaListagem.Grupo)
                    .Add("@GRUPO", oDocumentoReferenciaListagem.Grupo)
                    .Add("@ID_GERACAO", oDocumentoReferenciaListagem.IdGeracao)
                    ;

                string sQuery = @"SELECT 
	                                    ISNULL(C.ID, 0) AS ID
	                                    ,C.CODIGO_PECA AS MATERIAL_LIST
	                                    ,S.DOC_REFERENCIA AS DOCUMENTO_REFERENCIA
	                                    ,C.DESCRICAO_COMPONENTE
	                                    ,I.VALOR
	                                    ,S.ID AS ID_GERACAO
	                                    ,I.ID AS ID_ITEM

                                 FROM
	                                   WSQOLPCP2PECACOMPONENTE AS C

                                 LEFT JOIN
	                                   WSQOLEXPEDICAOCOMPONENTEGERACAONUMEROSERIE AS S
                                 ON
	                                C.CODIGO_PECA = S.CODIGO_PECA
                                 AND
	                                S.DOC_REFERENCIA = ?

                                 LEFT JOIN
	                                WSQOLEXPEDICAOCOMPONENTEGERACAONUMEROSERIEITEM AS I
                                 ON
	                                I.ID_GERACAO = S.ID
                                 AND
	                                I.DESCRICAO = C.DESCRICAO_COMPONENTE

                                 WHERE
	                                C.CODIGO_PECA = ?
                                 AND
	                                C.TIPO = ?
                                 AND 
	                               (C.GRUPO = CASE WHEN ? = '' THEN C.GRUPO ELSE ? END)
                                 AND
	                               S.ID = ?";

                try
                {
                    var query = oCommand.SetCommandText(sQuery);

                    var Teste = oCommand.QueryToString();

                    PrintLog.Verbose("Log da Query: " + oCommand.QueryToString()).Log();

                    oDocumentoReferenciaListagemList = oCommand.GetListaResultado<DocumentoReferenciaListagemList>();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }

                return oDocumentoReferenciaListagemList;
            }
        }

        public List<DocumentoReferenciaListagemList> GetRastreabilidadeComponenteNaoGerado(DocumentoReferenciaListagem oDocumentoReferenciaListagem)
        {
            int TipoRastreabilidade = 20;

            int Ativo = 1;

            string GrupoTodos = oDocumentoReferenciaListagem.Grupo.Replace("TODOS", "");

            oDocumentoReferenciaListagem.Grupo = GrupoTodos;

            List<DocumentoReferenciaListagemList> oDocumentoReferenciaListagemList;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@DOC_REFERENCIA", oDocumentoReferenciaListagem.DocReferencia)
                    .Add("@CODIGO_PECA", oDocumentoReferenciaListagem.Material)
                    .Add("@TIPO", TipoRastreabilidade)
                    .Add("@ATIVO", Ativo)
                    .Add("@GRUPO", oDocumentoReferenciaListagem.Grupo)
                    .Add("@GRUPO", oDocumentoReferenciaListagem.Grupo)
                    ;

                string sQuery = @"SELECT 
                                     ISNULL(C.ID, 0) AS ID
	                                ,C.CODIGO_PECA AS MATERIAL_LIST
                                    ,S.DOC_REFERENCIA AS DOCUMENTO_REFERENCIA
	                                ,C.DESCRICAO_COMPONENTE
	                                ,'' AS VALOR
	                                ,0 AS ID_GERACAO
                                    ,0 AS ID_ITEM

                                  FROM
	                                  WSQOLPCP2PECACOMPONENTE AS C

                                  LEFT JOIN
	                                  WSQOLEXPEDICAOCOMPONENTEGERACAONUMEROSERIE AS S
                                  ON
	                                  C.CODIGO_PECA = S.CODIGO_PECA
                                  AND
	                                  S.DOC_REFERENCIA = ?

                                  LEFT JOIN
	                                  WSQOLEXPEDICAOCOMPONENTEGERACAONUMEROSERIEITEM AS I
                                  ON
	                                  I.ID_GERACAO = S.ID
                                  AND
	                                  I.DESCRICAO = C.DESCRICAO_COMPONENTE

                                  WHERE
	                                  C.CODIGO_PECA = ?
                                  AND
	                                  C.TIPO = ?
                                  AND
                                      C.ATIVO = ?
                                  AND 
	                                 (C.GRUPO = CASE WHEN ? = '' THEN C.GRUPO ELSE ? END)";

                try
                {
                    var query = oCommand.SetCommandText(sQuery);

                    var Teste = oCommand.QueryToString();

                    PrintLog.Verbose("Log da Query: " + oCommand.QueryToString()).Log();

                    oDocumentoReferenciaListagemList = oCommand.GetListaResultado<DocumentoReferenciaListagemList>();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }

                return oDocumentoReferenciaListagemList;
            }
        }
    }
}

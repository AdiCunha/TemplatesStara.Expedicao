using AI1627Common20.TemplateDebugging;
using Common.Stara.Common.Business;
using sqoClassLibraryAI0502Biblio;
using sqoClassLibraryAI0502VariaveisSistema;
using sqoClassLibraryAI1151FilaProducao;
using sqoClassLibraryAI1151FilaProducao.Estrutura;
using sqoClassLibraryAI1151FilaProducao.Process;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;

namespace TelasDinamicas.Expedicao
{
    [TemplateDebug("Web.Expedicao.GeracaoVolumeRecompraListagem")]
    public class GeracaoVolumeRetrabalhoPaBusiness : IProcessBuscaDadosAuxiliaresProducao
    {
        private GeracaoVolumeRetrabalho oGeracaoVolumeRetrabalho;

        public string Executar(string sAction, string sXmlDados, string sXmlType, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao, List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sNivel, string sUsuario, object oObjAux)
        {
            String sXml = String.Empty;

            this.Init(sXmlDados);

            sXml = this.ProcessBusinessLogic();

            return sXml;
        }

        private void Init(String sXmlDados)
        {
            this.oGeracaoVolumeRetrabalho = new GeracaoVolumeRetrabalho();

            oGeracaoVolumeRetrabalho = sqoClassBiblioSerDes.DeserializeObject<GeracaoVolumeRetrabalho>(sXmlDados);
        }

        private String ProcessBusinessLogic()
        {
            try
            {
                String sXml = String.Empty;

                sXml = this.CreateXml();

                return sXml;
            }
            catch (Exception ex)
            {
                sqoClassMessageUserException oClassMessageUserException =
                   new sqoClassMessageUserException("Error " + Environment.NewLine + ex.Message, ex.InnerException);

                throw oClassMessageUserException;
            }
        }

        private String CreateXml()
        {
            List<GeracaoVolumeRetrabalhoPa> oListGeracaoVolumeRetrabalho = GeracaoVolumeRetrabalhoPaPersistence.GetGeracaoVolumeRetrabalho(this.oGeracaoVolumeRetrabalho.NrSerie);

            return this.SetXml(oListGeracaoVolumeRetrabalho);
        }

        private String SetXml(List<GeracaoVolumeRetrabalhoPa> oListGeracaoVolumeRetrabalho)
        {
            String sXmlResult = String.Empty;

            var oDetail = new sqoClassDetails();

            foreach (var oGeracaoVolumeRetrabalho in oListGeracaoVolumeRetrabalho)
            {
                oDetail.Add(oGeracaoVolumeRetrabalho);
            }

            var oXml = oDetail.Serializar();

            return oXml;
        }
    }

    internal class GeracaoVolumeRetrabalhoPaPersistence
    {
        public static List<GeracaoVolumeRetrabalhoPa> GetGeracaoVolumeRetrabalho(String sNrSerie)
        {
            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                String sQuery = @"SELECT
                                	ESTOQ.LOCAL_ORIGEM_MOV
                                	,ESTOQ.LOCAL
                                	,ESTOQ.CODIGO_PRODUTO
                                	,ESTOQ.CODIGO_RASTREABILIDADE 
                                	,CONVERT(VARCHAR(30),ESTOQ.DATA_ULTIMA_MOVIMENTACAO, 105)
		                                + ' ' + CONVERT( VARCHAR(30), ESTOQ.DATA_ULTIMA_MOVIMENTACAO ,108 ) AS DATA_ULTIMA_MOVIMENTACAO
                                	,ESTOQ.USUARIO_ULTIMA_MOVIMENTACAO
                                	,ESTOQ.STATUS_MOVIMENTACAO
                                	,ESTOQ.MENSAGEM_STATUS_MOVIMENTACAO
                                FROM
                                	WSQOLESTOQUEATUAL AS ESTOQ
                                INNER JOIN
                                	WSQOLEXPEDICAOVOLUME AS VOL
                                ON
                                	VOL.CODIGO_RASTREABILIDADE = ESTOQ.CODIGO_RASTREABILIDADE
                                --AND VOL.TIPO_VOLUME = 2
                                AND ISNULL(VOL.TIPO_EXPEDICAO,'') = ''
                                WHERE
                                	ESTOQ.QUANTIDADE > 0
                                AND VOL.ORDEM_PRODUCAO = @NR_SERIE";

                try
                {
                    oCommand
                        .Add("@NR_SERIE", sNrSerie, OleDbType.VarChar, 50)
                        ;

                    oCommand.SetCommandText(sQuery);

                    return oCommand.GetListaResultado<GeracaoVolumeRetrabalhoPa>();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }
            }
        }
    }

    [AutoPersistencia]
    public class GeracaoVolumeRetrabalhoPa : sqoClassItemDetailBase
    {
        public String LocalOrigemMov { get; set; }

        public String Local { get; set; }

        public String CodigoProduto { get; set; }

        public String CodigoRastreabilidade { get; set; }

        public String DataUltimaMovimentacao { get; set; }

        public String UsuarioUltimaMovimentacao { get; set; }

        public String StatusMovimentacao { get; set; }

        public String MensagemStatusMovimentacao { get; set; }
    }

}
using AI1627Common20.TemplateDebugging;
using sqoClassLibraryAI0502Message;
using sqoClassLibraryAI0502VariaveisSistema;
using sqoClassLibraryAI1151FilaProducao;
using sqoClassLibraryAI1151FilaProducao.Estrutura;
using sqoClassLibraryAI1151FilaProducao.Process;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using TemplatesStara.CommonStara;


namespace sqoTraceabilityStation
{
    [TemplateDebug("sqoImportCadastroVolume")]
    class sqoImportCadastroVolume : sqoClassProcessMovimentacao
    {
        private class LinhaPlanilha
        {
            [PlanilhaColunaInfo(1, "MATERIAL")]
            public string Material { get; set; }

            [PlanilhaColunaInfo(2, "CODIGO_VOLUME")]
            public string CodigoVolume { get; set; }

            [PlanilhaColunaInfo(3, "DESCRICAO_VOLUME")]
            public string DescricaoVolume { get; set; }

            [PlanilhaColunaInfo(4, "QUANTIDADE")]
            public string Quantidade { get; set; }

            [PlanilhaColunaInfo(5, "PESO_LIQUIDO")]
            public string PesoLiquido { get; set; }

            [PlanilhaColunaInfo(6, "PESO_BRUTO")]
            public string PesoBruto { get; set; }

            [PlanilhaColunaInfo(7, "ALTURA")]
            public string Altura { get; set; }

            [PlanilhaColunaInfo(8, "LARGURA")]
            public string Largura { get; set; }

            [PlanilhaColunaInfo(9, "COMPRIMENTO")]
            public string Comprimento { get; set; }

            [PlanilhaColunaInfo(10, "CODIGO_IMAGEM")]
            public string CodigoImagem { get; set; }

            [PlanilhaColunaInfo(11, "TIPO_EXPEDICAO")]
            public string TipoExpedicao { get; set; }

            [PlanilhaColunaInfo(12, "OPERACAO")]
            public string Operacao { get; set; }

            [PlanilhaColunaInfo(13, "ATIVO")]
            public string Ativo { get; set; }

            public int LineNumber { get; set; }

            public long Id { get; set; }

        }

        private List<LinhaPlanilha> oPlanilha = new List<LinhaPlanilha>();

        private string sFileName = "";
        private string sFilePath = "";

        public override sqoClassMessage Executar(string sAction, string sXmlDados, string sXmlType, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao, List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sNivel, string sUsuario, object oObjAux)
        {
            sqoClassSetMessageDefaults oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());

            using (var oDBConnection = new sqoClassDbConnection())
            {
                switch (sAction)
                {
                    case "ImportPlanilha":

                        CarregarParametrosMov(oListaParametrosMovimentacao);

                        oPlanilha = CommonStara.CarregarPlanilha<LinhaPlanilha>(sNivel, sFileName);

                        this.ValidarLinhasPlanilha(oPlanilha, oClassSetMessageDefaults);

                        this.Save(oDBConnection, oPlanilha, sUsuario, sNivel);

                        oClassSetMessageDefaults.SetarOk();

                        break;

                    case "DownloadExcel":
                        oClassSetMessageDefaults.Message.Dado = CommonStara.DownloadExcel<LinhaPlanilha>();
                        oClassSetMessageDefaults.SetarOk();
                        break;

                    case "DownloadPDF":
                        {
                            CarregarParametrosList(oListaParametrosListagem);

                            CommonStara.DownloadPdf(sFilePath, oClassSetMessageDefaults);

                            CommonStara.MessageBox(true, "PDF gerado com sucesso!", "", sqoClassMessage.MessageTypeEnum.OK, oClassSetMessageDefaults);

                            break;
                        }

                    default:
                        break;
                }

            }

            return oClassSetMessageDefaults.Message;
        }

        private void CarregarParametrosMov(List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao)
        {
            this.sFileName = oListaParametrosMovimentacao.Find(x => x.Campo == "Arquivo").Valor;
        }

        private void CarregarParametrosList(List<sqoClassParametrosEstrutura> oListaParametrosListagem)
        {
            this.sFilePath = oListaParametrosListagem.Find(x => x.Campo == "FilePath").Valor;
        }

        private void ValidarLinhasPlanilha(List<LinhaPlanilha> Planilha, sqoClassSetMessageDefaults oClassSetMessageDefaults)
        {
            string sMensagemErro = "";

            foreach (var oLinha in Planilha)
            {
                sMensagemErro += ValidarPreenchimentoColunas(oLinha);

                if (oLinha.Operacao.ToUpper() == "I")
                {
                    String sMessageValidateVolume;

                    sMessageValidateVolume = sqoCadastroVolumeCommon.ValidateVolume(oLinha.Material, oLinha.CodigoVolume, oLinha.TipoExpedicao, false);

                    if (!String.IsNullOrEmpty(sMessageValidateVolume))
                    {
                        sMensagemErro += sMessageValidateVolume 
                            + String.Format("Linha: {0} \n", oLinha.LineNumber);
                    }

                    if (oLinha.Material != oLinha.CodigoVolume)
                    {
                        String sMessageValidateParentVolume = "";
                        String sMessageValidateParentVolumeSheet = "";

                        sMessageValidateParentVolume = sqoCadastroVolumeCommon.ValidateParentVolume
                            (oLinha.Material, oLinha.CodigoVolume, "Insert");

                        if (!String.IsNullOrEmpty(sMessageValidateParentVolume))
                        {
                            sMessageValidateParentVolume += String.Format("Linha: {0}\n", oLinha.LineNumber);

                            sMessageValidateParentVolumeSheet = ValidateParentVolumeSheet(oPlanilha, oLinha);

                            if (!String.IsNullOrEmpty(sMessageValidateParentVolumeSheet))
                                sMensagemErro += sMessageValidateParentVolume + sMessageValidateParentVolumeSheet;
                        }
                    }

                    sMensagemErro += ValidateExpeditionType(oLinha.TipoExpedicao, oLinha.LineNumber.ToString());

                }
                else if (String.IsNullOrEmpty(sMensagemErro) && oLinha.Operacao.ToUpper() == "A")
                {
                    String sMessageValidateVolume;

                    sMessageValidateVolume = sqoCadastroVolumeCommon.ValidateVolume(oLinha.Material, oLinha.CodigoVolume, oLinha.TipoExpedicao, true);

                    if (!String.IsNullOrEmpty(sMessageValidateVolume))
                    {
                        sMensagemErro += sMessageValidateVolume + String.Format("/nLinha: {0}/n", oLinha.LineNumber);
                    }

                    sMensagemErro += ValidateExpeditionType(oLinha.TipoExpedicao, oLinha.LineNumber.ToString());
                }

            }

            if (!String.IsNullOrEmpty(sMensagemErro))
            {
                CommonStara.MessageBox(false, "Falha na validação de dados", sMensagemErro, sqoClassMessage.MessageTypeEnum.ERROR, oClassSetMessageDefaults);

                throw new sqoClassMessageUserException(oClassSetMessageDefaults.Message);
            }
        }


        private string ValidarPreenchimentoColunas(LinhaPlanilha oLinha)
        {
            string sMensagemErro = "";

            foreach (var item in oLinha.GetType().GetProperties())
            {
                if (item != null)
                {
                    var oValue = item.GetValue(oLinha);

                    if (oValue != null)
                    {
                        if (oValue.ToString().StartsWith(" ") || oValue.ToString().EndsWith(" "))
                        {
                            var oAtributo = item.GetCustomAttributes(typeof(PlanilhaColunaInfoAttribute), true);
                            var oPlanilhaColunaInfo = (PlanilhaColunaInfoAttribute)oAtributo.FirstOrDefault();

                            if (oPlanilhaColunaInfo != null && !String.IsNullOrEmpty(oPlanilhaColunaInfo.NomeColuna))
                                sMensagemErro += String.Format("A coluna {0} possui espaços em branco.\nLinha: {1}\n", oPlanilhaColunaInfo.NomeColuna, oLinha.LineNumber);
                        }
                    }
                }
            }

            if (String.IsNullOrEmpty(oLinha.Material))
            {
                sMensagemErro += String.Format(
                    "A coluna Material é obrigatória! MATERIAL: {0}.\nLinha: {1}\n"
                    , oLinha.Material
                    , oLinha.LineNumber
                    );
            }

            if (String.IsNullOrEmpty(oLinha.CodigoVolume))
            {
                sMensagemErro += String.Format(
                    "A coluna Código Volume é obrigatória! MATERIAL: {0} CODIGO_VOLUME: {1}.\nLinha: {2}\n"
                    , oLinha.Material
                    , oLinha.CodigoVolume
                    , oLinha.LineNumber
                    );
            }

            if (String.IsNullOrEmpty(oLinha.DescricaoVolume) && oLinha.Material != oLinha.CodigoVolume)
            {
                sMensagemErro += String.Format("A coluna Descrição Volume é obrigatória e deve ser preenchida! MATERIAL: {0} CODIGO_VOLUME: {1}" +
                    " CODIGO_VOLUME: {2}.\nLinha: {3}\n"
                    , oLinha.Material
                    , oLinha.CodigoVolume
                    , oLinha.DescricaoVolume
                    , oLinha.LineNumber);
            }

            if (String.IsNullOrEmpty(oLinha.Quantidade))
            {
                sMensagemErro += String.Format("A coluna Quantidade é obrigatória e deve ser preenchida! MATERIAL: {0} CODIGO_VOLUME: {1}" +
                    " QUANTIDADE: {2}.\nLinha: {3}\n"
                    , oLinha.Material
                    , oLinha.CodigoVolume
                    , oLinha.Quantidade
                    , oLinha.LineNumber);
            }
            else
            {
                int nQuantidade;

                bool resultQuantidade = Int32.TryParse(oLinha.Quantidade.ToString(), out nQuantidade);

                if (!resultQuantidade)
                {
                    sMensagemErro += String.Format("A coluna Quantidade deve ser preenchida com um número inteiro!" +
                        " MATERIAL: {0} CODIGO_VOLUME: {1} QUANTIDADE: {2}.\nLinha: {3}\n",
                        oLinha.Material
                        , oLinha.CodigoVolume
                        , oLinha.Quantidade
                        , oLinha.LineNumber);
                }
            }

            if (String.IsNullOrEmpty(oLinha.PesoLiquido))
            {
                sMensagemErro += String.Format("A coluna Peso Líquido é obrigatória e deve ser preenchida! MATERIAL: {0} CODIGO_VOLUME: {1}" +
                    " PESO_LIQUIDO: {2}.\nLinha: {3}\n"
                    , oLinha.Material
                    , oLinha.CodigoVolume
                    , oLinha.PesoLiquido
                    , oLinha.LineNumber);
            }
            else
            {
                double nPesoLiquido;

                bool resultPesoLiquido = Double.TryParse(oLinha.PesoLiquido, out nPesoLiquido);

                if (!resultPesoLiquido)
                {
                    sMensagemErro += String.Format("A coluna Peso Líquido deve ser preenchida com valor numérico!" +
                        " MATERIAL: {0} CODIGO_VOLUME: {1} PESO_LIQUIDO: {2}.\nLinha: {3}\n",
                        oLinha.Material
                        , oLinha.CodigoVolume
                        , oLinha.PesoLiquido
                        , oLinha.LineNumber);
                }
            }

            if (String.IsNullOrEmpty(oLinha.PesoBruto))
            {
                sMensagemErro += String.Format("A coluna Peso Bruto é obrigatória e deve ser preenchida! MATERIAL: {0} CODIGO_VOLUME: {1}" +
                    " PESO_BRUTO: {2}.\nLinha: {3}\n"
                    , oLinha.Material
                    , oLinha.CodigoVolume
                    , oLinha.PesoBruto
                    , oLinha.LineNumber);
            }
            else
            {
                double nPesoBruto;

                bool resultPesoBruto = Double.TryParse(oLinha.PesoBruto, out nPesoBruto);

                if (!resultPesoBruto)
                {
                    sMensagemErro += String.Format("A coluna Peso Bruto deve ser preenchida com valor numérico!" +
                        " MATERIAL: {0} CODIGO_VOLUME: {1} PESO_BRUTO: {2}.\nLinha: {3}\n",
                        oLinha.Material
                        , oLinha.CodigoVolume
                        , oLinha.PesoBruto
                        , oLinha.LineNumber);
                }
            }

            if (String.IsNullOrEmpty(oLinha.Altura))
            {
                sMensagemErro += String.Format("A coluna Altura é obrigatória e deve ser preenchida! MATERIAL: {0} CODIGO_VOLUME: {1}" +
                    " ALTURA: {2}.\nLinha: {3}\n"
                    , oLinha.Material
                    , oLinha.CodigoVolume
                    , oLinha.Altura
                    , oLinha.LineNumber);

            }
            else
            {
                double nAltura;

                bool resultAltura = Double.TryParse(oLinha.Altura, out nAltura);

                if (!resultAltura)
                {
                    sMensagemErro += String.Format("A coluna Altura deve ser preenchida com valor numérico!" +
                        " MATERIAL: {0} CODIGO_VOLUME: {1} ALTURA: {2}.\nLinha: {3}\n",
                        oLinha.Material
                        , oLinha.CodigoVolume
                        , oLinha.Altura
                        , oLinha.LineNumber);
                }
            }


            if (String.IsNullOrEmpty(oLinha.Largura))
            {
                sMensagemErro += String.Format("A coluna Largura é obrigatória e deve ser preenchida! MATERIAL: {0} CODIGO_VOLUME: {1}" +
                    " LARGURA: {2}.\nLinha: {3}\n"
                    , oLinha.Material
                    , oLinha.CodigoVolume
                    , oLinha.Largura
                    , oLinha.LineNumber);
            }
            else
            {
                double nLargura;

                bool resultLargura = Double.TryParse(oLinha.Largura, out nLargura);

                if (!resultLargura)
                {
                    sMensagemErro += String.Format("A coluna Largura deve ser preenchida com valor numérico!" +
                        " MATERIAL: {0} CODIGO_VOLUME: {1} LARGURA: {2}.\nLinha: {3}\n",
                        oLinha.Material
                        , oLinha.CodigoVolume
                        , oLinha.Largura
                        , oLinha.LineNumber);
                }
            }


            if (String.IsNullOrEmpty(oLinha.Comprimento))
            {
                sMensagemErro += String.Format("A coluna Comprimento é obrigatória e deve ser preenchida! MATERIAL: {0} CODIGO_VOLUME: {1}" +
                    " COMPRIMENTO: {2}.\nLinha: {3}\n"
                    , oLinha.Material
                    , oLinha.CodigoVolume
                    , oLinha.Comprimento
                    , oLinha.LineNumber);
            }
            else
            {
                double nComprimento;

                bool resultComprimento = Double.TryParse(oLinha.Comprimento, out nComprimento);

                if (!resultComprimento)
                {
                    sMensagemErro += String.Format("A coluna Comprimento deve ser preenchida com valor numérico!" +
                        " MATERIAL: {0} CODIGO_VOLUME: {1} COMPRIMENTO: {2}.\nLinha: {3}\n",
                        oLinha.Material
                        , oLinha.CodigoVolume
                        , oLinha.Comprimento
                        , oLinha.LineNumber);
                }
            }


            if (String.IsNullOrEmpty(oLinha.TipoExpedicao))
            {
                sMensagemErro += String.Format("A coluna Tipo Expedição é obrigatória e deve ser preenchida! MATERIAL: {0} CODIGO_VOLUME: {1}" +
                    " TIPO_EXPEDICAO: {2}.\nLinha: {3}\n"
                    , oLinha.Material
                    , oLinha.CodigoVolume
                    , oLinha.TipoExpedicao
                    , oLinha.LineNumber);
            }

            if (String.IsNullOrEmpty(oLinha.Ativo))
            {
                sMensagemErro += String.Format("A coluna Ativo é obrigatória e deve ser preenchida com 0(false) ou 1(true)! MATERIAL: {0} CODIGO_VOLUME: {1}" +
                    " ATIVO: {2}.\nLinha: {3}\n"
                    , oLinha.Material
                    , oLinha.CodigoVolume
                    , oLinha.Ativo
                    , oLinha.LineNumber);
            }
            else
            {
                bool nAtivo;

                bool resultAtivo = Boolean.TryParse(oLinha.Ativo, out nAtivo);

                if (!resultAtivo)
                {
                    sMensagemErro += String.Format("A coluna Ativo deve ser preenchida com valor \"true\" ou \"false\"!" +
                        " MATERIAL: {0} CODIGO_VOLUME: {1} ATIVO: {2}.\nLinha: {3}\n",
                        oLinha.Material
                        , oLinha.CodigoVolume
                        , oLinha.Ativo
                        , oLinha.LineNumber);
                }
            }

            if (!sqoCadastroVolumeCommon.ExistPeca(oLinha.Material))
            {
                sMensagemErro += String.Format("A coluna Material deve ser preenchida com um material existente! MATERIAL: {0} \nLinha: {1}\n"
                    , oLinha.Material
                    , oLinha.LineNumber);
            }

            if (String.IsNullOrEmpty(oLinha.Operacao) && oLinha.Operacao.ToUpper() != "I"
            && oLinha.Operacao.ToUpper() != "A")
                sMensagemErro += String.Format(
                    "A coluna Operação é obrigatória e deve ser preenchida com I = Incluir, A = Alterar MATERIAL: {0} CODIGO_VOLUME: {1}" +
                    " CODIGO_VOLUME: {2}.\nLinha: {3}\n"
                    , oLinha.Material
                    , oLinha.CodigoVolume
                    , oLinha.DescricaoVolume
                    , oLinha.LineNumber);

            var duplicateExists = oPlanilha.FindAll(x => x.Material == oLinha.Material && x.CodigoVolume == oLinha.CodigoVolume).Count() > 1;

            if (duplicateExists)
                sMensagemErro += String.Format(
                    "Existem registros duplicados na planilha! MATERIAL: {0} CODIGO_VOLUME: {1}.\nLinha: {2}\n"
                    , oLinha.Material
                    , oLinha.CodigoVolume
                    , oLinha.LineNumber);

            return sMensagemErro;
        }


        private void Save(sqoClassDbConnection oDBConnection, List<LinhaPlanilha> Planilha, String sUsuario, String sNivel)
        {

            oDBConnection.BeginTransaction();

            try
            {
                foreach (var oLinha in Planilha)
                {

                    SaveDB(oLinha, sUsuario);

                    CommonStara.GravarHistorico(oDBConnection, oLinha, sNivel, sFileName, sUsuario, oLinha.Operacao == "I" ? Operation.INSERT : Operation.UPDATE);
                }
                oDBConnection.Commit();
                    
            }
            catch (Exception)
            {
                oDBConnection.Rollback();
                throw;
            }


        }

        /// <summary>
        /// Insere os dados dos volumes nas tabelas de negócio
        /// </summary>
        private void SaveDB(LinhaPlanilha oLinha, String sUser)
        {
            String sInsert = "Insert";
            String sAlter = "Update";
            String sLink = "Link";

            String sAcao = String.Empty;

            if (sqoCadastroVolumeCommon.ExistPeca(oLinha.CodigoVolume) && oLinha.Operacao.ToUpper() == "I"
                && oLinha.CodigoVolume != oLinha.Material)
                    sAcao = sLink;

            else if (oLinha.Operacao.ToUpper().Equals("I"))
                sAcao = sInsert;

            else if (oLinha.Operacao.ToUpper().Equals("A"))
            {
                sAcao = sAlter;

                oLinha.Id = GetIdVolume(oLinha.Material, oLinha.CodigoVolume, oLinha.TipoExpedicao);
            }
                

            using (var oCommand = new sqoCommand(CommandType.StoredProcedure))
            {
                oCommand
                    .SetCommandText("WSQOLEXPEDICAOCADASTROVOLUME")
                    .Add("@MATERIAL", oLinha.Material, OleDbType.VarChar, 50)
                    .Add("@CODIGO_VOLUME", oLinha.CodigoVolume.ToUpper(), OleDbType.VarChar, 50)
                    .Add("@ACAO", sAcao, OleDbType.VarChar, 50)
                    .Add("@USUARIO", sUser, OleDbType.VarChar, 50)
                    .Add("@DESCRICAO_VOLUME", oLinha.DescricaoVolume.ToUpper(), OleDbType.VarChar, 100)
                    .Add("@CODIGO_IMAGEM", oLinha.CodigoImagem, OleDbType.VarChar, 50)
                    .Add("@QUANTIDADE", oLinha.Quantidade, OleDbType.Integer)
                    .Add("TIPO_EXPEDICAO", oLinha.TipoExpedicao.ToUpper(), OleDbType.VarChar, 50)
                    .Add("@PESO_LIQUIDO", oLinha.PesoLiquido, OleDbType.Double)
                    .Add("@PESO_BRUTO", oLinha.PesoBruto, OleDbType.Double)
                    .Add("@ALTURA", oLinha.Altura, OleDbType.Double)
                    .Add("@LARGURA", oLinha.Largura, OleDbType.Double)
                    .Add("@COMPRIMENTO", oLinha.Comprimento, OleDbType.Double)
                    .Add("@ATIVO", oLinha.Ativo, OleDbType.Boolean)
                    .Add("@ID", oLinha.Id, OleDbType.BigInt)
                    ;
                try
                {
                    oCommand.Execute();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Query: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }

            }
        }

        private string ValidateExpeditionType(String sTipoExpedicao, String sLine)
        {
            String sMessage = String.Empty;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@TIPO_EXPEDICAO", sTipoExpedicao, OleDbType.VarChar, 50)
                    ;

                String sQuery = @"SELECT 1 FROM WSQOPCP2PECAVOLUMETIPO WHERE TIPO_EXPEDICAO = @TIPO_EXPEDICAO";

                oCommand.SetCommandText(sQuery);

                try
                {
                    var oResult = oCommand.GetResultado();

                    if (oResult == null)
                    {
                        sMessage += String.Format(
                        "A coluna Tipo Expedição inválida TIPO_EXPEDICAO: {0}.\nLinha: {1}\n"
                        , sTipoExpedicao
                        , sLine);
                    }
                }
                catch (Exception ex)
                {
                    sMessage += ex.Message + Environment.NewLine + oCommand.GetForLog() + Environment.NewLine;
                }
            }

            return sMessage;
        }

        private string ValidateParentVolumeSheet(List<LinhaPlanilha> oPlanilha, LinhaPlanilha oLinha)
        {
            String sMessage = "";

            var ParenteExists = oPlanilha.FindAll(x => x.Material == oLinha.Material && x.CodigoVolume == oLinha.Material).Count() >= 1;

            if (!ParenteExists)
            {
                sMessage = String.Format(
                    "Não é possível cadastrar volume, dados do código pai {0} não encontrado na planilha como volume! \nLinha: {1} \n"
                    , oLinha.Material
                    , oLinha.LineNumber);

            }
            return sMessage;
        }

        private long GetIdVolume(String sCodigoPai, String sCodigoVolume, String sTipoExpedicao)
        {
            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@CODIGO_PAI", sCodigoPai, OleDbType.VarChar, 50)
                    .Add("@CODIGO_VOLUME", sCodigoVolume, OleDbType.VarChar, 50)
                    .Add("@TIPO_EXPEDICAO", sTipoExpedicao, OleDbType.VarChar, 50)
                    ;

                String sQuery = @"SELECT
                                	ID 
                                FROM 
                                	WSQOPCP2PECAVOLUME 
                                WHERE 
                                	CODIGO_PAI = @CODIGO_PAI 
                                AND CODIGO_VOLUME = @CODIGO_VOLUME 
                                AND TIPO_EXPEDICAO = @TIPO_EXPEDICAO"
                                ;

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.GetResultado();

                    if (oResult == null)
                        throw new Exception("ID não encontrado para o Código Pai " + sCodigoPai + ", Código Volume " + sCodigoVolume
                            + ", Tipo Expedicao " + sTipoExpedicao + " favor vericar cadastro!");
                    else
                        return (long)oResult;
                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }
            }
        }

    }
}

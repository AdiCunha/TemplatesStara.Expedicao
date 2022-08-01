using AI1627Common20.TemplateDebugging;
using Common.Stara.Common.Business;
using sqoClassLibraryAI0502Message;
using sqoClassLibraryAI0502VariaveisSistema;
using sqoClassLibraryAI1151FilaProducao;
using sqoClassLibraryAI1151FilaProducao.Estrutura;
using sqoClassLibraryAI1151FilaProducao.Process;
using sqoTraceabilityStation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using TemplatesStara.CommonStara;
using PlanilhaColunaInfoAttribute = Common.Stara.Common.DataModel.PlanilhaColunaInfoAttribute;

namespace TemplateStara.Expedicao.CadastroVolume
{
    [TemplateDebug("sqoImportCastroVolumeNova")]
    public class sqoImportCastroVolumeNova : IProcessMovimentacao
    {
        enum Action { Invalid = -1, Importar, DownloadExcel, DownloadPDF }
        private Action currentAction = Action.Invalid;
        private string sMensagemErro = "";
        private string FileName = "";
        private string FilePath = "";

        DownloadExcelImportCadastroVolumeNova oDownloadExcelImportCadastroVolumeNova = new DownloadExcelImportCadastroVolumeNova();

        List<sqoCadastroVolumeModel> oListsqoCadastroVolumeModel = new List<sqoCadastroVolumeModel>();

        sqoClassSetMessageDefaults oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());

        public sqoClassMessage Executar(string sAction, string sXmlDados, string sXmlType, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao, List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sNivel, string sUsuario, object oObjAux)
        {

            Enum.TryParse(sAction, out this.currentAction);

            using (sqoClassDbConnection oDBConnection = new sqoClassDbConnection())
            {
                switch (currentAction)
                {
                    case Action.Invalid:
                        break;
                    case Action.Importar:

                        foreach (sqoClassParametrosEstrutura item in oListaParametrosMovimentacao)
                        {
                            var tt = item.Campo;
                        }

                        //this.CarregarParametrosMov(oListaParametrosMovimentacao);

                        oListsqoCadastroVolumeModel = SheetBusiness.CarregarPlanilha<sqoCadastroVolumeModel>(oListaParametrosMovimentacao);

                        this.ValidarLinhasPlanilha(oListsqoCadastroVolumeModel, oClassSetMessageDefaults);

                        this.Save(oDBConnection, oListsqoCadastroVolumeModel, sUsuario, sNivel);

                        oClassSetMessageDefaults.SetarOk();

                        break;
                    case Action.DownloadExcel:

                        oClassSetMessageDefaults.Message.Dado = oDownloadExcelImportCadastroVolumeNova.DownloadExcel();

                        //oClassSetMessageDefaults.Message.Dado = CommonStara.DownloadExcel<LinhaPlanilha>();

                        oClassSetMessageDefaults.SetarOk();

                        break;
                    case Action.DownloadPDF:
                        {
                            CarregarParametrosList(oListaParametrosListagem);

                            CommonStara.DownloadPdf(FilePath, oClassSetMessageDefaults);

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
            this.FileName = oListaParametrosMovimentacao.Find(x => x.Campo == "Arquivo").Valor;
        }

        private void CarregarParametrosList(List<sqoClassParametrosEstrutura> oListaParametrosListagem)
        {
            this.FilePath = oListaParametrosListagem.Find(x => x.Campo == "FilePath").Valor;
        }

        private string ValidarPreenchimentoColunas(sqoCadastroVolumeModel sqoCadastroVolumeModel)
        {
            foreach (var item in sqoCadastroVolumeModel.GetType().GetProperties())
            {
                if (item != null)
                {
                    var oValue = item.GetValue(sqoCadastroVolumeModel);

                    if (oValue != null)
                    {
                        if (oValue.ToString().StartsWith(" ") || oValue.ToString().EndsWith(" "))
                        {
                            var oAtributo = item.GetCustomAttributes(typeof(PlanilhaColunaInfoAttribute), true);
                            var oPlanilhaColunaInfo = (PlanilhaColunaInfoAttribute)oAtributo.FirstOrDefault();

                            if (oPlanilhaColunaInfo != null && !string.IsNullOrEmpty(oPlanilhaColunaInfo.NomeColuna))
                                sMensagemErro += string.Format("Linha: {0} - A coluna {1} possui espaços em branco.\n", sqoCadastroVolumeModel.LineNumber, oPlanilhaColunaInfo.NomeColuna) + Environment.NewLine;
                        }
                    }
                }
            }

            if (sqoCadastroVolumeModel.Operacao != "A" && sqoCadastroVolumeModel.Operacao != "I")
            {
                sMensagemErro += string.Format(
                   "A coluna Operação deve ser preenchida com 'I' para Inserir ou 'A' para Alterar! Operação preenchida: {0}. Linha: {1}\n"
                   , sqoCadastroVolumeModel.Operacao
                   , sqoCadastroVolumeModel.LineNumber
                   );
            }

            if (string.IsNullOrEmpty(sqoCadastroVolumeModel.Material))
            {
                sMensagemErro += string.Format(
                    "A coluna Material é obrigatória! MATERIAL: {0}.\nLinha: {1}\n"
                    , sqoCadastroVolumeModel.Material
                    , sqoCadastroVolumeModel.LineNumber
                    );
            }

            if (string.IsNullOrEmpty(sqoCadastroVolumeModel.CodigoVolume))
            {
                sMensagemErro += string.Format(
                    "A coluna Código Volume é obrigatória! MATERIAL: {0} CODIGO_VOLUME: {1}.\nLinha: {2}\n"
                    , sqoCadastroVolumeModel.Material
                    , sqoCadastroVolumeModel.CodigoVolume
                    , sqoCadastroVolumeModel.LineNumber
                    );
            }

            if (string.IsNullOrEmpty(sqoCadastroVolumeModel.DescricaoVolume) && sqoCadastroVolumeModel.Material != sqoCadastroVolumeModel.CodigoVolume)
            {
                sMensagemErro += string.Format("A coluna Descrição Volume é obrigatória e deve ser preenchida! MATERIAL: {0} CODIGO_VOLUME: {1}" +
                    " CODIGO_VOLUME: {2}.\nLinha: {3}\n"
                    , sqoCadastroVolumeModel.Material
                    , sqoCadastroVolumeModel.CodigoVolume
                    , sqoCadastroVolumeModel.DescricaoVolume
                    , sqoCadastroVolumeModel.LineNumber);
            }

            if (string.IsNullOrEmpty(sqoCadastroVolumeModel.Quantidade))
            {
                sMensagemErro += string.Format("A coluna Quantidade é obrigatória e deve ser preenchida! MATERIAL: {0} CODIGO_VOLUME: {1}" +
                    " QUANTIDADE: {2}.\nLinha: {3}\n"
                    , sqoCadastroVolumeModel.Material
                    , sqoCadastroVolumeModel.CodigoVolume
                    , sqoCadastroVolumeModel.Quantidade
                    , sqoCadastroVolumeModel.LineNumber);
            }
            else
            {
                int nQuantidade;

                bool resultQuantidade = int.TryParse(sqoCadastroVolumeModel.Quantidade.ToString(), out nQuantidade);

                if (!resultQuantidade)
                {
                    sMensagemErro += string.Format("A coluna Quantidade deve ser preenchida com um número inteiro!" +
                        " MATERIAL: {0} CODIGO_VOLUME: {1} QUANTIDADE: {2}.\nLinha: {3}\n",
                        sqoCadastroVolumeModel.Material
                        , sqoCadastroVolumeModel.CodigoVolume
                        , sqoCadastroVolumeModel.Quantidade
                        , sqoCadastroVolumeModel.LineNumber);
                }
            }

            if (string.IsNullOrEmpty(sqoCadastroVolumeModel.PesoLiquido))
            {
                sMensagemErro += string.Format("A coluna Peso Líquido é obrigatória e deve ser preenchida! MATERIAL: {0} CODIGO_VOLUME: {1}" +
                    " PESO_LIQUIDO: {2}.\nLinha: {3}\n"
                    , sqoCadastroVolumeModel.Material
                    , sqoCadastroVolumeModel.CodigoVolume
                    , sqoCadastroVolumeModel.PesoLiquido
                    , sqoCadastroVolumeModel.LineNumber);
            }
            else
            {
                double nPesoLiquido;

                bool resultPesoLiquido = double.TryParse(sqoCadastroVolumeModel.PesoLiquido, out nPesoLiquido);

                if (!resultPesoLiquido)
                {
                    sMensagemErro += string.Format("A coluna Peso Líquido deve ser preenchida com valor numérico!" +
                        " MATERIAL: {0} CODIGO_VOLUME: {1} PESO_LIQUIDO: {2}.\nLinha: {3}\n",
                        sqoCadastroVolumeModel.Material
                        , sqoCadastroVolumeModel.CodigoVolume
                        , sqoCadastroVolumeModel.PesoLiquido
                        , sqoCadastroVolumeModel.LineNumber);
                }
            }

            if (string.IsNullOrEmpty(sqoCadastroVolumeModel.PesoBruto))
            {
                sMensagemErro += string.Format("A coluna Peso Bruto é obrigatória e deve ser preenchida! MATERIAL: {0} CODIGO_VOLUME: {1}" +
                    " PESO_BRUTO: {2}.\nLinha: {3}\n"
                    , sqoCadastroVolumeModel.Material
                    , sqoCadastroVolumeModel.CodigoVolume
                    , sqoCadastroVolumeModel.PesoBruto
                    , sqoCadastroVolumeModel.LineNumber);
            }
            else
            {
                double nPesoBruto;

                bool resultPesoBruto = double.TryParse(sqoCadastroVolumeModel.PesoBruto, out nPesoBruto);

                if (!resultPesoBruto)
                {
                    sMensagemErro += string.Format("A coluna Peso Bruto deve ser preenchida com valor numérico!" +
                        " MATERIAL: {0} CODIGO_VOLUME: {1} PESO_BRUTO: {2}.\nLinha: {3}\n",
                        sqoCadastroVolumeModel.Material
                        , sqoCadastroVolumeModel.CodigoVolume
                        , sqoCadastroVolumeModel.PesoBruto
                        , sqoCadastroVolumeModel.LineNumber);
                }
            }

            if (string.IsNullOrEmpty(sqoCadastroVolumeModel.Altura))
            {
                sMensagemErro += string.Format("A coluna Altura é obrigatória e deve ser preenchida! MATERIAL: {0} CODIGO_VOLUME: {1}" +
                    " ALTURA: {2}.\nLinha: {3}\n"
                    , sqoCadastroVolumeModel.Material
                    , sqoCadastroVolumeModel.CodigoVolume
                    , sqoCadastroVolumeModel.Altura
                    , sqoCadastroVolumeModel.LineNumber);

            }
            else
            {
                double nAltura;

                bool resultAltura = double.TryParse(sqoCadastroVolumeModel.Altura, out nAltura);

                if (!resultAltura)
                {
                    sMensagemErro += string.Format("A coluna Altura deve ser preenchida com valor numérico!" +
                        " MATERIAL: {0} CODIGO_VOLUME: {1} ALTURA: {2}.\nLinha: {3}\n",
                        sqoCadastroVolumeModel.Material
                        , sqoCadastroVolumeModel.CodigoVolume
                        , sqoCadastroVolumeModel.Altura
                        , sqoCadastroVolumeModel.LineNumber);
                }
            }


            if (string.IsNullOrEmpty(sqoCadastroVolumeModel.Largura))
            {
                sMensagemErro += string.Format("A coluna Largura é obrigatória e deve ser preenchida! MATERIAL: {0} CODIGO_VOLUME: {1}" +
                    " LARGURA: {2}.\nLinha: {3}\n"
                    , sqoCadastroVolumeModel.Material
                    , sqoCadastroVolumeModel.CodigoVolume
                    , sqoCadastroVolumeModel.Largura
                    , sqoCadastroVolumeModel.LineNumber);
            }
            else
            {
                double nLargura;

                bool resultLargura = double.TryParse(sqoCadastroVolumeModel.Largura, out nLargura);

                if (!resultLargura)
                {
                    sMensagemErro += string.Format("A coluna Largura deve ser preenchida com valor numérico!" +
                        " MATERIAL: {0} CODIGO_VOLUME: {1} LARGURA: {2}.\nLinha: {3}\n",
                        sqoCadastroVolumeModel.Material
                        , sqoCadastroVolumeModel.CodigoVolume
                        , sqoCadastroVolumeModel.Largura
                        , sqoCadastroVolumeModel.LineNumber);
                }
            }


            if (string.IsNullOrEmpty(sqoCadastroVolumeModel.Comprimento))
            {
                sMensagemErro += string.Format("A coluna Comprimento é obrigatória e deve ser preenchida! MATERIAL: {0} CODIGO_VOLUME: {1}" +
                    " COMPRIMENTO: {2}.\nLinha: {3}\n"
                    , sqoCadastroVolumeModel.Material
                    , sqoCadastroVolumeModel.CodigoVolume
                    , sqoCadastroVolumeModel.Comprimento
                    , sqoCadastroVolumeModel.LineNumber);
            }
            else
            {
                double nComprimento;

                bool resultComprimento = double.TryParse(sqoCadastroVolumeModel.Comprimento, out nComprimento);

                if (!resultComprimento)
                {
                    sMensagemErro += string.Format("A coluna Comprimento deve ser preenchida com valor numérico!" +
                        " MATERIAL: {0} CODIGO_VOLUME: {1} COMPRIMENTO: {2}.\nLinha: {3}\n",
                          sqoCadastroVolumeModel.Material
                        , sqoCadastroVolumeModel.CodigoVolume
                        , sqoCadastroVolumeModel.Comprimento
                        , sqoCadastroVolumeModel.LineNumber);
                }
            }


            if (string.IsNullOrEmpty(sqoCadastroVolumeModel.TipoExpedicao))
            {
                sMensagemErro += string.Format("A coluna Tipo Expedição é obrigatória e deve ser preenchida! MATERIAL: {0} CODIGO_VOLUME: {1}" +
                    " TIPO_EXPEDICAO: {2}.\nLinha: {3}\n"
                    , sqoCadastroVolumeModel.Material
                    , sqoCadastroVolumeModel.CodigoVolume
                    , sqoCadastroVolumeModel.TipoExpedicao
                    , sqoCadastroVolumeModel.LineNumber);
            }

            if (string.IsNullOrEmpty(sqoCadastroVolumeModel.Ativo))
            {
                sMensagemErro += string.Format("A coluna Ativo é obrigatória e deve ser preenchida com 0(false) ou 1(true)! MATERIAL: {0} CODIGO_VOLUME: {1}" +
                    " ATIVO: {2}.\nLinha: {3}\n"
                    , sqoCadastroVolumeModel.Material
                    , sqoCadastroVolumeModel.CodigoVolume
                    , sqoCadastroVolumeModel.Ativo
                    , sqoCadastroVolumeModel.LineNumber);
            }
            else
            {
                bool nAtivo;

                bool resultAtivo = bool.TryParse(sqoCadastroVolumeModel.Ativo, out nAtivo);

                if (!resultAtivo)
                {
                    sMensagemErro += string.Format("A coluna Ativo deve ser preenchida com valor \"true\" ou \"false\"!" +
                        " MATERIAL: {0} CODIGO_VOLUME: {1} ATIVO: {2}.\nLinha: {3}\n",
                        sqoCadastroVolumeModel.Material
                        , sqoCadastroVolumeModel.CodigoVolume
                        , sqoCadastroVolumeModel.Ativo
                        , sqoCadastroVolumeModel.LineNumber);
                }
            }

            if (!sqoCadastroVolumeCommon.ExistPeca(sqoCadastroVolumeModel.Material))
            {
                sMensagemErro += string.Format("A coluna Material deve ser preenchida com um material existente! MATERIAL: {0} \nLinha: {1}\n"
                    , sqoCadastroVolumeModel.Material
                    , sqoCadastroVolumeModel.LineNumber);
            }

            if (string.IsNullOrEmpty(sqoCadastroVolumeModel.Operacao) && sqoCadastroVolumeModel.Operacao.ToUpper() != "I"
            && sqoCadastroVolumeModel.Operacao.ToUpper() != "A")
                sMensagemErro += string.Format(
                    "A coluna Operação é obrigatória e deve ser preenchida com I = Incluir, A = Alterar MATERIAL: {0} CODIGO_VOLUME: {1}" +
                    " CODIGO_VOLUME: {2}.\nLinha: {3}\n"
                    , sqoCadastroVolumeModel.Material
                    , sqoCadastroVolumeModel.CodigoVolume
                    , sqoCadastroVolumeModel.DescricaoVolume
                    , sqoCadastroVolumeModel.LineNumber);

            var duplicateExists = oListsqoCadastroVolumeModel.FindAll(x => x.Material == sqoCadastroVolumeModel.Material && x.CodigoVolume == sqoCadastroVolumeModel.CodigoVolume).Count() > 1;

            if (duplicateExists)
                sMensagemErro += string.Format(
                    "Existem registros duplicados na planilha! MATERIAL: {0} CODIGO_VOLUME: {1}.\nLinha: {2}\n"
                    , sqoCadastroVolumeModel.Material
                    , sqoCadastroVolumeModel.CodigoVolume
                    , sqoCadastroVolumeModel.LineNumber);

            return sMensagemErro;
        }

        private void ValidarLinhasPlanilha(List<sqoCadastroVolumeModel> oListsqoCadastroVolumeModel, sqoClassSetMessageDefaults oClassSetMessageDefaults)
        {
            string sMensagemErro = "";

            foreach (var oLinha in oListsqoCadastroVolumeModel)
            {
                sMensagemErro += ValidarPreenchimentoColunas(oLinha);

                if (oLinha.Operacao.ToUpper() == "I")
                {
                    string sMessageValidateVolume = "";

                    sMessageValidateVolume += sqoCadastroVolumeCommon.ValidateVolume(oLinha.Material, oLinha.CodigoVolume, oLinha.TipoExpedicao, false);


                    if (oLinha.CodigoVolume != oLinha.Material)
                    {
                        sMessageValidateVolume += sqoCadastroVolumeCommon.ValidateVolumeType(oLinha.CodigoVolume);
                    }

                    if (!string.IsNullOrEmpty(sMessageValidateVolume))
                    {
                        sMensagemErro += string.Format("Linha: {0}", oLinha.LineNumber) + sMessageValidateVolume + Environment.NewLine;
                    }


                    if (oLinha.Material != oLinha.CodigoVolume)
                    {
                        string sMessageValidateParentVolume = "";
                        string sMessageValidateParentVolumeSheet = "";

                        sMessageValidateParentVolume = sqoCadastroVolumeCommon.ValidateParentVolume
                            (oLinha.Material, oLinha.CodigoVolume, "Insert");

                        if (!string.IsNullOrEmpty(sMessageValidateParentVolume))
                        {
                            sMessageValidateParentVolume += string.Format("Linha: {0}\n", oLinha.LineNumber);

                            sMessageValidateParentVolumeSheet = ValidateParentVolumeSheet(oListsqoCadastroVolumeModel, oLinha);

                            if (!string.IsNullOrEmpty(sMessageValidateParentVolumeSheet))
                                sMensagemErro += sMessageValidateParentVolume + sMessageValidateParentVolumeSheet;
                        }
                    }

                    sMensagemErro += ValidateExpeditionType(oLinha.TipoExpedicao, oLinha.LineNumber.ToString());

                }

                else if (string.IsNullOrEmpty(sMensagemErro) && oLinha.Operacao.ToUpper() == "A")
                {
                    string sMessageValidateVolume;

                    sMessageValidateVolume = sqoCadastroVolumeCommon.ValidateVolume(oLinha.Material, oLinha.CodigoVolume, oLinha.TipoExpedicao, true);

                    if (!string.IsNullOrEmpty(sMessageValidateVolume))
                    {
                        sMensagemErro += sMessageValidateVolume + string.Format("/nLinha: {0}/n", oLinha.LineNumber);
                    }

                    sMensagemErro += ValidateExpeditionType(oLinha.TipoExpedicao, oLinha.LineNumber.ToString());
                }
            }

            if (!string.IsNullOrEmpty(sMensagemErro))
            {
                CommonStara.MessageBox(false, "Falha na validação de dados", sMensagemErro, sqoClassMessage.MessageTypeEnum.ERROR, oClassSetMessageDefaults);

                throw new sqoClassMessageUserException(oClassSetMessageDefaults.Message);
            }

        }

        private string ValidateParentVolumeSheet(List<sqoCadastroVolumeModel> oListSqoCadastroVolumeModel, sqoCadastroVolumeModel oLinha)
        {
            string sMessage = "";

            var ParenteExists = oListSqoCadastroVolumeModel.FindAll(x => x.Material == oLinha.Material && x.CodigoVolume == oLinha.Material).Count() >= 1;

            if (!ParenteExists)
            {
                sMessage = string.Format("Não é possível cadastrar volume, dados do código pai {0} não encontrado na planilha como volume! \nLinha: {1} \n"
                    , oLinha.Material
                    , oLinha.LineNumber);
            }
            return sMessage;
        }

        private void Save(sqoClassDbConnection oDBConnection, List<sqoCadastroVolumeModel> oListSqoCadastroVolumeModel, string sUsuario, string sNivel)
        {
            oDBConnection.BeginTransaction();

            try
            {
                foreach (var oLinha in oListSqoCadastroVolumeModel)
                {

                    this.Gravar(oLinha, sUsuario);

                    CommonStara.GravarHistorico(oDBConnection, oLinha, sNivel, FileName, sUsuario, oLinha.Operacao == "I" ? Operation.INSERT : Operation.UPDATE);
                }

                oDBConnection.Commit();
            }

            catch (Exception)
            {
                oDBConnection.Rollback();
                throw;
            }
        }

        private void Gravar(sqoCadastroVolumeModel oSqoCadastroVolumeModel, string sUsuario)
        {
            string sInsert = "Insert";
            string sAlter = "Update";
            string sLink = "Link";

            string sAcao = string.Empty;

            if (sqoCadastroVolumeCommon.ExistPeca(oSqoCadastroVolumeModel.CodigoVolume) && oSqoCadastroVolumeModel.Operacao.ToUpper() == "I"
                && oSqoCadastroVolumeModel.CodigoVolume != oSqoCadastroVolumeModel.Material)
                sAcao = sLink;

            else if (oSqoCadastroVolumeModel.Operacao.ToUpper().Equals("I"))
                sAcao = sInsert;

            else if (oSqoCadastroVolumeModel.Operacao.ToUpper().Equals("A"))
            {
                sAcao = sAlter;

                oSqoCadastroVolumeModel.Id = GetIdVolume(oSqoCadastroVolumeModel.Material, oSqoCadastroVolumeModel.CodigoVolume, oSqoCadastroVolumeModel.TipoExpedicao);
            }

            using (var oCommand = new sqoCommand(CommandType.StoredProcedure))
            {
                oCommand
                    .SetCommandText("WSQOLEXPEDICAOCADASTROVOLUME")
                    .Add("@MATERIAL", oSqoCadastroVolumeModel.Material)
                    .Add("@CODIGO_VOLUME", oSqoCadastroVolumeModel.CodigoVolume.ToUpper())
                    .Add("@ACAO", sAcao)
                    .Add("@USUARIO", sUsuario)
                    .Add("@DESCRICAO_VOLUME", oSqoCadastroVolumeModel.DescricaoVolume.ToUpper())
                    .Add("@CODIGO_IMAGEM", oSqoCadastroVolumeModel.CodigoImagem)
                    .Add("@QUANTIDADE", oSqoCadastroVolumeModel.Quantidade)
                    .Add("TIPO_EXPEDICAO", oSqoCadastroVolumeModel.TipoExpedicao.ToUpper())
                    .Add("@PESO_LIQUIDO", oSqoCadastroVolumeModel.PesoLiquido)
                    .Add("@PESO_BRUTO", oSqoCadastroVolumeModel.PesoBruto)
                    .Add("@ALTURA", oSqoCadastroVolumeModel.Altura)
                    .Add("@LARGURA", oSqoCadastroVolumeModel.Largura)
                    .Add("@COMPRIMENTO", oSqoCadastroVolumeModel.Comprimento)
                    .Add("@ATIVO", oSqoCadastroVolumeModel.Ativo)
                    .Add("@ID", oSqoCadastroVolumeModel.Id)
                    ;
                try
                {
                    oCommand.Execute();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message + Environment.NewLine + "Query: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }
        }

        private string ValidateExpeditionType(string TipoExpedicao, string Linha)
        {
            string sMessage = string.Empty;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@TIPO_EXPEDICAO", TipoExpedicao)
                    ;

                string sQuery = @"SELECT 1 FROM WSQOPCP2PECAVOLUMETIPO WHERE TIPO_EXPEDICAO = @TIPO_EXPEDICAO";

                oCommand.SetCommandText(sQuery);

                try
                {
                    var oResult = oCommand.GetResultado();

                    if (oResult == null)
                    {
                        sMessage += "Linha: " + Linha + " - A coluna Tipo Expedição inválida TIPO_EXPEDICAO: " + TipoExpedicao + Environment.NewLine;
                    }
                }
                catch (Exception ex)
                {
                    sMessage += ex.Message + Environment.NewLine + oCommand.GetForLog() + Environment.NewLine;
                }
            }

            return sMessage;
        }


        private long GetIdVolume(string CodigoPai, string CodigoVolume, string TipoExpedicao)
        {
            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@CODIGO_PAI", CodigoPai, OleDbType.VarChar, 50)
                    .Add("@CODIGO_VOLUME", CodigoVolume, OleDbType.VarChar, 50)
                    .Add("@TIPO_EXPEDICAO", TipoExpedicao, OleDbType.VarChar, 50)
                    ;

                string sQuery = @"SELECT
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
                        throw new Exception("ID não encontrado para o Código Pai " + CodigoPai + ", Código Volume " + CodigoVolume + ", Tipo Expedicao " + TipoExpedicao + " favor vericar cadastro!");
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
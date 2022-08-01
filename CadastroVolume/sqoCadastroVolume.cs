//Comentar o define quando colar na web!
//#define NAO_COMPILAR

#if !NAO_COMPILAR
using sqoClassLibraryAI0502VariaveisSistema;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using sqoClassLibraryAI0502Biblio;
using sqoClassLibraryAI1151FilaProducao.Estrutura;
using sqoClassLibraryAI0502Message;
using AI1627Common20.TemplateDebugging;
using sqoClassLibraryAI1151FilaProducao.Process;
using sqoClassLibraryAI1151FilaProducao;
using System.Xml.Serialization;
using TemplatesStara.CommonStara;
using System.Linq;
using System.Xml;
using AI1627Common20.Log;

namespace sqoTraceabilityStation
{
    /// <summary>
    /// Template para cadastro de volumes de materiais
    /// </summary>
    [TemplateDebug("sqoCadastroVolume")]
    public class sqoCadastroVolume : sqoClassProcessMovimentacao
    {
        private String sUser;
        private Action currentAction = Action.Invalid;
        private sqoClassCadastroVolume oCadastroVolume;
        private sqoClassSetMessageDefaults oClassSetMessageDefaults;
        enum Action { Invalid = -1, Insert, Update, Delete, Duplicate, Link, Unlink }

        private int iQtdErros = 0;
        private String sMessage = "Falha na validação de dados";
        private String sDescription = String.Empty;
        private String sTipoPecaPA = "ZFER";
        private String sTipoPecaMVol = "MVOL";


        public override sqoClassMessage Executar(string sAction, string sXmlDados, string sXmlType,
           List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao, List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sNivel, string sUsuario, object oObjAux)
        {

            using (sqoClassDbConnection oDBConnection = new sqoClassDbConnection())
            {

                this.Init(sXmlDados, sUsuario, oListaParametrosListagem, sAction);

                this.Validation(oDBConnection, sUsuario);

                this.ProcessBussinessLogic(oDBConnection, sUsuario);

            }

            return this.oClassSetMessageDefaults.Message;
        }

        /// <summary>
        /// Inicializa e preenche os objetos globais da classe.
        /// </summary>
        /// <param name="sXmlDados"></param>
        /// <param name="sUsuario"></param>
        /// <param name="oListaParametrosListagem"></param>
        /// <param name="sAction"></param>
        private void Init(string sXmlDados, string sUsuario, List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sAction)
        {
            this.oCadastroVolume = new sqoClassCadastroVolume();
            this.oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());
            this.oCadastroVolume = sqoClassBiblioSerDes.DeserializeObject<sqoClassCadastroVolume>(sXmlDados);
            this.sUser = sUsuario;
            Enum.TryParse(sAction, out currentAction);
            if (this.currentAction.Equals(Action.Insert))
                this.FillPersistence();
            //else if (this.currentAction.Equals(Action.Duplicate))
            //    this.FillPersistenceDuplicate();
            else if (this.currentAction.Equals(Action.Link))
                this.FillPersistenceLink();
        }

        /// <summary>
        /// Executa a validação dos dados inseridos na web pelo usuário.
        /// </summary>
        private void Validation(sqoClassDbConnection oDBConnection, string sUsuario)
        {

            if (this.currentAction.Equals(Action.Insert) || this.currentAction.Equals(Action.Duplicate))
            {
                this.ValidationForm();
                this.ValidationInsert();
                this.ValidationMessage();
            }

            else if (this.currentAction.Equals(Action.Update))
            {
                this.ValidationForm();
                this.ValidationUpdate();
                this.ValidationMessage();
            }

            else if (this.currentAction.Equals(Action.Link))
            {
                this.ValidationForm();
                this.ValidationLink();
                this.ValidationMessage();
            }

            else if (this.currentAction.Equals(Action.Unlink))
            {
                this.ValidationVolumeUnlink();
                this.ValidationMessage();
            }

            //else if (this.currentAction.Equals(Action.Update))
            //this.ValidationUpdate();

            //throw new sqoClassMessageUserException(this.oClassSetMessageDefaults.Message);
        }

        /// <summary>
        /// Valida os dados do action insert na web
        /// </summary>
        public void ValidationForm()
        {
            String sMessageMaterial = " - Campo \"Material\" deve ser preenchido!";
            String sMessageCodigoVolume = " - Campo \"Código Volume\" deve ser preenchido!";
            String sMessageDescricaoVolume = " - Campo \"Descrição Volume\" deve ser preenchido!";
            String sMessagePesoLiquido = " - Campo \"Peso Liquido\" deve ser maior que zero!";
            String sMessagePesoBruto = " - Campo \"Peso Bruto\" deve ser maior que zero!";
            String sMessageTipoVolume = " - Campo \"Tipo Volume\" deve ser preenchido!";
            String sMessageQuantidade = " - Campo \"Quantidade\" deve ser preenchido!";
            String sMessageQuantidadeConvert = " - Campo \"Quantidade\" deve ser um número inteiro";
            String SMessageQuantidadeMaxVolume = " - Não é permitido cadastrar quantidade maior que 1 para Código Pai";

            foreach (var item in oCadastroVolume.GetType().GetProperties())
            {
                if (item != null)
                {
                    var oValue = item.GetValue(oCadastroVolume);

                    if (oValue != null)
                    {
                        if (oValue.ToString().StartsWith(" ") || oValue.ToString().EndsWith(" "))
                        {
                            var oAtributo = item.GetCustomAttributes(typeof(XmlElementAttribute), true);
                            var oPlanilhaColunaInfo = (XmlElementAttribute)oAtributo.FirstOrDefault();

                            if (oPlanilhaColunaInfo != null && !String.IsNullOrEmpty(oPlanilhaColunaInfo.ElementName))
                            {
                                this.iQtdErros++;
                                this.sDescription += this.iQtdErros + String.Format(" - Campo {0} possui espaços em branco. ", oPlanilhaColunaInfo.ElementName) + Environment.NewLine;
                            }
                        }
                    }
                }
            }

            int number;

            if (String.IsNullOrEmpty(this.oCadastroVolume.Material))
            {
                this.iQtdErros++;
                this.sDescription += this.iQtdErros + sMessageMaterial + Environment.NewLine;
            }

            if (String.IsNullOrEmpty(this.oCadastroVolume.CodigoVolume))
            {
                this.iQtdErros++;
                this.sDescription += this.iQtdErros + sMessageCodigoVolume + Environment.NewLine;
            }

            if (String.IsNullOrEmpty(this.oCadastroVolume.DescricaoVolume) & (this.oCadastroVolume.Material != this.oCadastroVolume.CodigoVolume))
            {
                this.iQtdErros++;
                this.sDescription += this.iQtdErros + sMessageDescricaoVolume + Environment.NewLine;
            }

            if (this.oCadastroVolume.Quantidade <= 0)
            {
                this.iQtdErros++;
                this.sDescription += this.iQtdErros + sMessageQuantidade + Environment.NewLine;
            }

            if (this.oCadastroVolume.PesoLiquido <= 0)
            {
                this.iQtdErros++;
                this.sDescription += this.iQtdErros + sMessagePesoLiquido + Environment.NewLine;
            }

            if (this.oCadastroVolume.PesoBruto <= 0)
            {
                this.iQtdErros++;
                this.sDescription += this.iQtdErros + sMessagePesoBruto + Environment.NewLine;
            }

            if (String.IsNullOrEmpty(this.oCadastroVolume.TipoVolume))
            {
                this.iQtdErros++;
                this.sDescription += this.iQtdErros + sMessageTipoVolume + Environment.NewLine;
            }

            bool result = Int32.TryParse(oCadastroVolume.Quantidade.ToString(), out number);
            if (!result)
            {
                this.iQtdErros++;
                this.sDescription += this.iQtdErros + sMessageQuantidadeConvert + Environment.NewLine;
            }

            if ((this.oCadastroVolume.Material.Equals(this.oCadastroVolume.CodigoVolume)) && this.oCadastroVolume.Quantidade > 1)
            {
                this.iQtdErros++;

                this.sDescription += this.iQtdErros.ToString() + SMessageQuantidadeMaxVolume + Environment.NewLine;
            }

        }

        /// <summary>
        /// Valida dados inseridos pelo usuário
        /// </summary>
        public void ValidationInsert()
        {
            String sMessageValidateVolume = "";
            String sMessageValidateParentVolume = "";
            String sMessageMaterialCodigoVolumeDiferentes = " - Campo \"Material\" e o campo \"Código Volume\" não podem conter códigos tipo ZFER diferentes!";
            String sMessageCodigoVolumeMaterialExistente = " - Campo \"Código Volume\" não podem conter código de material existente!";

            sMessageValidateVolume = sqoCadastroVolumeCommon.ValidateVolume(this.oCadastroVolume.Material, this.oCadastroVolume.CodigoVolume,
                this.oCadastroVolume.TipoVolume, false);

            if (!String.IsNullOrEmpty(sMessageValidateVolume))
            {
                this.iQtdErros++;
                this.sDescription += iQtdErros + sMessageValidateVolume + Environment.NewLine;
            }


            if (this.oCadastroVolume.Material != this.oCadastroVolume.CodigoVolume)
            {
                sMessageValidateParentVolume = sqoCadastroVolumeCommon.ValidateParentVolume(oCadastroVolume.Material, oCadastroVolume.CodigoVolume, "Insert");
                if (!String.IsNullOrEmpty(sMessageValidateParentVolume))
                {
                    this.iQtdErros++;
                    this.sDescription += iQtdErros + sMessageValidateParentVolume + Environment.NewLine;
                }
            }

            if (sqoCadastroVolumeCommon.ExistPeca(this.oCadastroVolume.CodigoVolume))
            {
                this.GetPeca();

                if (this.oCadastroVolume.TipoPeca.Equals(this.sTipoPecaPA) &&
                    (this.oCadastroVolume.Material != this.oCadastroVolume.CodigoVolume))
                {
                    this.iQtdErros++;

                    this.sDescription += iQtdErros + sMessageMaterialCodigoVolumeDiferentes + Environment.NewLine;
                }
                else if (!this.oCadastroVolume.TipoPeca.Equals(this.sTipoPecaMVol))
                {
                    this.iQtdErros++;

                    this.sDescription += iQtdErros + sMessageCodigoVolumeMaterialExistente + Environment.NewLine;
                }
            }

        }

        /// <summary>
        /// Valida os dados do action update na web
        /// </summary>
        private void ValidationUpdate()
        {
            if (!EqualsCodigoVolume(this.oCadastroVolume.CodigoVolume.TrimEnd(), this.oCadastroVolume.Id))
            {
                if (ExistCodigoVolume(this.oCadastroVolume.CodigoVolume.TrimEnd(), this.oCadastroVolume.Material))
                {
                    this.iQtdErros++;

                    this.sDescription += this.iQtdErros.ToString() + " - Não é possível alterar o campo Código Volume para um vinculo já existente"
                        + "Código Volume: " + this.oCadastroVolume.CodigoVolume + " Código Pai:" + this.oCadastroVolume.Material
                        + "!" + Environment.NewLine;
                }

            }

            else
            {
                this.GetPeca();

                if (this.oCadastroVolume.CodigoVolume.Equals(this.oCadastroVolume.Material))
                {
                    if (this.oCadastroVolume.DescricaoVolume != this.oCadastroVolume.DescricaoPeca)
                    {
                        this.iQtdErros++;
                        this.sDescription += this.iQtdErros + " - Não é possível alterar a descrição do material para o código pai "
                            + this.oCadastroVolume.Material + ", se necessário deve ser alterado cadastro no ERP(SAP)!" + Environment.NewLine;
                    }

                    if (this.oCadastroVolume.Ativo != this.oCadastroVolume.AtivoPeca)
                    {
                        this.iQtdErros++;
                        this.sDescription += this.iQtdErros + " - Não é possível alterar o status do material para o código pai "
                            + this.oCadastroVolume.Material + ", se necessário deve ser alterado cadastro no ERP(SAP)!" + Environment.NewLine;
                    }
                }
            }

            var oCadastro = sqoClassLESExpedicaoVolumeCadastroControlerDB.GetPcp2PecaVolumeById(oCadastroVolume.Id);

            foreach (var oItem in oCadastro)
            {
                if (oItem.CodigoPai.Equals(oItem.CodigoPai) && oCadastroVolume.CodigoVolume != oItem.CodigoVolume)
                {
                    this.iQtdErros++;

                    this.sDescription += this.iQtdErros.ToString() + " - Não é permitido alterar o código volume de um Código Pai!" + Environment.NewLine;
                }
            }
        }

        public void ValidationLink()
        {

            GetPeca();

            if (this.oCadastroVolume.TipoPeca.Equals(sTipoPecaPA))
            {
                this.iQtdErros++;
                this.sDescription += this.iQtdErros + " - Não é possível vincular Material a um volume \"PA\": "
                    + this.oCadastroVolume.CodigoVolume + "!" + Environment.NewLine;

            }

            if (String.IsNullOrEmpty(this.sDescription))
            {
                String sMessageValidateVolume = "";
                String sMessageValidateParentVolume = "";

                sMessageValidateVolume = sqoCadastroVolumeCommon.ValidateVolume(this.oCadastroVolume.Material, this.oCadastroVolume.CodigoVolume,
                    this.oCadastroVolume.TipoVolume, false);

                if (!String.IsNullOrEmpty(sMessageValidateVolume))
                {
                    this.iQtdErros++;
                    this.sDescription += iQtdErros + sMessageValidateVolume;
                }

                sMessageValidateParentVolume = sqoCadastroVolumeCommon.ValidateParentVolume(oCadastroVolume.Material, oCadastroVolume.CodigoVolume, "Link");
                if (!String.IsNullOrEmpty(sMessageValidateParentVolume))
                {
                    this.iQtdErros++;
                    this.sDescription += iQtdErros + sMessageValidateParentVolume;
                }

            }
        }

        public void ValidationVolumeUnlink()
        {
            if (!this.oCadastroVolume.TipoPeca.Equals(sTipoPecaMVol))
            {
                this.iQtdErros++;
                this.sDescription += this.iQtdErros + " - Não é possível desvincular volumes que não sejam do tipo " + sTipoPecaMVol + "!" + Environment.NewLine;
            }
        }

        public void ValidationMessage()
        {
            if (!String.IsNullOrEmpty(sDescription))
            {
                String sMessageDescription = iQtdErros > 1 ? ("Encontrados " + iQtdErros + " erros!") : ("Encontrado " + iQtdErros + " erro!");
                String sMessageBody = sMessageDescription + Environment.NewLine + sDescription;

                SetMessage(false, this.sMessage, sMessageBody, sqoClassMessage.MessageTypeEnum.ERROR);

                throw new sqoClassMessageUserException(this.oClassSetMessageDefaults.Message);
            }
        }

        /// <summary>
        /// Preenche os objetos default de mensagens
        /// </summary>
        /// <param name="Ok"></param>
        /// <param name="sMessage"></param>
        /// <param name="sMessageDescription"></param>
        /// <param name="Type"></param>
        public void SetMessage(Boolean Ok, String sMessage, String sMessageDescription, sqoClassMessage.MessageTypeEnum Type)
        {
            oClassSetMessageDefaults.Message.Ok = Ok;
            oClassSetMessageDefaults.Message.Message = sMessage;
            oClassSetMessageDefaults.Message.MessageDescription = sMessageDescription;
            oClassSetMessageDefaults.Message.MessageType = Type;
        }

        /// <summary>
        /// Método que executa a lógica de inserir volumes, alterar volumes, deletar volumes e vincular volumes.
        /// </summary>
        /// <param name="oDBConnection"></param>
        private sqoClassMessage ProcessBussinessLogic(sqoClassDbConnection oDBConnection, string sUsuario)
        {
            String sMessage = String.Empty;
            String sMessageBody = String.Empty;

            try
            {
                oDBConnection.BeginTransaction();

                if (this.currentAction.Equals(Action.Unlink))
                {
                    this.UnlinkVolume();
                    this.InstertHistUnlinkVolume(oDBConnection, sUsuario);
                }
                else
                    Save();

                oDBConnection.Commit();
            }
            catch (Exception ex)
            {
                sqoClassMessageUserException oClassMessageUserException =
                    new sqoClassMessageUserException("Error" + Environment.NewLine + ex.Message, ex.InnerException);
                oDBConnection.Rollback();

                throw oClassMessageUserException;
            }

            if (currentAction.Equals(Action.Insert) || currentAction.Equals(Action.Duplicate))
            {
                sMessage = "Dados inseridos com sucesso!";
                sMessageBody = sMessage;
            }
            else if (currentAction.Equals(Action.Update))
            {
                sMessage = "Dados alterados com sucesso!";
                sMessageBody = sMessage;
            }

            else if (currentAction.Equals(Action.Link))
            {
                sMessage = "Material vinculado com sucesso!";
                sMessageBody = sMessage;
            }

            else if (currentAction.Equals(Action.Unlink))
            {
                sMessage = "Volume desvinculado com sucesso!";
                sMessageBody = sMessage;
            }


            this.SetMessage(true, sMessage, sMessageBody, sqoClassMessage.MessageTypeEnum.OK);

            return oClassSetMessageDefaults.Message;
        }

        /// <summary>
        /// Insere os dados dos volumes nas tabelas de negócio
        /// </summary>
        private void Save()
        {
            using (var oCommand = new sqoCommand(CommandType.StoredProcedure))
            {
                oCommand
                    .SetCommandText("WSQOLEXPEDICAOCADASTROVOLUME")
                    .Add("@MATERIAL", this.oCadastroVolume.Material, OleDbType.VarChar, 50)
                    .Add("@CODIGO_VOLUME", this.oCadastroVolume.CodigoVolume, OleDbType.VarChar, 50)
                    .Add("@ACAO", this.currentAction, OleDbType.VarChar, 50)
                    .Add("@USUARIO", this.sUser, OleDbType.VarChar, 50)
                    .Add("@DESCRICAO_VOLUME", this.oCadastroVolume.DescricaoVolume, OleDbType.VarChar, 100)
                    .Add("@CODIGO_IMAGEM", this.oCadastroVolume.CodigoImagem, OleDbType.VarChar, 50)
                    .Add("@QUANTIDADE", this.oCadastroVolume.Quantidade, OleDbType.Integer)
                    .Add("TIPO_EXPEDICAO", this.oCadastroVolume.TipoVolume, OleDbType.VarChar, 50)
                    .Add("@PESO_LIQUIDO", this.oCadastroVolume.PesoLiquido, OleDbType.Double)
                    .Add("@PESO_BRUTO", this.oCadastroVolume.PesoBruto, OleDbType.Double)
                    .Add("@ALTURA", this.oCadastroVolume.Altura, OleDbType.Double)
                    .Add("@LARGURA", this.oCadastroVolume.Largura, OleDbType.Double)
                    .Add("@COMPRIMENTO", this.oCadastroVolume.Comprimento, OleDbType.Double)
                    .Add("@ATIVO", oCadastroVolume.Ativo, OleDbType.Boolean)
                    .Add("@ID_VOL", this.oCadastroVolume.Id, OleDbType.BigInt)
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

        /// <summary>
        /// Deleta os dados dos volumes nas tabelas de negócio
        /// </summary>
        private void UnlinkVolume()
        {
            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@ID", this.oCadastroVolume.Id, OleDbType.BigInt)
                    ;

                string sQuery = @"DELETE FROM WSQOPCP2PECAVOLUME WHERE ID = @ID";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    oCommand.Execute();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }
            }
        }


        private void InstertHistUnlinkVolume(sqoClassDbConnection oDBConnection, string sUsuario)
        {
            string sObservacao = "Volume desvinculado via Web";

            string sQuery = @"INSERT INTO [dbo].[WSQOPCP2PECAVOLUMEHIST]
                                           ([ID_VOLUME]
                                           ,[CODIGO_PAI]
                                           ,[CODIGO_VOLUME]
                                           ,[QUANTIDADE]
                                           ,[TIPO_EXPEDICAO]
                                           ,[PESO_LIQUIDO]
                                           ,[PESO_BRUTO]
                                           ,[ALTURA]
                                           ,[LARGURA]
                                           ,[COMPRIMENTO]
                                           ,[USUARIO]
                                           ,[DATA_OCORRENCIA]
                                           ,[OBSERVACAO]) 
                                     VALUES
                                           (@ID_VOLUME
                                           ,@CODIGO_PAI
                                           ,@CODIGO_VOLUME
                                           ,@QUANTIDADE
                                           ,@TIPO_EXPEDICAO
                                           ,@PESO_LIQUIDO
                                           ,@PESO_BRUTO
                                           ,@ALTURA
                                           ,@LARGURA
                                           ,@COMPRIMENTO
                                           ,@USUARIO
                                           ,@DATAOCORRENCIA
                                           ,@OBSERVACAO)";

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                try
                {
                    oCommand
                        .Add("@ID_VOLUME", oCadastroVolume.Id, OleDbType.BigInt)
                        .Add("@CODIGO_PAI", oCadastroVolume.Material, OleDbType.VarChar, 50)
                        .Add("@CODIGO_VOLUME", oCadastroVolume.CodigoVolume, OleDbType.VarChar, 50)
                        .Add("@QUANTIDADE", oCadastroVolume.Quantidade, OleDbType.Integer)
                        .Add("@TIPO_EXPEDICAO", oCadastroVolume.TipoVolume, OleDbType.VarChar, 50)
                        .Add("@PESO_LIQUIDO", oCadastroVolume.PesoLiquido, OleDbType.Numeric)
                        .Add("@PESO_BRUTO", oCadastroVolume.PesoBruto, OleDbType.Numeric)
                        .Add("@ALTURA", oCadastroVolume.Altura, OleDbType.Numeric)
                        .Add("@LARGURA", oCadastroVolume.Largura, OleDbType.Numeric)
                        .Add("@COMPRIMENTO", oCadastroVolume.Comprimento, OleDbType.Numeric)
                        .Add("@USUARIO", sUsuario, OleDbType.VarChar, 50)
                        .Add("@DATAOCORRENCIA", DateTime.Now, OleDbType.Date)
                        .Add("@OBSERVACAO", sObservacao, OleDbType.VarChar, 500)
                        ;

                    oCommand.SetCommandText(sQuery);

                    oCommand.Execute();

                }

                catch (Exception ex)
                {
                    sqoClassLog oLog = new sqoClassLog();

                    oLog.Verbose(oCommand.QueryToString()).Log();

                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Insert: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }

            }

        }


        public void GetPeca()
        {

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@DESCRICAO", this.oCadastroVolume.DescricaoPeca, OleDbType.VarChar, 50)
                    .Add("@TIPO_PECA", this.oCadastroVolume.TipoPeca, OleDbType.VarChar, 50)
                    .Add("@ATIVO", this.oCadastroVolume.AtivoPeca, OleDbType.Boolean, 50)
                    .Add("@CODIGO_VOLUME", this.oCadastroVolume.CodigoVolume, OleDbType.VarChar, 50)
                    ;

                String sQuery = @"SELECT
                                    @DESCRICAO = DESCRICAO
                                    ,@TIPO_PECA = TIPO_PECA
                                    ,@ATIVO = ATIVO
                                FROM 
                                    WSQOPCP2PECA 
                                WHERE 
                                    CODIGO_PECA = @CODIGO_VOLUME";

                oCommand.Command.Parameters["@DESCRICAO"].Direction = ParameterDirection.Output;
                oCommand.Command.Parameters["@TIPO_PECA"].Direction = ParameterDirection.Output;
                oCommand.Command.Parameters["@ATIVO"].Direction = ParameterDirection.Output;

                oCommand.SetCommandText(sQuery);

                try
                {
                    oCommand.Execute();
                    this.oCadastroVolume.DescricaoPeca = oCommand.Command.Parameters["@DESCRICAO"].Value.ToString();
                    this.oCadastroVolume.TipoPeca = oCommand.Command.Parameters["@TIPO_PECA"].Value.ToString();
                    this.oCadastroVolume.AtivoPeca = (Boolean)oCommand.Command.Parameters["@ATIVO"].Value;
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Query: " + oCommand.GetForLog() + Environment.NewLine, ex.InnerException);
                }
            }

        }

        /// <summary>
        /// Preenche o padrão com os valores da ação insert
        /// </summary>
        public void FillPersistence()
        {
            this.oCadastroVolume.Material = this.oCadastroVolume.MaterialInsert;
            this.oCadastroVolume.CodigoVolume = this.oCadastroVolume.CodigoVolumeInsert;
            this.oCadastroVolume.DescricaoVolume = this.oCadastroVolume.DescricaoVolumeInsert;
            this.oCadastroVolume.Quantidade = this.oCadastroVolume.QuantidadeInsert;
            this.oCadastroVolume.PesoLiquido = this.oCadastroVolume.PesoLiquidoInsert;
            this.oCadastroVolume.PesoBruto = this.oCadastroVolume.PesoBrutoInsert;
            this.oCadastroVolume.Altura = this.oCadastroVolume.AlturaInsert;
            this.oCadastroVolume.Largura = this.oCadastroVolume.LarguraInsert;
            this.oCadastroVolume.Comprimento = this.oCadastroVolume.ComprimentoInsert;
            this.oCadastroVolume.CodigoImagem = this.oCadastroVolume.CodigoImagemInsert;
            this.oCadastroVolume.TipoVolume = this.oCadastroVolume.TipoVolumeInsert;
        }

        //public void FillPersistenceDuplicate()
        //{
        //this.oCadastroVolume.CodigoVolume = this.oCadastroVolume.CodigoVolumeDuplicate;
        //}

        public void FillPersistenceLink()
        {
            this.oCadastroVolume.Material = this.oCadastroVolume.MaterialVinculo;
        }

        private bool EqualsCodigoVolume(String sCodigoVolume, long nId)
        {
            bool Result = true;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand

                    .Add("@CODIGO_VOLUME", sCodigoVolume, OleDbType.VarChar, 50)
                    .Add("@ID", nId, OleDbType.BigInt)
                    ;

                String sQuery = @"SELECT 1 FROM WSQOPCP2PECAVOLUME WHERE CODIGO_VOLUME = @CODIGO_VOLUME AND ID = @ID";

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
                        ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }
            }

            return Result;
        }

        private bool ExistCodigoVolume(String sCodigoVolume, String sCodigoPai)
        {
            bool Result = false;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand

                    .Add("@CODIGO_VOLUME", sCodigoVolume, OleDbType.VarChar, 50)
                    .Add("@CODIGO_PAI", sCodigoPai, OleDbType.VarChar, 50)
                    ;

                String sQuery = @"SELECT 1 FROM WSQOPCP2PECAVOLUME WHERE CODIGO_VOLUME = @CODIGO_VOLUME AND CODIGO_PAI = @CODIGO_PAI";

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
                        ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }
            }

            return Result;
        }

    }

}

/// <summary>
/// Classe para instanciar o modelo do criteria da tela.
/// </summary>
[XmlRoot("ItemFilaProducao")]
public class sqoClassCadastroVolume
{
    [XmlElement("ID")]
    public long Id { get; set; }

    [XmlElement("CODIGO_VOLUME_INSERT")]
    public String CodigoVolumeInsert { get; set; }

    [XmlElement("CODIGO_VOLUME_DUPLICATE")]
    public String CodigoVolumeDuplicate { get; set; }

    [XmlElement("CODIGO_VOLUME")]
    public String CodigoVolume { get; set; }

    [XmlElement("MATERIAL")]
    public String Material { get; set; }

    [XmlElement("MATERIAL_INSERT")]
    public String MaterialInsert { get; set; }

    [XmlElement("MATERIAL_VINCULO")]
    public String MaterialVinculo { get; set; }

    [XmlElement("DESCRICAO_VOLUME")]
    public String DescricaoVolume { get; set; }

    [XmlElement("DESCRICAO_VOLUME_INSERT")]
    public String DescricaoVolumeInsert { get; set; }

    [XmlElement("PESO_LIQUIDO")]
    public Double PesoLiquido { get; set; }

    [XmlElement("PESO_LIQUIDO_INSERT")]
    public Double PesoLiquidoInsert { get; set; }

    [XmlElement("PESO_BRUTO")]
    public double PesoBruto { get; set; }

    [XmlElement("PESO_BRUTO_INSERT")]
    public double PesoBrutoInsert { get; set; }

    [XmlElement("QUANTIDADE")]
    public Double Quantidade { get; set; }

    [XmlElement("QUANTIDADE_INSERT")]
    public Double QuantidadeInsert { get; set; }

    [XmlElement("COMPRIMENTO")]
    public Double Comprimento { get; set; }

    [XmlElement("COMPRIMENTO_INSERT")]
    public Double ComprimentoInsert { get; set; }

    [XmlElement("ALTURA")]
    public Double Altura { get; set; }

    [XmlElement("ALTURA_INSERT")]
    public Double AlturaInsert { get; set; }

    [XmlElement("LARGURA")]
    public Double Largura { get; set; }

    [XmlElement("LARGURA_INSERT")]
    public Double LarguraInsert { get; set; }

    [XmlElement("CODIGO_IMAGEM")]
    public String CodigoImagem { get; set; }

    [XmlElement("CODIGO_IMAGEM_INSERT")]
    public String CodigoImagemInsert { get; set; }

    [XmlElement("TIPO_VOLUME")]
    public String TipoVolume { get; set; }

    [XmlElement("TIPO_VOLUME_INSERT")]
    public String TipoVolumeInsert { get; set; }

    [XmlElement("ATIVO")]
    public Boolean Ativo { get; set; }

    [XmlElement("ATIVO_PECA")]
    public Boolean AtivoPeca { get; set; }

    [XmlElement("TIPO_PECA")]
    public String TipoPeca { get; set; }

    [XmlElement("DESCRICAO_PECA")]
    public String DescricaoPeca { get; set; }
}



#endif
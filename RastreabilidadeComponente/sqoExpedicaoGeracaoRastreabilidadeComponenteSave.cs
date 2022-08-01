using System.Data;
using sqoClassLibraryAI0502VariaveisSistema;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using sqoClassLibraryAI0502Biblio;
using sqoClassLibraryAI1151FilaProducao.Estrutura;
using AI1627Common20.TemplateDebugging;
using sqoClassLibraryAI1151FilaProducao.Process;
using System.Xml.Serialization;
using sqoClassLibraryAI1151FilaProducao;
using TemplatesStara.CommonStara;
using sqoClassLibraryAI0502Message;
using System.Linq;

namespace TemplateStara.Expedicao.RastreabilidadeComponente.Save
{
    [TemplateDebug("sqoExpedicaoGeracaoRastreabilidadeComponenteSave")]
    public class sqoExpedicaoGeracaoRastreabilidadeComponenteSave1 : IProcessMovimentacao
    {
        private sqoClassSetMessageDefaults oClassSetMessageDefaults;
        private sqoClassPersistenciaSerie oClassPersistenciaSerie;

        private string sModulo = string.Empty;
        private string sUsuario = string.Empty;

        private int nQtdErros = 0;
        private string sMessage = "Falha na validação de dados";
        private string sDescription = string.Empty;
        private wsqoPcp2PecaVolume oseqIdCodVolumePersistence;
        private wsqolExpedicaoComponenteGeracao owsqolExpedicaoComponenteGeracaoPercistence;

        public sqoClassMessage Executar(string sAction, string sXmlDados, string sXmlType, List<sqoClassParametrosEstrutura> oListaParametrosMovimentacao, List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sNivel, string sUsuario, object oObjAux)
        {
            using (sqoClassDbConnection oDBConnection = new sqoClassDbConnection())
            {
                this.Init(sXmlDados, oListaParametrosListagem, sUsuario);

                this.Validate();

                this.ProcessBusinessLogic(oDBConnection);
            }

            return oClassSetMessageDefaults.Message;
        }

        private void Init(string sXmlDados, List<sqoClassParametrosEstrutura> oListaParametrosListagem, string sUsuario)
        {
            this.oClassSetMessageDefaults = new sqoClassSetMessageDefaults(new sqoClassDefaultResposta());

            this.oClassPersistenciaSerie = new sqoClassPersistenciaSerie();

            this.oClassPersistenciaSerie = sqoClassBiblioSerDes.DeserializeObject<sqoClassPersistenciaSerie>(sXmlDados);

            this.sUsuario = sUsuario;
        }

        private void Validate()
        {
            foreach (var oItemRegra in this.oClassPersistenciaSerie.ListaRegrasItemFilaProducao)
            {

                List<sqoClassPersistenciaSerie> osqoClassPersistenciaSerieNR = new List<sqoClassPersistenciaSerie>();

                var ParenteExists = this.oClassPersistenciaSerie.ListaRegrasItemFilaProducao.FindAll(x => x.NumeroSerie == oItemRegra.NumeroSerie).Count > 1;

                if (string.IsNullOrEmpty(oItemRegra.NumeroSerie))
                {
                    this.nQtdErros++;

                    this.sDescription += this.nQtdErros.ToString() + " - Número Série é obrigatório, favor preencher!" + Environment.NewLine;
                }

                if (ParenteExists)
                {
                    this.nQtdErros++;

                    this.sDescription += this.nQtdErros.ToString() + " - Número Série: " + oItemRegra.NumeroSerie + " Repetido para o componente: " + oItemRegra.DescricaoComponente + " - Número Série deve ser único para cada componente!" + Environment.NewLine;
                }

            }

            this.ValidateMessage();
        }

        private void ValidateMessage()
        {
            if (!String.IsNullOrEmpty(sDescription))
            {
                string sMessageDescription = nQtdErros > 1 ? ("Encontrados " + nQtdErros + " erros!")
                    : ("Encontrado " + nQtdErros + " erro!");

                string sMessageBody = sMessageDescription + Environment.NewLine + sDescription;

                CommonStara.MessageBox(false, this.sMessage, sMessageBody, sqoClassMessage.MessageTypeEnum.ERROR, oClassSetMessageDefaults);

                throw new sqoClassMessageUserException(oClassSetMessageDefaults.Message);
            }
        }

        private void ProcessBusinessLogic(sqoClassDbConnection oDBConnection)
        {
            int nQtdContador = 0;
            int nContadorCount = 0;
            string sContadorTotal = string.Empty;

            try
            {
                oDBConnection.BeginTransaction();

                nContadorCount = oClassPersistenciaSerie.ListaRegrasItemFilaProducao.Count;

                foreach (var oItemRegra in this.oClassPersistenciaSerie.ListaRegrasItemFilaProducao)
                {
                    nQtdContador++;

                    sContadorTotal = nQtdContador.ToString() + "/" + nContadorCount.ToString();

                    IdNumeroSerieComponenteExpedicaoPersistence wsqolExpedicaoComponenteGeracaoPersistence = new IdNumeroSerieComponenteExpedicaoPersistence(null);

                    this.owsqolExpedicaoComponenteGeracaoPercistence = wsqolExpedicaoComponenteGeracaoPersistence.GetIdCompGeracao(this.oClassPersistenciaSerie.Material, oItemRegra.DescricaoComponente, oItemRegra.NumeroSerie);

                    if (owsqolExpedicaoComponenteGeracaoPercistence != null)
                    {
                        //if (owsqolExpedicaoComponenteGeracaoPercistence.NumeroSerie == oItemRegra.NumeroSerie)
                        //{
                        //this.SetIdCodComponenteGeracaoNrSerie(oItemRegra.NumeroSerie, owsqolExpedicaoComponenteGeracaoPercistence.Id);
                        //}

                        
                        if(this.GetIdNrSerie(oItemRegra.NumeroSerie) > 0)
                        {
                            this.CleanGeracaoDados(sUsuario, oItemRegra.IdGeracao);
                        }

                        this.SetIdCodVolumeGeracaoNSerie(oItemRegra.NumeroSerie, oClassPersistenciaSerie.IdVol, oClassPersistenciaSerie.CodigoVolume, owsqolExpedicaoComponenteGeracaoPercistence.Id, sUsuario, oItemRegra.IdComponente);

                    }

                    else
                    {
                        //long GetIdComponente = this.GetIdComponente(this.oClassPersistenciaSerie.Material, oItemRegra.DescricaoComponente);

                        if(oItemRegra.IdGeracao > 0)
                        {
                            this.SetIdCodVolumeGeracaoNSerie(oItemRegra.NumeroSerie, oClassPersistenciaSerie.IdVol, oClassPersistenciaSerie.CodigoVolume, oItemRegra.IdGeracao, sUsuario, oItemRegra.IdComponente);
                        }

                        else

                        this.InsertData(this.oClassPersistenciaSerie.Material
                            , oItemRegra.DescricaoComponente
                            , oItemRegra.DocReferencia
                            , oItemRegra.NumeroSerie
                            , oItemRegra.IdComponente
                            , oClassPersistenciaSerie.IdVol
                            , oClassPersistenciaSerie.CodigoVolume
                            , sUsuario);
                    }

                    //this.VinculoVolumeNrSerie(oItemRegra.NumeroSerie, oClassPersistenciaSerie.Material);
                }

                this.oClassSetMessageDefaults.SetarOk();

                oDBConnection.Commit();
            }
            catch (Exception ex)
            {
                sqoClassMessageUserException oClassMessageUserException =
                   new sqoClassMessageUserException("Error" + Environment.NewLine + ex.Message, ex.InnerException);
                oDBConnection.Rollback();

                throw oClassMessageUserException;
            }
        }

        private void VinculoVolumeNrSerie(string NSerie, string Material)
        {
            long IdComponenteGeracao = -1;

            IdComponenteGeracao = this.GetIdComponenteGeracao(NSerie);

            if (IdComponenteGeracao > 0)
            {
                IdCodVolumePersistence seqIdCodVolumePersistence = new IdCodVolumePersistence(null);

                this.oseqIdCodVolumePersistence = seqIdCodVolumePersistence.GetIdCodVolume(Material);

                this.SetIdCodComponenteGeracaoNrSerie(oseqIdCodVolumePersistence, IdComponenteGeracao);
            }

        }

        public class IdCodVolumePersistence
        {
            private object objContexto;

            public IdCodVolumePersistence(object objContexto)
            {
                this.objContexto = objContexto;
            }

            public wsqoPcp2PecaVolume GetIdCodVolume(string Material)
            {
                string sQuery = @"SELECT 
	                                 ID
	                                ,CODIGO_RASTREABILIDADE
                                 FROM
	                                WSQOLEXPEDICAOVOLUME
                                 WHERE
	                                CODIGO_VOLUME = ?";

                using (sqoCommand oCommand = new sqoCommand(this.objContexto))
                {
                    oCommand
                        .SetCommandText(sQuery)
                        .Add("@MATERIAL", Material, OleDbType.VarChar);


                    var result = oCommand.GetResultado<wsqoPcp2PecaVolume>();

                    return oCommand.GetResultado<wsqoPcp2PecaVolume>();
                }
            }
        }

        private void SetIdCodComponenteGeracaoNrSerie(wsqoPcp2PecaVolume oseqIdCodVolumePersistence, long IdComponenteGeracao)
        {
            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand.SetCommandText(@"UPDATE
                                             WSQOLEXPEDICAOCOMPONENTEGERACAONUMEROSERIE
                                          SET 
                                             ID_VOLUME = ?
                                            ,CODIGO_VOLUME = ?
                                          WHERE 
                                            ID = ? ")

                .Add("@ID_VOLUME", oseqIdCodVolumePersistence.Id, OleDbType.Integer)
                .Add("@CODIGO_VOLUME", oseqIdCodVolumePersistence.CodigoRastreabilidade, OleDbType.VarChar, 50)
                .Add("@ID", IdComponenteGeracao, OleDbType.Integer);

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

        private void SetIdVolume(wsqoPcp2PecaVolume oseqIdCodVolumePersistence, long IdComponenteGeracao)
        {
            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand.SetCommandText(@"UPDATE
                                             WSQOLEXPEDICAOCOMPONENTEGERACAONUMEROSERIE
                                          SET 
                                             ID_VOLUME = ?
                                            ,CODIGO_VOLUME = ?
                                          WHERE 
                                            ID = ? ")

                .Add("@ID_VOLUME", oseqIdCodVolumePersistence.Id, OleDbType.Integer)
                .Add("@CODIGO_VOLUME", oseqIdCodVolumePersistence.CodigoRastreabilidade, OleDbType.VarChar, 50)
                .Add("@ID", IdComponenteGeracao, OleDbType.Integer);

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

        private void InsertData(string sMaterial
                              , string sComponente
                              , string sOrdemProducao
                              , string sNumeroSerie
                              , long IdComponente
                              , int IdVol
                              , string CodigoVolume
                              , string sUsuario)
        {
            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@MATERIAL", sMaterial, OleDbType.VarChar, 50)
                    .Add("@ORDEM_PRODUCAO", sOrdemProducao, OleDbType.VarChar, 50)
                    .Add("@DATA_IMPRESSAO", DateTime.Now, OleDbType.DBTimeStamp)
                    .Add("@USUARIO", this.sUsuario, OleDbType.VarChar, 50)
                    .Add("@NUMERO_SERIE", sNumeroSerie, OleDbType.VarChar, 50)
                    .Add("@ID_COMPONENTE", IdComponente)
                    .Add("@ID_VOLUME", IdVol)
                    .Add("@CODIGO_VOLUME", CodigoVolume)
                    .Add("@USUARIO_VINCULO", sUsuario)
                    ;

                string sQuery = @"INSERT INTO [dbo].[WSQOLEXPEDICAOCOMPONENTEGERACAONUMEROSERIE]
                                       ([CODIGO_PECA]
                                       ,[DOC_REFERENCIA]
                                       ,[DATA]
                                       ,[USUARIO]
                                       ,[NUMERO_SERIE]
									   ,[ID_COMPONENTE]
                                       ,[ID_VOLUME]
                                       ,[CODIGO_VOLUME]
                                       ,[USUARIO_VINCULO])
                                 VALUES
                                       (?, ?, ?, ?, ?, ?, ?, ?, ?)";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.GetResultado();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                       ex.Message + Environment.NewLine + "Erro ao executar Proc: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }
            }
        }

        private void SetIdCodVolumeGeracaoNSerie(string NumeroSerie, int IdVolume, string CodigoVolume, long Id, string sUsuario, int IdComponente)
        {
            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand.SetCommandText(@"UPDATE
                                            WSQOLEXPEDICAOCOMPONENTEGERACAONUMEROSERIE
                                          SET 
                                             NUMERO_SERIE = ?
                                            ,ID_VOLUME  = ?
                                            ,CODIGO_VOLUME = ?
                                            ,USUARIO_VINCULO = ?
                                            ,ID_COMPONENTE = ?
                    

                                          WHERE 
                                             ID = ? ")

                .Add("@NUMERO_SERIE", NumeroSerie)
                .Add("@ID_VOLUME", IdVolume)
                .Add("@CODIGO_VOLUME", CodigoVolume)
                .Add("@USUARIO_VINCULO", sUsuario)
                .Add("@ID_COMPONENTE", IdComponente)
                .Add("@ID", Id);

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

        private void CleanGeracaoDados(string sUsuario, int IdGeracao)
        {
            object NullValue = DBNull.Value;


            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand.SetCommandText(@"UPDATE
                                            WSQOLEXPEDICAOCOMPONENTEGERACAONUMEROSERIE
                                          SET 
                                             ID_VOLUME = ?
                                            ,CODIGO_VOLUME = ?
                                            ,ID_COMPONENTE = ?
                                            ,USUARIO_VINCULO = ?
                                          WHERE 
                                             ID = ? ")

                .Add("@ID_VOLUME", NullValue)
                .Add("@CODIGO_VOLUME", NullValue)
                .Add("@ID_COMPONENTE", NullValue)
                .Add("@USUARIO_VINCULO", sUsuario)
                .Add("@ID", IdGeracao);

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

        private bool ValidaNumeroSerieExists(string NumeroSerie, string OrdemProducao)
        {
            bool Result = false;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@NUMERO_SERIE", NumeroSerie, OleDbType.VarChar, 50)
                    .Add("@ORDEM_PRODUCAO", OrdemProducao, OleDbType.VarChar, 50)
                    ;

                string sQuery = @"SELECT 
                                     1 
                                  FROM 
                                     WSQOLEXPEDICAOCOMPONENTEGERACAONUMEROSERIE
                                  WHERE
                                     NUMERO_SERIE = ?                  
                                  AND
                                     DOC_REFERENCIA <> ?";

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

        private long GetIdComponenteGeracao(string NSerie)
        {
            long oIdComponenteGeracaoNrSerie = -1;


            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@NUMERO_SERIE", NSerie, OleDbType.VarChar)
                    ;

                string sQuery = @"SELECT
		                              ID
                                  FROM
	                                  WSQOLEXPEDICAOCOMPONENTEGERACAONUMEROSERIE
                                  WHERE
	                                  NUMERO_SERIE = ?";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.GetResultado();

                    if (oResult != null)
                    {
                        oIdComponenteGeracaoNrSerie = (long)oResult;
                    }

                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }

                return oIdComponenteGeracaoNrSerie;
            }
        }

        private long GetIdComponente(string CodigoPeca, string DescricaoComponente)
        {
            long IdComponente = -1;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@NUMERO_SERIE", CodigoPeca, OleDbType.VarChar)
                    .Add("@NUMERO_SERIE", DescricaoComponente, OleDbType.VarChar)
                    ;

                string sQuery = @"SELECT 
	                                  ID
                                  FROM
	                                  WSQOLPCP2PECACOMPONENTE
                                  WHERE
	                                  CODIGO_PECA = ?
                                  AND
	                                  DESCRICAO_COMPONENTE = ?";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.GetResultado();

                    if (oResult != null)
                    {
                        IdComponente = (long)oResult;
                    }

                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }

                return IdComponente;
            }
        }

        private int GetIdNrSerie(string NrSerie)
        {
            int IdNumeroSerie = 0;

            using (var oCommand = new sqoCommand(CommandType.Text))
            {
                oCommand
                    .Add("@NUMERO_SERIE", NrSerie, OleDbType.VarChar)
                    ;

                string sQuery = @"SELECT
	                                  ID
                                  FROM
	                                  WSQOLEXPEDICAOCOMPONENTEGERACAONUMEROSERIE
                                  WHERE
	                                 NUMERO_SERIE = ?";

                try
                {
                    oCommand.SetCommandText(sQuery);

                    var oResult = oCommand.GetResultado();

                    if (oResult != null)
                    {
                        IdNumeroSerie = Convert.ToInt32(oResult);
                    }

                }
                catch (Exception ex)
                {
                    throw new Exception(
                        ex.Message + Environment.NewLine + "Erro ao executar Query: " + oCommand.QueryToString() + Environment.NewLine, ex.InnerException);
                }

                return IdNumeroSerie;
            }
        }

        public class IdNumeroSerieComponenteExpedicaoPersistence
        {
            private object objContexto;

            public IdNumeroSerieComponenteExpedicaoPersistence(object objContexto)
            {
                this.objContexto = objContexto;
            }

            public wsqolExpedicaoComponenteGeracao GetIdCompGeracao(string Material, string DescricaoComponente, string NumeroSerie)
            {
                string sQuery = @"SELECT 
		                                ID
                                    FROM 
	                                    WSQOLEXPEDICAOCOMPONENTEGERACAONUMEROSERIE
                                    WHERE
                                        NUMERO_SERIE = ?";

                using (sqoCommand oCommand = new sqoCommand(this.objContexto))
                {
                    oCommand
                        .SetCommandText(sQuery)
                        .Add("@NUMERO_SERIE", NumeroSerie, OleDbType.VarChar)
                        ;

                    var result = oCommand.GetResultado<wsqolExpedicaoComponenteGeracao>();

                    if (result != null)
                        return result;
                    else
                        return null;
                }
            }
        }
    }

    [XmlRoot("ItemFilaProducao")]
    public class sqoClassPersistenciaSerie
    {

        [XmlElement("MATERIAL")]
        public string Material { get; set; }

        [XmlElement("DESCRICAO_MATERIAL")]
        public string DescricaoMaterial { get; set; }

        [XmlElement("DOC_REFERENCIA")]
        public string DocReferencia { get; set; }

        [XmlElement("NR_SERIE")]
        public string NrSerie { get; set; }

        [XmlElement("ID_VOL")]
        public int IdVol { get; set; }

        [XmlElement("CODIGO_VOLUME")]
        public string CodigoVolume { get; set; }

        [XmlElement("ID_COMPONENTE")]
        public int IdComponente { get; set; }

        [XmlElement("ID_GERACAO")]
        public int IdGeracao { get; set; }

        private List<sqoClassRegraItemFilaProducaoEstrutura> oListaRegrasItemFilaProducao = new List<sqoClassRegraItemFilaProducaoEstrutura>();

        [XmlArray("Regras")]
        [XmlArrayItem("Regra", typeof(sqoClassRegraItemFilaProducaoEstrutura))]
        public List<sqoClassRegraItemFilaProducaoEstrutura> ListaRegrasItemFilaProducao
        {
            get { return oListaRegrasItemFilaProducao; }
            set { oListaRegrasItemFilaProducao = value; }
        }
    }

    [XmlRoot("Regra")]
    public class sqoClassRegraItemFilaProducaoEstrutura
    {
        private string sRegra = "";

        public string Regra
        {
            get { return sRegra; }
            set { sRegra = value; }
        }

        public int Id
        { get; set; }

        public string NumeroSerie
        { get; set; }

        public string DescricaoComponente
        { get; set; }

        public string DocReferencia
        { get; set; }

        public int IdComponente
        { get; set; }

        public int IdGeracao
        { get; set; }

    }

    [AutoPersistencia]
    public class wsqoPcp2PecaVolume
    {
        public int Id { get; set; }
        public string CodigoRastreabilidade { get; set; }

    }

    [AutoPersistencia]
    public class wsqolExpedicaoComponenteGeracao
    {
        public int Id { get; set; }
        public string NumeroSerie { get; set; }
    }
}
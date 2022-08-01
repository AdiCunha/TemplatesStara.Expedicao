using System;
using System.Globalization;
using System.Xml.Serialization;

namespace sqoTraceabilityStation
{
    //essa dependencia é a q está sendo utilizada

    [XmlRoot("ItemFilaProducao")]
    public class sqoClassPcpDynCriteriaItem
    {
        private int nID = -1;
        [XmlElement("ID")]
        public int ID
        {
            get { return nID; }
            set { nID = value; }
        }

        private string iLocalExpedicao = "";
        [XmlElement("Local Expedicao")]
        public string Local_Expedicao
        {
            get { return iLocalExpedicao; }
            set { iLocalExpedicao = value; }
        }

        private string sChaveExpedicao = "";
        [XmlElement("Chave Expedicao")]
        public string Chave_Expedicao
        {
            get { return sChaveExpedicao; }
            set { sChaveExpedicao = value; }
        }

        private string sPais = "";
        [XmlElement("País")]
        public string Pais
        {
            get { return sPais; }
            set { sPais = value; }
        }

        private string sTipo = "";
        [XmlElement("Tipo")]
        public string Tipo_Expedicao_Codigo
        {
            get { return sTipo; }
            set { sTipo = value; }
        }

        private DateTime dUltimaEdicao;
        [XmlElement("Data Última Edição")]
        public DateTime Last_Update_Date
        {
            get { return dUltimaEdicao; }
            set { dUltimaEdicao = value; }
        }

        private string sUsuario = "";
        [XmlElement("Usuario")]
        public string Last_Update_User
        {
            get { return sUsuario; }
            set { sUsuario = value; }
        }

        private string sMensagem = "";
        [XmlElement("Mensagem")]
        public string Message
        {
            get { return sMensagem; }
            set { sMensagem = value; }
        }
    }
}

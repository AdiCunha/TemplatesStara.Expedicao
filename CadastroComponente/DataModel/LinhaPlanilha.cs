using Common.Stara.Common.DataModel;
using sqoClassLibraryAI0502Biblio;
using System.Data.OleDb;

namespace sqoTraceabilityStation
{
    [AutoPersistencia]
    [PlanilhaInfo("WSQOLPCP2PECACOMPONENTE")]
    public class LinhaPlanilha
    {
        [PlanilhaColunaInfo(1, "MATERIAL", OleDbType.VarChar, whereKeyColumn: false)]
        public string Material { get; set; }

        [PlanilhaColunaInfo(2, "DESCRICAO_COMPONENTE", OleDbType.VarChar, whereKeyColumn: false)]
        public string DescricaoComponente { get; set; }

        [PlanilhaColunaInfo(3, "TIPO_COMPONENTE", OleDbType.Integer, whereKeyColumn: false)]
        public string TipoComponente { get; set; }

        [PlanilhaColunaInfo(4, "OPERACAO", OleDbType.VarChar, whereKeyColumn: false)]
        public string Operacao { get; set; }

        [PlanilhaColunaInfo(5, "GRUPO", OleDbType.VarChar, whereKeyColumn: false)]
        public string Grupo { get; set; }

        [PlanilhaColunaInfo(6, "OBSERVACAO", OleDbType.VarChar, whereKeyColumn: false)]
        public string Observacao { get; set; }

        public int LineNumber { get; set; }
    }
}
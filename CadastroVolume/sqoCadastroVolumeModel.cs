using Common.Stara.Common.DataModel;
using sqoClassLibraryAI0502Biblio;
using System.Data.OleDb;

namespace TemplateStara.Expedicao.CadastroVolume
{
    [AutoPersistencia]
    [PlanilhaInfo("")]
    public class sqoCadastroVolumeModel
    {
        [PlanilhaColunaInfo(1, "MATERIAL", OleDbType.VarChar, whereKeyColumn: false)]
        public string Material { get; set; }

        [PlanilhaColunaInfo(2, "CODIGO_VOLUME", OleDbType.VarChar, whereKeyColumn: false)]
        public string CodigoVolume { get; set; }

        [PlanilhaColunaInfo(3, "DESCRICAO_VOLUME", OleDbType.VarChar, whereKeyColumn: false)]
        public string DescricaoVolume { get; set; }

        [PlanilhaColunaInfo(4, "QUANTIDADE", OleDbType.VarChar, whereKeyColumn: false)]
        public string Quantidade { get; set; }

        [PlanilhaColunaInfo(5, "PESO_LIQUIDO", OleDbType.VarChar, whereKeyColumn: false)]
        public string PesoLiquido { get; set; }

        [PlanilhaColunaInfo(6, "PESO_BRUTO", OleDbType.VarChar, whereKeyColumn: false)]
        public string PesoBruto { get; set; }

        [PlanilhaColunaInfo(7, "ALTURA", OleDbType.VarChar, whereKeyColumn: false)]
        public string Altura { get; set; }

        [PlanilhaColunaInfo(8, "LARGURA", OleDbType.VarChar, whereKeyColumn: false)]
        public string Largura { get; set; }

        [PlanilhaColunaInfo(9, "COMPRIMENTO", OleDbType.VarChar, whereKeyColumn: false)]
        public string Comprimento { get; set; }

        [PlanilhaColunaInfo(10, "CODIGO_IMAGEM", OleDbType.VarChar, whereKeyColumn: false)]
        public string CodigoImagem { get; set; }

        [PlanilhaColunaInfo(11, "TIPO_EXPEDICAO", OleDbType.VarChar, whereKeyColumn: false)]
        public string TipoExpedicao { get; set; }

        [PlanilhaColunaInfo(12, "OPERACAO", OleDbType.VarChar, whereKeyColumn: false)]
        public string Operacao { get; set; }

        [PlanilhaColunaInfo(13, "ATIVO", OleDbType.VarChar, whereKeyColumn: false)]
        public string Ativo { get; set; }

        [PlanilhaColunaInfo(-1, "ID", OleDbType.Integer, whereKeyColumn: false)]
        public long Id { get; set; }

        public int LineNumber { get; set; }
    }
}
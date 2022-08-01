using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using sqoClassLibraryAI1151FilaProducao;
using System.Collections.Generic;
using System.IO;

namespace TemplateStara.Expedicao.CadastroComponente.Business
{
   public class ProcessImpCadCompDownloadExcel
    {
        public object DownloadExcel()
        {
            HSSFWorkbook oWorkbook = new HSSFWorkbook();

            oWorkbook.CreateSheet("Planilha");

            HSSFSheet oSheet = (HSSFSheet)oWorkbook.GetSheetAt(0);

            oSheet.CreateRow(0);
            oSheet.DefaultColumnWidth = 25;

            CellRangeAddressList addressList = new CellRangeAddressList(0, 65535, 2, 2);
            DVConstraint dvConstraint = DVConstraint.CreateExplicitListConstraint(new string[] { "10", "20" });
            HSSFDataValidation dataValidation = new HSSFDataValidation(addressList, dvConstraint);
            dataValidation.SuppressDropDownArrow = false;
            oSheet.AddValidationData(dataValidation);

            addressList = new CellRangeAddressList(0, 65535, 3, 3);
            dvConstraint = DVConstraint.CreateExplicitListConstraint(new string[] { "I", "A", "E" });
            dataValidation = new HSSFDataValidation(addressList, dvConstraint);
            dataValidation.SuppressDropDownArrow = false;
            oSheet.AddValidationData(dataValidation);

            HSSFRow oRow = (HSSFRow)oSheet.GetRow(0);

            var oFont = oWorkbook.CreateFont();
            oFont.Boldweight = HSSFFont.BOLDWEIGHT_BOLD;
            oFont.Color = HSSFColor.WHITE.index;

            var oStyle = oWorkbook.CreateCellStyle();
            oStyle.FillForegroundColor = HSSFColor.GREEN.index;
            oStyle.FillBackgroundColor = HSSFColor.GREEN.index;
            oStyle.FillPattern = HSSFCellStyle.SOLID_FOREGROUND;
            oStyle.Alignment = HSSFCellStyle.ALIGN_CENTER;
            oStyle.VerticalAlignment = HSSFCellStyle.VERTICAL_CENTER;

            oStyle.BorderBottom = HSSFCellStyle.BORDER_THIN;
            oStyle.BottomBorderColor = HSSFColor.BLACK.index;
            oStyle.BorderLeft = HSSFCellStyle.BORDER_THIN;
            oStyle.LeftBorderColor = HSSFColor.BLACK.index;
            oStyle.BorderRight = HSSFCellStyle.BORDER_THIN;
            oStyle.RightBorderColor = HSSFColor.BLACK.index;
            oStyle.BorderTop = HSSFCellStyle.BORDER_THIN;
            oStyle.TopBorderColor = HSSFColor.BLACK.index;

            oStyle.WrapText = true;

            List<string> oListFiedlExcel = this.FieldExcel();

            int index = 0;

            foreach (var oFieldExcel in oListFiedlExcel)
            {
                HSSFRichTextString richString = new HSSFRichTextString(oFieldExcel);
                var oCell = oRow.CreateCell(index);
                oCell.CellStyle = oStyle;
                oCell.CellStyle.SetFont(oFont);
                oCell.SetCellValue(richString);
                index++;
            }

            using (MemoryStream fs = new MemoryStream())
            {
                oWorkbook.Write(fs);

                return Tools.SaveFileToDownload(fs.ToArray(), "xls");
            }
        }

        public List<string> FieldExcel()
        {
            List<string> oListFiedExcel = new List<string>();

            oListFiedExcel.Add("Material");
            oListFiedExcel.Add("Descrição Componente");
            oListFiedExcel.Add("Tipo Componente (10 = Rastreabilidade, 20 = Geração Número de Série)");
            oListFiedExcel.Add("Operação (I = Inserir, A = Alterar, E = Excluir)");
            oListFiedExcel.Add("Grupo");
            oListFiedExcel.Add("Observação");

            return oListFiedExcel;
        }
    }
}

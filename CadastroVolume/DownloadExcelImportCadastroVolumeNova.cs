using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using sqoClassLibraryAI1151FilaProducao;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplateStara.Expedicao.CadastroVolume
{
    public class DownloadExcelImportCadastroVolumeNova
    {
        public object DownloadExcel()
        {
            HSSFWorkbook oWorkbook = new HSSFWorkbook();

            oWorkbook.CreateSheet("Planilha");

            HSSFSheet oSheet = (HSSFSheet)oWorkbook.GetSheetAt(0);

            oSheet.CreateRow(0);
            oSheet.DefaultColumnWidth = 25;

            CellRangeAddressList addressList = new CellRangeAddressList(0, 65535, 12, 12);
            DVConstraint dvConstraint = DVConstraint.CreateExplicitListConstraint(new string[] { "TRUE", "FALSE" });
            HSSFDataValidation dataValidation = new HSSFDataValidation(addressList, dvConstraint);
            dataValidation.SuppressDropDownArrow = false;
            oSheet.AddValidationData(dataValidation);

            addressList = new CellRangeAddressList(0, 65535, 11, 11);
            dvConstraint = DVConstraint.CreateExplicitListConstraint(new string[] { "I", "A" });
            dataValidation = new HSSFDataValidation(addressList, dvConstraint);
            dataValidation.SuppressDropDownArrow = false;
            oSheet.AddValidationData(dataValidation);

            HSSFRow oRow = (HSSFRow)oSheet.GetRow(0);

            var oFont = oWorkbook.CreateFont();
            oFont.Boldweight = HSSFFont.BOLDWEIGHT_BOLD;
            oFont.Color = HSSFColor.WHITE.index;

            var oStyle = oWorkbook.CreateCellStyle();
            oStyle.FillForegroundColor = HSSFColor.BLUE.index;
            oStyle.FillBackgroundColor = HSSFColor.BLUE.index;
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

        private List<string> FieldExcel()
        {
            List<string> oListFiedExcel = new List<string>();

            oListFiedExcel.Add("Material");
            oListFiedExcel.Add("Codigo Volume");
            oListFiedExcel.Add("Descrição Volume");
            oListFiedExcel.Add("Quantidade");
            oListFiedExcel.Add("Peso Líquido");
            oListFiedExcel.Add("Peso Bruto");
            oListFiedExcel.Add("Altura");
            oListFiedExcel.Add("Largura");
            oListFiedExcel.Add("Comprimento");
            oListFiedExcel.Add("Codigo Imagem");
            oListFiedExcel.Add("Tipo Expedição");  
            oListFiedExcel.Add("Operação (I = Inserir, A = Alterar)");
            oListFiedExcel.Add("Ativo (TRUE - FALSE)");

            return oListFiedExcel;
        }
    }
}

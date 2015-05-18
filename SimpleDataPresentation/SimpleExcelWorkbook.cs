using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SimpleUtils {
    public class SimpleExcelWorkbook {
        private IWorkbook workbook;
        private Dictionary<string, XSSFCellStyle> cellStyles = new Dictionary<string, XSSFCellStyle>();

        public SimpleExcelWorkbook() {
            this.workbook = new XSSFWorkbook();

            // Set headers font.
            XSSFCellStyle bigTitleStyle = (XSSFCellStyle)workbook.CreateCellStyle();
            XSSFFont bigTitleFont = (XSSFFont)workbook.CreateFont();
            bigTitleFont.Boldweight = (short)FontBoldWeight.Bold;
            bigTitleFont.FontHeightInPoints = 20;
            bigTitleStyle.SetFont( bigTitleFont );
            this.cellStyles.Add( "BigTitle", bigTitleStyle );

            XSSFCellStyle titleStyle = (XSSFCellStyle)workbook.CreateCellStyle();
            XSSFFont titleFont = (XSSFFont)workbook.CreateFont();
            titleFont.Boldweight = (short)FontBoldWeight.Bold;
            titleStyle.SetFont( titleFont );
            this.cellStyles.Add( "Title", titleStyle );
        }

        public XSSFCellStyle GetCellStyle( string indexIn ) {
            if( this.cellStyles.ContainsKey( indexIn ) ) {
                return this.cellStyles[indexIn];
            } else {
                return null;
            }
        }

        public XSSFCellStyle CreateCellStyle( string indexIn ) {
            XSSFCellStyle styleOut = (XSSFCellStyle)this.workbook.CreateCellStyle();
            this.cellStyles.Add( indexIn, styleOut );
            return styleOut;
        }

        public ISheet GetSheet( string titleIn ) {
            return this.workbook.GetSheet( titleIn );
        }

        public ISheet CreateSheet( string titleIn ) {
            return this.workbook.CreateSheet( titleIn );
        }

        public XSSFFont CreateFont() {
            return (XSSFFont)this.workbook.CreateFont();
        }

        public SimpleExcelSheet CreateSheet( string titleIn, SimpleExcelColumn[] columnsIn , IEnumerable<object> rowsIn ) {
            SimpleExcelSheet sheetOut = new SimpleExcelSheet( this.workbook.CreateSheet( titleIn ) );

            sheetOut.PrintSheetHeader( this.GetCellStyle( "Title" ), titleIn );

            sheetOut.PrintSheetTable( columnsIn, rowsIn );

            return sheetOut;
        }

        public void Stream( Stream target ) {
            this.workbook.Write( target );
        }
    }
}

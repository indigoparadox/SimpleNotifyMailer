using NPOI.HSSF.UserModel;
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
        private Dictionary<string, ICellStyle> cellStyles = new Dictionary<string, ICellStyle>();
        private bool useXlsx;

        public SimpleExcelWorkbook() : this( true ) { }

        public SimpleExcelWorkbook( bool useXlsxIn ) {
            if( useXlsxIn ) {
                this.workbook = new XSSFWorkbook();
            } else {
                this.workbook = new HSSFWorkbook();
            }

            this.useXlsx = useXlsxIn;

            // Set headers font.
            ICellStyle bigTitleStyle = workbook.CreateCellStyle();
            IFont bigTitleFont = workbook.CreateFont();
            bigTitleFont.Boldweight = (short)FontBoldWeight.Bold;
            bigTitleFont.FontHeightInPoints = 20;
            bigTitleStyle.SetFont( bigTitleFont );
            this.cellStyles.Add( "BigTitle", bigTitleStyle );

            ICellStyle titleStyle = workbook.CreateCellStyle();
            IFont titleFont = workbook.CreateFont();
            titleFont.Boldweight = (short)FontBoldWeight.Bold;
            titleStyle.SetFont( titleFont );
            this.cellStyles.Add( "Title", titleStyle );
        }

        public ICellStyle GetCellStyle( string indexIn ) {
            if( this.cellStyles.ContainsKey( indexIn ) ) {
                return this.cellStyles[indexIn];
            } else {
                return null;
            }
        }

        public ICellStyle CreateCellStyle( string indexIn ) {
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
            SimpleExcelSheet sheetOut = new SimpleExcelSheet( this.workbook.CreateSheet( titleIn ), useXlsx ? SimpleExcelSheet.MAX_ROWS_XSSF : SimpleExcelSheet.MAX_ROWS_HSSF );

            sheetOut.PrintSheetHeader( this.GetCellStyle( "Title" ), titleIn );

            sheetOut.PrintSheetTable(
                useXlsx ? SimpleExcelSheet.MAX_ROWS_XSSF : SimpleExcelSheet.MAX_ROWS_HSSF,
                columnsIn,
                rowsIn
            );

            return sheetOut;
        }

        public void Stream( Stream target ) {
            this.workbook.Write( target );
        }
    }
}

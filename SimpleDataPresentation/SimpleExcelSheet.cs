using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SimpleUtils {


    public class SimpleExcelSheet {

        public delegate string ConnectionPrintCallback( object rowIn );
        public delegate ICellStyle ConnectionStyleCallback( object rowIn );

        public enum ColumnSizeMethod {
            Automatic,
            Manual
        }

        private IWorkbook workbook;
        private Dictionary<string, XSSFCellStyle> cellStyles = new Dictionary<string, XSSFCellStyle>();

        public class ColumnDefinition {
            public int Index { get; set; }
            public string Title { get; set; }
            public ICellStyle TitleStyle { get; set; }
            public ConnectionStyleCallback CellStyleCallback { get; set; }
            public ConnectionPrintCallback CellPrintCallback { get; set; }
            public ColumnSizeMethod SizeMethod { get; set; }
            public int SizeWidth { get; set; }
            public bool BlankIsUnknown { get; set; }

            public ColumnDefinition(
                int indexIn,
                string titleIn,
                ICellStyle titleStyleIn,
                ConnectionPrintCallback cellPrintCallbackIn,
                bool blankIsUnknownIn
            )
                : this( indexIn, titleIn, titleStyleIn, null, cellPrintCallbackIn, blankIsUnknownIn ) {

            }

            public ColumnDefinition(
                int indexIn,
                string titleIn,
                ICellStyle titleStyleIn,
                ConnectionPrintCallback cellPrintCallbackIn,
                ColumnSizeMethod sizeMethodIn,
                int sizeWidthIn,
                bool blankIsUnknownIn
            )
                : this( indexIn, titleIn, titleStyleIn, null, cellPrintCallbackIn, sizeMethodIn, sizeWidthIn, blankIsUnknownIn ) {

            }

            public ColumnDefinition(
                int indexIn,
                string titleIn,
                ICellStyle titleStyleIn,
                ConnectionStyleCallback cellStyleCallbackIn,
                ConnectionPrintCallback cellPrintCallbackIn,
                bool blankIsUnknownIn
            )
                : this( indexIn, titleIn, titleStyleIn, cellStyleCallbackIn, cellPrintCallbackIn, ColumnSizeMethod.Automatic, -1, blankIsUnknownIn ) {

            }

            public ColumnDefinition(
                int indexIn,
                string titleIn,
                ICellStyle titleStyleIn,
                ConnectionStyleCallback cellStyleCallbackIn,
                ConnectionPrintCallback cellPrintCallbackIn,
                ColumnSizeMethod sizeMethodIn,
                int sizeWidthIn,
                bool blankIsUnknownIn
            ) {
                this.Index = indexIn;
                this.Title = titleIn;
                this.TitleStyle = titleStyleIn;
                this.CellStyleCallback = cellStyleCallbackIn;
                this.CellPrintCallback = cellPrintCallbackIn;
                this.SizeMethod = sizeMethodIn;
                this.SizeWidth = sizeWidthIn;
                this.BlankIsUnknown = blankIsUnknownIn;
            }
        }

        public SimpleExcelSheet() {
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
            return this.cellStyles[indexIn];
        }

        public void SetCellStyle( string indexIn, XSSFCellStyle styleIn ) {
            this.cellStyles.Add( indexIn, styleIn );
        }

        public XSSFCellStyle CreateCellStyle() {
            return (XSSFCellStyle)this.workbook.CreateCellStyle();
        }

        public XSSFFont CreateFont() {
            return (XSSFFont)this.workbook.CreateFont();
        }

        public ISheet GetSheet( string titleIn ) {
            return this.workbook.GetSheet( titleIn );
        }

        public ISheet CreateSheet( string titleIn ) {
            return this.workbook.CreateSheet( titleIn );
        }

        public void Stream( Stream target ) {
            this.workbook.Write( target );
        }

        public static void AutoSizeColumn( ISheet sheet, int index ) {
            sheet.AutoSizeColumn( index );
            sheet.SetColumnWidth( index, sheet.GetColumnWidth( index ) + 1024 );
        }

        public static void PrintSheetHeader( ISheet sheet, ICellStyle style, string title ) {
            ICell titleCell = sheet.CreateRow( 0 ).CreateCell( 0 );
            titleCell.SetCellValue( title );
            titleCell.CellStyle = style;
            sheet.GetRow( 0 ).HeightInPoints = 40;
            sheet.AddMergedRegion( new CellRangeAddress( 0, 0, 0, 10 ) );
        }

        public static void PrintSheetTable( ISheet sheet, ColumnDefinition[] columnsIn, IEnumerable<object> reportObjectsIn ) {
            int highestIndex = 0;

            // Create headers.
            int rowIndex = 2;
            IRow row = sheet.CreateRow( rowIndex );

            foreach( ColumnDefinition column in columnsIn ) {
                row.CreateCell( column.Index ).SetCellValue( column.Title );
                row.GetCell( column.Index ).CellStyle = column.TitleStyle;

                // Figure out the highest index for later.
                if( column.Index > highestIndex ) {
                    highestIndex = column.Index;
                }
            }

            foreach( object rowIter in reportObjectsIn ) {

                // Create the row for this connection.
                rowIndex++;
                row = sheet.CreateRow( rowIndex );

                foreach( ColumnDefinition column in columnsIn ) {
                    string cellString = column.CellPrintCallback( rowIter ).Trim();
                    int cellNumber;
                    if( column.BlankIsUnknown && string.IsNullOrEmpty( cellString ) ) {
                        row.CreateCell( column.Index ).SetCellValue( "Unknown" );
                    } else if( int.TryParse( cellString, out cellNumber ) ) {
                        row.CreateCell( column.Index ).SetCellValue( cellNumber );
                    } else {
                        row.CreateCell( column.Index ).SetCellValue( cellString );
                    }
                    if( null != column.CellStyleCallback ) {
                        row.GetCell( column.Index ).CellStyle = column.CellStyleCallback( rowIter );
                    }
                }
            }

            // Size the columns.
            foreach( ColumnDefinition column in columnsIn ) {
                if( ColumnSizeMethod.Automatic == column.SizeMethod ) {
                    AutoSizeColumn( sheet, column.Index );
                } else if( 0 <= column.SizeWidth ) {
                    sheet.SetColumnWidth( column.Index, column.SizeWidth );
                }
            }

            // Turn on filtering/sorting.
            sheet.SetAutoFilter( new CellRangeAddress(
                2,
                row.RowNum,
                0,
                highestIndex
            ) );
        }
    }
}

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

        protected ISheet sheet;

        public SimpleExcelSheet( ISheet sheetIn ) {
            this.sheet = sheetIn;
        }
        
        /*
        public XSSFFont CreateFont() {
            return (XSSFFont)this.workbook.CreateFont();
        }
         */

        public static void AutoSizeColumn( ISheet sheet, int index ) {
            sheet.AutoSizeColumn( index );
            sheet.SetColumnWidth( index, sheet.GetColumnWidth( index ) + 1024 );
        }

        public void PrintSheetHeader( ICellStyle style, string title ) {
            this.PrintSheetHeader( 0, style, title );
        }

        public void PrintSheetHeader( int rowStartIndexIn, ICellStyle style, string title ) {
            ICell titleCell = this.sheet.CreateRow( rowStartIndexIn ).CreateCell( 0 );
            titleCell.SetCellValue( title );
            titleCell.CellStyle = style;
            this.sheet.GetRow( rowStartIndexIn ).HeightInPoints = 40;
            this.sheet.AddMergedRegion( new CellRangeAddress( rowStartIndexIn, rowStartIndexIn, 0, 10 ) );
        }

        public void PrintSheetTable( SimpleExcelColumn[] columnsIn, IEnumerable<object> reportObjectsIn ) {
            this.PrintSheetTable( 2, columnsIn, reportObjectsIn );
        }

        public void PrintSheetTable( int rowStartIndexIn, SimpleExcelColumn[] columnsIn, IEnumerable<object> reportObjectsIn ) {
            int highestIndex = 0;

            // Create headers.
            int rowIndex = rowStartIndexIn;
            IRow row = this.sheet.CreateRow( rowIndex );

            foreach( SimpleExcelColumn column in columnsIn ) {
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
                row = this.sheet.CreateRow( rowIndex );

                foreach( SimpleExcelColumn column in columnsIn ) {
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
            foreach( SimpleExcelColumn column in columnsIn ) {
                if( SimpleExcelColumnSize.Automatic == column.SizeMethod ) {
                    AutoSizeColumn( this.sheet, column.Index );
                } else if( 0 <= column.SizeWidth ) {
                    this.sheet.SetColumnWidth( column.Index, column.SizeWidth );
                }
            }

            // Turn on filtering/sorting.
            sheet.SetAutoFilter( new CellRangeAddress(
                rowStartIndexIn,
                row.RowNum,
                0,
                highestIndex
            ) );
        }
    }
}

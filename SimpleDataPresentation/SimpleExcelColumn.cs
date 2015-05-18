using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleUtils {
    public delegate string ConnectionPrintCallback( object rowIn );
    public delegate ICellStyle ConnectionStyleCallback( object rowIn );

    public enum SimpleExcelColumnSize {
        Automatic,
        Manual
    }

    public class SimpleExcelColumn {
        public int Index { get; set; }
        public string Title { get; set; }
        public ICellStyle TitleStyle { get; set; }
        public ConnectionStyleCallback CellStyleCallback { get; set; }
        public ConnectionPrintCallback CellPrintCallback { get; set; }
        public SimpleExcelColumnSize SizeMethod { get; set; }
        public int SizeWidth { get; set; }
        public bool BlankIsUnknown { get; set; }

        public SimpleExcelColumn(
            int indexIn,
            string titleIn,
            ICellStyle titleStyleIn,
            ConnectionPrintCallback cellPrintCallbackIn,
            bool blankIsUnknownIn
        ) : this( indexIn, titleIn, titleStyleIn, null, cellPrintCallbackIn, blankIsUnknownIn ) {

        }

        public SimpleExcelColumn(
            int indexIn,
            string titleIn,
            ICellStyle titleStyleIn,
            ConnectionPrintCallback cellPrintCallbackIn,
            SimpleExcelColumnSize sizeMethodIn,
            int sizeWidthIn,
            bool blankIsUnknownIn
        ) : this( indexIn, titleIn, titleStyleIn, null, cellPrintCallbackIn, sizeMethodIn, sizeWidthIn, blankIsUnknownIn ) {

        }

        public SimpleExcelColumn(
            int indexIn,
            string titleIn,
            ICellStyle titleStyleIn,
            ConnectionStyleCallback cellStyleCallbackIn,
            ConnectionPrintCallback cellPrintCallbackIn,
            bool blankIsUnknownIn
        ) : this( indexIn, titleIn, titleStyleIn, cellStyleCallbackIn, cellPrintCallbackIn, SimpleExcelColumnSize.Automatic, -1, blankIsUnknownIn ) {

        }

        public SimpleExcelColumn(
            int indexIn,
            string titleIn,
            ICellStyle titleStyleIn,
            ConnectionStyleCallback cellStyleCallbackIn,
            ConnectionPrintCallback cellPrintCallbackIn,
            SimpleExcelColumnSize sizeMethodIn,
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
}

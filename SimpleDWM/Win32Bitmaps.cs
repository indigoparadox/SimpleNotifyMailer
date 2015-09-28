using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDWM {
    public class Win32Bitmaps {
        public static Bitmap CreateRatioScaledBitmap( Bitmap bitmap, uint width, uint height ) {
            return CreateRatioScaledBitmap( bitmap, width, height, Brushes.Transparent );
        }

        public static Bitmap CreateRatioScaledBitmap( Bitmap bitmap, uint width, uint height, Brush backgroundColor ) {
            float scale = Math.Min( (float)width / bitmap.Width, (float)height / bitmap.Height );
            int scaleWidth = (int)(bitmap.Width * scale);
            int scaleHeight = (int)(bitmap.Height * scale);
            Bitmap temp = new Bitmap( (int)width, (int)height );
            using( Graphics g = Graphics.FromImage( temp ) ) {
                g.FillRectangle( backgroundColor, 0, 0, width, height );
                g.DrawImage(
                    bitmap,
                    new Rectangle(
                        (int)((width / 2) - (scaleWidth / 2)),
                        (int)((height / 2) - (scaleHeight / 2)),
                        scaleWidth,
                        scaleHeight
                    )
                );
            }
            return temp;
        }
    }
}

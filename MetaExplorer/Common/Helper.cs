using System.Diagnostics;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Common.Properties;

namespace MetaExplorer.Common
{
    public class Helper
    {
        private static BitmapImage _NAimage = null;

        /// <summary>
        /// Gibt einen MD5 Hash als String zurück
        /// </summary>
        /// <param name="TextToHash">string der Gehasht werden soll.</param>
        /// <returns>Hash als string.</returns>
        public static string GetMD5Hash(string TextToHash)
        {
            //Prüfen ob Daten übergeben wurden.
            if ((TextToHash == null) || (TextToHash.Length == 0))
            {
                return string.Empty;
            }

            //MD5 Hash aus dem String berechnen. Dazu muss der string in ein Byte[]
            //zerlegt werden. Danach muss das Resultat wieder zurück in ein string.
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] textToHash = Encoding.Default.GetBytes(TextToHash);
            byte[] result = md5.ComputeHash(textToHash);

            return System.BitConverter.ToString(result);
        }

        public static void Play(string playerLocation, string fileName)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = playerLocation;
            psi.Arguments = "\"" + fileName + "\"";

            Process p = new Process();
            p.StartInfo = psi;
            p.Start();
        }

        public static RenderTargetBitmap CrossBitmapImage(BitmapSource bi)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap(bi.PixelWidth, bi.PixelHeight, bi.DpiX, bi.DpiY, PixelFormats.Pbgra32);
            DrawingVisual drawingVisual = new DrawingVisual();

            // Retrieve the DrawingContext in order to create new drawing content.
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            // draw the image and text in the DrawingContext.
            Rect rect = new Rect(new System.Windows.Size(bi.Width, bi.Height));
            drawingContext.DrawImage(bi, rect);

            System.Windows.Media.Pen pen = new System.Windows.Media.Pen(System.Windows.Media.Brushes.Red, 5);
            drawingContext.DrawLine(pen, rect.TopLeft, rect.BottomRight);
            drawingContext.DrawLine(pen, rect.TopRight, rect.BottomLeft);
            
            // Persist the drawing content.
            drawingContext.Close();
            rtb.Render(drawingVisual);

            return rtb;
        }

        public static BitmapImage NAimage
        {
            get
            {
                if (_NAimage == null)
                {
                    Bitmap bitmap = Resources.na;
                    var memoryStream = new System.IO.MemoryStream();
                    bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                    _NAimage = new BitmapImage();
                    _NAimage.BeginInit();
                    _NAimage.StreamSource = new System.IO.MemoryStream(memoryStream.ToArray());
                    _NAimage.EndInit();
                }

                return _NAimage;
            }
        }
    }
}

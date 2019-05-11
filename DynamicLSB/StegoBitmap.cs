using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DynamicLSB
{
    class StegoBitmap
    {
        private BitmapImage sourceFile;
        public long FileSize { get; }
        public byte[] RedArray { get; }
        public byte[] GreenArray { get; }
        public byte[] BlueArray { get; }
        public byte[] AlphaArray { get; }

        public int Stride { get; }

        public StegoBitmap(string filePath) : this(new BitmapImage(new Uri(filePath))) { }
        public StegoBitmap(BitmapImage bitmapImage)
        {
            sourceFile = bitmapImage;
            FileSize = Convert.ToInt32(sourceFile.Width * sourceFile.Height * 4);
            Stride = Convert.ToInt32(sourceFile.Width * 4);
            int arraySize = Convert.ToInt32(Stride * sourceFile.Height);
            byte[] bitmapArray = new byte[arraySize]; //R-G-B-A byte array

            sourceFile.CopyPixels(bitmapArray, Stride, 0);
            RedArray = bitmapArray.Where((element, index) => index % 4 == 0).ToArray();
            GreenArray = bitmapArray.Where((element, index) => index % 4 == 1).ToArray();
            BlueArray = bitmapArray.Where((element, index) => index % 4 == 2).ToArray();
            AlphaArray = bitmapArray.Where((element, index) => index % 4 == 3).ToArray();
        }
        public StegoBitmap(StegoBitmap stegoBitmap, byte[] changedChannel, Channel channel)
        {
            RedArray = stegoBitmap.RedArray;
            GreenArray = stegoBitmap.GreenArray;
            BlueArray = stegoBitmap.BlueArray;
            AlphaArray = stegoBitmap.AlphaArray;

            if (channel == Channel.R) RedArray = changedChannel;
            if (channel == Channel.G) GreenArray = changedChannel;
            if (channel == Channel.B) BlueArray = changedChannel;

            int arraySize = Convert.ToInt32(stegoBitmap.Stride * stegoBitmap.sourceFile.Height);
            byte[] bitmapArray = new byte[arraySize];

            int counter = 0;
            for (int i = 0; i < RedArray.Length; i += 4)
            {
                bitmapArray[i] = RedArray[counter];
                bitmapArray[i + 1] = GreenArray[counter];
                bitmapArray[i + 2] = BlueArray[counter];
                bitmapArray[i + 3] = AlphaArray[counter];
                counter++;
            }
            BitmapImage sf = stegoBitmap.sourceFile;
            BitmapSource bs = BitmapSource.Create((int)sf.Width, (int)sf.Height, sf.DpiX, sf.DpiY, sf.Format, sf.Palette, bitmapArray, stegoBitmap.Stride);
            BitmapSourceToBitmapImage(bs);
        }

        private void BitmapSourceToBitmapImage(BitmapSource bs)
        {
            sourceFile = new BitmapImage();
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bs));
            using (MemoryStream memoryStream = new MemoryStream())
            {
                encoder.Save(memoryStream);
                memoryStream.Position = 0;
                sourceFile.BeginInit();
                sourceFile.StreamSource = new MemoryStream(memoryStream.ToArray());
                sourceFile.EndInit();
            }
        }

        internal BitmapImage GetImage()
        {
            return sourceFile;
        }

        internal int GetMaxCapacity()
        {
            return Convert.ToInt32(sourceFile.Width * sourceFile.Height / 8) - 2; //First byte for 'E' second byte for length
        }

        internal byte[] GetInBytes(Channel channel)
        {
            if (channel == Channel.R) return RedArray;
            if (channel == Channel.G) return GreenArray;
            if (channel == Channel.B) return BlueArray;
            else return null;
        }
        internal void SaveBitmap(string FilePath)
        {
            BitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(sourceFile));

            using (var fileStream = new System.IO.FileStream(FilePath, System.IO.FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }
    }
}

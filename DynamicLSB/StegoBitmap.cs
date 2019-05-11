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
        private long fileSize;
        private byte[] redArray;
        private byte[] greenArray;
        private byte[] blueArray;
        private BitmapImage bitmapImage;

        public StegoBitmap(string filePath)
        {
            sourceFile = new BitmapImage(new Uri(filePath));
            fileSize = new FileInfo(filePath).Length;
            redArray = sourceFile.
        }

        public StegoBitmap(BitmapImage bitmapImage)
        {
            this.bitmapImage = bitmapImage;
        }

        internal BitmapImage GetImage()
        {
            return sourceFile;
        }

        internal int GetMaxCapacity()
        {
            return Convert.ToInt32(sourceFile.Width * sourceFile.Height - 3); //First byte for 'E' second byte for length
        }

        internal long GetFileSize()
        {
            return fileSize;
        }

        internal byte[] GetRinBytes()
        {
            return redArray;
        }

        internal byte[] GetGinBytes()
        {
            return greenArray;
        }

        internal byte[] GetBinBytes()
        {
            return blueArray;
        }
        internal byte[] GetInBytes(Channel channel)
        {
            if (channel == Channel.R) return GetRinBytes();
            if (channel == Channel.G) return GetGinBytes();
            if (channel == Channel.B) return GetBinBytes();
            else return null;
        }
        internal void SaveBitmap(string FilePath)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(sourceFile));

            using (var fileStream = new System.IO.FileStream(FilePath, System.IO.FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using System.Drawing;

namespace DynamicLSB
{
    class StegoBitmap
    {
        readonly private Bitmap sourceFile;
        public long FileSize { get; }
        public byte[] RedChannel { get; }
        public byte[] GreenChannel { get; }
        public byte[] BlueChannel { get; }

        public StegoBitmap(string filePath) : this(new Bitmap(filePath)) { }
        public StegoBitmap(Bitmap bitmap)
        {
            sourceFile = bitmap;
            FileSize = Convert.ToInt32(sourceFile.Width * sourceFile.Height * 4);

            RedChannel = ReadChannel(bitmap, Channel.R);
            GreenChannel = ReadChannel(bitmap, Channel.G);
            BlueChannel = ReadChannel(bitmap, Channel.B);
        }

        public StegoBitmap(StegoBitmap stegoBitmap, byte[] changedChannel, Channel channel)
        {
            RedChannel = stegoBitmap.RedChannel;
            GreenChannel = stegoBitmap.GreenChannel;
            BlueChannel = stegoBitmap.BlueChannel;

            if (channel == Channel.R) RedChannel = changedChannel;
            if (channel == Channel.G) GreenChannel = changedChannel;
            if (channel == Channel.B) BlueChannel = changedChannel;

            sourceFile = new Bitmap(stegoBitmap.sourceFile.Width, stegoBitmap.sourceFile.Height);

            int counter = 0;
            for (int i = 0; i < sourceFile.Width; i++)
                for (int j = 0; j < sourceFile.Height; j++)
                {
                    sourceFile.SetPixel(i, j, System.Drawing.Color.FromArgb(RedChannel[counter], GreenChannel[counter], BlueChannel[counter]));
                    counter++;
                }
        }

        internal Bitmap GetImage()
        {
            return sourceFile;
        }

        internal int GetMaxCapacity()
        {
            return Convert.ToInt32(sourceFile.Width * sourceFile.Height / 8);
        }

        internal byte[] GetChannel(Channel channel)
        {
            if (channel == Channel.R) return RedChannel;
            if (channel == Channel.G) return GreenChannel;
            if (channel == Channel.B) return BlueChannel;
            else return null;
        }
        private byte[] ReadChannel(Bitmap bitmap, Channel channel)
        {
            byte[] array = new byte[bitmap.Width * bitmap.Height];
            int counter = 0;
            for (int i = 0; i < bitmap.Width; i++)
                for (int j = 0; j < bitmap.Height; j++)
                    if (channel == Channel.R)
                        array[counter++] = bitmap.GetPixel(i, j).R;
                    else if (channel == Channel.G)
                        array[counter++] = bitmap.GetPixel(i, j).G;
                    else if (channel == Channel.B)
                        array[counter++] = bitmap.GetPixel(i, j).B;
            return array;
        }

        internal void SaveBitmap(string FilePath)
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
            sourceFile.Save(FilePath);
        }
    }
}

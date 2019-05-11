using System;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace DynamicLSB
{
    enum Channel { R, G, B }
    internal class Stego
    {
        internal static byte[] StringToByteArray(string Text)
        {
            return Encoding.ASCII.GetBytes(Text);
        }

        internal static int CalculateChange(byte[] First, byte[] Second)
        {
            return (from x in First
                    from y in Second
                    select x - y).Sum();
        }

        internal static BitmapImage Hide(StegoBitmap bitmapImage, string text, Channel channel)
        {
            byte[] modifiedChannel = bitmapImage.GetInBytes(channel);

        }
        internal static string GetHiddenValue(StegoBitmap bitmapImage,Channel channel)
        {
            byte[] hidden = bitmapImage.GetInBytes(channel);

        }

        internal static bool HasHiddenValue(StegoBitmap bitmapImage)
        {
            if (HasE(bitmapImage.GetRinBytes()) || HasE(bitmapImage.GetRinBytes()) || HasE(bitmapImage.GetRinBytes())) return true;
            else return false;
        }

        private static bool HasE(byte[] v)
        {
            return BoolArrayToByte(v.Take(8).SelectMany(x => GetNLastBit(x, 1)).ToArray()) == Convert.ToByte('E');
        }

        private static bool[] GetNLastBit(byte x, int count)
        {
            return ByteToBoolArray(x).Skip(8 - count).Take(count).ToArray();
        }
        private static byte BoolArrayToByte(bool[] array)
        {
            byte result = 0;
            int index = 8 - array.Length;
            foreach (bool b in array)
            {
                if (b)
                    result |= (byte)(1 << (7 - index));
                index++;
            }
            return result;
        }
        private static bool[] ByteToBoolArray(byte b)
        {
            bool[] result = new bool[8];
            for (int i = 0; i < 8; i++)
                result[i] = (b & (1 << i)) == 0 ? false : true;
            Array.Reverse(result);
            return result;
        }
    }
}
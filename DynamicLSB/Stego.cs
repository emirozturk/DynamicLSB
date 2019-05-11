using System;
using System.Collections.Generic;
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
        internal static string ByteArrayToString(byte[] Array)
        {
            return Encoding.ASCII.GetString(Array);
        }
        internal static int CalculateChange(byte[] First, byte[] Second)
        {
            return (from x in First
                    from y in Second
                    select x - y).Sum();
        }

        internal static BitmapImage Hide(StegoBitmap bitmapImage, string text, Channel channel)
        {
            byte[] sourceChannel = bitmapImage.GetInBytes(channel);
            bool[] stegoBits = StringToByteArray(text).SelectMany(x => ByteToBoolArray(x)).ToArray();
            for (int i = 0; i < stegoBits.Length; i++)
                if (sourceChannel[i] % 2 == 0 && stegoBits[i])
                    sourceChannel[i]++;
                else if (sourceChannel[i] % 2 == 1 && !stegoBits[i])
                    sourceChannel[i]--;
            return new StegoBitmap(bitmapImage, sourceChannel, channel).GetImage();
        }
        internal static string GetHiddenValue(StegoBitmap bitmapImage, Channel channel)
        {
            byte[] hidden = bitmapImage.GetInBytes(channel);
            bool[] bits = hidden.SelectMany(x => GetNLastBit(x, 1)).ToArray();
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < bits.Length; i += 8)
                bytes.Add(BoolArrayToByte(bits.Skip(i).Take(8).ToArray()));
            return ByteArrayToString(bytes.Skip(2).Take(bytes[2]).ToArray());
        }

        internal static Channel? HasHiddenValue(StegoBitmap bitmapImage)
        {
            if (HasE(bitmapImage.RedArray)) return Channel.R;
            else if (HasE(bitmapImage.GreenArray)) return Channel.G;
            else if (HasE(bitmapImage.BlueArray)) return Channel.B;
            else return null;
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
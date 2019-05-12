using System;
using System.Collections.Generic;
using System.Drawing;
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
            bool[] secondBits = Second.SelectMany(x => ByteToBoolArray(x)).ToArray();
            bool[] firstBits = First.Take(secondBits.Length).SelectMany(x => GetNLastBit(x, 1)).ToArray();
            int count = 0;
            for (int i = 0; i < secondBits.Length; i++)
                if (firstBits[i] != secondBits[i])
                    count++;
            return count;
        }

        internal static Bitmap Hide(StegoBitmap bitmap, string text, Channel channel)
        {
            byte[] sourceChannel = bitmap.GetChannel(channel);
            byte flag = Convert.ToByte('E');
            byte size = GetSizeOfLength(text.Length);
            byte[] length = GetLengthInBytes(text.Length, size);
            byte[] stegoText = StringToByteArray(text);

            byte[] stegoBytes = new byte[1 + 1 + length.Length + stegoText.Length];
            stegoBytes[0] = flag;
            stegoBytes[1] = size;
            int counter = 0;
            for (int i = 0; i < length.Length; i++)stegoBytes[i+2] = length[counter++];
            counter = 0;
            for (int i = 0; i <stegoText.Length; i++)stegoBytes[i+2+length.Length] = stegoText[counter++];

            bool[] stegoBits = stegoBytes.SelectMany(x => ByteToBoolArray(x)).ToArray();

            for (int i = 0; i < stegoBits.Length; i++)
                if ((sourceChannel[i] % 2 == 0) && stegoBits[i])
                    sourceChannel[i]++;
                else if ((sourceChannel[i] % 2 == 1) && !stegoBits[i])
                    sourceChannel[i]--;
            return new StegoBitmap(bitmap, sourceChannel, channel).GetImage();
        }

        private static byte[] GetLengthInBytes(int Length, int SizeOfLength)
        {
            if (SizeOfLength == 1)
                return new byte[1] { (byte)Length };
            else if (SizeOfLength == 2)
                return BitConverter.GetBytes((short)Length);
            else
                return BitConverter.GetBytes(Length);
        }

        private static byte GetSizeOfLength(int length)
        {
            if (length > 0 && length < 256) return 1;
            else if (length < 65536) return 2;
            else return 4;
        }

        internal static string GetHiddenValue(StegoBitmap bitmap, Channel channel)
        {
            byte[] hidden = bitmap.GetChannel(channel);

            bool[] headerBits = hidden.Take(16).SelectMany(x => GetNLastBit(x, 1)).ToArray();
            byte E = BoolArrayToByte(headerBits.Take(8).ToArray());
            byte lengthSize = BoolArrayToByte(headerBits.Skip(8).Take(8).ToArray());
            bool[] lengthBits = hidden.Skip(16).Take(lengthSize * 8).SelectMany(x => GetNLastBit(x, 1)).ToArray();
            List<byte> lengthBytes = new List<byte>();
            for (int i = 0; i < lengthBits.Length; i += 8)
                lengthBytes.Add(BoolArrayToByte(lengthBits.Skip(i).Take(8).ToArray()));
            int length = GetIntFromBytes(lengthBytes);

            bool[] bits = hidden.Skip(16+lengthSize*8).Take(length*8).SelectMany(x => GetNLastBit(x, 1)).ToArray();
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < bits.Length; i += 8)
                bytes.Add(BoolArrayToByte(bits.Skip(i).Take(8).ToArray()));
            return ByteArrayToString(bytes.ToArray());
        }

        private static int GetIntFromBytes(List<byte> lengthBytes)
        {
            while (lengthBytes.Count < 8) lengthBytes.Add(0);
            return BitConverter.ToInt32(lengthBytes.ToArray(), 0);
        }

        internal static Channel? HasHiddenValue(StegoBitmap bitmapImage)
        {
            if (HasE(bitmapImage.RedChannel)) return Channel.R;
            else if (HasE(bitmapImage.GreenChannel)) return Channel.G;
            else if (HasE(bitmapImage.BlueChannel)) return Channel.B;
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
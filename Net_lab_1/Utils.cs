using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Net_lab_1
{
    public static class Utils
    {
        public static int CheckSum(BitArray array)
        {
            int k = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i])
                {
                    k += 1;
                }
            }

            return k;
        }

        public static BitArray ToBinary(this int number)
        {
            BitArray array = new BitArray(new[] {BitConverter.GetBytes(number)[0]});
            return array;
        }

        public static BitArray ToChecksum(this BitArray array)
        {
            var result = new BitArray(Frame.ChecksumSize);

            int sum = 0;
            for (var i = 0; i < array.Count; i++)
            {
                sum += array[i] ? 1 : 0;
            }

            result.Set(0, sum % 2 > 0);

            return result;
        }

        public static bool CompareChecksums(BitArray left, BitArray right)
        {
            // Because we set only the very first bit of checksum
            return left[0] == right[0];
        }

        public static BitArray Subsequence(this BitArray array, int offset, int length)
        {
            BitArray result = new BitArray(length);

            for (int i = 0; i < length; i++)
            {
                result[i] = array[offset + i];
            }

            return result;
        }

        public static void Write(this BitArray array, int offset, BitArray data)
        {
            for (var i = 0; i < data.Count; i++)
            {
                array[offset + i] = data[i];
            }
        }

        public static byte[] ToByteArray(this BitArray array)
        {
            var bytes = new byte[array.Count / 8];
            array.CopyTo(bytes, 0);
            return bytes;
        }

        public static List<BitArray> Split(this byte[] data, int size)
        {
            List<BitArray> list = new();

            int offset = 0;
            while (offset + size / 8 < data.Length)
            {
                list.Add(new(data.Skip(offset).Take(size / 8).ToArray()));
                offset += size / 8;
            }

            if (offset < data.Length)
            {
                list.Add(new(data.Skip(offset).Take(data.Length - offset).ToArray()));
            }

            return list;
        }
    }
}
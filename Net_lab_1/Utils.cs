using System;
using System.Collections;

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
    }
}
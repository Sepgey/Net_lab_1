using System.Collections;

namespace Net_lab_1
{
    public class Frame
    {
        public const int ControlSize = 16;
        public const int ChecksumSize = 8;

        public const int MaxDataSizeBits = 64;

        public BitArray ControlBits { get; set; }

        public BitArray Data { get; set; }

        public BitArray Checksum { get; set; }

        public Frame(BitArray controlBits, BitArray data)
        {
            ControlBits = controlBits;
            Data = data;
        }

        public BitArray Build()
        {
            Checksum = Data.ToChecksum();

            int frameSize =
                ControlSize +
                Data.Length +
                ChecksumSize;

            BitArray frameArray = new BitArray(frameSize);

            frameArray.Write(0, ControlBits);
            frameArray.Write(ControlBits.Count, Data);
            frameArray.Write(ControlBits.Count + Data.Count, Checksum);

            return frameArray;
        }

        public static Frame Parse(BitArray array)
        {
            Frame frame = new Frame(
                array.Subsequence(0, ControlSize),
                array.Subsequence(ControlSize, array.Length - ControlSize - ChecksumSize)
            )
            {
                Checksum = array.Subsequence(array.Length - ChecksumSize, ChecksumSize)
            };
            return frame;
        }
    }
}
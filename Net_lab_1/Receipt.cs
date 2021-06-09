using System.Collections;

namespace Net_lab_1
{
    public class Receipt
    {
        private static BitArray _receipt;

        public static BitArray CreateReciept()
        {
            _receipt = new BitArray(1);
            _receipt[0] = true;
            return _receipt;
        }
    }
}
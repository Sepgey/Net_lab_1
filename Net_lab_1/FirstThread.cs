using System.Collections;
using System.Threading;

namespace Net_lab_1
{
    public class FirstThread
    {
        private readonly Semaphore _sendSemaphore;
        private readonly Semaphore _receiveSemaphore;
        private BitArray[] _receivedMessages;
        private PostToSecondWT _post;

        public FirstThread(Semaphore sendSemaphore, Semaphore receiveSemaphore)
        {
            _sendSemaphore = sendSemaphore;
            _receiveSemaphore = receiveSemaphore;
        }

        public void FirstThreadMain(object obj)
        {
            _post = (PostToSecondWT) obj;
            ConsoleHelper.WriteToConsole("1 поток", "Начинаю работу. Готовлю данные для передачи.");

            // 100 - connect
            // 101 - RR (Receiver Ready)
            // 102 - RNR (Receiver Not Ready)
            // 103 - REJ (Reject)
            // 104 - SREJ (Selective Reject)
            // 50 - Data Frame

            while (!SendConnection())
            {
                ConsoleHelper.WriteToConsole(Thread.CurrentThread.Name, "Не удалось подключиться.");
            }
            
            ConsoleHelper.WriteToConsole(Thread.CurrentThread.Name, "НПодключение установлено, начинаю передачу..");
            
            // TODO: SEND DATA

            // TODO: SEND END

            ConsoleHelper.WriteToConsole("1 поток", "Завершаю работу.");
        }

        private bool SendConnection()
        {
            Frame frame = new Frame(new BitArray(new byte[] {0, 100}), new BitArray(0));

            var bitArray = frame.Build();

            _post(new[] {bitArray});
            _sendSemaphore.Release();

            _receiveSemaphore.WaitOne();

            var responseFrame = Frame.Parse(_receivedMessages[0]);

            byte[] responseControl = new byte[2];
            responseFrame.ControlBits.CopyTo(responseControl, 0);
            if (responseControl[1] == 101)
            {
                // Receiver Ready
                return true;
            }
            else if(responseControl[1] == 102)
            {
                ConsoleHelper.WriteToConsole(Thread.CurrentThread.Name, "Получен RNR, Ожидаю!");
                Thread.Sleep(500);
                return true;
            }

            return false;
        }


        public void ReceiveData(BitArray[] array)
        {
            _receivedMessages = array;
        }
    }
}
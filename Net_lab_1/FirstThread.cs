using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
            _post = (PostToSecondWT)obj;
            ConsoleHelper.WriteToConsole("1 поток", "Начинаю работу. Готовлю данные для передачи.");

            // 100 - connect
            // 101 - RR (Receiver Ready)
            // 102 - RNR (Receiver Not Ready)
            // 103 - REJ (Reject)
            // 104 - SREJ (Selective Reject)
            // 50 - Data Frame
            // 150 - End
            // 151 - End Confirm

            while (!SendConnection())
            {
                ConsoleHelper.WriteToConsole(Thread.CurrentThread.Name, "Не удалось подключиться.");
            }

            ConsoleHelper.WriteToConsole(Thread.CurrentThread.Name, "Подключение установлено, начинаю передачу.");

            // byte[] data = Encoding.UTF8.GetBytes("Александр Городников Жрёт Говно Пачками");
            byte[] data = Encoding.UTF8.GetBytes("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.");
            // byte[] data = File.ReadAllBytes(@"C:\Users\Admin\Downloads\egop awesome file.txt");
            
            List<BitArray> bitArrays = data.Split(Frame.MaxDataSizeBits);

            for (var i = 0; i < bitArrays.Count; i++)
            {
                if (!SendDataFrame(i, bitArrays))
                {
                    ConsoleHelper.WriteToConsole("1 поток", "Получен неверный ответ. Отмена!");
                    break;
                }
            }

            while (!SendEnd())
            {
                ConsoleHelper.WriteToConsole(Thread.CurrentThread.Name, "Ошибка отключения.");
            }

            ConsoleHelper.WriteToConsole("1 поток", "Завершаю работу.");
        }

        private bool SendDataFrame(int i, List<BitArray> bitArrays)
        {
            Frame frame = new Frame(new BitArray(new byte[] {(byte)(i % 2), 50}), bitArrays[i]);

            _post(new[] {frame.Build()});
            _sendSemaphore.Release();

            int tries = 0;
            while (!_receiveSemaphore.WaitOne(30) && tries < 3)
            {
                ConsoleHelper.WriteToConsole("1 поток", $"Не получено подтверждение о получении {++tries}");
            }

            if (tries == 3)
            {
                return false;
            }

            Frame responseFrame = Frame.Parse(_receivedMessages[0]);

            byte[] responseControl = new byte[2];
            responseFrame.ControlBits.CopyTo(responseControl, 0);
            if (responseControl[1] == 101)
            {
                // Receiver Ready
                ConsoleHelper.WriteToConsole("1 поток", "Кадр передан успешно");
                return true;
            }
            else if (responseControl[1] == 102)
            {
                ConsoleHelper.WriteToConsole("1 поток", "Кадр передан успешно, получен RNR. Ожидаю.");
                Thread.Sleep(150);
                return true;
            }
            else if (responseControl[1] == 103)
            {
                ConsoleHelper.WriteToConsole("1 поток", "Получен REJ.");

                return SendDataFrame(i, bitArrays);
            }

            return false;
        }

        private bool SendConnection()
        {
            Frame frame = new Frame(new BitArray(new byte[] {0, 100}), new BitArray(0));

            BitArray bitArray = frame.Build();

            _post(new[] {bitArray});
            _sendSemaphore.Release();

            _receiveSemaphore.WaitOne();

            Frame responseFrame = Frame.Parse(_receivedMessages[0]);

            byte[] responseControl = new byte[2];
            responseFrame.ControlBits.CopyTo(responseControl, 0);
            if (responseControl[1] == 101)
            {
                // Receiver Ready
                return true;
            }
            else if (responseControl[1] == 102)
            {
                ConsoleHelper.WriteToConsole(Thread.CurrentThread.Name, "Получен RNR, Ожидаю!");
                Thread.Sleep(500);
                return true;
            }

            return false;
        }
        
        private bool SendEnd()
        {
            Frame frame = new Frame(new BitArray(new byte[] {0, 150}), new BitArray(0));

            BitArray bitArray = frame.Build();

            _post(new[] {bitArray});
            _sendSemaphore.Release();

            _receiveSemaphore.WaitOne();

            Frame responseFrame = Frame.Parse(_receivedMessages[0]);

            byte[] responseControl = new byte[2];
            responseFrame.ControlBits.CopyTo(responseControl, 0);
            if (responseControl[1] == 151)
            {
                // Receiver Accepted Disconnection
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
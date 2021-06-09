using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Net_lab_1
{
    public class SecondThread
    {
        private readonly Semaphore _sendSemaphore;
        private readonly Semaphore _receiveSemaphore;
        private BitArray[] _receivedMessages;
        private PostToFirstWT _post;

        public SecondThread(Semaphore sendSemaphore, Semaphore receiveSemaphore)
        {
            _sendSemaphore = sendSemaphore;
            _receiveSemaphore = receiveSemaphore;
        }

        public void SecondThreadMain(object obj)
        {
            _post = (PostToFirstWT)obj;
            ConsoleHelper.WriteToConsole("2 поток", "Начинаю работу. Жду передачи данных.");

            Random random = new(DateTime.Now.Millisecond);

            ReceiveConnection();

            List<byte[]> data = new();

            while (true)
            {
                _receiveSemaphore.WaitOne();

                var receivedFrame = Frame.Parse(_receivedMessages[0]);

                byte[] receivedControl = new byte[2];
                receivedFrame.ControlBits.CopyTo(receivedControl, 0);
                if (receivedControl[1] == 50)
                {
                    if (random.Next(0, 1000) > 800)
                    {
                        // С вероятностью 20% инвертируем рандомный бит в данных (шумы)
                        int index = random.Next(0, receivedFrame.Data.Count);
                        receivedFrame.Data[index] = !receivedFrame.Data[index];
                    }

                    // Data Frame
                    ConsoleHelper.WriteToConsole("2 поток", "Получены данные");

                    var dataChecksum = receivedFrame.Data.ToChecksum();
                    var sentChecksum = receivedFrame.Checksum;

                    if (!Utils.CompareChecksums(dataChecksum, sentChecksum))
                    {
                        ConsoleHelper.WriteToConsole("2 поток", "Контрольная сумма не совпала");
                        _post(new[] {new Frame(new BitArray(new byte[] {0, 103}), new BitArray(0)).Build()});
                    }
                    else
                    {
                        data.Add(receivedFrame.Data.ToByteArray());

                        _post(new[] {new Frame(new BitArray(new byte[] {0, 101}), new BitArray(0)).Build()});
                    }

                    Thread.Sleep(random.Next(20, 35));
                    _sendSemaphore.Release();
                }
                else if (receivedControl[1] == 150)
                {
                    // End Frame
                    ConsoleHelper.WriteToConsole("2 поток", "Получен запрос на отключение");
                    _post(new[] {new Frame(new BitArray(new byte[] {0, 151}), new BitArray(0)).Build()});
                    _sendSemaphore.Release();
                    break;
                }
            }

            var receivedBytes = data.SelectMany(t => t).ToArray();
            
            File.WriteAllBytes("file.txt", receivedBytes);

            Process.Start("notepad.exe", "file.txt");
            
            // ConsoleHelper.WriteToConsole("2 поток", "Получены данные: " + Encoding.UTF8.GetString(receivedBytes));

            ConsoleHelper.WriteToConsole("2 поток", "Заканчиваю работу");
        }

        private void ReceiveConnection()
        {
            _receiveSemaphore.WaitOne();

            var receivedFrame = Frame.Parse(_receivedMessages[0]);

            byte[] receivedControl = new byte[2];
            receivedFrame.ControlBits.CopyTo(receivedControl, 0);
            if (receivedControl[1] == 100)
            {
                // Receiver Ready
                ConsoleHelper.WriteToConsole("2 поток", "Получен запрос на подключение.");

                _post(new[] {new Frame(new BitArray(new byte[] {0, 101}), new BitArray(0)).Build()});
                _sendSemaphore.Release();
            }
            else
            {
                ConsoleHelper.WriteToConsole("2 поток", "Получен невалидный запрос на подключение");
            }
        }

        public void ReceiveData(BitArray[] messages)
        {
            _receivedMessages = messages;
        }
    }
}
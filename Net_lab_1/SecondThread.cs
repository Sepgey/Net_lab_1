using System;
using System.Collections;
using System.Threading;

namespace Net_lab_1
{
    public class SecondThread
    {
        private Semaphore _sendSemaphore;
        private Semaphore _receiveSemaphore;
        private BitArray[] _receivedMessage;
        private PostToFirstWT _post;

        public SecondThread(Semaphore sendSemaphore, Semaphore receiveSemaphore)
        {
            _sendSemaphore = sendSemaphore;
            _receiveSemaphore = receiveSemaphore;
        }

        public void SecondThreadMain(Object obj)
        {
            _post = (PostToFirstWT) obj;
            ConsoleHelper.WriteToConsole("2 поток", "Начинаю работу. Жду передачи данных.");

            // TODO: RECEIVE CONNECTION
            
            // TODO: RECEIVE DATA
            
            // TODO: (While in receive) we received an end,
            // TODO: Process end

            ConsoleHelper.WriteToConsole("2 поток", "Заканчиваю работу");
        }

        public void ReceiveData(BitArray[] messages)
        {
            _receivedMessage = messages;
        }
    }
}
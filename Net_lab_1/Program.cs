using System;
using System.Collections;
using System.Threading;

namespace Net_lab_1
{
    public delegate void PostToFirstWT(BitArray[] messages);

    public delegate void PostToSecondWT(BitArray[] messages);

    public static class Program
    {
        static void Main(string[] args)
        {
            ConsoleHelper.WriteToConsole("Главный поток", "");
            
            Semaphore firstReceiveSemaphore = new Semaphore(0, 1);
            Semaphore secondReceiveSemaphore = new Semaphore(0, 1);
            
            FirstThread firstThread = new FirstThread(secondReceiveSemaphore, firstReceiveSemaphore);
            SecondThread secondThread = new SecondThread(firstReceiveSemaphore, secondReceiveSemaphore);
            
            Thread threadFirst = new Thread(new ParameterizedThreadStart(firstThread.FirstThreadMain));
            Thread threadSecond = new Thread(new ParameterizedThreadStart(secondThread.SecondThreadMain));
            
            threadFirst.Name = "1 поток";
            threadSecond.Name = "2 поток";

            PostToFirstWT postToFirstWt = new PostToFirstWT(firstThread.ReceiveData);
            PostToSecondWT postToSecondWt = new PostToSecondWT(secondThread.ReceiveData);
            
            threadFirst.Start(postToSecondWt);
            threadSecond.Start(postToFirstWt);
            
            Console.ReadLine();
        }
    }
}
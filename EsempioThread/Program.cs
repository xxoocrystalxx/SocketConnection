using System.Net.Sockets;

namespace EsempioThread
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var array = new int[] { 1, 2, 3, 4, 5, 6, 7, };
            Thread t1 = new Thread(a=> Worker((int[])a));
            t1.Start(array);
            Thread t2 = new Thread(a => Worker2((int[])a));
            t2.Start(array);

            t1.Join();
            t2.Join();

            Console.WriteLine("main: "+array[0]);

        }

        private static void Worker2(int[] array)
        {
            array[0] = 3;
            Console.WriteLine("thread 2: "+ array[0]);
        }

        private static void Worker(int[] array)
        {
            array[0] = 2;
            Console.WriteLine("thread 1: "+array[0]);
        }
    }
}
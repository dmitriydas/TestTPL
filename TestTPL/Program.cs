using System;

namespace TestTPL
{
    class Program
    {
        static void Main(string[] args)
        {
            ServicePipeline sp = new ServicePipeline(10);
            sp.Start();
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }
    }
}

using System;

namespace NxtConsole
{
    class Program
    {
        static void Main()
        {
            var nxt = new Nxt.NET.Nxt();
            nxt.Start();
            Console.WriteLine("Press any key to quit.");
            Console.ReadLine();
        }
    }
}

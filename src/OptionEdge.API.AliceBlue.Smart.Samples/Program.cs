using System;

namespace OptionEdge.API.AliceBlue.Smart.Samples // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DevTest devTest = new DevTest();

            devTest.Run();  

            Console.WriteLine("Press any key to continue...");
            Console.ReadLine();
        }
    }
}
using System;
using System.IO;
using Ionic.Zlib;

namespace FFZlibScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("==== FastFile Utility ====");
            Console.WriteLine("1. Extract FastFile (.ff)");
            Console.WriteLine("2. [Reserved]");
            Console.WriteLine("0. Exit");
            Console.Write("Select an option: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.Write("Enter path to FF file: ");
                    // Trim quotes if dragged/dropped
                    string path = Console.ReadLine()?.Trim().Trim('"');
                    FastFileExtractor.Extract(path);
                    break;
                case "0":
                    Console.WriteLine("Exiting...");
                    return;
                default:
                    Console.WriteLine("Invalid selection.");
                    break;
            }

            Console.WriteLine("Press any key to return to menu...");
            Console.ReadKey();
            Console.Clear();
            Main(args);
        }
    }
}

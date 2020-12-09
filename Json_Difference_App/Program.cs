using System;

namespace Json_Difference_App
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Json_Difference_App!");
            Comparer comparer = new Comparer();
            comparer.Compare();
            Console.ReadKey();
        }
    }
}

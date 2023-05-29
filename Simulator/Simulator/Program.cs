using System;

namespace Simulator
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            int rows = Int32.Parse(args[0]);
            int cols = Int32.Parse(args[1]);
            int nThreads = Int32.Parse(args[2]);
            int nOperations = Int32.Parse(args[3]);
            int msSleep = Int32.Parse(args[4]);
            Simulator sim = new Simulator(rows, cols, nThreads, nOperations, msSleep);
        }
    }
}
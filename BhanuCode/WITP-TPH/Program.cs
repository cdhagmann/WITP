using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WITP_TPH
{
    class Program
    {
        static void Main(string[] args)
        {
            TPH tph = new TPH();
            tph.ReadInputData();
            tph.OptimizeFn();
            Console.Read();
        }
    }
}

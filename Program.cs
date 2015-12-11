using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Lib;
using System.Threading;

namespace TwoWayStreamSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var s = new MemoryStream();
            {
                var bs = new StreamWriter(s);
                bs.WriteLine("Hello!");
                bs.WriteLine("This is a test program for ReadAheadStream.");
                for (long i = 1; i <= 60; i++)
                {
                    bs.WriteLine("あいうえお");
                    bs.WriteLine($"{i}*{i} = {i * i}");
                }
                bs.WriteLine("end of stream. good luck.");
                bs.Flush();
            }

            s.Seek(0, SeekOrigin.Begin);

            var s2 = ReadAheadStream.Start(s);
            using (var reader = new StreamReader(s2))
            {
                while (true)
                {
                    char[] buff = new char[1];
                    var ret = reader.ReadBlock(buff, 0, buff.Length);
                    if (ret <= 0) break;
                    Console.Write(buff);
                    Thread.Sleep(5);
                }
            }

            Console.WriteLine("Finished. Please press enter key.");
            Console.ReadLine();
        }
    }
}

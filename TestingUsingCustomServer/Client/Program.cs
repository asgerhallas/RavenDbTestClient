using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication2
{
	class Program
	{
		static void Main(string[] args)
		{
			var buffer = new byte[4*1024];
			var url = args[0];
			for (int i = 0; i < 100; i++)
			{
				var sp = Stopwatch.StartNew();
				var webRequest = WebRequest.Create(url);
				using(var response = webRequest.GetResponse())
				using (var stream = response.GetResponseStream())
				{
					while (stream.Read(buffer, 0, buffer.Length) > 0)
					{
						
					}
				}

				Console.WriteLine("{0,-3}: {1}", i, sp.ElapsedMilliseconds);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
	class Program
	{
		static void Main()
		{
			var listener = new HttpListener
			{
				Prefixes = { "http://+:8080/" }
			};
			listener.Start();

			var buffer = new byte[512 * 1024];
			for (int i = 0; i < buffer.Length; i++)
			{
				buffer[i] = (byte)'a';
			}
			while (true)
			{
				try
				{
					var ctx = listener.GetContext();
					var sp = Stopwatch.StartNew();
					switch (ctx.Request.HttpMethod)
					{
						case "GET":
							break;
						default: // PUT/ POST
							// this just consume all sent data to memory
							using (var buffered = new BufferedStream(ctx.Request.InputStream))
							{
								while (buffered.ReadByte() != -1)
								{

								}
							}
							break;
					}

					
					// one write call
					//ctx.Response.OutputStream.Write(buffer, 0, buffer.Length);

					// 16 write calls
					int size = buffer.Length/16;
					var wrote = 0;
					for (int i = 0; i < size; i++)
					{
						ctx.Response.OutputStream.Write(buffer, wrote, size);
						wrote += size;
					}



					ctx.Response.Close();
					Console.WriteLine(DateTime.Now + ": " + sp.ElapsedMilliseconds);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			}


		}
	}
}

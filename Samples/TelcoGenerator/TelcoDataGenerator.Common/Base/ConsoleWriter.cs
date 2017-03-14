using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelcoDataGenerator.Common.IBase;

namespace TelcoDataGenerator.Common.Base
{
	public class ConsoleWriter : IWriter
	{
		public void Write(string toWrite)
		{
			Console.WriteLine(toWrite);
		}
	}
}

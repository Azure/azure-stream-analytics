using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcoDataGenerator.Common.IBase
{
	public interface IDisplay5
	{
		DateTime Created { get; set; }
		string Value1 { get; set; }
		string Value2 { get; set; }
		string Value3 { get; set; }
		string Value4 { get; set; }
		string Value5 { get; set; }
	}
}

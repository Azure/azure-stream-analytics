using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelcoDataGenerator.Common.IBase;

namespace TelcoDataGenerator.Common.Base
{
	public class Display5 : IBase.IDisplay5
	{
		public DateTime Created { get; set; } = DateTime.UtcNow;

		public string Value1 { get; set; } = "";

		public string Value2 { get; set; } = "";

		public string Value3 { get; set; } = "";

		public string Value4 { get; set; } = "";

		public string Value5 { get; set; } = "";

		public override string ToString()
		{
			return string.Join(" - ", new string[] { Value1, Value2, Value3, Value4, Value5 });
		}

		public static void SendMessage(IDisplay5 displayData)
		{
			GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(displayData);
		}
	}
}

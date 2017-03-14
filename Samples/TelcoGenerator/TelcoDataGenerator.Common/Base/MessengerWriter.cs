using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelcoDataGenerator.Common.IBase;

namespace TelcoDataGenerator.Common.Base
{
	public class MessengerWriter : IWriter
	{
		public void Write(string toWrite)
		{
			Messenger.Default.Send<string>(toWrite);
		}
	}
}

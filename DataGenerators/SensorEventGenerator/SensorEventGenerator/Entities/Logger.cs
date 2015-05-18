//********************************************************* 
// 
//    Copyright (c) Microsoft. All rights reserved. 
//    This code is licensed under the Microsoft Public License. 
//    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF 
//    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY 
//    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR 
//    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT. 
// 
//********************************************************* 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace SensorEventGenerator
{
    public class Logger
    {
        private string path;

        public Logger(string path)
        {
            this.path = path;
            var folder = Path.GetDirectoryName(this.path);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

        public void Write(string log)
        {
            var content = string.Format("[{0}] [Information] {1}{2}", DateTime.UtcNow.ToString("o"), log, Environment.NewLine);
            File.AppendAllText(this.path, content);
        }

        public void Write(Exception e)
        {
            var content = string.Format("[{0}] [Exception] {1}{2}{3}{4}", DateTime.UtcNow.ToString("o"), e.Message, Environment.NewLine, e.StackTrace, Environment.NewLine);
            Console.Write(content);
            File.AppendAllText(this.path, content);
        }
    }
}

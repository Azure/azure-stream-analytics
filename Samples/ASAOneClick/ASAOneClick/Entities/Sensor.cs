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

namespace SensorEventGenerator
{
    public class Sensor
    {
        public string time;
        public string dspl;
        public int temp;
        public int hmdt;

        static Random R = new Random();
        static string[] sensorNames = new[] { "sensorA", "sensorB", "sensorC", "sensorD", "sensorE" };
        
        public static Sensor Generate()
        {
            return new Sensor { time = DateTime.UtcNow.ToString(), dspl = sensorNames[R.Next(sensorNames.Length)], temp = R.Next(70, 150), hmdt = R.Next(30, 70) };
        }
    }
}

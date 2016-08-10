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

namespace SecurityBadgeScan
{
    public class Badge
    {
        public string TimeStamp;
        public string Alias;
        public string Building;

        static Random R = new Random();
        static string[] Aliases = new[] { "emp01", "emp02", "emp03", "emp04", "emp05", "emp06", "emp07", "emp08", "emp09", "emp10" };
        static string[] Buildings = new[] { "1", "2", "3", "4", "5", "6", "30", "31", "32", "33", "34", "35", "Millinium", "Red West", "Commons", "Studio A" };

        public static Badge Generate()
        {
            return new Badge { TimeStamp = DateTime.UtcNow.ToString(), Alias = Aliases[R.Next(Aliases.Length)], Building = Buildings[R.Next(Buildings.Length)] };
        }
    }
}

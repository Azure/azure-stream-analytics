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

namespace telcodatagen
{

    class GenConfig
    {
        public int nSets { get; set; }
        public int nFilesPerDump { get; set; }
        public int nCDRPerFile { get; set; }
        public float nCallBackPercent { get; set; }
        public int nDurationHours { get; set; }

        // Constructor
        public GenConfig(int _set, int _files, int _cdr, float _callback, int hours)
        {
            nSets = _set;
            nFilesPerDump = _files;
            nCDRPerFile = _cdr;
            nCallBackPercent = _callback;
            nDurationHours = hours;
        }

        
        // Assume there is only 1 file, and 1 set
        public GenConfig(int _cdr, float _callback, int hours)
        {
            nSets = 1;
            nFilesPerDump = 1;
            nCDRPerFile = _cdr;
            nCallBackPercent = _callback;
            nDurationHours = hours;
        }

        override public String ToString()
        {
            return "#Sets: " + nSets + ",#FilesDump: " + nFilesPerDump + ",#CDRPerFile: " + nCDRPerFile + ",%CallBack: " + nCallBackPercent + ", #DurationHours: " + nDurationHours;
        }
    }
}

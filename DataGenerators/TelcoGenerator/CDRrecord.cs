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
using System.Collections;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace telcodatagen
{
    
    [DataContract]
    public class CDRrecord
    {
        [DataMember]
        public String RecordType {get; set; }
        
        [DataMember]
        public String SystemIdentity {get; set; }
        
        [DataMember]
        public String FileNum {get; set; }
        
        [DataMember]
        public String SwitchNum {get; set; }

        
        [DataMember]
        public String CallingNum {get; set; }

        [DataMember]
        public String CallingIMSI {get; set; }

        [DataMember]
        public String CalledNum {get; set; }

        [DataMember]
        public String CalledIMSI {get; set; }

        [DataMember]
        public String DateS {get; set; }


        [DataMember]
        public String TimeS {get; set; }


        [DataMember]
        public int TimeType {get; set; }

        [DataMember]
        public  int CallPeriod {get; set; }

        [DataMember]
        public String CallingCellID {get; set; }

        [DataMember]
        public String CalledCellID {get; set; }

        [DataMember]
        public String ServiceType {get; set; }

        [DataMember]
        public int Transfer {get; set; }

        [DataMember]
        public String IncomingTrunk {get; set; }

        [DataMember]
        public String OutgoingTrunk {get; set; }

        [DataMember]
        public String MSRN {get; set; }

        [DataMember]
        public String CalledNum2 {get; set; }

        [DataMember]
        public String FCIFlag {get; set; }

        [DataMember]
        public DateTime callrecTime {get; set; }

        


        static string[] columns = {"RecordType","SystemIdentity","FileNum","SwitchNum","CallingNum","CallingIMSI",
                            "CalledNum","CalledIMSI","Date","Time","TimeType","CallPeriod","CallingCellID","CalledCellID",
                            "ServiceType","Transfer","IMEI","EndType","IncomingTrunk","OutgoingTrunk","MSRN","CalledNum2","FCIFlag","DateTime"};

        static string[] ServiceTypeList = { "a", "b", "S", "V" };
        static string[] TimeTypeList = { "a", "d", "r", "s" };
        static string[] EndTypeList = { "0", "3", "4"};
        static string[] OutgoingTrunkList = { "F", "442", "623", "418", "425", "443", "426", "621", "614", "609", "419", "402", "411", "422", "420", "423", "421", "300", "400", "405", "409", "424" };
        static string[] IMSIList = { "466923300507919","466921602131264","466923200348594","466922002560205","466922201102759","466922702346260","466920400352400","466922202546859","466923000886460","466921302209862","466923101048691","466921200135361","466922202613463","466921402416657","466921402237651","466922202679249","466923300236137","466921602343040","466920403025604","262021390056324","466920401237309","466922000696024","466923100098619","466922702341485","466922200432822","466923000464324","466923200779222","466923100807296","466923200408045" };
        static string[] MSRNList = { "886932428687", "886932429021", "886932428306", "1415982715962", "886932429979", "1416916990491", "886937415371", "886932428876", "886932428688", "1412983121877", "886932429242", "1416955584542", "886932428258", "1412930064972", "886932429155", "886932423548", "1415980332015", "14290800303585", "14290800033338", "886932429626", "886932428112", "1417955696232", "1418986850453", "886932428927", "886932429827", "886932429507", "1416960750071", "886932428242", "886932428134", "886932429825" ,""};
        
        static Random coin = new Random();
        Hashtable data;

        public CDRrecord()
        {
            data = new Hashtable();
            _init();
        }

        public void _init()
        {
            int idx = 0;
            // Initialize default values
            data.Add("SystemIdentity", "d0");
            data.Add("RecordType", "MO");
            this.SystemIdentity = "d0";
            this.RecordType = "MO";


            idx = coin.Next(0, TimeTypeList.Length);
            data.Add("TimeType", idx);
            this.TimeType = idx;

            idx = coin.Next(0, ServiceTypeList.Length);
            data.Add("ServiceType", ServiceTypeList[idx]);
            this.ServiceType = ServiceTypeList[idx];

            idx = coin.Next(0, EndTypeList.Length);
            data.Add("EndType", EndTypeList[idx]);
            

            idx = coin.Next(0, OutgoingTrunkList.Length);
            data.Add("OutgoingTrunk", OutgoingTrunkList[idx]);
            this.OutgoingTrunk = OutgoingTrunkList[idx];

            data.Add("Transfer", coin.Next(0, 2));
            this.Transfer = coin.Next(0, 2);

            idx = coin.Next(0, IMSIList.Length);
            data.Add("CallingIMSI", IMSIList[idx]);
            this.CallingIMSI = IMSIList[idx];

            idx = coin.Next(0, IMSIList.Length);
            data.Add("CalledIMSI", IMSIList[idx]);
            this.CalledIMSI = IMSIList[idx];

            idx = coin.Next(0, MSRNList.Length);
            data.Add("MSRN", MSRNList[idx]);
            this.MSRN = MSRNList[idx];
        }

        // set the data for the CDR record
        public void setData(string key, string value)
        {
            if (data.ContainsKey(key))
                data[key] = value;
            else
                data.Add(key, value);

            switch (key)
            {
                case "RecordType": 
                    this.RecordType = value;
                    break;
                case "SystemIdentity":
                    this.SystemIdentity = value;
                    break;
                case "FileNum":
                    this.FileNum = value;
                    break;
                case "SwitchNum":
                    this.SwitchNum = value;
                    break;
                case "CallingNum":
                    this.CallingNum = value;
                    break;
                case "CallingIMSI":
                    this.CallingIMSI = value;
                    break;
                case "CalledNum":
                    this.CalledNum = value;
                    break;
                case "CalledIMSI":
                    this.CalledIMSI = value;
                    break;
                case "Date":
                    this.DateS = value;
                    break;
                case "Time":
                    break;
                    this.TimeS = value;
                case "TimeType":
                    this.TimeType = Int32.Parse(value);
                    break;
                case "CallPeriod":
                    this.CallPeriod = Int32.Parse(value);
                    break;
                case "CallingCellID":
                    this.CallingCellID = value;
                    break;
                case "CalledCellID":
                    this.CalledCellID = value;
                    break;
                case "ServiceType":
                    this.ServiceType = value;
                    break;
                case "Transfer":
                    this.Transfer = Int32.Parse(value);
                    break;                
                case "IncomingTrunk":
                    this.IncomingTrunk = value;
                    break;
                case "OutgoingTrunk":
                    this.OutgoingTrunk = value;
                    break;
                case "MSRN":
                    this.MSRN = value;
                    break;
                case "CalledNum2":
                    this.CalledNum2 = value;
                    break;
                case "FCIFlag":
                    this.FCIFlag = value;
                    break;
                case "DateTime":
                    if (value.Length > 13) { 
                        int hour = Int32.Parse(value.Substring(9, 2));
                        int min = Int32.Parse(value.Substring(11, 2));
                        int secs = Int32.Parse(value.Substring(13, 2));

                        int year = Int32.Parse(value.Substring(0, 4));
                        int month = Int32.Parse(value.Substring(4, 2));
                        int day = Int32.Parse(value.Substring(6, 2));

                        this.callrecTime = new DateTime(year, month, day, hour, min, secs).ToUniversalTime();
                    }
                    
                    break;
            }
        }

        override public String ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < columns.Length; i++)
            {
                if (!data.ContainsKey(columns[i]) || data[columns[i]] == null)
                    sb.Append("");
                else
                    sb.Append(data[columns[i]]);

                if (i < columns.Length - 1)
                    sb.Append(",");
            }

            return sb.ToString();            
        }
    }
}

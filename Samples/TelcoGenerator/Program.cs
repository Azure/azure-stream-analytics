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
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading.Tasks;


namespace telcodatagen
{
    class Program
    {
        #region Fields
        static string eventHubName;
        static EventHubClient client;
        static int partitionCurrent;
        #endregion
        static TextWriter writer = null;

        static void Main(string[] args)
        {
            // Show Usage information
            if (args.Length < 3)
                Usage();

            // Init
            _init();

            // Setup service bus
            string connectionString = GetServiceBusConnectionString();
            NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            eventHubName = ConfigurationManager.AppSettings["EventHubName"];
            partitionCurrent = 0;

            // Assumes you already have the Event Hub created.
            client = EventHubClient.Create(eventHubName);


            // Generate the data
            GenerateData(args);

            Console.ReadKey();
        }

        static void _init()
        {

        }

        private static string GetServiceBusConnectionString()
        {
            string connectionString = ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];
            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("Did not find Service Bus connections string in appsettings (app.config)");
                return string.Empty;
            }
            ServiceBusConnectionStringBuilder builder = new ServiceBusConnectionStringBuilder(connectionString);
            builder.TransportType = TransportType.Amqp;
            return builder.ToString();
        }


        // Generate data
        static void GenerateData(string[] args)
        {
            Queue callBackQ = new Queue();

            // Parse parameters
            // Number of cdr records, probability of fraud, and number of hours
            GenConfig config = new GenConfig(Int32.Parse(args[0]), float.Parse(args[1]), Int32.Parse(args[2]));

            // print statistics
            Console.Error.WriteLine(config);

            // Start the generation
            
            // Generate the call nos
            CallStore mobileNos = new CallStore(100000);

            int numCallbackPerFile = (int) (config.nCallBackPercent * config.nCDRPerFile);

            // Data generation always start with the current time
            

            double timeAdvancementPerSet = ( 0.0 + config.nDurationHours )/ config.nSets;

            Console.Error.WriteLine("Time Increment Per Set: " + timeAdvancementPerSet);

            // Start generating per set
            DateTime simulationTime = DateTime.Now;
            Random r = new Random();

            bool invalidRec = false;
            bool genCallback = false;


            // TOOD: Update this to number of hours
            DateTimeOffset ptTime = DateTimeOffset.Now;
            DateTimeOffset endTime = ptTime.AddHours(config.nDurationHours);
            
            while ( endTime.Subtract(ptTime) >= TimeSpan.Zero  )
            {
                DateTimeOffset currentTime = ptTime;

                Console.WriteLine(String.Format("{0:yyyyMMdd HHmmss}", simulationTime));
                


                    for (int cdr = 0; cdr < config.nCDRPerFile; cdr++)
                    {
                        currentTime = ptTime;

                        // Determine whether to generate an invalid CDR record
                        double pvalue = r.NextDouble();
                        if (pvalue < 0.1)
                            invalidRec = true;
                        else
                            invalidRec = false;

                        // Determine whether there will be a callback
                        pvalue = r.NextDouble();
                        if (pvalue >= config.nCallBackPercent)
                            genCallback = true;
                        else
                            genCallback = false;


                        // Determine called and calling num
                        int calledIdx = r.Next(0, mobileNos.CallNos.Length);
                        int callingIdx = r.Next(0, mobileNos.CallNos.Length);

                                               
                        CDRrecord rec = new CDRrecord();
                        rec.setData("FileNum", "" +  cdr);

                        int switchIdx = r.Next(0, mobileNos.switchCountries.Length);
                        int switchAltIdx = r.Next(0, mobileNos.switchCountries.Length);

                        // Find an alternate switch
                        while (switchAltIdx == switchIdx)
                        {
                            switchAltIdx = r.Next(0, mobileNos.switchCountries.Length);
                        }

                        rec.setData("SwitchNum", mobileNos.switchCountries[switchIdx]);


                        if (invalidRec)
                        {
                            rec.setData("Date", "F");
                            rec.setData("Time", "F");
                            rec.setData("DateTime", "F F");
                        }
                        else
                        {
                            String callDate = String.Format("{0:yyyyMMdd}", currentTime);
                            String callTime = String.Format("{0:HHmmss}", currentTime);

                            rec.setData("Date", callDate);
                            rec.setData("Time", callTime);
                            rec.setData("DateTime", callDate + " " + callTime);

                            String calledNum = mobileNos.CallNos[calledIdx];
                            String callingNum = mobileNos.CallNos[callingIdx];

                            rec.setData("CalledNum", calledNum);
                            rec.setData("CallingNum", callingNum);


                            // Sim card fraud record
                            if (genCallback)
                            {
                                // For call back the A->B end has duration 0
                                rec.setData("CallPeriod", "0");

                                // need to generate another set of no
                                calledIdx = callingIdx;
                                callingIdx = r.Next(0, mobileNos.CallNos.Length);

                                CDRrecord callbackRec = new CDRrecord();
                                callbackRec.setData("FileNum", "" + cdr);
                                callbackRec.setData("SwitchNum", mobileNos.switchCountries[switchAltIdx]);

                                //callbackRec.setData("SwitchNum", "" + (f + 1));

                                // Pertub second 
                                int pertubs = r.Next(0, 30);

                                callDate = String.Format("{0:yyyyMMdd}", currentTime);
                                callTime = String.Format("{0:HHmmss}", currentTime.AddMinutes(pertubs));

                                callbackRec.setData("Date", callDate);
                                callbackRec.setData("Time", callTime);
                                callbackRec.setData("DateTime", callDate + " " + callTime);

                                // Set it as the same calling IMSI
                                callbackRec.setData("CallingIMSI", rec.CallingIMSI);

                                calledNum = mobileNos.CallNos[calledIdx];
                                callingNum = mobileNos.CallNos[callingIdx];

                                callbackRec.setData("CalledNum", calledNum);
                                callbackRec.setData("CallingNum", callingNum);


                                // Determine duration of call
                                int callPeriod = r.Next(1, 1000);
                                callbackRec.setData("CallPeriod", "" + callPeriod);

                                // Enqueue the call back rec 
                                callBackQ.Enqueue(callbackRec);
                                cdr++;
                            }
                            else
                            {
                                int callPeriod = r.Next(1, 800);
                                rec.setData("CallPeriod", "" + callPeriod);
                            }
                        }

                        // send cdr rec to output
                        //if (genCallback)  Console.Write("callback A->B ");
                        outputCDRRecs(rec);

                        if (callBackQ.Count > 0 && (cdr % 7 == 0))
                        {
                            CDRrecord drec;
                            drec = (CDRrecord)callBackQ.Dequeue();
                            outputCDRRecs(drec);

                            //Console.Write("callback C->A!");
                            //outputCDRRecs(s, f, drec);
                        }

                        // Sleep for 1000ms
                        System.Threading.Thread.Sleep(100);

                        // get the current time after generation
                        ptTime = DateTimeOffset.Now;
                    } // cdr


                    // Clear the remaining entries in the call back queue
                    if (callBackQ.Count > 0)
                    {
                        // need to empty queue
                        while (callBackQ.Count > 0)
                        {
                            CDRrecord dr = (CDRrecord)callBackQ.Dequeue();
                            outputCDRRecs(dr);
                            //outputCDRRecs(s, f, dr);
                        }
                    }

                    // close the file
                    if (writer != null)
                    {
                        writer.Flush();
                        writer.Close();
                        writer = null;
                    }
                
                
                // Advance Time
                if (timeAdvancementPerSet < 1.0)
                    simulationTime = simulationTime.AddMinutes(timeAdvancementPerSet * 60);
                else
                    simulationTime = simulationTime.AddHours(timeAdvancementPerSet);

                

                // Sleep for 1000ms
                System.Threading.Thread.Sleep(1000);

            } // while - within duration

        
        }


        // Print usage information
        static void Usage()
        {
            // In this case, we treat the #FilesPerDump as the number of switch, which is not 100% true

            Console.WriteLine("Usage: telcodatagen [#NumCDRsPerHour] [SIM Card Fraud Probability] [#DurationHours]");
            System.Environment.Exit(-1);
        }


        // Handle output of cdr recs
        static void outputCDRRecs(CDRrecord r)
        {
            //Console.WriteLine("RecordType,SystemIdentity,FileNum,SwitchNum,CallingNum,CallingIMSI,CalledNum,CalledIMSI,Date,Time,TimeType,CallPeriod,CallingCellID,CalledCellID,ServiceType");
            //Console.WriteLine(r);

            try
            {
                List<Task> tasks = new List<Task>();
                var serializedString = JsonConvert.SerializeObject(r);
                EventData data = new EventData(Encoding.UTF8.GetBytes(serializedString))
                {
                    PartitionKey = r.CallingIMSI
                };

                partitionCurrent++;
                if (partitionCurrent >8)
                    partitionCurrent = 0;

                // Send the metric to Event Hub
                tasks.Add(client.SendAsync(data));

                Console.WriteLine(r);

                Task.WaitAll(tasks.ToArray());
            }
            catch (Exception e)
            {
                Console.WriteLine("Error on send: " + e.Message);
            }
        }

    }
}

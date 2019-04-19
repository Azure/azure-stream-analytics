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

using System.Collections.Generic;
using System.IO;

using Microsoft.Azure.StreamAnalytics;
using Microsoft.Azure.StreamAnalytics.Serialization;

namespace ExampleCustomCode.Serialization
{
    // Deserializes a stream into objects of type CustomEvent. 
    // It reads the Stream line by line and assumes each line has three columns separated by ",".
    // Writes an error to diagnostics and skips the line otherwise.
    public class CustomCsvDeserializer : StreamDeserializer<CustomEvent>
    {
        // streamingDiagnostics is used to write error to diagnostic logs
        private StreamingDiagnostics streamingDiagnostics;
        
        // Initializes the operator and provides context that is required for publishing diagnostics        
        public override void Initialize(StreamingContext streamingContext)
        {
            this.streamingDiagnostics = streamingContext.Diagnostics;
        }
        
        // Deserializes a stream into objects of type CustomEvent        
        public override IEnumerable<CustomEvent> Deserialize(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                string line = sr.ReadLine();
                while (line != null)
                {
                    if (line.Length > 0 && !string.IsNullOrWhiteSpace(line))
                    {
                        string[] parts = line.Split(',');
                        if (parts.Length != 3)
                        {
                            //if there are not 3 columns in the input, write error to diagnostic log, skip the line and continue deserializing rest of the stream.
                            streamingDiagnostics.WriteError("Did not get expected number of columns", $"Invalid line: {line}");
                        }
                        else
                        {
                            // create a new CustomEvent object with 3 values
                            yield return new CustomEvent()
                            {
                                Column1 = parts[0],
                                Column2 = parts[1],
                                Column3 = parts[2]
                            };
                        }
                    }
                    line = sr.ReadLine();
                }
            }
        }
    }

    /*
    The CustomEvent class follows the rules mentioned below.
    
    All public fields are either:
        1. One of [long, DateTime, string, double] or their nullableequivalents
        2. Another struct or class following the same rules
        3. Array of type <T2> that follows the same rules
        4. IList`T2` where T2 follows the same rules
        5. Does not have any recursive types.
     */
    public class CustomEvent
    {
        public string Column1 { get; set; }

        public string Column2 { get; set; }

        public string Column3 { get; set; }
    }
}

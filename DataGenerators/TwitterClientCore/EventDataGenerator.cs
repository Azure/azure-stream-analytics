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
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using Microsoft.Azure.EventHubs;

namespace TwitterClient
{
    internal class EventDataGenerator : IObserver<string>
    {
        private readonly int maxSizePerMessageInBytes;
        private readonly IObserver<EventData> eventDataOutputObserver;
        private readonly Stopwatch waitIntervalStopWatch = Stopwatch.StartNew();
        private readonly TimeSpan maxTimeToBuffer;

        private MemoryStream memoryStream;
        private GZipStream gzipStream;
        private StreamWriter streamWriter;

        private long messagesCount = 0;
        private long tweetCount = 0;

        public EventDataGenerator(IObserver<EventData> eventDataOutputObserver, int maxSizePerMessageInBytes, int maxSecondsToBuffer)
        {
            this.maxTimeToBuffer = TimeSpan.FromSeconds(maxSecondsToBuffer);
            this.maxSizePerMessageInBytes = maxSizePerMessageInBytes;
            this.eventDataOutputObserver = eventDataOutputObserver;
            this.memoryStream = new MemoryStream(this.maxSizePerMessageInBytes);
            this.gzipStream = new GZipStream(this.memoryStream, CompressionMode.Compress);
            this.streamWriter = new StreamWriter(this.gzipStream);
        }

        public void OnCompleted()
        {
            this.SendEventData(isCompleted: true);
            Console.WriteLine($"Completed Sent TweetCount = {this.tweetCount} MessageCount = {this.messagesCount}");
            this.eventDataOutputObserver.OnCompleted();
        }

        public void OnError(Exception error)
        {
            this.eventDataOutputObserver.OnError(error);
        }

        public void OnNext(string value)
        {
            this.tweetCount++;
            this.streamWriter.WriteLine(value);
            this.streamWriter.Flush();
            if(this.waitIntervalStopWatch.Elapsed > this.maxTimeToBuffer || this.memoryStream.Length >= this.maxSizePerMessageInBytes)
            {
                this.SendEventData();
            }
        }

        private void SendEventData(bool isCompleted = false)
        {
            if(this.memoryStream.Length == 0)
            {
                return;
            }

            this.messagesCount++;
            this.gzipStream.Close();
            var eventData = new EventData(this.memoryStream.ToArray());
            this.eventDataOutputObserver.OnNext(eventData);

            this.gzipStream.Dispose();
            this.memoryStream.Dispose();
            if(!isCompleted)
            {
                this.memoryStream = new MemoryStream(this.maxSizePerMessageInBytes);
                this.gzipStream = new GZipStream(this.memoryStream, CompressionMode.Compress);
                this.streamWriter = new StreamWriter(this.gzipStream);
            }

            Console.WriteLine($"Time: {DateTime.UtcNow:o} Sent TweetCount = {this.tweetCount} MessageCount = {this.messagesCount}");
            this.waitIntervalStopWatch.Restart();
        }
    }
}
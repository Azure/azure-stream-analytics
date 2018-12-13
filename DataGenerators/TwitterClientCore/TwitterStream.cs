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
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;

namespace TwitterClient
{
    internal static class TwitterStream
    {
        public static IEnumerable<string> StreamStatuses(TwitterConfig config)
        {
            TextReader streamReader = ReadTweets(config);
            if(streamReader == StreamReader.Null)
            {
                throw new Exception("Could not connect to twitter with credentials provided");
            }

            while (true)
            {
                string line = null;
                try 
                { 
                    line = streamReader.ReadLine(); 
                }
                catch (Exception e) 
                { 
                    Console.WriteLine($"Ignoring :{e}");
                }

                if (!string.IsNullOrWhiteSpace(line))
                {
                    yield return line;
                }

                // Reconnect to the twitter feed.
                if (line == null)
                {
                    streamReader = ReadTweets(config);
                }
            }
        }

        static TextReader ReadTweets(TwitterConfig config)
        {
            var oauth_version = "1.0";
            var oauth_signature_method = "HMAC-SHA1";

            // unique request details
            var oauth_nonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
            var oauth_timestamp = Convert.ToInt64(
                (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc))
                    .TotalSeconds).ToString();

            var resource_url = "https://stream.twitter.com/1.1/statuses/filter.json";

            // create oauth signature
            var baseString = string.Format(
                "oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}&" +
                "oauth_timestamp={3}&oauth_token={4}&oauth_version={5}&track={6}",
                config.OAuthConsumerKey,
                oauth_nonce,
                oauth_signature_method,
                oauth_timestamp,
                config.OAuthToken,
                oauth_version,
                Uri.EscapeDataString(config.Keywords));

            baseString = string.Concat("POST&", Uri.EscapeDataString(resource_url), "&", Uri.EscapeDataString(baseString));

            var compositeKey = string.Concat(Uri.EscapeDataString(config.OAuthConsumerSecret),
            "&", Uri.EscapeDataString(config.OAuthTokenSecret));

            string oauth_signature;
            using (var hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(compositeKey)))
            {
                oauth_signature = Convert.ToBase64String(
                hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes(baseString)));
            }

            // create the request header
            var authHeader = string.Format(
                "OAuth oauth_nonce=\"{0}\", oauth_signature_method=\"{1}\", " +
                "oauth_timestamp=\"{2}\", oauth_consumer_key=\"{3}\", " +
                "oauth_token=\"{4}\", oauth_signature=\"{5}\", " +
                "oauth_version=\"{6}\"",
                Uri.EscapeDataString(oauth_nonce),
                Uri.EscapeDataString(oauth_signature_method),
                Uri.EscapeDataString(oauth_timestamp),
                Uri.EscapeDataString(config.OAuthConsumerKey),
                Uri.EscapeDataString(config.OAuthToken),
                Uri.EscapeDataString(oauth_signature),
                Uri.EscapeDataString(oauth_version)
            );

            // make the request
            ServicePointManager.Expect100Continue = false;

            var postBody = "track=" + config.Keywords;
            resource_url += "?" + postBody;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(resource_url);
            request.Headers.Add("Authorization", authHeader);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.PreAuthenticate = true;
            request.AllowWriteStreamBuffering = true;
            request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.BypassCache);

            // bail out and retry after 5 seconds
            var tresponse = request.GetResponseAsync();
            if (tresponse.Wait(5000))
                return new StreamReader(tresponse.Result.GetResponseStream());
            else
            {
                request.Abort(); 
                return StreamReader.Null;
            }
        }
    }
}
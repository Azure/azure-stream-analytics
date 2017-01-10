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
using System.Web;

namespace TwitterClient.Common
{
    public struct TwitterConfig
    {
        public readonly string OAuthToken;
        public readonly string OAuthTokenSecret;
        public readonly string OAuthConsumerKey;
        public readonly string OAuthConsumerSecret;
        public readonly string Keywords;
		public readonly string SearchGroups;

        public TwitterConfig(string oauthToken, string oauthTokenSecret, string oauthConsumerKey, string oauthConsumerSecret, string keywords, string searchGroups)
        {
            OAuthToken = oauthToken;
            OAuthTokenSecret = oauthTokenSecret;
            OAuthConsumerKey = oauthConsumerKey;
            OAuthConsumerSecret = oauthConsumerSecret;
            Keywords = keywords;
			SearchGroups = searchGroups;

		}
    }

    [DataContract]
    public class TwitterUser
    {
        [DataMember(Name = "time_zone")]               public string TimeZone;
        [DataMember(Name = "name")]                    public string Name;
        [DataMember(Name = "profile_image_url")]       public string ProfileImageUrl;
    }

    [DataContract]
    public class Tweet
    {
        [DataMember(Name = "id")]                      public Int64 Id;
        [DataMember(Name = "in_reply_to_status_id")]   public Int64? ReplyToStatusId;
        [DataMember(Name = "in_reply_to_user_id")]     public Int64? ReplyToUserId;
        [DataMember(Name = "in_reply_to_screen_name")] public string ReplyToScreenName;
        [DataMember(Name = "retweeted")]               public bool Retweeted;
        [DataMember(Name = "text")]                    public string Text;
        [DataMember(Name = "lang")]                    public string Language;
        [DataMember(Name = "source")]                  public string Source;
        [DataMember(Name = "retweet_count")]           public string RetweetCount;
        [DataMember(Name = "user")]                    public TwitterUser User;
        [DataMember(Name = "created_at")]              public string CreatedAt;
        [IgnoreDataMember]                             public string RawJson;
		public Tweet()
		{
			keepRunning = true;
		}
		public bool keepRunning { get; set; }
        public IEnumerable<Tweet> StreamStatuses(TwitterConfig config)
        {
            DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(Tweet));

            var streamReader = ReadTweets(config);

            while (keepRunning)
            {
                string line = null;
                try { line = streamReader.ReadLine(); }
                catch (Exception) { }

                if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("{\"delete\""))
                {
                    var result = (Tweet)jsonSerializer.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(line)));
                    result.RawJson = line;
                    yield return result;
                }

                // Oops the Twitter has ended... or more likely some error have occurred.
                // Reconnect to the twitter feed.
                if (line == null)
                {
                    streamReader = ReadTweets(config);
                }
            }
        }
		public HttpWebRequest Request { get;  set;}

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

            var postBody = "track=" + HttpUtility.UrlEncode(config.Keywords);
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


    public class TwitterPayload
    {
        public Int64 ID;
        public DateTime CreatedAt;
        public string UserName;
        public string TimeZone;
        public string ProfileImageUrl;
        public string Text;
        public string Language;
        public string Topic;
        public int SentimentScore;

        public string RawJson;

        public override string ToString()
        {
            return new { ID, CreatedAt, UserName, TimeZone, ProfileImageUrl, Text, Language, Topic, SentimentScore }.ToString();
        }
    }

    public class Payload
    {
        public DateTime CreatedAt { get; set; }
        public string Topic { get; set; }
		public int SentimentScore { get; set; }
		public string Author { get; set; }
		public string Text { get; set; }
		public bool SendExtended { get; set; }

		public override string ToString()
        {
            return SendExtended ?  new { CreatedAt, Topic, SentimentScore, Author, Text }.ToString() : new { CreatedAt, Topic, SentimentScore }.ToString();
        }
    }

    public class TwitterMin
    {
        public Int64 ID;
        public DateTime CreatedAt;
        public string UserName;
//        public string TimeZone;
//        public string ProfileImageUrl;
        public string Text;
//        public string Language;
        public string Topic;
        public int SentimentScore;

//        public string RawJson;

        public override string ToString()
        {
            return new { ID, CreatedAt, UserName, Text, Topic, SentimentScore }.ToString();
        }
    }

}

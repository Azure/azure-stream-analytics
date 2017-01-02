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

using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using TwitterClient.Common.MASC;


namespace TwitterClient.Common
{
    public static class Sentiment
    {
        public static TwitterPayload ComputeScore(Tweet tweet, string searchGroups, string mode)
        {

            return new TwitterPayload
            {
                ID = tweet.Id,
                CreatedAt = ParseTwitterDateTime(tweet.CreatedAt),
                UserName = tweet.User != null ? tweet.User.Name : null,
                TimeZone = tweet.User != null ? (tweet.User.TimeZone != null ? tweet.User.TimeZone : "(unknown)") : "(unknown)", 
                ProfileImageUrl = tweet.User != null ? (tweet.User.ProfileImageUrl != null ? tweet.User.ProfileImageUrl : "(unknown)") : "(unknown)",
                Text = tweet.Text,
                Language = tweet.Language != null ? tweet.Language : "(unknown)",
                RawJson = tweet.RawJson,
                SentimentScore = (int)Analyze(tweet.Text),
                Topic = DetermineTopicEfficiently(tweet.Text, searchGroups, mode),
            };
        }

        static DateTime ParseTwitterDateTime(string p)
        {
            if (p == null)
                return DateTime.Now;
            p = p.Replace("+0000 ", "");
            DateTimeOffset result;

            if (DateTimeOffset.TryParseExact(p, "ddd MMM dd HH:mm:ss yyyy", CultureInfo.GetCultureInfo("en-us").DateTimeFormat, DateTimeStyles.AssumeUniversal, out result))
                return result.DateTime;
            else
                return DateTime.Now;
        }

        enum SentimentScore
        {
            Positive = 4,
            Neutral = 2,
            Negative = 0,
            Undefined = -1
        }

		//Mark Rowe
        static SentimentScore Analyze(string textToAnalyze)
        {
            try
            {
                string url = string.Format("http://www.sentiment140.com/api/classify?text={0}",
                                            HttpUtility.UrlEncode(textToAnalyze, Encoding.UTF8));
                var response = HttpWebRequest.Create(url).GetResponse();

                using (var streamReader = new StreamReader(response.GetResponseStream()))
                {
                    try
                    {
                        // Read from source
                        var line = streamReader.ReadLine();

                        // Parse
                        var jObject = JObject.Parse(line);

                        int polarity = jObject.SelectToken("results", true).SelectToken("polarity", true).Value<int>();
						if (polarity == -1)
						{
							return SentimentScore.Undefined;
						}
						return (SentimentScore)polarity;
                    }
                    catch (Exception)
                    {
						//False Positive
						//return SentimentScore.Neutral;
						return SentimentScore.Undefined;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Sentiment calculation FAILED with:/n{0}", e);
				//False Positive
				//return SentimentScore.Neutral;
				return SentimentScore.Undefined;
			}
        }

        /// <summary>
        /// This is a simple text analysis from the twitter text based on some keywords
        /// </summary>
        /// <param name="tweetText"></param>
        /// <param name="keywordFilters"></param>
        /// <returns></returns>
        //static string DetermineTopc(string tweetText, string keywordFilters)
        //{
        //    if (string.IsNullOrEmpty(tweetText))
        //        return string.Empty;

        //    string subject = string.Empty;

        //    //keyPhrases are specified in app.config separated by commas.  Can have no leading or trailing spaces.  Example of key phrases in app.config
        //    //	<add key="twitter_keywords" value="Microsoft, Office, Surface,Windows Phone,Windows 8,Windows Server,SQL Server,SharePoint,Bing,Skype,XBox,System Center"/><!--comma to spit multiple keywords-->
        //    string[] keyPhrases = keywordFilters.Split(',');

        //    foreach (string keyPhrase in keyPhrases)
        //    {
        //        subject = keyPhrase;

        //        //a key phrase may have multiple key words, like: Windows Phone.  If this is the case we will only assign it a subject if both words are 
        //        //included and in the correct order. For example, a tweet will match if "Windows 8" is found within the tweet but will not match if
        //        // the tweet is "There were 8 broken Windows".  This is not case sensitive

        //        //Creates one array that breaks the tweet into individual words and one array that breaks the key phrase into individual words.  Within 
        //        //This for loop another array is created from the tweet that includes the same number of words as the keyphrase.  These are compared.  For example,
        //        // KeyPhrase = "Microsoft Office" Tweet= "I Love Microsoft Office"  "Microsoft Office" will be compared to "I Love" then "Love Microsoft" and 
        //        //Finally "Microsoft Office" which will be returned as the subject.  if no match is found "Do Not Include" is returned. 
        //        string[] KeyChunk = keyPhrase.Trim().Split(' ');
        //        string[] tweetTextChunk = tweetText.Split(' ');
        //        string Y;
        //        for (int i = 0; i <= (tweetTextChunk.Length - KeyChunk.Length); i++)
        //        {
        //            Y = null;
        //            for (int j = 0; j <= (KeyChunk.Length - 1); j++)
        //            {
        //                Y += tweetTextChunk[(i + j)] + " ";
        //            }
        //            if (Y != null) Y = Y.Trim();
        //            if (Y.ToUpper().Contains(keyPhrase.ToUpper()))
        //            {
        //                return subject;
        //            }
        //        }
        //    }

        //    return "Unknown";
        //}

		//MWR Add multikeyword so "bob car" can be found in "the car was bobs"
		public static string DetermineTopicEfficiently(string tweetText, string searchGroups, string mode)// bool multikeyword = false, bool all = false)
		{
			var eachGroup = searchGroups.Split('|').ToList();
			foreach(var group in eachGroup)
			{
				var allKeywords = group.Split(',').ToList();
				var allTweetText = tweetText.Split(' ').ToList();
				var subject = string.Empty;
				var all = mode.ContainsIgnoreCase("all");
				//Just look for the whole keyword in the entire phrase.. WAY LESS CYCLES than each set of words. 
				var allKeywordsRemoved = allKeywords.ToList();
				foreach (var keyword in allKeywords)
				{
					if (tweetText.ContainsIgnoreCase(keyword))
					{
						if (all)
						{
							allKeywordsRemoved.Remove(keyword);
							if (!allKeywordsRemoved.Any())
							{
								return string.Join(",", allKeywords);
							}
						}
						else
						{
							return keyword;
						}

					}
				}

			}
				return "No Match";

		}
    }
}

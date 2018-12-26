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

namespace TwitterClient
{
    public struct TwitterConfig
    {
        public readonly string OAuthToken;
        public readonly string OAuthTokenSecret;
        public readonly string OAuthConsumerKey;
        public readonly string OAuthConsumerSecret;
        public readonly string Keywords;

        public TwitterConfig(string oauthToken, string oauthTokenSecret, string oauthConsumerKey, string oauthConsumerSecret, string keywords)
        {
            OAuthToken = oauthToken;
            OAuthTokenSecret = oauthTokenSecret;
            OAuthConsumerKey = oauthConsumerKey;
            OAuthConsumerSecret = oauthConsumerSecret;
            Keywords = keywords;
        }
    }
}

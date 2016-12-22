using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using TwitterClient;
using System.Reactive.Linq;

namespace TwitterWpfClient.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
		private Brush RunColor = Brushes.Green;
		private Brush StopColor = Brushes.White;
		private Brush _currentColor;
		public Brush CurrentColor
		{
			get { return _currentColor; }
			set { Set(() => CurrentColor, ref _currentColor, value); }
		}
		private ObservableCollection<Payload> _allReadingsWithTopic;
		public ObservableCollection<Payload> AllReadingsWithTopic
		{
			get { return _allReadingsWithTopic; }
			set { Set(() => AllReadingsWithTopic, ref _allReadingsWithTopic, value); }
		}

		private bool _clearUnfoundSentiment;
		public bool ClearUnfoundSentiment
		{
			get { return _clearUnfoundSentiment; }
			set { Set(() => ClearUnfoundSentiment, ref _clearUnfoundSentiment, value); }
		}
		private bool _sendExtendedInformation;
		public bool SendExtendedInformation
		{
			get { return _sendExtendedInformation; }
			set { Set(() => SendExtendedInformation, ref _sendExtendedInformation, value); }
		}
		private bool _requireAll;
		public bool RequireAll
		{
			get { return _requireAll; }
			set { Set(() => RequireAll, ref _requireAll, value); }
		}
		//private DateTime _created;
		//public DateTime Created
		//{
		//	get { return _created; }
		//	set { Set(() => Created, ref _created, value); }
		//}

		//private int _sentimentScore;
		//public int SentimentScore
		//{
		//	get { return _sentimentScore; }
		//	set { Set(() => SentimentScore, ref _sentimentScore, value); }
		//}

		//private string _author;
		//public string Author
		//{
		//	get { return _author; }
		//	set { Set(() => Author, ref _author, value); }
		//}

		//private string _topic;
		//public string Topic
		//{
		//	get { return _topic; }
		//	set { Set(() => Topic, ref _topic, value); }
		//}

		//private string _text;
		//public string Text
		//{
		//	get { return _text; }
		//	set { Set(() => Text, ref _text, value); }
		//}
		private string _oAuthToken;
		public string OAuthToken
		{
			get { return _oAuthToken; }
			set { Set(() => OAuthToken, ref _oAuthToken, value); }
		}
		private string _oAuthTokenSecret;
		public string OAuthTokenSecret
		{
			get { return _oAuthTokenSecret; }
			set { Set(() => OAuthTokenSecret, ref _oAuthTokenSecret, value); }
		}

		private string _oAuthCustomerKey;
		public string OAuthCustomerKey
		{
			get { return _oAuthCustomerKey; }
			set { Set(() => OAuthCustomerKey, ref _oAuthCustomerKey, value); }
		}
		private string _oAuthConsumerSecret;
		public string OAuthConsumerSecret
		{
			get { return _oAuthConsumerSecret; }
			set { Set(() => OAuthConsumerSecret, ref _oAuthConsumerSecret, value); }
		}
		private string _searchGroups;
		public string SearchGroups
		{
			get { return _searchGroups; }
			set { Set(() => SearchGroups, ref _searchGroups, value); }
		}

		private string _eventHubName;
		public string EventHubName
		{
			get { return _eventHubName; }
			set { Set(() => EventHubName, ref _eventHubName, value); }
		}
		private string _eventHubConnectionString;
		public string EventHubConnectionString
		{
			get { return _eventHubConnectionString; }
			set { Set(() => EventHubConnectionString, ref _eventHubConnectionString, value); }
		}

		/// <summary>
		/// Initializes a new instance of the MainViewModel class.
		/// </summary>
		public MainViewModel()
        {
			CurrentColor = StopColor;
        }
		public RelayCommand StartStop
		{
			get
			{
				return new RelayCommand(() => {
					var isRunning = CurrentColor == RunColor;
					CurrentColor = isRunning ? StopColor :RunColor;
					var shouldRun = !isRunning;
					if (shouldRun)
					{
						Run();
					}
					else
					{
						Stop();
					}
				});
			}
		}

		private void Stop()
		{
			if (SendingPayload != null)
			{
				SendingPayload.Dispose();
			}
					
		}
		IDisposable SendingPayload { get; set; }

		private void Run()
		{
			var keywords = SearchGroups.Contains("|") ? string.Join(",", SearchGroups.Split('|')) : SearchGroups;
			var config = new EventHubConfig();
			config.ConnectionString = EventHubConnectionString;
			config.EventHubName = EventHubName;


			var myEventHubObserver = new EventHubObserver(config);

			var sendingPayload = Tweet.StreamStatuses(new TwitterConfig(OAuthToken, OAuthTokenSecret, OAuthCustomerKey, OAuthConsumerSecret,
				keywords, SearchGroups)).Select(tweet => Sentiment.ComputeScore(tweet, SearchGroups, RequireAll ? "all" : "any")).Select(tweet => new Payload { CreatedAt = tweet.CreatedAt, Topic = tweet.Topic, SentimentScore = tweet.SentimentScore, Author = tweet.UserName, Text = tweet.Text, SendExtended = SendExtendedInformation });
			if (ClearUnfoundSentiment)
			{
				sendingPayload = sendingPayload.Where(e => e.SentimentScore > -1);
			}
			sendingPayload = sendingPayload.Where(e => e.Topic != "No Match");
			SendingPayload = sendingPayload.ToObservable().Subscribe(myEventHubObserver);
		}
	}
}
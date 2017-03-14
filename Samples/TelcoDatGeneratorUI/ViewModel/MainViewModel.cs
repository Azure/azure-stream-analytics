using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using TelcoDataGenerator.Common;
using TelcoDataGenerator.Common.Engine;
using TelcoDataGenerator.Common.IBase;

namespace TelcoDataGeneratorUI.ViewModel
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
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>

		private const string Running = "\u25A0 ";

		private const string Stopped = "\u25B6";

		private ObservableCollection<string> _allReadingsWithTopic;

		private string _buttonText;

		private bool _clearUnfoundSentiment;

		private Brush _currentColor;

		private string _eventHubConnectionString;

		private string _eventHubName;

		private string _oAuthConsumerSecret;

		private string _oAuthCustomerKey;

		private string _oAuthToken;

		private string _oAuthTokenSecret;

		private bool _requireAll;

		private string _searchGroups;

		private bool _sendExtendedInformation;

		/// <summary>
		/// Initializes a new instance of the MainViewModel class.
		/// </summary>
		private Brush RunColor = Brushes.Green;

		private Brush StopColor = Brushes.Pink;

		/// <summary>
		/// Initializes a new instance of the MainViewModel class.
		/// </summary>
		public MainViewModel()
		{
			CurrentColor = RunColor;
			ButtonText = Stopped;
			RegisterAggregates();
			AllReadingsWithTopic = new ObservableCollection<string>();
			AllReadingsWithTopic.Add("Welcome to the TwitterWPF Client - Bugs? Contact Mark.Rowe@microsoft.com");
			AllReadingsWithTopic.Add("Setting above are Pulled from .CONFIG or edit manually here");
			LoadFromConfigIfAvailable();
			SmallSave = true;
		}

		public ObservableCollection<string> AllReadingsWithTopic
		{
			get { return _allReadingsWithTopic; }
			set { Set(() => AllReadingsWithTopic, ref _allReadingsWithTopic, value); }
		}

		public string ButtonText
		{
			get { return _buttonText; }
			set { Set(() => ButtonText, ref _buttonText, value); }
		}
		private int _durationInHours;
		public int DurationInHours
		{
			get { return _durationInHours; }
			set { Set(() => DurationInHours, ref _durationInHours, value); }
		}
		private int _numberOfCDRs;
		public int NumberOfCDRs
		{
			get { return _numberOfCDRs; }
			set { Set(() => NumberOfCDRs, ref _numberOfCDRs, value); }
		}

		private float _simCardProbability;
		public float SimCardProbability
		{
			get { return _simCardProbability; }
			set { Set(() => SimCardProbability, ref _simCardProbability, value); }
		}



		public Brush CurrentColor
		{
			get { return _currentColor; }
			set { Set(() => CurrentColor, ref _currentColor, value); }
		}

		public string EventHubConnectionString
		{
			get { return _eventHubConnectionString; }
			set { Set(() => EventHubConnectionString, ref _eventHubConnectionString, value); }
		}

		public string EventHubName
		{
			get { return _eventHubName; }
			set { Set(() => EventHubName, ref _eventHubName, value); }
		}

	
		

		private bool _smallSave;
		public bool SmallSave
		{
			get { return _smallSave; }
			set { Set(() => SmallSave, ref _smallSave, value); }
		}
		public bool SendExtendedInformation
		{
			get { return _sendExtendedInformation; }
			set { Set(() => SendExtendedInformation, ref _sendExtendedInformation, value); }
		}

		public RelayCommand StartStop
		{
			get
			{
				return new RelayCommand(() =>
				{
					if (EventHubConnectionString.ToLower().Contains("entitypath"))
					{
						MessageBox.Show("Please remove 'entitypath=' and the value from your connection string");
						return;
					}
					if (NumberOfCDRs == 0 ||
					SimCardProbability == 0 ||
					DurationInHours ==0)
					{
						MessageBox.Show("Missing Required parameters, Defaults are Number of CDR's 1000, SimCard Probability .2, Duration In hours 2");
						return;
					}
					Client = EventHubClient.Create(EventHubName);
					Engine = new TelcoGenerationEngine(Client, false);

					var isRunning = CurrentColor == StopColor;
					CurrentColor = isRunning ? RunColor : StopColor;
					var shouldRun = !isRunning;
					ButtonText = shouldRun ? Running : Stopped;
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

		

		private void LoadFromConfigIfAvailable()
		{
			NumberOfCDRs = int.Parse(ConfigurationManager.AppSettings["NumberOfCDRs"]);
			SimCardProbability = float.Parse( ConfigurationManager.AppSettings["SimCardProbability"]);
			DurationInHours = int.Parse(ConfigurationManager.AppSettings["DurationInHours"]);
		

			EventHubConnectionString = ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];
			EventHubName = ConfigurationManager.AppSettings["EventHubName"];

		
		}

		private string _last = "";

		private void RegisterAggregates()
		{
			GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<string>(this, e =>
			{
				if (e == _last) 
				{
					return;
				}
				_last = e;
				App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
				{
					AllReadingsWithTopic.Insert(2, e);
				});
			});
		}
		private string CreateRealSymbol(string symbol)
		{
			//return string.Format("#{0} OR ${0} OR @{0}",symbol.Trim());
			return string.Format("${0}", symbol.Trim());
		}
		Task RunTask { get; set; }
		CancellationToken CancellationToken { get; set; }
		CancellationTokenSource TokenSource { get; set; }
		EventHubClient Client { get; set; }
		TelcoGenerationEngine Engine { get; set; }
		private async System.Threading.Tasks.Task Run()
		{

			RunTask = System.Threading.Tasks.Task.Factory.StartNew(() =>
			{
				
				try
				{
					// Show Usage information
					NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(EventHubConnectionString);
					// Generate the data
					Engine.GenerateData(new string[] { NumberOfCDRs.ToString().Trim(), SimCardProbability.ToString().Trim(), DurationInHours.ToString().Trim() });
				}
				catch (Exception e)
				{
					AllReadingsWithTopic.Insert(0, e.Message);
				}
				

			});

			return;
		}


	
		private void Stop()
		{
			FlowController.CanRun = false;

		}
	}
}
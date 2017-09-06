using System.Configuration;

namespace TollApp.Configs
{
    /// <summary>
    /// Reads app settings from app.config file
    /// </summary>
    public static class CloudConfiguration
    {
        #region DocumentDB config

        public static string DocumentDbUri => ConfigurationManager.AppSettings["DocumentDbUri"];
        public static string DocumentDbKey => ConfigurationManager.AppSettings["DocumentDbKey"];
        public static string DocumentDbDatabaseName => ConfigurationManager.AppSettings["DocumentDbDatabaseName"];
        public static string DocumentDbCollectionName => ConfigurationManager.AppSettings["DocumentDbCollectionName"];

        #endregion

        #region Storage account configs

        public static string StorageAccountUrl => ConfigurationManager.AppSettings["ConnectionString"];
        public static string StorageAccountContainer => ConfigurationManager.AppSettings["Container"];
        public static string RegistrationFileBlob => ConfigurationManager.AppSettings["RegistrationFileBlob"];

        #endregion

        #region Event Hub config

        public static string EventHubConnectionString => ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];
        public static string EntryName => ConfigurationManager.AppSettings["EntryName"];
        public static string ExitName => ConfigurationManager.AppSettings["ExitName"];

        #endregion

        #region Timer settings
        /// <summary>
        /// Specifies the amount of time between vehicles entering the tollbooth in seconds.
        /// </summary>
        /// <value>
        /// The timer interval.
        /// </value>
        public static string TimerInterval => ConfigurationManager.AppSettings["TimerInterval"];
      
        #endregion

    }
}

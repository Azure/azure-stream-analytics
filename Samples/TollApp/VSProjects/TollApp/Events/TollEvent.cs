namespace TollApp.Events
{
    /// <summary>
    /// Contains common properties used by Entry and Exit events
    /// </summary>
    public class TollEvent
    {
        public int TollId { get; set; }

        public string LicensePlate { get; set; }

    }
}

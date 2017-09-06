namespace TollApp.Models
{
    /// <summary>
    /// The car object entering/leaving the tollbooth
    /// </summary>
    public class CarModel
    {
        public string Make { get; set; }
        public string Model { get; set; }
        public int VehicleType { get; set; }
        public double VehicleWeight { get; set; }

    }
}

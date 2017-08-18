namespace TollApp.Models
{
    public class CarModel
    {
        public readonly string Make;
        public readonly string Model;
        public readonly int VehicleType;
        public readonly double VehicleWeight;

        public CarModel(string make, string model, int vehicleType, double vehicleWeight)
        {
            Model = model;
            Make = make;
            VehicleType = vehicleType;
            VehicleWeight = vehicleWeight;
        }
    }
}

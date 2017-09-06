using TollApp.Models;

namespace TollApp.Events
{
    /// <summary>
    /// Static class containing pre defined car models and states
    /// </summary>
    public class LookupData
    {
        public static readonly CarModel[] CarModels =
        {
            new CarModel {Make = "Toyota", Model = "Camry", VehicleType = 1, VehicleWeight = 0},
            new CarModel {Make = "Ford", Model = "Taurus", VehicleType = 1, VehicleWeight = 0},
            new CarModel {Make = "Toyota", Model = "Corolla", VehicleType = 1, VehicleWeight = 0},
            new CarModel {Make = "Honda", Model = "CRV", VehicleType = 1, VehicleWeight = 0},
            new CarModel {Make = "Toyota", Model = "4x4", VehicleType = 1, VehicleWeight = 0},
            new CarModel {Make = "Honda", Model = "Accord", VehicleType = 1, VehicleWeight = 0},
            new CarModel {Make = "Ford", Model = "Mustang", VehicleType = 1, VehicleWeight = 0},
            new CarModel {Make = "Volvo", Model = "S80", VehicleType = 1, VehicleWeight = 0},
            new CarModel {Make = "Volvo", Model = "C30", VehicleType = 1, VehicleWeight = 0},
            new CarModel {Make = "Volvo", Model = "V70", VehicleType = 1, VehicleWeight = 0},
            new CarModel {Make = "Toyota", Model = "Rav4", VehicleType = 1, VehicleWeight = 0},
            new CarModel {Make = "Ford", Model = "Focus", VehicleType = 1, VehicleWeight = 0},
            new CarModel {Make = "Chevy", Model = "Malibu", VehicleType = 1, VehicleWeight = 0},
            new CarModel {Make = "Mac", Model = "Granite", VehicleType = 2, VehicleWeight = 2.710},
            new CarModel {Make = "Kenworth", Model = "T680", VehicleType = 2, VehicleWeight = 4.320},
            new CarModel {Make = "Peterbilt", Model = "389", VehicleType = 2, VehicleWeight = 2.675},
            new CarModel {Make = "Honda", Model = "Civic", VehicleType = 1, VehicleWeight = 0},
        };

        public static readonly string[] States =
        {
            "WA",
            "OR",
            "CA",
            "AL",
            "TX",
            "NJ",
            "CT",
            "PA"
        };
    }
}

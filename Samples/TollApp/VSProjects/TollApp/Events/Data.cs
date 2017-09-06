using TollApp.Models;

namespace TollApp.Events
{
    public class Data
    {
        public static readonly CarModel[] CarModels =
        {
            new CarModel("Toyota", "Camry", 1, 0),
            new CarModel("Ford", "Taurus", 1, 0),
            new CarModel("Toyota", "Corolla", 1, 0),
            new CarModel("Honda", "CRV", 1, 0),
            new CarModel("Toyota", "4x4", 1, 0),
            new CarModel("Honda", "Accord", 1, 0),
            new CarModel("Ford", "Mustang", 1, 0),
            new CarModel("Volvo", "S80", 1, 0),
            new CarModel("Volvo", "C30", 1, 0),
            new CarModel("Volvo", "V70", 1, 0),
            new CarModel("Toyota", "Rav4", 1, 0),
            new CarModel("Ford", "Focus", 1, 0),
            new CarModel("Chevy", "Malibu", 1, 0),
            new CarModel("Mac", "Granite", 2, 2.710),
            new CarModel("Kenworth", "T680", 2, 4.320),
            new CarModel("Peterbilt", "389", 2, 2.675),
            new CarModel("Honda", "Civic", 1, 0),
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

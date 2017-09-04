using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TollApp.Models
{
    /// <summary>
    /// The car registration number and license plate details
    /// </summary>
    public class Registration
    {
        public string LicensePlate { get; set; }
        public string RegistrationId { get; set; }
        public int Expired { get; set; }
    }

}

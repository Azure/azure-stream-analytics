using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TollApp.WebApp.Models
{
    public class DeploymentDetailsModel
    {
        public string SubscriptionId { get; set; }
        public string ResourceGroup { get; set; }
        public string JobName { get; set; }
        public string DeploymentLogLink { get; set; }
    }
}
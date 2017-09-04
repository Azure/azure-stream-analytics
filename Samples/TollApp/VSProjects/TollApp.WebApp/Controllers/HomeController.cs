﻿using System.Configuration;
using System.Web.Mvc;
using TollApp.WebApp.Models;

namespace TollApp.WebApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var deploymentDetailsModel = new DeploymentDetailsModel
            {
                DeploymentLogLink = ConfigurationManager.AppSettings["DeploymentLogLink"],
                JobName = ConfigurationManager.AppSettings["WebjobName"],
                ResourceGroup = ConfigurationManager.AppSettings["ResourceGroup"],
                SubscriptionId = ConfigurationManager.AppSettings["SubscriptionId"]
            };

            return View("Index", deploymentDetailsModel);
        }
    }
}
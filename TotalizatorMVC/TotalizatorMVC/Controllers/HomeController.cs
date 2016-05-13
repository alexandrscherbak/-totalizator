using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TotalizatorMVC.Models;
using TotalizatorMVC.BusinesModels;
using TotalizatorMVC.Predictors;
using System.Data;
using System.Data.Entity;
using TotalizatorMVC.RatesAndCoefficients;
using Microsoft.AspNet.Identity;

namespace TotalizatorMVC.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
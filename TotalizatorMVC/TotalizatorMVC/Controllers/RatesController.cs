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
    public class RatesController : Controller
    {
        RatesContext ratesDb = new RatesContext();

        [HttpGet]
        public ActionResult MakeRate(int matchId, int result, double coefficient)
        {
            Rate rate = new Rate();
            rate.MatchId = matchId;
            rate.Result = result;
            rate.Amount = 0m;
            rate.Coefficient = coefficient.ToString();

            return PartialView("MakeRate", rate);
        }

        [HttpPost]
        public ActionResult MakeRate(Rate rate)
        {
            rate.UserId = User.Identity.GetUserId();
            rate.Coefficient = double.Parse(rate.Coefficient).ToString("0.00");
            ratesDb.Rates.Add(rate);
            ratesDb.SaveChanges();

            return RedirectToAction("FutureEvents", "Home");
        }

        [HttpGet]
        public ActionResult RatesInfo()
        {
            string userId = User.Identity.GetUserId();
            List<Rate> rates = ratesDb.Rates.Where(r => String.Equals(r.UserId, userId)).ToList();
            List<MatchInfo> _matchesInfo = GamesController.getMatchesInfo();
            List<MatchInfo> _ratedMatchesInfo = new List<MatchInfo>();
            foreach (Rate rate in rates)
            {
                _ratedMatchesInfo.Add(_matchesInfo.Find(m => m.id == rate.MatchId));
            }

            ViewBag.Rates = rates;
            ViewBag.Matches = _ratedMatchesInfo;
            ViewBag.i = 0;
            ViewBag.ResultsEncoder = ResultsEncoder.results;
            return View();
        }
    }
}
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
    public class GamesController : Controller
    {
        private static GamesContext db = new GamesContext();
        private static List<MatchInfo> matchesInfo = new List<MatchInfo>();
        private static bool addFirstTeam;
        private static int? _firstTeamId;
        private static int? _secondTeamId;

        // Results
        public ActionResult Results()
        {
            ViewBag.ResultsEncoder = ResultsEncoder.results;
            try
            {
                ViewBag.Matches = getMatchesInfo().Where(m => m.realResult != 3).ToList();
            }
            catch (Exception)
            {

                ViewBag.Matches = new List<MatchInfo>();
            }
            return View();
        }

        // EditMatches
        public ActionResult EditMatches()
        {
            var matches = db.Matches.ToList();
            var teams = db.Teams.ToList();
            foreach (var match in matches)
            {
                match.FirstTeam = teams.Where(t => t.Id == match.FirstTeamId).ToList()[0];
                match.SecondTeam = teams.Where(t => t.Id == match.SecondTeamId).ToList()[0];
            }
            return View(matches);
        }

        // CreateMatch
        public ActionResult CreateMatch(bool? useTeamIds)
        {
            Match match = new Match();
            if (useTeamIds == null)
            {
                _firstTeamId = null;
                _secondTeamId = null;
            }
            match.FirstTeamId = _firstTeamId;
            match.SecondTeamId = _secondTeamId;
            return View("CreateMatch", match);
        }
        [HttpPost]
        public ActionResult CreateMatch(Match match)
        {
            match.FirstTeamId = _firstTeamId;
            match.SecondTeamId = _secondTeamId;
            db.Matches.Add(match);
            db.SaveChanges();
            return RedirectToAction("EditMatches");
        }

        //EditMatch
        public ActionResult EditMatch(int id)
        {
            Match match = db.Matches.Find(id);
            if (match != null)
            {
                return PartialView("EditMatch", match);
            }
            return RedirectToAction("EditMatches");
        }
        [HttpPost]
        public ActionResult EditMatch(Match match)
        {
            db.Entry(match).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("EditMatches");
        }

        // CreateTeam
        [HttpGet]
        public ActionResult CreateTeam(bool isFirstTeam)
        {
            addFirstTeam = isFirstTeam;
            return PartialView("CreateTeam");
        }
        [HttpPost]
        public ActionResult CreateTeam(Team team)
        {
            team.FuzzyInfo = FuzzyPredictor.analizeTeamInfo((new TeamInfo(team)).getListOfParameters());
            db.Teams.Add(team);
            db.SaveChanges();
            if (addFirstTeam)
            {
                _firstTeamId = team.Id;
                return RedirectToAction("CreateMatch", new { useTeamIds = true });
            }
            else
            {
                _secondTeamId = team.Id;
                return RedirectToAction("CreateMatch", new { useTeamIds = true });
            }
        }

        public static List<MatchInfo> getMatchesInfo()
        {
            if (matchesInfo.Count == 0)
            {
                var matches = db.Matches.ToList();
                var teams = db.Teams.ToList();
                List<Team> teamsToUpdate = new List<Team>();
                List<TeamInfo> teamesInfo = new List<TeamInfo>();

                foreach (var match in matches)
                {
                    Team firstTeamFromDb = teams.Where(t => t.Id == match.FirstTeamId).ToList()[0];
                    Team secondTeamFromDb = teams.Where(t => t.Id == match.SecondTeamId).ToList()[0];
                    match.FirstTeam = firstTeamFromDb;
                    match.SecondTeam = secondTeamFromDb;
                    TeamInfo firstTeam = new TeamInfo(match.FirstTeam);
                    TeamInfo secondTeam = new TeamInfo(match.SecondTeam);
                    if (firstTeam.fuzzyInfo[0] == '-')
                    {
                        firstTeam.fuzzyInfo = FuzzyPredictor.analizeTeamInfo(firstTeam.getListOfParameters());
                        firstTeamFromDb.FuzzyInfo = firstTeam.fuzzyInfo;
                        teamsToUpdate.Add(firstTeamFromDb);
                    }
                    if (secondTeam.fuzzyInfo[0] == '-')
                    {
                        secondTeam.fuzzyInfo = FuzzyPredictor.analizeTeamInfo(secondTeam.getListOfParameters());
                        secondTeamFromDb.FuzzyInfo = secondTeam.fuzzyInfo;
                        teamsToUpdate.Add(secondTeamFromDb);
                    }
                    MatchInfo matchInfo = new MatchInfo(match, firstTeam, secondTeam);
                    matchesInfo.Add(matchInfo);
                }
                if (teamsToUpdate.Count > 0)
                    updateTeamsTable(teamsToUpdate);
            }
            return matchesInfo;
        }

        private static void updateTeamsTable(List<Team> teamsToUpdate)
        {
            foreach (Team team in teamsToUpdate)
            {
                db.Entry(team).State = EntityState.Modified;
            }
            db.SaveChanges();
        }
    }
}
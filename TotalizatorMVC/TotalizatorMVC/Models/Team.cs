using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TotalizatorMVC.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Position { get; set; }
        public int Points { get; set; }
        public int Goals { get; set; }
        public int MissingBalls { get; set; }
        public int InjuredPlayers { get; set; }
        public int DaysOfRest { get; set; }
        public int ZeroGames { get; set; }
        public int GoalGames { get; set; }
        public int HomeMatch { get; set; }
        public string FuzzyInfo { get; set; }
    }
}
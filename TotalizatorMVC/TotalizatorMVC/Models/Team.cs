using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TotalizatorMVC.Models
{
    public class Team
    {
        public int Id { get; set; }
        [Display(Name = "Название")]
        public string Name { get; set; }
        [Display(Name = "Место в турнирной таблице")]
        public int Position { get; set; }
        [Display(Name = "Набрано очков за 5 матчей")]
        public int Points { get; set; }
        [Display(Name = "Забито голов за 5 матчей")]
        public int Goals { get; set; }
        [Display(Name = "Пропущено голов за 5 матчей")]
        public int MissingBalls { get; set; }
        [Display(Name = "Игроков пропускает матч")]
        public int InjuredPlayers { get; set; }
        [Display(Name = "Дней отдыха")]
        public int DaysOfRest { get; set; }
        [Display(Name = "Не пропускали голов подряд")]
        public int ZeroGames { get; set; }
        [Display(Name = "Забивали голы подряд")]
        public int GoalGames { get; set; }
        [Display(Name = "Домашний матч (1, 0)")]
        public int HomeMatch { get; set; }
        public string FuzzyInfo { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TotalizatorMVC.Models
{
    public class Match
    {
        public int Id { get; set; }
        [Display(Name = "Результат матча")]
        public string Result { get; set; }
        [Display(Name = "Id первой команды")]
        public int? FirstTeamId { get; set; }
        [Display(Name = "Id второй команды")]
        public int? SecondTeamId { get; set; }

        public Team FirstTeam { get; set; }
        public Team SecondTeam { get; set; }
    }
}
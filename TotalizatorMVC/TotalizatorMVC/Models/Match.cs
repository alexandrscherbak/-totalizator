using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TotalizatorMVC.Models
{
    public class Match
    {
        public int Id { get; set; }
        public string Result { get; set; }
        public int? FirstTeamId { get; set; }
        public int? SecondTeamId { get; set; }

        public Team FirstTeam { get; set; }
        public Team SecondTeam { get; set; }
    }
}
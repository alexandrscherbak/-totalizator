using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TotalizatorMVC.Models
{
    public class Rate
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int MatchId { get; set; }
        public int Result { get; set; }
        public decimal Amount { get; set; }
        public string Coefficient { get; set; }
    }
}
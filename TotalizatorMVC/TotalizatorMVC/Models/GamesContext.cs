using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace TotalizatorMVC.Models
{
    public class GamesContext : DbContext
    {
        public DbSet<Match> Matches { get; set; }
        public DbSet<Team> Teams { get; set; }
    }
}
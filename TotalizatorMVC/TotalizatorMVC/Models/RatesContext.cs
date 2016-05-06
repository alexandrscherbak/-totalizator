using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace TotalizatorMVC.Models
{
    public class RatesContext : DbContext
    {
        public DbSet<Rate> Rates { get; set; }
    }
}
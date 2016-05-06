using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace TotalizatorMVC.Models
{
    public class NeuralNetworkContext : DbContext
    {
        public DbSet<OutputLayerWeights> OutputLayerWeights { get; set; }
        public DbSet<TeachedRBF> TeachedRBFs { get; set; }
    }
}
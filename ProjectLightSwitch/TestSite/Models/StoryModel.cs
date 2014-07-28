using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace TestSite.Models
{
    public class StoryModel : DbContext
    {
        public StoryModel()
            : base("name=DefaultConnection")
        {

        }
        public DbSet<Survey> Survey { get; set; }
        public DbSet<StoryType> StoryType { get; set; }
    }
}
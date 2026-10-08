using Microsoft.EntityFrameworkCore;
using System.Configuration;
namespace EHUB.Utilities
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
        : base(options)
        {
        }

  
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    if (!optionsBuilder.IsConfigured)
        //    {
        //        optionsBuilder.UseSqlServer(System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionStr"].ToString());
        //    }
        //}
    }
	
}
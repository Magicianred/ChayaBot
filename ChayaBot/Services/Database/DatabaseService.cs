using ChayaBot.Services.Database.Fun;
using ChayaBot.Services.Database.Ranking;
using ChayaBot.Services.Database.Streams;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChayaBot.Services.Database
{
    public class DatabaseService
    {

        public DatabaseContext GetContext() => new DatabaseContext();

    }

    public class DatabaseContext : DbContext
    {

        // Properties
        // -> Ranking
        public DbSet<Rank> Ranks { get; set; }
        public DbSet<RankLevel> RankLevels { get; set; }
        public DbSet<Ranking.Ranking> Rankings { get; set; }
        // -> Fun
        public DbSet<Hug> Hugs { get; set; }
        // -> Streams
        public DbSet<StreamCategory> StreamCategories { get; set; }
        public DbSet<Stream> Streams { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=APOKAH\SQLEXPRESS;Initial Catalog=ChayaBot_DB;Integrated Security=True");

            base.OnConfiguring(optionsBuilder);
        }

        public List<Rank> GetRanks() => Ranks.Include(f => f.RankLevels).ToList();

        public List<Ranking.Ranking> GetRankings() => Rankings.Include(f => f.Rank).ThenInclude(f => f.RankLevels).ToList();

        public List<Stream> GetStreams() => Streams.Include(f => f.Category).ToList();

    }

}

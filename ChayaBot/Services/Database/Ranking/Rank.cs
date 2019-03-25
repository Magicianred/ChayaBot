using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChayaBot.Services.Database.Ranking
{
    public class Rank
    {

        // Properties
        [Key]
        public short Id { get; set; }
        public string Name { get; set; }
        public int ExperiencePerLevel { get; set; }
        public int TotalExperience { get; set; }
        public int TotalRequiredExperience { get; set; }
        public string Image { get; set; }

        public List<RankLevel> RankLevels { get; set; }
    
    }
}

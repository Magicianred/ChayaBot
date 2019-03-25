using System.ComponentModel.DataAnnotations;

namespace ChayaBot.Services.Database.Ranking
{
    public class RankLevel
    {

        // Properties
        [Key]
        public short Id { get; set; }
        public short Level { get; set; }
        public int RequiredExperience { get; set; }
        public int TotalRequiredExperience { get; set; }


        public short RankId { get; set; }
        //public Rank Rank { get; set; } TODO: Wait for lazy loading

    }
}

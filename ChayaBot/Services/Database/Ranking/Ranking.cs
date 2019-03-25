using System.ComponentModel.DataAnnotations;

namespace ChayaBot.Services.Database.Ranking
{
    public class Ranking
    {

        // Properties
        [Key]
        public int Id { get; set; }
        public long UserId { get; set; }
        public long GuildId { get; set; }
        public int CurrentExperience { get; set; }
        public short CurrentLevel { get; set; }


        public short RankId { get; set; }
        public Rank Rank { get; set; }

    }
}

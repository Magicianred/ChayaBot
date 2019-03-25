using Discord.Commands;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using ChayaBot.Services.Database;
using ChayaBot.Services.Database.Ranking;

namespace ChayaBot.Modules
{
    public class RankingModule : ModuleBase
    {

        // Fields
        private DatabaseContext database;


        // Constructor
        public RankingModule(DatabaseService databaseService)
        {
            database = databaseService.GetContext();
        }


        [Command("rank")]
        public async Task RankCommand()
        {
            await HandleRankCommand(Context.User as IGuildUser, true);
        }

        [Command("rank")]
        public async Task RankCommand(IGuildUser user)
        {
            await HandleRankCommand(user, user.Id == Context.Message.Author.Id);
        }

        private async Task HandleRankCommand(IGuildUser user, bool self = true)
        {
            Ranking ranking = database.GetRankings().FirstOrDefault(f => f.UserId == (long)user.Id && f.GuildId == (long)Context.Guild.Id);

            if (ranking == null)
            {
                string s = self ? "you aren't" : $"{user.Mention} isn't";
                await ReplyAsync($"{s} ranked yet. Talk, dont be shy!");
                return;
            }

            EmbedBuilder builder = new EmbedBuilder()
            {
                Color = new Color(0, 0, 0),
                Author = new EmbedAuthorBuilder()
                {
                    Name = user.Username,
                    IconUrl = user.GetAvatarUrl()
                },
                ThumbnailUrl = ranking.Rank.Image,
                Footer = new EmbedFooterBuilder() { Text = "ChayaBot" }
            };

            RankLevel nextLvl = ranking.Rank.RankLevels.FirstOrDefault(f => f.Level == ranking.CurrentLevel + 1);
            double expPercent = ((double)ranking.CurrentExperience / nextLvl.RequiredExperience) * 100;

            builder.AddInlineField("Level", ranking.CurrentLevel);
            builder.AddInlineField("Experience", string.Format("{0:N2}%", expPercent));

            await ReplyAsync("", false, builder.Build());
        }

    }
}

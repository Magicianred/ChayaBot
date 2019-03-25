using ChayaBot.Services.Database;
using ChayaBot.Services.Database.Ranking;
using Discord;
using Discord.WebSocket;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChayaBot.Services
{
    public class RankingService
    {

        // Fields
        private static readonly Regex codeblock = new Regex(@"`{3}(?:\S*$)((?:.*\n)*)`{3}", RegexOptions.Compiled | RegexOptions.Multiline);
        private DiscordSocketClient discord;
        private DatabaseContext database;


        // Constructor
        public RankingService(DiscordSocketClient discord, DatabaseService databaseService)
        {
            this.discord = discord;
            database = databaseService.GetContext();
        }


        public void Initialize()
        {
            discord.MessageReceived += async (msg) => await HandleMessage(msg, true);
            discord.MessageUpdated += async (c, msg, ch) => await HandleMessage(msg);
        }

        private async Task HandleMessage(SocketMessage msg, bool updated = false)
        {
            if (msg == null || msg.Author.IsBot)
                return;

            SocketGuildChannel channel = msg.Channel as SocketGuildChannel;

            // Get the user's ranking
            Ranking ranking = database.GetRankings().FirstOrDefault(f => f.UserId == (long)msg.Author.Id && f.GuildId == (long)channel.Guild.Id);

            // If the user is max rank/level
            if (ranking?.CurrentLevel == 15 && ranking?.RankId == 7)
                return;

            // If the user isn't ranked yet
            if (ranking == null)
            {
                ranking = new Ranking()
                {
                    UserId = (long)msg.Author.Id,
                    GuildId = (long)channel.Guild.Id,
                    RankId = 1,
                    CurrentExperience = 0,
                    CurrentLevel = 1
                };

                database.Rankings.Add(ranking);
                await database.SaveChangesAsync();
            }

            // Handle the exp
            Rank rank = ranking.Rank != null ? ranking.Rank : database.GetRanks().FirstOrDefault(f => f.Id == ranking.RankId);
            RankLevel nextLevel = rank.RankLevels.FirstOrDefault(f => f.Level == ranking.CurrentLevel + 1);

            int expNeededForNextLevel = nextLevel.RequiredExperience - ranking.CurrentExperience;
            int expWon = updated ? (int)(rank.ExperiencePerLevel * 0.4) : 
                rank.ExperiencePerLevel + (int)(rank.ExperiencePerLevel * (GetBonusExpPercent(msg) * 0.01));

            // Level up
            if (expWon >= expNeededForNextLevel)
            {
                ranking.CurrentLevel++;
                ranking.CurrentExperience = expWon - expNeededForNextLevel; // Exp left (if not 0)

                // Rank up
                if (ranking.CurrentLevel == 15 && ranking.RankId < 7)
                {
                    ranking.RankId++;
                    ranking.CurrentLevel = 1;
                    Rank newRank = database.GetRanks().FirstOrDefault(f => f.Id == ranking.RankId);
                    await msg.Channel.SendMessageAsync("", false, BuildEmbedBuilder("Rank Up!", $"Congrats {msg.Author.Mention}, you reached {newRank.Name}!",
                                                        msg.Author, newRank.Image));
                }
                else if (ranking.CurrentLevel < 15)
                {
                    await msg.Channel.SendMessageAsync("", false, BuildEmbedBuilder("Level Up!",
                                                        $"Congrats {msg.Author.Mention}, you reached level {ranking.CurrentLevel}!", msg.Author));
                }
            }
            else
            {
                ranking.CurrentExperience += expWon;
            }

            database.SaveChanges();
        }

        private Embed BuildEmbedBuilder(string title, string description, SocketUser author = null, string thumbnailUrl = null)
        {
            return new EmbedBuilder()
            {
                Title = title,
                Color = new Color(0, 0, 0),
                Description = description,
                ThumbnailUrl = thumbnailUrl,
                Author = author == null ? null : new EmbedAuthorBuilder()
                {
                    Name = author.Username,
                    IconUrl = author.GetAvatarUrl()
                },
                Footer = new EmbedFooterBuilder() { Text = "ChayaBot" }
            }.Build();
        }

        private int GetBonusExpPercent(SocketMessage msg)
        {
            int p = 0;

            if (msg.MentionedChannels.Count + msg.MentionedRoles.Count + msg.MentionedUsers.Count > 0)
                p += 5;

            if (codeblock.IsMatch(msg.Content))
                p += 12;

            if (msg.Content.StartsWith("`") && msg.Content.EndsWith("`"))
                p += 10;

            return p;
        }

    }
}

using ChayaBot.Services.Database;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChayaBot.Modules
{
    public class FunModule : ModuleBase
    {

        // Fields
        private DatabaseContext database;
        private Random random;


        // Constructor
        public FunModule(DatabaseService databaseService)
        {
            database = databaseService.GetContext();

            random = new Random();
        }


        [Command("hug")]
        public async Task HugCommand(IGuildUser user)
        {
            if (Context.User == user)
            {
                await ReplyAsync("Are you that lonely.. ?");
                return;
            }

            int r = random.Next(0, database.Hugs.Count());
            EmbedBuilder builder = new EmbedBuilder()
            {
                Description = $"{user}, here's a hug from {Context.User.Mention}",
                ImageUrl = database.Hugs.ToList()[r].ImageUrl
            };
            await ReplyAsync("", false, builder.Build());
        }

    }
}

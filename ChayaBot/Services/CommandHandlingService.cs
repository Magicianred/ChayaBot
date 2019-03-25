using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace ChayaBot.Services
{
    public class CommandHandlingService
    {
        private readonly DiscordSocketClient discord;
        private readonly CommandService commands;
        private IServiceProvider provider;

        public CommandHandlingService(DiscordSocketClient discord, CommandService commands)
        {
            this.discord = discord;
            this.commands = commands;

            discord.MessageReceived += MessageReceived;
        }

        public async Task InitializeAsync(IServiceProvider provider)
        {
            this.provider = provider;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task MessageReceived(SocketMessage rawMessage)
        {
            // Ignore system messages and messages from bots
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            // Ignore commands in DM channels
            if (message.Channel is IDMChannel) return;

            int argPos = 0;
            if (!message.HasStringPrefix("Chaya ", ref argPos) && !message.HasMentionPrefix(discord.CurrentUser, ref argPos)) return;

            var context = new SocketCommandContext(discord, message);
            var result = await commands.ExecuteAsync(context, argPos, provider);

            if ((result is ParseResult ||
                result is PreconditionResult ||
                result is ExecuteResult ||
                result is TypeReaderResult) &&
                !result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ToString());
        }
    }
}

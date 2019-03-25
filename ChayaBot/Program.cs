using ChayaBot.Services;
using ChayaBot.Services.Database;
using ChayaBot.Services.Games;
using ChayaBot.Services.Music;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace ChayaBot
{
    class Program
    {

        // Fields
        private DiscordSocketClient client;
        private IServiceProvider services;


        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            client = new DiscordSocketClient();

            services = CreateServices();

            // Initiate services
            services.GetRequiredService<LogService>();
            await services.GetRequiredService<CommandHandlingService>().InitializeAsync(services);
            services.GetRequiredService<RankingService>().Initialize();

            await client.LoginAsync(TokenType.Bot, Configuration.Token);
            await client.StartAsync();

            await Task.Delay(-1);
        }

        private IServiceProvider CreateServices()
        {
            return new ServiceCollection()
                // Base
                .AddSingleton(client)
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<DatabaseService>()
                .AddSingleton<InteractiveService>()
                .AddSingleton<RankingService>()
                .AddSingleton<MusicService>()
                .AddSingleton<TicTacToeService>()

                // Extra
                .AddLogging()
                .AddSingleton<LogService>()

                // Build
                .BuildServiceProvider();
        }

    }
}
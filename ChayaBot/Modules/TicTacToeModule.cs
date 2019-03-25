using ChayaBot.Core.Games;
using ChayaBot.Services;
using ChayaBot.Services.Games;
using Discord;
using Discord.Addons.EmojiTools;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ChayaBot.Modules
{
    public class TicTacToeModule : ModuleBase<SocketCommandContext>
    {

        // Fields
        private InteractiveService interactiveService;
        private TicTacToeService tttService;


        // Constructor
        public TicTacToeModule(InteractiveService interactiveService, TicTacToeService tttService)
        {
            this.interactiveService = interactiveService;
            this.tttService = tttService;
        }


        [Command("tictactoe duel", RunMode = RunMode.Async)]
        public async void duel(IGuildUser user)
        {
            if (user == Context.User)
            {
                await ReplyAsync("Are you that lonely?");
                return;
            }

            await ReplyAsync($"Hey {user.Mention}, {Context.User.Mention} is dueling you to a Tic Tac Toe match!\n" +
                "Answer 'yes' if you want to accept the duel and whatever else to refuse.");

            var msg = await interactiveService.WaitForMessageAsync(user, Context.Channel as IMessageChannel);
            if (msg.Content.ToLower() == "yes")
            {
                tttService.Start(Context.User.Id, user.Id);
                await ReplyAsync("Duel started !\n" +
                    "To play (if it's your turn), type 'Chaya tictactoe play <col> <line>'.");

                await PrintGrid();

                var currentPlayer = Context.Guild.GetUser(tttService.ticTacToe.CurrentPlayer.PlayerId);
                await ReplyAsync($"{currentPlayer.Mention}, it's your turn !");
            }
            else
            {
                await ReplyAsync("Such a pussy");
            }
        }

        [Command("tictactoe play", RunMode = RunMode.Async)]
        public async void play(byte line, byte col)
        {
            if (tttService.ticTacToe.CurrentPlayer.PlayerId != Context.User.Id)
            {
                await ReplyAsync("NOT YOUR TURN");
                return;
            }

            CellChangeTypeResult result = tttService.ticTacToe.ChangeCellType(line, col);
            switch (result)
            {
                case CellChangeTypeResult.ALREADY_FILLED:
                    await ReplyAsync("This cell is already filled, retry.");
                    break;
                case CellChangeTypeResult.INVALID_POSITION:
                    await ReplyAsync("Invalid column or line, retry.");
                    break;
                case CellChangeTypeResult.CHANGED:
                    await PrintGrid();

                    var currentPlayer = Context.Guild.GetUser(tttService.ticTacToe.CurrentPlayer.PlayerId);
                    await ReplyAsync($"{currentPlayer.Mention}, it's your turn !");
                    break;
                case CellChangeTypeResult.DRAW:
                    await ReplyAsync("DRAW !");
                    break;
                case CellChangeTypeResult.WON:
                    await ReplyAsync($"CONGRATS {Context.User.Mention}, YOU WON !");
                    break;
            }
        }

        private async Task PrintGrid()
        {
            CellTypes[,] grid = tttService.ticTacToe.GetGrid();

            Emoji GetEmoji(CellTypes type)
            {
                if (type == CellTypes.NONE)
                    return EmojiExtensions.FromText(":white_medium_square:");
                else if (type == CellTypes.CROSS)
                    return EmojiExtensions.FromText(":x:");
                else
                    return EmojiExtensions.FromText(":red_circle:");
            }

            StringBuilder sb = new StringBuilder();

            for (int x = 0; x < tttService.ticTacToe.Size; x++)
            {
                for (int y = 0; y < tttService.ticTacToe.Size; y++)
                {
                    sb.Append($"{GetEmoji(grid[x, y]).Name} ");
                }
                sb.AppendLine();
            }

            await ReplyAsync("", false, new EmbedBuilder()
            {
                Description = sb.ToString()
            });
        }

    }
}

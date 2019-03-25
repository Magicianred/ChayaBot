using Discord;
using Discord.WebSocket;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChayaBot.Services
{
    public class InteractiveService
    {

        // Fields
        private DiscordSocketClient client;


        // Constructor
        public InteractiveService(DiscordSocketClient client)
        {
            this.client = client;
        }


        public async Task<IUserMessage> WaitForMessageAsync(IUser user, IMessageChannel channel = null, int time = -1)
        {
            if (time < -1)
                time = -1;

            CancellationTokenSource cts = new CancellationTokenSource();
            IUserMessage response = null;

            Task checkIfValid(IMessage messageReceived)
            {
                var message = messageReceived as IUserMessage;
                if (!(message == null ||
                    message.Author.Id != user.Id ||
                    (channel != null && message.Channel != channel)))
                {
                    response = message;
                    cts.Cancel();
                }

                return Task.CompletedTask;
            }

            client.MessageReceived += checkIfValid;

            try
            {
                await Task.Delay(time, cts.Token);
            }
            catch (OperationCanceledException)
            {
                return response;
            }
            finally
            {
                client.MessageReceived -= checkIfValid;
            }

            return null;
        }

    }
}

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DiscordBot
{
    class Program
    {
        static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        private DiscordSocketConfig _config;

        //startup
        public async Task RunBotAsync()
        {
            _config = new DiscordSocketConfig() { MessageCacheSize = 100 };
            _client = new DiscordSocketClient(_config);
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            string token = "";

            _client.Log += _client_Log;

            await RegisterCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, token);

            await _client.StartAsync();

            await Task.Delay(-1);
        }

        //useless afaik but if I remove the code breaks
        private Task _client_Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        //commands ig
        public async Task RegisterCommandsAsync()
        {
            _client.MessageDeleted += DeLog;
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        //message recieved
        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);
            if (message.Author.IsBot) return;

            //getting the name of the channel
            var SChannel = message.Channel as SocketGuildChannel;
            var type = SChannel.GetChannelType();

            //filtering out logs and nsfw chats
            if (type != ChannelType.Text) return;
            if (SChannel.Name.ToLower() == "logs") return;
            if (SChannel.Name.ToLower() == "nsfw") return;

            //commands handler
            int argPos = 0;
            if (message.HasStringPrefix(".", ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);

                //command failed
                if (!result.IsSuccess)
                {
                    //basically useless, I never make errors, I would never
                    Console.WriteLine(result.ErrorReason);

                    //sending message and reaction for incorrect command
                    var m = await message.ReplyAsync("If this was meant to be a macro, this is not a valid macro, please type .macros.");

                    //countdown and deletion
                    await Task.Delay(10000);

                    await m.DeleteAsync();
                }
            }
        }

        //message deleted
        private async Task DeLog(Cacheable<IMessage, ulong> Message, Cacheable<IMessageChannel, ulong> Channel)
        {
            Color custom = new(0x00ffc8);

            //getting message info
            var message = await Message.GetOrDownloadAsync();

            //getting server and channel
            SocketGuildChannel channel = (SocketGuildChannel)message.Channel;

            var user = message.Author;

            //filtering out commands + channels
            if (message.Content.StartsWith(".")) return;
            if (channel.Name == "logs") return;
            if (channel.Name == "nsfw") return;
            if (user.IsBot) return;

            var des = $"<#{channel.Id}>: {message.Content}";

            var server = channel.Guild;

            var count = message.Attachments.Count;

            for (int i = 0; i < count; i++)
            {
                var attachment = message.Attachments.ElementAt(i);
                des += $"\n [{attachment}]({attachment.Url.Replace("cdn.discordapp.com", "media.discordapp.net")})";
            }

            var embed = new EmbedBuilder
            {
                Title = "Message deleted",
                Description = des,
            }
            .WithAuthor(user)
            .WithCurrentTimestamp()
            .WithColor(custom);

            var Log = server.TextChannels.SingleOrDefault(x => x.Name == "logs");

            await Log.SendMessageAsync(embed: embed.Build());
        }
    }
}
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using EightBallApiWrapper;

namespace DiscordBot.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {

        Color custom = new Color(0x00ffc8);

        //macros
        [Command("macros")]
        public async Task macros()
        {
            var icon = Context.Guild.IconUrl;

            var embed = new EmbedBuilder
            {
                Title = "Macros",
                Description = "creator\nvote\n8ball\ncry",
                Color = custom
            };
            embed.AddField("Admin Macros",
                "say\npurge\nupurge")
                .WithFooter("Current preset is .")
                .WithThumbnailUrl(icon);

            try
            {
                var dm = await Context.Message.Author.CreateDMChannelAsync();
                await dm.SendMessageAsync(embed: embed.Build());
            }
            catch
            {
                var m = await Context.Message.ReplyAsync(embed: embed.Build());
                await Task.Delay(10000);
                await m.DeleteAsync();
            }
            
            await Context.Message.DeleteAsync();
        }

        //creator
        [Command("creator")]
        public async Task creator()
        {
            await Context.Message.DeleteAsync();

            var embed = new EmbedBuilder
            {
                Title = "HelloHidnf",
                Description = "Hello, the creator of this bot is HelloHidnf, links for youtube and twitch are down below, don't expect many uploads"
            };

            embed.AddField("Socials",
                "[Youtube](https://www.youtube.com/channel/UChMrrmwVdde88CIV4ZH0_rw)\n[Twitch](https://www.twitch.tv/hellohidnf1)")
                .WithAuthor(Context.Client.GetUser(410643436044156938))
                .WithColor(custom);

            await ReplyAsync(embed: embed.Build());
        }

        //Voting command
        [Command("vote")]
        public async Task test([Remainder] string question)
        {
            Emoji check = new Emoji("✅");
            Emoji cross = new Emoji("❌");

            var embed = new EmbedBuilder
            {
                Title = question,
                Color = custom
            }.WithAuthor(Context.User).Build();

            var vote = await Context.Channel.SendMessageAsync(embed: embed);
            await vote.AddReactionAsync(check);
            await vote.AddReactionAsync(cross);
            await Context.Message.DeleteAsync();
        }

        //8ball
        [Command("8ball")]
        public async Task _8ball([Remainder] string question)
        {
            EightBall ball = new EightBall();
            var result = ball.AskQuestion(question);

            var embed = new EmbedBuilder
            {
                Title = question,
                Description = result.Answer,
                Color = custom
            }
            .WithAuthor(Context.User).Build();

            await ReplyAsync(embed: embed);

            await Context.Message.DeleteAsync();
        }

        //cry
        [Command("cry")]
        public async Task cry()
        {
            Emoji crying = new Emoji("😭");

            await Context.Message.AddReactionAsync(crying);

            await ReplyAsync($"{Context.Message.Author.Mention} made me cry.");
        }

        //say
        [Command("say")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task say([Remainder] string say)
        {
            await Context.Message.DeleteAsync();
            await ReplyAsync(say);
        }

        //purge
        [Command("purge")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task purge(int amount)
        {
            await Context.Message.DeleteAsync();

            var messages = await Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, amount).FlattenAsync();

            var filteredMessages = messages.Where(x => (DateTimeOffset.UtcNow - x.Timestamp).TotalDays <= 14);

            await (Context.Channel as ITextChannel).DeleteMessagesAsync(filteredMessages);
        }

        //user purge
        [Command("upurge")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task purge_user(int amount, SocketUser user)
        {
            await Context.Message.DeleteAsync();

            var messages = await Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, amount).FlattenAsync();

            var filteredMessages = messages.Where(x => (DateTimeOffset.UtcNow - x.Timestamp).TotalDays <= 14);

            filteredMessages = messages.Where(x => (x.Author.Id == user.Id));

            await (Context.Channel as ITextChannel).DeleteMessagesAsync(filteredMessages);
        }

        //sets playing ...
        [Command("game")]
        [RequireOwner]
        public async Task game([Remainder] string game)
        {
            await Context.Client.SetGameAsync(name: game, type: ActivityType.Playing);
            await Context.Message.DeleteAsync();
        }
    }
}
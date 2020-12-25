using Discord.Commands;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    public class Generic : ModuleBase<SocketCommandContext>
    {
        public Generic()
        {
        }

        [Command("ping")]
        [Summary("Check whether the bot is working or not.")]
        public async Task Ping()
        {
            await ReplyAsync("Pong!");
        }
    }
}

using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyCustomDiscordBot.Services;
using MyCustomDiscordBot.Settings;

namespace MyCustomDiscordBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var config = hostContext.Configuration;

                    //Add settings here
                    services
                        .Configure<BotSettings>(config.GetSection(nameof(BotSettings)))
                        .Configure<ChannelSettings>(config.GetSection(nameof(ChannelSettings)))
                        .Configure<RoleSettings>(config.GetSection(nameof(RoleSettings)))
                        .Configure<EmoteSettings>(config.GetSection(nameof(EmoteSettings)))
                        .Configure<GameSettings>(config.GetSection(nameof(GameSettings)));

                    //Add services here
                    services
                        .AddSingleton<DiscordSocketClient>()
                        .AddSingleton<BaseSocketClient, DiscordSocketClient>(sp =>
                        {
                            return sp.GetRequiredService<DiscordSocketClient>();
                        })
                        .AddSingleton<CommandHandler>()
                        .AddSingleton<CommandService>()
                        .AddSingleton<PugService>();

                    services.AddHostedService<Worker>();
                });
    }
}

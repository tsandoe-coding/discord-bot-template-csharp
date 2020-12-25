using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MyCustomDiscordBot.Settings;

namespace MyCustomDiscordBot
{
    public class Worker : BackgroundService
    {
        private readonly BaseSocketClient _client;
        private readonly BotSettings _botSettings;
        private readonly CommandHandler _commandHandler;

        public Worker(BaseSocketClient client, IOptions<BotSettings> botSettings, CommandHandler commandHandler)
        {
            _botSettings = botSettings.Value;
            _client = client;
            _commandHandler = commandHandler;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await _client.LoginAsync(TokenType.Bot, _botSettings.Token);
            await _client.StartAsync();
            await _commandHandler.Init();
            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }

    public class CommandHandler
    {
        private readonly BotSettings _botSettings;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _sp;

        public CommandHandler(IOptions<BotSettings> botSettings,
            IServiceProvider sp, DiscordSocketClient client,
            CommandService commandService)
        {
            _botSettings = botSettings.Value;
            _client = client;
            _commands = commandService;
            _sp = sp;
        }

        public async Task Init()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: _sp);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            if (!(messageParam is SocketUserMessage message)) return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasCharPrefix(_botSettings.Prefix, ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            var context = new SocketCommandContext(_client, message);

            // we dont do anything with the result, so we dont need to get it
            await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: _sp);

        }

        public class LoggingService
        {
            public LoggingService(BaseSocketClient client, CommandService command)
            {
                client.Log += LogAsync;
                command.Log += LogAsync;
            }

            private Task LogAsync(LogMessage message)
            {
                if (message.Exception is CommandException cmdException)
                {
                    Console.WriteLine($"[Command/{message.Severity}] {cmdException.Command.Aliases.First()}"
                        + $" failed to execute in {cmdException.Context.Channel}.");
                    Console.WriteLine(cmdException);
                }
                else
                    Console.WriteLine($"[General/{message.Severity}] {message}");

                return Task.CompletedTask;
            }
        }
    }
}

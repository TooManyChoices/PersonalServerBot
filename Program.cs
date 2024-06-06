using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Bot
{
    class Program
    {
        public static Random rng;
        public static string[] StartupArgs;
        private static DiscordSocketClient _client;

        public static async Task Main(string[] args)
        {
            StartupArgs = args;
            rng = new Random();
            Config.Init();
            Person.Init();
            Database.Init();

            DiscordSocketConfig socketconfig = new DiscordSocketConfig
            {
                LogLevel = (LogSeverity)(5 - Config.GetSetting<int>("ignore-log-severity", 0)),
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers
            };
            _client = new DiscordSocketClient(socketconfig);
            _client.Log += Log;
            _client.Ready += SlashCommandRegistrar.RegisterCommands;

            await _client.LoginAsync(TokenType.Bot, Config.GetSetting<string>("token"));
            await _client.StartAsync();

            PinReaction.Init();
            SubscribedMessages.Init();
            _client.SlashCommandExecuted += SlashCommandRegistrar.SlashCommandExecuted;
            _client.UserJoined += UserJoined;

            await Task.Delay(-1);
        }
        
        public static DiscordSocketClient GetClient() => _client;

        private static async Task UserJoined(SocketGuildUser user)
        {
            if (Database.ServerDataFromId(user.Guild.Id).linked_roles.member != 0)
                await user.AddRoleAsync(Database.ServerDataFromId(user.Guild.Id).linked_roles.member);

            if (user.Guild.GetWelcomeMessagesEnabled())
                await user.Guild.SystemChannel.SendMessageAsync(Person.GetRandomItem("user_joined"));
        }

        public static Task Log(LogMessage msg)
        {
            Console.WriteLine($"{msg.ToString()}");
            return Task.CompletedTask;
        }
    }
}
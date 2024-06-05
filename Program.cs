using System;
using System.Timers;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.VisualBasic;

namespace Bot
{
    class Program
    {
        public static Random rng;
        public static string[] StartupArgs;

        private static DiscordSocketClient _client;
        private static Timer timer;

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
            _client.Ready += SlashCommands.RegisterCommands;
            if (!Config.GetSetting<bool>("disable-ready-messages", false)) _client.Ready += ReadyMessage;

            await _client.LoginAsync(TokenType.Bot, Config.GetSetting<string>("token"));
            await _client.StartAsync();

            _client.SlashCommandExecuted += SlashCommands.SlashCommandExecuted;
            _client.UserJoined += UserJoined;
            _client.ReactionAdded += ReactionAdded;
            _client.ReactionRemoved += ReactionRemoved;
            timer = new Timer();
            DateTime nowTime = DateTime.Now;
            DateTime nextTime = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, 0, 0, 0, 0);
            if (nowTime > nextTime) nextTime = nextTime.AddDays(1);
            timer.Interval = (nextTime - nowTime).TotalMilliseconds;
            timer.Elapsed += InsaneMessage;
            timer.Start();

            await Task.Delay(-1);
        }
        
        public static DiscordSocketClient GetClient() => _client;

        private static async Task ReactionRemoved(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            if (reaction.Emote.Name == "pin")
            {
                IUserMessage realMessage = await message.GetOrDownloadAsync();
                if (!realMessage.Reactions.ContainsKey(reaction.Emote))
                {
                    await realMessage.UnpinAsync();
                }
            }
        }

        private static async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            IUserMessage realMessage = await message.GetOrDownloadAsync();
            if (reaction.Emote.Name == "pin")
            {
                await realMessage.PinAsync();
            }
        }

        private static async Task UserJoined(SocketGuildUser user)
        {
            if (Database.ServerDataFromId(user.Guild.Id).linked_roles.member != 0)
                await user.AddRoleAsync(Database.ServerDataFromId(user.Guild.Id).linked_roles.member);

            if (user.Guild.GetWelcomeMessagesEnabled())
                await user.Guild.SystemChannel.SendMessageAsync(Person.GetRandomItem("user_joined"));
        }

        private static async Task ReadyMessage()
        {
            string messageToSend = Person.GetRandomItem("on_connect");
            foreach (var server in Database.GetServersEnumerable())
            {
                if (server.Value.linked_channels.on_connect != 0)
                {
                    var channel = (IMessageChannel) _client.GetGuild((ulong)Convert.ToInt64(server.Key)).GetChannel((ulong)Convert.ToInt64(server.Value.linked_channels.on_connect));
                    await channel.SendMessageAsync(messageToSend);
                }
            }
        }

        private static async void InsaneMessage(object sender, ElapsedEventArgs e)
        {
            if (rng.NextSingle() > Config.GetSetting<double>("insane-rambling-chance", 100.0) / 100) return;

            string messageToSend = Person.GetRandomItem("insane_ramblings");
            foreach (var server in Database.GetServersEnumerable())
            {
                if (server.Value.linked_channels.insane_ramblings != 0)
                {
                    var channel = (IMessageChannel) _client.GetGuild((ulong)Convert.ToInt64(server.Key)).GetChannel((ulong)Convert.ToInt64(server.Value.linked_channels.insane_ramblings));
                    await channel.SendMessageAsync(messageToSend);
                }
            }

            DateTime nowTime = DateTime.Now;
            DateTime nextTime = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, 0, 0, 0, 0);
            if (nowTime > nextTime) nextTime = nextTime.AddDays(1);
            timer.Interval = (nextTime - nowTime).TotalMilliseconds;
        }

        public static Task Log(LogMessage msg)
        {
            Console.WriteLine($"{msg.ToString()}");
            return Task.CompletedTask;
        }
    }
}
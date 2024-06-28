using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Bot
{
    class Program
    {
        public static Random rng;
        public static string[] StartupArgs;
        private static DiscordSocketClient _client;

        public static CommandRegistrar<SocketSlashCommand> SlashCommands;
        public static CommandRegistrar<SocketUserCommand> UserCommands;
        public static CommandRegistrar<SocketMessageCommand> MessageCommands;

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
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.GuildMembers
            };
            _client = new DiscordSocketClient(socketconfig);
            _client.Log += Log;
            _client.Ready += RegisterCommands;

            SlashCommands = new();
            Bot.SlashCommands.Init();
            UserCommands = new();
            MessageCommands = new();

            await _client.LoginAsync(TokenType.Bot, Config.GetSetting<string>("token"));
            await _client.StartAsync();

            PinReaction.Init();
            ThreadReaction.Init();
            SubscribedMessages.Init();
            _client.SlashCommandExecuted += SlashCommands.CommandExecuted;
            _client.UserCommandExecuted += UserCommands.CommandExecuted;
            _client.MessageCommandExecuted += MessageCommands.CommandExecuted;
            _client.UserJoined += UserJoined;

            await Task.Delay(-1);
        }
        
        public static DiscordSocketClient GetClient() => 
            _client;

        public static string GetConfigPath() => 
            Program.StartupArgs.Length > 0 ? Program.StartupArgs[0] : System.Environment.GetEnvironmentVariable("BOT_CONFIG", EnvironmentVariableTarget.User);

        private static async Task UserJoined(SocketGuildUser user)
        {
            if (Database.ServerDataFromId(user.Guild.Id).linked_roles.member != 0)
                await user.AddRoleAsync(Database.ServerDataFromId(user.Guild.Id).linked_roles.member);

            if (user.Guild.GetWelcomeMessagesEnabled())
                await user.Guild.SystemChannel.SendMessageAsync(Person.GetRandomItem("user_joined", user.Mention));
        }

        public static Task Log(LogMessage msg)
        {
            Console.WriteLine($"{msg.ToString()}");
            return Task.CompletedTask;
        }

        public static async Task RegisterCommands()
        {
            var applicationCommandProperties = new ApplicationCommandProperties[] {
                new SlashCommandBuilder()
                    .WithName("set")
                    .WithDescription("Set some values, idfk.")
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("color")
                        .WithDescription("Set your color.")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("hex", ApplicationCommandOptionType.String, "hex", 
                            isRequired:true
                        )
                    )
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("title")
                        .WithDescription("Set your title.")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("title", ApplicationCommandOptionType.String, "title", 
                            isRequired:true
                        )
                    )
                    .Build(),
                new SlashCommandBuilder()
                    .WithName("git")
                    .WithDescription("Link to the GitHub repo for this bot.").Build(),
                new SlashCommandBuilder()
                    .WithName("level")
                    .WithDescription("View your level.")
                    .Build(),
                new SlashCommandBuilder()
                    .WithName("channel")
                    .WithDescription("Manage channel links.")
                    .WithDefaultMemberPermissions(GuildPermission.ManageGuild)
                    .AddOptions(
                        new SlashCommandOptionBuilder()
                            .WithName("onconnect")
                            .WithDescription("Receive message when the bot starts up.")
                            .WithType(ApplicationCommandOptionType.SubCommand),
                        new SlashCommandOptionBuilder()
                            .WithName("insaneramblings")
                            .WithDescription("Receive a message in the middle of the night.")
                            .WithType(ApplicationCommandOptionType.SubCommand)
                    )
                    .Build(),
                new SlashCommandBuilder()
                    .WithName("role")
                    .WithDescription("Manage role links.")
                    .WithDefaultMemberPermissions(GuildPermission.ManageGuild)
                    .AddOptions(
                        new SlashCommandOptionBuilder()
                            .WithName("member")
                            .WithDescription("Role that any non-bot user is assigned to upon joining.")
                            .WithType(ApplicationCommandOptionType.Role)
                    )
                    .Build(),
                new SlashCommandBuilder()
                    .WithName("purge")
                    .WithDescription("Delete a lot of messages.")
                    .WithDefaultMemberPermissions(GuildPermission.ManageMessages)
                    .AddOptions(
                        new SlashCommandOptionBuilder()
                            .WithName("the-last-x")
                            .WithDescription("Delete the last x messages")
                            .WithType(ApplicationCommandOptionType.Integer),
                        new SlashCommandOptionBuilder()
                            .WithName("until-x")
                            .WithDescription("Delete messages until reach x message ID/url")
                            .WithType(ApplicationCommandOptionType.String)
                        )
                    .Build()
            };

            try
            {
                await _client.BulkOverwriteGlobalApplicationCommandsAsync(applicationCommandProperties.ToArray());
            }
            catch(HttpException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
                Console.WriteLine(json);
            }
        }
    }
}
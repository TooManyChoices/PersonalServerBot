using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace MyDiscordBot
{
    class Program
    {
        private static DiscordSocketClient _client;
        public static Dictionary<string, dynamic> config;
        private static string _token;

        public static async Task Main(string[] args)
        {
            if (args.Length == 0) 
            {
                Console.WriteLine("Please specify location of config.json in executable arguments.");
                return;
            }
            // Get the config.json and the token
            config = JsonConvert.DeserializeObject<Dictionary<string,dynamic>>(File.ReadAllText(args[0]));
            _token = (string)config["token"];

            // Initialize client
            DiscordSocketConfig socketconfig = new DiscordSocketConfig
            {
                LogLevel = (LogSeverity)(5 - (int)config["ignore-log-severity"])
            };
            _client = new DiscordSocketClient(socketconfig);
            _client.Log += Log;
            _client.Ready += RegisterCommands;

            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();

            _client.SlashCommandExecuted += SlashCommandExecuted;

            await Task.Delay(-1);
        }

        public static Task Log(LogMessage msg)
        {
            Console.WriteLine($"{msg.ToString()}");
            return Task.CompletedTask;
        }

        public static async Task RegisterCommands()
        {
            List<ApplicationCommandProperties> applicationCommandProperties = new();
            var colorCommand = new SlashCommandBuilder()
                .WithName("set")
                .WithDescription("Set some values, idfk.")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("color")
                    .WithDescription("Set your color.")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption("hex", ApplicationCommandOptionType.String, "A hex code", 
                        isRequired:true
                    )
                )
                .WithDMPermission(false);
            applicationCommandProperties.Add(colorCommand.Build());

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

        public static async Task SlashCommandExecuted(SocketSlashCommand command)
        {
            switch (command.Data.Name)
            {
                case "set":
                    var role = await GetPersonalRoleAsync(command);
                    SocketGuild guild = _client.GetGuild(command.GuildId ?? 0);
                    if (role == null) break;
                    var subcommand = command.Data.Options.First().Name;
                    switch (subcommand)
                    {
                        case "color":
                            string value = (string)command.Data.Options.First().Options.First().Value;
                            if (value[0] != '#') value = '#'+value;
                            System.Drawing.Color newColor = ColorTranslator.FromHtml(value);
                            Discord.Color roleColor = new(newColor.R, newColor.G, newColor.B);
                            await role.ModifyAsync(x => {x.Color = roleColor;});
                            await command.RespondAsync("Updated your color!", ephemeral: true);
                            break;
                    }
                    break;
            }
        }

        public static async Task<IRole> GetPersonalRoleAsync(IGuild guild, IUser user)
        {
            string username = user.Username;
            
            IRole foundRole = guild.Roles.FirstOrDefault(x => x.Name == username);
            if (foundRole != null) return foundRole;
            
            var role = await guild.CreateRoleAsync(username, null, null, false, null);
            await (user as IGuildUser).AddRoleAsync(
                role.Id
            );
            return role;
        }
        public static async Task<IRole> GetPersonalRoleAsync(SocketGuildUser guildUser)
        {
            return await GetPersonalRoleAsync(guildUser.Guild, guildUser);
        }
        public static async Task<IRole> GetPersonalRoleAsync(SocketSlashCommand command)
        {
            return await GetPersonalRoleAsync(_client.GetGuild(command.GuildId ?? 0), command.User);
        }
    }
}
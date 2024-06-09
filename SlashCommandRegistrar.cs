using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Bot
{
    public class SlashCommandRegistrar
    {
        public static Dictionary<string, Func<SocketSlashCommand, Task>> commands = new();

        public static void RegisterCommandEvent(string key, Func<SocketSlashCommand, Task> listener)
        {
            commands.Add(key, (command)=>listener(command));
        }

        public static async Task RegisterCommands()
        {
            SlashCommands.Init();

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
                await Program.GetClient().BulkOverwriteGlobalApplicationCommandsAsync(applicationCommandProperties.ToArray());
            }
            catch(HttpException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
                Console.WriteLine(json);
            }
        }

        public static async Task SlashCommandExecuted(SocketSlashCommand command)
        {
            await commands[command.Data.Name](command);
        }
    }
}
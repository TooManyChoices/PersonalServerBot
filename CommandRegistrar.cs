using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;

public class CommandRegistrar<T>
{
    public Dictionary<string, Func<T, Task>> commands;

    public CommandRegistrar()
    {
        commands = new();
    }

    public void RegisterEvent(string commandName, Func<T, Task> listener)
    {
        commands.Add(commandName, (command)=>listener(command));
    }

    public async Task CommandExecuted(T command)
    {
        await commands[(command as SocketCommandBase).CommandName](command);
    }
}
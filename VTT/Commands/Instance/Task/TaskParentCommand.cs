using System;
using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;

namespace VTT.Commands.Instance.Task;

[Command("task")]
public class TaskParentCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        throw new CommandException("",showHelp:true);
    }
}

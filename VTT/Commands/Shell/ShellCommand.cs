using System;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace VTT.Commands.Shell;

[Command("shell")]
public class ShellCommand : ICommand
{
    [CommandParameter(0, Name = "Args", Description = "Run Vtt Terminal", IsRequired = true)]
    public ICollection<string> Args { get; set; } = default!;
    public ValueTask ExecuteAsync(IConsole console)
    {
        var _ = new Shell().RunAsync(Args.ToArray()).Result;
        return ValueTask.CompletedTask;
    }
}

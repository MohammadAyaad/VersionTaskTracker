using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesX.Extensions;

using static VTT.Program;


namespace VTT.Commands.Instance;

[Command("log")]
public class LogCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        return WhenInstanceReady(console,(c,instance) =>
        {
            if (instance.TasksDbContext.Tasks.Count() == 0) c.Output.WriteLine("No tasks added yet!");
            else c.Output.WriteLine(string.Join('\n', instance.TasksDbContext.Tasks.Map((t) => $"{t.Id.ToString()} | {t.Status.PadRight(10)} | {t.Label}")));
            return ValueTask.CompletedTask;
        });
    }
}

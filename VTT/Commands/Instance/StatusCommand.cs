using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesX;
using UtilitiesX.Extensions;
using VersionTaskTracker.Model.Tracking;
using static VTT.Program;

namespace VTT.Commands.Instance;

[Command("status")]
public class StatusCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        return WhenInstanceReady(console, (c,i) =>
        {
            c.Output.WriteLine("Version Task Tracker System is initialized.");
            c.Output.WriteLine(
            string.Join("\n",
            Program.Instance.TasksDbContext.GetUntracked(Program.Instance.WorkingDirectory,Program.Instance.VTTIgnore)
            .FlattenTransform(c => c.Children != null ? c.Children:new List<Component>(),c => c.Path.Branch(c=> c.Equals(Program.Instance.WorkingDirectory),onTrue: (c) => "\\",onFalse: (c) => c.Replace(Program.Instance.WorkingDirectory, "")))
            .Map(m => $"untracked: '{m}'")));
            return ValueTask.CompletedTask;
        });
    }
}

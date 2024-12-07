using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using GlobExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesX;
using UtilitiesX.Extensions;
using VersionTaskTracker.Model.Tracking;

namespace VTT.Commands.Instance;

[Command("track")]
public class TrackCommand : ICommand
{

    [CommandParameter(0, Name = "File", Description = "The target's path to track", IsRequired = true)]
    public string Path { get; set; } = "";
    public ValueTask ExecuteAsync(IConsole console)
    {
        Program.Instance!.TasksDbContext.AddRange(
        Program.Instance!.TasksDbContext.GetUntracked(Program.Instance.WorkingDirectory, Program.Instance.VTTIgnore)
            .FlattenTransform(c => c.Children != null ? c.Children : new List<Component>(), c => c)
            .Where(c => Glob.IsMatch(c.Path,Path)).Map(c =>
            {
                console.Output.WriteLine($"ADDED: {c.Path}");
                return c;
            }));

        Program.Instance.Save();

        return ValueTask.CompletedTask;
    }
}

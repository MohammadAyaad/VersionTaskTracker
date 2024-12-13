using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using UtilitiesX;
using UtilitiesX.Extensions;
using VersionTaskTracker.Model.Tracking;
using static VersionTaskTracker.Cli.Program;

namespace VersionTaskTracker.Cli.Commands.Instance;

[Command("status")]
public class StatusCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        return WhenInstanceReady(
            console,
            (c, instance) =>
            {
                c.Output.WriteLine("Version Task Tracker System is initialized.");
                c.WithForegroundColor(ConsoleColor.Red);
                c.Output.WriteLine(
                    string.Join(
                        "\n",
                        instance
                            .TasksDbContext.GetUntracked(
                                instance.WorkingDirectory,
                                instance.VTTIgnore
                            )
                            .Map(c => c.Path)
                            .Map(m => $"untracked: '{m}'")
                    )
                );
                c.ResetColor();
                return ValueTask.CompletedTask;
            }
        );
    }
}

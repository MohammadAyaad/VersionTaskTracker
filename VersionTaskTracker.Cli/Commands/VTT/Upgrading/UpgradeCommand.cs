using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace VersionTaskTracker.Cli.Commands.VTT.Upgrading;

[Command(
    "upgrade",
    Description = "Manages application upgrades, downgrades, and pre-release channels."
)]
public class UpgradeCommand : ICommand
{
    public UpgradeCommand() { }

    public ValueTask ExecuteAsync(IConsole console)
    {
        return ValueTask.CompletedTask;
    }
}

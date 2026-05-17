using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using VersionTaskTracker.Cli.Services;

namespace VersionTaskTracker.Cli.Commands.VTT.Upgrading;

[Command(
    "upgrade to",
    Description = "Manages application upgrades, downgrades, and pre-release channels."
)]
public class UpgradeToCommand : ICommand
{
    [CommandParameter(0, Name = "version", Description = "Target version", IsRequired = true)]
    public string Version { get; set; } = "";

    public UpgradeToCommand() { }

    public ValueTask ExecuteAsync(IConsole console)
    {
        var output = console.Output;
        output.WriteLine($"Preparing to upgrade to {Version}");
        string vFileName = UpgradeService.GetVersionFileName(Version).Result;
        output.WriteLine($"File Found in repository '{vFileName}'");
        output.WriteLine($"Downloading '{vFileName}' From repository...");
        string fileName = UpgradeService.DownloadVersion(Version, vFileName).Result;
        output.WriteLine($"File Downloaded!");
        output.WriteLine($"Preparing for installation...");
        UpgradeService.CopySelfToTemp().Wait();
        var _ = UpgradeService.RunTempVersion();
        return ValueTask.CompletedTask;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using VersionTaskTracker.Cli.Services;

namespace VersionTaskTracker.Cli.Commands.VTT.Upgrading.UpgradeChannel;

[Command("upgrade latest", Description = "Upgrades to the latest version in the release channel.")]
public class UpgradeLatestCommand : ICommand
{
    public UpgradeLatestCommand() { }

    public ValueTask ExecuteAsync(IConsole console)
    {
        var output = console.Output;
        output.WriteLine("determining the latest version...");
        var Version = UpgradeService
            .GetAvailableVersions()
            .Result.Where(v => Regex.IsMatch(v, @"^v\d+\.\d+\.\d+$"))
            .OrderDescending()
            .First();
        output.WriteLine($"latest version found to be '{Version}'");
        output.Write("Do you wish to proceed with the installation ? [y/n]");
        ConsoleKeyInfo key = output.Console.ReadKey(true);
        if (key.KeyChar.ToString().ToLower() != "y")
        {
            output.WriteLine("update canceled!");
            return ValueTask.CompletedTask;
        }
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

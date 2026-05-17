using System.Reflection;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using VersionTaskTracker.Cli.Services;

namespace VersionTaskTracker.Cli.Commands.VTT.Upgrading;

[Command("upgrade continue", Description = "continues the updating process.")]
public class UpgradeContinueCommand : ICommand
{
    public UpgradeContinueCommand() { }

    public ValueTask ExecuteAsync(IConsole console)
    {
        var output = console.Output;
        output.WriteLine("Waiting for Process to exit...");
        UpgradeService.RemoveSelfOldFromTemp().Wait();
        output.WriteLine("Successfully cleaned all installation.");
        output.WriteLine("Preparing new version for installation...");
        UpgradeService.CopyNewCompressedVersion().Wait();
        output.WriteLine("Installing new Version...");
        UpgradeService.ExtractNewVersion().Wait();
        var _ = UpgradeService.RunNewVersion();
        return default;
    }
}

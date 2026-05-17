using System.Reflection;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using VersionTaskTracker.Cli.Services;

namespace VersionTaskTracker.Cli.Commands.VTT.Upgrading;

[Command("upgrade clean", Description = "continues the updating process.")]
public class UpgradeCleanCommand : ICommand
{
    public UpgradeCleanCommand() { }

    public ValueTask ExecuteAsync(IConsole console)
    {
        var output = console.Output;
        output.WriteLine("Successfully installed the new version!");
        output.WriteLine("Cleaning up...");
        UpgradeService.CleanOldVersion().Wait();
        output.WriteLine("Ready!");

        return default;
    }
}

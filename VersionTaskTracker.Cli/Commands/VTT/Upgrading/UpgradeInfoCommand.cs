using System.Reflection;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using VersionTaskTracker.Cli.Services;

namespace VersionTaskTracker.Cli.Commands.VTT.Upgrading;

[Command(
    "upgrade info",
    Description = "Manages application upgrades, downgrades, and pre-release channels."
)]
public class UpgradeInfoCommand : ICommand
{
    public UpgradeInfoCommand() { }

    public ValueTask ExecuteAsync(IConsole console)
    {
        var output = console.Output;
        Version entryVersion = Assembly.GetEntryAssembly()?.GetName().Version!;
        output.WriteLine($"current: VTT v{entryVersion}");
        output.WriteLine($"checking for available versions...");
        output.WriteLine(
            $"Available versions:\n{string.Join("\n", UpgradeService.GetAvailableVersions().Result)}"
        );
        return ValueTask.CompletedTask;
    }
}

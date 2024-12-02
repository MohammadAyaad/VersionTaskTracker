using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VTT.Commands.VTT;

[Command("version")]
public class VersionCommand : ICommand
{
    [CommandOption("onlyversion", 'v', IsRequired = false, Description = "Print only the version number")]
    public bool OnlyVersion { get; set; } = false;
    public ValueTask ExecuteAsync(IConsole console)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        console.Output.WriteLine(OnlyVersion ? $"{version}" :
        $"{assembly.GetName().FullName} Version {version}");
        return default;
    }
}

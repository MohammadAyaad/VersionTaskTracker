using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VersionTaskTracker.Model.Tracker;
using VersionTaskTracker.Model.Tracking;
using VersionTaskTracker.Services;

using static VTT.Program;

namespace VTT.Commands.Instance;

[Command("init")]
public class InitializeInstanceCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine($"initializing directory...");
        IDirectoryInfo currentPath = new FileSystem().DirectoryInfo.New(Directory.GetCurrentDirectory());
        console.Output.WriteLine($"CURRENT PATH : \'{currentPath}\'");

        return WhenInstanceReady(console, (c, i) =>
        {
            console.Output.WriteLine("Instance already exists");
            return ValueTask.CompletedTask;
        },
        (c) =>
        {
            Program.Instance = new VTTInstance(currentPath,Program.Environment.Config);
            if (Program.Instance.InstanceExists()) console.Output.WriteLine("Instance already exists");
            else
            {
                console.Output.WriteLine("Creating instance");
                Program.Instance.Save();
                console.Output.WriteLine("Instance Created!");
            }
            return ValueTask.CompletedTask;
        });
    }
}

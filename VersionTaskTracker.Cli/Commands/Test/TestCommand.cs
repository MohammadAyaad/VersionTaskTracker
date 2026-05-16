using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Newtonsoft.Json.Linq;
using VersionTaskTracker.Model.Tracker;

namespace VersionTaskTracker.Cli.Commands.Test
{
    [Command("test")]
    public class TestCommand : ICommand
    {
        public ValueTask ExecuteAsync(IConsole console)
        {
            console.Output.WriteLine("No Test to perform");
            return ValueTask.CompletedTask;
        }
    }
}

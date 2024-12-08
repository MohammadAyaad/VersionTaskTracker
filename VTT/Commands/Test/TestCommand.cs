using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VersionTaskTracker.Model.Tracker;

namespace VTT.Commands.Test
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

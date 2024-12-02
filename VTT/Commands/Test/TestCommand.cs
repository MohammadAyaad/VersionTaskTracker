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
            console.Output.WriteLine($"initializing directory...");
            string currentPath = Directory.GetCurrentDirectory();
            console.Output.WriteLine($"CURRENT PATH : \'{currentPath}\'");

            Program.Instance.Metadata.RootComponentId = Program.Instance.TasksDbContext.FromDirectory(currentPath,
                new List<string>()
                {
                "node_modules/",
                ".git/",
                "src/res/",
                ".angular/",
                ".github/",
                ".vs/",
                ".vscode/",
                "dist/",
                ".gitignore",
                ".prettierignore",
                });

            Program.Instance.Save();

            console.Output.WriteLine("RESULT OBJECT : ");
            console.Output.WriteLine(JObject.FromObject(Program.Instance.TasksDbContext.Components.First(c => c.Id.Equals(Program.Instance.Metadata.RootComponentId))).ToString());




            return ValueTask.CompletedTask;
        }
    }
}

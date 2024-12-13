using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Microsoft.EntityFrameworkCore;
using static VersionTaskTracker.Cli.Program;

namespace VersionTaskTracker.Cli.Commands.Instance;

[Command("add")]
public class AddCommand : ICommand
{
    [CommandParameter(0, Name = "Label", Description = "The Task's Label", IsRequired = true)]
    public string Label { get; set; } = "";

    public ValueTask ExecuteAsync(IConsole console)
    {
        return WhenInstanceReady(
            console,
            (c, instance) =>
            {
                c.Output.WriteLine($"Creating Task '{Label}'");
                Guid id = Guid.NewGuid();
                instance.TasksDbContext.Tasks.Add(
                    new VersionTaskTracker.Model.Tracking.Task()
                    {
                        Id = id,
                        Label = Label,
                        Description = "UNASSIGNED",
                        Status = "TODO",
                        Int_Id = 0,
                        ParentComponent = null,
                    }
                );
                instance.TasksDbContext.SaveChanges();
                c.Output.WriteLine($"Task {id.ToString()} Created!");
                return ValueTask.CompletedTask;
            }
        );
    }
}

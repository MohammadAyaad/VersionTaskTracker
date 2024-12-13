using System;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using VersionTaskTracker.Model.Tracking;
using static VersionTaskTracker.Cli.Program;

namespace VersionTaskTracker.Cli.Commands.Instance.Task;

[Command("task create")]
public class CreateTaskCommand : ICommand
{
    [CommandParameter(
        0,
        Name = "Target",
        Description = "The Target Component's Path",
        IsRequired = true
    )]
    public string TargetPath { get; set; } = "";

    [CommandParameter(1, Name = "Label", Description = "The Task's Label", IsRequired = true)]
    public string Label { get; set; } = "";

    [CommandParameter(2, Name = "Status", Description = "Task's Status", IsRequired = true)]
    public string Status { get; set; } = "";

    [CommandParameter(
        3,
        Name = "Description",
        Description = "Task's Description",
        IsRequired = false
    )]
    public string Description { get; set; } = "";

    public ValueTask ExecuteAsync(IConsole console)
    {
        return WhenInstanceReady(
            console,
            (c, instance) =>
            {
                c.Output.WriteLine($"Creating Task '{Label}'");
                Guid id = Guid.NewGuid();

                Component? target = instance.TasksDbContext.Components.FirstOrDefault(x =>
                    x.Path.Equals(TargetPath)
                );

                if (target == null)
                {
                    console.Output.WriteLine($"Component '{TargetPath}' Doesn't exist");
                    return ValueTask.CompletedTask;
                }
                var maxIntId =
                    instance
                        .TasksDbContext.Tasks.OrderByDescending(t => t.Int_Id)
                        .FirstOrDefault()
                        ?.Int_Id ?? 0;

                instance.TasksDbContext.Tasks.Add(
                    new VersionTaskTracker.Model.Tracking.Task()
                    {
                        Id = id,
                        Label = Label,
                        Description = Description,
                        Status = Status,
                        Int_Id = maxIntId + 1,
                        ParentComponent = target,
                    }
                );

                instance.TasksDbContext.SaveChanges();

                VersionTaskTracker.Model.Tracking.Task task =
                    instance.TasksDbContext.Tasks.FirstOrDefault(x => x.Id.Equals(id))!;

                c.Output.WriteLine($"Task {id.ToString()} | #{task.Int_Id} Created!");
                return ValueTask.CompletedTask;
            }
        );
    }
}

using System;
using System.Text.RegularExpressions;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using static VersionTaskTracker.Cli.Program;

namespace VersionTaskTracker.Cli.Commands.Instance.Task.Update;

[Command("task update description")]
public class UpdateTaskDescriptionCommand : ICommand
{
    [CommandParameter(
        0,
        Name = "Task id",
        Description = "the task identifier either Guid or Task number",
        IsRequired = true
    )]
    public string Id { get; set; } = "";

    [CommandParameter(
        1,
        Name = "New description",
        Description = "the task's new description",
        IsRequired = true
    )]
    public string Description { get; set; } = "";

    public ValueTask ExecuteAsync(IConsole console)
    {
        return WhenInstanceReady(
            console,
            (console, instance) =>
            {
                Guid guid;
                if (Guid.TryParse(Id, out guid))
                {
                    VersionTaskTracker.Model.Tracking.Task? task =
                        instance.TasksDbContext.Tasks.FirstOrDefault(t => t.Id.Equals(guid));
                    if (task == null)
                    {
                        console.Output.WriteLine("could not find the specified task!");
                        return ValueTask.CompletedTask;
                    }
                    string oldDescription = task.Description;
                    task.Description = Description;
                    instance.TasksDbContext.Tasks.Update(task);
                    instance.Save();
                    console.Output.WriteLine("Task's Description Updated!");
                    console.Output.WriteLine(
                        $"{task.Label} {task.Status}\n{oldDescription} => {task.Label} {task.Status}\n{task.Description}"
                    );
                    return ValueTask.CompletedTask;
                }
                else if (Regex.IsMatch(Id, @"^#[0-9]*$") || Regex.IsMatch(Id, @"^[0-9]*$"))
                {
                    VersionTaskTracker.Model.Tracking.Task? task =
                        instance.TasksDbContext.Tasks.FirstOrDefault(t =>
                            t.Int_Id.Equals(Convert.ToInt64(Id.Replace("#", "")))
                        );
                    if (task == null)
                    {
                        console.Output.WriteLine("could not find the specified task!");
                        return ValueTask.CompletedTask;
                    }
                    string oldDescription = task.Description;
                    task.Description = Description;
                    instance.TasksDbContext.Tasks.Update(task);
                    instance.Save();
                    console.Output.WriteLine("Task's Description Updated!");
                    console.Output.WriteLine(
                        $"{task.Label} {task.Status}\n{oldDescription} => {task.Label} {task.Status}\n{task.Description}"
                    );
                    return ValueTask.CompletedTask;
                }
                else
                {
                    console.Output.WriteLine(
                        "Could not identify the id entered to be of a valid type"
                    );
                }

                return ValueTask.CompletedTask;
            }
        );
    }
}

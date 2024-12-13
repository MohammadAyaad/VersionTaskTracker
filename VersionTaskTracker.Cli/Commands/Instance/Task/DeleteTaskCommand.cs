using System;
using System.Text.RegularExpressions;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using UtilitiesX.Extensions;
using static VersionTaskTracker.Cli.Program;

namespace VersionTaskTracker.Cli.Commands.Instance.Task;

[Command("task delete")]
public class DeleteTaskCommand : ICommand
{
    [CommandParameter(
        0,
        Name = "Task id",
        Description = "the task identifier either Guid or Task number",
        IsRequired = true
    )]
    public string Id { get; set; } = "";

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
                        console.Output.WriteLine("Could not find the task with the Guid entered!");
                    else
                    {
                        instance.TasksDbContext.Tasks.Remove(task);
                        instance.Save();
                        console.Output.WriteLine($"Task :\n{task.ToString()}");
                        console.Output.WriteLine("Task deleted successfully!");
                    }
                    return ValueTask.CompletedTask;
                }
                else if (Regex.IsMatch(Id, @"^#[0-9]*$") || Regex.IsMatch(Id, @"^[0-9]*$"))
                {
                    VersionTaskTracker.Model.Tracking.Task? task =
                        instance.TasksDbContext.Tasks.FirstOrDefault(t =>
                            t.Int_Id.Equals(Convert.ToInt64(Id.Replace("#", "")))
                        );
                    if (task == null)
                        console.Output.WriteLine(
                            "Could not find the task with the Task number entered!"
                        );
                    else
                    {
                        instance.TasksDbContext.Tasks.Remove(task);
                        instance.Save();
                        console.Output.WriteLine($"Task :\n{task.ToString()}");
                        console.Output.WriteLine("Task deleted successfully!");
                    }
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

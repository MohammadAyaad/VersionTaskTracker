using System;
using System.Text.RegularExpressions;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using UtilitiesX.Extensions;
using VersionTaskTracker.Model.Tracking;
using static VersionTaskTracker.Cli.Program;

namespace VersionTaskTracker.Cli.Commands.Instance.Task;

[Command("task read")]
public class ReadTaskCommand : ICommand
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
                    string str = "";
                    if (task != null)
                        str = task.ToString();
                    else
                        str = "Task Guid not found!";
                    console.Output.WriteLine(str);
                    return ValueTask.CompletedTask;
                }
                else if (Regex.IsMatch(Id, @"^#[0-9]*$") || Regex.IsMatch(Id, @"^[0-9]*$"))
                {
                    VersionTaskTracker.Model.Tracking.Task? task =
                        instance.TasksDbContext.Tasks.FirstOrDefault(t =>
                            t.Int_Id.Equals(Convert.ToInt64(Id.Replace("#", "")))
                        );
                    string str = "";
                    if (task != null)
                        str = task.ToString();
                    else
                        str = "Task Int_Id not found!";
                    console.Output.WriteLine(str);
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

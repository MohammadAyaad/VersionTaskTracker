using System;
using System.Text.RegularExpressions;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using static VTT.Program;

namespace VTT.Commands.Instance.Task.Update;

[Command("task update label")]
public class UpdateTaskLabelCommand : ICommand
{
    [CommandParameter(0,Name = "Task id",Description = "the task identifier either Guid or Task number",IsRequired = true)]
    public string Id { get; set; } = "";

    [CommandParameter(1,Name = "New Label",Description = "the task's new label",IsRequired = true)]
    public string Label { get; set; } = "";
    public ValueTask ExecuteAsync(IConsole console)
    {
        return WhenInstanceReady(console,
        (console,instance) => {
            Guid guid;
            if(Guid.TryParse(Id, out guid)) {
                VersionTaskTracker.Model.Tracking.Task? task = instance.TasksDbContext.Tasks.FirstOrDefault(t => t.Id.Equals(guid));
                if(task == null) {
                    console.Output.WriteLine("could not find the specified task!");
                    return ValueTask.CompletedTask;
                }
                string oldLabel = task.Label;
                task.Label = Label;
                instance.TasksDbContext.Tasks.Update(task);
                instance.Save();
                console.Output.WriteLine("Task's Label Updated!");
                console.Output.WriteLine($"{oldLabel} {task.Status} => {task.Label} {task.Status}");
                return ValueTask.CompletedTask;
            }
            else if(Regex.IsMatch(Id,@"^#[0-9]*$") || Regex.IsMatch(Id,@"^[0-9]*$")) {
                VersionTaskTracker.Model.Tracking.Task? task = instance.TasksDbContext.Tasks.FirstOrDefault(t => t.Int_Id.Equals(Convert.ToInt64(Id.Replace("#",""))));
                if(task == null) {
                    console.Output.WriteLine("could not find the specified task!");
                    return ValueTask.CompletedTask;
                }
                string oldLabel = task.Label;
                task.Label = Label;
                instance.TasksDbContext.Tasks.Update(task);
                instance.Save();
                console.Output.WriteLine("Task's Label Updated!");
                console.Output.WriteLine($"{oldLabel} {task.Status} => {task.Label} {task.Status}");
                return ValueTask.CompletedTask;
            }
            else {
                console.Output.WriteLine("Could not identify the id entered to be of a valid type");
            }

            return ValueTask.CompletedTask;

        });
    }
}

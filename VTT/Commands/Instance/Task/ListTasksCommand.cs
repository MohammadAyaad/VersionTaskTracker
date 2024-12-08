using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesX;
using UtilitiesX.Extensions;
using VersionTaskTracker.Model.Tracking;
using static VTT.Program;


namespace VTT.Commands.Instance.Task;

[Command("task list")]
public class ListTasksCommand : ICommand
{
    [CommandParameter(0, Name = "Target", Description = "The Target Component's Path", IsRequired = false)]
    public string TargetPath { get; set; } = "";
    public ValueTask ExecuteAsync(IConsole console)
    {
        return WhenInstanceReady(console,(c,instance) =>
        {
            if (instance.TasksDbContext.Tasks.Count() == 0) c.Output.WriteLine("No tasks added yet!");
            else c.Output.WriteLine(string.Join('\n', instance.TasksDbContext.Tasks.Branch<IQueryable<VersionTaskTracker.Model.Tracking.Task>>(t => TargetPath.Trim() != "",onTrue: t => t.Where(x => x.ParentComponent.Path.Equals(TargetPath)),onFalse: f => f).Map((t) => $"[{t.Id.ToString()}|#{t.Int_Id.ToString()}] {t.Status.PadRight(10)} | {t.Label.PadRight(25)} | {t.ParentComponent.Path}")));
            return ValueTask.CompletedTask;
        });
    }
}

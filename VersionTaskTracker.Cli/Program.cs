using System.IO.Abstractions;
using CliFx;
using CliFx.Infrastructure;
using VersionTaskTracker.Cli.Commands.Instance;
using VersionTaskTracker.Cli.Commands.Instance.Task;
using VersionTaskTracker.Cli.Commands.Instance.Task.Update;
using VersionTaskTracker.Cli.Commands.Test;
using VersionTaskTracker.Cli.Commands.VTT;
using VersionTaskTracker.Services;

namespace VersionTaskTracker.Cli;

public static class Program
{
    public static readonly string ENVIRONMENT_PATH = AppContext.BaseDirectory;

    public static VTTEnvironment Environment = VTTEnvironment.Setup(ENVIRONMENT_PATH);
    public static VTTInstance? Instance;

    public static async Task Main(string[] args)
    {
        Instance = new VTTInstance(
            new FileSystem().DirectoryInfo.New(Directory.GetCurrentDirectory()),
            Environment.Config
        );

        await new CliApplicationBuilder()
            .AddCommand<VersionCommand>()
            .AddCommand<TestCommand>()
            .AddCommand<InitializeInstanceCommand>()
            .AddCommand<StatusCommand>()
            .AddCommand<TrackCommand>()
            .AddCommand<TaskParentCommand>()
            .AddCommand<CreateTaskCommand>()
            .AddCommand<ReadTaskCommand>()
            .AddCommand<ListTasksCommand>()
            .AddCommand<UpdateTaskCommand>()
            .AddCommand<UpdateTaskStatusCommand>()
            .AddCommand<UpdateTaskLabelCommand>()
            .AddCommand<UpdateTaskDescriptionCommand>()
            .AddCommand<DeleteTaskCommand>()
            .SetExecutableName("vtt")
            .Build()
            .RunAsync(args);
    }

    private static ValueTask InstanceNotReady(IConsole console)
    {
        console.Output.WriteLine("Failed to read instance");
        return ValueTask.CompletedTask;
    }

    public static ValueTask WhenInstanceReady(
        IConsole console,
        Func<IConsole, VTTInstance, ValueTask> f,
        Func<IConsole, ValueTask>? notReady = null
    )
    {
        notReady ??= InstanceNotReady;
        if (Instance == null || !Instance.InstanceExists())
        {
            return notReady(console);
        }
        else
        {
            return f(console, Instance);
        }
    }
}

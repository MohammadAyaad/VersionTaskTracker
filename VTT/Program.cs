using System.Reflection;
using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using CliFx;
using CliFx.Infrastructure;
using PathLib;
using UtilitiesX;
using UtilitiesX.Extensions;
using VersionTaskTracker.Services;
using VTT.Commands.Instance;
using VTT.Commands.Instance.Task;
using VTT.Commands.Instance.Task.Update;
using VTT.Commands.Test;
using VTT.Commands.VTT;

namespace VTT;

public static class Program
{
    public static readonly string ENVIRONMENT_PATH = Path.GetDirectoryName(
        Assembly.GetExecutingAssembly().Location
    );

    public static VTTEnvironment Environment;
    public static VTTInstance? Instance;

    public static async Task Main(string[] args)
    {
        Environment = VTTEnvironment.Setup(ENVIRONMENT_PATH);
        Instance = new VTTInstance(new FileSystem().DirectoryInfo.New(Directory.GetCurrentDirectory()), Environment.Config);

        await new CliApplicationBuilder()
            .AddCommand<VersionCommand>()
            .AddCommand<TestCommand>()
            .AddCommand<InitializeInstanceCommand>()
            .AddCommand<LogCommand>()
            .AddCommand<StatusCommand>()
            .AddCommand<TrackCommand>()
            .AddCommand<TaskParentCommand>()
            .AddCommand<CreateTaskCommand>()
            .AddCommand<ReadTaskCommand>()
            .AddCommand<UpdateTaskCommand>()
            .AddCommand<UpdateTaskStatusCommand>()
            .AddCommand<UpdateTaskLabelCommand>()
            .AddCommand<UpdateTaskDescriptionCommand>()
            .AddCommand<DeleteTaskCommand>()
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
        Func<IConsole, ValueTask> notReady = null
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
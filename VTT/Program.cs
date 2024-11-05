using CliFx;

namespace VTT;

public static class Program
{
    public static async Task Main(string[] args)
    {
        await new CliApplicationBuilder()
            //.AddCommand<ICommand>()
            .Build()
            .RunAsync(args);
    }
}

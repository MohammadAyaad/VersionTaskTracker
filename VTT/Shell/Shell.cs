using System;
using System.Text.RegularExpressions;
using McMaster.Extensions.CommandLineUtils;

namespace VTT.Commands.Shell;

public class Shell
{
    public string Location { get; private set; }

    public Shell() { }

    public async Task<int> RunAsync(string[] args)
    {
        Location = AppDomain.CurrentDomain.BaseDirectory;
        while (true)
        {
            try
            {
                Console.Write($"{Location}>");
                string input = Console.ReadLine()!;
                await Program.Main(SplitArgs(input));
            }
            catch(ExitShellException e) {
                return 0;
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
            }
        }
    }
    public static string[] SplitArgs(string input)
    {
        var matches = Regex.Matches(input, @"[\""].+?[\""]|[^ ]+");
        var args = new List<string>();
        foreach (Match match in matches)
        {
            args.Add(match.Value.Trim('"'));
        }
        return args.ToArray();
    }
}

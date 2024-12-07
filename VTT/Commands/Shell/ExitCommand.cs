using System;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace VTT.Commands.Shell;

[Command("exit")]
public class ExitCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        throw new ExitShellException();
    }
}

[System.Serializable]
public class ExitShellException : System.Exception
{
    public ExitShellException() { }
    public ExitShellException(string message) : base(message) { }
    public ExitShellException(string message, System.Exception inner) : base(message, inner) { }
    protected ExitShellException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

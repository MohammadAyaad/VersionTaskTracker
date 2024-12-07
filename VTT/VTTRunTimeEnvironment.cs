using System;

namespace VTT;

public class VTTRunTimeEnvironment
{
    public string? CurrentComponentPath { get; private set; }
    public string CurrentSystemPath { get; private set; }
    public string GlobalShellEnvironmentPath { get; private set; }

}

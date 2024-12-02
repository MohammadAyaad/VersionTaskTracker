using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesX.Extensions;
using VersionTaskTracker.Context;
using VersionTaskTracker.Model.Configuration;
using VersionTaskTracker.Model.FileSystem;
using VersionTaskTracker.Model.Tracker;
using VersionTaskTracker.Model.Tracking;

namespace VersionTaskTracker.Services;

public class VTTInstance : IStorable<VTTInstance>
{
    private TasksDbContext _tasks;
    private VTTInstanceConfig _config;
    private string _workingDirectory;
    private List<string> _vttIgnore;
    private VTTInstanceMetadata _metadata;

    public string WorkingDirectory { get { return this._workingDirectory; } }
    public TasksDbContext TasksDbContext { get { return this._tasks; } }
    public VTTInstanceConfig InstanceConfiguration { get { return this._config; } }
    public List<string> VTTIgnore { get { return this._vttIgnore; } }
    public VTTInstanceMetadata Metadata { get { return this._metadata; } }

    public VTTInstance(string workingDirectory,VTTConfig vttconfig,VTTInstanceConfig config = null)
    {
        config ??= new VTTInstanceConfig(vttconfig,workingDirectory);
        this._workingDirectory = workingDirectory;
        this._config = config;
        this._vttIgnore = LoadIgnores();
        Component c = new Component()
        {
            Id = Guid.NewGuid(),
            ComponentType = ComponentType.DIRECTORY,
            Name = Path.GetDirectoryName(workingDirectory).Split("\\").Last(),
            ParentComponentId = null,
            ParentComponent = null,
            Path = "/",
            Tasks = new List<Model.Tracking.Task>(),
            Children = new List<Component>(),
            Description = $"Hello {workingDirectory.Split("\\").Last()}"
        };
        this._metadata = new VTTInstanceMetadata(this._config,workingDirectory,c.Id); 
        string tasksDbPath = Path.Combine(workingDirectory, _config.VTTInstanceDirName!, _config.VTTInstanceTasksDatabaseName!);
        this._tasks = new TasksDbContext(tasksDbPath);
        this._tasks.Components.Add(c);
    }

    private VTTInstance(TasksDbContext tasks, VTTInstanceConfig iconfig,VTTInstanceMetadata metadata,VTTInstanceState instanceState,List<string> vttIgnore, string workingDirectory)
    {
        _tasks = tasks;
        _config = iconfig;
        _workingDirectory = workingDirectory;
        _vttIgnore = vttIgnore;
        _metadata = metadata;
    }

    public bool InstanceExists()
    {
        string tasksDbPath = Path.Combine(this._workingDirectory, _config.VTTInstanceDirName!, _config.VTTInstanceTasksDatabaseName!);
        return File.Exists(tasksDbPath);
    }
    
    public void Save()
    {
        this._tasks.Save();
        this._config.Save();
        this._metadata.Save();
    }
    public void Load()
    {
        this._config.Load();
        this._vttIgnore = LoadIgnores();
        this._metadata.Load();
        this._tasks = new TasksDbContext(Path.Combine(this._workingDirectory, this._config.VTTInstanceDirName!, this._config.VTTInstanceTasksDatabaseName!));
    }

    private List<string> LoadIgnores()
    {
        string ignoreFile = Path.Combine(this._workingDirectory, this._config.VTTInstanceIgnoreFile!);

        return File.Exists(ignoreFile) ? File.ReadAllLines(ignoreFile).Map(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l) && !string.IsNullOrWhiteSpace(l) && !l.StartsWith("#")).Map(l =>
        {
            if (l.Contains("#"))
            {
                return l.Substring(0, l.IndexOf('#')).Trim();
            }
            return l;
        }).ToList() : new List<string>();
    }

}


public class VTTInstanceMetadata : IStorable<VTTInstanceMetadata>
{
    [JsonIgnore]
    private string _workingDirectory;
    [JsonIgnore]
    private VTTInstanceConfig _config;
    public VTTInstanceMetadata(VTTInstanceConfig config, string workingDir, Guid rootId = default)
    {
        this._config = config;
        _workingDirectory = workingDir;
        this.RootComponentId = rootId;
    }
    public Guid RootComponentId;

    public void Load()
    {
        string metadataFilePath = Path.Combine(this._workingDirectory, this._config.VTTInstanceDirName!, this._config.VTTInstanceMetadataFile!);

        VTTInstanceMetadata metadata = JObject.Parse(File.ReadAllText(metadataFilePath)).ToObject<VTTInstanceMetadata>()!;

        this.RootComponentId = metadata.RootComponentId;
    }

    public void Save()
    {
        string metadataFilePath = Path.Combine(this._workingDirectory, this._config.VTTInstanceDirName!, this._config.VTTInstanceMetadataFile!);

        File.WriteAllText(metadataFilePath, JObject.FromObject(this).ToString());
    }
}
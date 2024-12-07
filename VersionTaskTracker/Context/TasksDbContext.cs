using GlobExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.IO.Abstractions;
using System.Text.RegularExpressions;
using UtilitiesX;
using UtilitiesX.Extensions;
using VersionTaskTracker.Model.FileSystem;
using VersionTaskTracker.Model.Tracker;
using VersionTaskTracker.Model.Tracking;

namespace VersionTaskTracker.Context;

public class TasksDbContext : DbContext , IStorable<TasksDbContext>
{
    private string _workingDirectory;
    private string _path;
    public string DatabasePath { get {  return _path; } }

    public TasksDbContext(string path, string workingDirectory)
    {
        this._path = path;
        this._workingDirectory = workingDirectory;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Component>()
            .HasOne(c => c.ParentComponent)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentComponentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Component>()
            .HasIndex(c => c.Path)
            .IsUnique(true);

        modelBuilder.Entity<VersionTaskTracker.Model.Tracking.Task>()
            .HasIndex(t => t.Int_Id)
            .IsUnique();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={this._path};");
        //    .EnableSensitiveDataLogging()  
        //    .LogTo(Console.WriteLine, LogLevel.Information);
        optionsBuilder.UseLazyLoadingProxies();
    }

    public void Load()
    {
        throw new NotImplementedException();
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(this._path));
        Console.WriteLine(this.ChangeTracker.DebugView.LongView);
        this.Database.EnsureCreated();
        this.Database.Migrate();
        this.SaveChanges();
    }


    public Guid FromDirectory(IDirectoryInfo dir, List<string> ignores)
    {
        Component c = TraverseFromDirectory(dir,null,
        d => {
            return (!ShouldIgnore(GetRelativePath(dir.FullName,d.FullName)+"/", ignores))  && (this.Components.AsEnumerable().Any(c => c.Path.Equals(d.FullName)));
        },
        f => {
            return (!ShouldIgnore(GetRelativePath(dir.FullName,f.FullName),ignores)) && (this.Components.AsEnumerable().Any(c => c.Path.Equals(f.FullName)));
        });
        Console.WriteLine(JObject.FromObject(this.ChangeTracker.DebugView).ToString());
        return c.Id;
    }

    public Component GetWorkingTree(IDirectoryInfo workingDirectory, List<string> ignores) {
        return TraverseFromDirectory(workingDirectory, null, 
        d => {
            string relative = GetRelativePath(workingDirectory.FullName,d.FullName)+"/";
            return (!ShouldIgnore(relative, ignores));
        },
        f => {
            string relative = GetRelativePath(workingDirectory.FullName,f.FullName);
            return (!ShouldIgnore(relative, ignores));
        });
    }

    public List<Component> GetUntracked(IDirectoryInfo workingDirectory, List<string> ignores)
    {
        return GetWorkingTree(workingDirectory,ignores)
        .Flatten(c => c.Children ?? new List<Component>())
        .Where(c => !this.Components.Any(x => x.Path.Equals(c.Path)))
        .ToList();
    }

    private string GetRelativePath(string root, string path) {
        List<string> rl = root.Split(Path.DirectorySeparatorChar).ToList();
        List<string> pl = path.Split(Path.DirectorySeparatorChar).ToList();
        int i;
        for(i = 0;i < rl.Count;i++) {
            if(!(i >= 0 && i < rl.Count && i < pl.Count)) break; 
            if(!rl[i].Equals(pl[i])) break; 
        }
        return string.Join(Path.DirectorySeparatorChar,pl.Slice(i,pl.Count - i));
    }

    private Component TraverseFromDirectory(IDirectoryInfo dir, Component parent,Func<IDirectoryInfo,bool> IsDirIncluded,Func<IFileInfo,bool> IsFileIncluded)
    {
        Component r = 
            (this.Components.Any(c => c.Path.Equals(GetRelativePath(this._workingDirectory,dir.FullName)+"/")))?
                this.Components.AsNoTracking().FirstOrDefault(c => c.Path.Equals(GetRelativePath(this._workingDirectory,dir.FullName)+"/"))!
                : new Component()
        {
            Id = Guid.NewGuid(),
            Name = dir.Name,
            Path = GetRelativePath(this._workingDirectory,dir.FullName) + "/",
            ComponentType = ComponentType.DIRECTORY,
            ParentComponent = parent,
            ParentComponentId = (parent == null) ? null : parent.Id,
            Description = "",
            Tasks = new List<Model.Tracking.Task>(),
            Children = new List<Component>()
        };

        IDirectoryInfo[] dirs = dir.GetDirectories()
            .Where(d => IsDirIncluded(d))
            .ToArray();

        IFileInfo[] files = dir.GetFiles()
            .Where(f => IsFileIncluded(f))
            .ToArray();

        foreach (IFileInfo f in files)
        {
            //Console.WriteLine($"FILE {f.FullName}");
            string fr_path = GetRelativePath(this._workingDirectory,f.FullName);
            Component c = 
                (this.Components.Any(x => x.Path.Equals(fr_path)))?
                this.Components.FirstOrDefault(x => x.Path.Equals(fr_path))!:
            new Component()
            {
                Id = Guid.NewGuid(),
                Name = f.Name,
                Path = GetRelativePath(this._workingDirectory,f.FullName),
                Tasks = new List<Model.Tracking.Task>(),
                ComponentType = ComponentType.FILE,
                ParentComponent = r,
                Children = null,
                Description = "",
                ParentComponentId = r.Id
            };
            r.Children.Add(c);
        }

        foreach (IDirectoryInfo d in dirs)
        {
            //Console.WriteLine($"DIR {d.FullName}");
            r.Children.Add(TraverseFromDirectory(d,r, IsDirIncluded,IsFileIncluded));
        }
        return r;
    }

    static bool ShouldIgnore(string path, List<string> ignorePatterns)
    {
        return ignorePatterns.Any(i => Glob.IsMatch(path, i, GlobOptions.CaseInsensitive));
    }

    public DbSet<Model.Tracking.Task> Tasks { get; set; } = default!;
    public DbSet<Model.Tracking.Component> Components { get; set; } = default!;
}

using GlobExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UtilitiesX;
using UtilitiesX.Extensions;
using VersionTaskTracker.Model.FileSystem;
using VersionTaskTracker.Model.Tracker;
using VersionTaskTracker.Model.Tracking;

namespace VersionTaskTracker.Context;

public class TasksDbContext : DbContext , IStorable<TasksDbContext>
{
    private string _path;
    public string DatabasePath { get {  return _path; } }

    public TasksDbContext(string path)
    {
        this._path = path;
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
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={this._path};")
            .EnableSensitiveDataLogging()  
            .LogTo(Console.WriteLine, LogLevel.Information);
        optionsBuilder.UseLazyLoadingProxies();
    }

    public void Load()
    {
        throw new NotImplementedException();
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(this._path));
        this.Database.EnsureCreated();
        this.Database.Migrate();
        this.SaveChanges();
    }


    public Guid FromDirectory(string dir, List<string> ignores)
    {
        Component c = TraverseFromDirectory(dir, "", null, (p,dir,root,isDir) => !ShouldIgnore(p,ignores));
        Console.WriteLine(JObject.FromObject(this.ChangeTracker.DebugView).ToString());
        return c.Id;
    }

    
    public Component GetUntracked(string workingDirectory, List<string> ignores)
    {
        return TraverseFromDirectory(workingDirectory + "\\", "", null, (p,dir,root,isDir) =>
        {
            return (!ShouldIgnore(p, ignores)) && (this.Components.AsEnumerable().Where(c => ProcessPath(c.Path,dir,root,isDir).Equals(p)).Count() == 0);
        });
    }
    public static string ConvertWindowsToUnixPath(string windowsPath)
    {
        return windowsPath.Replace("\\", "/").Branch(c => Regex.IsMatch(c, ".:/.*"), onTrue: (c) => c.Replace($"{c.First()}:", $"/{c.First()}"));
    }
    public static string ConvertUnixToWindows(string unixPath)
    {
        return unixPath.Replace("/", "\\").Branch(c => Regex.IsMatch(c,@"\\.\\.*"),onTrue: c=> c.Replace($"\\{c.First()}",$"{c.First()}:"));
    }
    private static string ProcessPath(string x,string dir,string root,bool isDir)
    {
        //string final = root + x.Replace(dir, "").Replace("\\", "/");
        //if (final.StartsWith("/"))
        //{
        //    final = final.Remove(0, 1);
        //}
        //final += "/";
        string a = ConvertWindowsToUnixPath(x);
        string b = ConvertWindowsToUnixPath(dir).Branch(c => c.Last() != '/',onTrue: c => $"{c}/");
        string f = a.Replace(b, "");
        //if (x.Contains("res")) Debugger.Break();
        return f;
    }
    private Component TraverseFromDirectory(string dir, string root, Component parent, Func<string,string,string,bool,bool> IsIncluded)
    {
        Component r = new Component()
        {
            Id = Guid.NewGuid(),
            Name = dir.Split('\\').Last(),
            Path = root,
            ComponentType = ComponentType.DIRECTORY,
            ParentComponent = parent,
            ParentComponentId = (parent == null) ? null : parent.Id,
            Description = "",
            Tasks = new List<Model.Tracking.Task>(),
            Children = new List<Component>()
        };

        string[] dirs = Directory.GetDirectories(dir).Map(d => ProcessPath(d,dir,root,true) + "/").Where(d => IsIncluded(d,dir,root,true)).Map(s => (!string.IsNullOrEmpty(root)) ? s.Replace(root, "") : s).ToArray();
        string[] files = Directory.GetFiles(dir).Map(d => ProcessPath(d,dir,root,false)).Where(d => IsIncluded(d,dir,root,false)).Map(s => (!string.IsNullOrEmpty(root)) ? s.Replace(root, "") : s).ToArray();

        foreach (string f in files)
        {
            string file = $"{root}{f}";//Path.Combine(dir, string.Join('\\', f.Split("/").Where(s => !string.IsNullOrEmpty(s))));
            Console.WriteLine($"FILE {root}{f}");
            Component c = new Component()
            {
                Name = Path.GetFileName(file),
                Path = file,
                Tasks = new List<Model.Tracking.Task>(),
                ComponentType = ComponentType.FILE,
                ParentComponent = r,
                Children = null,
                Description = "",
                Id = Guid.NewGuid(),
                ParentComponentId = r.Id
            };
            r.Children.Add(c);
        }

        foreach (string dd in dirs)
        {
            string x = ConvertUnixToWindows(dd);
            string d = $"{dir}{x}";
            Console.WriteLine($"DIR {root}{dd}".Replace("//","/"));
            r.Children.Add(TraverseFromDirectory(d, $"{root}{dd}".Replace("//","/"), r, IsIncluded));
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

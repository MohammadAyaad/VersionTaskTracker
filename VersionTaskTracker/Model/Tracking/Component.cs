using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace VersionTaskTracker.Model.Tracking;

public enum ComponentType : byte
{
    DIRECTORY,
    FILE,
}

public class Component
{
    public Guid Id { get; set; }
    public required string Path { get; set; }
    public required string Name { get; set; }
    public required ComponentType ComponentType { get; set; }
    public string Description { get; set; } = string.Empty;

    [NotMapped]
    public bool Tracked { get; set; } = false;

    public Guid? ParentComponentId { get; set; }

    [JsonIgnore]
    [ForeignKey("ParentComponentId")]
    public virtual Component? ParentComponent { get; set; }
    public virtual ICollection<Task> Tasks { get; set; } = default!;
    public virtual ICollection<Component>? Children { get; set; } = new List<Component>();

    public List<Component> Flatten()
    {
        if (Children == null)
            return new List<Component>();

        List<Component> result = Children.ToList();
        foreach (var child in Children)
        {
            result.AddRange(child.Flatten());
        }
        return result;
    }
}

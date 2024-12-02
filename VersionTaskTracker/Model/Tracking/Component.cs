using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public Guid? ParentComponentId { get; set; }
    [JsonIgnore]
    [ForeignKey("ParentComponentId")]
    public virtual Component ParentComponent { get; set; }
    public virtual ICollection<Task> Tasks { get; set; }
    public virtual ICollection<Component> Children { get; set; } = new List<Component>();
}

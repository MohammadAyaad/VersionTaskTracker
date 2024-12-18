﻿namespace VersionTaskTracker.Model.Tracking;

public class Task
{
    public Guid Id { get; set; }
    public long Int_Id { get; set; }
    public required string Label { get; set; }
    public required string Status { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid ParentComponentId { get; set; }
    public virtual Component ParentComponent { get; set; } = default!;
    
    public override string ToString()
    {
        return $"[{Id.ToString()}|#{Int_Id}]: {Label}\nStatus : {Status}\n{Description}";
    }
}

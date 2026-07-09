namespace Core.Infrastructure.Entities;

using System.ComponentModel.DataAnnotations;
using static Directories.Domain.Entities.Resource;

public class Resource : IGuidIdentity
{
    [Key]
    public Guid Guid { get; set; }
    public string Name { get; set; } = string.Empty;
    public Conditions Condition { get; set; }
}

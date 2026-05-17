namespace Core.Infrastructure.Entities;

using System.ComponentModel.DataAnnotations;
using static Directories.Domain.Entities.Client;

public class Client : IGuidIdentity
{
    [Key]
    public Guid Guid { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public Conditions Condition { get; set; }
}

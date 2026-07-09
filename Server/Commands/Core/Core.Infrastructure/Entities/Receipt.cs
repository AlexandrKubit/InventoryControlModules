namespace Core.Infrastructure.Entities;

using System.ComponentModel.DataAnnotations;

public class Receipt : IGuidIdentity
{
    [Key]
    public Guid Guid { get; set; }
    public string Number { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}

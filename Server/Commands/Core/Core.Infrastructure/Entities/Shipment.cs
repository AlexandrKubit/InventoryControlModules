namespace Core.Infrastructure.Entities;

using System.ComponentModel.DataAnnotations;
using static global::Shipment.Domain.Entities.Document;

public class Shipment : IGuidIdentity
{
    [Key]
    public Guid Guid { get; set; }
    public string Number { get; set; } = string.Empty;
    public Guid ClientGuid { get; set; }
    public DateTime Date { get; set; }
    public Conditions Condition { get; set; }
}

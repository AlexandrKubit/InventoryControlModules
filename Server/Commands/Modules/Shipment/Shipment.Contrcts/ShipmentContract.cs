namespace Shipment.Contracts;
using Core.Domain;

public class ShipmentContract
{
    public record SignedRangeArg(HashSet<Guid> DocumentGuids, IData Data);
    public static Action<Func<SignedRangeArg, Task>> OnSignedRange { get; set; }
    public record UnsignedRangeArg(HashSet<Guid> DocumentGuids, IData Data);
    public static Action<Func<UnsignedRangeArg, Task>> OnUnsignedRange { get; set; }
}

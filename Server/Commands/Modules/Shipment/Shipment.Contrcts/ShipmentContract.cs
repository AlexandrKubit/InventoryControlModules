namespace Shipment.Contracts;

using Core.Domain;

public static class ShipmentContract
{
    static ShipmentContract()
    {
        Domain.Entities.Document.OnSignedRange(OnSignedRangeHandler);
        Domain.Entities.Document.OnUnsignedRange(OnUnsignedRangeHandler);
    }

    public record SignedRangeArg(HashSet<Guid> DocumentGuids, IData Data);
    public static event Func<SignedRangeArg, Task> SignedRange = _ => Task.CompletedTask;
    public record UnsignedRangeArg(HashSet<Guid> DocumentGuids, IData Data);
    public static Func<UnsignedRangeArg, Task> UnsignedRange = _ => Task.CompletedTask;

    private static async Task OnSignedRangeHandler(Domain.Entities.Document.SignedRangeArg args)
    {
        await SignedRange.Invoke(new SignedRangeArg(args.DocumentGuids, args.Data));
    }

    private static async Task OnUnsignedRangeHandler(Domain.Entities.Document.UnsignedRangeArg args)
    {
        await UnsignedRange.Invoke(new UnsignedRangeArg(args.DocumentGuids, args.Data));
    }
}

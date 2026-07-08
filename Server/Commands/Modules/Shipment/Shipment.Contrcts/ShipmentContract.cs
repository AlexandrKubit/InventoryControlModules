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
    private static readonly DomainEvent<SignedRangeArg> SignedRange = new();
    public static Action<Func<SignedRangeArg, Task>> OnSignedRange => SignedRange.Subscribe;

    public record UnsignedRangeArg(HashSet<Guid> DocumentGuids, IData Data);
    private static readonly DomainEvent<UnsignedRangeArg> UnsignedRange = new();
    public static Action<Func<UnsignedRangeArg, Task>> OnUnsignedRange => UnsignedRange.Subscribe;

    private static async Task OnSignedRangeHandler(Domain.Entities.Document.SignedRangeArg args)
    {
        await SignedRange.Invoke(new SignedRangeArg(args.DocumentGuids, args.Data));
    }

    private static async Task OnUnsignedRangeHandler(Domain.Entities.Document.UnsignedRangeArg args)
    {
        await UnsignedRange.Invoke(new UnsignedRangeArg(args.DocumentGuids, args.Data));
    }
}

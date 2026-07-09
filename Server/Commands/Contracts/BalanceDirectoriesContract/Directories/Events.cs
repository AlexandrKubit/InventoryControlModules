namespace BalanceDirectoriesContract.Directories;
using Core.Domain;


public static class Events
{
    public static class MeasureUnit
    {
        public record DeletedRangeArg(HashSet<Guid> Guids, IData Data);
        public static DomainEvent<DeletedRangeArg> DeletedRange { get; } = new();
    }
    public static class Resource
    {
        public record DeletedRangeArg(HashSet<Guid> Guids, IData Data);
        public static DomainEvent<DeletedRangeArg> DeletedRange { get; } = new();
    }
}

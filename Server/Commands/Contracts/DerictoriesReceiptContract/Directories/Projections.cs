namespace DerictoriesReceiptContract.Directories;

public static class Projections
{
    public static class MeasureUnit
    {
        public interface IMeasureUnit
        {
            Guid Guid { get; }
        }

        public interface IMeasureUnitRepository
        {
            IEnumerable<IMeasureUnit> List { get; }
            Task EnsureActiveByGuids(HashSet<Guid> guids);
        }
    }

    public static class Resource
    {
        public interface IResource
        {
            Guid Guid { get; }
        }

        public interface IResourceRepository
        {
            IEnumerable<IResource> List { get; }
            Task EnsureActiveByGuids(HashSet<Guid> guids);
        }
    }
}

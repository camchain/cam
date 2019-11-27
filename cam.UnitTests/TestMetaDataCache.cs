using Cam.IO;
using Cam.IO.Caching;

namespace Cam.UnitTests
{
    public class TestMetaDataCache<T> : MetaDataCache<T> where T : class, ICloneable<T>, ISerializable, new()
    {
        public TestMetaDataCache()
            : base(null)
        {
        }

        protected override void AddInternal(T item)
        {
        }

        protected override T TryGetInternal()
        {
            return null;
        }

        protected override void UpdateInternal(T item)
        {
        }
    }
}

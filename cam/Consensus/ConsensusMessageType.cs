using Cam.IO.Caching;

namespace Cam.Consensus
{
    internal enum ConsensusMessageType : byte
    {
        [ReflectionCache(typeof(ChangeView))]
        ChangeView = 0x00,
        [ReflectionCache(typeof(PrepareRequest))]
        PrepareRequest = 0x20,
        [ReflectionCache(typeof(PrepareResponse))]
        PrepareResponse = 0x21,
    }
}

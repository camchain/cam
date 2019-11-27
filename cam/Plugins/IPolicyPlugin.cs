using Cam.Network.P2P.Payloads;
using System.Collections.Generic;

namespace Cam.Plugins
{
    public interface IPolicyPlugin
    {
        bool FilterForMemoryPool(Transaction tx);
        IEnumerable<Transaction> FilterForBlock(IEnumerable<Transaction> transactions);
        int MaxTxPerBlock { get; }
        int MaxLowPriorityTxPerBlock { get; }
    }
}

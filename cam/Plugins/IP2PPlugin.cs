using Cam.Network.P2P;
using Cam.Network.P2P.Payloads;

namespace Cam.Plugins
{
    public interface IP2PPlugin
    {
        bool OnP2PMessage(Message message);
        bool OnConsensusMessage(ConsensusPayload payload);
    }
}

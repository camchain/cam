using Cam.IO;
using Cam.VM;
using System.IO;

namespace Cam.Core
{



    public interface IVerifiable : ISerializable, IScriptContainer
    {



        Witness[] Scripts { get; set; }




        void DeserializeUnsigned(BinaryReader reader);




        UInt160[] GetScriptHashesForVerifying();




        void SerializeUnsigned(BinaryWriter writer);
    }
}

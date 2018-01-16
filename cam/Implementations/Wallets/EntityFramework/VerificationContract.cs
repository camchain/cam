using Cam.IO;
using Cam.SmartContract;
using Cam.VM;
using System;
using System.IO;
using System.Linq;

namespace Cam.Implementations.Wallets.EntityFramework
{
    public class VerificationContract : SmartContract.Contract, IEquatable<VerificationContract>, ISerializable
    {
        public int Size => 20 + ParameterList.GetVarSize() + Script.GetVarSize();




        public void Deserialize(BinaryReader reader)
        {
            reader.ReadSerializable<UInt160>();
            ParameterList = reader.ReadVarBytes().Select(p => (ContractParameterType)p).ToArray();
            Script = reader.ReadVarBytes();
        }





        public bool Equals(VerificationContract other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (ReferenceEquals(null, other)) return false;
            return ScriptHash.Equals(other.ScriptHash);
        }





        public override bool Equals(object obj)
        {
            return Equals(obj as VerificationContract);
        }




        public override int GetHashCode()
        {
            return ScriptHash.GetHashCode();
        }




        public void Serialize(BinaryWriter writer)
        {
            throw new NotSupportedException();
        }
    }
}

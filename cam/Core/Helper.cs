using Cam.Cryptography;
using Cam.SmartContract;
using Cam.VM;
using Cam.Wallets;
using System;
using System.IO;
using System.Linq;

namespace Cam.Core
{



    public static class Helper
    {
        public static byte[] GetHashData(this IVerifiable verifiable)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                verifiable.SerializeUnsigned(writer);
                writer.Flush();
                return ms.ToArray();
            }
        }






        public static byte[] Sign(this IVerifiable verifiable, KeyPair key)
        {
            using (key.Decrypt())
            {
                return Crypto.Default.Sign(verifiable.GetHashData(), key.PrivateKey, key.PublicKey.EncodePoint(false).Skip(1).ToArray());
            }
        }

        public static UInt160 ToScriptHash(this byte[] script)
        {
            return new UInt160(Crypto.Default.Hash160(script));
        }

        internal static bool VerifyScripts(this IVerifiable verifiable)
        {
            UInt160[] hashes;
            try
            {
                hashes = verifiable.GetScriptHashesForVerifying();
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            if (hashes.Length != verifiable.Scripts.Length) return false;
            for (int i = 0; i < hashes.Length; i++)
            {
                byte[] verification = verifiable.Scripts[i].VerificationScript;
                if (verification.Length == 0)
                {
                    using (ScriptBuilder sb = new ScriptBuilder())
                    {
                        sb.EmitAppCall(hashes[i].ToArray());
                        verification = sb.ToArray();
                    }
                }
                else
                {
                    if (hashes[i] != verifiable.Scripts[i].ScriptHash) return false;
                }

                Cam.IO.Caching.DataCache<UInt160, ContractState> contracts = Blockchain.Default.CreateCache<UInt160, ContractState>();

                CachedScriptTable script_table = new CachedScriptTable(contracts);

                ApplicationEngine engine = new ApplicationEngine(TriggerType.Verification, verifiable, script_table, Blockchain.Default, StateReader.Default, Fixed8.Zero);
                engine.LoadScript(verification, false);
                engine.LoadScript(verifiable.Scripts[i].InvocationScript, true);
                if (!engine.Execute()) return false;
                if (engine.EvaluationStack.Count != 1 || !engine.EvaluationStack.Pop().GetBoolean()) return false;
            }
            return true;
        }
    }
}

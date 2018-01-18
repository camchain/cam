using Cam.Cryptography;
using Cam.Cryptography.ECC;
using Cam.IO;
using Cam.IO.Caching;
using Cam.SmartContract;
using Cam.VM;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cam.Core
{



    public abstract class Blockchain : IDisposable, IBlock
    {
        public static event EventHandler<BlockNotifyEventArgs> Notify;
        public static event EventHandler<Block> PersistCompleted;



        public const uint SecondsPerBlock = 15;
        public const uint DecrementInterval = 2000000;



        public static readonly uint[] GenerationAmount = { 20, 16, 13, 11, 9, 7, 5, 3, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1 };



        public static readonly int CAMShare = 200000000;



        public static readonly TimeSpan TimePerBlock = TimeSpan.FromSeconds(SecondsPerBlock);



        public static readonly ECPoint[] StandbyValidators = Settings.Default.StandbyValidators.OfType<string>().Select(p => ECPoint.DecodePoint(p.HexToBytes(), ECCurve.Secp256r1)).ToArray();

#pragma warning disable CS0612
        public static readonly RegisterTransaction GoverningToken = new RegisterTransaction
        {
            AssetType = AssetType.GoverningToken,            
            Name = "[{\"lang\":\"zh-CN\",\"name\":\"CAM\"},{\"lang\":\"en\",\"name\":\"CAM\"}]",
            Amount = Fixed8.FromDecimal(CAMShare),
            Precision = 0,
            Owner = ECCurve.Secp256r1.Infinity,
            Admin = (new[] { (byte)OpCode.PUSHT }).ToScriptHash(),
            Attributes = new TransactionAttribute[0],
            Inputs = new CoinReference[0],
            Outputs = new TransactionOutput[0],
            Scripts = new Witness[0]
        };

        public static readonly RegisterTransaction UtilityToken = new RegisterTransaction
        {
            AssetType = AssetType.UtilityToken,            
            Name = "[{\"lang\":\"zh-CN\",\"name\":\"GAS\"},{\"lang\":\"en\",\"name\":\"GAS\"}]",
            Amount = Fixed8.FromDecimal(GenerationAmount.Sum(p => p) * DecrementInterval),
            Precision = 8,
            Owner = ECCurve.Secp256r1.Infinity,
            Admin = (new[] { (byte)OpCode.PUSHF }).ToScriptHash(),
            Attributes = new TransactionAttribute[0],
            Inputs = new CoinReference[0],
            Outputs = new TransactionOutput[0],
            Scripts = new Witness[0]
        };
#pragma warning restore CS0612



        public static readonly Block GenesisBlock = new Block
        {
            PrevHash = UInt256.Zero,            
            Timestamp = (new DateTime(2017, 11, 15, 8, 0, 0, DateTimeKind.Utc)).ToTimestamp(),
            Index = 0,

            ConsensusData = 88596195, 
            NextConsensus = GetConsensusAddress(StandbyValidators),
            Script = new Witness
            {
                InvocationScript = new byte[0],
                VerificationScript = new[] { (byte)OpCode.PUSHT }
            },
            Transactions = new Transaction[]
            {
                new MinerTransaction
                {
                    Nonce = 88596195,
                    Attributes = new TransactionAttribute[0],
                    Inputs = new CoinReference[0],
                    Outputs = new TransactionOutput[0],
                    Scripts = new Witness[0]
                },
                GoverningToken,
                UtilityToken,
                new IssueTransaction
                {
                    Attributes = new TransactionAttribute[0],
                    Inputs = new CoinReference[0],
                    Outputs = new[]
                    {
                        new TransactionOutput
                        {
                            AssetId = GoverningToken.Hash,
                            Value = GoverningToken.Amount,
                            ScriptHash = Contract.CreateMultiSigRedeemScript(StandbyValidators.Length / 2 + 1, StandbyValidators).ToScriptHash()
                        }
                    },
                    Scripts = new[]
                    {
                        new Witness
                        {
                            InvocationScript = new byte[0],
                            VerificationScript = new[] { (byte)OpCode.PUSHT }
                        }
                    }
                }
            }
        };



        public abstract UInt256 CurrentBlockHash { get; }



        public abstract UInt256 CurrentHeaderHash { get; }



        public static Blockchain Default { get; private set; } = null;



        public abstract uint HeaderHeight { get; }



        public abstract uint Height { get; }

        static Blockchain()
        {
            GenesisBlock.RebuildMerkleRoot();
        }





        public abstract bool AddBlock(Block block);




        protected internal abstract void AddHeaders(IEnumerable<Header> headers);

        public static Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, bool ignoreClaimed = true)
        {
            List<SpentCoin> unclaimed = new List<SpentCoin>();
            foreach (var group in inputs.GroupBy(p => p.PrevHash))
            {
                Dictionary<ushort, SpentCoin> claimable = Default.GetUnclaimed(group.Key);
                if (claimable == null || claimable.Count == 0)
                    if (ignoreClaimed)
                        continue;
                    else
                        throw new ArgumentException();
                foreach (CoinReference claim in group)
                {
                    if (!claimable.TryGetValue(claim.PrevIndex, out SpentCoin claimed))
                        if (ignoreClaimed)
                            continue;
                        else
                            throw new ArgumentException();
                    unclaimed.Add(claimed);
                }
            }
            return CalculateBonusInternal(unclaimed);
        }

        public static Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, uint height_end)
        {
            List<SpentCoin> unclaimed = new List<SpentCoin>();
            foreach (var group in inputs.GroupBy(p => p.PrevHash))
            {
                Transaction tx = Default.GetTransaction(group.Key, out int height_start);
                if (tx == null) throw new ArgumentException();
                if (height_start == height_end) continue;
                foreach (CoinReference claim in group)
                {
                    if (claim.PrevIndex >= tx.Outputs.Length || !tx.Outputs[claim.PrevIndex].AssetId.Equals(GoverningToken.Hash))
                        throw new ArgumentException();
                    unclaimed.Add(new SpentCoin
                    {
                        Output = tx.Outputs[claim.PrevIndex],
                        StartHeight = (uint)height_start,
                        EndHeight = height_end
                    });
                }
            }
            return CalculateBonusInternal(unclaimed);
        }

        private static Fixed8 CalculateBonusInternal(IEnumerable<SpentCoin> unclaimed)
        {
            Fixed8 amount_claimed = Fixed8.Zero;
            foreach (var group in unclaimed.GroupBy(p => new { p.StartHeight, p.EndHeight }))
            {
                uint amount = 0;
                uint ustart = group.Key.StartHeight / DecrementInterval;
                if (ustart < GenerationAmount.Length)
                {
                    uint istart = group.Key.StartHeight % DecrementInterval;
                    uint uend = group.Key.EndHeight / DecrementInterval;
                    uint iend = group.Key.EndHeight % DecrementInterval;
                    if (uend >= GenerationAmount.Length)
                    {
                        uend = (uint)GenerationAmount.Length;
                        iend = 0;
                    }
                    if (iend == 0)
                    {
                        uend--;
                        iend = DecrementInterval;
                    }
                    while (ustart < uend)
                    {
                        amount += (DecrementInterval - istart) * GenerationAmount[ustart];
                        ustart++;
                        istart = 0;
                    }
                    amount += (iend - istart) * GenerationAmount[ustart];
                }
                amount += (uint)(Default.GetSysFeeAmount(group.Key.EndHeight - 1) - (group.Key.StartHeight == 0 ? 0 : Default.GetSysFeeAmount(group.Key.StartHeight - 1)));
                amount_claimed += group.Sum(p => p.Value) / CAMShare * amount;
            }
            return amount_claimed;
        }





        public abstract bool ContainsBlock(UInt256 hash);





        public abstract bool ContainsTransaction(UInt256 hash);

        public bool ContainsUnspent(CoinReference input)
        {
            return ContainsUnspent(input.PrevHash, input.PrevIndex);
        }

        public abstract bool ContainsUnspent(UInt256 hash, ushort index);

        public abstract DataCache<TKey, TValue> CreateCache<TKey, TValue>()
            where TKey : IEquatable<TKey>, ISerializable, new()
            where TValue : class, ISerializable, new();

        public abstract void Dispose();

        public abstract AccountState GetAccountState(UInt160 script_hash);

        public abstract AssetState GetAssetState(UInt256 asset_id);




        public abstract List<AssetState> GetAllAssetState();





        public Block GetBlock(uint height)
        {
            UInt256 hash = GetBlockHash(height);
            if (hash == null) return null;
            return GetBlock(hash);
        }





        public abstract Block GetBlock(UInt256 hash);





        public abstract UInt256 GetBlockHash(uint height);

        public abstract ContractState GetContract(UInt160 hash);

        public abstract IEnumerable<ValidatorState> GetEnrollments();





        public abstract Header GetHeader(uint height);





        public abstract Header GetHeader(UInt256 hash);





        public static UInt160 GetConsensusAddress(ECPoint[] validators)
        {
            return Contract.CreateMultiSigRedeemScript(validators.Length - (validators.Length - 1) / 3, validators).ToScriptHash();
        }

        private List<ECPoint> _validators = new List<ECPoint>();




        public ECPoint[] GetValidators()
        {
            lock (_validators)
            {
                if (_validators.Count == 0)
                {
                    _validators.AddRange(GetValidators(Enumerable.Empty<Transaction>()));
                }
                return _validators.ToArray();
            }
        }

        public virtual IEnumerable<ECPoint> GetValidators(IEnumerable<Transaction> others)
        {


















            return StandbyValidators;
        }





        public abstract Block GetNextBlock(UInt256 hash);





        public abstract UInt256 GetNextBlockHash(UInt256 hash);

        byte[] IBlock.GetScript(byte[] script_hash)
        {
            return GetContract(new UInt160(script_hash)).Script;
        }




        uint IBlock.GetTimestamp()
        {
            Header header = GetHeader(Default.HeaderHeight);
            return header.Timestamp;
        }

        public abstract StorageItem GetStorageItem(StorageKey key);





        public virtual long GetSysFeeAmount(uint height)
        {
            return GetSysFeeAmount(GetBlockHash(height));
        }





        public abstract long GetSysFeeAmount(UInt256 hash);





        public Transaction GetTransaction(UInt256 hash)
        {
            return GetTransaction(hash, out _);
        }






        public abstract Transaction GetTransaction(UInt256 hash, out int height);

        public abstract Dictionary<ushort, SpentCoin> GetUnclaimed(UInt256 hash);






        public abstract TransactionOutput GetUnspent(UInt256 hash, ushort index);

        public abstract IEnumerable<TransactionOutput> GetUnspent(UInt256 hash);




        public IEnumerable<VoteState> GetVotes()
        {
            return GetVotes(Enumerable.Empty<Transaction>());
        }

        public abstract IEnumerable<VoteState> GetVotes(IEnumerable<Transaction> others);





        public abstract bool IsDoubleSpend(Transaction tx);

        protected void OnNotify(Block block, NotifyEventArgs[] notifications)
        {
            Notify?.Invoke(this, new BlockNotifyEventArgs(block, notifications));
        }




        protected void OnPersistCompleted(Block block)
        {
            lock (_validators)
            {
                _validators.Clear();
            }
            PersistCompleted?.Invoke(this, block);
        }





        public static Blockchain RegisterBlockchain(Blockchain blockchain)
        {
            if (Default != null) Default.Dispose();
            Default = blockchain ?? throw new ArgumentNullException();
            return blockchain;
        }
    }
}

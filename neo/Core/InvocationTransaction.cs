using Neo.IO;
using Neo.IO.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Neo.Core
{
    public class InvocationTransaction : Transaction
    {
        public byte[] Script;
        public Fixed8 Fuel;

        public override int Size => base.Size + Script.GetVarSize();

        public override Fixed8 SystemFee => Fuel;

        public InvocationTransaction()
            : base(TransactionType.InvocationTransaction)
        {
        }

        protected override void DeserializeExclusiveData(BinaryReader reader)
        {
            if (Version > 1) throw new FormatException();
            Script = reader.ReadVarBytes(65536);
            if (Script.Length == 0) throw new FormatException();
            if (Version >= 1)
            {
                Fuel = reader.ReadSerializable<Fixed8>();
                if (Fuel < Fixed8.Zero) throw new FormatException();
            }
            else
            {
                Fuel = Fixed8.Zero;
            }
        }

        public static Fixed8 GetFuel(Fixed8 consumed)
        {
            Fixed8 fuel = consumed - Fixed8.FromDecimal(10);
            if (fuel <= Fixed8.Zero) return Fixed8.Zero;
            return fuel.Ceiling();
        }

        protected override void SerializeExclusiveData(BinaryWriter writer)
        {
            writer.WriteVarBytes(Script);
            if (Version >= 1)
                writer.Write(Fuel);
        }

        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["script"] = Script.ToHexString();
            json["fuel"] = Fuel.ToString();
            return json;
        }

        public override bool Verify(IEnumerable<Transaction> mempool)
        {
            if (Fuel.GetData() % 100000000 != 0) return false;
            return base.Verify(mempool);
        }
    }
}

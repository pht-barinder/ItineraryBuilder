using System;

namespace ItineraryBuilder.Repository.Dapper
{
    public class Identity
    {
        public long IDLong { get; set; }

        public ulong IDULong { get; set; }

        public int IDInt { get; set; }

        public uint IDUInt { get; set; }

        public UInt64 UInt64 { get; set; }
    }
}
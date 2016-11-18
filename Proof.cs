using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace khovratovich_equihash_cs
{
    public class Proof
    {
        public uint n;
        public uint k;
        public uint[] seed;
        public uint nonce;
        public List<uint> inputs;
    }
}

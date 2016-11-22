using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace khovratovich_equihash_cs
{
    public class Proof
    {
        public Proof(uint n, uint k, uint[] seed, uint nonce, List<uint> inputs)
        {
            this.n = n;
            this.k = k;
            this.seed = seed;
            this.nonce = nonce;
            this.inputs = inputs;
        }
        public uint n;
        public uint k;
        public uint[] seed;
        public uint nonce;
        public List<uint> inputs;
    }
}

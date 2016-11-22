using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace khovratovich_equihash_cs
{
    public class Seed : List<uint>
    {
        public Seed(uint x) : base()
        {
            for (int i = 0; i < Equihash.SEED_LENGTH; i++)
                this.Add(x);
        }
    }
}

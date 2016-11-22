using System;
using System.Collections.Generic;
using System.Data.HashFunction;
using System.IO;
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

        /*
        bool Proof::Test()
        {
            uint32_t input[SEED_LENGTH + 2];
            for (unsigned i = 0; i < SEED_LENGTH; ++i)
                input[i] = seed[i];
            input[SEED_LENGTH] = nonce;
            input[SEED_LENGTH + 1] = 0;
            uint32_t buf[MAX_N / 4];
            std::vector<uint32_t> blocks(k + 1, 0);
            for (unsigned i = 0; i < inputs.size(); ++i)
            {
                input[SEED_LENGTH + 1] = inputs[i];
                blake2b((uint8_t*)buf, &input, NULL, sizeof(buf), sizeof(input), 0);
                for (unsigned j = 0; j < (k + 1); ++j)
                {
                    //select j-th block of n/(k+1) bits
                    blocks[j] ^= buf[j] >> (32 - n / (k + 1));
                }
            }
            bool b = true;
            for (unsigned j = 0; j < (k + 1); ++j)
            {
                b &= (blocks[j] == 0);
            }
            if (b && inputs.size() != 0)
            {
                printf("Solution found:\n");
                for (unsigned i = 0; i < inputs.size(); ++i)
                {
                    printf(" %x ", inputs[i]);
                }
                printf("\n");
            }
            return b;
        }
        */
        public bool Test()
        {
            uint[] input = new uint[Equihash.SEED_LENGTH + 2];
            for (uint i = 0; i < Equihash.SEED_LENGTH; ++i)
                input[i] = seed[i];
            input[Equihash.SEED_LENGTH] = nonce;
            input[Equihash.SEED_LENGTH + 1] = 0;
            uint[] buf = new uint[Equihash.MAX_N / 4];
            List<uint> blocks = new List<uint>(); for(int i = 0; i < k + 1; i++) { blocks.Add(0); }
            for (int i = 0; i < inputs.Count; ++i)
            {
                input[Equihash.SEED_LENGTH + 1] = inputs[i];
                byte[] inputBytes;
                using (MemoryStream ms = new MemoryStream())
                {
                    for (int x = 0; x < input.Length; x++)
                    {
                        ms.Write(BitConverter.GetBytes(input[x]), 0, 4);
                    }

                    inputBytes = ms.ToArray();
                }

                Blake2B blake2b = new Blake2B(256);
                byte[] result = blake2b.ComputeHash(inputBytes);
                for (int x = 0; x < buf.Length; x++)
                {
                    buf[x] = BitConverter.ToUInt32(result, x * 4);
                }
                for (int j = 0; j < (k + 1); ++j)
                {
                    //select j-th block of n/(k+1) bits
                    blocks[j] = blocks[j] ^ (buf[j] >> (int)(32 - n / (k + 1)));
                }
            }
            bool b = true;
            for (int j = 0; j < (k + 1); ++j)
            {
                b = (blocks[j] == 0);
            }
            if (b && inputs.Count != 0)
            {
                Console.WriteLine("Solution found:");
                for (int i = 0; i < inputs.Count; ++i)
                {
                    Console.Write($" {inputs[i]} ");
                }
                Console.WriteLine("");
            }
            return b;
        }
    }
}

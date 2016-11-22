using System;
using System.Collections.Generic;
using System.Data.HashFunction;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using Fork = System.Tuple<uint, uint>;

namespace khovratovich_equihash_cs
{
    public class Equihash
    {
        public Equihash(uint n, uint k, Seed seed)
        {
            this.n = n;
            this.k = k;
            this.seed = seed.ToArray();
        }

        public const int SEED_LENGTH = 4; //Length of seed in dwords ;
        public const int NONCE_LENGTH = 24; //Length of nonce in bytes;
        public const int MAX_NONCE = 0xFFFFF;
        public const int MAX_N = 32; //Max length of n in bytes, should not exceed 32
        public const int LIST_LENGTH = 5;
        public const uint FORK_MULTIPLIER = 3; //Maximum collision factor

        public uint n;
        public uint k;

        private List<Proof> solutions = new List<Proof>();
        private List<List<Fork>> forks = new List<List<Fork>>();

        uint[] default_tuple;
        uint[,] def_tuples;
        uint[,,] tupleList;
        uint[] filledList;

        private static uint REFERENCE_BLOCK_INDEX = 0;

        private uint[] seed;
        private uint nonce;




        /*
        void Equihash::InitializeMemory()
        {
            uint32_t  tuple_n = ((uint32_t)1) << (n / (k + 1));
            Tuple default_tuple(k); // k blocks to store (one left for index)
            std::vector<Tuple> def_tuples(LIST_LENGTH, default_tuple);
            tupleList = std::vector<std::vector<Tuple>>(tuple_n, def_tuples);

            filledList= std::vector<unsigned>(tuple_n, 0);

            solutions.resize(0);
            forks.resize(0);
        }
        */
        public void InitializeMemory()
        {
            uint tuple_n = ((uint)1) << (int)(n / (k + 1));

            /*
                Instead of C++ vector's I have decided to use arrays in this translation.

                I have also changed the way the memory structures are represented as follows:

                The original C++ 'Tuple' class:
            
                class Tuple {
                public:
                    std::vector<uint32_t> blocks;
                    uint32_t reference;
                };

                ~ has a uin32 list "blocks" and another uint32 "reference".
                
                This translation will not reference a "Tuple" class, but instead
                it will treat all "Tuple's" as uint32 arrays with an additional item for storing the 'reference' block.
                The reference block's index will be stored in a variable REFERENCE_BLOCK_INDEX;

            */
            default_tuple = new uint[k + 1];
            def_tuples = new uint[Equihash.LIST_LENGTH, k + 1];
            tupleList = new uint[tuple_n, Equihash.LIST_LENGTH, k + 1];
            REFERENCE_BLOCK_INDEX = k;

            filledList = new uint[tuple_n];

            solutions.Clear();
            forks.Clear();
        }




        /*
        void Equihash::PrintTuples(FILE* fp) {
            unsigned count = 0;
            for (unsigned i = 0; i < tupleList.size(); ++i) {
                for (unsigned m = 0; m < filledList[i]; ++m) {
                    fprintf(fp, "[%d][%d]:", i,m);
                    for (unsigned j = 0; j < tupleList[i][m].blocks.size(); ++j)
                        fprintf(fp, " %x ", tupleList[i][m].blocks[j]);
                    fprintf(fp, " || %x", tupleList[i][m].reference);
                    fprintf(fp, " |||| ");
                }
                count += filledList[i];
                fprintf(fp, "\n");
            }
            fprintf(fp, "TOTAL: %d elements printed", count);
        }
        */
        public void PrintTuples(StreamWriter sw)
        {
            #if DEBUG
            uint count = 0;
            for (uint i = 0; i <= tupleList.GetUpperBound(0); ++i) {
                for (uint m = 0; m < filledList[i]; ++m) {
                    sw.Write($"[{i.ToString().PadRight(7, ' ')}][{m.ToString()}]:");
                    for (uint j = 0; j <= tupleList.GetUpperBound(2) - 1; ++j)
                        sw.Write($" {tupleList[i, m, j]} ");
                    sw.Write($" || {tupleList[i,m, REFERENCE_BLOCK_INDEX]}");
                    sw.Write(" |||| ");
                }
                count += filledList[i];
                sw.WriteLine();
            }
            sw.Write($"TOTAL: {count} elements printed");
            #endif
        }




        /*
        void Equihash::FillMemory(uint32_t length) //works for k<=7
        {
            uint32_t input[SEED_LENGTH + 2];
            for (unsigned i = 0; i < SEED_LENGTH; ++i)
                input[i] = seed[i];
            input[SEED_LENGTH] = nonce;
            input[SEED_LENGTH + 1] = 0;
            uint32_t buf[MAX_N / 4];
            for (unsigned i = 0; i < length; ++i, ++input[SEED_LENGTH + 1]) {
                blake2b((uint8_t*)buf, &input, NULL, sizeof(buf), sizeof(input), 0);
                uint32_t index = buf[0] >> (32 - n / (k + 1));
                unsigned count = filledList[index];
                if (count < LIST_LENGTH) {
                    for (unsigned j = 1; j < (k + 1); ++j) {
                        //select j-th block of n/(k+1) bits
                        tupleList[index][count].blocks[j - 1] = buf[j] >> (32 - n / (k + 1));
                    }
                    tupleList[index][count].reference = i;
                    filledList[index]++;
                }
            }
        }
        */
        public void FillMemory(ulong length) //works for k<=7
        {
            uint[] input = new uint[Equihash.SEED_LENGTH + 2];
            for (uint i = 0; i < SEED_LENGTH; ++i)
                input[i] = seed[i];
            input[SEED_LENGTH] = nonce;
            input[SEED_LENGTH + 1] = 0;
            uint[] buf = new uint[Equihash.MAX_N / 4];
            for (uint i = 0; i < length; ++i, ++input[SEED_LENGTH + 1])
            {
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

                uint index = buf[0] >> (int)(32 - n / (k + 1));
                uint count = filledList[index];
                if (count < LIST_LENGTH)
                {
                    for (uint j = 1; j < (k + 1); ++j)
                    {
                        //select j-th block of n/(k+1) bits
                        tupleList[index, count, j - 1] = buf[j] >> (int)(32 - n / (k + 1));
                    }
                    tupleList[index, count, REFERENCE_BLOCK_INDEX] = i;
                    filledList[index]++;
                }
            }
        }




        /*
        std::vector<Input> Equihash::ResolveTreeByLevel(Fork fork, unsigned level) {
            if (level == 0)
                return std::vector<Input>{fork.ref1, fork.ref2};
            auto v1 = ResolveTreeByLevel(forks[level - 1][fork.ref1], level - 1);
            auto v2 = ResolveTreeByLevel(forks[level - 1][fork.ref2], level - 1);
            v1.insert(v1.end(), v2.begin(), v2.end());
            return v1;
        }
        */
        public List<uint> ResolveTreeByLevel(Fork fork, uint level) {
            if (level == 0)
                return new List<uint> { fork.Item1, fork.Item2 };
            List<uint> v1 = ResolveTreeByLevel(forks[(int)level - 1][(int)fork.Item1], level - 1);
            List<uint> v2 = ResolveTreeByLevel(forks[(int)level - 1][(int)fork.Item2], level - 1);
            v1.AddRange(v2);
            return v1;
        }
        



        /*
        std::vector<Input> Equihash::ResolveTree(Fork fork) {
            return ResolveTreeByLevel(fork, forks.size());
        }
        */
        public List<uint> ResolveTree(Fork fork) {
            return ResolveTreeByLevel(fork, (uint)forks.Count);
        }




        /*
        void Equihash::ResolveCollisions(bool store) {
            const unsigned tableLength = tupleList.size();  //number of rows in the hashtable 
            const unsigned maxNewCollisions = tupleList.size()*FORK_MULTIPLIER;  //max number of collisions to be found
            const unsigned newBlocks = tupleList[0][0].blocks.size() - 1;// number of blocks in the future collisions
            std::vector<Fork> newForks(maxNewCollisions); //list of forks created at this step
            auto tableRow = vector<Tuple>(LIST_LENGTH, Tuple(newBlocks)); //Row in the hash table
            vector<vector<Tuple>> collisionList(tableLength,tableRow);
            std::vector<unsigned> newFilledList(tableLength,0);  //number of entries in rows
            uint32_t newColls = 0; //collision counter
            for (unsigned i = 0; i < tableLength; ++i) {        
                for (unsigned j = 0; j < filledList[i]; ++j)        {
                    for (unsigned m = j + 1; m < filledList[i]; ++m) {   //Collision
                        //New index
                        uint32_t newIndex = tupleList[i][j].blocks[0] ^ tupleList[i][m].blocks[0];
                        Fork newFork = Fork(tupleList[i][j].reference, tupleList[i][m].reference);
                        //Check if we get a solution
                        if (store) {  //last step
                            if (newIndex == 0) {//Solution
                                std::vector<Input> solution_inputs = ResolveTree(newFork);
                                solutions.push_back(Proof(n, k, seed, nonce, solution_inputs));
                            }
                        }
                        else {         //Resolve
                            if (newFilledList[newIndex] < LIST_LENGTH && newColls < maxNewCollisions) {
                                for (unsigned l = 0; l < newBlocks; ++l) {
                                    collisionList[newIndex][newFilledList[newIndex]].blocks[l] 
                                        = tupleList[i][j].blocks[l+1] ^ tupleList[i][m].blocks[l+1];
                                }
                                newForks[newColls] = newFork;
                                collisionList[newIndex][newFilledList[newIndex]].reference = newColls;
                                newFilledList[newIndex]++;
                                newColls++;
                            }//end of adding collision
                        }
                    }
                }//end of collision for i
            }
            forks.push_back(newForks);
            std::swap(tupleList, collisionList);
            std::swap(filledList, newFilledList);
        }
        */
        public void ResolveCollisions(bool store){
            uint tableLength = Convert.ToUInt32(tupleList.GetLongLength(0));
            uint maxNewCollisions = tableLength * Equihash.FORK_MULTIPLIER;
            uint newBlocks = (uint)tupleList.GetLongLength(2) - 1;
            uint NEW_REFERENCE_BLOCK_INDEX = REFERENCE_BLOCK_INDEX - 1; // Decrease the reference to the last block we are using to store reference.
            Fork[] newForks = new Fork[maxNewCollisions];
            uint[,] tableRow = new uint[Equihash.LIST_LENGTH, newBlocks];
            uint[,,] collisionList = new uint[tableLength, Equihash.LIST_LENGTH, newBlocks];
            uint[] newFilledList = new uint[tableLength];
            uint newColls = 0;
            for (uint i = 0; i < tableLength; ++i) {
                for (uint j = 0; j < filledList[i]; ++j) {
                    for (uint m = j + 1; m < filledList[i]; ++m) {
                        uint newIndex = tupleList[i, j, 0] ^ tupleList[i, m, 0];
                        Fork newFork = new Fork(tupleList[i,j,REFERENCE_BLOCK_INDEX], tupleList[i,m,REFERENCE_BLOCK_INDEX]);
                        //Check if we get a solution
                        if (store)
                        {  //last step
                            if (newIndex == 0)
                            {//Solution
                                List<uint> solution_inputs = ResolveTree(newFork);
                                solutions.Add(new Proof(n, k, seed, nonce, solution_inputs));
                            }
                        }
                        else
                        {         //Resolve
                            if (newFilledList[newIndex] < LIST_LENGTH && newColls < maxNewCollisions)
                            {
                                for (uint l = 0; l < newBlocks - 1; ++l) // <-- newBlocks - 1 because of the extra 'referenceBlock'
                                {
                                    collisionList[newIndex,newFilledList[newIndex],l]
                                        = tupleList[i, j, l + 1] ^ tupleList[i, m, l + 1];
                                }
                                newForks[newColls] = newFork;
                                collisionList[newIndex, newFilledList[newIndex], NEW_REFERENCE_BLOCK_INDEX] = newColls;
                                newFilledList[newIndex]++;
                                newColls++;
                            }//end of adding collision
                        }
                    }
                }//end of collision for i
            }
            forks.Add(new List<Fork>(newForks));
            tupleList = collisionList;
            filledList = newFilledList;
            REFERENCE_BLOCK_INDEX = NEW_REFERENCE_BLOCK_INDEX;
        }




        /*
        Proof Equihash::FindProof(){
            FILE* fp = fopen("proof.log", "w+");
            fclose(fp);
            this->nonce = 1;
            while (nonce < MAX_NONCE) {
                nonce++;
                printf("Testing nonce %d\n", nonce);
                uint64_t start_cycles = rdtsc();
                //InitializeMemory(); //allocate
                FillMemory(4UL << (n / (k + 1)-1));   //fill with hashes
                uint64_t fill_end = rdtsc();
                printf("Filling %2.2f  Mcycles \n", (double)(fill_end - start_cycles) / (1UL << 20));
                / * fp = fopen("proof.log", "a+");
                fprintf(fp, "\n===MEMORY FILLED:\n");
                PrintTuples(fp);
                fclose(fp); * /
                for (unsigned i = 1; i <= k; ++i) {
                    uint64_t resolve_start = rdtsc();
                    bool to_store = (i == k);
                    ResolveCollisions(to_store); //XOR collisions, concatenate indices and shift
                    uint64_t resolve_end = rdtsc();
                    printf("Resolving %2.2f  Mcycles \n", (double)(resolve_end - resolve_start) / (1UL << 20));
                   / * fp = fopen("proof.log", "a+");
                    fprintf(fp, "\n===RESOLVED AFTER STEP %d:\n", i);
                    PrintTuples(fp);
                    fclose(fp); * /
                }
                uint64_t stop_cycles = rdtsc();

                double mcycles_d = (double)(stop_cycles - start_cycles) / (1UL << 20);
                uint32_t kbytes = (tupleList.size() * LIST_LENGTH * k * sizeof(uint32_t)) / (1UL << 10);
                printf("Time spent for n=%d k=%d  %d KiB: %2.2f  Mcycles \n",
                    n, k, kbytes,
                    mcycles_d);

                //Duplicate check
                for (unsigned i = 0; i<solutions.size(); ++i) {
                    auto vec = solutions[i].inputs;
                    std::sort(vec.begin(), vec.end());
                    bool dup = false;
                    for (unsigned k = 0; k<vec.size() - 1; ++k) {
                        if (vec[k] == vec[k + 1])
                            dup = true;
                    }
                    if (!dup)
                        return solutions[i];
                }
            }
            return Proof(n, k, seed, nonce, std::vector<uint32_t>());
        }
        */
        public Proof FindProof()
        {
            FileStream fp = File.Open("proof.log", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read); StreamWriter sw = new StreamWriter(fp);
            nonce = 1;
            while (nonce < MAX_NONCE)
            {
                nonce++;
                Console.WriteLine($"Testing nonce {nonce}");
                long start_cycles = Stopwatch.GetTimestamp();
                InitializeMemory();
                FillMemory(4UL << (int)n / ((int)k + 1) - 1);
                long fill_end = Stopwatch.GetTimestamp();
                // printf("Filling %2.2f  Mcycles \n", (double)(fill_end - start_cycles) / (1UL << 20));
                Console.WriteLine($"Filling {(double)(fill_end - start_cycles) / (1UL << 20)} Ticks");
                Console.WriteLine($"Filling {TimeSpan.FromSeconds((fill_end - start_cycles) / Stopwatch.Frequency).ToString()}");
#if DEBUG
                sw.WriteLine($"\r\n===MEMORY FILLED:");
                PrintTuples(sw);
#endif
                for (uint i = 1; i <= k; ++i)
                {
                    long resolve_start = Stopwatch.GetTimestamp();
                    bool to_store = (i == k);
                    ResolveCollisions(to_store); //XOR collisions, concatenate indices and shift
                    long resolve_end = Stopwatch.GetTimestamp();
                    Console.WriteLine($"Filling {(double)(resolve_end - resolve_start)} Ticks \r\n");
                    Console.WriteLine($"Filling {TimeSpan.FromSeconds((resolve_end - resolve_start) / Stopwatch.Frequency).ToString()}");
#if DEBUG
                    sw.WriteLine($"\r\n===RESOLVED AFTER STEP {i}:");
                    PrintTuples(sw);
#endif
                }
                long stop_cycles = Stopwatch.GetTimestamp();

                double ticks = stop_cycles - start_cycles;
                uint kbytes = ((uint)tupleList.GetLongLength(1) * Equihash.LIST_LENGTH * k * sizeof(uint)) / (int)(1UL << 10);
                Console.WriteLine($"Time spent for n={n} k={k}  {kbytes} KiB: {ticks} Ticks");
                Console.WriteLine($"Time spent for n={n} k={k}  {kbytes} KiB: {TimeSpan.FromSeconds(ticks / Stopwatch.Frequency).ToString()}");

                //Duplicate check
                for (int i = 0; i < solutions.Count; ++i)
                {
                    List<uint> vec = solutions[i].inputs;
                    vec.Sort();
                    bool dup = false;
                    for (int k = 0; k < vec.Count - 1; ++k)
                    {
                        if (vec[k] == vec[k + 1])
                            dup = true;
                    }
                    if (!dup)
                        return solutions[i];
                }
            }
            return new Proof(n, k, seed, nonce, new List<uint>());
        }




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
            throw new NotImplementedException();
        }

    }
}
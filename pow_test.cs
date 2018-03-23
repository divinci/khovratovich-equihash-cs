namespace khovratovich_equihash_cs
{
    using System;
    using System.Reflection;

    class pow_test
    {
        /*
        void TestEquihash(unsigned n, unsigned k, Seed seed)
        {
            Equihash equihash(n, k, seed);
            Proof p = equihash.FindProof();
            p.Test();
        }
        */
        public static void TestEquihash(uint n, uint k, Seed seed)
        {
            Equihash equihash = new Equihash(n, k, seed);
            Proof p = equihash.FindProof();
            p.Test();
        }





        /*
        static void fatal(const char* error) {
            fprintf(stderr, "Error: %s\n", error);
            exit(1);
        }
        */

        public static int fatal(string error) {
            Console.WriteLine($"Error: {error}");
            return 1;
        }





        /*
        static void usage(const char* cmd)
        {
            printf("Usage: %s  [-n N] [-k K] "

                "[-s S]\n",
                cmd);
            printf("Parameters:\n");
            printf("\t-n N \t\tSets the tuple length of iterations to N\n");
            printf("\t-k K\t\tSets the number of steps to K \n");
            printf("\t-s S\t\tSets seed  to S\n");
        }
        */
        public static void usage(string cmd)
        {
            Console.WriteLine($"Usage: {cmd}  [-n N] [-k K] [-s S]");
            Console.WriteLine("Parameters:\n");
            Console.WriteLine("\t-n N \t\tSets the tuple length of iterations to N");
            Console.WriteLine("\t-k K\t\tSets the number of steps to K");
            Console.WriteLine("\t-s S\t\tSets seed  to S");
        }





        /*
        int main(int argc, char* argv[])
        {
            uint32_t n = 0, k = 0;
            Seed seed;
            if (argc < 2)
            {
                usage(argv[0]);
                return 1;
            }

            / * parse options * /
            for (int i = 1; i < argc; i++)
            {
                const char* a = argv[i];
                unsigned long input = 0;
                if (!strcmp(a, "-n"))
                {
                    if (i < argc - 1)
                    {
                        i++;
                        input = strtoul(argv[i], NULL, 10);
                        if (input == 0 ||
                            input > 255)
                        {
                            fatal("bad numeric input for -n");
                        }
                        n = input;
                        continue;
                    }
                    else
                    {
                        fatal("missing -n argument");
                    }
                }
                else if (!strcmp(a, "-k"))
                {
                    if (i < argc - 1)
                    {
                        i++;
                        input = strtoul(argv[i], NULL, 10);
                        if (input == 0 ||
                            input > 20)
                        {
                            fatal("bad numeric input for -k");
                        }
                        k = input;
                        continue;
                    }
                    else
                    {
                        fatal("missing -k argument");
                    }
                }
                if (!strcmp(a, "-s"))
                {
                    if (i < argc - 1)
                    {
                        i++;
                        input = strtoul(argv[i], NULL, 10);
                        if (input == 0 ||
                            input > 0xFFFFFF)
                        {
                            fatal("bad numeric input for -s");
                        }
                        seed = Seed(input);
                        continue;
                    }
                    else
                    {
                        fatal("missing -s argument");
                    }
                }
            }
            printf("N:\t%" PRIu32 " \n", n);
            printf("K:\t%" PRIu32 " \n", k);
            printf("SEED: ");
            for (unsigned i = 0; i < SEED_LENGTH; ++i)
            {
                printf(" \t%" PRIu32 " ", seed[i]);
            }
            printf("\n");
            printf("Memory:\t\t%" PRIu64 "KiB\n", ((((uint32_t)1) << (n / (k + 1))) * LIST_LENGTH * k * sizeof(uint32_t)) / (1 << 10));
            TestEquihash(n, k, seed);

            return 0;
        }
        */

        //  try
        //  equihash -n 120 -k 5 -s 3
        static int Main(string[] argc)
        {
            uint n = 0, k = 0;
            Seed seed = new Seed(0);
            if (argc.Length < 6)
            {
                usage(Assembly.GetExecutingAssembly().GetName().Name);
                return 1;
            }

            /* parse options */
            for (int i = 0; i < argc.Length; i++)
            {
                if (argc[i] == "-n")
                {
                    if (i < argc.Length - 1)
                    {
                        i++;
                        uint input;
                        if (!uint.TryParse(argc[i], out input) || input == 0 || input > 255)
                        {
                            return fatal("bad numeric input for -n");
                        }
                        n = input;
                        continue;
                    }
                    else
                    {
                        return fatal("missing -n argument");
                    }
                }
                else if (argc[i] == "-k")
                {
                    if (i < argc.Length - 1)
                    {
                        i++;
                        uint input;
                        if (!uint.TryParse(argc[i], out input) || input == 0 || input > 20)
                        {
                            fatal("bad numeric input for -k");
                        }
                        k = input;
                        continue;
                    }
                    else
                    {
                        fatal("missing -k argument");
                    }
                }
                if (argc[i] == "-s")
                {
                    if (i < argc.Length - 1)
                    {
                        i++;
                        uint input;
                        if (!uint.TryParse(argc[i], out input) || input == 0 || input > 0xFFFFFF)
                        {
                            fatal("bad numeric input for -s");
                        }
                        seed = new Seed(input);
                        continue;
                    }
                    else
                    {
                        fatal("missing -s argument");
                    }
                }
            }
            Console.WriteLine($"N:\t{n}");
            Console.WriteLine($"K:\t{k}");
            Console.Write("SEED: ");
            for (int i = 0; i < Equihash.SEED_LENGTH; ++i)
            {
                Console.Write($" \t{seed[i]} ");
            }
            Console.WriteLine();
            Console.WriteLine($"Memory:\t\t{((((uint)1) << (int)(n / (k + 1))) * Equihash.LIST_LENGTH * k * sizeof(uint)) / (1 << 10)}KiB");
            TestEquihash(n, k, seed);

            return 0;
        }
    }
}
using System;
using System.Numerics;
using System.Security.Cryptography;

class Program
{
    static void Main()
    {
        Console.WriteLine("Diffie-Hellman Key Exchange (interactive)");
        Console.WriteLine();
        Console.WriteLine("Explanations:");
        Console.WriteLine(" - p : a prime number agreed publicly (modulus)");
        Console.WriteLine(" - g : a base (1 < g < p), also public");
        Console.WriteLine(" - a : Alice's private secret (1 < a < p)");
        Console.WriteLine(" - b : Bob's private secret (1 < b < p)");
        Console.WriteLine();
        Console.WriteLine("You will input values for p, g, a and b. The program will show intermediate results:");
        Console.WriteLine(" - ga mod p (sent from Alice to Bob)");
        Console.WriteLine(" - gb mod p (sent from Bob to Alice)");
        Console.WriteLine(" - shared secret S computed by both sides");
        Console.WriteLine();

        BigInteger p = ReadBigInteger("Enter prime p (min 3)", min: 3);
        if (!IsProbablyPrime(p))
        {
            Console.WriteLine("Warning: The supplied p does not appear to be prime (or primality check inconclusive).");
            Console.WriteLine("Diffie-Hellman requires p to be prime for standard security properties.");
            Console.WriteLine();
        }
        BigInteger g = ReadBigInteger($"Enter base g (1 < g < p) (p = {p})", min: 2, max: p - 1);
        BigInteger a = ReadBigInteger($"Enter Alice's private a (1 < a < p) (p = {p})", min: 2, max: p - 1);
        BigInteger b = ReadBigInteger($"Enter Bob's private b (1 < b < p) (p = {p})", min: 2, max: p - 1);

        BigInteger ga = BigInteger.ModPow(g, a, p);
        BigInteger gb = BigInteger.ModPow(g, b, p);

        Console.WriteLine("Intermediate values:");
        Console.WriteLine($" - Alice computes ga mod p = g^a mod p = {g}^{a} mod {p} = {ga}");
        Console.WriteLine($" - Bob computes gb mod p = g^b mod p = {g}^{b} mod {p} = {gb}");
        Console.WriteLine();

        BigInteger S_alice = BigInteger.ModPow(gb, a, p); // (g^b)^a mod p
        BigInteger S_bob = BigInteger.ModPow(ga, b, p);   // (g^a)^b mod p

        Console.WriteLine("Shared secret computations:");
        Console.WriteLine($" - Alice computes S = (gb)^a mod p = {gb}^{a} mod {p} = {S_alice}");
        Console.WriteLine($" -  Bob  computes S = (ga)^b mod p = {ga}^{b} mod {p} = {S_bob}");
        Console.WriteLine();

        if (S_alice == S_bob)
            Console.WriteLine($"Success: Both computed the same shared secret S = {S_alice}");
        else
            Console.WriteLine("Error: Alice and Bob computed different secrets (inputs may be invalid).");

        Console.WriteLine();
        Console.WriteLine("Note: For real-world use, use large primes (hundreds or thousands of bits) and validated parameters.");
    }

    static BigInteger ReadBigInteger(string prompt, BigInteger? min = null, BigInteger? max = null)
    {
        while (true)
        {
            Console.Write($"{prompt}: ");
            string? s = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(s))
            {
                Console.WriteLine("Input required.");
                continue;
            }

            if (s.Equals("random", StringComparison.OrdinalIgnoreCase) && min.HasValue && max.HasValue)
            {
                return RandomBigInteger(min.Value, max.Value);
            }

            if (BigInteger.TryParse(s.Trim(), out BigInteger value))
            {
                if (min.HasValue && value < min.Value)
                {
                    Console.WriteLine($"Value must be >= {min.Value}.");
                    continue;
                }

                if (max.HasValue && value > max.Value)
                {
                    Console.WriteLine($"Value must be <= {max.Value}.");
                    continue;
                }

                return value;
            }

            Console.WriteLine("Invalid integer, try again. To generate a random value in the allowed range type 'random'.");
        }
    }

    // static method
    static BigInteger RandomBigInteger(BigInteger minInclusive, BigInteger maxInclusive)
    {
        if (minInclusive > maxInclusive) throw new ArgumentException("min > max");
        BigInteger range = maxInclusive - minInclusive + 1;
        int bytes = range.ToByteArray().Length;
        byte[] buf = new byte[bytes];
        BigInteger r;
        using var rng = RandomNumberGenerator.Create();
        do
        {
            rng.GetBytes(buf);
            buf[^1] &= 0x7F; // force positive
            r = new BigInteger(buf);
        } while (r >= range);
        return minInclusive + r;
    }

    // Miller-Rabin probable prime test for BigInteger to check for primality of p. Not deterministic for large values, but good enough for demonstration.
    static bool IsProbablyPrime(BigInteger value, int witnesses = 6)
    {
        if (value <= 1) return false;
        if (value <= 3) return true;
        if (value % 2 == 0) return false;

        // small primes quick check
        int[] smallPrimes = { 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37 };
        foreach (int p in smallPrimes)
        {
            if (value == p) return true;
            if (value % p == 0) return false;
        }

        BigInteger d = value - 1;
        int s = 0;
        while (d % 2 == 0)
        {
            d /= 2;
            s++;
        }

        // Deterministic bases for 64-bit values
        BigInteger[] bases = { 2, 3, 5, 7, 11, 13 };
        int used = 0;
        foreach (var a in bases)
        {
            if (a >= value) break;
            if (!MillerRabinWitness(a, value, d, s)) return false;
            used++;
            if (used >= witnesses) break;
        }

        return true;
    }

    static bool MillerRabinWitness(BigInteger a, BigInteger n, BigInteger d, int s)
    {
        BigInteger x = BigInteger.ModPow(a, d, n);
        if (x == 1 || x == n - 1) return true;

        for (int r = 1; r < s; r++)
        {
            x = BigInteger.ModPow(x, 2, n);
            if (x == n - 1) return true;
            if (x == 1) return false;
        }
        return false;
    }
}
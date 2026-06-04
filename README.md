# Diffie-Hellman Key Exchange — Interactive Demo

**********************************************
Software:	&emsp;	C# 14 / .NET 10

Version:	&emsp;  1.0

Date: 	&emsp;		Jun 06, 2026

Author:	&emsp;		Dirk Mueller
**********************************************

## Purpose

In setting up a secure communication protocol between two endpoints, such as in TLS, it is required to negotiate a shared secret with an as-yet-unconfirmed opposite party — without ever transmitting that secret over the wire.

This program demonstrates the method developed by Whitfield Diffie and Martin Hellman (1976): two parties each hold a private exponent and exchange only public values. Because discrete logarithms are computationally hard, an eavesdropper who sees all exchanged values still cannot recover the secret.

The program is interactive and educational. It prints every intermediate value so the mathematics can be followed step by step.

## High-level architecture

Conceptually, the flow is:

```
User enters prime p
│
▼
Primality check on p  (Miller-Rabin; warns if p is not prime)
│
▼
User enters public base g  (1 < g < p)
│
▼
User enters Alice's private exponent a  (1 < a < p)
│
▼
User enters Bob's private exponent b  (1 < b < p)
│
▼
Compute public values:  ga = g^a mod p  |  gb = g^b mod p
│
▼
Compute shared secret:  S = gb^a mod p  =  ga^b mod p
│
▼
Result printed in the terminal
```

## Components

| Method | Role |
|---|---|
| `Main()` | Drives the interactive session; collects inputs and prints all intermediate results |
| `ReadBigInteger()` | Validated console input for arbitrary-precision integers; accepts `"random"` to auto-generate a value in range |
| `RandomBigInteger()` | Cryptographically secure uniform random `BigInteger` in `[min, max]` using `RandomNumberGenerator` |
| `IsProbablyPrime()` | Miller-Rabin primality test — deterministic for values below ~82 bits, probabilistic (≤ 4⁻²⁰ error) above |
| `MillerRabinWitness()` | Single Miller-Rabin witness check (core of the primality test) |

## Implementation

The Diffie-Hellman protocol relies on modular exponentiation: given public values `p`, `g`, `ga = g^a mod p`, and `gb = g^b mod p`, it is computationally infeasible to recover `a` or `b` (the discrete logarithm problem). Both parties independently compute the same shared secret:

```
Alice:  S = gb^a mod p  =  g^(ab) mod p
Bob:    S = ga^b mod p  =  g^(ab) mod p
```

`BigInteger.ModPow` from `System.Numerics` is used for all modular exponentiation. Random values are generated with `System.Security.Cryptography.RandomNumberGenerator` to ensure cryptographic quality.

The primality test uses the Miller-Rabin algorithm. For `p < 3,317,044,064,679,887,385,961,981` (~82 bits) the fixed witness set `{2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41}` gives a provably correct result. For larger values, 20 additional random witnesses are used, bounding the false-prime probability at 4⁻²⁰ ≈ 10⁻¹².

## Code structure

All logic is contained in a single file, `Program.cs`:

```
Program.cs
├── Main()                  — entry point / interactive flow
├── ReadBigInteger()        — input helper
├── RandomBigInteger()      — CSPRNG-backed BigInteger sampling
├── IsProbablyPrime()       — Miller-Rabin primality test
└── MillerRabinWitness()    — single-witness Miller-Rabin round
```

## Usage

```
dotnet run
```

At each prompt you can either type an integer or type `random` (for `g`, `a`, and `b`) to let the program pick a cryptographically random value in the valid range.

**Example session (small parameters for readability):**

```
Enter prime p (min 3): 23
Enter base g (1 < g < p) (p = 23): 5
Enter Alice's private a (1 < a < p) (p = 23): random
Enter Bob's private b (1 < b < p) (p = 23): random

Intermediate values:
 - Alice computes ga mod p = g^a mod p = 5^6 mod 23 = 8
 - Bob computes gb mod p = g^b mod p = 5^15 mod 23 = 19

Shared secret computations:
 - Alice computes S = (gb)^a mod p = 19^6 mod 23 = 2
 -  Bob  computes S = (ga)^b mod p = 8^15 mod 23 = 2

Success: Both computed the same shared secret S = 2
```

> **Note:** For real-world use, `p` must be a safe prime of at least 2048 bits and `g` must be a verified generator. This program is for educational purposes only.

## References

[1] W. Diffie and M. Hellman, "New Directions in Cryptography," *IEEE Transactions on Information Theory*, vol. 22, no. 6, pp. 644–654, Nov. 1976.

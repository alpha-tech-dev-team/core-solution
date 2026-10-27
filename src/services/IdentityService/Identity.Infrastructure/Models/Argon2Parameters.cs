namespace Identity.Infrastructure.Models;

internal readonly record struct Argon2Parameters(int MemorySize, int Iterations, int Parallelism);

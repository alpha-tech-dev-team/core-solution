namespace Identity.Infrastructure.Security;

public static class PasswordHasherConstants
{
    public const string Format = "${0}${1}$m={2},t={3},p={4}${5}${6}";
    public const string Algorithm = "argon2id";
    public const string Version = "v=19";

    public const char SectionDelimiter = '$';
    public const char ParameterDelimiter = ',';
    public const string MemoryPrefix = "m=";
    public const string IterationsPrefix = "t=";
    public const string ParallelismPrefix = "p=";

    public const int ExpectedSectionCount = 6;
    public const int AlgorithmSectionIndex = 1;
    public const int VersionSectionIndex = 2;
    public const int ParametersSectionIndex = 3;
    public const int SaltSectionIndex = 4;
    public const int HashSectionIndex = 5;
    public const int ExpectedParameterCount = 3;

    public const int MinSaltSize = 16;
    public const int MaxSaltSize = 64;
    public const int MinHashSize = 16;
    public const int MaxHashSize = 64;
    public const int MinMemorySize = 1;
    public const int MaxMemorySize = 256 * 1024;
    public const int MinIterations = 1;
    public const int MaxIterations = 10;
    public const int MinParallelism = 1;
    public const int MaxParallelism = 8;
}

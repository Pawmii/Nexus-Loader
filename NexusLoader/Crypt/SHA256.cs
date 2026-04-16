using System;

namespace NexusLoader.Crypt;

public static class SHA256
{
    public static string Hash(byte[] data) 
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();

        return Convert.ToBase64String(sha256.ComputeHash(data));
    }
}
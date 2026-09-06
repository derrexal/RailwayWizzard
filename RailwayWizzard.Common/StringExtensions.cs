using System.Security.Cryptography;
using System.Text;

namespace RailwayWizzard.Common;

public static class StringExtensions
{
    public static byte[] ToSha256Hash(this string input)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes(input));
    }
}
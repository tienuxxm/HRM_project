using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Domain.Shared;
using Newtonsoft.Json.Serialization;

namespace Application.Extensions;

public static class Extention
{
    public static String HmacSHA512(string key, String inputData)
    {
        var hash = new StringBuilder();
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
        using (var hmac = new HMACSHA512(keyBytes))
        {
            byte[] hashValue = hmac.ComputeHash(inputBytes);
            foreach (var theByte in hashValue)
            {
                hash.Append(theByte.ToString("x2"));
            }
        }

        return hash.ToString();
    }

    public static string? ToSnakeCase(this string? str) => str is null
        ? null
        : new DefaultContractResolver() { NamingStrategy = new SnakeCaseNamingStrategy() }.GetResolvedPropertyName(str);

    public static string ToVndFormat(this Money money) =>
        money.Amount.ToString("#,###", CultureInfo.GetCultureInfo("vi-VN").NumberFormat) + " " + "VND";

    public static List<Guid> ExtractUUIDs(this string text)
    {
        var guids = new List<Guid>();
        var pattern = @"[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}";
        MatchCollection matches = Regex.Matches(text, pattern);

        foreach (Match match in matches)
        {
            if (Guid.TryParse(match.Value, out Guid guid))
            {
                guids.Add(guid);
            }
        }

        return guids;
    }

    public static string GenerateRandomString(int length, string prefix = "")
    {
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var stringChars = new char[length];

        for (var i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        return prefix + new String(stringChars);
    }

    public static string ToCustomDateFormat(this DateTime dateTime)
    {
        return dateTime.ToString("dd/MM/yyyy");
    }
}
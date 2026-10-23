using System;
using System.Text.RegularExpressions;

namespace WaterSort
{
    public enum WaterSortPixKeyType { CpfCnpj, Phone, Email, Random }

    /// <summary>Local Pix key formatting only; registration is verified by the payment provider.</summary>
    public static class WaterSortPixKey
    {
        static readonly Regex EmailPattern = new Regex(
            @"^[a-z0-9.!#$%&'*+/=?^_`{|}~\-]+@[a-z0-9](?:[a-z0-9\-]*[a-z0-9])?(?:\.[a-z0-9](?:[a-z0-9\-]*[a-z0-9])?)+$",
            RegexOptions.CultureInvariant);

        public static string Id(WaterSortPixKeyType type)
        {
            switch (type)
            {
                case WaterSortPixKeyType.Phone: return "phone";
                case WaterSortPixKeyType.Email: return "email";
                case WaterSortPixKeyType.Random: return "random";
                default: return "cpf_cnpj";
            }
        }

        public static WaterSortPixKeyType RestoreType(string id, string account)
        {
            switch (id)
            {
                case "cpf_cnpj": return WaterSortPixKeyType.CpfCnpj;
                case "phone": return WaterSortPixKeyType.Phone;
                case "email": return WaterSortPixKeyType.Email;
                case "random": return WaterSortPixKeyType.Random;
            }
            // Legacy saves have only an account value. Preserve it while choosing a useful tab.
            string value = (account ?? string.Empty).Trim();
            if (value.IndexOf('@') >= 0) return WaterSortPixKeyType.Email;
            if (Guid.TryParseExact(value, "D", out _)) return WaterSortPixKeyType.Random;
            string compact = RemoveFormatting(value);
            if (value.StartsWith("+", StringComparison.Ordinal) || value.IndexOf('(') >= 0 ||
                ((compact.Length == 12 || compact.Length == 13) && compact.StartsWith("55", StringComparison.Ordinal)))
                return WaterSortPixKeyType.Phone;
            return WaterSortPixKeyType.CpfCnpj;
        }

        public static bool TryNormalize(WaterSortPixKeyType type, string value, out string normalized)
        {
            normalized = (value ?? string.Empty).Trim();
            if (normalized.Length == 0) return false;
            switch (type)
            {
                case WaterSortPixKeyType.CpfCnpj:
                    normalized = RemoveFormatting(normalized).ToUpperInvariant();
                    if (normalized.Length == 11)
                        return IsAsciiDigits(normalized, 0, normalized.Length);
                    if (normalized.Length != 14 || !IsAsciiDigits(normalized, 12, 2)) return false;
                    for (int i = 0; i < 12; i++)
                        if (!IsDigit(normalized[i]) && (normalized[i] < 'A' || normalized[i] > 'Z')) return false;
                    return true;
                case WaterSortPixKeyType.Phone:
                    normalized = RemoveFormatting(normalized);
                    if (!normalized.StartsWith("+", StringComparison.Ordinal))
                    {
                        if (normalized.Length == 10 || normalized.Length == 11) normalized = "+55" + normalized;
                        else if (normalized.StartsWith("55", StringComparison.Ordinal) &&
                                 (normalized.Length == 12 || normalized.Length == 13)) normalized = "+" + normalized;
                        else return false;
                    }
                    return normalized.Length >= 9 && normalized.Length <= 16 &&
                           normalized[1] != '0' && IsAsciiDigits(normalized, 1, normalized.Length - 1);
                case WaterSortPixKeyType.Email:
                    normalized = normalized.ToLowerInvariant();
                    return normalized.Length <= 77 && EmailPattern.IsMatch(normalized);
                case WaterSortPixKeyType.Random:
                    if (!Guid.TryParseExact(normalized, "D", out Guid key) || key == Guid.Empty) return false;
                    normalized = key.ToString("D");
                    return true;
                default: return false;
            }
        }

        static string RemoveFormatting(string value) => value.Replace(" ", string.Empty)
            .Replace(".", string.Empty).Replace("-", string.Empty).Replace("/", string.Empty)
            .Replace("(", string.Empty).Replace(")", string.Empty);

        static bool IsAsciiDigits(string value, int start, int count)
        {
            for (int i = start; i < start + count; i++) if (!IsDigit(value[i])) return false;
            return true;
        }

        static bool IsDigit(char value) => value >= '0' && value <= '9';
    }
}

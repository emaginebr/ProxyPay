using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace ProxyPay.Domain.Core
{
    public static class Utils
    {
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
        public static bool IsValidCpf(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            cpf = cpf.Replace(".", "").Replace("-", "").Trim();

            if (cpf.Length != 11 || !long.TryParse(cpf, out _))
                return false;

            if (cpf == new string(cpf[0], 11))
                return false;

            var sum = 0;
            for (var i = 0; i < 9; i++)
                sum += (cpf[i] - '0') * (10 - i);
            var remainder = sum % 11;
            var digit1 = remainder < 2 ? 0 : 11 - remainder;
            if ((cpf[9] - '0') != digit1)
                return false;

            sum = 0;
            for (var i = 0; i < 10; i++)
                sum += (cpf[i] - '0') * (11 - i);
            remainder = sum % 11;
            var digit2 = remainder < 2 ? 0 : 11 - remainder;
            if ((cpf[10] - '0') != digit2)
                return false;

            return true;
        }

        public static string CleanCpf(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return cpf;
            return cpf.Replace(".", "").Replace("-", "").Trim();
        }

        public static void Copy<TParent, TChild>(TParent parent, TChild child)
        {
            var parentProperties = parent.GetType().GetProperties();
            var childProperties = child.GetType().GetProperties();

            foreach (var parentProperty in parentProperties)
            {
                foreach (var childProperty in childProperties)
                {
                    if (parentProperty.Name == childProperty.Name && parentProperty.PropertyType == childProperty.PropertyType)
                    {
                        try
                        {
                            childProperty.SetValue(child, parentProperty.GetValue(parent));
                        }
                        catch (Exception) { break; }
                        break;
                    }
                }
            }
        }
    }
}

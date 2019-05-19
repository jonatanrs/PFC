using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PFC.WebApp.Support
{
    public static class FriendlyBytesFormat
    {
        public static readonly string[] units = { "B", "KB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        public static string ToFriendlyBytesFormat(long bytes)
        {
            if (bytes < 1024)
                return bytes + units[0];

            decimal result = bytes;
            var i = 0;
            while (result >= 1024)
            {
                result = result / 1024;
                i++;
            }

            return result.ToString("F") + units[i];
        }
    }
}

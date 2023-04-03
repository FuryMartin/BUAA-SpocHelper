using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BUAAToolkit.Core.Helpers;
public static class MD5Helper
{
    public static string GetFileMD5(string filePath)
    {
        byte[] hash;

        using (MD5 md5 = MD5.Create())
        {
            using FileStream stream = File.OpenRead(filePath);
            hash = md5.ComputeHash(stream);
        }

        var md5String = BitConverter.ToString(hash).Replace("-", string.Empty);
        return md5String.ToLower();
    }
}

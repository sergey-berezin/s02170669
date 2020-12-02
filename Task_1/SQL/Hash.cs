using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SQL
{
    public class Hash
    {
        public static string GetHash(byte [] input)
        {
            HashAlgorithm hashAlgorithm = SHA256.Create();
            // Convert the input string to a byte array and compute the hash.
            byte[] data = hashAlgorithm.ComputeHash(input);

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        // Verify a hash against a string.
        public static bool VerifyHash(string first_hash, string second_hash)
        {
            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            return comparer.Compare(first_hash, second_hash) == 0;
        }

        public static bool ByteArrayCompare(byte[] array_1, byte[] array_2)
        {
            if (array_1.Length != array_2.Length)
                return false;

            for (int i = 0; i < array_1.Length; i++)
                if (array_1[i] != array_2[i])
                    return false;

            return true;
        }
    }
}

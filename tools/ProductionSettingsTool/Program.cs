using System;
using System.Security.Cryptography;
using System.Text;

namespace ProductionSettingsTool
{
    class Program
    {
        static void Main(string[] args)
        {
            // this will help generate production required settins:
            // 1. Hash of the password for jwt authentication
            // 2. Secret key for signing jwt tokens

            Console.WriteLine("Enter user password:");
            var password = Console.ReadLine();

            using (var sha = SHA512.Create())
            {
                byte[] computedHash = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                var base64Hash = Convert.ToBase64String(computedHash);

                Console.WriteLine(base64Hash);
            }

            var hmac = new HMACSHA256();
            var key = Convert.ToBase64String(hmac.Key);

            Console.WriteLine("\nSigning key:");
            Console.WriteLine(key);

            Console.ReadKey();
        }
    }
}
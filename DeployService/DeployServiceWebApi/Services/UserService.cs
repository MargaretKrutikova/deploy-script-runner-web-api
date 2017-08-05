using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DeployServiceWebApi.Options;
using Microsoft.Extensions.Options;

namespace DeployServiceWebApi.Services
{
    public interface IUserService
    {
        bool Authenticate(string username, string password);
    }
 
    public class UserService : IUserService
    {
        private readonly byte[] _storedPasswordHash;
        private readonly string _storedUsername;

        public UserService(IOptions<CustomAuthorizationOptions> options)
        {
            _storedUsername = options.Value.Username;
            _storedPasswordHash = Convert.FromBase64String(options.Value.PasswordHash);
        }
 
        public bool Authenticate(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return false;
 
            return username == _storedUsername && VerifyPasswordHash(password, _storedPasswordHash);
        }

        private static bool VerifyPasswordHash(
            string password, 
            byte[] passwordHash)
        {
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty string.", "password");
            if (passwordHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");

            using (var sha = SHA512.Create())
            {
                var computedHash = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i]) return false;
                }
            }
 
            return true;
        }
    }
}
using System;
using Microsoft.AspNetCore.DataProtection;

namespace Datack.Agent.Services
{
    public class DataProtector
    {
        private readonly IDataProtector _protector;

        public DataProtector(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("Datack.Agent.DataProtector");
        }

        public String Encrypt(String input)
        {
            return _protector.Protect(input);
        }

        public String Decrypt(String input)
        {
            return _protector.Unprotect(input);
        }
    }
}

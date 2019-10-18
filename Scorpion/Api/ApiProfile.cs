using System;

namespace Scorpion.Api
{
    public class ApiProfile
    {
        public string BaseUrl { get; set; }
        public string CovenantToken { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool IgnoreSSL { get; set; }

        public bool HasCovenantToken()
        {
            return String.IsNullOrEmpty(CovenantToken);
        }

        public override string ToString()
        {
            return $"BaseUrl: {BaseUrl} - Username: {UserName} - Password: *** - IgnoreSSL: {IgnoreSSL}";
        }
    }
}
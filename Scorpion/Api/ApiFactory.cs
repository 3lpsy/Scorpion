using System;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Rest;
using Covenant.API;
using Covenant.API.Models;

namespace Scorpion.Api
{
    public class ApiFactory
    {
        public ApiProfile Profile { get; set; }


        public ApiFactory(ApiProfile profile)
        {
            Profile = profile;
        }

        public CovenantAPI Api()
        {
            if (Profile.IgnoreSSL)
            {
                Console.WriteLine("Creating insecure SSL Covenant API");
                return new CovenantAPI(new System.Uri(Profile.BaseUrl), MakeCredentials(), MakeUnsafeSSlHttpHandler());
            }
            Console.WriteLine("Creating Covenant API");
            return new CovenantAPI(new System.Uri(Profile.BaseUrl), MakeCredentials());
        }

        public async Task<CovenantAPI> AuthenticatedApi()
        {
            if (!Profile.HasCovenantToken())
            {
                Console.WriteLine("Authenticating API");
                var result = await Api().ApiUsersLoginPostWithHttpMessagesAsync(ProfileUserLogin());
                Profile.CovenantToken = result.Body.CovenantToken;
                Console.WriteLine($"TOKEN: {Profile.CovenantToken}");
            }
            return Api();
        }

        public CovenantUserLogin ProfileUserLogin()
        {
            return new CovenantUserLogin(Profile.UserName, Profile.Password);
        }

        public ServiceClientCredentials MakeCredentials()
        {
            if (Profile.HasCovenantToken())
            {
                return new TokenCredentials(Profile.CovenantToken);
            }
            return new BasicAuthenticationCredentials
            {
                UserName = Profile.UserName,
                Password = Profile.Password
            };
        }

        public HttpClientHandler MakeUnsafeSSlHttpHandler()
        {
            return new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
            };
        }
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

namespace Scorpion.RestClient
{
    public partial class HttpRestClientBase
    {
        // TODO: this should probably be in a cofig or HTTPProfile class
        public string CovenantToken { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool IgnoreSSL { get; set; }
        public bool Debug { get; set; }

        protected async Task<HttpClient> CreateHttpClientAsync(CancellationToken cancellationToken)
        {
            if (Debug)
            {
                Console.WriteLine("CreateHttpClientAsync");

            }
            HttpClient client;

            if (IgnoreSSL)
            {
                if (Debug)
                {
                    Console.WriteLine("Attaching unsafe SSL Handler");
                }
                client = new HttpClient(new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
                });
            }
            else
            {
                client = new HttpClient();

            }
            // TODO: Customize HTTP client
            return client;
        }


    }
}

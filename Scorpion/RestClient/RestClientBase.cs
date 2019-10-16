using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

namespace Scorpion.RestClient
{
    public partial class RestClientBase
    {

        protected async Task<HttpClient> CreateHttpClientAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("CreateHttpClientAsync");
            var client = new HttpClient();
            // TODO: Customize HTTP client
            return client;
        }

    }
}

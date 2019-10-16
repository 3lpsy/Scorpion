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

        partial void PrepareRequest(HttpClient request, ref string url)
        {
            // TODO: Prepare the request and modify the URL if needed
            Console.WriteLine("Preparing Request");
        }

        partial void ProcessResponse(HttpClient request, HttpResponseMessage response)
        {
            // TODO: Post-process the response
            Console.WriteLine("Procesing Request");
        }
    }
}

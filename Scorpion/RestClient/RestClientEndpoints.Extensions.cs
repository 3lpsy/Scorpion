
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;

namespace Scorpion.RestClient
{
    public partial class RestClient
    {
        partial void PrepareRequest(HttpClient client, HttpRequestMessage request, string url)
        {
            Console.WriteLine("Preparing request");
        }
        partial void PrepareRequest(HttpClient client, HttpRequestMessage request, StringBuilder urlBuilder)
        {
            Console.WriteLine("Preparing request");
        }

        partial void ProcessResponse(HttpClient request, HttpResponseMessage response)
        {
            // TODO: Post-process the response
            Console.WriteLine("Procesing Request");
        }
    }
}
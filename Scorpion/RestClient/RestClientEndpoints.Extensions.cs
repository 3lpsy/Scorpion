
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Scorpion.RestClient
{
    public partial class HttpRestClient
    {
        partial void PrepareRequest(HttpClient client, HttpRequestMessage request, string url)
        {
            if (Debug)
            {
                Console.WriteLine($"Preparing request: client - {request}");
                Console.WriteLine($"Preparing request: request - {request}");
                Console.WriteLine($"Preparing request: url - {url}");
            }


        }
        partial void PrepareRequest(HttpClient client, HttpRequestMessage request, StringBuilder urlBuilder)
        {

            if (HasCovenantToken())
            {
                if (Debug)
                {
                    Console.WriteLine($"Preparing request: attaching jwt");
                }
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CovenantToken);
            }

            if (Debug)
            {
                Console.WriteLine($"Preparing request: client - {client}");
                Console.WriteLine($"Preparing request: request - {request}");
                Console.WriteLine($"Preparing request: urlBuilder - {urlBuilder}");
            }


        }

        partial void ProcessResponse(HttpClient request, HttpResponseMessage response)
        {
            if (Debug)
            {
                Console.WriteLine($"Processing response: request - {request}");
                Console.WriteLine($"Processing response: response - {response}");
            }

        }

        public async Task<int> Authenticate()
        {
            if (String.IsNullOrEmpty(UserName))
            {
                Console.Write("Enter Covenant UserName: ");
                UserName = Console.ReadLine();
            }

            if (String.IsNullOrEmpty(Password))
            {
                Console.Write("Enter Covenant Password: ");
                Password = GetPassword();
            }
            CovenantUserLogin login = new CovenantUserLogin();
            login.UserName = UserName;
            login.Password = Password;
            try
            {
                CovenantUserLoginResult result = await ApiUsersLoginPostAsync(login);
                CovenantToken = result.CovenantToken;
                return 0;
            }
            catch (ApiException ex)
            {
                Console.WriteLine($"An Api Error Occurred: {ex.GetType()}");
                Console.WriteLine($"Error: {ex.Message}");
                if (Debug)
                {
                    throw ex;
                }
                return 1;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"An HTTP Error Occurred: {ex.GetType()}");
                Console.WriteLine($"Error: {ex.Message}");
                if (Debug)
                {
                    throw ex;
                }
                return 1;
            }
        }

        public async Task<int> EnsureAuthenticated()
        {
            if (!HasCovenantToken())
            {
                return await Authenticate();
            }

            return 0;
        }

        public bool HasCovenantToken()
        {
            return !String.IsNullOrEmpty(CovenantToken);
        }

        // TODO: move somewhere else
        public string GetPassword()
        {
            string password = "";
            ConsoleKeyInfo nextKey = Console.ReadKey(true);
            while (nextKey.Key != ConsoleKey.Enter)
            {
                if (nextKey.Key == ConsoleKey.Backspace)
                {
                    if (password.Length > 0)
                    {
                        password = password.Substring(0, password.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else
                {
                    password += nextKey.KeyChar;
                    Console.Write("*");
                }
                nextKey = Console.ReadKey(true);
            }
            return password;
        }
    }
}
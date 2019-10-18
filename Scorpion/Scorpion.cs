using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Rest;
using McMaster.Extensions.CommandLineUtils;
using Scorpion.Api;

using Scorpion.Commands;
// using Scorpion.RestClient;
// using Covenant.API;

using System.Net.Http;

namespace Scorpion
{
    class Program
    {
        static int Main(string[] args)
        {
            var provider = ConfigureServices();

            var app = new CommandLineApplication<ScorpionCommand>();

            app.UsePagerForHelpText = false;
            app.MakeSuggestionsInErrorMessage = true;
            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(provider);

            try
            {
                return app.Execute(args);
            }
            catch (UnrecognizedCommandParsingException ex)
            {
                Console.WriteLine($"Parsing Error: {ex.Message}");
                app.ShowHelp();
                return 1;
            }
        }
        public static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<ApiFactory>((s) =>
            {
                var profile = BuildApiProfile();
                // Console.WriteLine(profile);
                return new ApiFactory(profile);
            });
            return services.BuildServiceProvider();
        }

        public static ApiProfile BuildApiProfile()
        {
            var profile = new ApiProfile()
            {
                BaseUrl = Environment.GetEnvironmentVariable("COVENANT_BASE_URL") ?? "https://127.0.0.1:7443",
                CovenantToken = Environment.GetEnvironmentVariable("COVENANT_TOKEN") ?? "",
                UserName = Environment.GetEnvironmentVariable("COVENANT_USERNAME") ?? "",
                Password = Environment.GetEnvironmentVariable("COVENANT_PASSWORD") ?? "",
                IgnoreSSL = Environment.GetEnvironmentVariable("COVENANT_IGNORE_SSL") == "1" ? true : false
            };
            return profile;
        }
    }
}

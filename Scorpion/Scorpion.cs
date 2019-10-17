using System;
using Scorpion.Commands;
using Scorpion.RestClient;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;

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

            services.AddSingleton<HttpRestClient>((services) =>
            {
                HttpRestClient client = new HttpRestClient();
                client.BaseUrl = Environment.GetEnvironmentVariable("COVENANT_BASE_URL") ?? "https://127.0.0.1:7443";
                client.CovenantToken = Environment.GetEnvironmentVariable("COVENANT_TOKEN") ?? "";
                client.UserName = Environment.GetEnvironmentVariable("COVENANT_USERNAME") ?? "";
                client.Password = Environment.GetEnvironmentVariable("COVENANT_PASSWORD") ?? "";
                client.IgnoreSSL = Environment.GetEnvironmentVariable("COVENANT_IGNORE_SSL") == "1" ? true : false;
                // I don't like this, needs a log level and logger, will be removed later
                client.Debug = Environment.GetEnvironmentVariable("COVENANT_HTTP_DEBUG") == "1" ? true : false;
                return client;
            });
            // bind RestClientBase instance to the class.
            return services.BuildServiceProvider();
        }
    }
}

using System;
using Scorpion.Commands;
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
            // bind RestClientBase instance to the class.
            return services.BuildServiceProvider();
        }
    }
}

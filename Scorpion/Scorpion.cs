using System;
using System.IO;
using System.Reflection;
using Scorpion.Commands;
// using Curator.Data.EntityFramework.Context;
// using Curator.Data.EntityFramework.Sqlite;
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
            return services.BuildServiceProvider();
        }
    }
}

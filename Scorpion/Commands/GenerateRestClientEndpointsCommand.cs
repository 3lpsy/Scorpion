using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using NSwag;
using NSwag.CodeGeneration.CSharp;

namespace Scorpion.Commands
{
    [Command("generate")]
    public class GenerateRestClientEndpointsCommand
    {
        public ScorpionCommand Parent { get; set; }

        public async Task<int> OnExecuteAsync(IConsole console)
        {
            System.Net.WebClient wclient = new System.Net.WebClient();

            string apiJson = System.IO.File.ReadAllText("./../current.api.json");
            var document = await OpenApiDocument.FromJsonAsync(apiJson);

            var settings = new CSharpClientGeneratorSettings
            {
                ClassName = "RestClient",
                ClientBaseClass = "RestClientBase",
                UseHttpClientCreationMethod = true,
                CSharpGeneratorSettings =
                {
                    Namespace = "Scorpion.RestClient"
                }
            };

            var generator = new CSharpClientGenerator(document, settings);
            var code = generator.GenerateFile();
            Console.WriteLine("Writing to RestClientEndpoints.cs...");
            using (StreamWriter outputFile = new StreamWriter(Path.Combine("RestClient", "RestClientEndpoints.cs")))
            {
                outputFile.Write(code);
            }

            return 0;
        }
    }
}

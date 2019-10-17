using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using Scorpion.RestClient;

namespace Scorpion.Commands
{
    [Command("get-token")]
    public class GetTokenCommand
    {
        public ScorpionCommand Parent { get; set; }

        public async Task<int> OnExecuteAsync(IConsole console)
        {
            if (Parent == null) throw new ArgumentNullException(nameof(Parent));
            if (console == null) throw new ArgumentNullException(nameof(console));

            await Parent.HttpRestClient.EnsureAuthenticated();

            Console.WriteLine($"Token: {Parent.HttpRestClient.CovenantToken}");

            return 0;
        }
    }
}

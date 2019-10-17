using System;
using Scorpion.RestClient;
using McMaster.Extensions.CommandLineUtils;

namespace Scorpion.Commands
{

    [Subcommand(typeof(AddUserCommand), typeof(ListUsersCommand), typeof(GenerateRestClientEndpointsCommand))]
    public class ScorpionCommand : IDisposable
    {

        public HttpRestClient HttpRestClient { get; }

        public ScorpionCommand(HttpRestClient restClient)
        {
            HttpRestClient = restClient;
        }

        private int OnExecute(CommandLineApplication application)
        {
            application.ShowHelp(false);
            return 0;
        }

        public void Dispose()
        {
        }
    }
}

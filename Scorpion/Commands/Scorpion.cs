using System;
using McMaster.Extensions.CommandLineUtils;
using Scorpion.Api;

namespace Scorpion.Commands
{

    // [Subcommand(typeof(AddUserCommand), typeof(ListUsersCommand), typeof(GetTokenCommand), typeof(RemoveUserCommand))]
    [Subcommand(typeof(ListUsersCommand))]
    public class ScorpionCommand : IDisposable
    {

        public ApiFactory ApiFactory { get; set; }

        public ScorpionCommand(ApiFactory apiFactory)
        {
            ApiFactory = apiFactory;
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

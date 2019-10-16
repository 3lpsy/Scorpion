using System;
using McMaster.Extensions.CommandLineUtils;

namespace Scorpion.Commands
{

    [Subcommand(typeof(AddUserCommand), typeof(ListUsersCommand), typeof(GenerateClientCommand))]
    public class ScorpionCommand : IDisposable
    {

        public ScorpionCommand()
        {
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

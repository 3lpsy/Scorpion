using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Covenant.API;
using Covenant.API.Models;
using System.Collections.Generic;
using Microsoft.Rest;
using Scorpion.Exceptions;
using Scorpion.Commands;
using Scorpion.Jobs;

namespace Scorpion.Commands.User
{
    [Command("rmuser", Description = "Remove user by username")]
    public class RemoveUserCommand : Command
    {

        [Argument(0, name: "Username")]
        public string TargetUsername { get; set; }

        public async Task<int> OnExecuteAsync(IConsole console)
        {
            await SetupCommandAsync(console, true, true);
            return await RunAsync(console);
        }

        public async Task<int> RunAsync(IConsole console)
        {
            if (String.IsNullOrEmpty(TargetUsername)) throw new MissingParameterException(nameof(TargetUsername));
            return await new RemoveUserJob(console, Api).RunAsync(TargetUsername);
        }
    }
}

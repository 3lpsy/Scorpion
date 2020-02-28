using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Covenant.API;
using Covenant.API.Models;
using System.Collections.Generic;
using Scorpion.Commands;

namespace Scorpion.Commands.User
{
    [Command("token")]
    public class GetTokenCommand : Command
    {
        public async Task<int> OnExecuteAsync(IConsole console)
        {
            await SetupCommandAsync(console, true, true);
            return await RunAsync(console);
        }

        public async Task<int> RunAsync(IConsole console)
        {
            Console.WriteLine($"Current User: {Parent.ApiFactory.Profile.UserName}");
            Console.WriteLine($"Bearer Token: {Parent.ApiFactory.Profile.CovenantToken}");
            return await Task.FromResult(0);
        }
    }
}

using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Covenant.API;
using Covenant.API.Models;
using System.Collections.Generic;
using Microsoft.Rest;
using Scorpion.Exceptions;
using Scorpion.Jobs;

namespace Scorpion.Commands
{
    [Command("adduser")]
    public class AddUserCommand : Command
    {

        [Argument(0, name: "Username")]
        public string NewUserName { get; set; }
        [Argument(1, name: "Password")]
        public string NewPassword { get; set; }

        public async Task<int> OnExecuteAsync(IConsole console)
        {
            await SetupCommandAsync(console, true, true);
            return await RunAsync(console);
        }

        public async Task<int> RunAsync(IConsole console)
        {

            if (String.IsNullOrEmpty(NewUserName)) throw new MissingParameterException(nameof(NewUserName));
            if (String.IsNullOrEmpty(NewPassword))
            {
                Console.Write("Enter Password: ");
                NewPassword = GetPassword();
                Console.WriteLine();
            }

            return await new AddUserJob(console, Api).RunAsync(NewUserName, NewPassword);
        }
    }
}

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using Covenant.API;
using Covenant.API.Models;
using System.Net.Http;
using Scorpion.Exceptions;

namespace Scorpion.Commands
{
    public abstract class Command
    {
        public ScorpionCommand Parent { get; set; }
        public CovenantAPI Api { get; set; }

        public async Task<int> SetupCommandAsync(IConsole console, bool apiRequired = false, bool authRequired = false)
        {
            if (Parent == null) throw new ArgumentNullException(nameof(Parent));
            if (console == null) throw new ArgumentNullException(nameof(console));
            Parent.LoadRootOptions();
            if (apiRequired)
            {
                await LoadApiAsync(authRequired);
            }
            return await Task.FromResult(0);
        }

        public async Task<int> LoadApiAsync(bool authRequired)
        {
            if (authRequired)
            {
                try
                {
                    Api = await Parent.ApiFactory.AuthenticatedApi();
                }
                catch (HttpRequestException ex)
                {
                    throw new AppException($"Could not connect to or authenticate to API. Quiting: {ex.Message}");
                }
            }
            else
            {
                throw new NotImplementedException();
            }
            return await Task.FromResult(0);
        }

        // stolen, probably busted
        public string GetPassword()
        {
            string pass = "";
            do
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                // Backspace Should Not Work
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        pass = pass.Substring(0, (pass.Length - 1));
                        Console.Write("\b \b");
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                }
            } while (true);
            return pass;
        }
    }
}

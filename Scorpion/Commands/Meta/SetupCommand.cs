using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using Covenant.API;
using Covenant.API.Models;
using Scorpion.Commands;
using Scorpion.Jobs;
using Scorpion.Exceptions;

namespace Scorpion.Commands.Meta
{
  [Command("setup", Description = "Setup")]
  public class SetupCommand : Command
  {
    [Argument(0, name: "ConnectAddress")]
    public string ConnectAddress { get; set; }

    public async Task<int> OnExecuteAsync(IConsole console)
    {
      await SetupCommandAsync(console, true, true);
      return await RunAsync(console);
    }
    public async Task<int> RunAsync(IConsole console)
    {
      if (String.IsNullOrEmpty(ConnectAddress)) throw new MissingParameterException(nameof(ConnectAddress));

      return await new SetupJob(console, Api).RunAsync(ConnectAddress);
    }
  }
}

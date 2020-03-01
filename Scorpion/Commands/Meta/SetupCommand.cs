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

    [Option("--connect-address", Description = "Find Listener by ConnectAddress. New listener will be created if none is found.")]
    public string ConnectAddress { get; set; }

    [Option("--connect-port", Description = "Find Listener by ConnectPort (can be combined with --connect-address). New listener will be created if none is found and --connect-address is provided")]
    public int ConnectPort { get; set; } = 0;

    [Option("--listener", Description = "Existing Listener Name to use (alternative use --connect-address/port to filter)")]
    public string ListenerName { get; set; }

    [Option("--count", Description = "Number of SMB Grunts to create")]
    public int SmbGruntCount { get; set; } = 10;

    public async Task<int> OnExecuteAsync(IConsole console)
    {
      await SetupCommandAsync(console, true, true);
      return await RunAsync(console);
    }
    public async Task<int> RunAsync(IConsole console)
    {
      if (String.IsNullOrEmpty(ListenerName)) {
        if (String.IsNullOrEmpty(ConnectAddress)) throw new MissingParameterException(nameof(ConnectAddress));
      }
      return await new SetupJob(console, Api).RunAsync(ListenerName, ConnectAddress, ConnectPort, SmbGruntCount);
    }
  }
}

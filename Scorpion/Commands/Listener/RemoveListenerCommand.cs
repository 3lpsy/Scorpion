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

namespace Scorpion.Commands.Listener
{
  [Command("rmlistener", Description = "Remove listener by Name")]
  public class RemoveListenerCommand : Command
  {

    [Argument(0, name: "Name")]
    public string TargetName { get; set; }

    public async Task<int> OnExecuteAsync(IConsole console)
    {
      await SetupCommandAsync(console, true, true);
      return await RunAsync(console);
    }

    public async Task<int> RunAsync(IConsole console)
    {
      if (String.IsNullOrEmpty(TargetName)) throw new MissingParameterException(nameof(TargetName));
      return await new RemoveListenerJob(console, Api).RunAsync(TargetName);
    }
  }
}

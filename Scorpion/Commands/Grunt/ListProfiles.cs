using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using Covenant.API;
using Covenant.API.Models;
using Scorpion.Commands;
using Scorpion.Jobs;

namespace Scorpion.Commands.Grunt
{
  [Command("profiles", Description = "List Profiles")]
  public class ListProfilesCommand : Command
  {
    public async Task<int> OnExecuteAsync(IConsole console)
    {
      await SetupCommandAsync(console, true, true);
      return await RunAsync(console);
    }
    public async Task<int> RunAsync(IConsole console)
    {

      return await new ListProfilesJob(console, Api).RunAsync();
    }
  }
}

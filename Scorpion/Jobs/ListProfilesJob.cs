
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using Covenant.API;
using Covenant.API.Models;
using System.Net.Http;
using Scorpion.Exceptions;
using Scorpion.Jobs;
using Scorpion.Api;

namespace Scorpion.Jobs
{
  public class ListProfilesJob : Job
  {
    public ListProfilesJob(IConsole console, CovenantAPI api) : base(console, api) { }

    public async Task<int> RunAsync()
    {
      IList<Profile> profiles = await new RequestBuilder(Api).GetProfiles();
      Console.WriteLine("Users: ");
      foreach (Profile profile in profiles) {
        Console.WriteLine($"      {profile.Name} - {profile.Id}");
      }
      return await Task.FromResult(0);
    }
  }
}

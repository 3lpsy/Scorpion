
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
  public class RemoveListenerJob : Job
  {
    public RemoveListenerJob(IConsole console, CovenantAPI api) : base(console, api) { }

    public async Task<int> RunAsync(string targetName)
    {
      var request = new RequestBuilder(Api);
      IList<Listener> listeners = await request.GetListeners();

      foreach (Listener listener in listeners) {
        if (listener.Name.ToLower() == targetName.ToLower()) {
          Console.WriteLine($"Deleting Listener: {listener.Id}");

          var deleteListenerResult = await request.DeleteListenerById((int)listener.Id);
          Console.WriteLine($"Deleted Listener: {listener.Name}");
          return 0;
        }
      }
      Console.WriteLine($"Failed to find listener to delete: {targetName}");
      return 0;
    }
  }
}

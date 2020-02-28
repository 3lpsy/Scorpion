using System.Threading.Tasks;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using Covenant.API;
using Covenant.API.Models;
using Scorpion.Api;

namespace Scorpion.Jobs
{
  public class ListImplantTemplatesJob : Job
  {
    public ListImplantTemplatesJob(IConsole console, CovenantAPI api) : base(console, api) { }

    public async Task<int> RunAsync()
    {
      IList<ImplantTemplate> templates = await new RequestBuilder(Api).GetImplantTemplates();

      Console.WriteLine("Templates: ");
      foreach (ImplantTemplate template in templates) {
        Console.WriteLine($"      {template.Name} - {template.Id}");
      }
      return await Task.FromResult(0);
    }
  }
}

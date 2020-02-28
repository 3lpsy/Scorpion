
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
    public class ListUsersJob : Job
    {
        public ListUsersJob(IConsole console, CovenantAPI api) : base(console, api) { }

        public async Task<int> RunAsync()
        {
            IList<CovenantUser> users = await new RequestBuilder(Api).GetUsers();
            Console.WriteLine("Users: ");
            foreach (CovenantUser user in users)
            {
                Console.WriteLine($"      {user.UserName} - {user.Id}");
            }
            return await Task.FromResult(0);
        }
    }
}


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
    public class RemoveUserJob : Job
    {
        public RemoveUserJob(IConsole console, CovenantAPI api) : base(console, api) { }

        public async Task<int> RunAsync(string targetUsername)
        {
            var request = new RequestBuilder(Api);
            IList<CovenantUser> users = await request.GetUsers();

            foreach (CovenantUser user in users)
            {
                if (user.UserName.ToLower() == targetUsername.ToLower())
                {
                    var deleteUserResult = await request.DeleteUserById(user.Id);
                    Console.WriteLine($"Deleted User: {user.UserName}");
                    return 0;
                }
            }
            Console.WriteLine($"Failed to find user to delete: {targetUsername}");
            return 0;
        }
    }
}


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

using Microsoft.Rest;

namespace Scorpion.Jobs
{
    public class AddUserJob : Job
    {
        public AddUserJob(IConsole console, CovenantAPI api) : base(console, api) { }

        public async Task<int> RunAsync(string newUsername, string newPassword)
        {

            var request = new RequestBuilder(Api);

            CovenantUser user = await request.CreateUser(newUsername, newPassword);
            Console.WriteLine($"Added User: {user.UserName} - {user.Id}");
            IList<IdentityRole> roles = await request.GetRoles();
            foreach (IdentityRole role in roles)
            {
                if (role.Name.ToLower() == "user" || role.Name.ToLower() == "administrator")
                {
                    await request.AddUserRole(user.Id, role.Id);
                    Console.WriteLine($"Added User to Role: {role.Name}");
                }
            }
            return 0;
        }
    }
}

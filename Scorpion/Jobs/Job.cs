
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using Covenant.API;
using Covenant.API.Models;
using System.Net.Http;
using Scorpion.Exceptions;

namespace Scorpion.Jobs
{
    public class Job
    {
        public IConsole Console { get; set; }
        public CovenantAPI Api { get; set; }
        public Job(IConsole console, CovenantAPI api)
        {
            Console = console;
            Api = api;
        }
    }
}

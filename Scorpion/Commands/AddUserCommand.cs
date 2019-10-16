using System;
using McMaster.Extensions.CommandLineUtils;

namespace Scorpion.Commands
{
    [Command("add-user")]
    public class AddUserCommand
    {
        public ScorpionCommand Parent { get; set; }

        [Argument(0, name: "Item name")]
        public string Name { get; set; }

        public int OnExecute()
        {
            if (Parent == null) throw new ArgumentNullException(nameof(Parent));
            if (String.IsNullOrEmpty(Name)) throw new ArgumentNullException(nameof(Name));

            Console.Write($"Add Name: {Name}");
            return 0;
        }
    }
}

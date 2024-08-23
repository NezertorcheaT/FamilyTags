using System;

namespace Core.Commands.Instances
{
    internal class Help : IProgramCommand
    {
        public string Literal => "help";
        public string Document => @"stop it
get some help";

        public bool Execute(string arg)
        {
            foreach (var command in CommandsRegister.Commands)
            {
                Console.WriteLine($"\n- {command.Literal}:\n{command.Document}");
            }

            return false;
        }
    }
}
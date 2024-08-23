using System;

namespace Core.Commands.Instances
{
    internal class FindTag : IProgramCommand
    {
        public string Literal => "find_tag";

        public string Document => @"finds tag by name
name as argument";

        public bool Execute(string arg)
        {
            if (Vault.Current is null)
            {
                Console.WriteLine("Vault not initialized");
                return false;
            }

            foreach (var tag in Vault.Current.Storage[arg])
            {
                Console.WriteLine(tag.ToString());
            }

            return false;
        }
    }
}
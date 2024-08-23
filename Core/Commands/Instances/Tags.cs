using System;

namespace Core.Commands.Instances
{
    internal class Tags : IProgramCommand
    {
        public string Literal => "tags";

        public string Document => @"returns all registered tags";

        public bool Execute(string arg)
        {
            if (Vault.Current is null)
            {
                Console.WriteLine("Vault not initialized");
                return false;
            }

            foreach (var tag in Vault.Current.Storage)
            {
                Console.WriteLine(tag.ToString());
            }

            return false;
        }
    }
}
using System;

namespace Core.Commands.Instances
{
    internal class NewTag : IProgramCommand
    {
        public string Literal => "new_tag";

        public string Document => @"creates new tag
tag name as argument";

        public bool Execute(string arg)
        {
            if (Vault.Current is null)
            {
                Console.WriteLine("Vault not initialized");
                return false;
            }

            Vault.Current.Storage.Add(new Tag {Name = arg});
            return false;
        }
    }
}
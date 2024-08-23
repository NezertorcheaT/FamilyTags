using System;
using System.Linq;

namespace Core.Commands.Instances
{
    internal class UntagFile : IProgramCommand
    {
        public string Literal => "untag";

        public string Document => @"removes tag from file
tag id and file path as argument";

        public bool Execute(string arg)
        {
            if (Vault.Current is null)
            {
                Console.WriteLine("Vault not initialized");
                return false;
            }

            var strs = arg.Split(" ").ToArray();
            if (strs.Length < 2)
            {
                Console.WriteLine("Not enough arguments");
                return false;
            }

            var id = int.Parse(strs[0]);
            var path = strs[1];

            Vault.Current.UntagFile(Vault.Current.Storage[id], path);

            return false;
        }
    }
}
using System;

namespace Core.Commands.Instances
{
    internal class CacheAll : IProgramCommand
    {
        public string Literal => "cache_all";
        public string Document => @"checks and caches all files in vault directory";

        public bool Execute(string arg)
        {
            if (Vault.Current is null)
            {
                Console.WriteLine("Vault not initialized");
                return false;
            }

            Vault.Current.CacheFiles(0);
            return false;
        }
    }
}
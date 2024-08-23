using System;

namespace Core.Commands.Instances
{
    internal class RenameVault : IProgramCommand
    {
        public string Literal => "rename_vault";

        public string Document => @"renames current vault
name as argument";

        public bool Execute(string arg)
        {
            if (Vault.Current is null)
            {
                Console.WriteLine("Vault not initialized");
                return false;
            }

            Vault.Current.Name = arg;
            return false;
        }
    }
}
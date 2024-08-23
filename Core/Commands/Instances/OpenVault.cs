using System;

namespace Core.Commands.Instances
{
    internal class OpenVault : IProgramCommand
    {
        public string Literal => "open";

        public string Document => @"opens vault from vault file
.vaultFile full path as argument";

        public bool Execute(string arg)
        {
            Vault.OpenVault(arg);
            Console.WriteLine($"Vault \"{Vault.Current.Name}\" opened");
            return false;
        }
    }
}
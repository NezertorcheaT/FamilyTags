using System.IO;

namespace Core.Commands.Instances
{
    internal class NewVault : IProgramCommand
    {
        public string Literal => "init";

        public string Document => @"initializes new vault in current folder
folder path as argument, if null uses execution path";

        public bool Execute(string arg)
        {
            Vault.CreateNewVault(
                string.IsNullOrWhiteSpace(arg) || string.IsNullOrEmpty(arg)
                    ? Directory.GetCurrentDirectory()
                    : arg
            );
            return false;
        }
    }
}
namespace Core.Commands.Instances
{
    internal class Stop : IProgramCommand
    {
        public string Literal => "exit";
        public string Document => @"stops the program";

        public bool Execute(string arg)
        {
            Vault.Current?.Dispose();
            return true;
        }
    }
}
namespace Core.Commands.Instances
{
    internal class Close : IProgramCommand
    {
        public string Literal => "close";
        public string Document => @"closes current vault";

        public bool Execute(string arg)
        {
            Vault.Current?.Dispose();
            return false;
        }
    }
}
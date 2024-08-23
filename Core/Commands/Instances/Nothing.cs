namespace Core.Commands.Instances
{
    internal class Nothing : IProgramCommand
    {
        public string Literal => "";
        public string Document => @"empty";
        
        public bool Execute(string arg) => false;
    }
}
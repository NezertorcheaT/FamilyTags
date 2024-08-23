namespace Core.Commands.Instances
{
    internal class Halp : IProgramCommand
    {
        public string Literal => "halp";

        public string Document => @"stop it
get some halp";

        public bool Execute(string arg) => new Help().Execute(arg);
    }
}
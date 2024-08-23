namespace Core.Commands
{
    internal interface IProgramCommand
    {
        string Literal { get; }
        string Document { get; }
        bool Execute(string arg);
    }
}
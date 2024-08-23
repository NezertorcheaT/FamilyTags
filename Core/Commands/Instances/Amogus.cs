using System;

namespace Core.Commands.Instances
{
    internal class Amogus : IProgramCommand
    {
        public string Literal => "amogus";
        public string Document => @"prints amogus";

        public bool Execute(string arg)
        {
            Console.WriteLine(@"that console is sus
 @@@@@
@   @@@@
@@@@@@@@
@@@@@@@@
 @@ @@");
            return false;
        }
    }
}
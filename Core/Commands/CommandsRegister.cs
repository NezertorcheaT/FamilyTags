using System.Linq;
using Core.Commands.Instances;

namespace Core.Commands
{
    internal static class CommandsRegister
    {
        public static IProgramCommand[] Commands =
        {
            new NewVault(),
            new Stop(),
            new Nothing(),
            new FindTag(),
            new Amogus(),
            new Halp(),
            new Help(),
            new Tags(),
            new OpenVault(),
            new Close(),
            new NewTag(),
            new TagFile(),
            new UntagFile(),
            new RenameVault(),
            new CacheAll(),
        };

        public static IProgramCommand? FindByLiteral(string literal) =>
            Commands.SingleOrDefault(i => i.Literal == literal);
    }
}
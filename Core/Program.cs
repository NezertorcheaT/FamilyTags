using System;
using System.Linq;
using Core.Commands;

namespace Core
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine(Vault.TagDivisor.ToInt());
            Console.WriteLine(Vault.VaultNameEnded.ToInt());
            
            var command = "";
            var arg = "";
            if (args.Length == 1)
                command = args[0];
            else if (args.Length >= 2)
            {
                command = args[0];
                arg = args.Skip(1).Aggregate((a, b) => a + b);
            }

            while (true)
            {
                var executor = CommandsRegister.FindByLiteral(command);
                try
                {
                    if (executor is not null && executor.Execute(arg))
                        break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                if (executor is null)
                    Console.WriteLine($"Command \"{command}\" not found");

                Console.Write(" -> ");
                var spl = (Console.ReadLine() ?? "").Split(" ");
                if (spl.Length == 1)
                    command = spl[0];
                else if (spl.Length >= 2)
                {
                    command = spl[0];
                    arg = spl.Skip(1).Aggregate((a, b) => $"{a} {b}");
                }
            }

            Console.WriteLine("Ending...");
        }
    }
}
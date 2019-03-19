using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CompilerRMLCSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            

            string source;

            using (StreamReader sr = new StreamReader("in.txt"))
            {
                source = sr.ReadToEnd();
            }

            Lex lex = new Lex(source);
            List<Token> tokens = new List<Token>();

            try
            {
                while (true)
                {
                    Token token = lex.parseNext();
                    tokens.Add(token);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Лексемы:\n");
                ConsoleSetup(ConsoleColor.Green);
                foreach (var token in tokens)
                {
                    Console.WriteLine("\t\t" + (LexState)token.state);
                }


                ConsoleSetup(ConsoleColor.Red);
                Console.WriteLine("\nОшибка: " + ex.Message);

                Console.ReadKey();
            }

        }


        public static void ConsoleSetup(ConsoleColor color)
        {
            Console.ForegroundColor = color;
        }
    }
}

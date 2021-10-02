﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SxScript.SxStatements;

namespace SxScript
{
    public class Program
    {
        public static void TestAstPrinter()
        {
            SxExpression sxExpression = new SxBinaryExpression(
                new SxUnaryExpression(
                    new SxToken(SxTokenTypes.Minus, "-", null, 1),
                    new SxLiteralExpression(123)),
                new SxToken(SxTokenTypes.Star, "*", null, 1),
                new SxGroupingExpression(new SxLiteralExpression(45.67)));

            Console.WriteLine(new SxAstPrinter().Print(sxExpression));
        }
        
        public static void TestAstPrinter2(SxExpression expr)
        {
            Console.WriteLine(new SxAstPrinter().Print(expr));
        }
        
        public static void TestAstPrinter2(List<SxStatement> expr)
        {
            foreach (SxStatement stmt in expr)
            {
                Console.WriteLine(new SxAstPrinter().Print(stmt.Expr));   
            }
        }

        public static async Task Main()
        {
            while (true)
            {
                if (false)
                {
                    TestAstPrinter();
                    Console.ReadKey();
                }
            
                Console.WriteLine("Napiš program. Ukonči zápis slovem 'end' na samostatném řádku.");
                List<string> source = ReadProgram();
                string str = "";
            
                SxLexer lexer = new SxLexer(source[0]);
                List<SxToken> tokens = lexer.Tokenize();

                if (true)
                {
                    SxParser<string> parser = new SxParser<string>(tokens);
                    List<SxStatement> exprStatements = parser.Parse();

                    //TestAstPrinter2(exprStatements);

                    SxInterpreter interpreter = new SxInterpreter();
                    object? obj = interpreter.Evaluate(exprStatements);
                
                    //Console.WriteLine("Výsledek interpretace:");
                    //Console.WriteLine(obj);
                }
            

                if (lexer.Messages.Count > 0)
                {
                    foreach (string message in lexer.Messages)
                    {
                        Console.WriteLine(message);
                    }
                }
                else
                {
                    Console.WriteLine("Tokeny");
                    Console.WriteLine("---------------------");
                    foreach (SxToken token in tokens)
                    {
                        Console.WriteLine($"Token: {token.DebugPrint()}");
                    } 
                }

                Console.ReadKey();   
            }
        }

        public static List<string> ReadProgram()
        {
            List<string> lines = new List<string>();
            while (true)
            {
                Console.Write("> ");
                string line = Console.ReadLine();
                if (line == "end")
                {
                    return lines;
                }

                lines.Add(line);
            }
        }
    }
}
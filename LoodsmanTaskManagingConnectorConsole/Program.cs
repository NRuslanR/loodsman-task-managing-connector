using System;
using System.Linq;

namespace LoodsmanTaskManagingConnectorConsole
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                ILoodsmanTaskManagingConnectorApp.ConsoleLoodsmanTaskManagingConnectorAppBuilder
                    .Instance
                    .WithConsoleLoodsmanTaskManagingConnectorInputSource(args)
                    .WithDataFlowLoodsmanTaskManagingConnectorBuilder()
                    .Build()
                    .Run();
            }
            catch (Exception exception)
            {
                var errorMessage = 
                    exception is AggregateException aggregateException ? 
                        string.Join(
                            Environment.NewLine, 
                            aggregateException.InnerExceptions.Select(e => e.Message)) : exception.Message;
                ;
                Console.WriteLine(errorMessage);
            }

            Console.ReadKey();
        }
    }
}
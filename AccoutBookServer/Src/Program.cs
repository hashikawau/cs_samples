using AccoutBookServer.Server;
using CSSamples.Common.Logger;
using System;

namespace AccoutBookServer
{
    class Program
    {
        private static Logger logger = Logger.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            Logger.Appenders += new ConsoleAppender().Append;
            Logger.Appenders += new FileAppender("C:/tmp/logs/account_book_server.log").Append;
            logger.Info("run");

            HttpServer server = HttpServer.Create();
            server.Start();

            string input;
            do
                Console.WriteLine("to stop, type 'quit'");
            while ((input = Console.ReadLine()) != "quit");
        }
    }
}

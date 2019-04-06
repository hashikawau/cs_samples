using CSSamples.Bluetooth.Server;
using CSSamples.Common.Logger;
using System;

namespace Bluetooth
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.Appenders += new ConsoleAppender().Append;
            Logger.Appenders += new FileAppender("C:/tmp/logs/app.log").Append;

            var server = new BluetoothSPPServer();
            server.Start();

            Console.WriteLine("input quit to quit");
            string command;
            while ((command = Console.ReadLine()) != "quit")
            {
                switch (command)
                {
                    case "start":
                        server.Start();
                        break;
                    case "stop":
                        server.Stop();
                        break;
                }
            }
        }

    }
}

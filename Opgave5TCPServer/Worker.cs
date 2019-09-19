using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ObligatoriskOpgave1_CSharpAndUnitTest;

namespace Opgave5TCPServer
{
    internal class Worker
    {
        private static readonly List<Bog> bogList = new List<Bog>()
        {
            new Bog("C Sharp 101", "Morten", 598, "ISBN123456789"),
            new Bog("HTML 101", "Morten", 258, "ISBN123456788"),
            new Bog("CSS 101", "Morten", 198, "ISBN123456777"),
            new Bog("TypeScript 101", "Morten", 123, "ISBN123456666"),
            new Bog("SCRUM 101", "Morten", 999, "ISBN123455555")
        };

        public void Start()
        {
            TcpListener server = new TcpListener(IPAddress.Loopback, 4646);
            server.Start();

            while (true)
            {
                TcpClient socket = server.AcceptTcpClient();
                Task.Run(() =>
                {
                    TcpClient tempSocket = socket;
                    DoClient(tempSocket);
                });
            }
        }

        private void DoClient(TcpClient socket)
        {
            using (StreamReader sr = new StreamReader(socket.GetStream(), Encoding.GetEncoding("ISO-8859-1")))
            using (StreamWriter sw = new StreamWriter(socket.GetStream(), Encoding.GetEncoding("ISO-8859-1")))
            {
                bool stop = false;

                while (!stop)
                {
                    string kommando = sr.ReadLine();
                    string kommando2;

                    if (kommando.Equals("HentAlle"))
                    {
                        foreach (var bog in bogList)
                        {
                            sw.WriteLine(JsonConvert.SerializeObject(bog));
                        }
                    }
                    else if (kommando.Equals("Hent"))
                    {
                        kommando2 = sr.ReadLine();

                        if (kommando2.Length == 13)
                        {
                            sw.WriteLine(bogList.Find(i => i.Isbn13 == kommando2));
                        }
                        else sw.WriteLine("Forkert syntaks. Skriv /h for hjælp!");
                    }
                    else if (kommando.Equals("Gem"))
                    {
                        kommando2 = sr.ReadLine();

                        try
                        {
                            bogList.Add(JsonConvert.DeserializeObject<Bog>(kommando2));
                        }
                        catch (Exception e)
                        {
                            sw.WriteLine("Forkert syntaks. Skriv /h for hjælp!");
                        }
                    }
                    else if (kommando.Equals("STOP"))
                    {
                        stop = true;
                        socket.Close();
                        Environment.Exit(1);
                    }
                    else if (kommando.Equals("/h"))
                    {
                        sw.WriteLine("Mulige kommandoer:");
                        sw.WriteLine("HentAlle");
                        sw.WriteLine("- Henter alle bøger.");
                        sw.WriteLine("Hent");
                        sw.WriteLine("<<ISBN13>>");
                        sw.WriteLine("- Henter alle bestemt bog ud fra ISBN13.");
                        sw.WriteLine("Gem");
                        sw.WriteLine("<<bog som json>>");
                        sw.WriteLine("- Tilføjer/gemmer en bog, som skal formuleres i JSON format.");
                        sw.WriteLine("  Eksempel på JSON format:");
                        sw.WriteLine("  {\"Titel\":\"UML\",\"Forfatter\":\"Larman\",\"Sidetal\":654,\"Isbn13\":\"9780133594140\"}\r");
                    }
                    else sw.WriteLine("Forkert syntaks. Skriv /h for hjælp!");

                    sw.Flush();
                }
            }

            socket?.Close();
        }
    }
}
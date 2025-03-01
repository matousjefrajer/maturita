using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;

namespace matura_2pc
{
    internal class Game
    {
        static bool endofgame = true;
        static bool SkipInput;
        static string receivedMessage = "";
        static int CardCount = 0;
        static bool ScoreBoard = false;
       
        static public void Comunication()
        {
            while (endofgame)
            {
                SkipInput = false;
                
                try
                {   
                    bool wronginput = true;

                    using (UdpClient udpClient = new UdpClient(Client.Port))
                    {
                        IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(Client.ServerIP), Client.Port);

                        byte[] receivedData = udpClient.Receive(ref serverEndPoint);
                        receivedMessage = Encoding.UTF8.GetString(receivedData);
                        
                        DecryptionOfTheMessage();
                        if (ScoreBoard == true) { break; }

                        if (SkipInput == false)
                        {
                            do
                            {

                                if (int.TryParse(Console.ReadLine(), out int response))
                                {
                                    if (response < CardCount && response > 0 || !receivedMessage.Contains("jakou barvu chceš") && response <= CardCount) //&& response > 0
                                    {
                                        byte[] sendresponse = Encoding.UTF8.GetBytes(response.ToString());
                                        Thread.Sleep(100); // bez něj se to občas sekne když je někdo rychlej
                                        udpClient.Send(sendresponse, sendresponse.Length, serverEndPoint);
                                       
                                        udpClient.Close();
                                        wronginput = false;
                                    }
                                    else
                                    {
                                        if (receivedMessage.Contains("jakou barvu chceš"))
                                        {
                                            Console.WriteLine($"     Máš poue 4 možnosti a nesmíš 0 ");
                                        }
                                        else
                                        {
                                            Console.WriteLine($"     Máš pouze {CardCount} karet");
                                        }
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("     Zkus to znova");
                                }
                            } while (wronginput);
                        }

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Chyba: {ex.Message}");
                    Console.WriteLine("poukud je toto spuštěno na stejném počítači jako server, tak jen chvíli počkej");
                }
            }
            Environment.Exit(0);
        }
        static void DecryptionOfTheMessage()
        { 
            if (receivedMessage.Contains("Scoreboard"))
            {
                Console.Clear();
                
                Console.WriteLine($"{receivedMessage}");
                endofgame = false;
               
                ScoreBoard = true;
            }
            else if (receivedMessage.Contains("DOHRÁL jsi") || receivedMessage.Contains("VYHRÁL jsi"))
            {
                Console.ForegroundColor = ConsoleColor.Red; //chatgpt
                Console.WriteLine($"{     receivedMessage}"); ;
                Console.ResetColor();   
                SkipInput = true;
            }
            else if (receivedMessage.Contains("informace"))
            {
                string[] parts = receivedMessage.Split('.');

                Visuals.UpdateHistory(parts[0]);
                
                string[] players = parts[1].Split("\n", StringSplitOptions.RemoveEmptyEntries);
                Visuals.UpdatePlayers(players);

                Visuals.UpdateScreen();
                
                SkipInput = true;
                

            }
            else if (receivedMessage.Contains("firstinfo"))
            {
                string[] parts = receivedMessage.Split('.');

                string[] players = parts[1].Split("\n", StringSplitOptions.RemoveEmptyEntries);
                Visuals.UpdatePlayers(players);

                string[] Cards = parts[0].Split(',');

                Visuals.Cards = Cards[1];
                Visuals.LastCard = Cards[0];
                Visuals.UpdateScreen();
                
                SkipInput = true;
            }
            else if (receivedMessage.Contains("onturn"))
            {
                string[] parts = receivedMessage.Split('.');

                Visuals.WhoIsOnTurn = parts[0];
               
                SkipInput = true;
                Visuals.UpdateScreen();

            }
            else if (receivedMessage == "MAUMAUPLAYER")
            {
                // děje se, pokud je nějaký hráč na stejné zařízení jako server
                SkipInput = true;

            }
            else if (receivedMessage.Contains("zkus to znova"))
            {
                Console.WriteLine($"     {receivedMessage}");///blbá zpráva od {serverEndPoint.Address}: \n
                                                        ///Console.WriteLine($"počet karet {CardCount} ");
            }
            else if (receivedMessage.Contains("jakou barvu chceš")) //pokud je to ta normalni zprava co jde delit tak deli, jinak mi ten pocet zustava
            {
                ///Console.WriteLine("sem sdse to dostalo23...");

                Console.WriteLine($"{receivedMessage}");///Přijatá zpráva od {serverEndPoint.Address}: \n
                CardCount = 5; //vybírá ze 4 barev, ale necham to jako karty pro zjednodušení kodu
                ///Console.WriteLine($"počet možnmostí {CardCount} ");
                Console.WriteLine("     Napiš číslo barvy, kterou chceš");
            }
            else if (receivedMessage.Contains("Otáčí se") || receivedMessage.Contains("Rozdaly se") || receivedMessage.Contains("DOHRÁL") || receivedMessage.Contains("VYHRÁL") || receivedMessage.Contains("ZAČALA HRA")) //pokud je to ta normalni zprava co jde delit tak deli, jinak mi ten pocet zustava + rozdaly se karty
            {
                Console.WriteLine($"{receivedMessage}");
                Visuals.UpdateHistory(receivedMessage);
                SkipInput = true;

            }
            else //(receivedMessage != "jsi blbej, zkus to znova" && !receivedMessage.Contains("jakou barvu chceš")) //pokud je to ta normalni zprava co jde delit tak deli, jinak mi ten pocet zustava
            {
                //Console.Clear();
                //Console.WriteLine("čistím");
                //Console.WriteLine($"{LastMessage}"); //zanechání posledního tahu
                //LastMessage = "";
                string[] parts = receivedMessage.Split('.');

                //mainMessage = parts[0]; //počtu karet tam neni
                string[] Cards = parts[0].Split('|');

                Visuals.Cards = Cards[1];
                Visuals.LastCard = Cards[0];
                

                CardCount = int.Parse(parts[1]); //počet karet (za tečkou)
                                                 //Console.WriteLine($"");
                                                 //Console.WriteLine($"{mainMessage}");///Přijatá zpráva od {serverEndPoint.Address}: \n
                ///Console.WriteLine($"počet karet {CardCount} ");
                //Visuals.Game = "";
                Visuals.UpdateScreen();

                
                 

                Console.WriteLine($"");

                Console.ForegroundColor = ConsoleColor.Red; //chatgpt
                Console.WriteLine("     Napiš číslo karty, kterou chceš zahrát, nebo 0 pro líznutí si");
                Console.ResetColor();
            }
        }
        
    }
}


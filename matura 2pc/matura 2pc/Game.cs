using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace matura_2pc
{
    internal class Game
    {
        static public void Comunication()
        {
            //Console.WriteLine("zmackni neco");
            //Console.ReadKey();

            int CardCount = 0;
            while (true)
            {
                //UdpClient udpServer = null;

                try
                {
                    UdpClient udpServer = new UdpClient(Client.Port);
                    Console.WriteLine("Čekám na zprávu...");

                    IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(Client.ServerIP), Client.Port);

                    byte[] receivedData = udpServer.Receive(ref serverEndPoint);
                    string receivedMessage = Encoding.UTF8.GetString(receivedData);
                    Console.WriteLine("sem sdse to dostalo...");


                    
                    if (receivedMessage == "jsi blbej, zkus to znova")
                    {
                        Console.WriteLine($"blbá zpráva od {serverEndPoint.Address}: \n{receivedMessage}");
                        Console.WriteLine($"počet karet {CardCount} ");
                    }
                    else if (receivedMessage.Contains("jakou barvu chceš")) //pokud je to ta normalni zprava co jde delit tak deli, jinak mi ten pocet zustava
                    {
                        Console.WriteLine("sem sdse to dostalo23...");
                        //string[] parts = receivedMessage.Split('.');

                        //string mainMessage = parts[0]; //počtu karet tam neni
                        //CardCount = int.Parse(parts[1]); //počet karet (za tečkou)
                        Console.WriteLine($"Přijatá zpráva od {serverEndPoint.Address}: \n{receivedMessage}");
                        CardCount = 4; //vybírá ze 4 barev, ale necham to jako karty pro zjednodušení kodu
                        Console.WriteLine($"počet možnmostí {CardCount} ");
                    }
                    else //(receivedMessage != "jsi blbej, zkus to znova" && !receivedMessage.Contains("jakou barvu chceš")) //pokud je to ta normalni zprava co jde delit tak deli, jinak mi ten pocet zustava
                    {
                        string[] parts = receivedMessage.Split('.');

                        string mainMessage = parts[0]; //počtu karet tam neni
                        CardCount = int.Parse(parts[1]); //počet karet (za tečkou)
                        Console.WriteLine($"Přijatá zpráva od {serverEndPoint.Address}: \n{mainMessage}");
                        Console.WriteLine($"počet karet {CardCount} ");
                    }
                    //else
                    //{
                    //    Console.WriteLine($"blbá zpráva od {serverEndPoint.Address}: \n{receivedMessage}");
                    //    Console.WriteLine($"počet karet {CardCount} ");
                    //}



                    Console.WriteLine("write number of the card you wanna play or 0 to draw");
                    //Console.WriteLine($"počet karet {CardCount} ");
                    //int CardCount = receivedMessage.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).Length - 2; //poradil chat

                    //Console.WriteLine($"parag {paragraphCount} ");

                    bool wronginput = true;
                    do
                    {

                        if (int.TryParse(Console.ReadLine(), out int response))
                        {
                            if (response <= CardCount) //&& response > 0
                            {
                                byte[] sendresponse = Encoding.UTF8.GetBytes(response.ToString());
                                Thread.Sleep(500); // bez něj se to občas sekne když je někdo rychlej
                                udpServer.Send(sendresponse, sendresponse.Length, serverEndPoint);
                                Console.WriteLine("you tried to play");

                                udpServer.Close();
                                wronginput = false;
                            }
                            else
                            {
                                Console.WriteLine($"you have only {CardCount} options, or you cant write 0 ");
                                //wronginput = false;

                            }
                        }
                        else
                        {
                            Console.WriteLine("wrong number,try again");
                        }
                    } while (wronginput);




                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Chyba: {ex.Message}");
                    Console.WriteLine("poukud je toto spuštěno na stejném počítači jako server, tak jen chvíli počkej");

                }

            }


        }
    }
}


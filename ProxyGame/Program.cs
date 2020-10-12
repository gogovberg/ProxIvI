using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Net.NetworkInformation;
using System.Threading;

namespace ProxyGame
{
    public class Program
    {
        public static void Main(string[] args)
        {

            List<ProxyItem> itemList = new List<ProxyItem>();

            itemList.Add(new ProxyItem("77.119.240.209", 8080));
            itemList.Add(new ProxyItem("85.238.167.170", 51915));
            itemList.Add(new ProxyItem("217.196.81.221", 54828));
            itemList.Add(new ProxyItem("178.115.231.163", 8080));
            itemList.Add(new ProxyItem("188.20.125.238", 8080));
            itemList.Add(new ProxyItem("178.189.92.118", 3129));
            itemList.Add(new ProxyItem("128.131.36.188", 8888));
            itemList.Add(new ProxyItem("93.82.197.107", 3129));

            ProxyGame proxyGame = new ProxyGame(itemList);
            proxyGame.Start();

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();

        }

      

    }
}

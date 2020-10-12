using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Timers;

namespace ProxyGame
{
    public class ProxyGame
    {

        private ProxyItem _iPAddress;

        private List<ProxyItem> _addressList;

        private int _status;


        System.Timers.Timer _pingGoogleCheckTimer;


        public ProxyGame(ProxyItem IPAddress)
        {
            _addressList = new List<ProxyItem>();
            _addressList.Add(IPAddress);
            _iPAddress = null;
            _status = 0;
        }
        public ProxyGame(List<ProxyItem> AddressList)
        {
            _addressList = AddressList;
            _iPAddress = null;
            _status = 0;
        }
        public ProxyGame()
        {
            _addressList = null;
            _iPAddress = null;
            _status = 0;
        }

        public bool Start()
        {
            bool result = false;
            Clear();
            try
            {
                if (_addressList != null && _addressList.Count > 0)
                {
                    foreach (ProxyItem item in _addressList)
                    {
                        if (Ping(item) && CheckGoogle(item))
                        {
                            _iPAddress = item;
                            break;

                        }
                        else
                        {
                            Clear();
                        }
                    }

                }
                if (_iPAddress != null)
                {
                    Set(_iPAddress);
                    _status = 1;
                    _pingGoogleCheckTimer = new System.Timers.Timer(5000);
                    _pingGoogleCheckTimer.Elapsed += new ElapsedEventHandler(PingAndCheckGoogle);
                    _pingGoogleCheckTimer.Enabled = true;
                }

                LogToConsole("ProxyGame Start");

            }
            catch (Exception ex)
            {
                Console.WriteLine("ProxyGame Start Error: " + ex.ToString());
            }

            return result;
        }
        public void Stop()
        {
            _addressList = null;
            _iPAddress = null;
            _status = 0;
            if(_pingGoogleCheckTimer!=null)
            {
                _pingGoogleCheckTimer.Enabled = false;
                _pingGoogleCheckTimer = null;
            }


            LogToConsole("ProxyGame Stop");
        }
        bool Set(ProxyItem IPAddress)
        {
            bool result = false;
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);

                string prevValueEnable = registryKey.GetValue("ProxyEnable").ToString();
                string prevValueServer = registryKey.GetValue("ProxyServer").ToString();

                registryKey.SetValue("ProxyEnable", 1);
                registryKey.SetValue("ProxyServer", IPAddress.ToString());

                registryKey.Close();

                result = true;

                LogToConsole("ProxyGame Set for item: ", IPAddress);
            }
            catch(Exception ex)
            {
                LogToConsole("ProxyGame Set Error: " + ex.ToString(), IPAddress);
              
            }

            return result;

        }
        bool Ping(ProxyItem IPAddress)
        {
            LogToConsole("ProxyGame Ping start for item ", IPAddress);
       
            bool result = false;
            Ping p = new Ping();
            try
            {
                int iter = 0;
                bool status = false;
                PingReply reply = p.Send(IPAddress.Ip, 1000);
                while (iter < 10)
                {
                    reply = p.Send(IPAddress.Ip, 1000);
                    status = reply.Status == IPStatus.Success;
                    iter++;

                }

                Console.WriteLine(reply.RoundtripTime + " " + reply.Status);
                LogToConsole("ProxyGame Ping end for item ", IPAddress);
                return status;
            }
            catch (Exception ex)
            {
                LogToConsole("ProxyGame Ping Error: " + ex.ToString(), IPAddress);
         
            }
            return result;
        }
        bool CheckGoogle(ProxyItem IPAddress)
        {

            LogToConsole("ProxyGame CheckGoogle start for item: ", IPAddress);
            bool result = false;
         
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://google.com");
                WebProxy webProxy = new WebProxy(IPAddress.Ip, IPAddress.Port);
                request.Proxy = webProxy;
                //request.Timeout = 5000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

               
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream receiveStream = response.GetResponseStream();
                    StreamReader readStream = null;

                    if (String.IsNullOrWhiteSpace(response.CharacterSet))
                    {
                        readStream = new StreamReader(receiveStream);
                    } 
                    else
                    {
                        readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                    }
                        

                    string data = readStream.ReadToEnd();

                    response.Close();
                    readStream.Close();
                    result = true;
                    IPAddress.Status = result;
                    LogToConsole("ProxyGame CheckGoogle end for item: ", IPAddress);

                }

            }
            catch (Exception ex)
            {
                LogToConsole("ProxyGame CheckGoogle Error: "+ex.ToString(), IPAddress);
            }
            LogToConsole("ProxyGame CheckGoogle for item: ", IPAddress);
          
            return result;
        }
        public bool Clear()
        {

            bool result = false;
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);

                string prevValueEnable = registryKey.GetValue("ProxyEnable").ToString();
                string prevValueServer = registryKey.GetValue("ProxyServer").ToString();

                registryKey.SetValue("ProxyEnable", 0);
                registryKey.SetValue("ProxyServer", "");

                registryKey.Close();

                result = true;

           
                LogToConsole("ProxyGame Clear");

            }
            catch (Exception ex)
            {
                LogToConsole("ProxyGame Clear Error: " + ex.ToString());
             
            }
            return result;
        }
        void LogToConsole(string msg, ProxyItem IPAddress=null)
        {
            string ip = "";
            if (IPAddress!=null)
            {
                ip = IPAddress.ToString() + " Status: " + IPAddress.Status;
            }

            string message = DateTime.UtcNow.ToString("s", CultureInfo.CreateSpecificCulture("de-DE")) +" - "+msg +" - "+ip;
            Console.WriteLine(message);
        }
        
        void PingAndCheckGoogle(object sender, ElapsedEventArgs e)
        {
            if (!Ping(_iPAddress) || CheckGoogle(_iPAddress))
            {
                Clear();
                Stop();
                Start();
            }

        }
    
    }

    public class ProxyItem
    {
        private string _ip;
        private int _port;
        public bool Status { set; get; }
        public string Ip
        {
            get { return _ip; }
        }
        public int Port
        {
            get { return _port; }
        }

        public ProxyItem(string IP, int Port)
        {
            _ip = IP;
            _port = Port;
            Status = false;
        }
        public override string ToString()
        {
            string result = "";
            if(!string.IsNullOrEmpty(_ip) && _port>0)
            {
                result = _ip + ":" + _port;
            }
            return result;
        }
    }
}

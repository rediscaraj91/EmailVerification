using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ARSoft.Tools.Net.Dns;


namespace VerificationDos
{
    class Program
    {
        public static string mailAddr = "";
        public static TextWriter textExport;
        public static string conf_path = System.IO.Directory.GetCurrentDirectory() + "\\log.txt";
        static void Main(string[] args)
        {
            textExport = new StreamWriter(conf_path, false, Encoding.UTF8);
            string[] mailAddrs = {
                "contactus@1800caught.com.au", "seal_it@unwired.com.au", "help@1800ithelp.com.au",
                "getfit@180degrees.com.au", "mascot@1car1.com.au", "southcoasttermites@netspeed.com",
                "1otsbd@westnet.com.au", "michael@ipo.com.au", "brett@1stclassmobiledetailing.com.au",
                "1stclasstowing@gmail.com", "sbrown@1stfleet.com.au", "info@1st-impressions.com.au",
                "enquiries@24-7media.com.au", "sales@247studios.com.au", "info@24-7vending.com.au"

            };
            writelogfile();
            //writelogfile("-Input eMail address.");
            //mailAddr = Console.ReadLine();

            foreach( var addr in mailAddrs)
            {
                Process(addr);
            }            
            
            writelogfile();
            writelogfile();

            textExport.Flush();
            textExport.Close();
            //   WaitAnyKey();
        }
        private static void Process(string addr)
        {
            writelogfile();
            writelogfile(addr);

            if (IsValidEmail(addr) == false)
            {
                writelogfile();
                writelogfile(addr + " is invalid mail address" );
                //   WaitAnyKey();
                return;
            }

            string[] host = (addr.Split('@'));
            string hostname = host[1];

            var resolver = new DnsStubResolver();

            try
            {
                var records = resolver.Resolve<MxRecord>(hostname, RecordType.Mx);

                if (records.Count == 0)
                {
                    writelogfile("MX server don't exists");
                    return;
                }
                else
                {
                    int exist = 0;
                    foreach (var record in records)
                    {
                        if (record.ExchangeDomainName?.ToString() != null)
                        {
                            string server = record.ExchangeDomainName?.ToString();
                            writelogfile();
                            writelogfile(server);
                            exist = IsEmailAccountValid(server, addr);

                            //  if (exist != 0) writelogfile("\tResp:" + exist.ToString() + ":   email account exists");
                            //  else writelogfile("\temail account don't exists");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                writelogfile("Error : " + ex.Message);

            }
        }
        private static void writelogfile(string log)
        {
            string delimeter = Environment.NewLine;
            textExport.Write(log);
            Console.WriteLine(log);
            textExport.Write(delimeter);
        }

        private static void writelogfile()
        {
            string delimeter = Environment.NewLine;
            textExport.Write(" ");
            textExport.Write(delimeter);
        }

        private static int IsEmailAccountValid(string tcpClient, string emailAddress)
        {
            TcpClient tClient = null; 
            try
            {
                tClient = new TcpClient(tcpClient, 25);
                string CRLF = "\r\n";
                byte[] dataBuffer;
                string ResponseString;
                NetworkStream netStream = tClient.GetStream();
                StreamReader reader = new StreamReader(netStream);
                ResponseString = reader.ReadLine();
                WriteResponse(ResponseString);

                // Perform HELO to SMTP Server and get Response

                if (tcpClient.IndexOf("google", StringComparison.OrdinalIgnoreCase) != -1 )
                    dataBuffer = BytesFromString("HELO Hi" + CRLF);                
                else
                    dataBuffer = BytesFromString("HELO" + CRLF);

                netStream.Write(dataBuffer, 0, dataBuffer.Length);
                ResponseString = reader.ReadLine();
                WriteResponse(ResponseString);

                dataBuffer = BytesFromString("MAIL FROM:<rediscaraj91@gmail.com>" + CRLF);
                netStream.Write(dataBuffer, 0, dataBuffer.Length);
                ResponseString = reader.ReadLine();
                WriteResponse(ResponseString);

                // Read Response of the RCPT TO Message to know from google if it exist or not
                dataBuffer = BytesFromString("RCPT TO:<" + emailAddress + ">" + CRLF);
                netStream.Write(dataBuffer, 0, dataBuffer.Length);
                ResponseString = reader.ReadLine();
                WriteResponse(ResponseString);

                /* QUITE CONNECTION */
                int ret = GetResponseCode(ResponseString);
                dataBuffer = BytesFromString("QUITE" + CRLF);
                netStream.Write(dataBuffer, 0, dataBuffer.Length);
                tClient.Close();

                return ret;
            }
            catch(Exception ex)
            {
                if(tClient != null) tClient.Close();
                writelogfile("Error: " +  ex.Message);
                return 0;
            }
            
           
        }
        private static void WriteResponse(string respons)
        {
            writelogfile("\t" + respons);
        }
        private static byte[] BytesFromString(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }

        private static int GetResponseCode(string ResponseString)
        {
            return int.Parse(ResponseString.Substring(0, 3));
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        private static void WaitAnyKey()
        {
            writelogfile("Press any key to exit...");
            ConsoleKeyInfo k;
            while (true)
            {
                k = Console.ReadKey(true);
                break;
            }

        }
    }
}

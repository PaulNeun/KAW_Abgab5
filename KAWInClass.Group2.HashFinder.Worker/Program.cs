using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using KAWInClass.Group2.HashFinder.Common;

namespace KAWInClass.Group2.HashFinder.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            List<Thread> threads = new List<Thread>();
            for (int threadIndex = 0; threadIndex < 3; threadIndex++)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(MainPerClient));
                thread.Start(threadIndex);
                threads.Add(thread);
            }

            foreach (Thread thread in threads)
            {
                thread.Join(); // wait till end
            }
        }

        public static void MainPerClient(object parameter)
        {
            int clientId = (int)parameter;
            Console.WriteLine($"[{clientId}] client started");

            IConnectionFactory factory = new ConnectionFactory("tcp://localhost:61616");
            ((ConnectionFactory)factory).PrefetchPolicy.SetAll(0);

            using IConnection connection = factory.CreateConnection();
            connection.Start();

            using ISession session = connection.CreateSession();
            using IDestination destination = session.GetQueue("KAW.MD5Hasher");

            using IMessageConsumer consumer = session.CreateConsumer(destination);

            IMessage message;
            using (MD5 md5Hash = MD5.Create())
            {
                while ((message = consumer.Receive(TimeSpan.FromSeconds(10))) != null)
                {
                    IObjectMessage objectMessage = message as IObjectMessage;
                    HFMessage hfMessage = objectMessage.Body as HFMessage;
                    Console.WriteLine($"[{clientId}] message recv (start: {hfMessage.Start} - end: {hfMessage.End})");

                    // Extracted the Logic Part of the Application so that it's testable
                    Console.WriteLine(TryHackMessage(hfMessage, md5Hash, clientId));

                }
            }
        }


        /*
         * Found Problem 1: If HfMessage equals null, than there is a NullPointerException --> Check for null and return Error
         * Found Problem 2: If md5Hash equals null, thank there is a NullPointerException --> Check for null and return Error
         * Found kind of a Problem 3: If the ending of the Range is lower than the start, than the Method will just behave like it hasn't found something -->
         * --> Changed it for higher usability (Now checks and gives a neat error message back)
         * Found kind of a Problem 4: If the HashedSecret in the HfMessage is not set the method will check each guess versus null and will just -->
         * exit with nothing found --> Changed it for higher usability (Now checks and gives a neat error message back)
         * */

        public static string TryHackMessage(HFMessage hfMessage, MD5 md5Hash, int clientId)
        {
            if (hfMessage == null) return "Error: Message equals null"; //Change Nr1
            if (md5Hash == null) return "Error: Hash equals null"; //Change Nr2
            if (hfMessage.End < hfMessage.Start) return "Error: Ending range can not be lower than starting range"; //Change Nr3
            if (hfMessage.HashedSecret == null) return "Error: The Hashed Secret in the Message is not set (null)"; //Change Nr4
            for (int guess = hfMessage.Start; guess <= hfMessage.End; guess++)
            {
                string hashedGuess = GetMd5Hash(md5Hash, guess.ToString());

                if (hashedGuess == hfMessage.HashedSecret)
                {
                    return $"[{clientId}] found secret: {guess} with hash {hashedGuess}";
                }
            }

            return String.Empty; //To not create a bunch of unnecessary outputs this is left empty 
        }

        public static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }
}

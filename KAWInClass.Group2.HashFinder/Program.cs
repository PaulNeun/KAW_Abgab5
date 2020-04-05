using System;
using System.Security.Cryptography;
using System.Text;
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using KAWInClass.Group2.HashFinder.Common;

namespace KAWInClass.Group2.HashFinder
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("enter a number> ");
            var rawInput = Console.ReadLine();
            int secret = int.Parse(rawInput);

            using (MD5 md5Hash = MD5.Create())
            {
                var hashedSecret = GetMd5Hash(md5Hash, secret.ToString());
                Console.WriteLine(hashedSecret);

                IConnectionFactory factory = new ConnectionFactory("tcp://localhost:61616");
                using IConnection connection = factory.CreateConnection();
                connection.Start();

                using ISession session = connection.CreateSession();
                using IDestination destination = session.GetQueue("KAW.MD5Hasher");
                using IMessageProducer producer = session.CreateProducer(destination);

                for (int messageNr = 0; messageNr < 20; messageNr++)
                {
                    IMessage message = producer.CreateObjectMessage(new HFMessage()
                    {
                        Start = messageNr * 100000000,
                        End = ((messageNr + 1) * 100000000) - 1,
                        HashedSecret = hashedSecret,
                    });
                    producer.Send(message);
                }

                producer.Close();
                session.Close();
                connection.Close();

                /*
                for (int guess = 0; guess < int.MaxValue; guess++)
                {
                    string hashedGuess = GetMd5Hash(md5Hash, guess.ToString());

                    if (hashedGuess == hashedSecret)
                    {
                        Console.WriteLine($"found secret: {guess} with hash {hashedGuess}");
                        break;
                    }
                }*/

            }
        }

        static string GetMd5Hash(MD5 md5Hash, string input)
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

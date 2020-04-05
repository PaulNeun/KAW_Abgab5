using System;
using System.Security.Cryptography;
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Apache.NMS.ActiveMQ.Commands;
using KAWInClass.Group2.HashFinder.Common;
using Moq;
using NUnit.Framework;

namespace KAWInClass.Group2.HashFinder.Worker.Tests
{
    /*
     * The arrange Part of all the tests is kind of the same and it's bulked up. Is there a better way that is still nice to check?
     */

    [TestFixture]
    public class Tests
    {
        [Test]
        public void TryHackMessage_HashInRange()
        {
            // arrange
            int clientId = 1;
            int secret = 99;

            MD5 md5Hash = MD5.Create();
            string correctMd5Hash = Program.GetMd5Hash(md5Hash, secret.ToString());

            HFMessage message = new HFMessage()
            {
                Start = 1,
                End = 100,
                HashedSecret = correctMd5Hash
            };

            string actual, expected = $"[{clientId}] found secret: {secret} with hash {correctMd5Hash}";

            // act
            actual = Program.TryHackMessage(message, md5Hash, clientId);

            // assert
            Assert.AreEqual(expected, actual);
        }



        [Test]
        public void TryHackMessage_HashOutsideRange()
        {
            // arrange
            int clientId = 1;
            int secret = 101;

            MD5 md5Hash = MD5.Create();
            string correctMd5Hash = Program.GetMd5Hash(md5Hash, secret.ToString());

            HFMessage message = new HFMessage()
            {
                Start = 1,
                End = 100,
                HashedSecret = correctMd5Hash
            };

            string actual, expected = "";

            // act
            actual = Program.TryHackMessage(message, md5Hash, clientId);

            // assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TryHackMessage_HashExactlyAtStart()
        {
            // arrange
            int clientId = 1;
            int secret = 1;

            MD5 md5Hash = MD5.Create();
            string correctMd5Hash = Program.GetMd5Hash(md5Hash, secret.ToString());

            HFMessage message = new HFMessage()
            {
                Start = 1,
                End = 100,
                HashedSecret = correctMd5Hash
            };

            string actual, expected = $"[{clientId}] found secret: {secret} with hash {correctMd5Hash}";

            // act
            actual = Program.TryHackMessage(message, md5Hash, clientId);

            // assert
            Assert.AreEqual(expected, actual);
        }

        /*
         * Not really a problem, but i modified it so that the end is inclusive, because I find it more logical if I say check from e.g. 1 to 100 that the
         * program checks from 1-100 and not 1,2, ....,98, 99
         */
        [Test]
        public void TryHackMessage_HashExactlyAtEnd()
        {
            // arrange
            int clientId = 1;
            int secret = 100;

            MD5 md5Hash = MD5.Create();
            string correctMd5Hash = Program.GetMd5Hash(md5Hash, secret.ToString());

            HFMessage message = new HFMessage()
            {
                Start = 1,
                End = 100,
                HashedSecret = correctMd5Hash
            };

            string actual, expected = $"[{clientId}] found secret: {secret} with hash {correctMd5Hash}";

            // act
            actual = Program.TryHackMessage(message, md5Hash, clientId);

            // assert
            Assert.AreEqual(expected, actual);
        }

        /*
         * Takes around 5.8 seconds on my computer
         */
        [Test]
        public void TryHackMessage_LargeHashNumberWithLargeRange()
        {
            // arrange
            int clientId = 1;
            int secret = 4965634;

            MD5 md5Hash = MD5.Create();
            string correctMd5Hash = Program.GetMd5Hash(md5Hash, secret.ToString());

            HFMessage message = new HFMessage()
            {
                Start = 1,
                End = 10000000,
                HashedSecret = correctMd5Hash
            };

            string actual, expected = $"[{clientId}] found secret: {secret} with hash {correctMd5Hash}";

            // act
            actual = Program.TryHackMessage(message, md5Hash, clientId);

            // assert
            Assert.AreEqual(expected, actual);
        }



        /*
         * Found Problem 1: If HfMessage equals null there is a NullPointerException --> Check for null and return an Error
         */
        [Test]
        public void TryHackMessage_NullMessage()
        {
            // arrange
            int clientId = 1;
            int secret = 57;

            MD5 md5Hash = MD5.Create();
            string correctMd5Hash = Program.GetMd5Hash(md5Hash, secret.ToString());

            HFMessage message = null;

            string actual, expected = "Error: Message equals null";

            // act
            actual = Program.TryHackMessage(message, md5Hash, clientId);

            // assert
            Assert.AreEqual(expected, actual);
        }

        /*
         * Found Problem 2: If md5Hash equals null, thank there is a NullPointerException --> Check for null and return Error
         */
        [Test]
        public void TryHackMessage_NullHash()
        {
            // arrange
            int clientId = 1;
            int secret = 57;

            MD5 md5Hash = MD5.Create();
            string correctMd5Hash = Program.GetMd5Hash(md5Hash, secret.ToString());

            HFMessage message = new HFMessage()
            {
                Start = 1,
                End = 100,
                HashedSecret = correctMd5Hash
            };

            string actual, expected = "Error: Hash equals null";

            // act
            actual = Program.TryHackMessage(message, null, clientId); //Mad the null here, otherwise I would have to check the GetMd5Hash aswell

            // assert
            Assert.AreEqual(expected, actual);
        }

        /*
         * Found kind of a Problem 3: If the ending of the Range is lower than the start, than the Method will just behave like it hasn't found something -->
         * --> Changed it for higher usability (Now checks and gives a neat error message back)
         */
        [Test]
        public void TryHackMessage_RangeEndIsSmallerThanStart()
        {
            // arrange
            int clientId = 1;
            int secret = 57;

            MD5 md5Hash = MD5.Create();
            string correctMd5Hash = Program.GetMd5Hash(md5Hash, secret.ToString());

            HFMessage message = new HFMessage()
            {
                Start = 100,
                End = 1,
                HashedSecret = correctMd5Hash
            };

            string actual, expected = "Error: Ending range can not be lower than starting range";

            // act
            actual = Program.TryHackMessage(message, md5Hash, clientId);

            // assert
            Assert.AreEqual(expected, actual);
        }

        // Expected to not find anything
        [Test]
        public void TryHackMessage_HashedSecretIsNotANumber()
        {
            // arrange
            int clientId = 1;
            string secret = "testing";

            MD5 md5Hash = MD5.Create();
            string correctMd5Hash = Program.GetMd5Hash(md5Hash, secret);

            HFMessage message = new HFMessage()
            {
                Start = 1,
                End = 100,
                HashedSecret = correctMd5Hash
            };

            string actual, expected = "";

            // act
            actual = Program.TryHackMessage(message, md5Hash, clientId);

            // assert
            Assert.AreEqual(expected, actual);
        }

        // Expected not to find anything because the properties in HfMessage will automatically be set to 0 and than the method has a range from 0 - 0
        [Test]
        public void TryHackMessage_MessageRangeNotSet()
        {
            // arrange
            int clientId = 1;
            int secret = 99;

            MD5 md5Hash = MD5.Create();
            string correctMd5Hash = Program.GetMd5Hash(md5Hash, secret.ToString());

            HFMessage message = new HFMessage()
            {
                HashedSecret = correctMd5Hash
            };

            string actual, expected = "";

            // act
            actual = Program.TryHackMessage(message, md5Hash, clientId);

            // assert
            Assert.AreEqual(expected, actual);
        }

        /*
         * Found kind of a Problem 4: If the HashedSecret in the HfMessage is not set the method will check each guess versus null and will just -->
         * exit with nothing found --> Changed it for higher usability (Now checks and gives a neat error message back)
         */
        [Test]
        public void TryHackMessage_MessageHasNoHashedSecret()
        {
            // arrange
            int clientId = 1;
            int secret = 99;

            MD5 md5Hash = MD5.Create();
            string correctMd5Hash = Program.GetMd5Hash(md5Hash, secret.ToString());

            HFMessage message = new HFMessage()
            {
                Start = 0,
                End = 100
            };

            string actual, expected = "Error: The Hashed Secret in the Message is not set (null)";

            // act
            actual = Program.TryHackMessage(message, md5Hash, clientId);

            // assert
            Assert.AreEqual(expected, actual);
        }

    }
}
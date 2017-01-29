﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Security;

namespace ElectionAuthority
{
    class SerialNumberGenerator
    {
        public static List<BigInteger> generateListOfSerialNumber(int numberOfSerials, int numberOfBits)
        {

            List<BigInteger> listOfSerialNumber = new List<BigInteger>();
            RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
            byte[] data = new byte[numberOfBits];
            random.GetBytes(data);

            BigInteger startValue = new BigInteger(data);
            for (int i = 0; i < numberOfSerials; i++)
            {
                if (i == 0)
                {
                    listOfSerialNumber.Add(startValue.Add(new BigInteger(1.ToString())).Abs());
                }
                else
                {
                    listOfSerialNumber.Add((listOfSerialNumber[i - 1].Add(new BigInteger(1.ToString()))).Abs());
                }

            }

            Utils.Extentions.Shuffle(listOfSerialNumber);
            return listOfSerialNumber;
        }

        public static List<AsymmetricCipherKeyPair> generatePreTokens(int numberOfSerials, int numberOfBits)
        {
            List<AsymmetricCipherKeyPair> preTokens = new List<AsymmetricCipherKeyPair>();
            for (int i = 0; i < numberOfSerials; i++)
            {
                KeyGenerationParameters para = new KeyGenerationParameters(new SecureRandom(), numberOfBits);
                RsaKeyPairGenerator keyGen = new RsaKeyPairGenerator();
                keyGen.Init(para);
                AsymmetricCipherKeyPair keypair = keyGen.GenerateKeyPair();
                preTokens.Add(keypair);
            }

            return preTokens;
        }
    }
}

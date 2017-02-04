﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Generators;

namespace ElectionAuthority
{
    class Auditor
    {
        private RsaKeyParameters privKey;                   

        private RsaKeyParameters pubKey;    

        private BigInteger[] commitedPermutation;
        public BigInteger[] CommitedPermatation
        {
            set { commitedPermutation = value; }
            get { return commitedPermutation; }
        }

        public Auditor()
        {
            initKeyPair();
        }

        public bool checkPermutation(List<BigInteger> explicitPermutation)
        {
            int i=0;
            foreach (BigInteger partPermutation in explicitPermutation)
            {
                // verify using RSA formula
                if (!partPermutation.Equals(commitedPermutation[i].ModPow(privKey.Exponent, pubKey.Modulus)))
                {
                    return false;
                }
                i++;               
            }

            return true;
        }


        public void blindPermutation(List<Permutation> permutationList)
        {
            int size = permutationList.Count;
            BigInteger[] toSend = new BigInteger[size];

            //preparing List of permutation to send
            int k = 0;
            string[] strPermuationList = new string[permutationList.Count];
            foreach (Permutation list in permutationList)
            {
                string str = "";
                foreach (BigInteger big in list.getPermutation())
                {
                    str += big.ToString();
                }
                strPermuationList[k] = str;
                k++;
            }

            //RSA formula (bit commitment)
            int i = 0;
            foreach (string str in strPermuationList)
            {
                BigInteger toBlind = new BigInteger(str);
                BigInteger e = pubKey.Exponent;
                BigInteger n = pubKey.Modulus;
                BigInteger b = toBlind.ModPow(e, n);
                toSend[i] = b;
                i++;
            }
            this.CommitedPermatation = toSend;
        }

        public void unblindPermutation(List<Permutation> permutationList)
        {
            int size = permutationList.Count;
            List<BigInteger> toSend = new List<BigInteger>();

            int k = 0;
            string[] strPermuationList = new string[permutationList.Count];

            foreach (Permutation list in permutationList)
            {
                string str = null;
                foreach (BigInteger big in list.getPermutation())
                {
                    str += big.ToString();
                }
                strPermuationList[k] = str;
                k++;
            }

            foreach (string str in strPermuationList)
            {
                BigInteger b = new BigInteger(str);
                toSend.Add(b);
            }


            //checking permutations RSA (auditor checks all of the permutations)
            if (this.checkPermutation(toSend))
            {
                Utils.Logs.addLog("EA", NetworkLib.Constants.BIT_COMMITMENT_OK, true, NetworkLib.Constants.LOG_INFO, true);
            }
            else
            {
                Utils.Logs.addLog("EA", NetworkLib.Constants.BIT_COMMITMENT_FAIL, true, NetworkLib.Constants.LOG_ERROR, true);
            }
        }

        private void initKeyPair()
        {
            //init key pair generator (for RSA bit-commitment)
            KeyGenerationParameters para = new KeyGenerationParameters(new SecureRandom(), 1024);
            RsaKeyPairGenerator keyGen = new RsaKeyPairGenerator();
            keyGen.Init(para);

            //generate key pair and get keys (for bit-commitment)
            AsymmetricCipherKeyPair keypair = keyGen.GenerateKeyPair();
            this.privKey = (RsaKeyParameters)keypair.Private;
            this.pubKey = (RsaKeyParameters)keypair.Public;
        }
    }
}

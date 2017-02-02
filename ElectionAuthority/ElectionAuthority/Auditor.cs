using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace ElectionAuthority
{
    class Auditor
    {
        private BigInteger[] commitedPermutation;
        public BigInteger[] CommitedPermatation
        {
            set { commitedPermutation = value; }
            get { return commitedPermutation; }
        }

        public Auditor()
        {
        }

        public bool checkPermutation(RsaKeyParameters privateKey, RsaKeyParameters publicKey, List<BigInteger> explicitPermutation)
        {
            int i=0;
            foreach (BigInteger partPermutation in explicitPermutation)
            {
                // verify using RSA formula
                if (!partPermutation.Equals(commitedPermutation[i].ModPow(privateKey.Exponent, publicKey.Modulus)))
                {
                    return false;
                }
                i++;               
            }

            return true;
        }


        public void blindPermutation(List<List<BigInteger>> permutationList, RsaKeyParameters pubKey)
        {
            int size = permutationList.Count;
            BigInteger[] toSend = new BigInteger[size];

            //preparing List of permutation to send
            int k = 0;
            string[] strPermuationList = new string[permutationList.Count];
            foreach (List<BigInteger> list in permutationList)
            {
                string str = "";
                foreach (BigInteger big in list)
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

        public void unblindPermutation(List<List<BigInteger>> permutationList, RsaKeyParameters pubKey, RsaKeyParameters privKey)
        {
            int size = permutationList.Count;
            List<BigInteger> toSend = new List<BigInteger>();

            int k = 0;
            string[] strPermuationList = new string[permutationList.Count];

            foreach (List<BigInteger> list in permutationList)
            {
                string str = null;
                foreach (BigInteger big in list)
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
            if (this.checkPermutation(privKey, pubKey, toSend))
            {
                Utils.Logs.addLog("EA", NetworkLib.Constants.BIT_COMMITMENT_OK, true, NetworkLib.Constants.LOG_INFO, true);
            }
            else
            {
                Utils.Logs.addLog("EA", NetworkLib.Constants.BIT_COMMITMENT_FAIL, true, NetworkLib.Constants.LOG_ERROR, true);
            }
        }

    }
}

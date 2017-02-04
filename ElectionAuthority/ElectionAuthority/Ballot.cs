using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElectionAuthority
{
    class Ballot
    {
        public int numberOfVoters = 5;
        private BigInteger sl;
        public BigInteger SL
        {
            get { return sl; }
        }

        private List<BigInteger> tokenList;
        public List<BigInteger> TokenList
        {
            get { return tokenList; }
            set { tokenList = value; }
        }

        private List<BigInteger> exponentsList;
        public List<BigInteger> ExponentsList
        {
            get { return exponentsList; }
            set { exponentsList = value; }
        }

        private List<BigInteger> signatureFactor;
        public List<BigInteger> SignatureFactor
        {
            get { return signatureFactor; }
            set { signatureFactor = value; }
        }

        private BigInteger[] signedColumn;
        public BigInteger[] SignedColumn
        {
            get { return signedColumn; }
        }

        private BigInteger[] blindColumn;
        public BigInteger[] BlindColumn
        {
            set { blindColumn = value; }
        }

        private string[,] unblindedBallot;
        public string[,] UnblindedBallot
        {
            set { unblindedBallot = value; }
            get { return unblindedBallot; }
        }

        private Permutation permutation;
        public Permutation Permutation
        {
            set { permutation = value; }
            get { return permutation; }
        }

        private BigInteger permutationToken;
        private BigInteger permutationExponent;

        public Ballot(BigInteger SL)
        {
            this.sl = SL;
            this.Permutation = new Permutation(this.numberOfVoters);
            this.permutationToken = new BigInteger("0");
            this.permutationExponent = new BigInteger("0");
            this.generateTokens();
        }

        private void generateTokens()
        {
            List<AsymmetricCipherKeyPair> preToken = new List<AsymmetricCipherKeyPair>(SerialNumberGenerator.generatePreTokens(Convert.ToInt32(this.numberOfVoters), NetworkLib.Constants.NUMBER_OF_BITS_TOKEN));

            List<BigInteger> tokens = new List<BigInteger>();
            List<BigInteger> exps = new List<BigInteger>();
            List<BigInteger> signFactor = new List<BigInteger>();

            foreach (AsymmetricCipherKeyPair token in preToken)
            {
                RsaKeyParameters publKey = (RsaKeyParameters)token.Public;
                RsaKeyParameters prKey = (RsaKeyParameters)token.Private;
                tokens.Add(publKey.Modulus);
                exps.Add(publKey.Exponent);
                signFactor.Add(prKey.Exponent);
            }

            RsaKeyParameters publicKey = (RsaKeyParameters)preToken[0].Public;
            RsaKeyParameters privKey = (RsaKeyParameters)preToken[0].Private;
            permutationToken = (publicKey.Modulus);
            permutationExponent = (publicKey.Exponent);

            this.exponentsList = exps;
            this.tokenList = tokens;
            this.signatureFactor = signFactor;
        }

        public void signColumn()
        {
            BigInteger[] signed = new BigInteger[NetworkLib.Constants.BALLOT_SIZE];
            int i = 0;
            foreach (BigInteger column in blindColumn)
            {
                signed[i] = column.ModPow(signatureFactor[i], tokenList[i]);
                i++;
            }

            this.signedColumn = signed;
        }
    }
}

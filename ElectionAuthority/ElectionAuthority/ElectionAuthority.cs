using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Windows.Forms;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Generators;

namespace ElectionAuthority
{
    public class ElectionAuthority
    {
        ASCIIEncoding encoder;

        private Server serverClient; 
        public Server ServerClient
        {
            get { return serverClient; }
        }

        private CandidateList candidateList;

        private Configuration configuration;

        private Permutation permutation;

        private List<List<BigInteger>> permutationsList;

        private List<List<BigInteger>> inversePermutationList;

        private List<BigInteger> serialNumberList;

        private List<List<BigInteger>> tokensList;          

        private List<List<BigInteger>> exponentsList;  
    
        private List<List<BigInteger>> signatureFactor;     


        private Dictionary<BigInteger, List<BigInteger>> dictionarySLPermuation;

        private Dictionary<BigInteger, List<BigInteger>> dictionarySLInversePermutation;

        private Dictionary<BigInteger, List<List<BigInteger>>> dictionarySLTokens;

        private Dictionary<string, Ballot> ballots;

        private int numberOfVoters;

        private List<int> finalResults;

        private Auditor auditor;                            

        private RsaKeyParameters privKey;                   

        private RsaKeyParameters pubKey;                    

        private List<BigInteger> permutationTokensList;

        private List<BigInteger> permutationExponentsList;


        public ElectionAuthority(string filename)
        {
            Configuration configuration = new Configuration();
            configuration.loadConfiguration(filename);
            this.encoder = new ASCIIEncoding();
            this.configuration = configuration;
            //server for Clients
            this.serverClient = new Server(this);

            this.numberOfVoters = Convert.ToInt32(this.configuration.NumberOfVoters);
            permutation = new Permutation();

            this.ballots = new Dictionary<string, Ballot>();

            this.auditor = new Auditor();

            this.initKeyPair();

            this.loadCandidateList("C:\\Users\\mskwarek\\Documents\\Visual Studio 2015\\Projects\\PKRY\\Config\\CandidateList.xml");
            finalResults = new List<int>(this.candidateList.getNumberOfCandidates());
        }

        private void initKeyPair()
        {
            //init key pair generator (for RSA bit-commitment)
            KeyGenerationParameters para = new KeyGenerationParameters(new SecureRandom(), 1024);
            RsaKeyPairGenerator keyGen = new RsaKeyPairGenerator();
            keyGen.Init(para);

            //generate key pair and get keys (for bit-commitment)
            AsymmetricCipherKeyPair keypair = keyGen.GenerateKeyPair();
            privKey = (RsaKeyParameters)keypair.Private;
            pubKey = (RsaKeyParameters)keypair.Public;
        }

        public void startServices()
        {
            this.serverClient.startServer(configuration.ElectionAuthorityPortClient);
        }

        public void loadCandidateList(string pathToElectionAuthorityConfig)
        {
            candidateList = new CandidateList();
            candidateList.loadCanidateList(pathToElectionAuthorityConfig);
        }

        private void generatePermutation()
        {
            this.generatePermutationList();
            connectSerialNumberAndPermutation();
            generateInversePermutation();
            generatePermutationTokens();
            this.auditor.blindPermutation(permutationsList, this.pubKey);              //Send commited permutation to Auditor
            Utils.Logs.addLog("EA", NetworkLib.Constants.PERMUTATION_GEN_SUCCESSFULLY, true, NetworkLib.Constants.LOG_INFO);
        }

        private void generatePermutationList()
        {
            //generating permutation and feeling List
            permutationsList = new List<List<BigInteger>>();
            for (int i = 0; i < this.numberOfVoters; i++)
            {
                this.permutationsList.Add(new List<BigInteger>(this.permutation.generatePermutation(this.candidateList.getNumberOfCandidates())));
            }
        }

        private void generatePermutationTokens()
        {
            this.permutationTokensList = new List<BigInteger>();
            this.permutationExponentsList = new List<BigInteger>();

            for (int i = 0; i < this.numberOfVoters; i++)
            {
                // we use the same method like to generate serial number, 
                // there is another random generator used inside this method
                List<AsymmetricCipherKeyPair> preToken = new List<AsymmetricCipherKeyPair>(SerialNumberGenerator.generatePreTokens(1, NetworkLib.Constants.NUMBER_OF_BITS_TOKEN));

                RsaKeyParameters publicKey = (RsaKeyParameters)preToken[0].Public;
                RsaKeyParameters privKey = (RsaKeyParameters)preToken[0].Private;
                permutationTokensList.Add(publicKey.Modulus);
                permutationExponentsList.Add(publicKey.Exponent);
            }
            Utils.Logs.addLog("EA", "Permutation tokens generated", true, NetworkLib.Constants.LOG_INFO, true);
        }

        private void generateInversePermutation()
        {
            //using mathematics to generate inverse permutation for our List
            this.inversePermutationList = new List<List<BigInteger>>();
            for (int i = 0; i < this.numberOfVoters; i++)
            {
                this.inversePermutationList.Add(this.permutation.getInversePermutation(this.permutationsList[i]));
            }
            Utils.Logs.addLog("EA", NetworkLib.Constants.GENERATE_INVERSE_PERMUTATION, true, NetworkLib.Constants.LOG_INFO, true);
            connectSerialNumberAndInversePermutation();
        }

        private void connectSerialNumberAndInversePermutation()
        {
            dictionarySLInversePermutation = new Dictionary<BigInteger, List<BigInteger>>();
            for (int i = 0; i < this.serialNumberList.Count; i++)
            {
                dictionarySLInversePermutation.Add(this.serialNumberList[i], this.inversePermutationList[i]);
            }
            Utils.Logs.addLog("EA", NetworkLib.Constants.SL_CONNECTED_WITH_INVERSE_PERMUTATION, true, NetworkLib.Constants.LOG_INFO, true);
        }

        private void generateSerialNumber()
        {
            //Generating serial numbers (SL)
            serialNumberList = new List<BigInteger>();
            serialNumberList = SerialNumberGenerator.generateListOfSerialNumber(this.numberOfVoters, NetworkLib.Constants.NUMBER_OF_BITS_SL);

            Utils.Logs.addLog("EA", NetworkLib.Constants.SERIAL_NUMBER_GEN_SUCCESSFULLY, true, NetworkLib.Constants.LOG_INFO, true);
        }

        private void generateTokens()
        {
            //preparing Big Integers for RSA blind signature (token have to fulfil requirments) 
            this.tokensList = new List<List<BigInteger>>();
            this.exponentsList = new List<List<BigInteger>>();
            this.signatureFactor = new List<List<BigInteger>>();


            for (int i = 0; i < this.numberOfVoters; i++)
            { // we use the same method like to generate serial number, there is another random generator used inside this method
                List<AsymmetricCipherKeyPair> preToken = new List<AsymmetricCipherKeyPair>(SerialNumberGenerator.generatePreTokens(4, NetworkLib.Constants.NUMBER_OF_BITS_TOKEN));
                List<BigInteger> tokens = new List<BigInteger>();
                List<BigInteger> exps = new List<BigInteger>();
                List<BigInteger> signFactor = new List<BigInteger>();

                foreach (AsymmetricCipherKeyPair token in preToken)
                {
                    RsaKeyParameters publicKey = (RsaKeyParameters)token.Public;
                    RsaKeyParameters privKey = (RsaKeyParameters)token.Private;
                    tokens.Add(publicKey.Modulus);
                    exps.Add(publicKey.Exponent);
                    signFactor.Add(privKey.Exponent);
                }
                this.tokensList.Add(tokens);
                this.exponentsList.Add(exps);
                this.signatureFactor.Add(signFactor);
            }


            Utils.Logs.addLog("EA", NetworkLib.Constants.TOKENS_GENERATED_SUCCESSFULLY, true, NetworkLib.Constants.LOG_INFO, true);
            connectSerialNumberAndTokens();

        }

        private void connectSerialNumberAndPermutation()
        {
            dictionarySLPermuation = new Dictionary<BigInteger, List<BigInteger>>();
            for (int i = 0; i < this.serialNumberList.Count; i++)
            {
                dictionarySLPermuation.Add(this.serialNumberList[i], this.permutationsList[i]);
            }
            Utils.Logs.addLog("EA", NetworkLib.Constants.SL_CONNECTED_WITH_PERMUTATION, true, NetworkLib.Constants.LOG_INFO);
        }

        private void connectSerialNumberAndTokens()
        {
            this.dictionarySLTokens = new Dictionary<BigInteger, List<List<BigInteger>>>();
            for (int i = 0; i < this.serialNumberList.Count; i++)
            {
                List<List<BigInteger>> tokens = new List<List<BigInteger>>();
                tokens.Add(tokensList[i]);
                tokens.Add(exponentsList[i]);
                tokens.Add(signatureFactor[i]);
                this.dictionarySLTokens.Add(this.serialNumberList[i], tokens);
            }

            Utils.Logs.addLog("EA", NetworkLib.Constants.SL_CONNECTED_WITH_TOKENS, true, NetworkLib.Constants.LOG_INFO);
        }

        public void generateDate()
        {
            generateSerialNumber();
            generateTokens();
            generatePermutation();

        }
        public void sendSLAndTokensToProxy()
        {
            //before sending we have to convert dictionary to string. We use our own conversion to recoginize message in proxy and reparse it to dictionary

            string msg = NetworkLib.Constants.SL_TOKENS + "&";
            for (int i = 0; i < this.serialNumberList.Count; i++)
            {
                msg = msg + this.serialNumberList[i].ToString() + "=";
                for (int j = 0; j < this.tokensList[i].Count - 1; j++)
                {
                    msg = msg + this.tokensList[i][j].ToString() + ",";
                }
                msg = msg + this.tokensList[i][this.tokensList[i].Count-1].ToString() + ":";


                for (int j = 0; j < this.exponentsList[i].Count - 1; j++)
                {
                    msg = msg + this.exponentsList[i][j].ToString() + ",";
                }
                msg += this.exponentsList[i][this.exponentsList[i].Count - 1].ToString();
            }
            msg += ";";

            this.serverClient.sendMessage(NetworkLib.Constants.PROXY, msg);
        }

        public void getCandidateListPermuated(string name, BigInteger SL)
        {
            string candidateListString = this.candidateList.getCandidateListPermuated(name, this.dictionarySLPermuation[SL]);
            this.serverClient.sendMessage(name, candidateListString);
        }

        public void saveBlindBallotMatrix(string message)
        {
            //saving data recived from Proxy

            string[] words = message.Split(';');

            //1st parameter = name of voter
            string name = words[0];

            //2nd = SL of VOTER
            BigInteger SL = new BigInteger(words[1]);

            //Then tokens
            List<BigInteger> tokenList = new List<BigInteger>();
            string[] strTokens = words[2].Split(',');
            for (int i = 0; i < strTokens.Length; i++)
            {
                tokenList.Add(new BigInteger(strTokens[i]));
            }

            //Exponent list (used for blind signature)
            List<BigInteger> exponentList = new List<BigInteger>();
            string[] strExpo = words[3].Split(',');
            for (int i = 0; i < strExpo.Length; i++)
            {
                exponentList.Add(new BigInteger(strExpo[i]));
            }

            //and at least voted colums
            BigInteger[] columns = new BigInteger[4];
            string[] strColumns = words[4].Split(',');
            for (int i = 0; i < columns.Length; i++)
            {
                columns[i] = new BigInteger(strColumns[i]);
            }

            this.ballots.Add(name, new Ballot(SL));

            this.ballots[name].BlindColumn = columns;
            this.ballots[name].Permutation = this.dictionarySLPermuation[SL];
            this.ballots[name].InversePermutation = this.dictionarySLInversePermutation[SL];
            this.ballots[name].TokenList = tokenList;
            this.ballots[name].ExponentsList = exponentList;
            this.ballots[name].SignatureFactor = this.dictionarySLTokens[SL][2];

            Utils.Logs.addLog("EA", NetworkLib.Constants.BLIND_PROXY_BALLOT_RECEIVED + name, true, NetworkLib.Constants.LOG_INFO, true);

            this.signColumn(name);
        }

        private void signColumn(string name)
        {
            this.ballots[name].signColumn();
            string signColumns = null;

            for (int i = 0; i < this.ballots[name].SignedColumn.Length - 1; i++)
            {
                signColumns = signColumns + this.ballots[name].SignedColumn[i].ToString() + ",";
            }

            signColumns += this.ballots[name].SignedColumn[this.ballots[name].SignedColumn.Length - 1].ToString();

            string msg = NetworkLib.Constants.SIGNED_PROXY_BALLOT + "&" + name + ";" + signColumns;
            this.serverClient.sendMessage(NetworkLib.Constants.PROXY, msg);
            Utils.Logs.addLog("EA", NetworkLib.Constants.SIGNED_BALLOT_MATRIX_SENT, true, NetworkLib.Constants.LOG_INFO, true);
        }


        public void saveUnblindedBallotMatrix(string message)
        {
            //the same as previous saving
            string[] words = message.Split(';');

            string name = words[0];
            string[] strUnblinedColumns = words[1].Split(',');

            string[,] unblindedBallot = new string[this.candidateList.getNumberOfCandidates(), NetworkLib.Constants.BALLOT_SIZE];
            for (int i = 0; i < strUnblinedColumns.Length; i++)
            {
                for (int j = 0; j < strUnblinedColumns[i].Length; j++)
                {
                    unblindedBallot[j, i] = strUnblinedColumns[i][j].ToString();
                }
            }

            string[,] unblindedUnpermuatedBallot = new string[this.candidateList.getNumberOfCandidates(), NetworkLib.Constants.BALLOT_SIZE];
            BigInteger[] inversePermutation = this.ballots[name].InversePermutation.ToArray();

            for (int i = 0; i < unblindedUnpermuatedBallot.GetLength(0); i++)
            {
                string strRow = inversePermutation[i].ToString();
                int row = Convert.ToInt32(strRow) - 1;
                for (int j = 0; j < unblindedUnpermuatedBallot.GetLength(1); j++)
                {
                    unblindedUnpermuatedBallot[i, j] = unblindedBallot[row, j];
                }
            }

            this.ballots[name].UnblindedBallot = unblindedUnpermuatedBallot;
            Utils.Logs.addLog("EA", NetworkLib.Constants.UNBLINED_BALLOT_MATRIX_RECEIVED, true, NetworkLib.Constants.LOG_INFO, true);
        }


        public void disbaleProxy()
        {
            Utils.Logs.addLog("EA",NetworkLib.Constants.VOTIGN_STOPPED, true, NetworkLib.Constants.LOG_INFO, true);
        }

        public void countVotes()
        {
            /*ublindPermutation - EA send to voter unblinded permutation (and then private key) so Audiotr
                can check RSA formula*/
            this.auditor.unblindPermutation(permutationsList, this.pubKey, this.privKey);

            for (int i = 0; i < this.ballots.Count; i++)
            {
                int signleVote = checkVote(i);
                if (signleVote != -1)
                {
                    this.finalResults[signleVote] += 1;
                }
            }

            this.announceResultsOfElection();
        }

        private string announceResultsOfElection()
        {

            int maxValue = this.finalResults.Max();
            int maxIndex = this.finalResults.ToList().IndexOf(maxValue);
            int winningCandidates = 0;
            string winners = null;
            string resultOfVoting = null;
            for (int i = 0; i < this.finalResults.Count; i++)
            {
                resultOfVoting = resultOfVoting + this.candidateList.getCandidateByIndex(i) + " received: " + this.finalResults[i] + " votes" +Environment.NewLine;
                if (this.finalResults[i] == maxValue)
                {
                    winningCandidates += 1; // a few candidates has the same number of votes.
                    winners = winners + this.candidateList.getCandidateByIndex(i) + " ";
                }
            }

            return winners;
        }

        private int checkVote(int voterNumber)
        {
            Ballot ballot = this.ballots.ElementAt(voterNumber).Value;
            string[,] vote = ballot.UnblindedBallot;
            Console.WriteLine("Voter number " + voterNumber);
            int voteCastOn = -1;
            for (int i = 0; i < vote.GetLength(0); i++)
            {
                int numberOfYes = 0;
                for (int j = 0; j < vote.GetLength(1); j++)
                {
                    if (vote[i, j] == "1")
                    {
                        numberOfYes += 1;
                    }
                }


                if (numberOfYes == 3)
                {
                    voteCastOn = i;
                    break;
                }
            }

            return voteCastOn;
        }
    }
}


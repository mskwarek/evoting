using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Newtonsoft.Json;

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

        private List<BigInteger> serialNumberList;

        private Dictionary<string, Ballot> ballots;

        private List<int> finalResults;

        private Auditor auditor;                                            

        private int getNumberOfVoters()
        {
            return Convert.ToInt32(this.configuration.NumberOfVoters);
        }

        public ElectionAuthority(string filename)
        {
            Configuration configuration = new Configuration();
            configuration.loadConfiguration(filename);
            this.encoder = new ASCIIEncoding();
            this.configuration = configuration;
            //server for Clients
            this.serverClient = new Server(this);

            this.ballots = new Dictionary<string, Ballot>();
            this.serialNumberList = new List<BigInteger>();
            this.auditor = new Auditor();
            this.loadCandidateList("C:\\Users\\mskwarek\\Documents\\Visual Studio 2015\\Projects\\PKRY\\Config\\CandidateList.xml");
            finalResults = new List<int>(this.candidateList.getNumberOfCandidates());
            this.generateDate(); //method generate Serial number (SL), permutations of candidate list and tokens

            this.sendSLAndTokensToProxyJson();
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
            connectSerialNumberAndPermutation();
            generatePermutationTokens();
            this.auditor.blindPermutation(this.getPermutationsList());              //Send commited permutation to Auditor
            Utils.Logs.addLog("EA", NetworkLib.Constants.PERMUTATION_GEN_SUCCESSFULLY, true, NetworkLib.Constants.LOG_INFO);
        }

        private List<Permutation> getPermutationsList()
        {
            List<Permutation> permutations = new List<Permutation>();
            //generating permutation and feeling List
            for (int i = 0; i < this.getNumberOfVoters(); i++)
            {
                permutations.Add(this.ballots.Values.ToList().ElementAt(i).Permutation);
            }
            return permutations;
        }

        private void generatePermutationTokens()
        {
            Utils.Logs.addLog("EA", "Permutation tokens generated", true, NetworkLib.Constants.LOG_INFO, true);
        }

        private void generateSerialNumber()
        {
            //Generating serial numbers (SL)
            serialNumberList = new List<BigInteger>();
            serialNumberList = SerialNumberGenerator.generateListOfSerialNumber(this.getNumberOfVoters(), NetworkLib.Constants.NUMBER_OF_BITS_SL);

            Utils.Logs.addLog("EA", NetworkLib.Constants.SERIAL_NUMBER_GEN_SUCCESSFULLY, true, NetworkLib.Constants.LOG_INFO, true);
        }

        private void generateTokens()
        {
            //preparing Big Integers for RSA blind signature (token have to fulfil requirments) 
            //this.tokensList = new List<List<BigInteger>>();
            //this.exponentsList = new List<List<BigInteger>>();
            //this.signatureFactor = new List<List<BigInteger>>();


            for (int i = 0; i < this.getNumberOfVoters(); i++)
            {
                // we use the same method like to generate serial number, 
                // there is another random generator used inside this method
                // List<AsymmetricCipherKeyPair> preToken = new List<AsymmetricCipherKeyPair>(SerialNumberGenerator.generatePreTokens(Convert.ToInt32(configuration.NumberOfVoters), NetworkLib.Constants.NUMBER_OF_BITS_TOKEN));
                
                // this.tokensList.Add(tokens);
                // this.exponentsList.Add(exps);
                // this.signatureFactor.Add(signFactor);
            }


            Utils.Logs.addLog("EA", NetworkLib.Constants.TOKENS_GENERATED_SUCCESSFULLY, true, NetworkLib.Constants.LOG_INFO, true);
            connectSerialNumberAndTokens();

        }

        private void connectSerialNumberAndPermutation()
        {
            Utils.Logs.addLog("EA", NetworkLib.Constants.SL_CONNECTED_WITH_PERMUTATION, true, NetworkLib.Constants.LOG_INFO);
        }

        private void connectSerialNumberAndTokens()
        {
            //this.dictionarySLTokens = new Dictionary<BigInteger, List<List<BigInteger>>>();
            //for (int i = 0; i < this.serialNumberList.Count; i++)
            //{
            //    List<List<BigInteger>> tokens = new List<List<BigInteger>>();
            //    tokens.Add(tokensList[i]);
            //    tokens.Add(exponentsList[i]);
            //    tokens.Add(signatureFactor[i]);
            //    this.dictionarySLTokens.Add(this.serialNumberList[i], tokens);
            //}

            Utils.Logs.addLog("EA", NetworkLib.Constants.SL_CONNECTED_WITH_TOKENS, true, NetworkLib.Constants.LOG_INFO);
        }

        public void generateDate()
        {
            generateSerialNumber();
            for(int i = 0; i < this.candidateList.getNumberOfCandidates(); ++i)
            {
                this.ballots.Add(i.ToString(), new Ballot(this.serialNumberList.ElementAt(i)));
            }
            generateTokens();
            generatePermutation();

        }

        public void sendSLAndTokensToProxyJson()
        {
            Ballot test = this.ballots.Values.First();
            NetworkLib.MessageSLTokens msg = new NetworkLib.MessageSLTokens(test.SL, test.TokenList, test.ExponentsList);
            string data = JsonConvert.SerializeObject(msg);
            NetworkLib.JsonMessage request = new NetworkLib.JsonMessage(NetworkLib.Constants.SL_TOKENS, data);
            string json = JsonConvert.SerializeObject(request);

            Utils.Logs.addLog("EA request json", json, true, NetworkLib.Constants.LOG_INFO, true);
            this.serverClient.sendMessage(NetworkLib.Constants.PROXY, json);
        }

        public void getCandidateListPermuated(string name, BigInteger SL)
        {
            Ballot specBallot = this.ballots.Values.ToList().Find(x => x.SL.Equals(SL));
            string candidateListString = this.candidateList.getCandidateListPermuated(name, specBallot.Permutation.getPermutation());
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

            
            this.ballots[name].BlindColumn = columns;
            this.ballots[name].TokenList = tokenList;
            this.ballots[name].ExponentsList = exponentList;
            //this.ballots[name].SignatureFactor = this.dictionarySLTokens[SL][2];

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
            BigInteger[] inversePermutation = this.ballots[name].Permutation.getInversePermutation().ToArray();

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
            this.auditor.unblindPermutation(this.getPermutationsList());

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


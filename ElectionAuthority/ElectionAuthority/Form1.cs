using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace ElectionAuthority
{
    public partial class Form1 : Form
    {
        private ElectionAuthority electionAuthority;
        
        public Form1()
        {
            InitializeComponent();
            this.loadConfig("C:\\Users\\mskwarek\\Documents\\Visual Studio 2015\\Projects\\evoting\\Config\\ElectionAuthority.xml");
            this.startEaService();
        }

        private void startElectionAuthorityButton_Click(object sender, EventArgs e)
        {
            this.startElectionAuthorityButton.Enabled = false;
            this.sendSLTokensAndTokensButton.Enabled = true;
            startEaService();
        }

        public void startEaService()
        {
            this.electionAuthority.startServices();
        }

        private void configButton_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            loadConfig(openFileDialog.FileName);
            enableButtonAfterConfiguration();
        }

        public void loadConfig(string filename)
        {
            electionAuthority = new ElectionAuthority(filename);
        }
        private void enableButtonAfterConfiguration()
        {
            this.startElectionAuthorityButton.Enabled = true;
            this.configButton.Enabled = false;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.electionAuthority != null)
            {
                if (this.electionAuthority.ServerClient != null)
                {
                    this.electionAuthority.ServerClient.stopServer();
                }
            }           
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        public void disableSendSLTokensAndTokensButton()
        {
            this.sendSLTokensAndTokensButton.Enabled = false;
            this.finishVotingButton.Enabled = true;
        }

        private void finishVotingButton_Click(object sender, EventArgs e)
        {
            this.electionAuthority.disbaleProxy();
            this.finishVotingButton.Enabled = false;
            this.countVotesButton.Enabled = true;
        }

        private void countVotesButton_Click(object sender, EventArgs e)
        {
            this.electionAuthority.countVotes();
            this.countVotesButton.Enabled = false;
        }

    }
}

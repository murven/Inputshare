using InputshareLib.NamedIPC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Diagnostics;

namespace Inputshare
{
    public partial class ClientForm : Form
    {
        private NamedIpcClient ipcClient;
        private MainForm mainFormRef;

        private bool switchingToServer = false;

        public ClientForm(MainForm form)
        {
            mainFormRef = form;
            this.FormBorderStyle = FormBorderStyle.Fixed3D;
            InitializeComponent();
            this.FormClosed += ClientForm_FormClosed;
            this.Resize += ClientForm_Resize;
        }

        private void ClientForm_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                this.Hide();
            }
        }

        private void SetIpcStatusLabelText(string text)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>{ SetIpcStatusLabelText(text); }));
                return;
            }

            IpcConnectingStatusLabel.Text = text;
        }

        private void ClientForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(!switchingToServer)
                mainFormRef.Invoke(new Action(() => { mainFormRef.Close(); }));
        }

        private void ClientForm_Load(object sender, EventArgs e)
        {
            this.MaximizeBox = false;
            SetIpcStatusLabelText("Checking if service is\nrunning...");
            if (!IsProcessRunning())
            {
                MessageBox.Show("Error: Inputshareservice is not running. exiting");
                Exit();
                return;
            }

            SetIpcStatusLabelText("Connecting to IPC...");
            DisconnectedPanel.Hide();
            IpcDisconnectedPanel.Show();
            IpcDisconnectedPanel.BringToFront();
            ConnectedPanel.Visible = false;

            Task.Run(new Action(() => { IpcConnect(); }));
            ClientNameTextBox.Text = Environment.MachineName;
        }

        private bool IsProcessRunning()
        {
            Process[] procs = Process.GetProcessesByName("inputshareservice");
            if(procs.Length == 0)
            {
                return false;
            }
            return true;
        }

        private void IpcConnect()
        {
            try
            {
                SetIpcStatusLabelText("Connecting to IPC...");
                ipcClient = new NamedIpcClient();
                ipcClient.ServiceStateReceived += IpcClient_ServiceStateReceived;
                ipcClient.MessageReceived += IpcClient_MessageReceived;
                ipcClient.Connected += IpcClient_Connected;
                ipcClient.Disconnected += IpcClient_Disconnected;
                ipcClient.Connect(3000);
                this.Invoke(new Action(() => { IpcDisconnectedPanel.Hide(); }));
                ipcClient.SendObject(NIpcBasicMessage.GetState);
            }catch(Exception ex)
            {
                SetIpcStatusLabelText("Failed to connect to IPC");
                MessageBox.Show("Could not communicate with service: " + ex.Message, "Service error. Make sure that the inputshareservice is running!", MessageBoxButtons.OK);
                ipcClient.Dispose();
                Exit();
                return;
            }
        }

        private void IpcClient_Disconnected(object sender, EventArgs e)
        {
            if (!switchingToServer)
            {
                MessageBox.Show("IPC connection error");
                Exit();
            }
           
        }

        private void IpcClient_Connected(object s, EventArgs e)
        {
            IpcDisconnectedPanel.Hide();
        }

        private void Exit()
        {
            ipcClient?.Dispose();
            mainFormRef.Invoke(new Action(() => { mainFormRef.SwitchToServerMode(); mainFormRef.Show(); }));
            this.Invoke(new Action(() => { this.Dispose(); }));
           
        }

        private void IpcClient_MessageReceived(object sender, NIpcBasicMessage message)
        {
            if (message == NIpcBasicMessage.Connected)
            {
                OnConnect();
                Task.Run(() => { ipcClient.SendObject(NIpcBasicMessage.GetState); });
            }
            else if (message == NIpcBasicMessage.ConnectionError || message == NIpcBasicMessage.ConnectionFailed
               || message == NIpcBasicMessage.Disconnected)
            {
                OnDisconnect();
            }else if(message == NIpcBasicMessage.AttemptingConnection)
            {
                OnAttemptingConnection();
            }
            else
            {
                MessageBox.Show("Service: " + message);
            }
        }
        private void OnConnect()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => { OnConnect(); }));
                return;
            }

            
            IpcDisconnectedPanel.Hide();
            ConnectedPanel.Show();
            DisconnectedPanel.Hide();
        }

        private void OnDisconnect()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => { OnDisconnect(); }));
                return;
            }
            ConnectButton.Text = "Connect";
            ConnectButton.Enabled = true;
            IpcDisconnectedPanel.Hide();
            DisconnectedPanel.Show();
            ConnectedPanel.Hide();
        }

        private void OnAttemptingConnection()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => { OnAttemptingConnection(); }));
                return;
            }

            ConnectButton.Text = "Connecting...";
            ConnectButton.Enabled = false;
        }


        private void IpcClient_ServiceStateReceived(object sender, NIpcServiceStateMessage state)
        {

            if (state.Connected)
            {
                OnConnect();

                Action set = new Action(() => {
                    ConnectedPanelAddressLabel.Text = "Connected to " + state.Address +
                    "\n as " + state.UserName;
                });
                if (this.InvokeRequired)
                {
                    this.Invoke(set);
                }
                set();
            }
            else
            {
                OnDisconnect();
            }
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            try
            {
                NIpcConnectMessage.AddressMode mode = NIpcConnectMessage.AddressMode.IP;
                IPAddress.TryParse(AddressTextBox.Text, out IPAddress addr);

                if(addr == null)
                {
                    mode = NIpcConnectMessage.AddressMode.HostName;
                }

                if(ClientNameTextBox.Text.Length > 32 || ClientNameTextBox.Text == "")
                {
                    MessageBox.Show("Invalid client name");
                    return;
                }

                int.TryParse(AddressPortTextBox.Text, out int port);

                if(port == 0 || port > 65535)
                {
                    MessageBox.Show("Invalid port");
                    return;
                }

                NIpcConnectMessage msg = new NIpcConnectMessage(AddressTextBox.Text, port, mode,
                    ClientNameTextBox.Text, Guid.Empty);

                Task.Run(() => { ipcClient.SendObject(msg); });
                

            }catch(Exception ex)
            {
                MessageBox.Show("Could not send connect message to service: " + ex.Message);
            }
        }

        private void DisconnectButton_Click(object sender, EventArgs e)
        {
            try
            {
                Task.Run(() => { ipcClient.SendObject(NIpcBasicMessage.Disconnect); });
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error sending message to service: " + ex.Message);
            }
            
        }

        private void SwitchToServerButton_Click(object sender, EventArgs e)
        {
            switchingToServer = true;
            Exit();
            return;
        }
    }
}

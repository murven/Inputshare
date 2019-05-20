using InputshareLib;
using InputshareLib.Input;
using InputshareLib.Input.Hotkeys;
using InputshareLib.Server;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Inputshare
{
    public partial class MainForm : Form
    {
        [DllImport("user32.dll")]
        static extern uint MapVirtualKeyA(uint code, uint mapType = 0);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hwnd);

        private ISServer server;
        private ConnectedClientInfo selectedClient;
        private Hotkey lastEnteredHotkey;
        private bool hotkeyEntering;
        private ClientForm cForm;

        private NotifyIcon trayIcon;

        private bool clientMode = false;

        public MainForm()
        {
            this.FormClosing += MainForm_FormClosing;
            this.FormClosed += MainForm_FormClosed;
            this.Resize += MainForm_Resize;
            InitializeComponent();
            server = new ISServer();
            cForm = new ClientForm(this);
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if(FormWindowState.Minimized == this.WindowState)
            {
                trayIcon.Visible = true;
                this.Hide();
            }
        }

        public void SwitchToServerMode()
        {
            clientMode = false;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ISLogger.MessageOut -= ISLogger_MessageOut;
            if (server == null || server.Running)
            {
                server.Stop();
            }
            trayIcon.Visible = false;
            ISLogger.Exit();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;
            ConsoleTextBox.BackColor = SystemColors.ControlLightLight;
            this.MaximizeBox = false;
            ISLogger.MessageOut += ISLogger_MessageOut;
            server.ClientConnected += Server_ClientConnected;
            server.ClientDisconnected += Server_ClientDisconnected;
            server.InputClientSwitched += Server_InputClientSwitched;
            server.ServerStopped += Server_ServerStopped;
            server.DisplayConfigChanged += Server_DisplayConfigChanged;
            ClientListBox.MouseClick += ClientListBox_MouseClick;
            LeftClientListBox.SelectionChangeCommitted += LeftClientListBox_SelectionChangeCommitted;
            RightClientListBox.SelectionChangeCommitted += RightClientListBox_SelectionChangeCommitted;
            AboveClientListBox.SelectionChangeCommitted += AboveClientListBox_SelectionChangeCommitted;
            BelowClientListBox.SelectionChangeCommitted += BelowClientListBox_SelectionChangeCommitted;

            ClientListBox.Hide();
            ClientSettingsPanel.Hide();

            trayIcon = new NotifyIcon();

            try
            {
                trayIcon.Icon = new Icon("TrayIcon.ico");
            }catch(Exception ex)
            {
                ISLogger.Write($"Failed to open TrayIcon.ico");
                ISLogger.Write(ex.Message); 
            }

            trayIcon.Visible = true;
            trayIcon.Click += TrayIcon_Click;
        }

        private void TrayIcon_Click(object sender, EventArgs e)
        {
            if (clientMode)
            {
                cForm.Invoke(new Action(() => { cForm?.Show();
                    cForm.BringToFront();
                    SetForegroundWindow(cForm.Handle);
                }));
            }
            else
            {
                this.Show();
                this.BringToFront();
                SetForegroundWindow(this.Handle);
            }
        }

        private void Server_DisplayConfigChanged(object sender, EventArgs e)
        {
            UpdateClientList();
            RedrawClientSettings();
        }

        private bool CheckAdmin()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        private void BelowClientListBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            ConnectedClientInfo clientA = BelowClientListBox.SelectedItem as ConnectedClientInfo;
            if (clientA == ConnectedClientInfo.None)
                server.SetClientEdge(null, BoundEdge.Bottom, selectedClient);
            else
                server.SetClientEdge(clientA, BoundEdge.Bottom, selectedClient);
            UpdateClientList();
            RedrawClientSettings();
        }

        private void AboveClientListBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            ConnectedClientInfo clientA = AboveClientListBox.SelectedItem as ConnectedClientInfo;
            if (clientA == ConnectedClientInfo.None)
                server.SetClientEdge(null, BoundEdge.Top, selectedClient);
            else
                server.SetClientEdge(clientA, BoundEdge.Top, selectedClient);
            UpdateClientList();
            RedrawClientSettings();
        }

        private void RightClientListBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            ConnectedClientInfo clientA = RightClientListBox.SelectedItem as ConnectedClientInfo;
            if (clientA == ConnectedClientInfo.None)
                server.SetClientEdge(null, BoundEdge.Right, selectedClient);
            else
                server.SetClientEdge(clientA, BoundEdge.Right, selectedClient);

            UpdateClientList();
            RedrawClientSettings();
        }

        private void LeftClientListBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            ConnectedClientInfo clientA = LeftClientListBox.SelectedItem as ConnectedClientInfo;
            if (clientA == ConnectedClientInfo.None)
                server.SetClientEdge(null, BoundEdge.Left, selectedClient);
            else
                server.SetClientEdge(clientA, BoundEdge.Left, selectedClient);
            UpdateClientList();
            RedrawClientSettings();
        }

        private void Server_ServerStopped(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => { Server_ServerStopped(sender, e); }));
                return;
            }

            ServerPortTextBox.Visible = true;
            ServerStartButton.Text = "Start server";
            ClientSettingsPanel.Hide();
            UpdateClientList();
            RedrawClientSettings();
        }

        private void Server_InputClientSwitched(object sender, EventArgs e)
        {
            ISLogger.Write("Current client: " + server.GetInputClient().ClientName);
        }

        private void ClientListBox_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            int iIndex = ClientListBox.IndexFromPoint(e.Location);
            if (iIndex != ListBox.NoMatches)
            {
                ConnectedClientInfo info = ClientListBox.Items[iIndex] as ConnectedClientInfo;
                selectedClient = info;
                ClientSettingsPanel.Show();
                //UpdateClientList();
                RedrawClientSettings();
            }
        }


        private ConnectedClientInfo GetlocalhostInfo()
        {
            List<ConnectedClientInfo> clients = new List<ConnectedClientInfo>(server.GetClients());
            foreach (var client in clients)
            {
                if (client.ClientId == Guid.Empty)
                {
                    return client;
                }
            }
            return null;
        }

        private void Server_ClientDisconnected(object sender, ISServer.ClientDisconnectedArgs e)
        {
            UpdateClientList();
            RedrawClientSettings();
        }

        private void Server_ClientConnected(object sender, ConnectedClientInfo e)
        {
            UpdateClientList();
            RedrawClientSettings();
        }

        private void UpdateClientList()
        {
            if (ClientListBox.InvokeRequired)
            {
                ClientListBox.Invoke(new Action(() => { UpdateClientList(); }));
                return;
            }

            ClientListBox.Items.Clear();

            if (server == null || !server.Running)
                return;

            foreach (ConnectedClientInfo client in server.GetClients())
            {
                ClientListBox.Items.Add(client);
            }

            if (selectedClient == null)
                ClientSettingsPanel.Hide();
        }

        private void ISLogger_MessageOut(object sender, string text)
        {
            if (ConsoleTextBox.InvokeRequired)
            {
                ConsoleTextBox.Invoke(new Action(() => ISLogger_MessageOut(null, text)));
            }
            else
            {
                ConsoleTextBox.AppendText(text);
            }
        }

        private void ServerStartButton_Click(object sender, EventArgs e)
        {
            if (server.Running)
            {
                Task.Run(() => StopServer());
                ServerPortTextBox.Visible = true;
                Task.Run(new Action(() => {
                    this.Invoke(new Action(() => { ServerStartButton.Enabled = false; }));
                    Thread.Sleep(1000);
                    this.Invoke(new Action(() => { ServerStartButton.Enabled = true; }));
                }));

            }
            else
            {
                int.TryParse(ServerPortTextBox.Text, out int port);
                if(port == 0 || port > 65535)
                {
                    MessageBox.Show("Invalid port");
                    return;
                }
                
                Task.Run(new Action(() => {
                    try
                    {
                        StartServer(port);
                    }catch(Exception ex)
                    {
                        MessageBox.Show("Failed to start server: " + ex.Message);
                        return;
                    }
                   
                    this.Invoke(new Action(() => { ServerPortTextBox.Visible = false; ServerStartButton.Enabled = false; }));
                    Thread.Sleep(1000);
                    this.Invoke(new Action(() => { ServerStartButton.Enabled = true; }));
                }));
            }
        }

        private void StartServer(int port)
        {
            server.Start(port);
            ServerStartButton.Invoke(new Action(() => { ServerStartButton.Text = "Stop server";
                ClientSettingsPanel.Show();
                selectedClient = GetlocalhostInfo();
                ClientListBox.Show();
                UpdateClientList();
                RedrawClientSettings();
                ReadServerSettings();
            }));
            
        }

        private void StopServer()
        {
            server.Stop();
            ServerStartButton.Invoke(new Action(() => { ServerStartButton.Text = "Start server";
                ClientListBox.Hide();
                for(int i = 0; i < SettingsCheckedListBox.Items.Count; i++)
                {
                    SettingsCheckedListBox.SetItemChecked(i, false);
                }
                
                UpdateClientList();
                RedrawClientSettings();
            }));
            
            
        }

        private void ConsoleTextBox_TextChanged(object sender, EventArgs e)
        {
            ConsoleTextBox.SelectionStart = ConsoleTextBox.Text.Length;
            ConsoleTextBox.ScrollToCaret();
        }

        private void ClientHotkeyButton_Click(object sender, EventArgs e)
        {
            if (hotkeyEntering)
            {
                ClientHotkeyButton.BackColor = default(Color);
                hotkeyEntering = false;


                if (server == null || !server.Running || lastEnteredHotkey == null || selectedClient == null)
                {
                    RedrawClientSettings();
                    UpdateClientList();
                    return;
                }

                server.SetClientHotkey(selectedClient, lastEnteredHotkey);

                Hotkey key = server.GetclientHotkey(selectedClient);
                ClientHotkeyButton.Text = key.ToString();
                UpdateClientList();

                foreach (var info in ClientListBox.Items)
                {
                    ConnectedClientInfo client = info as ConnectedClientInfo;
                    selectedClient = client;
                }

                RedrawClientSettings();
            }
            else
            {
                hotkeyEntering = true;
                ClientHotkeyButton.BackColor = Color.Gray;
            }
        }

        private void RedrawClientSettings()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => { RedrawClientSettings(); }));
                return;
            }

            if (selectedClient == null)
            {
                selectedClient = GetlocalhostInfo();

                if (selectedClient == null)
                    return;
            }

            ClientSettingsClientNameLabel.Text = selectedClient.ClientName;
            if (selectedClient.Key == null)
                ClientHotkeyButton.Text = "None";
            else
                ClientHotkeyButton.Text = selectedClient.Key.ToString();

            FillComboBoxWithClients(LeftClientListBox, selectedClient, selectedClient.LeftClient == ConnectedClientInfo.None ? ConnectedClientInfo.None : selectedClient.LeftClient);
            FillComboBoxWithClients(RightClientListBox, selectedClient, selectedClient.RightClient == ConnectedClientInfo.None ? ConnectedClientInfo.None : selectedClient.RightClient);
            FillComboBoxWithClients(BelowClientListBox, selectedClient, selectedClient.BelowClient == ConnectedClientInfo.None ? ConnectedClientInfo.None : selectedClient.BelowClient);
            FillComboBoxWithClients(AboveClientListBox, selectedClient, selectedClient.AboveClient == ConnectedClientInfo.None ? ConnectedClientInfo.None : selectedClient.AboveClient);
        }

        private void FillComboBoxWithClients(ComboBox list, ConnectedClientInfo ignoreClient = null, ConnectedClientInfo currentSetting = null)
        {
            if (list.InvokeRequired)
            {
                list.Invoke(new Action(() => { FillComboBoxWithClients(list, ignoreClient, currentSetting); }));
                return;
            }

            ConnectedClientInfo[] clients = server.GetClients();

            list.Items.Clear();
            list.SelectedItem = null;
            int index = 0;
            list.Items.Add(ConnectedClientInfo.None);
            list.SelectedIndex = 0;
            foreach (var client in clients)
            {
                if (ignoreClient == null)
                {
                    list.Items.Add(client);

                    if (currentSetting != ConnectedClientInfo.None && currentSetting.ClientName == client.ClientName)
                    {
                        list.SelectedItem = client;
                    }
                }
                else if (client.ClientId != ignoreClient.ClientId)
                {
                    list.Items.Add(client);
                    if (currentSetting != ConnectedClientInfo.None && currentSetting.ClientName == client.ClientName)
                    {
                        list.SelectedItem = client;
                    }

                    index++;
                }
            }
        }


        private void MainForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs args)
        {
            if (hotkeyEntering)
            {
                uint key = (uint)args.KeyCode;

                ScanCode code = (ScanCode)MapVirtualKeyA(key);

                if(code == ScanCode.Control || code == ScanCode.RShift || code == ScanCode.LShift || 
                    code == ScanCode.Alt)
                {
                    code = ScanCode.None;
                }

                Hotkey.Modifiers hkMods = new Hotkey.Modifiers();

                if (args.Alt)
                    hkMods = hkMods |= Hotkey.Modifiers.Alt;
                if (args.Shift)
                    hkMods = hkMods |= Hotkey.Modifiers.Shift;
                if (args.Control)
                    hkMods = hkMods |= Hotkey.Modifiers.Ctrl;

                Hotkey hk = new Hotkey(code, hkMods);
                lastEnteredHotkey = hk;
                ClientHotkeyButton.Text = hk.ToString();

            }
        }

        private void ClientSettingsHeader_Click(object sender, EventArgs e)
        {

        }

        private void SettingsCheckedListBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ApplyServerSettingsButton_Click(object sender, EventArgs e)
        {
            ISServer.ISServerSettings settings = new ISServer.ISServerSettings(serverSettingChecked(0),
                serverSettingChecked(1), serverSettingChecked(2));
            server.SetSettings(settings);
        }

        private void ReadServerSettings()
        {
            ISServer.ISServerSettings settings = server.GetSettings();
            SettingsCheckedListBox.SetItemChecked(0, settings.EnableCursorSwitch);
            SettingsCheckedListBox.SetItemChecked(1, settings.ReleaseAllOnSwitch);
            SettingsCheckedListBox.SetItemChecked(2, settings.DisableSwitchWhileFullscreen);
        }

        private bool serverSettingChecked(int index)
        {
            if (SettingsCheckedListBox.GetItemCheckState(index) == CheckState.Checked)
                return true;
            else
                return false;
        }

        private void ClientStartButton_Click(object sender, EventArgs e)
        {
            if(server != null && server.Running)
            {
                MessageBox.Show("Server must be stopped before switching to client mode");
                return;
            }

            if (!CheckAdmin())
            {
                MessageBox.Show("Cannot start client: not running as administrator");
                return;
            }

            clientMode = true;
            this.Hide();
            if (cForm != null)
                cForm.Dispose();

            cForm = new ClientForm(this);
            cForm.Show();
        }
    }
}

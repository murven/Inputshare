namespace Inputshare
{
    partial class ClientForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ConnectButton = new System.Windows.Forms.Button();
            this.AddressHeaderLabel = new System.Windows.Forms.Label();
            this.AddressTextBox = new System.Windows.Forms.TextBox();
            this.ClientNameHeaderLabel = new System.Windows.Forms.Label();
            this.ClientNameTextBox = new System.Windows.Forms.TextBox();
            this.AddressPortTextBox = new System.Windows.Forms.TextBox();
            this.DisconnectButton = new System.Windows.Forms.Button();
            this.DisconnectedPanel = new System.Windows.Forms.Panel();
            this.ConnectedPanel = new System.Windows.Forms.Panel();
            this.ConnectedPanelAddressLabel = new System.Windows.Forms.Label();
            this.IpcDisconnectedPanel = new System.Windows.Forms.Panel();
            this.IpcConnectingStatusLabel = new System.Windows.Forms.Label();
            this.SwitchToServerButton = new System.Windows.Forms.Button();
            this.DisconnectedPanel.SuspendLayout();
            this.ConnectedPanel.SuspendLayout();
            this.IpcDisconnectedPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // ConnectButton
            // 
            this.ConnectButton.Location = new System.Drawing.Point(29, 78);
            this.ConnectButton.Name = "ConnectButton";
            this.ConnectButton.Size = new System.Drawing.Size(171, 23);
            this.ConnectButton.TabIndex = 0;
            this.ConnectButton.Text = "Connect";
            this.ConnectButton.UseVisualStyleBackColor = true;
            this.ConnectButton.Click += new System.EventHandler(this.ConnectButton_Click);
            // 
            // AddressHeaderLabel
            // 
            this.AddressHeaderLabel.AutoSize = true;
            this.AddressHeaderLabel.Location = new System.Drawing.Point(90, -3);
            this.AddressHeaderLabel.Name = "AddressHeaderLabel";
            this.AddressHeaderLabel.Size = new System.Drawing.Size(48, 13);
            this.AddressHeaderLabel.TabIndex = 1;
            this.AddressHeaderLabel.Text = "Address:";
            // 
            // AddressTextBox
            // 
            this.AddressTextBox.Location = new System.Drawing.Point(38, 13);
            this.AddressTextBox.Name = "AddressTextBox";
            this.AddressTextBox.Size = new System.Drawing.Size(100, 20);
            this.AddressTextBox.TabIndex = 2;
            this.AddressTextBox.Text = "192.168.0.7";
            // 
            // ClientNameHeaderLabel
            // 
            this.ClientNameHeaderLabel.AutoSize = true;
            this.ClientNameHeaderLabel.Location = new System.Drawing.Point(73, 36);
            this.ClientNameHeaderLabel.Name = "ClientNameHeaderLabel";
            this.ClientNameHeaderLabel.Size = new System.Drawing.Size(65, 13);
            this.ClientNameHeaderLabel.TabIndex = 3;
            this.ClientNameHeaderLabel.Text = "Client name:";
            // 
            // ClientNameTextBox
            // 
            this.ClientNameTextBox.Location = new System.Drawing.Point(62, 52);
            this.ClientNameTextBox.Name = "ClientNameTextBox";
            this.ClientNameTextBox.Size = new System.Drawing.Size(100, 20);
            this.ClientNameTextBox.TabIndex = 4;
            // 
            // AddressPortTextBox
            // 
            this.AddressPortTextBox.Location = new System.Drawing.Point(144, 13);
            this.AddressPortTextBox.Name = "AddressPortTextBox";
            this.AddressPortTextBox.Size = new System.Drawing.Size(56, 20);
            this.AddressPortTextBox.TabIndex = 5;
            this.AddressPortTextBox.Text = "44101";
            // 
            // DisconnectButton
            // 
            this.DisconnectButton.Location = new System.Drawing.Point(3, 78);
            this.DisconnectButton.Name = "DisconnectButton";
            this.DisconnectButton.Size = new System.Drawing.Size(223, 23);
            this.DisconnectButton.TabIndex = 6;
            this.DisconnectButton.Text = "Disconnect";
            this.DisconnectButton.UseVisualStyleBackColor = true;
            this.DisconnectButton.Click += new System.EventHandler(this.DisconnectButton_Click);
            // 
            // DisconnectedPanel
            // 
            this.DisconnectedPanel.Controls.Add(this.ConnectButton);
            this.DisconnectedPanel.Controls.Add(this.AddressHeaderLabel);
            this.DisconnectedPanel.Controls.Add(this.AddressPortTextBox);
            this.DisconnectedPanel.Controls.Add(this.AddressTextBox);
            this.DisconnectedPanel.Controls.Add(this.ClientNameTextBox);
            this.DisconnectedPanel.Controls.Add(this.ClientNameHeaderLabel);
            this.DisconnectedPanel.Location = new System.Drawing.Point(12, 2);
            this.DisconnectedPanel.Name = "DisconnectedPanel";
            this.DisconnectedPanel.Size = new System.Drawing.Size(229, 111);
            this.DisconnectedPanel.TabIndex = 7;
            // 
            // ConnectedPanel
            // 
            this.ConnectedPanel.Controls.Add(this.ConnectedPanelAddressLabel);
            this.ConnectedPanel.Controls.Add(this.DisconnectButton);
            this.ConnectedPanel.Location = new System.Drawing.Point(12, 2);
            this.ConnectedPanel.Name = "ConnectedPanel";
            this.ConnectedPanel.Size = new System.Drawing.Size(229, 111);
            this.ConnectedPanel.TabIndex = 8;
            // 
            // ConnectedPanelAddressLabel
            // 
            this.ConnectedPanelAddressLabel.AutoSize = true;
            this.ConnectedPanelAddressLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ConnectedPanelAddressLabel.Location = new System.Drawing.Point(3, 7);
            this.ConnectedPanelAddressLabel.Name = "ConnectedPanelAddressLabel";
            this.ConnectedPanelAddressLabel.Size = new System.Drawing.Size(90, 16);
            this.ConnectedPanelAddressLabel.TabIndex = 7;
            this.ConnectedPanelAddressLabel.Text = "Connected to ";
            // 
            // IpcDisconnectedPanel
            // 
            this.IpcDisconnectedPanel.Controls.Add(this.IpcConnectingStatusLabel);
            this.IpcDisconnectedPanel.Location = new System.Drawing.Point(12, 2);
            this.IpcDisconnectedPanel.Name = "IpcDisconnectedPanel";
            this.IpcDisconnectedPanel.Size = new System.Drawing.Size(229, 111);
            this.IpcDisconnectedPanel.TabIndex = 9;
            // 
            // IpcConnectingStatusLabel
            // 
            this.IpcConnectingStatusLabel.AutoSize = true;
            this.IpcConnectingStatusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IpcConnectingStatusLabel.Location = new System.Drawing.Point(3, 36);
            this.IpcConnectingStatusLabel.Name = "IpcConnectingStatusLabel";
            this.IpcConnectingStatusLabel.Size = new System.Drawing.Size(171, 24);
            this.IpcConnectingStatusLabel.TabIndex = 0;
            this.IpcConnectingStatusLabel.Text = "Connecting to IPC..";
            // 
            // SwitchToServerButton
            // 
            this.SwitchToServerButton.Location = new System.Drawing.Point(12, 119);
            this.SwitchToServerButton.Name = "SwitchToServerButton";
            this.SwitchToServerButton.Size = new System.Drawing.Size(229, 25);
            this.SwitchToServerButton.TabIndex = 10;
            this.SwitchToServerButton.Text = "Switch to server";
            this.SwitchToServerButton.UseVisualStyleBackColor = true;
            this.SwitchToServerButton.Click += new System.EventHandler(this.SwitchToServerButton_Click);
            // 
            // ClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(249, 156);
            this.Controls.Add(this.IpcDisconnectedPanel);
            this.Controls.Add(this.SwitchToServerButton);
            this.Controls.Add(this.ConnectedPanel);
            this.Controls.Add(this.DisconnectedPanel);
            this.Name = "ClientForm";
            this.Text = "Inputshare Client";
            this.Load += new System.EventHandler(this.ClientForm_Load);
            this.DisconnectedPanel.ResumeLayout(false);
            this.DisconnectedPanel.PerformLayout();
            this.ConnectedPanel.ResumeLayout(false);
            this.ConnectedPanel.PerformLayout();
            this.IpcDisconnectedPanel.ResumeLayout(false);
            this.IpcDisconnectedPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button ConnectButton;
        private System.Windows.Forms.Label AddressHeaderLabel;
        private System.Windows.Forms.TextBox AddressTextBox;
        private System.Windows.Forms.Label ClientNameHeaderLabel;
        private System.Windows.Forms.TextBox ClientNameTextBox;
        private System.Windows.Forms.TextBox AddressPortTextBox;
        private System.Windows.Forms.Button DisconnectButton;
        private System.Windows.Forms.Panel DisconnectedPanel;
        private System.Windows.Forms.Panel ConnectedPanel;
        private System.Windows.Forms.Panel IpcDisconnectedPanel;
        private System.Windows.Forms.Label IpcConnectingStatusLabel;
        private System.Windows.Forms.Button SwitchToServerButton;
        private System.Windows.Forms.Label ConnectedPanelAddressLabel;
    }
}
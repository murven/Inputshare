namespace Inputshare
{
    partial class MainForm
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
            System.Windows.Forms.Label ServerSettingsHeaderLabel;
            this.ConsoleTextBox = new System.Windows.Forms.RichTextBox();
            this.ServerStartButton = new System.Windows.Forms.Button();
            this.ClientListBox = new System.Windows.Forms.ListBox();
            this.ClientListHeaderLabel = new System.Windows.Forms.Label();
            this.ClientSettingsHeader = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.ClientHotkeyButton = new System.Windows.Forms.Button();
            this.ClientSettingsPanel = new System.Windows.Forms.Panel();
            this.RightClientListBox = new System.Windows.Forms.ComboBox();
            this.LeftClientListBox = new System.Windows.Forms.ComboBox();
            this.BelowClientListBox = new System.Windows.Forms.ComboBox();
            this.AboveClientListBox = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.ClientSettingsClientNameLabel = new System.Windows.Forms.Label();
            this.SettingsCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.ApplyServerSettingsButton = new System.Windows.Forms.Button();
            this.ClientStartButton = new System.Windows.Forms.Button();
            this.ServerPortTextBox = new System.Windows.Forms.TextBox();
            ServerSettingsHeaderLabel = new System.Windows.Forms.Label();
            this.ClientSettingsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // ServerSettingsHeaderLabel
            // 
            ServerSettingsHeaderLabel.AutoSize = true;
            ServerSettingsHeaderLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            ServerSettingsHeaderLabel.Location = new System.Drawing.Point(230, 9);
            ServerSettingsHeaderLabel.Name = "ServerSettingsHeaderLabel";
            ServerSettingsHeaderLabel.Size = new System.Drawing.Size(118, 20);
            ServerSettingsHeaderLabel.TabIndex = 11;
            ServerSettingsHeaderLabel.Text = "Server Settings";
            // 
            // ConsoleTextBox
            // 
            this.ConsoleTextBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ConsoleTextBox.Location = new System.Drawing.Point(0, 221);
            this.ConsoleTextBox.Name = "ConsoleTextBox";
            this.ConsoleTextBox.ReadOnly = true;
            this.ConsoleTextBox.Size = new System.Drawing.Size(770, 160);
            this.ConsoleTextBox.TabIndex = 0;
            this.ConsoleTextBox.Text = "";
            this.ConsoleTextBox.TextChanged += new System.EventHandler(this.ConsoleTextBox_TextChanged);
            // 
            // ServerStartButton
            // 
            this.ServerStartButton.Location = new System.Drawing.Point(12, 30);
            this.ServerStartButton.Name = "ServerStartButton";
            this.ServerStartButton.Size = new System.Drawing.Size(112, 23);
            this.ServerStartButton.TabIndex = 1;
            this.ServerStartButton.Text = "Start server";
            this.ServerStartButton.UseVisualStyleBackColor = true;
            this.ServerStartButton.Click += new System.EventHandler(this.ServerStartButton_Click);
            // 
            // ClientListBox
            // 
            this.ClientListBox.FormattingEnabled = true;
            this.ClientListBox.Location = new System.Drawing.Point(399, 32);
            this.ClientListBox.Name = "ClientListBox";
            this.ClientListBox.Size = new System.Drawing.Size(160, 186);
            this.ClientListBox.TabIndex = 2;
            // 
            // ClientListHeaderLabel
            // 
            this.ClientListHeaderLabel.AutoSize = true;
            this.ClientListHeaderLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ClientListHeaderLabel.Location = new System.Drawing.Point(439, 9);
            this.ClientListHeaderLabel.Name = "ClientListHeaderLabel";
            this.ClientListHeaderLabel.Size = new System.Drawing.Size(61, 20);
            this.ClientListHeaderLabel.TabIndex = 3;
            this.ClientListHeaderLabel.Text = "Clients:";
            // 
            // ClientSettingsHeader
            // 
            this.ClientSettingsHeader.AutoSize = true;
            this.ClientSettingsHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ClientSettingsHeader.Location = new System.Drawing.Point(611, 9);
            this.ClientSettingsHeader.Name = "ClientSettingsHeader";
            this.ClientSettingsHeader.Size = new System.Drawing.Size(112, 20);
            this.ClientSettingsHeader.TabIndex = 5;
            this.ClientSettingsHeader.Text = "Client Settings";
            this.ClientSettingsHeader.Click += new System.EventHandler(this.ClientSettingsHeader_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(74, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Hotkey";
            // 
            // ClientHotkeyButton
            // 
            this.ClientHotkeyButton.Location = new System.Drawing.Point(3, 46);
            this.ClientHotkeyButton.Name = "ClientHotkeyButton";
            this.ClientHotkeyButton.Size = new System.Drawing.Size(194, 42);
            this.ClientHotkeyButton.TabIndex = 7;
            this.ClientHotkeyButton.Text = "None";
            this.ClientHotkeyButton.UseVisualStyleBackColor = true;
            this.ClientHotkeyButton.Click += new System.EventHandler(this.ClientHotkeyButton_Click);
            // 
            // ClientSettingsPanel
            // 
            this.ClientSettingsPanel.Controls.Add(this.RightClientListBox);
            this.ClientSettingsPanel.Controls.Add(this.LeftClientListBox);
            this.ClientSettingsPanel.Controls.Add(this.BelowClientListBox);
            this.ClientSettingsPanel.Controls.Add(this.AboveClientListBox);
            this.ClientSettingsPanel.Controls.Add(this.label6);
            this.ClientSettingsPanel.Controls.Add(this.label5);
            this.ClientSettingsPanel.Controls.Add(this.label4);
            this.ClientSettingsPanel.Controls.Add(this.label3);
            this.ClientSettingsPanel.Controls.Add(this.ClientSettingsClientNameLabel);
            this.ClientSettingsPanel.Controls.Add(this.ClientHotkeyButton);
            this.ClientSettingsPanel.Controls.Add(this.label2);
            this.ClientSettingsPanel.Location = new System.Drawing.Point(570, 32);
            this.ClientSettingsPanel.Name = "ClientSettingsPanel";
            this.ClientSettingsPanel.Size = new System.Drawing.Size(200, 186);
            this.ClientSettingsPanel.TabIndex = 8;
            // 
            // RightClientListBox
            // 
            this.RightClientListBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.RightClientListBox.FormattingEnabled = true;
            this.RightClientListBox.Location = new System.Drawing.Point(45, 158);
            this.RightClientListBox.Name = "RightClientListBox";
            this.RightClientListBox.Size = new System.Drawing.Size(121, 21);
            this.RightClientListBox.TabIndex = 16;
            // 
            // LeftClientListBox
            // 
            this.LeftClientListBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.LeftClientListBox.FormattingEnabled = true;
            this.LeftClientListBox.Location = new System.Drawing.Point(45, 135);
            this.LeftClientListBox.Name = "LeftClientListBox";
            this.LeftClientListBox.Size = new System.Drawing.Size(121, 21);
            this.LeftClientListBox.TabIndex = 15;
            // 
            // BelowClientListBox
            // 
            this.BelowClientListBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.BelowClientListBox.FormattingEnabled = true;
            this.BelowClientListBox.Location = new System.Drawing.Point(45, 114);
            this.BelowClientListBox.Name = "BelowClientListBox";
            this.BelowClientListBox.Size = new System.Drawing.Size(121, 21);
            this.BelowClientListBox.TabIndex = 14;
            // 
            // AboveClientListBox
            // 
            this.AboveClientListBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.AboveClientListBox.FormattingEnabled = true;
            this.AboveClientListBox.Location = new System.Drawing.Point(45, 91);
            this.AboveClientListBox.Name = "AboveClientListBox";
            this.AboveClientListBox.Size = new System.Drawing.Size(121, 21);
            this.AboveClientListBox.TabIndex = 13;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 114);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(36, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Below";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 161);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(32, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Right";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(1, 91);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Above";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 138);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(25, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Left";
            // 
            // ClientSettingsClientNameLabel
            // 
            this.ClientSettingsClientNameLabel.AutoSize = true;
            this.ClientSettingsClientNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ClientSettingsClientNameLabel.Location = new System.Drawing.Point(61, 0);
            this.ClientSettingsClientNameLabel.Name = "ClientSettingsClientNameLabel";
            this.ClientSettingsClientNameLabel.Size = new System.Drawing.Size(54, 20);
            this.ClientSettingsClientNameLabel.TabIndex = 8;
            this.ClientSettingsClientNameLabel.Text = "---------";
            // 
            // SettingsCheckedListBox
            // 
            this.SettingsCheckedListBox.FormattingEnabled = true;
            this.SettingsCheckedListBox.Items.AddRange(new object[] {
            "Enable cursor switch",
            "Release all inputs on client switch",
            "Disable cursor switch while fullscreen"});
            this.SettingsCheckedListBox.Location = new System.Drawing.Point(191, 32);
            this.SettingsCheckedListBox.Name = "SettingsCheckedListBox";
            this.SettingsCheckedListBox.Size = new System.Drawing.Size(202, 154);
            this.SettingsCheckedListBox.TabIndex = 9;
            this.SettingsCheckedListBox.SelectedIndexChanged += new System.EventHandler(this.SettingsCheckedListBox_SelectedIndexChanged);
            // 
            // ApplyServerSettingsButton
            // 
            this.ApplyServerSettingsButton.Location = new System.Drawing.Point(191, 190);
            this.ApplyServerSettingsButton.Name = "ApplyServerSettingsButton";
            this.ApplyServerSettingsButton.Size = new System.Drawing.Size(202, 28);
            this.ApplyServerSettingsButton.TabIndex = 12;
            this.ApplyServerSettingsButton.Text = "Apply settings";
            this.ApplyServerSettingsButton.UseVisualStyleBackColor = true;
            this.ApplyServerSettingsButton.Click += new System.EventHandler(this.ApplyServerSettingsButton_Click);
            // 
            // ClientStartButton
            // 
            this.ClientStartButton.Location = new System.Drawing.Point(12, 57);
            this.ClientStartButton.Name = "ClientStartButton";
            this.ClientStartButton.Size = new System.Drawing.Size(173, 23);
            this.ClientStartButton.TabIndex = 13;
            this.ClientStartButton.Text = "Switch to client";
            this.ClientStartButton.UseVisualStyleBackColor = true;
            this.ClientStartButton.Click += new System.EventHandler(this.ClientStartButton_Click);
            // 
            // ServerPortTextBox
            // 
            this.ServerPortTextBox.Location = new System.Drawing.Point(130, 32);
            this.ServerPortTextBox.Name = "ServerPortTextBox";
            this.ServerPortTextBox.Size = new System.Drawing.Size(55, 20);
            this.ServerPortTextBox.TabIndex = 14;
            this.ServerPortTextBox.Text = "44101";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(770, 381);
            this.Controls.Add(this.ServerPortTextBox);
            this.Controls.Add(this.ClientStartButton);
            this.Controls.Add(this.ApplyServerSettingsButton);
            this.Controls.Add(ServerSettingsHeaderLabel);
            this.Controls.Add(this.SettingsCheckedListBox);
            this.Controls.Add(this.ClientSettingsHeader);
            this.Controls.Add(this.ClientSettingsPanel);
            this.Controls.Add(this.ClientListHeaderLabel);
            this.Controls.Add(this.ClientListBox);
            this.Controls.Add(this.ServerStartButton);
            this.Controls.Add(this.ConsoleTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MainForm";
            this.Text = "Inputshare";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ClientSettingsPanel.ResumeLayout(false);
            this.ClientSettingsPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox ConsoleTextBox;
        private System.Windows.Forms.Button ServerStartButton;
        private System.Windows.Forms.ListBox ClientListBox;
        private System.Windows.Forms.Label ClientListHeaderLabel;
        private System.Windows.Forms.Label ClientSettingsHeader;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button ClientHotkeyButton;
        private System.Windows.Forms.Panel ClientSettingsPanel;
        private System.Windows.Forms.Label ClientSettingsClientNameLabel;
        private System.Windows.Forms.ComboBox RightClientListBox;
        private System.Windows.Forms.ComboBox LeftClientListBox;
        private System.Windows.Forms.ComboBox BelowClientListBox;
        private System.Windows.Forms.ComboBox AboveClientListBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckedListBox SettingsCheckedListBox;
        private System.Windows.Forms.Button ApplyServerSettingsButton;
        private System.Windows.Forms.Button ClientStartButton;
        private System.Windows.Forms.TextBox ServerPortTextBox;
    }
}
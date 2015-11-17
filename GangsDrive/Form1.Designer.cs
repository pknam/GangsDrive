namespace GangsDrive
{
    partial class Form1
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnIsoStart = new System.Windows.Forms.Button();
            this.btnIsoStop = new System.Windows.Forms.Button();
            this.btnSftpStart = new System.Windows.Forms.Button();
            this.btnSftpStop = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbIsoPath = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbSftpPasswd = new System.Windows.Forms.TextBox();
            this.tbSftpUsername = new System.Windows.Forms.TextBox();
            this.tbSftpPort = new System.Windows.Forms.TextBox();
            this.tbSftpHost = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnIsoStart
            // 
            this.btnIsoStart.Location = new System.Drawing.Point(488, 20);
            this.btnIsoStart.Name = "btnIsoStart";
            this.btnIsoStart.Size = new System.Drawing.Size(75, 23);
            this.btnIsoStart.TabIndex = 2;
            this.btnIsoStart.Text = "ISO Start";
            this.btnIsoStart.UseVisualStyleBackColor = true;
            this.btnIsoStart.Click += new System.EventHandler(this.btnIsoStart_Click);
            // 
            // btnIsoStop
            // 
            this.btnIsoStop.Location = new System.Drawing.Point(488, 49);
            this.btnIsoStop.Name = "btnIsoStop";
            this.btnIsoStop.Size = new System.Drawing.Size(75, 23);
            this.btnIsoStop.TabIndex = 3;
            this.btnIsoStop.Text = "ISO Stop";
            this.btnIsoStop.UseVisualStyleBackColor = true;
            this.btnIsoStop.Click += new System.EventHandler(this.btnIsoStop_Click);
            // 
            // btnSftpStart
            // 
            this.btnSftpStart.Location = new System.Drawing.Point(494, 20);
            this.btnSftpStart.Name = "btnSftpStart";
            this.btnSftpStart.Size = new System.Drawing.Size(75, 23);
            this.btnSftpStart.TabIndex = 5;
            this.btnSftpStart.Text = "sftp start";
            this.btnSftpStart.UseVisualStyleBackColor = true;
            this.btnSftpStart.Click += new System.EventHandler(this.btnSftpStart_Click);
            // 
            // btnSftpStop
            // 
            this.btnSftpStop.Location = new System.Drawing.Point(494, 49);
            this.btnSftpStop.Name = "btnSftpStop";
            this.btnSftpStop.Size = new System.Drawing.Size(75, 23);
            this.btnSftpStop.TabIndex = 6;
            this.btnSftpStop.Text = "sftp stop";
            this.btnSftpStop.UseVisualStyleBackColor = true;
            this.btnSftpStop.Click += new System.EventHandler(this.btnSftpStop_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.tbIsoPath);
            this.groupBox1.Controls.Add(this.btnIsoStart);
            this.groupBox1.Controls.Add(this.btnIsoStop);
            this.groupBox1.Location = new System.Drawing.Point(31, 16);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(569, 88);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "ISO Drive";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 12);
            this.label1.TabIndex = 5;
            this.label1.Text = "ISO Path";
            // 
            // tbIsoPath
            // 
            this.tbIsoPath.Location = new System.Drawing.Point(66, 30);
            this.tbIsoPath.Name = "tbIsoPath";
            this.tbIsoPath.Size = new System.Drawing.Size(416, 21);
            this.tbIsoPath.TabIndex = 4;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.tbSftpPasswd);
            this.groupBox2.Controls.Add(this.tbSftpUsername);
            this.groupBox2.Controls.Add(this.tbSftpPort);
            this.groupBox2.Controls.Add(this.tbSftpHost);
            this.groupBox2.Controls.Add(this.btnSftpStart);
            this.groupBox2.Controls.Add(this.btnSftpStop);
            this.groupBox2.Location = new System.Drawing.Point(31, 124);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(569, 88);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "SFTP Drive";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(296, 54);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 12);
            this.label5.TabIndex = 14;
            this.label5.Text = "Passwd";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 54);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 12);
            this.label4.TabIndex = 13;
            this.label4.Text = "Username";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(296, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(27, 12);
            this.label3.TabIndex = 12;
            this.label3.Text = "Port";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 12);
            this.label2.TabIndex = 11;
            this.label2.Text = "Host";
            // 
            // tbSftpPasswd
            // 
            this.tbSftpPasswd.Location = new System.Drawing.Point(353, 49);
            this.tbSftpPasswd.Name = "tbSftpPasswd";
            this.tbSftpPasswd.PasswordChar = '*';
            this.tbSftpPasswd.Size = new System.Drawing.Size(129, 21);
            this.tbSftpPasswd.TabIndex = 10;
            // 
            // tbSftpUsername
            // 
            this.tbSftpUsername.Location = new System.Drawing.Point(75, 49);
            this.tbSftpUsername.Name = "tbSftpUsername";
            this.tbSftpUsername.Size = new System.Drawing.Size(195, 21);
            this.tbSftpUsername.TabIndex = 9;
            // 
            // tbSftpPort
            // 
            this.tbSftpPort.Location = new System.Drawing.Point(353, 22);
            this.tbSftpPort.Name = "tbSftpPort";
            this.tbSftpPort.Size = new System.Drawing.Size(129, 21);
            this.tbSftpPort.TabIndex = 8;
            // 
            // tbSftpHost
            // 
            this.tbSftpHost.Location = new System.Drawing.Point(75, 22);
            this.tbSftpHost.Name = "tbSftpHost";
            this.tbSftpHost.Size = new System.Drawing.Size(195, 21);
            this.tbSftpHost.TabIndex = 7;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(630, 285);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnIsoStart;
        private System.Windows.Forms.Button btnIsoStop;
        private System.Windows.Forms.Button btnSftpStart;
        private System.Windows.Forms.Button btnSftpStop;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbIsoPath;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbSftpPasswd;
        private System.Windows.Forms.TextBox tbSftpUsername;
        private System.Windows.Forms.TextBox tbSftpPort;
        private System.Windows.Forms.TextBox tbSftpHost;
    }
}


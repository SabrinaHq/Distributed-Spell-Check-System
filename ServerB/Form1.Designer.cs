
namespace ServerB
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.receivedword = new System.Windows.Forms.ListBox();
            this.BtnDisconnect = new System.Windows.Forms.Button();
            this.ConnectedClient = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(408, 111);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 20);
            this.label3.TabIndex = 18;
            this.label3.Text = "Added words:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 111);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(129, 20);
            this.label2.TabIndex = 17;
            this.label2.Text = "Connected clients:";
            // 
            // receivedword
            // 
            this.receivedword.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.receivedword.FormattingEnabled = true;
            this.receivedword.ItemHeight = 20;
            this.receivedword.Location = new System.Drawing.Point(408, 148);
            this.receivedword.Name = "receivedword";
            this.receivedword.Size = new System.Drawing.Size(383, 204);
            this.receivedword.TabIndex = 16;
            // 
            // BtnDisconnect
            // 
            this.BtnDisconnect.Location = new System.Drawing.Point(176, 379);
            this.BtnDisconnect.Name = "BtnDisconnect";
            this.BtnDisconnect.Size = new System.Drawing.Size(287, 29);
            this.BtnDisconnect.TabIndex = 13;
            this.BtnDisconnect.Text = "Disconnect";
            this.BtnDisconnect.UseVisualStyleBackColor = true;
            this.BtnDisconnect.Click += new System.EventHandler(this.BtnDisconnect_Click);
            // 
            // ConnectedClient
            // 
            this.ConnectedClient.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.ConnectedClient.FormattingEnabled = true;
            this.ConnectedClient.ItemHeight = 20;
            this.ConnectedClient.Location = new System.Drawing.Point(10, 148);
            this.ConnectedClient.Name = "ConnectedClient";
            this.ConnectedClient.Size = new System.Drawing.Size(383, 204);
            this.ConnectedClient.TabIndex = 11;
            this.ConnectedClient.SelectedIndexChanged += new System.EventHandler(this.ConnectedClient_SelectedIndexChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.receivedword);
            this.Controls.Add(this.BtnDisconnect);
            this.Controls.Add(this.ConnectedClient);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox receivedword;
        private System.Windows.Forms.Button BtnDisconnect;
        private System.Windows.Forms.ListBox ConnectedClient;
    }
}


namespace Utilities
{
    partial class Form1
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
            this.rtbExpList = new System.Windows.Forms.RichTextBox();
            this.btnCalcAvgExp = new System.Windows.Forms.Button();
            this.rtbWhatever = new System.Windows.Forms.RichTextBox();
            this.btnWhatever = new System.Windows.Forms.Button();
            this.btnExpGen = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnPlaylist = new System.Windows.Forms.Button();
            this.tbPlaylistUrl = new System.Windows.Forms.TextBox();
            this.tbLive = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // rtbExpList
            // 
            this.rtbExpList.Location = new System.Drawing.Point(12, 12);
            this.rtbExpList.Name = "rtbExpList";
            this.rtbExpList.Size = new System.Drawing.Size(162, 232);
            this.rtbExpList.TabIndex = 0;
            this.rtbExpList.Text = "";
            // 
            // btnCalcAvgExp
            // 
            this.btnCalcAvgExp.Location = new System.Drawing.Point(12, 250);
            this.btnCalcAvgExp.Name = "btnCalcAvgExp";
            this.btnCalcAvgExp.Size = new System.Drawing.Size(162, 31);
            this.btnCalcAvgExp.TabIndex = 1;
            this.btnCalcAvgExp.Text = "Calculate Average Exp";
            this.btnCalcAvgExp.UseVisualStyleBackColor = true;
            this.btnCalcAvgExp.Click += new System.EventHandler(this.btnCalcAvgExp_Click);
            // 
            // rtbWhatever
            // 
            this.rtbWhatever.Location = new System.Drawing.Point(180, 12);
            this.rtbWhatever.Name = "rtbWhatever";
            this.rtbWhatever.Size = new System.Drawing.Size(251, 157);
            this.rtbWhatever.TabIndex = 2;
            this.rtbWhatever.Text = "";
            // 
            // btnWhatever
            // 
            this.btnWhatever.Location = new System.Drawing.Point(180, 175);
            this.btnWhatever.Name = "btnWhatever";
            this.btnWhatever.Size = new System.Drawing.Size(251, 31);
            this.btnWhatever.TabIndex = 3;
            this.btnWhatever.Text = "whatever coded";
            this.btnWhatever.UseVisualStyleBackColor = true;
            this.btnWhatever.Click += new System.EventHandler(this.btnWhatever_Click);
            // 
            // btnExpGen
            // 
            this.btnExpGen.Location = new System.Drawing.Point(180, 250);
            this.btnExpGen.Name = "btnExpGen";
            this.btnExpGen.Size = new System.Drawing.Size(251, 31);
            this.btnExpGen.TabIndex = 4;
            this.btnExpGen.Text = "generate exp";
            this.btnExpGen.UseVisualStyleBackColor = true;
            this.btnExpGen.Click += new System.EventHandler(this.btnExpGen_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(437, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(235, 31);
            this.button1.TabIndex = 5;
            this.button1.Text = "test extract mp3";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnPlaylist
            // 
            this.btnPlaylist.Location = new System.Drawing.Point(437, 108);
            this.btnPlaylist.Name = "btnPlaylist";
            this.btnPlaylist.Size = new System.Drawing.Size(235, 31);
            this.btnPlaylist.TabIndex = 6;
            this.btnPlaylist.Text = "test get playlist info";
            this.btnPlaylist.UseVisualStyleBackColor = true;
            this.btnPlaylist.Click += new System.EventHandler(this.btnPlaylist_Click);
            // 
            // tbPlaylistUrl
            // 
            this.tbPlaylistUrl.Location = new System.Drawing.Point(437, 82);
            this.tbPlaylistUrl.Name = "tbPlaylistUrl";
            this.tbPlaylistUrl.Size = new System.Drawing.Size(235, 20);
            this.tbPlaylistUrl.TabIndex = 7;
            // 
            // tbLive
            // 
            this.tbLive.Location = new System.Drawing.Point(437, 149);
            this.tbLive.Name = "tbLive";
            this.tbLive.Size = new System.Drawing.Size(235, 20);
            this.tbLive.TabIndex = 9;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(437, 175);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(235, 31);
            this.button2.TabIndex = 8;
            this.button2.Text = "test get live stream";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(437, 213);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(235, 31);
            this.button3.TabIndex = 10;
            this.button3.Text = "test get live stream";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(437, 250);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(235, 31);
            this.button4.TabIndex = 11;
            this.button4.Text = "test get live stream";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 293);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.tbLive);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.tbPlaylistUrl);
            this.Controls.Add(this.btnPlaylist);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnExpGen);
            this.Controls.Add(this.btnWhatever);
            this.Controls.Add(this.rtbWhatever);
            this.Controls.Add(this.btnCalcAvgExp);
            this.Controls.Add(this.rtbExpList);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbExpList;
        private System.Windows.Forms.Button btnCalcAvgExp;
        private System.Windows.Forms.RichTextBox rtbWhatever;
        private System.Windows.Forms.Button btnWhatever;
        private System.Windows.Forms.Button btnExpGen;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnPlaylist;
        private System.Windows.Forms.TextBox tbPlaylistUrl;
        private System.Windows.Forms.TextBox tbLive;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
    }
}


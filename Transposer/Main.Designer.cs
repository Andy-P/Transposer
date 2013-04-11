namespace Transposer
{
    partial class Main
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
            this.dataGridViewTrnspsr = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTrnspsr)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridViewTrnspsr
            // 
            this.dataGridViewTrnspsr.AllowUserToAddRows = false;
            this.dataGridViewTrnspsr.AllowUserToDeleteRows = false;
            this.dataGridViewTrnspsr.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewTrnspsr.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewTrnspsr.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewTrnspsr.Name = "dataGridViewTrnspsr";
            this.dataGridViewTrnspsr.ReadOnly = true;
            this.dataGridViewTrnspsr.Size = new System.Drawing.Size(190, 128);
            this.dataGridViewTrnspsr.TabIndex = 0;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(190, 128);
            this.Controls.Add(this.dataGridViewTrnspsr);
            this.MaximizeBox = false;
            this.Name = "Main";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Transposer";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTrnspsr)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridViewTrnspsr;
    }
}


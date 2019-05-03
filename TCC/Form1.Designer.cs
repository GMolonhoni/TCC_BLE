namespace TCC
{
    partial class PrincipalForm
    {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Windows Form Designer

        /// <summary>
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.devicesListComboBox = new System.Windows.Forms.ComboBox();
            this.updateButton = new System.Windows.Forms.Button();
            this.connectButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // devicesListComboBox
            // 
            this.devicesListComboBox.FormattingEnabled = true;
            this.devicesListComboBox.Location = new System.Drawing.Point(345, 57);
            this.devicesListComboBox.Name = "devicesListComboBox";
            this.devicesListComboBox.Size = new System.Drawing.Size(121, 21);
            this.devicesListComboBox.TabIndex = 0;
            this.devicesListComboBox.SelectedIndexChanged += new System.EventHandler(this.devicesListComboBox_SelectedIndexChanged);
            // 
            // updateButton
            // 
            this.updateButton.Location = new System.Drawing.Point(250, 57);
            this.updateButton.Name = "updateButton";
            this.updateButton.Size = new System.Drawing.Size(75, 23);
            this.updateButton.TabIndex = 1;
            this.updateButton.Text = "Update";
            this.updateButton.UseVisualStyleBackColor = true;
            this.updateButton.Click += new System.EventHandler(this.updateButton_Click);
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(493, 55);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(75, 23);
            this.connectButton.TabIndex = 2;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // PrincipalForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.updateButton);
            this.Controls.Add(this.devicesListComboBox);
            this.Name = "PrincipalForm";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.PrincipalForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox devicesListComboBox;
        private System.Windows.Forms.Button updateButton;
        private System.Windows.Forms.Button connectButton;
    }
}


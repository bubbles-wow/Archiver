using CefSharp.WinForms;

namespace MSAuth.Popup
{
    partial class OAuthPopup
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
            SuspendLayout();
            // 
            // OAuthPopup
            // 
            AutoScaleDimensions = new SizeF(14F, 31F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImageLayout = ImageLayout.Center;
            ClientSize = new Size(2099, 1124);
            Margin = new Padding(2, 5, 2, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "OAuthPopup";
            ShowIcon = false;
            ShowInTaskbar = false;
            Text = "Microsoft Account";
            Load += OAuthPopup_Load;
            ResumeLayout(false);
        }

        #endregion
    }
}
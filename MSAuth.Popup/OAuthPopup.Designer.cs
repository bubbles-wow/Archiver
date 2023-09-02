﻿using CefSharp.WinForms;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OAuthPopup));
            SuspendLayout();
            // 
            // OAuthPopup
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(14F, 31F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoValidate = AutoValidate.EnablePreventFocusChange;
            BackgroundImageLayout = ImageLayout.Center;
            ClientSize = new Size(868, 703);
            ForeColor = SystemColors.ControlText;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(2, 5, 2, 5);
            Name = "OAuthPopup";
            Text = "Sign in Microsoft Account";
            Load += OAuthPopup_Load;
            ResumeLayout(false);
        }

        #endregion
    }
}
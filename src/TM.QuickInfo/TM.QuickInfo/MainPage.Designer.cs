namespace TM.QuickInfo
{
    partial class MainPage
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

        #region Wisej.NET Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pnlControls = new Wisej.Web.Panel();
            btnSearch = new Wisej.Web.Button();
            txtSearch = new Wisej.Web.TextBox();
            lblPrompt = new Wisej.Web.Label();
            tabResults = new Wisej.Web.TabControl();
            tabHtml = new Wisej.Web.TabPage();
            webBrowser = new Wisej.Web.HtmlPanel();
            tabNative = new Wisej.Web.TabPage();
            pnlNativeResults = new Wisej.Web.Panel();
            pnlControls.SuspendLayout();
            tabResults.SuspendLayout();
            tabHtml.SuspendLayout();
            tabNative.SuspendLayout();
            SuspendLayout();
            // 
            // pnlControls
            // 
            pnlControls.Controls.Add(btnSearch);
            pnlControls.Controls.Add(txtSearch);
            pnlControls.Controls.Add(lblPrompt);
            pnlControls.Cursor = null;
            pnlControls.Dock = Wisej.Web.DockStyle.Top;
            pnlControls.Location = new System.Drawing.Point(0, 0);
            pnlControls.Name = "pnlControls";
            pnlControls.Size = new System.Drawing.Size(1250, 60);
            pnlControls.TabIndex = 0;
            // 
            // btnSearch
            // 
            btnSearch.Anchor = Wisej.Web.AnchorStyles.Top | Wisej.Web.AnchorStyles.Right;
            btnSearch.Location = new System.Drawing.Point(1150, 18);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new System.Drawing.Size(88, 30);
            btnSearch.TabIndex = 2;
            btnSearch.Text = "Search";
            btnSearch.Click += btnSearch_Click;
            // 
            // txtSearch
            // 
            txtSearch.Anchor = Wisej.Web.AnchorStyles.Top | Wisej.Web.AnchorStyles.Left | Wisej.Web.AnchorStyles.Right;
            txtSearch.Location = new System.Drawing.Point(100, 20);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new System.Drawing.Size(1044, 22);
            txtSearch.TabIndex = 1;
            txtSearch.KeyDown += txtSearch_KeyDown;
            // 
            // lblPrompt
            // 
            lblPrompt.AutoSize = true;
            lblPrompt.Location = new System.Drawing.Point(12, 23);
            lblPrompt.Name = "lblPrompt";
            lblPrompt.Size = new System.Drawing.Size(68, 16);
            lblPrompt.TabIndex = 0;
            lblPrompt.Text = "Quick Info:";
            // 
            // tabResults
            // 
            tabResults.Controls.Add(tabHtml);
            tabResults.Controls.Add(tabNative);
            tabResults.Cursor = null;
            tabResults.Dock = Wisej.Web.DockStyle.Fill;
            tabResults.Location = new System.Drawing.Point(0, 60);
            tabResults.Name = "tabResults";
            tabResults.PageInsets = new Wisej.Web.Padding(1, 25, 1, 1);
            tabResults.Size = new System.Drawing.Size(1250, 912);
            tabResults.TabIndex = 1;
            // 
            // tabHtml
            // 
            tabHtml.Controls.Add(webBrowser);
            tabHtml.Cursor = null;
            tabHtml.Location = new System.Drawing.Point(1, 25);
            tabHtml.Name = "tabHtml";
            tabHtml.Padding = new Wisej.Web.Padding(3);
            tabHtml.Size = new System.Drawing.Size(1248, 886);
            tabHtml.Text = "HTML View";
            // 
            // webBrowser
            // 
            webBrowser.Dock = Wisej.Web.DockStyle.Fill;
            webBrowser.Focusable = false;
            webBrowser.Location = new System.Drawing.Point(3, 3);
            webBrowser.Name = "webBrowser";
            webBrowser.Size = new System.Drawing.Size(1242, 880);
            webBrowser.TabIndex = 0;
            webBrowser.TabStop = false;
            // 
            // tabNative
            // 
            tabNative.Controls.Add(pnlNativeResults);
            tabNative.Cursor = null;
            tabNative.Location = new System.Drawing.Point(1, 25);
            tabNative.Name = "tabNative";
            tabNative.Padding = new Wisej.Web.Padding(3);
            tabNative.Size = new System.Drawing.Size(1248, 886);
            tabNative.Text = "Native View";
            // 
            // pnlNativeResults
            // 
            pnlNativeResults.AutoScroll = true;
            pnlNativeResults.Cursor = null;
            pnlNativeResults.Dock = Wisej.Web.DockStyle.Fill;
            pnlNativeResults.Location = new System.Drawing.Point(3, 3);
            pnlNativeResults.Name = "pnlNativeResults";
            pnlNativeResults.Size = new System.Drawing.Size(1242, 880);
            pnlNativeResults.TabIndex = 0;
            // 
            // MainPage
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            AutoScaleMode = Wisej.Web.AutoScaleMode.Font;
            Controls.Add(tabResults);
            Controls.Add(pnlControls);
            Name = "MainPage";
            Size = new System.Drawing.Size(1250, 972);
            Text = "QuickInfo - Instant Answers";
            pnlControls.ResumeLayout(false);
            pnlControls.PerformLayout();
            tabResults.ResumeLayout(false);
            tabHtml.ResumeLayout(false);
            tabNative.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private Wisej.Web.Panel pnlControls;
        private Wisej.Web.Button btnSearch;
        private Wisej.Web.TextBox txtSearch;
        private Wisej.Web.Label lblPrompt;
        private Wisej.Web.TabControl tabResults;
        private Wisej.Web.TabPage tabHtml;
        private Wisej.Web.HtmlPanel webBrowser;
        private Wisej.Web.TabPage tabNative;
        private Wisej.Web.Panel pnlNativeResults;
    }
}

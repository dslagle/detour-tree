﻿namespace DetourList
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
            this.detourSelectionList1 = new DetourList.DetourSelectionList();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // detourSelectionList1
            // 
            this.detourSelectionList1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(191)))), ((int)(((byte)(192)))));
            this.detourSelectionList1.EmptyMessage = "No Items to Display";
            this.detourSelectionList1.Font = new System.Drawing.Font("Segoe UI", 7.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.detourSelectionList1.Location = new System.Drawing.Point(50, 59);
            this.detourSelectionList1.Name = "detourSelectionList1";
            this.detourSelectionList1.Size = new System.Drawing.Size(833, 337);
            this.detourSelectionList1.Style.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.detourSelectionList1.Style.CheckColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(70)))), ((int)(((byte)(140)))));
            this.detourSelectionList1.Style.CheckFillChecked = System.Drawing.Color.Transparent;
            this.detourSelectionList1.Style.CheckFillCheckedPartial = System.Drawing.Color.Transparent;
            this.detourSelectionList1.Style.CheckFillUnchecked = System.Drawing.Color.Transparent;
            this.detourSelectionList1.Style.ExpandColorCollapsed = System.Drawing.Color.Black;
            this.detourSelectionList1.Style.ExpandColorExpanded = System.Drawing.Color.Black;
            this.detourSelectionList1.Style.ExpandColorHot = System.Drawing.Color.Black;
            this.detourSelectionList1.Style.ExpandFillCollapsed = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(170)))), ((int)(((byte)(140)))));
            this.detourSelectionList1.Style.ExpandFillExpanded = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
            this.detourSelectionList1.Style.ExpandFillHot = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(70)))), ((int)(((byte)(140)))));
            this.detourSelectionList1.Style.Font = new System.Drawing.Font("Segoe UI", 7.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.detourSelectionList1.Style.HasBorder = true;
            this.detourSelectionList1.Style.ItemHighlightColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
            this.detourSelectionList1.Style.ItemIndent = 8;
            this.detourSelectionList1.Style.ItemSelectedColor = System.Drawing.Color.White;
            this.detourSelectionList1.Style.TextColor = System.Drawing.Color.Empty;
            this.detourSelectionList1.TabIndex = 0;
            this.detourSelectionList1.Text = "detourSelectionList1";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(50, 419);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(118, 38);
            this.button1.TabIndex = 1;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1203, 854);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.detourSelectionList1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private DetourSelectionList detourSelectionList1;
        private System.Windows.Forms.Button button1;
    }
}


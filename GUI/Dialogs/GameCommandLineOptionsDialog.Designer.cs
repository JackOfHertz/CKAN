namespace CKAN.GUI
{
    partial class GameCommandLineOptionsDialog
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(GameCommandLineOptionsDialog));
            this.AdditionalArguments = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.AcceptChangesButton = new System.Windows.Forms.Button();
            this.CancelChangesButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // AdditionalArguments
            //
            this.AdditionalArguments.Location = new System.Drawing.Point(15, 25);
            this.AdditionalArguments.Name = "AdditionalArguments";
            this.AdditionalArguments.Size = new System.Drawing.Size(457, 20);
            this.AdditionalArguments.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.AdditionalArguments.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.AdditionalArguments.TabIndex = 1;
            //
            // label1
            //
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 2;
            resources.ApplyResources(this.label1, "label1");
            //
            // AcceptChangesButton
            //
            this.AcceptChangesButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.AcceptChangesButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AcceptChangesButton.Location = new System.Drawing.Point(397, 51);
            this.AcceptChangesButton.Name = "AcceptChangesButton";
            this.AcceptChangesButton.Size = new System.Drawing.Size(75, 23);
            this.AcceptChangesButton.TabIndex = 3;
            this.AcceptChangesButton.UseVisualStyleBackColor = true;
            resources.ApplyResources(this.AcceptChangesButton, "AcceptChangesButton");
            //
            // CancelChangesButton
            //
            this.CancelChangesButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelChangesButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CancelChangesButton.Location = new System.Drawing.Point(316, 51);
            this.CancelChangesButton.Name = "CancelChangesButton";
            this.CancelChangesButton.Size = new System.Drawing.Size(75, 23);
            this.CancelChangesButton.TabIndex = 4;
            this.CancelChangesButton.UseVisualStyleBackColor = true;
            resources.ApplyResources(this.CancelChangesButton, "CancelChangesButton");
            //
            // GameCommandLineOptionsDialog
            //
            this.AcceptButton = this.AcceptChangesButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CancelChangesButton;
            this.ClientSize = new System.Drawing.Size(481, 112);
            this.ControlBox = false;
            this.Controls.Add(this.CancelChangesButton);
            this.Controls.Add(this.AcceptChangesButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.AdditionalArguments);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = EmbeddedImages.AppIcon;
            this.Name = "GameCommandLineOptionsDialog";
            resources.ApplyResources(this, "$this");
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button AcceptChangesButton;
        private System.Windows.Forms.Button CancelChangesButton;
        public System.Windows.Forms.TextBox AdditionalArguments;
    }
}

namespace ItemModifier
{
    partial class ItemTypePopup
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
            this.createItem = new System.Windows.Forms.Button();
            this.cancel = new System.Windows.Forms.Button();
            this.itemType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.itemBaseBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.itemBaseBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // createItem
            // 
            this.createItem.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.createItem.Location = new System.Drawing.Point(139, 27);
            this.createItem.Name = "createItem";
            this.createItem.Size = new System.Drawing.Size(75, 23);
            this.createItem.TabIndex = 0;
            this.createItem.Text = "Create Item";
            this.createItem.UseVisualStyleBackColor = true;
            this.createItem.Click += new System.EventHandler(this.CreateItem_Click);
            // 
            // cancel
            // 
            this.cancel.BackColor = System.Drawing.Color.LightCoral;
            this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancel.ForeColor = System.Drawing.Color.Black;
            this.cancel.Location = new System.Drawing.Point(12, 104);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(202, 23);
            this.cancel.TabIndex = 1;
            this.cancel.Text = "Cancel";
            this.cancel.UseVisualStyleBackColor = false;
            // 
            // itemType
            // 
            this.itemType.DataBindings.Add(new System.Windows.Forms.Binding("SelectedValue", this.itemBaseBindingSource, "Name", true));
            this.itemType.FormattingEnabled = true;
            this.itemType.Items.AddRange(new object[] {
            "Armor",
            "Consumable",
            "Necklace",
            "Resource",
            "Ring",
            "Weapon"});
            this.itemType.Location = new System.Drawing.Point(12, 29);
            this.itemType.Name = "itemType";
            this.itemType.Size = new System.Drawing.Size(121, 21);
            this.itemType.Sorted = true;
            this.itemType.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Select New Item Type:";
            // 
            // itemBaseBindingSource
            // 
            this.itemBaseBindingSource.DataSource = typeof(Items.ItemBase);
            // 
            // ItemTypePopup
            // 
            this.AcceptButton = this.createItem;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancel;
            this.ClientSize = new System.Drawing.Size(223, 144);
            this.ControlBox = false;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.itemType);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.createItem);
            this.KeyPreview = true;
            this.Name = "ItemTypePopup";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ItemTypePopup";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.itemBaseBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button createItem;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.ComboBox itemType;
        private System.Windows.Forms.BindingSource itemBaseBindingSource;
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ItemModifier
{
    public partial class ItemTypePopup : Form
    {
        public string ItemType { get; private set; }

        public ItemTypePopup()
        {
            InitializeComponent();
        }

        private void CreateItem_Click(object sender, EventArgs e)
        {
        }
    }
}

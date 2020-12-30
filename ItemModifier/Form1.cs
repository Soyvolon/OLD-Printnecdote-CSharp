using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Items;
using Items.Modifiers;
using Items.Utility;

namespace ItemModifier
{
    public partial class Editor : Form
    {
        // Database Information
        public static readonly SqlConnectionStringBuilder conn = new SqlConnectionStringBuilder //Create internal SQL configuration
        {
            UserID = "admin",
            DataSource = "cessumdb.cam7nhcocotl.us-east-1.rds.amazonaws.com",
            TrustServerCertificate = true,
        };

        public Editor()
        {
            InitializeComponent();
            wDmgModsList.DataSource = Enum.GetValues(typeof(DamageModifiers));
            armorModsList.DataSource = Enum.GetValues(typeof(DamageModifiers));
            nModsList.DataSource = Enum.GetValues(typeof(PlayerModifiers));
            ringModsList.DataSource = Enum.GetValues(typeof(PlayerModifiers));
            rarityComboBox.DataSource = Enum.GetValues(typeof(Rarity));
        }

        private void loginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            conn.Password = Interaction.InputBox("Enter SQL Password for Printnecdote.");
            using (SqlConnection server = new SqlConnection(conn.ConnectionString))
            {
                try
                {
                    server.Open();
                    mainPanel.Enabled = true;
                    server.Close();
                    pullDevDb_Click(sender, e);
                }
                catch
                {
                    Interaction.MsgBox("Invalid Password\nor an error occoured", MsgBoxStyle.Exclamation);
                }
            }
        }

        private void weaponRad_CheckedChanged(object sender, EventArgs e) { }

        private ItemBase lastSelection;
        private int lastIndex = -1;
        private void itemBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (itemBox.SelectedIndex != lastIndex)
            {
                if (itemBox.SelectedItem != null)
                {
                    ItemBase structure = itemBox.SelectedItem as ItemBase;
                    FillForm(structure);
                    EnableItemEditor();
                }
                else
                {
                    DisableItemEditor();
                }
                /*
                if (lastSelection != null)
                {
                    saveItem(lastSelection, lastIndex);
                }

                lastSelection = itemBox.SelectedItem as ItemBase;
                lastIndex = itemBox.SelectedIndex;*/
            }
        }

        private void newItem_Click(object sender, EventArgs e)
        {
            ItemTypePopup popup = new ItemTypePopup();
            DialogResult result = popup.ShowDialog();
            if (result == DialogResult.OK)
            {
                int nextId = -1;
                List<ItemBase> items = new List<ItemBase>();
                foreach(var item in itemBox.Items)
                {
                    items.Add(item as ItemBase);
                }

                for (int i = 0; nextId == -1; i++)
                {
                    if(items.Any(x => x.ItemId == i) == false)
                    {
                        nextId = i;
                    }
                }

                ItemBase toAdd = null;

                switch (popup.itemType.SelectedItem)
                {
                    case "Armor":
                        toAdd = new Armor("New Item", nextId, Rarity.Common);
                        break;
                    case "Consumable":
                        toAdd = new Consumable("New Item", nextId, Rarity.Common);
                        break;
                    case "Necklace":
                        toAdd = new Necklace("New Item", nextId, Rarity.Common);
                        break;
                    case "Resource":
                        toAdd = new Resource("New Item", nextId, Rarity.Common);
                        break;
                    case "Ring":
                        toAdd = new Ring("New Item", nextId, Rarity.Common);
                        break;
                    case "Weapon":
                        toAdd = new Weapon("New Item", nextId, Rarity.Common);
                        break;
                }

                itemBox.Items.Add(toAdd);
                itemBox.SelectedItem = toAdd;
            }
            popup.Dispose();
        }

        private void EnableItemEditor()
        {
            infoControl.Enabled = true;
            itemBox.Enabled = false;
            saveToDev.Enabled = false;
            listRefresh.Enabled = false;
            newItem.Enabled = false;
            pullDevDb.Enabled = false;
            pullReleaseDb.Enabled = false;
        }
        private void DisablePanels()
        {
            armorPanel.Enabled = false;
            consumablePanel.Enabled = false;
            necklacePanel.Enabled = false;
            resourcePanel.Enabled = false;
            ringPanel.Enabled = false;
            weaponPanel.Enabled = false;
        }

        private void DisableItemEditor()
        {
            infoControl.Enabled = false;
            itemBox.Enabled = true;
            saveToDev.Enabled = true;
            listRefresh.Enabled = true;
            newItem.Enabled = true;
            pullDevDb.Enabled = true;
            pullReleaseDb.Enabled = true;
            DisablePanels();
        }

        private void FillForm(ItemBase s)
        {
            DisablePanels();

            idLabel.Text = s.ItemId.ToString();
            nameTextBox.Text = s.Name;
            rarityComboBox.SelectedItem = s.ItemRarity;

            switch (s)
            {
                case Consumable c:
                    consumableRad.Select();
                    FillConsumableForm(c);
                    break;
                case Necklace n:
                    necklaceRad.Select();
                    FillNecklaceForm(n);
                    break;
                case Resource r:
                    resourceRad.Select();
                    FillResourceForm(r);
                    break;
                case Weapon w:
                    weaponRad.Select();
                    FillWeaponForm(w);
                    break;
                case Ring r:
                    ringRad.Select();
                    FillRingForm(r);
                    break;
                case Armor a:
                    armorRad.Select();
                    FillArmorForm(a);
                    break;
            }

        }

        private void FillConsumableForm(Consumable item)
        {
            consumablePanel.Enabled = true;
        }

        private void FillNecklaceForm(Necklace item)
        {
            necklacePanel.Enabled = true;
            PlayerModifiersDict = item.Modifiers;
            if(PlayerModifiersDict ==  null)
            {
                PlayerModifiersDict = new Dictionary<PlayerModifiers, int>();
            }
        }

        private void FillResourceForm(Resource item)
        {
            resourcePanel.Enabled = true;
        }

        private void FillWeaponForm(Weapon item)
        {
            weaponPanel.Enabled = true;
            wBaseDmgTextBox.Text = item.BaseDamage.ToString();
            wDurTextBox.Text = item.Durability.ToString();
            WeaponDamageModifers = item.WeaponDamageModifers;
            if(WeaponDamageModifers == null)
            {
                WeaponDamageModifers = new Dictionary<DamageModifiers, int>();
            }
        }

        private void FillRingForm(Ring item)
        {
            ringPanel.Enabled = true;
            PlayerModifiersDict = item.Modifiers;
            if(PlayerModifiersDict == null)
            {
                PlayerModifiersDict = new Dictionary<PlayerModifiers, int>();
            }
        }

        private void FillArmorForm(Armor item)
        {
            armorPanel.Enabled = true;
            armorBaseArmorTextBox.Text = item.BaseArmor.ToString();
            armorDurabilityTextBox.Text = item.Durability.ToString();
            WeaponDamageModifers = item.ArmorModifiers;
            if(WeaponDamageModifers == null)
            {
                WeaponDamageModifers = new Dictionary<DamageModifiers, int>();
            }
            switch(item.Type)
            {
                case Armor.ArmorType.Head:
                    aTypeHead.Select();
                    break;
                case Armor.ArmorType.Chest:
                    aTypeChest.Select();
                    break;
                case Armor.ArmorType.Leg:
                    aTypeLeg.Select();
                    break;
                case Armor.ArmorType.Feet:
                    aTypeFeet.Select();
                    break;
            }
        }

        private void saveToDev_Click(object sender, EventArgs e)
        {
            switch (Interaction.MsgBox("Are you sure you want to replace the remote DB with your local data?", MsgBoxStyle.YesNo))
            {
                case MsgBoxResult.Yes:
                    using (SqlConnection server = new SqlConnection(conn.ConnectionString))
                    {
                        server.Open();
                        List<Tuple<long, int, string>> values = new List<Tuple<long, int, string>>();
                        foreach (ItemBase s in itemBox.Items)
                        {
                            values.Add(new Tuple<long, int, string>(s.ItemId, GetItemType(s), JsonConvert.SerializeObject(s)));
                        }
                        DataTable table = new DataTable();
                        DataRow row;
                        using (SqlDataAdapter a = new SqlDataAdapter($"SELECT TOP 0 * FROM PrintnecdoteDev.dbo.ItemConfig", server))
                        {
                            a.Fill(table);
                        }
                        foreach (Tuple<long, int, string> tup in values)
                        {
                            row = table.NewRow();
                            row["Id"] = tup.Item1;
                            row["ItemType"] = tup.Item2;
                            row["DataString"] = tup.Item3;
                            table.Rows.Add(row);
                        }
                        new SqlCommand("DELETE FROM PrintnecdoteDev.dbo.ItemConfig", server).ExecuteNonQuery();
                        using (SqlBulkCopy bulk = new SqlBulkCopy(conn.ConnectionString))
                        {
                            bulk.DestinationTableName = "PrintnecdoteDev.dbo.ItemConfig";
                            bulk.WriteToServer(table);
                            Interaction.MsgBox("Write to server complete.", MsgBoxStyle.Information);
                        }
                    }
                    break;
            }
        }

        private int GetItemType(ItemBase item)
        {
            switch (item)
            {
                case Armor _:
                    return 0;
                case Consumable _:
                    return 1;
                case Necklace _:
                    return 2;
                case Resource _:
                    return 3;
                case Ring _:
                    return 4;
                case Weapon _:
                    return 5;
            }
            return -1;
        }

        private void pullDevDb_Click(object sender, EventArgs e)
        {
            using (SqlConnection server = new SqlConnection(conn.ConnectionString))
            {
                server.Open();
                itemBox.Items.Clear();
                SqlCommand cmd = new SqlCommand("SELECT * FROM PrintnecdoteDev.dbo.ItemConfig", server);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        switch (reader.GetInt32(1))
                        {
                            case 0: //Armor
                                itemBox.Items.Add(JsonConvert.DeserializeObject<Armor>(reader.GetString(2)));
                                break;
                            case 1: //Consumable
                                itemBox.Items.Add(JsonConvert.DeserializeObject<Consumable>(reader.GetString(2)));
                                break;
                            case 2: //Necklace
                                itemBox.Items.Add(JsonConvert.DeserializeObject<Necklace>(reader.GetString(2)));
                                break;
                            case 3: //Resource
                                itemBox.Items.Add(JsonConvert.DeserializeObject<Resource>(reader.GetString(2)));
                                break;
                            case 4: //Ring
                                itemBox.Items.Add(JsonConvert.DeserializeObject<Ring>(reader.GetString(2)));
                                break;
                            case 5: //Weapon
                                itemBox.Items.Add(JsonConvert.DeserializeObject<Weapon>(reader.GetString(2)));
                                break;
                        }
                    }
                }
            }
        }

        private void saveActiveItem_Click(object sender, EventArgs e)
        {
            saveItem(itemBox.SelectedItem as ItemBase, itemBox.SelectedIndex);
        }

        private void saveItem(ItemBase s, int index)
        {
            s.SetItemName(nameTextBox.Text);
            s.SetItemId(Convert.ToInt64(idLabel.Text));
            switch (rarityComboBox.SelectedItem)
            {
                case Rarity r:
                    s.ItemRarity = r;
                    break;
            }


            switch (s)
            {
                case Consumable c:
                    SaveConsumableForm(c);
                    break;
                case Necklace n:
                    SaveNecklaceForm(n);
                    break;
                case Resource r:
                    SaveResourceForm(r);
                    break;
                case Weapon w:
                    SaveWeaponForm(w);
                    break;
                case Ring r:
                    SaveRingForm(r);
                    break;
                case Armor a:
                    SaveArmorForm(a);
                    break;
            }

            itemBox.Items[index] = s;
            itemBox.Refresh();

            if (Interaction.MsgBox("Do you want to stop editing this item?", MsgBoxStyle.YesNo) == MsgBoxResult.Yes)
            {
                DisableItemEditor();
            }

        }

        private void SaveConsumableForm(Consumable item)
        {

        }

        private void SaveNecklaceForm(Necklace item)
        {
            item.Modifiers = PlayerModifiersDict;
            PlayerModifiersDict = new Dictionary<PlayerModifiers, int>();
        }

        private void SaveResourceForm(Resource item)
        {

        }

        private void SaveWeaponForm(Weapon item)
        {
            // All values should be correct for parsing.
            item.BaseDamage = (wBaseDmgTextBox.Text != "0" && wBaseDmgTextBox.Text != "") ? int.Parse(wBaseDmgTextBox.Text) : 0;
            item.Durability = (wDurTextBox.Text != "0" && wDurTextBox.Text != "") ? int.Parse(wDurTextBox.Text) : 0;
            item.WeaponDamageModifers = WeaponDamageModifers;
            WeaponDamageModifers = new Dictionary<DamageModifiers, int>();
        }

        private void SaveRingForm(Ring item)
        {
            item.Modifiers = PlayerModifiersDict;
            PlayerModifiersDict = new Dictionary<PlayerModifiers, int>();
        }

        private void SaveArmorForm(Armor item)
        {
            item.Type = GetArmorType();
            item.BaseArmor = (armorBaseArmorTextBox.Text != "0" && armorBaseArmorTextBox.Text != "") ? int.Parse(armorBaseArmorTextBox.Text) : 0;
            item.Durability = (armorDurabilityTextBox.Text != "0" && armorDurabilityTextBox.Text != "") ? int.Parse(armorDurabilityTextBox.Text) : 0;
            item.ArmorModifiers = WeaponDamageModifers;
            WeaponDamageModifers = new Dictionary<DamageModifiers, int>();
        }

        private Armor.ArmorType GetArmorType()
        {
            if (aTypeHead.Enabled)
            {
                return Armor.ArmorType.Head;
            }
            else if (aTypeChest.Enabled)
            {
                return Armor.ArmorType.Chest;
            }
            else if (aTypeLeg.Enabled)
            {
                return Armor.ArmorType.Leg;
            }
            else
            {
                return Armor.ArmorType.Feet;
            }

        }

        private void revertActiveItem_Click(object sender, EventArgs e)
        {
            switch (Interaction.MsgBox("Are you sure you want revert your changes wihtout saving?", MsgBoxStyle.YesNo))
            {
                case MsgBoxResult.Yes:
                    itemBox.ClearSelected();
                    DisableItemEditor();
                    break;
            }
        }

        private void Editor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Interaction.MsgBox("Save to DevDB before closing?", MsgBoxStyle.YesNo) == MsgBoxResult.Yes)
            {
                saveToDev_Click(sender, e);
            }
        }

        private void UpdateId_Click(object sender, EventArgs e)
        {
            if (idTextBox.Text != "")
            {
                long id = 0;
                if (long.TryParse(idTextBox.Text, out id))
                {
                    ItemBase[] items = new ItemBase[itemBox.Items.Count];
                    itemBox.Items.CopyTo(items, 0);
                    if (items.FirstOrDefault(x => x.ItemId == id && x.ItemId != (itemBox.SelectedItem as ItemBase).ItemId) != null)
                    {
                        Interaction.MsgBox("ID already in use", MsgBoxStyle.Exclamation);
                        idTextBox.Text = "";
                    }
                    else
                    {
                        idLabel.Text = idTextBox.Text;
                        idTextBox.Text = "";
                    }
                }
                else
                {
                    Interaction.MsgBox("ID must be a number.", MsgBoxStyle.Exclamation);
                    idTextBox.Text = "";
                }
            }
        }

        private void ListRefresh_Click(object sender, EventArgs e)
        {
            // Do something?
        }

        /// <summary>
        /// Used to text for values that are integers. If the value isnt an integer clear the text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TestForIntegerValue(object sender, EventArgs e)
        {
            switch (sender)
            {
                case TextBox box:
                    if (box.Text != "0" && box.Text != null && box.Text != "")
                    {
                        if (int.TryParse(box.Text, out _))
                        {
                            // All good, dont change anything
                        }
                        else
                        {
                            // Tell the user that is not allowed and clear text boxs
                            Interaction.MsgBox("Value must be an integer");
                            box.Text = "0";
                        }
                    }
                    break;
            }
        }

        // Holds values for dmg to be saved
        private Dictionary<DamageModifiers, int> WeaponDamageModifers = new Dictionary<DamageModifiers, int>();
        private DamageModifiers lastDmgMod;

        #region Weapon
        private void WDmgModsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Adjusts the informationd displayed when a different mod is slected
            DamageModifiers mod;
            if (Enum.TryParse(wDmgModsList.SelectedValue.ToString(), out mod))
            {
                switch (mod)
                {
                    case DamageModifiers.Blunt:
                        wDmgModsLbl.Text = "Blunt Damage Mod:";
                        break;
                    case DamageModifiers.Burn:
                        wDmgModsLbl.Text = "Burn Damage Mod:";
                        break;
                    case DamageModifiers.Freeze:
                        wDmgModsLbl.Text = "Freeze Damage Mod:";
                        break;
                    case DamageModifiers.Magic:
                        wDmgModsLbl.Text = "Magic Damage Mod:";
                        break;
                    case DamageModifiers.Pierce:
                        wDmgModsLbl.Text = "Pierce Damage Mod:";
                        break;
                    case DamageModifiers.Slash:
                        wDmgModsLbl.Text = "Slash Damage Mod:";
                        break;
                }
                // Updates the text box value of the mod if it exsists, else place a 0 in
                wDmgModsTextBox.Text = WeaponDamageModifers.ContainsKey(mod) ? WeaponDamageModifers[mod].ToString() : "0";

                wModsSaveLbl.Text = "Saved";
                wModsSaveLbl.ForeColor = Color.DarkGreen;

                lastDmgMod = mod;
            }
            else
            {
                wDmgModsTextBox.Text = "Error: mod not found in Enum";
            }
        }

        private void WModsUpdate_Click(object sender, EventArgs e)
        {
            if (int.TryParse(wDmgModsTextBox.Text, out int modValue))
            {
                // Add new mod or update exsisting with text box value
                if (WeaponDamageModifers.ContainsKey(lastDmgMod))
                {
                    WeaponDamageModifers[lastDmgMod] = modValue;
                }
                else
                {
                    WeaponDamageModifers.Add(lastDmgMod, modValue);
                }

                wModsSaveLbl.Text = "Saved";
                wModsSaveLbl.ForeColor = Color.DarkGreen;
            }
            else
            {
                Interaction.MsgBox("Mod value must be a number.", MsgBoxStyle.OkOnly);
            }
        }

        private void WDmgModsTextBox_TextChanged(object sender, EventArgs e)
        {
            wModsSaveLbl.Text = "Not Saved";
            wModsSaveLbl.ForeColor = Color.DarkRed;
        }

        
        #endregion
        #region Armor
        private void ArmorModsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Adjusts the informationd displayed when a different mod is slected
            DamageModifiers mod;
            if (Enum.TryParse(wDmgModsList.SelectedValue.ToString(), out mod))
            {
                switch (mod)
                {
                    case DamageModifiers.Blunt:
                        armorModLbl.Text = "Blunt Damage Mod:";
                        break;
                    case DamageModifiers.Burn:
                        armorModLbl.Text = "Burn Damage Mod:";
                        break;
                    case DamageModifiers.Freeze:
                        armorModLbl.Text = "Freeze Damage Mod:";
                        break;
                    case DamageModifiers.Magic:
                        armorModLbl.Text = "Magic Damage Mod:";
                        break;
                    case DamageModifiers.Pierce:
                        armorModLbl.Text = "Pierce Damage Mod:";
                        break;
                    case DamageModifiers.Slash:
                        armorModLbl.Text = "Slash Damage Mod:";
                        break;
                }
                // Updates the text box value of the mod if it exsists, else place a 0 in
                armorModTextBox.Text = WeaponDamageModifers.ContainsKey(mod) ? WeaponDamageModifers[mod].ToString() : "0";

                armorModsSaved.Text = "Saved";
                armorModsSaved.ForeColor = Color.DarkGreen;

                lastDmgMod = mod;
            }
            else
            {
                wDmgModsTextBox.Text = "Error: mod not found in Enum";
            }
        }

        private void ArmorModsUpdate_Click(object sender, EventArgs e)
        {
            if (int.TryParse(wDmgModsTextBox.Text, out int modValue))
            {
                // Add new mod or update exsisting with text box value
                if (WeaponDamageModifers.ContainsKey(lastDmgMod))
                {
                    WeaponDamageModifers[lastDmgMod] = modValue;
                }
                else
                {
                    WeaponDamageModifers.Add(lastDmgMod, modValue);
                }

                armorModsSaved.Text = "Saved";
                armorModsSaved.ForeColor = Color.DarkGreen;
            }
            else
            {
                Interaction.MsgBox("Mod value must be a number.", MsgBoxStyle.OkOnly);
            }
        }

        private void ArmorModTextBox_TextChanged(object sender, EventArgs e)
        {
            armorModsSaved.Text = "Not Saved";
            armorModsSaved.ForeColor = Color.DarkRed;
        }
        #endregion

        // Holds vlues for player mods to be saved
        private Dictionary<PlayerModifiers, int> PlayerModifiersDict = new Dictionary<PlayerModifiers, int>();
        private PlayerModifiers lastPMod;

        #region Necklace
        private void NModsTextBox_TextChanged(object sender, EventArgs e)
        {
            nModsSaved.Text = "Not Saved";
            nModsSaved.ForeColor = Color.DarkRed;
        }

        private void NModsUpdate_Click(object sender, EventArgs e)
        {
            if (int.TryParse(nModsTextBox.Text, out int modValue))
            {
                // Add new mod or update exsisting with text box value
                if (PlayerModifiersDict.ContainsKey(lastPMod))
                {
                    PlayerModifiersDict[lastPMod] = modValue;
                }
                else
                {
                    PlayerModifiersDict.Add(lastPMod, modValue);
                }

                nModsSaved.Text = "Saved";
                nModsSaved.ForeColor = Color.DarkGreen;
            }
            else
            {
                Interaction.MsgBox("Mod value must be a number.", MsgBoxStyle.OkOnly);
            }
        }

        private void NModsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Adjusts the informationd displayed when a different mod is slected
            PlayerModifiers mod;
            if (Enum.TryParse(nModsList.SelectedValue.ToString(), out mod))
            {
                switch (mod)
                {
                    case PlayerModifiers.Health:
                        nModsLbl.Text = "Health Modifier:";
                        break;
                    case PlayerModifiers.Magic:
                        nModsLbl.Text = "Magic Modifier:";
                        break;
                    case PlayerModifiers.Speed:
                        nModsLbl.Text = "Speed Modifier:";
                        break;

                }
                // Updates the text box value of the mod if it exsists, else place a 0 in
                nModsTextBox.Text = PlayerModifiersDict.ContainsKey(mod) ? PlayerModifiersDict[mod].ToString() : "0";

                nModsSaved.Text = "Saved";
                nModsSaved.ForeColor = Color.DarkGreen;

                lastPMod = mod;
            }
            else
            {
                nModsLbl.Text = "Error: mod not found in Enum";
            }
        }
        #endregion

        private void RingModsTextBox_TextChanged(object sender, EventArgs e)
        {
            ringModsSaved.Text = "Not Saved";
            ringModsSaved.ForeColor = Color.DarkRed;
        }

        private void RingModsUpdate_Click(object sender, EventArgs e)
        {
            if (int.TryParse(ringModsTextBox.Text, out int modValue))
            {
                // Add new mod or update exsisting with text box value
                if (PlayerModifiersDict.ContainsKey(lastPMod))
                {
                    PlayerModifiersDict[lastPMod] = modValue;
                }
                else
                {
                    PlayerModifiersDict.Add(lastPMod, modValue);
                }

                ringModsSaved.Text = "Saved";
                ringModsSaved.ForeColor = Color.DarkGreen;
            }
            else
            {
                Interaction.MsgBox("Mod value must be a number.", MsgBoxStyle.OkOnly);
            }
        }

        private void RingModsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Adjusts the informationd displayed when a different mod is slected
            PlayerModifiers mod;
            if (Enum.TryParse(nModsList.SelectedValue.ToString(), out mod))
            {
                switch (mod)
                {
                    case PlayerModifiers.Health:
                        ringModsLbl.Text = "Health Modifier:";
                        break;
                    case PlayerModifiers.Magic:
                        ringModsLbl.Text = "Magic Modifier:";
                        break;
                    case PlayerModifiers.Speed:
                        ringModsLbl.Text = "Speed Modifier:";
                        break;

                }
                // Updates the text box value of the mod if it exsists, else place a 0 in
                ringModsTextBox.Text = PlayerModifiersDict.ContainsKey(mod) ? PlayerModifiersDict[mod].ToString() : "0";

                ringModsSaved.Text = "Saved";
                ringModsSaved.ForeColor = Color.DarkGreen;

                lastPMod = mod;
            }
            else
            {
                nModsLbl.Text = "Error: mod not found in Enum";
            }
        }

        private void DeleteItem_Click(object sender, EventArgs e)
        {
            itemBox.Items.Remove(itemBox.SelectedItem);
            itemBox.Refresh();
            infoControl.Enabled = false;
            itemBox.Enabled = true;
            saveToDev.Enabled = true;
            DisablePanels();
        }

    }
}

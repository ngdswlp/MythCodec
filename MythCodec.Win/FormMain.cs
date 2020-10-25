using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
//using Newtonsoft.Json;
using MythCodec.Lib;
using System.Web.Script.Serialization;

namespace MythCodec.Win
{
    public partial class FormMain : Form
    {
        private Dictionary<string, string> keys=new Dictionary<string, string>();
        private MythMan mm;
        const string ConfigFile = "config.json";
        public FormMain()
        {
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            labCapacity.Visible = false;
        }
        private void SaveConfig()
        {
            //File.WriteAllText(ConfigFile, JsonConvert.SerializeObject(keys,Formatting.Indented));
            File.WriteAllText(ConfigFile,new JavaScriptSerializer().Serialize(keys));
            
        }
        private void AddKey(string name,string key)
        {
            keys[name] = key;
            cmbKeys.DataSource = new BindingSource(keys, null);
            cmbKeys.SelectedIndex = keys.Count - 1;
            SaveConfig();
        }
        private void DelKey(string name)
        {
            keys.Remove(name);
            cmbKeys.DataSource = new BindingSource(keys, null);
        }
        private void FormMain_Load(object sender, EventArgs e)
        {
            try
            {
                //keys = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(ConfigFile));
                keys = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(File.ReadAllText(ConfigFile));
            }
            catch
            {

            }
            finally
            {
                if (keys.Count < 1)
                    keys["谜语人"] = Model.DefaultString;
                cmbKeys.ValueMember = "Key";
                cmbKeys.DisplayMember = "Key";
                cmbKeys.DataSource = new BindingSource(keys, null);
            }
        }
        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveConfig();
        }
        private void Encode_Click(object sender, EventArgs e)
        {
            try
            {
                txt.Text = mm.Surprise(txt.Text);
            }catch(Exception _)
            {
                MessageBox.Show(_.Message, "错误", MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        private void Decode_Click(object sender, EventArgs e)
        {
            try
            {
                txt.Text = mm.Translate(txt.Text);
            }catch(Exception _)
            {
                MessageBox.Show(_.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Add_Click(object sender, EventArgs e)
        {
            if (InputBox.Show("请输入谜钥名称", "新建谜钥", out var value) == DialogResult.OK)
            {
                if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(txt.Text))
                {
                    if (!keys.ContainsKey(value))
                        AddKey(value, txt.Text);
                    else
                    {
                        if (MessageBox.Show("重复的谜钥名,是否覆盖？", "提示", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            AddKey(value, txt.Text);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("名称和谜钥不能为空","注意",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                }
            }
        }

        private void Del_Click(object sender, EventArgs e)
        {
            if(cmbKeys.Items.Count>1)
                DelKey(cmbKeys.Text);
        }

        private void Keys_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                mm = new MythMan(keys[cmbKeys.Text]);
                labCapacity.Text = mm.BitLength + "b";
            }
            catch
            {
                MessageBox.Show("谜钥解析错误","错误",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
            mm.Extended = !chkMode.Checked;
        }

        private void Mode_CheckedChanged(object sender, EventArgs e)
        {
            mm.Extended = !chkMode.Checked;
            labCapacity.Visible = chkMode.Checked;
        }

        private void Show_Click(object sender, EventArgs e)
        {
            txt.Text = keys[cmbKeys.Text];
        }
    }
}

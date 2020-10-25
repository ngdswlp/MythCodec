using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MythCodec.Win
{
    public partial class InputBox : Form
    {
        public static DialogResult Show(string caption,string title,out string value)
        {
            var inputBox = new InputBox();
            inputBox.Text = title;
            inputBox.labCaption.Text = caption;
            var result = inputBox.ShowDialog();
            value = inputBox.textBox.Text;
            return result;
        }
        public InputBox()
        {
            InitializeComponent();
        }
    }
}

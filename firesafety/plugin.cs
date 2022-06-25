using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace firesafety
{
    public partial class plugin : Form
    {
        public plugin()
        {
            InitializeComponent();
        }

        public void setValue(string titleMessage, string resultMessage)
        {
            label2.Text = titleMessage;
            label3.Text = resultMessage;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}

using ClassLibrary1;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormInterfaceForLas2Witsml
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Witsml witsmlFile;
        Las lasFile;
        Uom uom;
        StreamReader inputStream;
        StreamWriter outputStream;

        private void openLASfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openLasFileDialog.ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openUomFileDialog.ShowDialog();
        }

        private void openLasFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            var pathToLasFile = openLasFileDialog.FileName;
            inputStream = new StreamReader(pathToLasFile);
            lasFile = new Las(inputStream);
            label2.Text = pathToLasFile;
            button3.Enabled = true;
        }

        private void openUomFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            var pathToUomFile = openUomFileDialog.FileName;
            try
            {
                uom = new Uom(pathToUomFile);
                label1.Text = pathToUomFile;
                button2.Enabled = true;
                openLASfileToolStripMenuItem.Enabled = true;
            }
            catch(Exception ee)
            {
                MessageBox.Show("Error!");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openLasFileDialog.ShowDialog();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            saveWitsmlFileDialog.ShowDialog();
        }

        private void saveWitsmlFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            var pathSaveWitsmlFile = saveWitsmlFileDialog.FileName;
            outputStream = new StreamWriter(pathSaveWitsmlFile);
            label3.Text = pathSaveWitsmlFile;
            int version = Convert.ToInt32(textBoxVersionWitsml.Text);
            witsmlFile = new Witsml(outputStream, version, uom);
            string uidWell = textBox1.Text;
            string uidWellbore = textBox2.Text;
            string uid = textBox3.Text;
            string name = textBox4.Text;
            saveWitsmlFileDialog.Dispose();

            try
            {
                lasFile.Process();
                witsmlFile.FromLasFile(lasFile, uidWell, uidWellbore, uid, name);
                outputStream.Close();
                inputStream.Close();
                MessageBox.Show("Complete!");
            }
            catch(Exception ee)
            {
                MessageBox.Show("Error!");
            }
        }

        private void openUomfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openUomFileDialog.ShowDialog();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

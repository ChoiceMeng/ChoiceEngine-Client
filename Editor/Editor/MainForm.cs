using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Editor
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        Viewport viewport;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            initViewport();
        }

        private void initViewport()
        {
            viewport = new Viewport();
            try
            {
                viewport.Start(panelViewport.Handle);
            }
            catch(Exception e)
            {
                int a = 1;
            }
        }

        private void MainForm_Closed(object sender, FormClosedEventArgs e)
        {
            viewport.Stop();
        }
    }
}

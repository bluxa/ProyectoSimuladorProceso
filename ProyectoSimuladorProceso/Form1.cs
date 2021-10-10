using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProyectoSimuladorProceso
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void picCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void picMinimizar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void cmbTipoProceso_SelectedIndexChanged(object sender, EventArgs e)
        {
            int indice = cmbTipoProceso.SelectedIndex;

            switch (indice)
            {
                case 0:
                    txtQuantum.Text = "1 minuto";
                    txtMemoria.Text = "500 Kb";
                    txtCpu.Text = "10%";
                    break;
                case 1:
                    txtQuantum.Text = "45 segundos";
                    txtMemoria.Text = "10 mb";
                    txtCpu.Text = "20%";
                    break;
                case 2:
                    txtQuantum.Text = "30 segundos";
                    txtMemoria.Text = "50 mb";
                    txtCpu.Text = "30%";
                    break;
                case 3:
                    txtQuantum.Text = "2 minutos";
                    txtMemoria.Text = "1 gb";
                    txtCpu.Text = "70%";
                    break;
                case 4:
                    txtQuantum.Text = "30 segundos";
                    txtMemoria.Text = "256 mb";
                    txtCpu.Text = "50%";
                    break;
                default:
                    break;
            }
        }
    }
}

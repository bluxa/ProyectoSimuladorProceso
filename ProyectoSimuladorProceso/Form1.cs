using ProyectoSimuladorProceso.Proceso;
using ProyectoSimuladorProceso.Cola;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace ProyectoSimuladorProceso
{
    public partial class Form1 : Form
    {
        private static Mutex mut = new Mutex();
        delegate void delegado(object valor);
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
                    txtQuantum.Text = "60";
                    txtMemoria.Text = "0.0005";
                    txtCpu.Text = "10%";
                    break;
                case 1:
                    txtQuantum.Text = "45";
                    txtMemoria.Text = "0.01";
                    txtCpu.Text = "20%";
                    break;
                case 2:
                    txtQuantum.Text = "30";
                    txtMemoria.Text = "0.05";
                    txtCpu.Text = "30%";
                    break;
                case 3:
                    txtQuantum.Text = "120";
                    txtMemoria.Text = "1";
                    txtCpu.Text = "70%";
                    break;
                case 4:
                    txtQuantum.Text = "30";
                    txtMemoria.Text = "0.256";
                    txtCpu.Text = "50%";
                    break;
                case 5:
                    txtQuantum.Text = "120";
                    txtMemoria.Text = "2";
                    txtCpu.Text = "85%";
                    break;
                default:
                    break;
            }
        } 

        //Insertarlo a una cola
        Cola.Cola miColaProceso = new Cola.Cola();
        Cola.Cola readyCola = new Cola.Cola();

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            ClsProceso nuevoProceso = new ClsProceso(1,cmbTipoProceso.SelectedItem.ToString(),
            Convert.ToInt32(txtQuantum.Text),float.Parse(txtMemoria.Text),8);
    
            miColaProceso.Push(nuevoProceso);

            string[] auxProceso = nuevoProceso.ToString().Split(';');
            dgvColaProceso.Rows.Add(auxProceso);
  
        }

        public void CrearHilo()
        {
            int valor = miColaProceso.NumeroElementos;
         
            for (int i = 0; i < valor; i++)
            {
                if (dgvColaProceso.RowCount > 0)
                {
                    MessageBox.Show("Eliminando " + dgvColaProceso.CurrentRow.Index);
                    dgvColaProceso.Rows.RemoveAt(dgvColaProceso.CurrentRow.Index);
                    
                }

                ClsProceso item;
                item = (ClsProceso)miColaProceso.Pop();

                string[] auxProceso = item.ToString().Split(';');
                dgvNew.Rows.Add(auxProceso);

                new Thread(metodo).Start(item);
            }
        }
        public void metodo(object item)
        {
            mut.WaitOne();
            
            Thread.Sleep(2000);
            
            readyCola.Push(item);

            delegado MD = new delegado(Actualizar1);
            this.Invoke(MD, new object[] { item });     
            
            mut.ReleaseMutex(); 
        }

        public void Actualizar1(object item)
        {
            string[] subs = item.ToString().Split(';');
            dataGridView3.Rows.Add(subs);

            if (dgvNew.RowCount > 0)
            {
                //MessageBox.Show("Eliminando " + dataGridView2.CurrentRow.Index);
                //dataGridView2.Rows.RemoveAt(dataGridView2.CurrentRow.Index);

                string[] auxProceso = item.ToString().Split(';');

                foreach (DataGridViewRow Row in dgvNew.Rows)
                {
                    String strFila = Row.Index.ToString();
                    string Valor = Convert.ToString(Row.Cells["dataGridViewTextBoxColumn2"].Value);

                    if (Valor == auxProceso[1])
                    {
                        //dataGridView2.Rows[Convert.ToInt32(strFila)].DefaultCellStyle.BackColor = Color.Green;
                        //MessageBox.Show("COLOREANDO " + strFila);
                        dgvNew.Rows.RemoveAt(Convert.ToInt32(strFila));
                    }
                }

            }
        }

        private void btnAuxiliar_Click(object sender, EventArgs e)
        {
            CrearHilo();
        }

    }
}
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
using ProyectoSimuladorProceso.Archivo;
using System.Windows.Forms.DataVisualization.Charting;

namespace ProyectoSimuladorProceso
{
    public partial class Form1 : Form
    {
        private static Mutex mutexReady = new Mutex();
        private static Mutex mutexRunning = new Mutex();
        private static Mutex mutexWaiting = new Mutex();

        delegate void delegado(object valor);
        delegate void delegadoaux(object valor,int i);

        delegate void delegadoWait(object valor);

        Log ArchivoLogFinalizado = new Log(@"ProcesosTerminados");

        //Insertarlo a una cola
        Cola.Cola miColaProceso = new Cola.Cola();
        Cola.Cola readyCola = new Cola.Cola();
        Cola.Cola runningCola = new Cola.Cola();
        Cola.Cola finalizadoCola = new Cola.Cola();
        Cola.Cola waitingCola = new Cola.Cola();


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
        int idProceso = 1;

        private void cmbTipoProceso_SelectedIndexChanged(object sender, EventArgs e)
        {
            int indice = cmbTipoProceso.SelectedIndex;

            switch (indice)
            {
                case 0:
                    txtQuantum.Text = "60";
                    txtMemoria.Text = "0.0005";
                    txtCpu.Text = "10";
                    break;
                case 1:
                    txtQuantum.Text = "45";
                    txtMemoria.Text = "0.01";
                    txtCpu.Text = "20";
                    break;
                case 2:
                    txtQuantum.Text = "30";
                    txtMemoria.Text = "0.05";
                    txtCpu.Text = "30";
                    break;
                case 3:
                    txtQuantum.Text = "120";
                    txtMemoria.Text = "1";
                    txtCpu.Text = "70";
                    break;
                case 4:
                    txtQuantum.Text = "30";
                    txtMemoria.Text = "0.256";
                    txtCpu.Text = "50";
                    break;
                case 5:
                    txtQuantum.Text = "120";
                    txtMemoria.Text = "2";
                    txtCpu.Text = "85";
                    break;
                default:
                    break;
            }
        } 

        

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            ClsProceso nuevoProceso = new ClsProceso(idProceso++,cmbTipoProceso.SelectedItem.ToString(),
            Convert.ToInt32(txtQuantum.Text),float.Parse(txtMemoria.Text), Convert.ToInt32(txtCpu.Text));
    
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
                    //MessageBox.Show(" "+ dgvColaProceso.CurrentRow.Index);
                    dgvColaProceso.Rows.RemoveAt(dgvColaProceso.CurrentRow.Index);
                    
                }

                ClsProceso item;
                item = (ClsProceso)miColaProceso.Pop();

                string[] auxProceso = item.ToString().Split(';');
                dgvNew.Rows.Add(auxProceso);

                new Thread(metodo).Start(item);
            }  // running
        }

        public void metodo(object item)
        {
            if (item != null)
            {
                mutexReady.WaitOne();

                readyCola.Push(item);  

                delegado MD = new delegado(ActualizarDgvReady);
                this.Invoke(MD, new object[] { item });

                //1500 = 15 segundos tiempo de espera para pasar al estado de READY
                Thread.Sleep(1500);

                mutexReady.ReleaseMutex();

                runningHilo();
            }
           
        }

        public void ActualizarDgvReady(object item)
        {
            string[] subs = item.ToString().Split(';');
            dgvReady.Rows.Add(subs);

            //HISTORIAL DE READY
            //Nodo indice;

            //for (indice = readyCola.Primero; indice != null; indice = indice.Siguiente)
            //{
            //    lstColaReady.Items.Add(indice.Dato.ToString());
            //}

            if (dgvNew.RowCount > 0)
            {
     
                string[] auxProceso = item.ToString().Split(';');

                foreach (DataGridViewRow Row in dgvNew.Rows)
                {
                    String strFila = Row.Index.ToString();
                    string Valor = Convert.ToString(Row.Cells["dataGridViewTextBoxColumn2"].Value);

                    if (Valor == auxProceso[1])
                    {
                        dgvNew.Rows.RemoveAt(Convert.ToInt32(strFila));
                    }
                }

            }
        }

        private void btnAuxiliar_Click(object sender, EventArgs e)
        {
            CrearHilo();       
        }

        public void runningHilo() 
        {
            mutexRunning.WaitOne();

            ClsProceso item;
            item = (ClsProceso)readyCola.Pop();

            //Obtiene el tiempo asignado
            int valor = int.Parse(obtenerDatoProceso(item, 2));


            //  Si el tiempo es Quantum < 1 minuto pasan a finaliza el hilo
            if (valor <= 60)
            {
                runningCola.Push(item);

                delegadoaux MD2 = new delegadoaux(ActualizarDgvs);
                this.Invoke(MD2, new object[] { item, 0 });
                string nombre = obtenerDatoProceso(item, 1);
                var result = MessageBox.Show("Desea interumpir el proceso  "+ nombre, "Mensaje", MessageBoxButtons.YesNo);

                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    MessageBox.Show("Se interumpe  " + nombre);
                    //readyCola.Push();
                   
                    delegadoaux MD3 = new delegadoaux(ActualizarDgvs);
                    this.Invoke(MD3, new object[] { item, 3 });
                  
                    metodo(runningCola.Pop());
                }
                else
                {
                    finalizadoCola.Push(runningCola.Pop());
                    delegadoaux M = new delegadoaux(ActualizarDgvs);
                    this.Invoke(M, new object[] { item, 1 });
                    Thread.Sleep(valor * 100);
                }
                //Proceso finalizad

                mutexRunning.ReleaseMutex();
               // runningHilo();

            }

            // Si el tiempo es Quantum > 1 minuto pasan a Waiting 
            else
            {
                runningCola.Push(item);

                Thread.Sleep((valor/2)*100);
                //MessageBox.Show("Valor Sleep:  "+ valor / 2);

                delegadoaux MD2 = new delegadoaux(ActualizarDgvs);
                this.Invoke(MD2, new object[] { item,0});

                //Actulizar a la espera
                ClsProceso nuevo = new ClsProceso();
                nuevo.tiempoQuantum(item,valor/2);

                runningCola.Pop();
                waitingCola.Push(item);

                delegadoaux MD1 = new delegadoaux(ActualizarDgvs);
                this.Invoke(MD1, new object[] { item, 2 });

                // MessageBox.Show("" + item.ToString()); 
                mutexRunning.ReleaseMutex();


            }

                
            if (waitingCola.NumeroElementos>0)
            {
                waitingHilo();
            }
         
        }

        public void waitingHilo() 
        {
            mutexWaiting.WaitOne();

            object auxItem = waitingCola.Pop();

            //Thread.Sleep(3000);
            delegadoWait MD1 = new delegadoWait(ActualizarTablaWaiting);
            Thread.Sleep(3000);
            this.Invoke(MD1, new object[] { auxItem });

            metodo(auxItem);

            //ELIMINAR DE LA TABLA WAITING
       
            mutexWaiting.ReleaseMutex();
        }

        public void ActualizarTablaWaiting(object item)
        {
            if (dvgWaiting.RowCount > 0 && item!=null)
            {

                string[] auxProceso = item.ToString().Split(';');

                foreach (DataGridViewRow Row in dvgWaiting.Rows)
                {
                    String strFila = Row.Index.ToString();
                    string Valor = Convert.ToString(Row.Cells["dataGridViewTextBoxColumn17"].Value);

                    if (Valor == auxProceso[1])
                    {
                        dvgWaiting.Rows.RemoveAt(Convert.ToInt32(strFila));
                    }
                }
            }
        }

        public string obtenerDatoProceso(object item, int i)
        {
            string[] subs = item.ToString().Split(';');
            return subs[i];
        }
        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyy/MM/dd HH:mm:ss.ffff");
        }
        public void ActualizarDgvs(object item,int opcion)
        {
            string[] subs = item.ToString().Split(';');


            if (opcion == 1)
            {
                dgvFinalizado.Rows.Add(subs);

             
                ArchivoLogFinalizado.Agregar("→ " + GetTimestamp(DateTime.Now)+  "  "+ item.ToString());

                if (dgvRunning.RowCount > 0)
                {

                    string[] auxProceso = item.ToString().Split(';');

                    foreach (DataGridViewRow Row in dgvRunning.Rows)
                    {
                        String strFila = Row.Index.ToString();
                        string Valor = Convert.ToString(Row.Cells["dataGridViewTextBoxColumn12"].Value);

                        if (Valor == auxProceso[1])
                        {
                            dgvRunning.Rows.RemoveAt(Convert.ToInt32(strFila));
                        }
                    }
                }
            }

            else if (opcion == 2)
            {
                dvgWaiting.Rows.Add(subs);

                if (dgvRunning.RowCount > 0)
                {

                    string[] auxProceso = item.ToString().Split(';');

                    foreach (DataGridViewRow Row in dgvRunning.Rows)
                    {
                        String strFila = Row.Index.ToString();
                        string Valor = Convert.ToString(Row.Cells["dataGridViewTextBoxColumn12"].Value);

                        if (Valor == auxProceso[1])
                        {
                            dgvRunning.Rows.RemoveAt(Convert.ToInt32(strFila));
                        }
                    }
                }
            }

            else if (opcion == 3)
            {
                if (dgvRunning.RowCount > 0)
                {

                    string[] auxProceso = item.ToString().Split(';');

                    foreach (DataGridViewRow Row in dgvRunning.Rows)
                    {
                        String strFila = Row.Index.ToString();
                        string Valor = Convert.ToString(Row.Cells["dataGridViewTextBoxColumn12"].Value);

                        if (Valor == auxProceso[1])
                        {
                            dgvRunning.Rows.RemoveAt(Convert.ToInt32(strFila));
                        }
                    }
                }
            }


            else if (opcion == 0)
            {
                dgvRunning.Rows.Add(subs);

                if (dgvReady.RowCount > 0)
                {

                    string[] auxProceso = item.ToString().Split(';');

                    foreach (DataGridViewRow Row in dgvReady.Rows)
                    {
                        String strFila = Row.Index.ToString();
                        string Valor = Convert.ToString(Row.Cells["dataGridViewTextBoxColumn7"].Value);

                        if (Valor == auxProceso[1])
                        {
                            dgvReady.Rows.RemoveAt(Convert.ToInt32(strFila));
                        }
                    }
                }

            }


        }

        string[] seriesCpu = { "CPU", "CPU en uso"};
        string[] seriesMemoria = { "MEMORIA", "memoria en uso" };

        

       

        private void dgvRunning_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
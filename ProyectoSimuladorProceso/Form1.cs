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
        private static Mutex mut = new Mutex();

        private static Mutex mut1 = new Mutex();
        private static Mutex mut2 = new Mutex();

        delegate void delegado(object valor);
        delegate void delegadoaux(object valor,int i);

        delegate void delegadoWait(object valor);

        Log ArchivoLogFinalizado = new Log(@"ProcesosTerminados");

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

        //Insertarlo a una cola
        Cola.Cola miColaProceso = new Cola.Cola();
        Cola.Cola readyCola = new Cola.Cola();
        Cola.Cola runningCola = new Cola.Cola();
        Cola.Cola finalizadoCola = new Cola.Cola();
        Cola.Cola waitingCola = new Cola.Cola();
        

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
                    MessageBox.Show("Moviendo a estado NEW " ,""+ dgvColaProceso.CurrentRow.Index, MessageBoxButtons.OK,MessageBoxIcon.Information);
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
                mut.WaitOne();

                Thread.Sleep(4000);


                readyCola.Push(item);  // 15) impresa   15) navegador 15) java

                delegado MD = new delegado(Actualizar1);
                this.Invoke(MD, new object[] { item });
           

                mut.ReleaseMutex();

                runningHilo();
            }
           
        }

        public void Actualizar1(object item)
        {
            string[] subs = item.ToString().Split(';');
            dvgReady.Rows.Add(subs);

            //HISTORIAL DE READY
            Nodo indice;

            for (indice = readyCola.Primero; indice != null; indice = indice.Siguiente)
            {
                lstColaReady.Items.Add(indice.Dato.ToString());
            }

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
            mut1.WaitOne();

            ClsProceso item;
            item = (ClsProceso)readyCola.Pop();

            //Obtiene el tiempo asignado
            int valor = int.Parse(obtenerDatoProceso(item, 2));


            //  Si el tiempo es Quantum < 1 minuto pasan a finaliza el hilo
            if (valor <= 60)
            {
                runningCola.Push(item);


                Thread.Sleep(valor*100);

                delegadoaux MD2 = new delegadoaux(Actualizar2);
                this.Invoke(MD2, new object[] { item,0 });

                //Proceso finalizado

                finalizadoCola.Push(runningCola.Pop());

                delegadoaux M = new delegadoaux(Actualizar2);
                this.Invoke(M, new object[] { item,1 });

                mut1.ReleaseMutex();

            }

            // Si el tiempo es Quantum > 1 minuto pasan a Waiting 
            else
            {
                runningCola.Push(item);

                Thread.Sleep((valor/2)*100);
                //MessageBox.Show("Valor Sleep:  "+ valor / 2);

                delegadoaux MD2 = new delegadoaux(Actualizar2);
                this.Invoke(MD2, new object[] { item,0});

                //Actulizar a la espera
                ClsProceso nuevo = new ClsProceso();
                nuevo.tiempoQuantum(item,valor/2);

                runningCola.Pop();
                waitingCola.Push(item);

                delegadoaux MD1 = new delegadoaux(Actualizar2);
                this.Invoke(MD1, new object[] { item, 2 });

                // MessageBox.Show("" + item.ToString()); 
                mut1.ReleaseMutex();


            }

                
            if (waitingCola.NumeroElementos>0)
            {
                waitingHilo();
            }
         
        }

        public void waitingHilo() 
        {
            mut2.WaitOne();

            object auxItem = waitingCola.Pop();

            //Thread.Sleep(3000);
            delegadoWait MD1 = new delegadoWait(ActualizarTablaWaiting);
            Thread.Sleep(3000);
            this.Invoke(MD1, new object[] { auxItem });

            

            metodo(auxItem);

            //ELIMINAR DE LA TABLA WAITING
            


            mut2.ReleaseMutex();
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

        public void Actualizar2(object item,int opcion)
        {
            string[] subs = item.ToString().Split(';');


            if (opcion == 1)
            {
                dgvFinalizado.Rows.Add(subs);

             
                ArchivoLogFinalizado.Agregar("→" + item.ToString());

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

            else if (opcion == 0)
            {
                dgvRunning.Rows.Add(subs);

                chart1.Series.Clear();
                chart1.Titles.Clear();

                chart2.Series.Clear();
                //chart2.Titles.Clear();

                
                graficaCpu(Convert.ToInt32(obtenerDatoProceso(item, 4)));

                graficaMemoria(float.Parse(obtenerDatoProceso(item, 3)));

                if (dvgReady.RowCount > 0)
                {

                    string[] auxProceso = item.ToString().Split(';');

                    foreach (DataGridViewRow Row in dvgReady.Rows)
                    {
                        String strFila = Row.Index.ToString();
                        string Valor = Convert.ToString(Row.Cells["dataGridViewTextBoxColumn7"].Value);

                        if (Valor == auxProceso[1])
                        {
                            dvgReady.Rows.RemoveAt(Convert.ToInt32(strFila));
                        }
                    }
                }

            }


        }

        string[] seriesCpu = { "CPU", "CPU en uso"};
        string[] seriesMemoria = { "MEMORIA", "memoria en uso" };

        public void graficaMemoria(float memoria)
        {
            float[] puntos = { 4, memoria };

            chart2.Palette = ChartColorPalette.Pastel;
            chart2.Titles.Add("Porcentaje Memoria " + memoria);


            for (int i = 0; i < seriesMemoria.Length; i++)
            {
                Series serie = chart2.Series.Add(seriesMemoria[i]);
                serie.ChartType = SeriesChartType.Pie;

                //serie.Label = puntos[i].ToString();
                //.Label = "Memoria Consumida "+memoria;

                //serie.Points.Add(puntos[i]);

                serie.Points.AddXY("Memoria utilizado ",memoria);
                serie.Points.AddXY("Memoria total 4", 4);

            }
        }

        public void graficaCpu(int porcentaCPU)
        {
            int[] puntos = { 100, porcentaCPU };

            chart1.Palette = ChartColorPalette.Pastel;
            chart1.Titles.Add("Porcentaje CPU "+porcentaCPU);

            for (int i = 0; i < seriesCpu.Length; i++)
            {
                Series serie = chart1.Series.Add(seriesCpu[i]);

                serie.Label = puntos[i].ToString();
                serie.Points.Add(puntos[i]);
            }
        }

        private void dgvRunning_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
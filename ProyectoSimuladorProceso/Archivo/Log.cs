using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoSimuladorProceso.Archivo
{
    public class Log
    {
        private string Direccion = "";

        public Log(string direccion)
        {
            Direccion = direccion;
        }

        public void Agregar(string sLog)
        {
            CrearDirectorio();
            string nombre = ObtenerNombreArchivo();
            string cadena = "";

            //cadena = DateTime.Now + "-" + sLog + Environment.NewLine;
            cadena = "-" + sLog + Environment.NewLine;

            StreamWriter sw = new StreamWriter(Direccion + "/" + nombre, true);
            sw.Write(cadena);
            sw.Close();
        }

        public void Vaciar()
        {
            CrearDirectorio();
            string nombre = ObtenerNombreArchivo();

            System.IO.File.WriteAllText(Direccion + "/" + nombre, string.Empty);
        }

        private string ObtenerNombreArchivo()
        {
            string nombre = "";

            nombre = "log " + DateTime.Now.Day + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + ".txt";

            return nombre;
        }

        private void CrearDirectorio()
        {
            try
            {
                if (!Directory.Exists(Direccion))
                    Directory.CreateDirectory(Direccion);
            }
            catch (DirectoryNotFoundException e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoSimuladorProceso.Proceso
{
    public class ClsProceso
    {
        int idProceso;
        string tipoProceso;
        int quantumProceso;
        float memoriaProceso;
        int cpuProceso;

        public ClsProceso(int idProceso, string tipoProceso, int quantumProceso, float memoriaProceso, int cpuProceso)
        {
            this.idProceso = idProceso;
            this.tipoProceso = tipoProceso;
            this.quantumProceso = quantumProceso;
            this.memoriaProceso = memoriaProceso;
            this.cpuProceso = cpuProceso;
        }

        public ClsProceso()
        {
        }


        public override string ToString()
        {
            return idProceso+";"+tipoProceso+";"+quantumProceso+";"+memoriaProceso+";"+cpuProceso;
            //return "Type Process " + tipoProceso + "  Quantum " + quantumProceso + " Memoria"+memoriaProceso + "  Uso del CPU"+cpuProceso;
        }
    }
}

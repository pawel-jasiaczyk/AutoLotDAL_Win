using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace AutoLotDisconnectedLayer
{
    class InventoryDALDisLayer
    {
        // pola danych
        private string cnString = string.Empty;
        private SqlDataAdapter dAdapt = null;

        public InventoryDALDisLayer(string connectionString)
        {
            cnString = connectionString;

            // skonfiguruj SqlDataAdapter
            ConfigureAdapter(out dAdapt);
        }

        private void ConfigureAdapter(out SqlDataAdapter dAdapt)
        {
            // tworzymy adapter i konfigurujemy SelectCommand
            dAdapt = new SqlDataAdapter("Select * From Inventory", cnString);

            // uzyskujemy pozostałe obiekty poleceń dynamicznie, w czasie wykonywania programu
            // za pomocą SqlCommandBuilder
            SqlCommandBuilder builder = new SqlCommandBuilder(dAdapt);
        }

        public DataTable GetAllInventoru()
        {
            DataTable inv = new DataTable("Inventory");
            dAdapt.Fill(inv);
            return inv;
        }

        public void UpdateInventory(DataTable modifiedTable)
        {
            dAdapt.Update(modifiedTable);
        }
    }
}

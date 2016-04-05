using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// tylko dla SQL Server - taki przykład
using System.Data;
using System.Data.SqlClient;

namespace AutoLotConnectedLayer
{
    public class InventoryDAL
    {
        // ta składowa będzie używana przez wszystkie metody
        private SqlConnection sqlCn = null;

        public void OpenConnection(string connectionString)
        {
            sqlCn = new SqlConnection();
            sqlCn.ConnectionString = connectionString;
            sqlCn.Open();
        }

        public void CloseConnection()
        {
            sqlCn.Close();
        }

        //public void InsertAuto(int id, string color, string make, string petName)
        //{
        //    // sformatuj i wykonaj instrukcję SQL
        //    string sql = string.Format("Insert Into Inventory" +
        //        "(CarID, Make, Color, PetName) Values" +
        //        "('{0}', '{1}', '{2}', '{3}')", id, make, color, petName);

        //    // wykonaj za pomocą naszego połączenia
        //    using (SqlCommand cmd = new SqlCommand(sql, this.sqlCn))
        //    {
        //        cmd.ExecuteNonQuery();
        //    }
        //}

        public void InsertAuto(int id, string color, string make, string petName)
        {
            // zwróć uwagę na "wypełniacze" w zapytaniu SQL
            string sql = string.Format("Insert Into Inventory" +
                "(CarID, Make, Color, PetName) Values" +
                "(@CarID, @Make, @Color,@PetName)");

            // to polecenie będzie miało parametry wewnętrzne
            using (SqlCommand cmd = new SqlCommand(sql, this.sqlCn))
            {
                SqlParameter param = new SqlParameter();
                param.ParameterName = "@CarID";
                param.Value = id;
                param.SqlDbType = SqlDbType.Int;
                cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@Make";
                param.Value = make;
                param.SqlDbType = SqlDbType.Char;
                param.Size = 10;
                cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@Color";
                param.Value = color;
                param.SqlDbType = SqlDbType.Char;
                param.Size = 10;
                cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@PetName";
                param.Value = petName;
                param.SqlDbType = SqlDbType.Char;
                param.Size = 10;
                cmd.Parameters.Add(param);

                cmd.ExecuteNonQuery();
            }
        }

        public void InsertAuto(NewCar car)
        {
            // sformatuj i wykonaj instrukcję SQL
            string sql = string.Format("Insert Into Inventory" +
                "(CarID, Make, Color, PetName) Values" +
                "('{0}', '{1}', '{2}', '{3}')", car.CarID, car.Make, car.Color, car.PetName);

            // wykonaj za pomocą naszego połączenia
            using (SqlCommand cmd = new SqlCommand(sql, sqlCn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteCar(int id)
        {
            string sql = string.Format("Delete from Inventory where CarID = '{0}'", id);
            using (SqlCommand cmd = new SqlCommand(sql, this.sqlCn))
            {
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    Exception error = new Exception("Sorry! That car is on order!", ex);
                    throw error;
                }
            }
        }

        public void UpdateCarPetName(int id, string newPetName)
        {
            // uzyskaj ID samochodu do modyfikacji i nową nazwę
            string sql = string.Format("Update Inventory Set PetName = '{0}' Where CarID = '{1}'",
                newPetName, id);
            using (SqlCommand cmd = new SqlCommand(sql, sqlCn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public List<NewCar> GetAllInventoryAsList()
        {
            // w tym obiekcie będą przechowywane rekordy
            List<NewCar> inv = new List<NewCar>();

            // przygotuj obiekt polecenia
            string sql = "Select * From Inventory";
            using (SqlCommand cmd = new SqlCommand(sql, this.sqlCn))
            {
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    inv.Add(new NewCar
                    {
                        CarID = (int)dr["CarID"],
                        Color = (string)dr["Color"],
                        Make = (string)dr["Make"],
                        PetName = (string)dr["PetName"]
                    });
                }
                dr.Close();
            }
            return inv;
        }

        public DataTable GetAllInventoryAsDataTable()
        {
            // w tym obiekcie będą przechowywane rekordy
            DataTable inv = new DataTable();

            // przygotuj obiekt polecenia
            string sql = "Select * From Inventory";
            using (SqlCommand cmd = new SqlCommand(sql, sqlCn))
            {
                SqlDataReader dr = cmd.ExecuteReader();
                // wypełnij obiekt DataTable danymi z czytnika i posprządaj
                inv.Load(dr);
                dr.Close();
            }
            return inv;
        }

        public string LookUpPetName(int carID)
        {
            string carPetName = string.Empty;
            
            // ustatalamy nazwę procedury składowanej
            using (SqlCommand cmd = new SqlCommand("GetPetName", this.sqlCn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // parametry wejściowe
                SqlParameter param = new SqlParameter();
                param.ParameterName = "@carID";
                param.SqlDbType = SqlDbType.Int;
                param.Value = carID;

                // domyślny kierunek to wejście, ale  dla jasności
                param.Direction = ParameterDirection.Input;
                cmd.Parameters.Add(param);

                // parametry wyjściowe
                param = new SqlParameter();
                param.ParameterName = "@petName";
                param.SqlDbType = SqlDbType.Char;
                param.Size = 10;
                param.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(param);

                // wykonaj procedurę składowaną
                cmd.ExecuteNonQuery();

                // zwróć parametr wyjściowy
                carPetName = (string)cmd.Parameters["@petName"].Value;
            }
            return carPetName;
        }

        public void ProcessCreditRisk(bool throwEx, int custID)
        {
            // najpierw wyszukjemy bieżącą nazwę na podstawie ID klienta
            string fName = string.Empty;
            string lName = string.Empty;
            SqlCommand cmdSelect = new SqlCommand(
                string.Format("Select * from Customers where CustID = {0}", custID), 
                sqlCn);

            using (SqlDataReader dr = cmdSelect.ExecuteReader())
            {
                if (dr.HasRows)
                {
                    dr.Read();
                    fName = (string)dr["FirstName"];
                    lName = (string)dr["LastName"];
                }
                else
                    return;
            }

            // tworzymy obiekty polecenia reprezentujące każdy etap operacji
            SqlCommand cmdRemove = new SqlCommand(
                String.Format("Delete from Customers where CustID = {0}",custID),
                sqlCn);
            SqlCommand cmdInsert = new SqlCommand(
                String.Format("Insert Into CreditRisks"+
                "(CustID, FirstName, LastName) Values"+
                "('{0}', '{1}', '{2}')", custID, fName, lName), sqlCn);

            // te informacje uzyskamy z obiektu połączenia
            SqlTransaction tx = null;
            try
            {
                tx = sqlCn.BeginTransaction();

                // rejestrujemy polecenia do tej transakcji
                cmdInsert.Transaction = tx;
                cmdRemove.Transaction = tx;

                // wykonujemy polecenia
                cmdInsert.ExecuteNonQuery();
                cmdRemove.ExecuteNonQuery();

                // pozorujemy błąd
                if(throwEx)
                {
                    throw new Exception("Sorry! Database Error!, Tx failed...");
                }

                // zatwierdzamy
                tx.Commit();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                // każdy błąd spowoduje wycofanie transakcji
                tx.Rollback();
            }
        }
    }

    public class NewCar
    {
        public int CarID { get; set; }
        public string Color { get; set; }
        public string Make { get; set; }
        public string PetName { get; set; }
    }
}

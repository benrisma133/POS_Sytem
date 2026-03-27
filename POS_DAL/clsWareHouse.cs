using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;

namespace POS_DAL
{
    public static class clsWareHouseData
    {

        // ============================
        // ADD NEW WAREHOUSE
        // ============================
        public static int AddNew(string name, string location, string description, string color)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                INSERT INTO Warehouses (Name, Location, Description, Color)
                VALUES (@Name, @Location, @Description, @Color);
                SELECT last_insert_rowid();
            ";

                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@Location", location);
                    command.Parameters.AddWithValue("@Description", description);
                    command.Parameters.AddWithValue("@Color", color);

                    object result = command.ExecuteScalar();
                    return result == null ? -1 : Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in AddNew Warehouse: " + ex.Message);
            }
        }


        // ============================
        // UPDATE WAREHOUSE
        // ============================
        public static bool Update(int warehouseID, string name, string location, string description, string color)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                UPDATE Warehouses
                SET Name = @Name,
                    Location = @Location,
                    Description = @Description,
                    Color = @Color
                WHERE WarehouseID = @WarehouseID
            ";

                    command.Parameters.AddWithValue("@WarehouseID", warehouseID);
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@Location", location);
                    command.Parameters.AddWithValue("@Description", description);
                    command.Parameters.AddWithValue("@Color", color);

                    int rows = command.ExecuteNonQuery();
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in Update Warehouse: " + ex.Message);
            }
        }


        // ============================
        // DELETE WAREHOUSE
        // ============================
        public static bool Delete(int warehouseID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                DELETE FROM Warehouses
                WHERE WarehouseID = @WarehouseID
            ";

                    command.Parameters.AddWithValue("@WarehouseID", warehouseID);

                    int rows = command.ExecuteNonQuery();
                    return rows > 0;
                }
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19) // FK constraint
            {
                // Warehouse is linked to other tables
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }


        // ============================
        // GET BY ID
        // ============================
        public static bool GetByID(int warehouseID ,ref string name ,ref string location ,ref string description ,ref string color)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                SELECT Name, Location, Description, Color
                FROM Warehouses
                WHERE WarehouseID = @WarehouseID
            ";

                    command.Parameters.AddWithValue("@WarehouseID", warehouseID);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            name = reader["Name"].ToString();
                            location = reader["Location"].ToString();
                            description = reader["Description"].ToString();
                            color = reader["Color"].ToString();
                            return true;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetByID Warehouse: " + ex.Message);
            }
        }


        // ============================
        // GET BY NAME
        // ============================
        public static bool GetByName(string name ,ref int warehouseID ,ref string location ,ref string description ,ref string color)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                SELECT WarehouseID, Location, Description, Color
                FROM Warehouses
                WHERE Name = @Name
            ";

                    command.Parameters.AddWithValue("@Name", name);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            warehouseID = Convert.ToInt32(reader["WarehouseID"]);
                            location = reader["Location"].ToString();
                            description = reader["Description"].ToString();
                            color = reader["Color"].ToString();
                            return true;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetByName Warehouse: " + ex.Message);
            }
        }


        // ============================
        // GET ALL
        // ============================
        public static DataTable GetAll()
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                SELECT WarehouseID, Name, Location, Description, Color
                FROM Warehouses
                ORDER BY WarehouseID
            ";

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        return dt;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetAll Warehouses: " + ex.Message);
            }
        }

        // ============================
        // GET ALL WAREHOUSES IDS
        // ============================
        public static List<int> GetAllWarehouseIDs()
        {
            List<int> warehouseIDs = new List<int>();

            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT WarehouseID FROM Warehouses";

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            warehouseIDs.Add(reader.GetInt32(0));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetAllWarehouseIDs: " + ex.Message);
            }

            return warehouseIDs;
        }

        public static bool IsWarehouseExistByName(string name)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT 1
                        FROM Warehouses
                        WHERE Name = @Name COLLATE NOCASE 
                        LIMIT 1;
                    ";

                    command.Parameters.AddWithValue("@Name", name);

                    object result = command.ExecuteScalar();
                    return result != null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in IsModelExistByName: " + ex.Message);
            }
        }

        public static bool IsWarehouseExistByName(string name, int ignoreWarehouseID)
        {
            using (var connection = DbHelper.OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT 1
                    FROM Warehouses
                    WHERE Name = @Name COLLATE NOCASE
                      AND WarehouseID <> @WarehouseID
                    LIMIT 1;
                ";

                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@WarehouseID", ignoreWarehouseID);

                return command.ExecuteScalar() != null;
            }
        }

        public static bool IsWarehouseExistByLocation(string location)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT 1
                        FROM Warehouses
                        WHERE Location = @Location COLLATE NOCASE 
                        LIMIT 1;
                    ";

                    command.Parameters.AddWithValue("@Location", location);

                    object result = command.ExecuteScalar();
                    return result != null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in IsModelExistByName: " + ex.Message);
            }
        }

        public static bool IsWarehouseExistByLocation(string location, int ignoreWarehouseID)
        {
            using (var connection = DbHelper.OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT 1
                    FROM Warehouses
                    WHERE Location = @Location COLLATE NOCASE
                      AND WarehouseID <> @WarehouseID
                    LIMIT 1;
                ";

                command.Parameters.AddWithValue("@Location", location);
                command.Parameters.AddWithValue("@WarehouseID", ignoreWarehouseID);

                return command.ExecuteScalar() != null;
            }
        }

    }
}

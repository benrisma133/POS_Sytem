using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;

namespace POS_DAL
{
    public static class clsBrandsData
    {
        // ============================
        // ADD NEW BRAND
        // ============================
        public static int AddNew(string name, string description)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        INSERT INTO Brands (Name, Description)
                        VALUES (@Name, @Description);
                        SELECT last_insert_rowid();
                    ";
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@Description", description ?? "");

                    object result = command.ExecuteScalar();
                    return result == null ? -1 : Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in AddNew Brand: " + ex.Message);
            }
        }

        // ============================
        // UPDATE BRAND
        // ============================
        public static bool Update(int brandID, string name, string description)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE Brands
                        SET Name = @Name,
                            Description = @Description
                        WHERE BrandID = @BrandID
                    ";
                    command.Parameters.AddWithValue("@BrandID", brandID);
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@Description", description ?? "");

                    int rows = command.ExecuteNonQuery();
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in Update Brand: " + ex.Message);
            }
        }

        // ============================
        // DELETE BRAND
        // ============================
        public static bool Delete(int brandID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        DELETE FROM Brands
                        WHERE BrandID = @BrandID
                    ";
                    command.Parameters.AddWithValue("@BrandID", brandID);

                    int rows = command.ExecuteNonQuery();
                    return rows > 0;
                }
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                // Brand is linked to Series or Models
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // ============================
        // GET BRAND BY ID
        // ============================
        public static bool GetByID(int brandID, ref string name, ref string description)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT Name, Description
                        FROM Brands
                        WHERE BrandID = @BrandID
                    ";
                    command.Parameters.AddWithValue("@BrandID", brandID);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            name = reader["Name"].ToString();
                            description = reader["Description"].ToString();
                            return true;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetByID Brand: " + ex.Message);
            }
        }

        // ============================
        // GET BRAND BY NAME
        // ============================
        public static bool GetByName(string name, ref int brandID, ref string description)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT BrandID, Description
                        FROM Brands
                        WHERE Name = @Name
                        LIMIT 1
                    ";
                    command.Parameters.AddWithValue("@Name", name);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            brandID = Convert.ToInt32(reader["BrandID"]);
                            description = reader["Description"].ToString();
                            return true;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetByName Brand: " + ex.Message);
            }
        }

        // ============================
        // GET ALL BRANDS
        // ============================
        public static DataTable GetAll()
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT BrandID, Name, Description
                        FROM Brands
                        ORDER BY BrandID
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
                throw new Exception("Error in GetAll Brands: " + ex.Message);
            }
        }

        // ============================
        // CHECK IF BRAND EXISTS BY NAME
        // ============================
        public static bool IsBrandExistByName(string name, int ignoreBrandID = -1)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT 1
                        FROM Brands
                        WHERE Name = @Name COLLATE NOCASE
                    ";
                    command.Parameters.AddWithValue("@Name", name);

                    if (ignoreBrandID > 0)
                    {
                        command.CommandText += " AND BrandID <> @BrandID";
                        command.Parameters.AddWithValue("@BrandID", ignoreBrandID);
                    }

                    command.CommandText += " LIMIT 1";

                    object result = command.ExecuteScalar();
                    return result != null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in IsBrandExistByName: " + ex.Message);
            }
        }

        // ============================
        // GET ALL BRAND IDS
        // ============================
        public static List<int> GetAllBrandIDs()
        {
            List<int> brandIDs = new List<int>();

            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT BrandID FROM Brands";

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            brandIDs.Add(reader.GetInt32(0));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetAllBrandIDs: " + ex.Message);
            }

            return brandIDs;
        }

        // ============================
        // GET ALL BRANDs BY WAREHOUSE ID
        // ============================
        public static DataTable GetByWarehouseID(int warehouseID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                {
                    using (SqliteCommand command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                    SELECT 
                        b.BrandID,
                        b.Name,
                        b.Description
                    FROM Brands b
                    INNER JOIN WarehouseBrands wb
                        ON b.BrandID = wb.BrandID
                    WHERE wb.WarehouseID = @WarehouseID;
                ";

                        command.Parameters.AddWithValue("@WarehouseID", warehouseID);

                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            DataTable dt = new DataTable();

                            // Add columns manually
                            dt.Columns.Add("BrandID", typeof(int));
                            dt.Columns.Add("Name", typeof(string));
                            dt.Columns.Add("Description", typeof(string));

                            while (reader.Read())
                            {
                                dt.Rows.Add(
                                    reader.GetInt32(0),
                                    reader.IsDBNull(1) ? null : reader.GetString(1),
                                    reader.IsDBNull(2) ? null : reader.GetString(2)
                                );
                            }

                            return dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetByWarehouseID Brands: " + ex.Message);
            }
        }
    }
}
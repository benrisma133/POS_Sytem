using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace POS_DAL
{
    public static class clsSeriesData
    {
        // ============================
        // ADD NEW SERIES
        // ============================
        public static int AddNew(int brandID, string name, string description)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        INSERT INTO Series (BrandID, Name, Description)
                        VALUES (@BrandID, @Name, @Description);
                        SELECT last_insert_rowid();
                    ";
                    command.Parameters.AddWithValue("@BrandID", brandID);
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@Description", description ?? "");

                    object result = command.ExecuteScalar();
                    return result == null ? -1 : Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in AddNew Series: " + ex.Message);
            }
        }

        // ============================
        // UPDATE SERIES
        // ============================
        public static bool Update(int seriesID, int brandID, string name, string description)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE Series
                        SET BrandID = @BrandID,
                            Name = @Name,
                            Description = @Description
                        WHERE SeriesID = @SeriesID
                    ";
                    command.Parameters.AddWithValue("@SeriesID", seriesID);
                    command.Parameters.AddWithValue("@BrandID", brandID);
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@Description", description ?? "");

                    int rows = command.ExecuteNonQuery();
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in Update Series: " + ex.Message);
            }
        }

        // ============================
        // DELETE SERIES
        // ============================
        public static bool Delete(int seriesID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        DELETE FROM Series
                        WHERE SeriesID = @SeriesID
                    ";
                    command.Parameters.AddWithValue("@SeriesID", seriesID);

                    int rows = command.ExecuteNonQuery();
                    return rows > 0;
                }
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                // Series is linked to Models
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // ============================
        // GET SERIES BY ID
        // ============================
        public static bool GetByID(int seriesID, ref int brandID, ref string name, ref string description)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT BrandID, Name, Description
                        FROM Series
                        WHERE SeriesID = @SeriesID
                    ";
                    command.Parameters.AddWithValue("@SeriesID", seriesID);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            brandID = Convert.ToInt32(reader["BrandID"]);
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
                throw new Exception("Error in GetByID Series: " + ex.Message);
            }
        }

        // ============================
        // GET SERIES BY NAME
        // ============================
        public static bool GetByName(string name, ref int seriesID, ref int brandID, ref string description)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT SeriesID, BrandID, Description
                        FROM Series
                        WHERE Name = @Name
                        LIMIT 1
                    ";
                    command.Parameters.AddWithValue("@Name", name);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            seriesID = Convert.ToInt32(reader["SeriesID"]);
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
                throw new Exception("Error in GetByName Series: " + ex.Message);
            }
        }

        // ============================
        // GET ALL SERIES
        // ============================
        public static DataTable GetAll()
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                                            SELECT 
                                                s.SeriesID,
                                                s.BrandID,
                                                b.Name AS BrandName,
                                                s.Name,
                                                s.Description
                                            FROM Series s
                                            INNER JOIN Brands b ON s.BrandID = b.BrandID
                                            ORDER BY s.SeriesID
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
                throw new Exception("Error in GetAll Series: " + ex.Message);
            }
        }

        public static DataTable GetByBrandID(int brandID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                SELECT 
                    s.SeriesID,
                    s.BrandID,
                    b.Name AS BrandName,
                    s.Name,
                    s.Description
                FROM Series s
                INNER JOIN Brands b ON s.BrandID = b.BrandID
                WHERE s.BrandID = @BrandID
                ORDER BY s.SeriesID;
            ";

                    command.Parameters.AddWithValue("@BrandID", brandID);

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
                throw new Exception("Error in Get Series By BrandID: " + ex.Message);
            }
        }

        // ============================
        // CHECK IF SERIES EXISTS BY NAME
        // ============================
        public static bool IsSeriesExistByName(string name, int ignoreSeriesID = -1)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT 1
                        FROM Series
                        WHERE Name = @Name COLLATE NOCASE
                    ";

                    command.Parameters.AddWithValue("@Name", name);

                    if (ignoreSeriesID > 0)
                    {
                        command.CommandText += " AND SeriesID <> @SeriesID";
                        command.Parameters.AddWithValue("@SeriesID", ignoreSeriesID);
                    }

                    command.CommandText += " LIMIT 1";

                    object result = command.ExecuteScalar();
                    return result != null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in IsSeriesExistByName: " + ex.Message);
            }
        }

        // ============================
        // GET ALL SERIES IDS
        // ============================
        public static List<int> GetAllSeriesIDs()
        {
            List<int> seriesIDs = new List<int>();

            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT SeriesID FROM Series";

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            seriesIDs.Add(reader.GetInt32(0));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetAllSeriesIDs: " + ex.Message);
            }

            return seriesIDs;
        }


        // ============================
        // GET ALL SERIES BY WAREHOUSE ID
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
                                                    s.SeriesID,
                                                    s.BrandID,
                                                    s.Name,
                                                    b.Name AS BrandName,
                                                    s.Description
                                                FROM Series s
                                                INNER JOIN Brands b ON s.BrandID = b.BrandID
                                                INNER JOIN WarehouseSeries ws ON s.SeriesID = ws.SeriesID
                                                WHERE ws.WarehouseID = @WarehouseID
                                                ORDER BY s.SeriesID
                                            ";

                        command.Parameters.AddWithValue("@WarehouseID", warehouseID);

                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            return dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetByWarehouseID Series: " + ex.Message);
            }
        }

        // ============================
        // ADD SERIES TO WAREHOUSES
        // ============================
        public static void AddSeriesToWarehouses(int seriesID, List<int> warehouseIDs)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                {
                    foreach (int warehouseID in warehouseIDs)
                    {
                        using (SqliteCommand command = connection.CreateCommand())
                        {
                            command.CommandText = @"
                                                        INSERT INTO WarehouseSeries (WarehouseID, SeriesID)
                                                        VALUES (@WarehouseID, @SeriesID);
                                                    ";

                            command.Parameters.AddWithValue("@WarehouseID", warehouseID);
                            command.Parameters.AddWithValue("@SeriesID", seriesID);

                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in AddSeriesToWarehouses: " + ex.Message);
            }
        }

        // ============================
        // ADD SERIES TO ALL WAREHOUSES
        // ============================
        public static void AddSeriesToAllWarehouses(int seriesID)
        {
            try
            {
                List<int> warehouseIDs = clsWareHouseData.GetAllWarehouseIDs();
                AddSeriesToWarehouses(seriesID, warehouseIDs);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in AddSeriesToAllWarehouses: " + ex.Message);
            }
        }

        // ============================
        // UPDATE SERIES WAREHOUSES (DELETE OLD AND ADD NEW)
        // ============================
        public static void UpdateSeriesWarehouses(int seriesID, List<int> warehouseIDs)
        {
            using (var connection = new SqliteConnection(clsDataAccessSettigs.ConnectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Delete old relations
                        using (var deleteCmd = connection.CreateCommand())
                        {
                            deleteCmd.Transaction = transaction;
                            deleteCmd.CommandText = @"
                                                        DELETE FROM WarehouseSeries
                                                        WHERE SeriesID = @SeriesID;
                                                    ";
                            deleteCmd.Parameters.AddWithValue("@SeriesID", seriesID);
                            deleteCmd.ExecuteNonQuery();
                        }

                        // Insert new (ignore duplicates)
                        foreach (int warehouseID in warehouseIDs.Distinct())
                        {
                            using (var insertCmd = connection.CreateCommand())
                            {
                                insertCmd.Transaction = transaction;
                                insertCmd.CommandText = @"
                            INSERT OR IGNORE INTO WarehouseSeries (WarehouseID, SeriesID)
                            VALUES (@WarehouseID, @SeriesID);
                        ";
                                insertCmd.Parameters.AddWithValue("@WarehouseID", warehouseID);
                                insertCmd.Parameters.AddWithValue("@SeriesID", seriesID);
                                insertCmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Error updating Series warehouses: " + ex.Message, ex);
                    }
                }
            }
        }

        // ============================
        // UPDATE SERIES TO ALL WAREHOUSES
        // ============================
        public static void UpdateSeriesToAllWarehouses(int seriesID)
        {
            try
            {
                List<int> warehouseIDs = clsWareHouseData.GetAllWarehouseIDs();
                UpdateSeriesWarehouses(seriesID, warehouseIDs);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in UpdateSeriesToAllWarehouses: " + ex.Message);
            }
        }

        // ============================
        // GET WAREHOUSE IDs BY SERIES ID
        // ============================
        public static List<int> GetWarehouseIDsBySeriesID(int seriesID)
        {
            List<int> ids = new List<int>();

            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                                                SELECT WarehouseID
                                                FROM WarehouseSeries
                                                WHERE SeriesID = @SeriesID;
                                            ";

                    command.Parameters.AddWithValue("@SeriesID", seriesID);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ids.Add(Convert.ToInt32(reader["WarehouseID"]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetWarehouseIDsBySeriesID: " + ex.Message);
            }

            return ids;
        }

    }
}
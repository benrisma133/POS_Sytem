using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace POS_DAL
{
    public static class clsCategoryData
    {

        private static ILogger _logger;

        // Called ONCE from UI
        public static void InitLogger(ILoggerFactory factory)
        {
            _logger = factory.CreateLogger("DAL.clsCategoryData");
        }

        // ============================
        // ADD NEW CATEGORY
        // ============================
        public static int AddNew(string name, string description, int? iconID = null)
        {
            try
            {


                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        INSERT INTO Categories (Name, Description, IconID)
                        VALUES (@Name, @Description, @IconID);
                        SELECT last_insert_rowid();
                    ";

                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue(
                        "@Description",
                        string.IsNullOrEmpty(description) ? (object)DBNull.Value : description
                    );
                    command.Parameters.AddWithValue("@IconID", iconID.HasValue ? (object)iconID.Value : DBNull.Value);

                    object result = command.ExecuteScalar();
                    return result == null ? -1 : Convert.ToInt32((long)result);
                }
            }
            catch (Exception ex)
            {
                string msg = $"AddNew failed for Category '{name}': {ex.Message}";
                _logger?.LogError(ex, msg); // Logs exception + stack trace
                Debug.WriteLine(msg);        // Optional: also output in VS Output window
                return -1;
            }
        }

        // ============================
        // ADD CATEGORY TO WAREHOUSE
        // ============================
        public static bool AddCategoryToWarehouse(int categoryID, int warehouseID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                INSERT INTO WarehouseCategories (WarehouseID, CategoryID)
                VALUES (@WarehouseID, @CategoryID);
            ";

                    command.Parameters.AddWithValue("@WarehouseID", warehouseID);
                    command.Parameters.AddWithValue("@CategoryID", categoryID);

                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in AddCategoryToWarehouse: " + ex.Message);
            }
        }

        // ======================================
        // ADD CATEGORY TO MULTIPLE WAREHOUSES
        // ======================================
        public static void AddCategoryToWarehouses(int categoryID, List<int> warehouseIDs)
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
                        INSERT INTO WarehouseCategories (WarehouseID, CategoryID)
                        VALUES (@WarehouseID, @CategoryID);
                    ";

                            command.Parameters.AddWithValue("@WarehouseID", warehouseID);
                            command.Parameters.AddWithValue("@CategoryID", categoryID);

                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in AddCategoryToWarehouses: " + ex.Message);
            }
        }

        // ======================================
        // ADD CATEGORY TO ALL WAREHOUSES
        // ======================================
        public static void AddCategoryToAllWarehouses(int categoryID)
        {
            try
            {
                List<int> warehouseIDs = clsWareHouseData.GetAllWarehouseIDs();

                AddCategoryToWarehouses(categoryID, warehouseIDs);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in AddCategoryToAllWarehouses: " + ex.Message);
            }
        }

        // ============================
        // UPDATE CATEGORY
        // ============================
        public static bool Update(int categoryID, string name, string description, int? iconID = null)
        {
            using (var connection = DbHelper.OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    UPDATE Categories
                    SET Name = @Name,
                        Description = @Description,
                        IconID = @IconID
                    WHERE CategoryID = @CategoryID;
                ";

                // ✅ Parameters
                command.Parameters.AddWithValue("@CategoryID", categoryID);
                command.Parameters.AddWithValue("@Name", string.IsNullOrEmpty(name) ? (object)DBNull.Value : name);
                command.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description);
                command.Parameters.AddWithValue("@IconID", iconID.HasValue ? (object)iconID.Value : DBNull.Value);


                int rows = command.ExecuteNonQuery();
                return rows > 0;
            }
        }



        // ======================================
        // UPDATE CATEGORY WAREHOUSES
        // ======================================
        // ======================================
        // SAFE UPDATE CATEGORY WAREHOUSES
        // ======================================
        public static void UpdateCategoryWarehouses(int categoryID, List<int> warehouseIDs)
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
                        DELETE FROM WarehouseCategories
                        WHERE CategoryID = @CategoryID;
                    ";
                            deleteCmd.Parameters.AddWithValue("@CategoryID", categoryID);
                            deleteCmd.ExecuteNonQuery();
                        }

                        // Insert new, ignoring duplicates automatically
                        foreach (int warehouseID in warehouseIDs.Distinct())
                        {
                            using (var insertCmd = connection.CreateCommand())
                            {
                                insertCmd.Transaction = transaction;
                                insertCmd.CommandText = @"
                            INSERT OR IGNORE INTO WarehouseCategories (WarehouseID, CategoryID)
                            VALUES (@WarehouseID, @CategoryID);
                        ";
                                insertCmd.Parameters.AddWithValue("@WarehouseID", warehouseID);
                                insertCmd.Parameters.AddWithValue("@CategoryID", categoryID);
                                insertCmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Error updating Category warehouses: " + ex.Message, ex);
                    }
                }
            }
        }

        // ======================================
        // UPDATE CATEGORY TO ALL WAREHOUSES
        // ======================================
        public static void UpdateCategoryToAllWarehouses(int categoryID)
        {
            try
            {
                List<int> warehouseIDs = clsWareHouseData.GetAllWarehouseIDs();
                UpdateCategoryWarehouses(categoryID, warehouseIDs);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in UpdateCategoryToAllWarehouses: " + ex.Message);
            }
        }


        // ============================
        // DELETE CATEGORY
        // ============================
        public static bool Delete(int categoryID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        DELETE FROM Categories
                        WHERE CategoryID = @CategoryID;
                    ";

                    command.Parameters.AddWithValue("@CategoryID", categoryID);

                    int rows = command.ExecuteNonQuery();
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in Delete Category: " + ex.Message);
            }
        }

        // ======================================
        // DELETE CATEGORY FROM ALL WAREHOUSES
        // ======================================
        public static bool DeleteCategoryFromAllWarehouses(int categoryID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                DELETE FROM WarehouseCategories
                WHERE CategoryID = @CategoryID;
            ";

                    command.Parameters.AddWithValue("@CategoryID", categoryID);
                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in DeleteCategoryFromAllWarehouses: " + ex.Message);
            }
        }

        // ======================================
        // DELETE CATEGORY FROM ONE WAREHOUSE
        // ======================================
        public static bool DeleteCategoryFromWarehouse(int categoryID, int warehouseID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                DELETE FROM WarehouseCategories
                WHERE CategoryID = @CategoryID
                  AND WarehouseID = @WarehouseID;
            ";

                    command.Parameters.AddWithValue("@CategoryID", categoryID);
                    command.Parameters.AddWithValue("@WarehouseID", warehouseID);

                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in DeleteCategoryFromWarehouse: " + ex.Message);
            }
        }

        // ======================================
        // DELETE CATEGORY COMPLETELY (SAFE)
        // ======================================
        public static bool DeleteCategoryCompletely(int categoryID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (var transaction = connection.BeginTransaction())
                {
                    // delete relations
                    using (SqliteCommand cmd1 = connection.CreateCommand())
                    {
                        cmd1.CommandText = "DELETE FROM WarehouseCategories WHERE CategoryID=@CategoryID;";
                        cmd1.Parameters.AddWithValue("@CategoryID", categoryID);
                        cmd1.ExecuteNonQuery();
                    }

                    // delete category
                    using (SqliteCommand cmd2 = connection.CreateCommand())
                    {
                        cmd2.CommandText = "DELETE FROM Categories WHERE CategoryID=@CategoryID;";
                        cmd2.Parameters.AddWithValue("@CategoryID", categoryID);
                        int rows = cmd2.ExecuteNonQuery();

                        transaction.Commit();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in DeleteCategoryCompletely: " + ex.Message);
            }
        }


        // ============================
        // GET CATEGORY BY ID
        // ============================
        public static bool GetByID(int categoryID, ref string name, ref string description, ref int? iconID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                SELECT Name, Description, IconID
                FROM Categories
                WHERE CategoryID = @CategoryID;
            ";

                    command.Parameters.AddWithValue("@CategoryID", categoryID);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            name = reader["Name"].ToString();
                            description = reader["Description"] == DBNull.Value ? null : reader["Description"].ToString();
                            iconID = reader["IconID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["IconID"]);
                            return true;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetByID Category: " + ex.Message);
            }
        }

        // ============================
        // GET CATEGORY BY NAME
        // ============================
        public static bool GetByName(string name, ref int categoryID, ref string description, ref int? iconID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                SELECT CategoryID, Description, IconID
                FROM Categories
                WHERE Name = @Name
                LIMIT 1;
            ";

                    command.Parameters.AddWithValue("@Name", name);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            categoryID = Convert.ToInt32(reader["CategoryID"]);
                            description = reader["Description"] == DBNull.Value ? null : reader["Description"].ToString();
                            iconID = reader["IconID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["IconID"]);
                            return true;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetByName Category: " + ex.Message);
            }
        }


        // ============================
        // GET ALL CATEGORIES
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
                            c.CategoryID,
                            c.Name,
                            c.Description,
                            IFNULL(i.IconData, X'') AS IconData
                        FROM Categories c
                        LEFT JOIN CategoryIcons i ON c.IconID = i.IconID;

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
                throw new Exception("Error in GetAll Categories: " + ex.Message);
            }
        }

        public static List<int> GetWarehouseIDsByCategoryID(int categoryID)
        {
            List<int> ids = new List<int>();

            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT WarehouseID
                        FROM WarehouseCategories
                        WHERE CategoryID = @CategoryID;
                    ";

                    command.Parameters.AddWithValue("@CategoryID", categoryID);

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
                throw new Exception("Error in GetWarehouseIDsByCategoryID: " + ex.Message);
            }

            return ids;
        }

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
                                c.CategoryID,
                                c.Name,
                                c.Description,
                                i.IconData
                            FROM Categories c
                            INNER JOIN WarehouseCategories wc
                                ON c.CategoryID = wc.CategoryID
                            LEFT JOIN CategoryIcons i
                                ON c.IconID = i.IconID
                            WHERE wc.WarehouseID = @WarehouseID;
                        ";

                        command.Parameters.AddWithValue("@WarehouseID", warehouseID);

                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            DataTable dt = new DataTable();

                            // Add columns manually to avoid constraints issues
                            dt.Columns.Add("CategoryID", typeof(int));
                            dt.Columns.Add("Name", typeof(string));
                            dt.Columns.Add("Description", typeof(string));
                            dt.Columns.Add("IconData", typeof(string));

                            while (reader.Read())
                            {
                                dt.Rows.Add(
                                    reader.GetInt32(0),
                                    reader.IsDBNull(1) ? null : reader.GetString(1),
                                    reader.IsDBNull(2) ? null : reader.GetString(2),
                                    reader.IsDBNull(3) ? null : reader.GetString(3)
                                );
                            }

                            return dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetByWarehouseID Categories: " + ex.Message);
            }
        }

        public static DataTable GetAllDistinct()
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                SELECT DISTINCT c.CategoryID, c.Name, c.Description
                FROM Categories c
                INNER JOIN WarehouseCategories wc
                    ON c.CategoryID = wc.CategoryID;
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
                throw new Exception("Error in GetAllDistinct Categories: " + ex.Message);
            }
        }

        // ============================
        // CHECK IF CATEGORY EXISTS BY NAME
        // ============================
        public static bool IsCategoryExistByName(string name)
        {
            using (var connection = DbHelper.OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT 1
                    FROM Categories
                    WHERE Name = @Name COLLATE NOCASE
                    LIMIT 1;
                ";

                command.Parameters.AddWithValue("@Name", name.Trim());

                return command.ExecuteScalar() != null;
            }
        }

        public static bool IsCategoryExistByName(string name, int ignoreCategoryID)
        {
            using (var connection = DbHelper.OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT 1
                    FROM Categories
                    WHERE Name = @Name COLLATE NOCASE
                      AND CategoryID <> @CategoryID
                    LIMIT 1;
                ";

                command.Parameters.AddWithValue("@Name", name.Trim());
                command.Parameters.AddWithValue("@CategoryID", ignoreCategoryID);

                return command.ExecuteScalar() != null;
            }
        }


    }
}

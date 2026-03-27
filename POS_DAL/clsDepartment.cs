using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_DAL
{
    static public class clsDepartment
    {

        // ============================
        // ADD NEW DEPARTMENT
        // ============================
        public static int AddNew(string name, string description, int? iconID = null)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        INSERT INTO SuperCategories (Name, Description, IconID)
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
                throw new Exception("Error in AddNew Department: " + ex.Message);
            }
        }

        // ============================
        // UPDATE DEPARTMENT
        // ============================
        public static bool Update(int departmentID, string name, string description, int? iconID = null)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE SuperCategories
                        SET Name = @Name,
                            Description = @Description,
                            IconID = @IconID
                        WHERE SuperCategoryID = @SuperCategoryID;
                    ";

                    command.Parameters.AddWithValue("@SuperCategoryID", departmentID);
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue(
                        "@Description",
                        string.IsNullOrEmpty(description) ? (object)DBNull.Value : description
                    );
                    command.Parameters.AddWithValue("@IconID", iconID.HasValue ? (object)iconID.Value : DBNull.Value);

                    int rows = command.ExecuteNonQuery();
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in Update SuperCategory: " + ex.Message);
            }
        }

        // ============================
        // DELETE DEPARTMENT
        // ============================
        public static bool Delete(int departmentID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        DELETE FROM SuperCategories
                        WHERE SuperCategoryID = @SuperCategoryID;
                    ";

                    command.Parameters.AddWithValue("@SuperCategoryID", departmentID);

                    int rows = command.ExecuteNonQuery();
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in Delete SuperCategory: " + ex.Message);
            }
        }

        // ============================
        // GET DEPARTMENT BY ID
        // ============================
        public static bool GetByID(int departmentID, ref string name, ref string description, ref int? iconID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT Name, Description, IconID
                        FROM Categories
                        WHERE SuperCategoryID = @SuperCategoryID;
                    ";

                    command.Parameters.AddWithValue("@SuperCategoryID", departmentID);

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
                throw new Exception("Error in GetByID SuperCategory: " + ex.Message);
            }
        }

        // ============================
        // GET DEPARTMENT BY NAME
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

    }
}

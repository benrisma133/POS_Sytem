using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;

namespace POS_DAL
{
    public static class clsCategoryIconData
    {
        // ============================
        // ADD NEW ICON
        // ============================
        public static int AddNew(string iconName, string iconData)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        INSERT INTO CategoryIcons (IconName, IconData)
                        VALUES (@IconName, @IconData);
                        SELECT last_insert_rowid();
                    ";

                    command.Parameters.AddWithValue("@IconName", iconName);
                    command.Parameters.AddWithValue("@IconData", iconData);

                    object result = command.ExecuteScalar();
                    return result == null ? -1 : Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in AddNew CategoryIcon: " + ex.Message);
            }
        }

        // ============================
        // UPDATE ICON
        // ============================
        public static bool Update(int iconID, string iconName, string iconData)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE CategoryIcons
                        SET IconName = @IconName,
                            IconData = @IconData
                        WHERE IconID = @IconID
                    ";

                    command.Parameters.AddWithValue("@IconID", iconID);
                    command.Parameters.AddWithValue("@IconName", iconName);
                    command.Parameters.AddWithValue("@IconData", iconData);

                    int rows = command.ExecuteNonQuery();
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in Update CategoryIcon: " + ex.Message);
            }
        }

        // ============================
        // DELETE ICON
        // ============================
        public static bool Delete(int iconID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        DELETE FROM CategoryIcons
                        WHERE IconID = @IconID
                    ";

                    command.Parameters.AddWithValue("@IconID", iconID);

                    int rows = command.ExecuteNonQuery();
                    return rows > 0;
                }
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19) // FK constraint
            {
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
        public static bool GetByID(int iconID, ref string iconName, ref string iconData)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT IconName, IconData
                        FROM CategoryIcons
                        WHERE IconID = @IconID
                    ";

                    command.Parameters.AddWithValue("@IconID", iconID);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            iconName = reader["IconName"].ToString();
                            iconData = reader["IconData"].ToString();
                            return true;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetByID CategoryIcon: " + ex.Message);
            }
        }

        // ============================
        // GET BY NAME
        // ============================
        public static bool GetByName(string iconName, ref int iconID, ref string iconData)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT IconID, IconData
                        FROM CategoryIcons
                        WHERE IconName = @IconName
                    ";

                    command.Parameters.AddWithValue("@IconName", iconName);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            iconID = Convert.ToInt32(reader["IconID"]);
                            iconData = reader["IconData"].ToString();
                            return true;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetByName CategoryIcon: " + ex.Message);
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
                        SELECT IconID, IconName, IconData
                        FROM CategoryIcons
                        ORDER BY IconID
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
                throw new Exception("Error in GetAll CategoryIcons: " + ex.Message);
            }
        }

        // ============================
        // GET ALL ICON IDS
        // ============================
        public static List<int> GetAllIconIDs()
        {
            List<int> iconIDs = new List<int>();

            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT IconID FROM CategoryIcons";

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            iconIDs.Add(reader.GetInt32(0));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetAllIconIDs: " + ex.Message);
            }

            return iconIDs;
        }


    }
}

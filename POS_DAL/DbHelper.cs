using Microsoft.Data.Sqlite;
using POS_DAL;

public static class DbHelper
{
    private static readonly string _cs = clsDataAccessSettigs.ConnectionString;

    public static SqliteConnection OpenConnection()
    {
        SqliteConnection con = new SqliteConnection(_cs);
        con.Open();

        using (SqliteCommand cmd = new SqliteCommand("PRAGMA foreign_keys = ON;", con))
        {
            cmd.ExecuteNonQuery();
        }

        return con; // FK is ON for this connection
    }
}

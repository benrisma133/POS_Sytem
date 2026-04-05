using System;
using System.Data;
using Microsoft.Data.Sqlite;

namespace POS_DAL
{
    public static class clsProductData
    {
        // ============================
        // GET PRODUCT IN WAREHOUSE
        // ============================
        public static bool GetProductInWarehouse(
            int productID, int warehouseID,
            ref int categoryID, ref int modelID, ref decimal price,
            ref string productName, ref string description,
            ref int stockID, ref int quantity)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT 
                            p.CategoryID,
                            p.ModelID,
                            p.Price,
                            p.ProductName,
                            p.Description,
                            s.StockID,
                            s.Quantity
                        FROM Products p
                        INNER JOIN Stock s ON p.ProductID = s.ProductID
                        WHERE p.ProductID = @ProductID
                          AND s.WarehouseID = @WarehouseID;
                    ";

                    command.Parameters.AddWithValue("@ProductID", productID);
                    command.Parameters.AddWithValue("@WarehouseID", warehouseID);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            categoryID = reader.GetInt32(0);
                            modelID = reader.GetInt32(1);
                            price = reader.GetDecimal(2);
                            productName = reader.GetString(3);
                            description = reader.IsDBNull(4) ? null : reader.GetString(4);
                            stockID = reader.GetInt32(5);
                            quantity = reader.GetInt32(6);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching product in warehouse: " + ex.Message);
            }

            return false;
        }

        // ============================
        // ADD NEW PRODUCT
        // ============================
        public static int AddNew(
            string productName, string description, decimal price,
            int? categoryID, int? modelID,
            int warehouseID, int quantity)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteTransaction transaction = connection.BeginTransaction())
                {
                    SqliteCommand cmdProduct = connection.CreateCommand();
                    cmdProduct.CommandText = @"
                        INSERT INTO Products (CategoryID, ModelID, Price, ProductName, Description)
                        VALUES (@CategoryID, @ModelID, @Price, @ProductName, @Description);
                        SELECT last_insert_rowid();
                    ";

                    cmdProduct.Parameters.AddWithValue("@CategoryID", categoryID ?? (object)DBNull.Value);
                    cmdProduct.Parameters.AddWithValue("@ModelID", modelID ?? (object)DBNull.Value);
                    cmdProduct.Parameters.AddWithValue("@Price", price);
                    cmdProduct.Parameters.AddWithValue("@ProductName", productName);
                    cmdProduct.Parameters.AddWithValue("@Description", description ?? (object)DBNull.Value);

                    int productID = Convert.ToInt32(cmdProduct.ExecuteScalar());

                    SqliteCommand cmdStock = connection.CreateCommand();
                    cmdStock.CommandText = @"
                        INSERT INTO Stock (ProductID, WarehouseID, Quantity, AddedDate, LastUpdate)
                        VALUES (@ProductID, @WarehouseID, @Quantity, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
                    ";

                    cmdStock.Parameters.AddWithValue("@ProductID", productID);
                    cmdStock.Parameters.AddWithValue("@WarehouseID", warehouseID);
                    cmdStock.Parameters.AddWithValue("@Quantity", quantity);
                    cmdStock.ExecuteNonQuery();

                    transaction.Commit();
                    return productID;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in AddNew Product: " + ex.Message);
            }
        }

        // ============================
        // UPDATE PRODUCT
        // ============================
        public static void Update(
            int productID, string productName, string description, decimal price,
            int? categoryID, int? modelID,
            int oldWarehouseID, int newWarehouseID, int quantity)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteTransaction transaction = connection.BeginTransaction())
                {
                    SqliteCommand cmdUpdateProduct = connection.CreateCommand();
                    cmdUpdateProduct.CommandText = @"
                        UPDATE Products
                        SET ProductName=@ProductName,
                            Description=@Description,
                            Price=@Price,
                            CategoryID=@CategoryID,
                            ModelID=@ModelID
                        WHERE ProductID=@ProductID;
                    ";

                    cmdUpdateProduct.Parameters.AddWithValue("@ProductID", productID);
                    cmdUpdateProduct.Parameters.AddWithValue("@ProductName", productName);
                    cmdUpdateProduct.Parameters.AddWithValue("@Description", description ?? (object)DBNull.Value);
                    cmdUpdateProduct.Parameters.AddWithValue("@Price", price);
                    cmdUpdateProduct.Parameters.AddWithValue("@CategoryID", categoryID ?? (object)DBNull.Value);
                    cmdUpdateProduct.Parameters.AddWithValue("@ModelID", modelID ?? (object)DBNull.Value);
                    cmdUpdateProduct.ExecuteNonQuery();

                    if (oldWarehouseID == newWarehouseID)
                    {
                        SqliteCommand cmdUpdateStock = connection.CreateCommand();
                        cmdUpdateStock.CommandText = @"
                            UPDATE Stock
                            SET Quantity=@Quantity,
                                LastUpdate=CURRENT_TIMESTAMP
                            WHERE ProductID=@ProductID
                              AND WarehouseID=@WarehouseID;
                        ";

                        cmdUpdateStock.Parameters.AddWithValue("@Quantity", quantity);
                        cmdUpdateStock.Parameters.AddWithValue("@ProductID", productID);
                        cmdUpdateStock.Parameters.AddWithValue("@WarehouseID", oldWarehouseID);
                        cmdUpdateStock.ExecuteNonQuery();
                    }
                    else
                    {
                        TransferProductInternal(connection, productID, oldWarehouseID, newWarehouseID, quantity);
                    }

                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in Update Product: " + ex.Message);
            }
        }

        // ============================
        // TRANSFER PRODUCT
        // ============================
        public static bool TransferProduct(
            int productID, int fromWarehouseID, int toWarehouseID, int quantity)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteTransaction transaction = connection.BeginTransaction())
                {
                    bool result = TransferProductInternal(
                        connection, productID, fromWarehouseID, toWarehouseID, quantity);

                    transaction.Commit();
                    return result;
                }
            }
            catch
            {
                return false;
            }
        }

        private static bool TransferProductInternal(
            SqliteConnection connection,
            int productID, int fromWarehouseID, int toWarehouseID, int quantity)
        {
            SqliteCommand cmdCheck = connection.CreateCommand();
            cmdCheck.CommandText =
                "SELECT Quantity FROM Stock WHERE ProductID=@PID AND WarehouseID=@WID;";
            cmdCheck.Parameters.AddWithValue("@PID", productID);
            cmdCheck.Parameters.AddWithValue("@WID", fromWarehouseID);

            object qtyObj = cmdCheck.ExecuteScalar();
            if (qtyObj == null || Convert.ToInt32(qtyObj) < quantity)
                return false;

            SqliteCommand cmdSub = connection.CreateCommand();
            cmdSub.CommandText = @"
                UPDATE Stock
                SET Quantity = Quantity - @Qty,
                    LastUpdate = CURRENT_TIMESTAMP
                WHERE ProductID=@PID AND WarehouseID=@WID;
            ";
            cmdSub.Parameters.AddWithValue("@Qty", quantity);
            cmdSub.Parameters.AddWithValue("@PID", productID);
            cmdSub.Parameters.AddWithValue("@WID", fromWarehouseID);
            cmdSub.ExecuteNonQuery();

            SqliteCommand cmdExists = connection.CreateCommand();
            cmdExists.CommandText =
                "SELECT COUNT(*) FROM Stock WHERE ProductID=@PID AND WarehouseID=@WID;";
            cmdExists.Parameters.AddWithValue("@PID", productID);
            cmdExists.Parameters.AddWithValue("@WID", toWarehouseID);

            long exists = (long)cmdExists.ExecuteScalar();

            SqliteCommand cmdAdd = connection.CreateCommand();
            if (exists > 0)
            {
                cmdAdd.CommandText = @"
                    UPDATE Stock
                    SET Quantity = Quantity + @Qty,
                        LastUpdate = CURRENT_TIMESTAMP
                    WHERE ProductID=@PID AND WarehouseID=@WID;
                ";
            }
            else
            {
                cmdAdd.CommandText = @"
                    INSERT INTO Stock (ProductID, WarehouseID, Quantity, AddedDate, LastUpdate)
                    VALUES (@PID, @WID, @Qty, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
                ";
            }

            cmdAdd.Parameters.AddWithValue("@PID", productID);
            cmdAdd.Parameters.AddWithValue("@WID", toWarehouseID);
            cmdAdd.Parameters.AddWithValue("@Qty", quantity);
            cmdAdd.ExecuteNonQuery();

            return true;
        }

        // ============================
        // DELETE FROM WAREHOUSE
        // ============================
        public static void DeleteFromWarehouse(int productID, int warehouseID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText =
                        "DELETE FROM Stock WHERE ProductID=@PID AND WarehouseID=@WID;";
                    command.Parameters.AddWithValue("@PID", productID);
                    command.Parameters.AddWithValue("@WID", warehouseID);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in DeleteFromWarehouse: " + ex.Message);
            }
        }

        // ============================
        // GET ALL PRODUCTS
        // ============================
        public static DataTable GetAllProductDetails()
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command =
                    new SqliteCommand("SELECT * FROM vw_ProductDetailsFull", connection))
                {
                    DataTable dt = new DataTable();
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                    return dt;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching product details: " + ex.Message);
            }
        }

        // ============================
        // DELETE PRODUCT COMPLETELY
        // ============================
        public static void DeleteCompletely(int productID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteTransaction transaction = connection.BeginTransaction())
                {
                    SqliteCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "DELETE FROM Products WHERE ProductID=@ProductID;";
                    cmd.Parameters.AddWithValue("@ProductID", productID);
                    cmd.ExecuteNonQuery();

                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in Delete Product: " + ex.Message);
            }
        }

        public static int GetQuantity(int productID, int warehouseID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                                                SELECT Quantity
                                                FROM Stock
                                                WHERE ProductID = @ProductID
                                                  AND WarehouseID = @WarehouseID;
                                            ";

                    command.Parameters.AddWithValue("@ProductID", productID);
                    command.Parameters.AddWithValue("@WarehouseID", warehouseID);

                    object result = command.ExecuteScalar();

                    if (result == null)
                        return 0; // ماكاينش record يعني stock = 0

                    return Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetQuantity: " + ex.Message);
            }
        }
    }
}

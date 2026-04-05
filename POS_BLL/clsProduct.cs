using System;
using System.Data;
using System.Security.Cryptography.X509Certificates;
using POS_DAL;

namespace POS_BLL
{
    public class clsProduct
    {
        public int ProductID { get; private set; }
        public int CategoryID { get; set; }
        public int ModelID { get; set; }
        public decimal Price { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }

        // New for stock operations
        public int WarehouseID { get;  set; }

        public int StockID { get; private set; }
        public int Quantity { get; set; }

        public clsCategory Category { get; private set; }
        public clsModel Model { get; private set; }

        public clsWareHouse Warehouse { get; private set; }
        //public clsStock Stock { get; private set; }

        private enum enMode { AddNew, Update, Transfer }
        private enMode _mode;
        private int _transferToWarehouseID; // Destination warehouse for transfer

        // ============================
        // PRIVATE CONSTRUCTOR (Find)
        // ============================
        private clsProduct(int productID,int warehouseID ,int stockID ,int quantity , int categoryID, int modelID, decimal price, string productName, string description)
        {
            ProductID = productID;
            CategoryID = categoryID;
            ModelID = modelID;
            Price = price;
            ProductName = productName;
            Description = description;
            WarehouseID = warehouseID;
            StockID = stockID;
            Quantity = quantity;

            Category = clsCategory.FindByID(categoryID);
            Model = clsModel.FindByID(modelID);
            Warehouse = clsWareHouse.FindByID(warehouseID);
            //StockID = clsStockData.FindByID(stockID);

            _mode = enMode.Update;
        }

        // ============================
        // PUBLIC CONSTRUCTOR (Add)
        // ============================
        public clsProduct()
        {
            ProductID = -1;
            CategoryID = -1;
            ModelID = -1;
            ProductName = string.Empty;
            Description = string.Empty;
            WarehouseID = -1;
            Quantity = 0;
            _mode = enMode.AddNew;
        }

        // ============================
        // SET TRANSFER MODE
        // ============================
        public void SetTransferMode(int fromWarehouseID, int toWarehouseID, int quantity)
        {
            WarehouseID = fromWarehouseID;
            _transferToWarehouseID = toWarehouseID;
            Quantity = quantity;
            _mode = enMode.Transfer;
        }

        // ============================
        // SAVE
        // ============================
        private bool _AddNew()
        {
            ProductID = clsProductData.AddNew(ProductName, Description, Price, CategoryID, ModelID, WarehouseID, Quantity);
            if (ProductID != -1)
                _mode = enMode.Update;
            return ProductID != -1;
        }

        private bool _Update()
        {
            // When not transferring, destination = same warehouse
            _transferToWarehouseID = WarehouseID;
            clsProductData.Update(ProductID, ProductName, Description, Price, CategoryID, ModelID, WarehouseID, _transferToWarehouseID, Quantity);
            return true;
        }

        public bool Transfer()
        {

            bool isTransfered = clsProductData.TransferProduct(ProductID, WarehouseID, _transferToWarehouseID, Quantity);

            if (isTransfered)
            {
                _mode = enMode.Update;
                return true;
            }

            return false;
        }

        public bool Save()
        {
            switch (_mode)
            {
                case enMode.AddNew:
                    {
                        if (_AddNew())
                        {
                            _mode = enMode.Update;
                            return true;
                        }
                        return false;
                    }
                case enMode.Update:
                    return _Update();
            }

            return false;
        }

        // ============================
        // FIND BY ID IN WAREHOUSE
        // ============================
        public static clsProduct FindByIDInWarehouse(int productID, int warehouseID)
        {
            int categoryID = -1, modelID = -1, stockID = -1, quantity = 0;
            decimal price = 0;
            string productName = string.Empty, description = string.Empty;

            bool found = clsProductData.GetProductInWarehouse(
                productID, warehouseID,
                ref categoryID, ref modelID, ref price,
                ref productName, ref description,
                ref stockID, ref quantity 
            );

            if (!found) return null;

            // Use the constructor that includes warehouse and stock
            return new clsProduct(productID, warehouseID, stockID, quantity, categoryID, modelID, price, productName, description);
        }

        // ============================
        // GET ALL & DELETE
        // ============================
        public static DataTable GetAll()
        {
            return clsProductData.GetAllProductDetails();
        }

        public static bool Delete(int id)
        {
            clsProductData.DeleteCompletely(id);
            return true;
        }

        public static int GetQuantityInWarehouse(int productID, int warehouseID)
        {
            return clsProductData.GetQuantity(productID, warehouseID);
        }
    }
}

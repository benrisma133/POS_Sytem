using POS_DAL;
using System;
using System.Collections.Generic;
using System.Data;

namespace POS_BLL
{
    public class clsCategory
    {
        enum enMode { AddNew = 1, Update = 2 }
        enMode _Mode;

        public int CategoryID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? IconID { get; set; }

        // ============================
        // Constructor for new category
        // ============================
        public clsCategory()
        {
            _Mode = enMode.AddNew;
            CategoryID = -1;
            Name = "";
            Description = "";
            IconID = -1;
        }

        // ============================
        // Private constructor for loading existing category
        // ============================
        private clsCategory(int categoryID, string name, string description ,int? iconID)
        {
            _Mode = enMode.Update;
            CategoryID = categoryID;
            Name = name;
            Description = description;
            IconID = iconID;
        }

        // ============================
        // Find by ID
        // ============================
        public static clsCategory FindByID(int categoryID)
        {
            string name = string.Empty;
            string description = string.Empty;
            int? iconID = -1;

            bool found = clsCategoryData.GetByID(categoryID, ref name, ref description ,ref iconID);
            if (!found)
                return null;

            return new clsCategory(categoryID, name, description ,iconID);
        }

        // ============================
        // Find by Name (first match)
        // ============================
        public static clsCategory FindByName(string name)
        {
            int categoryID = -1;
            string description = string.Empty;
            int? iconID = -1;

            bool found = clsCategoryData.GetByName(name, ref categoryID, ref description ,ref iconID);
            if (!found)
                return null;

            return new clsCategory(categoryID, name, description ,iconID);
        }

        // ============================
        // Add new category
        // ============================
        bool _AddNew()
        {
            this.CategoryID = clsCategoryData.AddNew(this.Name, this.Description ,this.IconID);
            return this.CategoryID != -1;
        }

        // ============================
        // Update existing category
        // ============================
        bool _Update()
        {
            return clsCategoryData.Update(this.CategoryID, this.Name, this.Description ,this.IconID);
        }

        // ============================
        // Save (AddNew or Update)
        // ============================
        public bool Save()
        {
            try
            {
                switch (_Mode)
                {
                    case enMode.AddNew:
                        if (_AddNew()) { _Mode = enMode.Update; return true; }
                        return false;

                    case enMode.Update:
                        return _Update(); // أي خطأ من DAL غادي يطلع هنا
                }
                return false;
            }
            catch (Exception ex)
            {
                // ❌ هنا نحول الخطأ لتسهيل التعامل مع المستخدم
                throw new BusinessException(
                    "تعذر حفظ أو تحديث الفئة. المرجو إعادة المحاولة.",
                    ex
                );
            }
        }


        // ============================
        // Delete category
        // ============================
        public static bool Delete(int categoryID)
        {
            return clsCategoryData.Delete(categoryID);
        }

        // ============================
        // Get all categories
        // ============================
        public static DataTable GetAll()
        {
            return clsCategoryData.GetAll();
        }

        // ====================================
        // NEW METHODS TO HANDLE WAREHOUSES
        // ====================================

        // Assign this category to a single warehouse
        public bool AssignToWarehouse(int warehouseID)
        {
            if (this.CategoryID == -1) throw new Exception("Category must be saved first.");
            return clsCategoryData.AddCategoryToWarehouse(this.CategoryID, warehouseID);
        }

        // Assign this category to multiple warehouses
        public bool AssignToWarehouses(List<int> warehouseIDs)
        {
            if (this.CategoryID == -1) throw new Exception("Category must be saved first.");
            try
            {
                clsCategoryData.AddCategoryToWarehouses(this.CategoryID, warehouseIDs);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Assign this category to all warehouses
        public bool AssignToAllWarehouses()
        {
            if (this.CategoryID == -1) throw new Exception("Category must be saved first.");
            try
            {
                clsCategoryData.AddCategoryToAllWarehouses(this.CategoryID);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateWarehouses(List<int> warehouseIDs)
        {
            if (CategoryID == -1)
                throw new Exception("Category must be saved first.");

            try
            {
                clsCategoryData.UpdateCategoryWarehouses(CategoryID, warehouseIDs);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateToAllWarehouses()
        {
            if (CategoryID == -1)
                throw new Exception("Category must be saved first.");

            try
            {
                clsCategoryData.UpdateCategoryToAllWarehouses(CategoryID);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool RemoveFromWarehouse(int warehouseID)
        {
            if (CategoryID == -1)
                throw new Exception("Category must be saved first.");

            return clsCategoryData.DeleteCategoryFromWarehouse(CategoryID, warehouseID);
        }

        static public bool RemoveFromAllWarehouses(int CategoryID)
        {
            if (CategoryID == -1)
                throw new Exception("Category must be saved first.");

            return clsCategoryData.DeleteCategoryFromAllWarehouses(CategoryID);
        }

        static public bool DeleteCompletely(int CategoryID)
        {
            if (CategoryID == -1)
                return false;

            return clsCategoryData.DeleteCategoryCompletely(CategoryID);
        }

        public List<int> GetAssignedWarehouseIDs()
        {
            if (this.CategoryID == -1)
                return new List<int>();

            return clsCategoryData.GetWarehouseIDsByCategoryID(this.CategoryID);
        }

        public static DataTable GetAllDistinct()
        {
            return clsCategoryData.GetAllDistinct();
        }

        public static DataTable GetByWarehouseID(int warehouseID)
        {
            return clsCategoryData.GetByWarehouseID(warehouseID);
        }

        public static bool IsCategoryExistByName(string name)
        {
            return clsCategoryData.IsCategoryExistByName(name);
        }

        public static bool IsCategoryExistByNameExceptID(string name, int exceptCategoryID)
        {
            return clsCategoryData.IsCategoryExistByName(name, exceptCategoryID);
        }


        

    }
}

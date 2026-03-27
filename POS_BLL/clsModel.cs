using POS_DAL;
using System;
using System.Collections.Generic;
using System.Data;

namespace POS_BLL
{
    public class clsModel
    {
        enum enMode { AddNew = 1, Update = 2 }
        enMode _Mode;

        public int ModelID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        // ============================
        // Constructor for new model
        // ============================
        public clsModel()
        {
            _Mode = enMode.AddNew;
            ModelID = -1;
            Name = "";
            Description = "";
        }

        // ============================
        // Private constructor for loading existing model
        // ============================
        private clsModel(int modelID, string name, string description)
        {
            _Mode = enMode.Update;
            ModelID = modelID;
            Name = name;
            Description = description;
        }

        // ============================
        // Find by ID
        // ============================
        public static clsModel FindByID(int modelID)
        {
            string name = "";
            string description = "";

            if (clsModelData.GetByID(modelID, ref name, ref description))
                return new clsModel(modelID, name, description);

            return null;
        }

        // ============================
        // Find by Name
        // ============================
        public static clsModel FindByName(string name)
        {
            int modelID = -1;
            string description = "";

            if (clsModelData.GetByName(name, ref modelID, ref description))
                return new clsModel(modelID, name, description);

            return null;
        }

        // ============================
        // Add new model
        // ============================
        bool _AddNew()
        {
            this.ModelID = clsModelData.AddNew(this.Name, this.Description);
            return this.ModelID != -1;
        }

        // ============================
        // Update existing model
        // ============================
        bool _Update()
        {
            return clsModelData.Update(this.ModelID, this.Name, this.Description);
        }

        // ============================
        // Save (AddNew or Update)
        // ============================
        public bool Save()
        {
            switch (_Mode)
            {
                case enMode.AddNew:
                    if (_AddNew())
                    {
                        _Mode = enMode.Update;
                        return true;
                    }
                    return false;

                case enMode.Update:
                    return _Update();
            }

            return false;
        }

        // ============================
        // Delete model
        // ============================
        public static bool Delete(int modelID)
        {
            return clsModelData.Delete(modelID);
        }

        // ============================
        // Get all models
        // ============================
        public static DataTable GetAll()
        {
            return clsModelData.GetAll();
        }

        // ============================
        // Warehouse Methods
        // ============================

        public bool AssignToWarehouse(int warehouseID)
        {
            if (this.ModelID == -1) throw new Exception("Model must be saved first.");
            return clsModelData.AddModelToWarehouse(this.ModelID, warehouseID);
        }

        public bool AssignToWarehouses(List<int> warehouseIDs)
        {
            if (this.ModelID == -1) throw new Exception("Model must be saved first.");
            try
            {
                clsModelData.AddModelToWarehouses(this.ModelID, warehouseIDs);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool AssignToAllWarehouses()
        {
            if (this.ModelID == -1) throw new Exception("Model must be saved first.");
            try
            {
                clsModelData.AddModelToAllWarehouses(this.ModelID);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateWarehouses(List<int> warehouseIDs)
        {
            if (this.ModelID == -1) throw new Exception("Model must be saved first.");
            try
            {
                clsModelData.UpdateModelWarehouses(this.ModelID, warehouseIDs);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateToAllWarehouses()
        {
            if (this.ModelID == -1) throw new Exception("Model must be saved first.");
            try
            {
                clsModelData.UpdateModelToAllWarehouses(this.ModelID);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool RemoveFromWarehouse(int warehouseID)
        {
            if (this.ModelID == -1) throw new Exception("Model must be saved first.");
            return clsModelData.DeleteModelFromWarehouse(this.ModelID, warehouseID);
        }

        public static bool RemoveFromAllWarehouses(int ModelID)
        {
            if (ModelID == -1) throw new Exception("Model must be saved first.");
            return clsModelData.DeleteModelFromAllWarehouses(ModelID);
        }
        
        public static bool DeleteCompletely(int ModelID)
        {
            if (ModelID == -1)
                return false;

            return clsModelData.DeleteModelCompletely(ModelID);
        }

        public List<int> GetAssignedWarehouseIDs()
        {
            if (this.ModelID == -1) return new List<int>();
            return clsModelData.GetWarehouseIDsByModelID(this.ModelID);
        }

        public static DataTable GetAllDistinct()
        {
            return clsModelData.GetAllDistinct();
        }

        public static DataTable GetByWarehouseID(int warehouseID)
        {
            return clsModelData.GetByWarehouseID(warehouseID);
        }

        public static bool IsModelExistsByName(string name)
        {
            return clsModelData.IsModelExistByName(name);
        }

        public static bool IsModelExistsByNameExcludingID(string name, int excludeModelID)
        {
            return clsModelData.IsModelExistByName(name, excludeModelID);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Data;
using POS_DAL;

namespace POS_BLL
{
    public class clsSeries
    {
        enum enMode { AddNew = 1, Update = 2 }
        enMode _Mode;

        public int SeriesID { get; set; }
        public string Name { get; set; }
        public int BrandID { get; set; }
        public string Description { get; set; }

        public clsBrand BrandInfo { get; set; }

        public clsSeries()
        {
            _Mode = enMode.AddNew;
            SeriesID = -1;
            Name = "";
            Description = "";
            BrandID = -1;
        }

        private clsSeries(int seriesID, string name, int brandID ,string description)
        {
            _Mode = enMode.Update;
            SeriesID = seriesID;
            Name = name;
            BrandID = brandID;
            BrandInfo = clsBrand.FindByID(brandID);
            Description = description;
        }

        public static clsSeries FindByID(int seriesID)
        {
            string name = string.Empty;
            int brandID = -1;
            string description = string.Empty;

            var dr = clsSeriesData.GetByID(seriesID, ref brandID, ref name ,ref description);
            if (!dr) return null;
            return new clsSeries(seriesID, name, brandID ,description);
        }

        public static clsSeries FindByName(string name)
        {
            int seriesID = -1;
            int brandID = -1;
            string description = string.Empty;

            var dr = clsSeriesData.GetByName(name, ref seriesID, ref brandID ,ref description);
            if (!dr) return null;
            return new clsSeries(seriesID, name, brandID ,description);
        }

        bool _AddNew()
        {
            this.SeriesID = clsSeriesData.AddNew(this.BrandID, this.Name ,this.Description);
            return this.SeriesID != -1;
        }

        bool _Update()
        {
            return clsSeriesData.Update(this.SeriesID, this.BrandID, this.Name ,this.Description);
        }

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

        public static bool Delete(int seriesID)
        {
            return clsSeriesData.Delete(seriesID);
        }

        public static DataTable GetAll()
        {
            return clsSeriesData.GetAll();
        }

        public static DataTable GetByBrandID(int brandID)
        {
            return clsSeriesData.GetByBrandID(brandID);
        }

        public static List<int> GetAllSeriesIDs()
        {
            return clsSeriesData.GetAllSeriesIDs();
        }

        public static bool IsSeriesExistByName(string name)
        {
            return clsSeriesData.IsSeriesExistByName(name);
        }

        public static bool IsSeriesExistByName(string name, int ignoreSeriesID)
        {
            return clsSeriesData.IsSeriesExistByName(name, ignoreSeriesID);
        }

        // ============================
        // GET SERIES BY WAREHOUSE
        // ============================
        public static DataTable GetByWarehouseID(int warehouseID)
        {
            return clsSeriesData.GetByWarehouseID(warehouseID);
        }

        // ============================
        // ASSIGN SERIES TO WAREHOUSES
        // ============================
        public bool AssignToWarehouses(List<int> warehouseIDs)
        {
            if (this.SeriesID == -1)
                throw new Exception("Series must be saved first.");

            try
            {
                clsSeriesData.AddSeriesToWarehouses(this.SeriesID, warehouseIDs);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ============================
        // UPDATE SERIES WAREHOUSES
        // ============================
        public bool UpdateWarehouses(List<int> warehouseIDs)
        {
            if (this.SeriesID == -1)
                throw new Exception("Series must be saved first.");

            try
            {
                clsSeriesData.UpdateSeriesWarehouses(this.SeriesID, warehouseIDs);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ============================
        // GET ASSIGNED WAREHOUSES
        // ============================
        public List<int> GetAssignedWarehouseIDs()
        {
            if (this.SeriesID == -1)
                return new List<int>();

            return clsSeriesData.GetWarehouseIDsBySeriesID(this.SeriesID);
        }
    }
}
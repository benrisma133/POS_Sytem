using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POS_DAL;

namespace POS_BLL
{
    public class clsWareHouse
    {

        enum enMode { AddNew = 1, Update = 2 }
        enMode _Mode;

        public int WareHouseID { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }

        public clsWareHouse()
        {
            _Mode = enMode.AddNew;
            WareHouseID = -1;
            Name = "";
            Location = "";
            Description = "";
            Color = "";
        }

        private clsWareHouse(int warehouseID, string name, string location ,string description ,string color)
        {
            _Mode = enMode.Update;
            WareHouseID = warehouseID;
            Name = name;
            Location = location;
            Description = description;
            Color = color;
        }

        public static clsWareHouse FindByID(int warehouseID)
        {
            string name = string.Empty;
            string location = string.Empty;
            string description = string.Empty;
            string color = string.Empty;

            var dr = clsWareHouseData.GetByID (warehouseID ,ref name ,ref location ,ref description ,ref color);
            if (dr == null)
                return null;
            return new clsWareHouse(
                warehouseID,
                name,
                location ,
                description ,
                color
            );
        }

        public static clsWareHouse FindByName(string name)
        {
            int warehouseID = -1;
            string location = string.Empty;
            string description = string.Empty;
            string color = string.Empty;

            var dr = clsWareHouseData.GetByName(name, ref warehouseID, ref location ,ref description ,ref color);
            if (dr == null)
                return null;
            return new clsWareHouse(
                warehouseID,
                name,
                location ,
                description ,
                color
            );
        }

        bool _AddNew()
        {
            this.WareHouseID = clsWareHouseData.AddNew(this.Name, this.Location ,this.Description ,this.Color);

            return this.WareHouseID != -1;
        }

        bool _Update()
        {
            return clsWareHouseData.Update(this.WareHouseID, this.Name, this.Location ,this.Description ,this.Color);
        }

        public bool Save()
        {
            switch (_Mode)
            {
                case enMode.AddNew:
                    {
                        if (_AddNew())
                        {
                            _Mode = enMode.Update;
                            return true;
                        }
                        else
                            return false;
                    }

                case enMode.Update:
                    {
                        return _Update();
                    }
            }

            return false;
        }

        public static bool Delete(int warehouseID)
        {
            return clsWareHouseData.Delete(warehouseID);
        }

        public static DataTable GetAll()
        {
            return clsWareHouseData.GetAll();
        }

        public static List<int> GetAllWarehouseIDs()
        {
            return clsWareHouseData.GetAllWarehouseIDs();
        }

        public static bool IsWarehouseExistByName(string name)
        {
            return clsWareHouseData.IsWarehouseExistByName(name);
        }

        public static bool IsWarehouseExistByName(string name, int ignoreWarehouseID)
        {
            return clsWareHouseData.IsWarehouseExistByName(name, ignoreWarehouseID);
        }

        public static bool IsWarehouseExistByLocation(string location)
        {
            return clsWareHouseData.IsWarehouseExistByLocation(location);
        }

        public static bool IsWarehouseExistByLocation(string location, int ignoreWarehouseID)
        {
            return clsWareHouseData.IsWarehouseExistByLocation(location, ignoreWarehouseID);
        }

    }
}

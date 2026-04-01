using System;
using System.Collections.Generic;
using System.Data;
using POS_DAL;

namespace POS_BLL
{
    public class clsBrand
    {
        enum enMode { AddNew = 1, Update = 2 }
        enMode _Mode;

        public int BrandID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public clsBrand()
        {
            _Mode = enMode.AddNew;
            BrandID = -1;
            Name = "";
            Description = "";
        }

        private clsBrand(int brandID, string name, string description)
        {
            _Mode = enMode.Update;
            BrandID = brandID;
            Name = name;
            Description = description;
        }

        public static clsBrand FindByID(int brandID)
        {
            string name = string.Empty;
            string description = string.Empty;

            var dr = clsBrandsData.GetByID(brandID, ref name, ref description);
            if (!dr) return null;
            return new clsBrand(brandID, name, description);
        }

        public static clsBrand FindByName(string name)
        {
            int brandID = -1;
            string description = string.Empty;

            var dr = clsBrandsData.GetByName(name, ref brandID, ref description);
            if (!dr) return null;
            return new clsBrand(brandID, name, description);
        }

        bool _AddNew()
        {
            this.BrandID = clsBrandsData.AddNew(this.Name, this.Description);
            return this.BrandID != -1;
        }

        bool _Update()
        {
            return clsBrandsData.Update(this.BrandID, this.Name, this.Description);
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

        public static bool Delete(int brandID)
        {
            return clsBrandsData.Delete(brandID);
        }

        public static DataTable GetAll()
        {
            return clsBrandsData.GetAll();
        }

        public static List<int> GetAllBrandIDs()
        {
            return clsBrandsData.GetAllBrandIDs();
        }

        public static bool IsBrandExistByName(string name)
        {
            return clsBrandsData.IsBrandExistByName(name);
        }

        public static bool IsBrandExistByName(string name, int ignoreBrandID)
        {
            return clsBrandsData.IsBrandExistByName(name, ignoreBrandID);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using POS_DAL;

namespace POS_BLL
{
    public class clsCategoryIcon
    {
        enum enMode { AddNew = 1, Update = 2 }
        enMode _Mode;

        public int IconID { get; set; }
        public string IconName { get; set; }
        public string IconData { get; set; }

        public clsCategoryIcon()
        {
            _Mode = enMode.AddNew;
            IconID = -1;
            IconName = "";
            IconData = "";
        }

        private clsCategoryIcon(int iconID, string iconName, string iconData)
        {
            _Mode = enMode.Update;
            IconID = iconID;
            IconName = iconName;
            IconData = iconData;
        }

        // ============================
        // FIND BY ID
        // ============================
        public static clsCategoryIcon FindByID(int iconID)
        {
            string iconName = string.Empty;
            string iconData = string.Empty;

            bool found = clsCategoryIconData.GetByID(iconID, ref iconName, ref iconData);

            if (!found)
                return null;

            return new clsCategoryIcon(
                iconID,
                iconName,
                iconData
            );
        }

        // ============================
        // FIND BY NAME
        // ============================
        public static clsCategoryIcon FindByName(string iconName)
        {
            int iconID = -1;
            string iconData = string.Empty;

            bool found = clsCategoryIconData.GetByName(iconName, ref iconID, ref iconData);

            if (!found)
                return null;

            return new clsCategoryIcon(
                iconID,
                iconName,
                iconData
            );
        }

        // ============================
        // ADD NEW
        // ============================
        bool _AddNew()
        {
            this.IconID = clsCategoryIconData.AddNew(this.IconName, this.IconData);
            return this.IconID != -1;
        }

        // ============================
        // UPDATE
        // ============================
        bool _Update()
        {
            return clsCategoryIconData.Update(this.IconID, this.IconName, this.IconData);
        }

        // ============================
        // SAVE
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
        // DELETE
        // ============================
        public static bool Delete(int iconID)
        {
            return clsCategoryIconData.Delete(iconID);
        }

        // ============================
        // GET ALL
        // ============================
        public static DataTable GetAll()
        {
            return clsCategoryIconData.GetAll();
        }

        // ============================
        // GET ALL ICON IDS
        // ============================
        public static List<int> GetAllIconIDs()
        {
            return clsCategoryIconData.GetAllIconIDs();
        }
    }
}

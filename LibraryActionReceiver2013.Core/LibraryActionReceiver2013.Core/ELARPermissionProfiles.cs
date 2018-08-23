using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryActionReceiver.Core
{
    public class ELARPermisssionProfiles
    {
        public static string ListUrl = "/Lists/LARPermissionProfiles";
        #region Columns
        public readonly static string ColProfileName = "Title";
        public readonly static string ColCreateFolder = "LARCreateFolder";
        public readonly static string ColEditFolder = "LAREditFolder";
        public readonly static string ColDeleteFolder = "LARDeleteFolder";
        public readonly static string ColCreateFile = "LARCreateFile";
        public readonly static string ColEditFile = "LAREditFile";
        public readonly static string ColDeleteFile = "LARDeleteFile";
        public readonly static string ColDeleteFolderWithFiles = "LARCanDeleteFolderWithFiles";

        #endregion

        #region Properties
        public int Id { get; set; }
        public string ProfileName { get; set; }
        public bool CreateFolder { get; set; }
        public bool EditFolder { get; set; }
        public bool DeleteFolder { get; set; }
        public bool DeleteFolderWithFiles { get; set; }
        public bool CreateFile { get; set; }
        public bool EditFile { get; set; }
        public bool DeleteFile { get; set; }

        #endregion

        public ELARPermisssionProfiles()
        {
        }

        public static List<ELARPermisssionProfiles> GetList(SPWeb webContext)
        {
            string listUrl = string.Format("{0}{1}", webContext.Url, ListUrl);
            List<ELARPermisssionProfiles> ls = new List<ELARPermisssionProfiles>();
            SPList list = webContext.GetList(listUrl);
            SPListItemCollection items = list.GetItems();

            foreach (SPListItem item in items)
            {
                MapObj(ls, webContext, item);
            }

            return ls;
        }

        public static ELARPermisssionProfiles GetById(SPWeb webContext, int id)
        {
            string listUrl = string.Format("{0}{1}", webContext.Url, ListUrl);
            ELARPermisssionProfiles obj = new ELARPermisssionProfiles();
            SPList list = webContext.GetList(listUrl);
            SPListItem item = list.GetItemById(id);
            MapObj(obj, webContext, item);
            return obj;
        }

        private static void MapObj(ELARPermisssionProfiles obj, SPWeb webContext, SPListItem item)
        {
            MapProperties(item, obj);
        }


        private static void MapObj(List<ELARPermisssionProfiles> ls, SPWeb rootWeb, SPListItem item)
        {
            ELARPermisssionProfiles obj = new ELARPermisssionProfiles();
            MapProperties(item, obj);
            ls.Add(obj);
        }

        private static void MapProperties(SPListItem item, ELARPermisssionProfiles obj)
        {
            obj.Id = item.ID;
            obj.ProfileName = item.Title;

            obj.CreateFolder = bool.Parse(item[ColCreateFolder].ToString());
            obj.EditFolder = bool.Parse(item[ColEditFolder].ToString());
            obj.DeleteFolder = bool.Parse(item[ColDeleteFolder].ToString());
            obj.DeleteFolderWithFiles = bool.Parse(item[ColDeleteFolderWithFiles].ToString());
            obj.CreateFile = bool.Parse(item[ColCreateFile].ToString());
            obj.EditFile = bool.Parse(item[ColEditFile].ToString());
            obj.DeleteFile = bool.Parse(item[ColDeleteFile].ToString());
        }
    }
}

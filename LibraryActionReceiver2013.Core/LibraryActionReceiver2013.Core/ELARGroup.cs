using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryActionReceiver.Core
{
    public class ELARGroup
    {
        public static string ListUrl = "/Lists/LARGroups";
        #region Columns
        public readonly static string ColDescription = "Title";
        public readonly static string ColGroup = "LARGroup";
        public readonly static string ColPermissionProfile = "LARPermissionProfile";
        public readonly static string ColActive = "LARActive";

        #endregion

        #region Properties
        public int Id { get; set; }
        public string Description { get; set; }
        public int Group { get; set; }
        public SPGroup GroupSP { get; set; }
        public int PermissionProfile { get; set; }

        public ELARPermisssionProfiles Profile { get; set; }
        public bool Active { get; set; }
        #endregion

        public ELARGroup()
        {
        }

        public static List<ELARGroup> GetList(SPWeb webContext)
        {
            string listUrl = string.Format("{0}{1}", webContext.Url, ListUrl);
            List<ELARGroup> ls = new List<ELARGroup>();
            SPList list = webContext.GetList(listUrl);
            SPListItemCollection items = list.GetItems();

            foreach (SPListItem item in items)
            {
                MapObj(ls, webContext, item);
            }

            return ls;
        }

        public static List<ELARGroup> GetListByActive(SPWeb webContext, bool active = true)
        {
            string listUrl = string.Format("{0}{1}", webContext.Url, ListUrl);
            List<ELARGroup> ls = new List<ELARGroup>();
            SPList list = webContext.GetList(listUrl);
            SPQuery qr = new SPQuery();
            qr.Query = string.Format("<Where><Eq><FieldRef Name='LARActive' /><Value Type='Boolean'>{0}</Value></Eq></Where>",
                active ? "1" : "0");
            SPListItemCollection items = list.GetItems(qr);

            foreach (SPListItem item in items)
            {
                MapObj(ls, webContext, item);
            }

            return ls;
        }

        private static void MapObj(List<ELARGroup> ls, SPWeb rootWeb, SPListItem item)
        {
            ELARGroup obj = new ELARGroup();

            obj.Id = item.ID;
            obj.Description = item.Title;
            SPFieldUserValue _group = new SPFieldUserValue(rootWeb, item[ColGroup].ToString());
            obj.Group = _group.LookupId;

            obj.GroupSP = rootWeb.Groups.GetByID(_group.LookupId);

            SPFieldLookupValue _permission = new SPFieldLookupValue(item[ColPermissionProfile].ToString());
            obj.PermissionProfile = _permission.LookupId;

            obj.Profile = ELARPermisssionProfiles.GetById(rootWeb, _permission.LookupId);

            obj.Active = bool.Parse(item[ColActive].ToString());

            ls.Add(obj);
        }
    }
}

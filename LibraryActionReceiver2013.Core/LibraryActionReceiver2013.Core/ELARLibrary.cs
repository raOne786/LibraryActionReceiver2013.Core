using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryActionReceiver.Core
{
    public class ELARLibrary
    {
        public static string ListUrl = "/Lists/LARLibraries";
        #region Columns
        public readonly static string ColLibraryTitle = "Title";
        public readonly static string ColDescription = "LARDescription";
        public readonly static string ColLibraryUrl = "LARLibraryUrl";
        public readonly static string ColMessageForUser = "LARMessageForUser";
        public readonly static string ColLibraryId = "LARLibraryId";
        public readonly static string ColActive = "LARActive";
        #endregion

        #region Properties
        public int Id { get; set; }
        public string LibraryTitle { get; set; }
        public string Description { get; set; }
        public string LibraryUrl { get; set; }
        public string MessageForUser { get; set; }
        public Guid LibraryId { get; set; }
        public bool Active { get; set; }
        #endregion

        public ELARLibrary()
        {
        }

        public static List<ELARLibrary> GetList(SPWeb webContext)
        {
            string listUrl = string.Format("{0}{1}", webContext.Url, ListUrl);
            List<ELARLibrary> ls = new List<ELARLibrary>();
            SPList list = webContext.GetList(listUrl);
            SPListItemCollection items = list.GetItems();

            foreach (SPListItem item in items)
            {
                MapObj(ls, webContext, item);
            }

            return ls;
        }

        public static List<ELARLibrary> GetListByActive(SPWeb webContext, bool active = true)
        {
            string listUrl = string.Format("{0}{1}", webContext.Url, ListUrl);
            List<ELARLibrary> ls = new List<ELARLibrary>();
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

        private static void MapObj(List<ELARLibrary> ls, SPWeb rootWeb, SPListItem item)
        {
            ELARLibrary obj = new ELARLibrary();

            obj.Id = item.ID;
            obj.LibraryTitle = item[ColLibraryTitle].ToString();
            obj.Description = item[ColDescription].ToString();
            SPFieldUrlValue link = new SPFieldUrlValue(item[ColLibraryUrl].ToString());
            obj.LibraryUrl = link.Url;
            obj.LibraryId = new Guid(item[ColLibraryId].ToString());
            obj.MessageForUser = item[ColMessageForUser].ToString();
            obj.Active = bool.Parse(item[ColActive].ToString());

            ls.Add(obj);
        }
    }
}

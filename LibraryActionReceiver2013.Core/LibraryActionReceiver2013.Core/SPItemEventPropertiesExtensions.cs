using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryActionReceiver.Core
{
    public static class SPItemEventPropertiesExtensions
    {
        public static SPFileSystemObjectType FSOType(this SPItemEventProperties properties)
        {
            object filesizeObj = properties.AfterProperties["vti_filesize"];
            if (filesizeObj != null)
            {
                int filesize = 0;
                if (Int32.TryParse(filesizeObj.ToString(), out filesize))
                {
                    return (filesize > 0) ? SPFileSystemObjectType.File : SPFileSystemObjectType.Folder;
                }
            }
            return SPFileSystemObjectType.Folder;
        }
    }
}

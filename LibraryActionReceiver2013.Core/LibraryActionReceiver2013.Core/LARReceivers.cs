using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryActionReceiver.Core
{
    public class LARReceivers
    {

        #region Receiver - LibraryId

        public static void ProcessLibraryIdUpdatingItem(SPItemEventProperties properties)
        {
            try
            {
                LibraryIdCommon(properties);
            }
            catch (Exception ex)
            {
                properties.ErrorMessage = ex.Message;
                properties.Status = SPEventReceiverStatus.CancelWithError;
            }
        }

        public static void ProcessLibraryIdAddingItem(SPItemEventProperties properties)
        {
            try
            {
                LibraryIdCommon(properties);
            }
            catch (Exception ex)
            {
                properties.ErrorMessage = ex.Message;
                properties.Status = SPEventReceiverStatus.CancelWithError;
            }
        }

        private static void LibraryIdCommon(SPItemEventProperties properties)
        {
            SPWeb web = properties.Web.IsRootWeb ? properties.Web : properties.Web.Site.RootWeb;

            string listUrl = properties.AfterProperties[ELARLibrary.ColLibraryUrl].ToString();
            SPFieldUrlValue fieldUrl = new SPFieldUrlValue(listUrl);

            string str = fieldUrl.Url;
            int lastSlash = str.LastIndexOf('/');
            str = (lastSlash > -1) ? str.Substring(0, lastSlash) : str;

            SPList list = null;


            using (SPWeb webUrl = web.Site.OpenWeb(str))
            {
                list = webUrl.GetList(fieldUrl.Url);
            }

            if (list != null)
                properties.AfterProperties[ELARLibrary.ColLibraryId] = list.ID.ToString().ToUpper();
        }


        #endregion

        #region Receiver - Libraries

        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties"></param>
        public static void ProcessLibrariesItemAdding(SPItemEventProperties properties)
        {
            try
            {
                                //Check if current item is Item or Folder
                bool isFolder = (properties.FSOType() == SPFileSystemObjectType.Folder);

                //Get root context.
                SPWeb web = properties.Web.IsRootWeb ? properties.Web : properties.Web.Site.RootWeb;
                
                Guid larGroups;
                Guid larLibraries;
                Guid larPermisssionProfiles;
                ExceptLists(web, out larGroups, out larLibraries, out larPermisssionProfiles);

                           if (larGroups != properties.ListId && larLibraries != properties.ListId && larPermisssionProfiles != properties.ListId)
                {

                    //Get all active libraries for run process
                    var libraries = ELARLibrary.GetListByActive(web);
                    //Get all active groups for comparison with current user groups.
                    var groups = ELARGroup.GetListByActive(web);//.Select(s => s.GroupSP).ToList<SPGroup>();
                    //Get all permission profiles
                    var profiles = ELARPermisssionProfiles.GetList(web);
                    //User Groups

                    SPRoleAssignmentCollection perm = null;
                    SPUser currentUser = properties.Web.CurrentUser;
                     if (properties.AfterUrl != null)
                    {
                        int position = properties.AfterUrl.LastIndexOf("/");
                        SPFolder workingFolder = web.GetFolder(web.Url + "/" + properties.AfterUrl.Substring(0, position));
                        SPBasePermissions spBse = workingFolder.Item.EffectiveBasePermissions;
                        if (workingFolder.Item == null)//if is a root folder
                        {
                            perm = workingFolder.DocumentLibrary.RoleAssignments;
                        }
                        else
	                    {
                            perm = workingFolder.Item.RoleAssignments;
	                    }   
                        
                    }
                    else if (properties.BeforeUrl != null)
                    {
                        int position = properties.BeforeUrl.LastIndexOf("/");
                        SPFolder workingFolder = web.GetFolder(web.Url + "/" + properties.BeforeUrl.Substring(0, position));
                        SPBasePermissions spBse = workingFolder.Item.EffectiveBasePermissions;
                        if (workingFolder.Item == null)//if is a root folder
                        {
                            perm = workingFolder.DocumentLibrary.RoleAssignments;
                        }
                        else
                        {
                            perm = workingFolder.Item.RoleAssignments;
                        }   
                    }

                    SPGroupCollection currentUserGroups = perm.Groups;
                    
                   
                        var groupsIds = groups.Select(s => s.Group).ToList<int>();
                        List<int> userGroupsIds = new List<int>();
                        foreach (SPGroup gp in currentUserGroups)
                        {
                        
                                userGroupsIds.Add(gp.ID);
                        }

                        var userInLARGroups = userGroupsIds.Intersect(groupsIds);

                    if (userInLARGroups.Count() > 0 && !web.CurrentUser.IsSiteAdmin)
                    {
                        bool canCreateFile = false;
                        bool canCreateFolder = false;
                        bool isLimited = false;
                        List<SPGroup> userGroups = new List<SPGroup>();
                        foreach (SPGroup gp in currentUserGroups)
                        {
                            //Checks if among the various groups that the user can be part of, if in any of them he is allowed to create a file or folder.
                            var g = groups.FirstOrDefault(s => s.Group == gp.ID);
                            if (g != null && g.GroupSP.ContainsCurrentUser)
                            {
                                foreach (SPRole role in g.GroupSP.Roles)
                                {
                                    //enumerate roles, if have colaborate add it, if dont have collaborate dont add.
                                    if (role.Name == "Limited Access" || role.Name == "Acceso limitado")
                                    {
                                        if (g.GroupSP.Roles.Count>1)
                                        {
                                            isLimited = false;
                                            break;
                                        }
                                        else
                                        {
                                            isLimited = true;
                                            break;
                                        }
                                    }
                                    
                                }

                                if (!isLimited)
                                {
                                    if (!canCreateFile)
                                        canCreateFile = g.Profile.CreateFile;

                                    if (!canCreateFolder)
                                        canCreateFolder = g.Profile.CreateFolder;
                                }
                                /*validate if the user is in the folder group*/
                            }

                        }

                        CheckUserActions(properties, isFolder, libraries, canCreateFile, canCreateFolder, false, ELARActionType.Adding);

                    }
                }
                    

                    

            }
            catch (Exception ex)
            {
                properties.ErrorMessage = ex.Message;
                properties.Status = SPEventReceiverStatus.CancelWithError;
            }
        }
        /// <summary>
        /// This method checks whether the current library can pass through validation.
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="libraries"></param>
        public static void ProcessLibrariesItemUpdating(SPItemEventProperties properties)
        {
            try
            {

                bool isFolder = (properties.FSOType() == SPFileSystemObjectType.Folder);

                //Get root context.
                SPWeb web = properties.Web.IsRootWeb ? properties.Web : properties.Web.Site.RootWeb;

                Guid larGroups;
                Guid larLibraries;
                Guid larPermisssionProfiles;
                ExceptLists(web, out larGroups, out larLibraries, out larPermisssionProfiles);

              
                if (larGroups != properties.ListId && larLibraries != properties.ListId && larPermisssionProfiles != properties.ListId)
                {

                    //Get all active libraries for run process
                    var libraries = ELARLibrary.GetListByActive(web);
                    //Get all active groups for comparison with current user groups.
                    var groups = ELARGroup.GetListByActive(web);//.Select(s => s.GroupSP).ToList<SPGroup>();
                    //Get all permission profiles
                    var profiles = ELARPermisssionProfiles.GetList(web);
                    //User Groups

                    SPRoleAssignmentCollection perm = null;
                   SPUser currentUser = properties.Web.CurrentUser;
                    if (properties.AfterUrl != null)
                    {
                        int position = properties.AfterUrl.LastIndexOf("/");
                        SPFolder workingFolder = web.GetFolder(web.Url + "/" + properties.AfterUrl.Substring(0, position));
                       SPBasePermissions spBse = workingFolder.Item.EffectiveBasePermissions;
                        if (workingFolder.Item == null)//if is a root folder
                        {
                            perm = workingFolder.DocumentLibrary.RoleAssignments;
                        }
                        else
                        {
                            perm = workingFolder.Item.RoleAssignments;
                        }   
                    }
                    else if (properties.BeforeUrl != null)
                    {
                        int position = properties.BeforeUrl.LastIndexOf("/");
                        SPFolder workingFolder = web.GetFolder(web.Url + "/" + properties.BeforeUrl.Substring(0, position));
                       SPBasePermissions spBse = workingFolder.Item.EffectiveBasePermissions;
                        if (workingFolder.Item == null)//if is a root folder
                        {
                            perm = workingFolder.DocumentLibrary.RoleAssignments;
                        }
                        else
                        {
                            perm = workingFolder.Item.RoleAssignments;
                        }   
                    }

                    SPGroupCollection currentUserGroups = perm.Groups;

                    var groupsIds = groups.Select(s => s.Group).ToList<int>();
                    List<int> userGroupsIds = new List<int>();
                    foreach (SPGroup gp in currentUserGroups)
                    {

                        userGroupsIds.Add(gp.ID);
                    }

                    var userInLARGroups = userGroupsIds.Intersect(groupsIds);

                    if (userInLARGroups.Count() > 0 && !web.CurrentUser.IsSiteAdmin)
                    {

                        bool canEditFile = false;
                        bool canEditFolder = false;
                        bool isLimited = false;
                        List<SPGroup> userGroups = new List<SPGroup>();
                        foreach (SPGroup gp in currentUserGroups)
                        {
                            //Checks if among the various groups that the user can be part of, if in any of them he is allowed to create a file or folder.
                            var g = groups.FirstOrDefault(s => s.Group == gp.ID);
                            if (g != null && g.GroupSP.ContainsCurrentUser)
                            {
                                foreach (SPRole role in g.GroupSP.Roles)
                                {
                                    //enumerate roles, if have colaborate add it, if dont have collaborate dont add.
                                    if (role.Name == "Limited Access" || role.Name == "Acceso limitado")
                                    {
                                        if (g.GroupSP.Roles.Count > 1)
                                        {
                                            isLimited = false;
                                            break;
                                        }
                                        else
                                        {
                                            isLimited = true;
                                            break;
                                        }
                                    }

                                }

                                if (!isLimited)
                                {
                                    if (!canEditFile)
                                        canEditFile = g.Profile.EditFile;

                                    if (!canEditFolder)
                                        canEditFolder = g.Profile.EditFolder;
                                }
                                /*validate if the user is in the folder group*/
                            }

                        }

                        CheckUserActions(properties, isFolder, libraries, canEditFile, canEditFolder, false, ELARActionType.Updating);

                    }
                }

            }
            catch (Exception ex)
            {
                properties.ErrorMessage = ex.Message;
                properties.Status = SPEventReceiverStatus.CancelWithError;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties"></param>
        public static void ProcessLibrariesItemDeleting(SPItemEventProperties properties)
        {
            try
            {
                bool isFolder = (properties.ListItem.FileSystemObjectType == SPFileSystemObjectType.Folder);

                //Get root context.
                SPWeb web = properties.Web.IsRootWeb ? properties.Web : properties.Web.Site.RootWeb;

                Guid larGroups;
                Guid larLibraries;
                Guid larPermisssionProfiles;
                ExceptLists(web, out larGroups, out larLibraries, out larPermisssionProfiles);

                if (larGroups != properties.ListId && larLibraries != properties.ListId && larPermisssionProfiles != properties.ListId)
                {

                    //Get all active libraries for run process
                    var libraries = ELARLibrary.GetListByActive(web);
                    //Get all active groups for comparison with current user groups.
                    var groups = ELARGroup.GetListByActive(web);//.Select(s => s.GroupSP).ToList<SPGroup>();
                    //Get all permission profiles
                    var profiles = ELARPermisssionProfiles.GetList(web);
                    //User Groups
                    SPRoleAssignmentCollection perm = null;
                   SPUser currentUser = properties.Web.CurrentUser;
                   
                    if (properties.AfterUrl != null )
                    {
                        int position = properties.AfterUrl.LastIndexOf("/");
                        SPFolder workingFolder = web.GetFolder(web.Url + "/" + properties.AfterUrl.Substring(0, position));
                        SPBasePermissions spBse = workingFolder.Item.EffectiveBasePermissions;
                        if (workingFolder.Item == null)//if is a root folder
                        {
                            perm = workingFolder.DocumentLibrary.RoleAssignments;
                        }
                        else
                        {
                            perm = workingFolder.Item.RoleAssignments;
                        }   
                    }
                    else if(properties.BeforeUrl != null)
                    {
                        int position = properties.BeforeUrl.LastIndexOf("/");
                        SPFolder workingFolder = web.GetFolder(web.Url + "/" + properties.BeforeUrl.Substring(0, position));
                        if (workingFolder.Item == null)//if is a root folder
                        {
                            perm = workingFolder.DocumentLibrary.RoleAssignments;
                        }
                        else
                        {
                            perm = workingFolder.Item.RoleAssignments;
                        }   
                    }

                    SPGroupCollection currentUserGroups = perm.Groups;

                    var groupsIds = groups.Select(s => s.Group).ToList<int>();
                    List<int> userGroupsIds = new List<int>();
                    foreach (SPGroup gp in currentUserGroups)
                    {

                        userGroupsIds.Add(gp.ID);
                    }

                    var userInLARGroups = userGroupsIds.Intersect(groupsIds);

                    if (userInLARGroups.Count() > 0 && !web.CurrentUser.IsSiteAdmin)
                    {
                        bool canDeleteFile = false;
                        bool canDeleteFolder = false;
                        bool canDeleteFolderWithFiles = false;
                        bool isLimited = false;
                        List<SPGroup> userGroups = new List<SPGroup>();

                        foreach (SPGroup gp in currentUserGroups)
                        {
                            //Checks if among the various groups that the user can be part of, if in any of them he is allowed to create a file or folder.
                            var g = groups.FirstOrDefault(s => s.Group == gp.ID);
                            if (g != null && g.GroupSP.ContainsCurrentUser)
                            {
                                foreach (SPRole role in g.GroupSP.Roles)
                                {
                                    //enumerate roles, if have colaborate add it, if dont have collaborate dont add.
                                    if (role.Name == "Limited Access" || role.Name == "Acceso limitado")
                                    {
                                        if (g.GroupSP.Roles.Count > 1)
                                        {
                                            isLimited = false;
                                            break;
                                        }
                                        else
                                        {
                                            isLimited = true;
                                            break;
                                        }
                                    }

                                }

                                if (!isLimited)
                                {
                                    if (!canDeleteFile)
                                        canDeleteFile = g.Profile.DeleteFile;

                                    if (!canDeleteFolder)
                                        canDeleteFolder = g.Profile.DeleteFolder;

                                    if (!canDeleteFolderWithFiles)
                                        canDeleteFolderWithFiles = g.Profile.DeleteFolderWithFiles;
                                }
                                /*validate if the user is in the folder group*/
                            }

                        }

                        

                        CheckUserActions(properties, isFolder, libraries, canDeleteFile, canDeleteFolder, canDeleteFolderWithFiles, ELARActionType.Deleting);

                    }
                }
            }
            catch (Exception ex)
            {
                properties.ErrorMessage = ex.Message;
                properties.Status = SPEventReceiverStatus.CancelWithError;
            }
        }


        #region Extras
        private static void ExceptLists(SPWeb web, out Guid larGroups, out Guid larLibraries, out Guid larPermisssionProfiles)
        {
            //Get ID of LARGroups and LARLibraries Lists for ignore in this process.
            larGroups = web.GetList(string.Format("{0}{1}", web.Url, ELARGroup.ListUrl)).ID;
            larLibraries = web.GetList(string.Format("{0}{1}", web.Url, ELARLibrary.ListUrl)).ID;
            larPermisssionProfiles = web.GetList(string.Format("{0}{1}", web.Url, ELARPermisssionProfiles.ListUrl)).ID;
        }
        private static void CheckLibraryAndShowMessage(SPItemEventProperties properties, List<ELARLibrary> libraries, bool isFolder=false)
        {
            //Checks if the current library is listed in the LARLibraries
           // var lib = libraries.FirstOrDefault(s => s.LibraryId == properties.ListId && s.LibraryUrl.Split('/')[s.LibraryUrl.Split('/').Count() - 1] == properties.AfterUrl.Split('/')[properties.AfterUrl.Split('/').Count()-1]);// EJRR buscar por id y por nombre
            bool isChecked = false;
            string meSSageForUser = string.Empty;
     
            foreach (ELARLibrary item in libraries)
            {
                if (isFolder)
                {
                    //Deleting folder becomes afterurl null
                    if (properties.AfterUrl == null)
                    {
                        if (item.LibraryId == properties.ListId && System.Net.WebUtility.UrlDecode(item.LibraryUrl.Split('/')[item.LibraryUrl.Split('/').Count() - 1]) ==
                 System.Net.WebUtility.UrlDecode(properties.BeforeUrl.Split('/')[properties.BeforeUrl.Split('/').Count() - 2]))
                        {
                            isChecked = true;
                            meSSageForUser = item.MessageForUser;
                        }
                    }
                    else
                    {
                        if (item.LibraryId == properties.ListId && System.Net.WebUtility.UrlDecode(item.LibraryUrl.Split('/')[item.LibraryUrl.Split('/').Count() - 1]) ==
                        System.Net.WebUtility.UrlDecode(properties.AfterUrl.Split('/')[properties.AfterUrl.Split('/').Count() - 2]))
                        {
                            isChecked = true;
                            meSSageForUser = item.MessageForUser;
                        }
                    }
                }
                else 
                {
                    if (item.LibraryId == properties.ListId && System.Net.WebUtility.UrlDecode(item.LibraryUrl.Split('/')[item.LibraryUrl.Split('/').Count() - 1]) ==
                  System.Net.WebUtility.UrlDecode(properties.BeforeUrl.Split('/')[properties.BeforeUrl.Split('/').Count() - 2]))
                    {
                        isChecked = true;
                        meSSageForUser = item.MessageForUser;
                    }
                }
               
            }

            //if different from null, then process run. 
            //if (lib != null )
            if (isChecked)
            {
                properties.Status = SPEventReceiverStatus.CancelWithError;
                //properties.ErrorMessage = lib.MessageForUser;
                properties.ErrorMessage = meSSageForUser;
            }
        }
        private static void CheckUserActions(SPItemEventProperties properties, bool isFolder, List<ELARLibrary> libraries, bool fileCondition, bool folderCondition, bool folderConditionWithFiles, ELARActionType actionType)
        {
            //Run only if not a folder.
            if (!isFolder)
            {
                //When the variable is false, the user will be denied access to create a file or upload.
                if (!fileCondition)
                {
                    CheckLibraryAndShowMessage(properties, libraries);
                }

            }
            else
            {
                if (actionType == ELARActionType.Deleting)
                {
                    SPFolder f = properties.Web.GetFolder(string.Format("{0}/{1}", properties.WebUrl, properties.BeforeUrl));


                    if (f.ItemCount > 0)
                    {
                        //When the variable is false, the user will be denied access to create a folder.
                        if (!folderConditionWithFiles)
                        {
                            CheckLibraryAndShowMessage(properties, libraries,isFolder);
                        }
                    }
                    else
                    {
                        //When the variable is false, the user will be denied access to create a folder.
                        if (!folderCondition)
                        {
                            CheckLibraryAndShowMessage(properties, libraries,isFolder);
                        }
                    }



                }
                else
                {
                    //When the variable is false, the user will be denied access to create a folder.
                    if (!folderCondition)
                    {
                        CheckLibraryAndShowMessage(properties, libraries,isFolder);
                    }
                }
            }
        }
        #endregion


        #endregion

    }
}

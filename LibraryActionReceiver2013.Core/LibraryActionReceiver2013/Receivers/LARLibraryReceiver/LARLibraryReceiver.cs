using System;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;
using LibraryActionReceiver.Core;
using System.Collections.Generic;
using System.Linq;

namespace LibraryActionReceiver2013.Receivers.LARLibraryReceiver
{
    /// <summary>
    /// List Item Events
    /// </summary>
    public class LARLibraryReceiver : SPItemEventReceiver
    {
        /// <summary>
        /// An item was added.
        /// </summary>
        public override void ItemAdded(SPItemEventProperties properties)
        {
            base.ItemAdded(properties);
        }
        /// <summary>
        /// An item is being added.
        /// </summary>
        public override void ItemAdding(SPItemEventProperties properties)
        {
            try
            {
                base.ItemAdding(properties);
                LARReceivers.ProcessLibrariesItemAdding(properties);
            }
            catch (Exception ex)
            {
                properties.ErrorMessage = ex.Message;
                properties.Status = SPEventReceiverStatus.CancelWithError;
            }

        }



        /// <summary>
        /// An item is being updated.
        /// </summary>
        public override void ItemUpdating(SPItemEventProperties properties)
        {
            base.ItemUpdating(properties);
            LARReceivers.ProcessLibrariesItemUpdating(properties);
        }



        /// <summary>
        /// An item is being deleted.
        /// </summary>
        public override void ItemDeleting(SPItemEventProperties properties)
        {
            base.ItemDeleting(properties);

            LARReceivers.ProcessLibrariesItemDeleting(properties);

        }


        /// <summary>
        /// An item is being checked in.
        /// </summary>
        public override void ItemCheckingIn(SPItemEventProperties properties)
        {
            base.ItemCheckingIn(properties);
        }

        /// <summary>
        /// An item is being checked out.
        /// </summary>
        public override void ItemCheckingOut(SPItemEventProperties properties)
        {
            base.ItemCheckingOut(properties);
        }



        /// <summary>
        /// An item was updated.
        /// </summary>
        public override void ItemUpdated(SPItemEventProperties properties)
        {
            base.ItemUpdated(properties);
        }

        /// <summary>
        /// An item was deleted.
        /// </summary>
        public override void ItemDeleted(SPItemEventProperties properties)
        {
            base.ItemDeleted(properties);
        }

        /// <summary>
        /// An item was checked in.
        /// </summary>
        public override void ItemCheckedIn(SPItemEventProperties properties)
        {
            base.ItemCheckedIn(properties);
        }

        /// <summary>
        /// An item was checked out.
        /// </summary>
        public override void ItemCheckedOut(SPItemEventProperties properties)
        {
            base.ItemCheckedOut(properties);
        }

        /// <summary>
        /// The list received a context event.
        /// </summary>
        public override void ContextEvent(SPItemEventProperties properties)
        {
            base.ContextEvent(properties);
        }


    }
}
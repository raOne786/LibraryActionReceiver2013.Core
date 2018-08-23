using System;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;
using LibraryActionReceiver.Core;

namespace LibraryActionReceiver2013.Receivers.LARLibraryIdReceiver
{
    /// <summary>
    /// List Item Events
    /// </summary>
    public class LARLibraryIdReceiver : SPItemEventReceiver
    {
        /// <summary>
        /// An item is being added.
        /// </summary>
        public override void ItemAdding(SPItemEventProperties properties)
        {
            base.ItemAdding(properties);
            LARReceivers.ProcessLibraryIdAddingItem(properties);
        }



        /// <summary>
        /// An item is being updated.
        /// </summary>
        public override void ItemUpdating(SPItemEventProperties properties)
        {
            base.ItemUpdating(properties);
            LARReceivers.ProcessLibraryIdUpdatingItem(properties);
        }



        /// <summary>
        /// An item was added.
        /// </summary>
        public override void ItemAdded(SPItemEventProperties properties)
        {
            base.ItemAdded(properties);




        }

        /// <summary>
        /// An item was updated.
        /// </summary>
        public override void ItemUpdated(SPItemEventProperties properties)
        {
            base.ItemUpdated(properties);
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
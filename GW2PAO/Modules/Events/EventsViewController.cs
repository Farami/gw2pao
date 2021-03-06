﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GW2PAO.Infrastructure;
using GW2PAO.Modules.Events.Interfaces;
using GW2PAO.Modules.Events.Views.EventNotification;
using GW2PAO.Modules.Events.Views.EventTracker;
using GW2PAO.Utility;
using Microsoft.Practices.Prism.Commands;
using NLog;

namespace GW2PAO.Modules.Events
{
    [Export(typeof(IEventsViewController))]
    public class EventsViewController : IEventsViewController
    {
        /// <summary>
        /// Default logger
        /// </summary>
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Composition container of composed parts
        /// </summary>
        [Import]
        private CompositionContainer Container { get; set; }

        /// <summary>
        /// The events tracker view
        /// </summary>
        private EventTrackerView eventTrackerView;

        /// <summary>
        /// The event notifications window containing all event notifications
        /// </summary>
        private EventNotificationWindow eventNotificationsView;

        /// <summary>
        /// Displays all previously-opened windows and other windows
        /// that must be shown at startup
        /// </summary>
        public void Initialize()
        {
            logger.Debug("Initializing");

            logger.Debug("Registering hotkey commands");
            HotkeyCommands.ToggleEventTrackerCommand.RegisterCommand(new DelegateCommand(this.ToggleEventsTracker));

            Threading.BeginInvokeOnUI(() =>
            {
                if (Properties.Settings.Default.IsEventTrackerOpen && this.CanDisplayEventsTracker())
                    this.DisplayEventsTracker();

                if (this.CanDisplayEventNotificationsWindow())
                    this.DisplayEventNotificationsWindow();
            });
        }

        /// <summary>
        /// Closes all windows and saves the "was previously opened" state for those windows.
        /// </summary>
        public void Shutdown()
        {
            logger.Debug("Shutting down");

            if (this.eventTrackerView != null)
            {
                Properties.Settings.Default.IsEventTrackerOpen = this.eventTrackerView.IsVisible;
                Threading.InvokeOnUI(() => this.eventTrackerView.Close());
            }

            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Displays the Event Tracker window, or, if already displayed, sets
        /// focus to the window
        /// </summary>
        public void DisplayEventsTracker()
        {
            if (this.eventTrackerView == null || !this.eventTrackerView.IsVisible)
            {
                this.eventTrackerView = new EventTrackerView();
                this.Container.ComposeParts(this.eventTrackerView);
                this.eventTrackerView.Show();
            }
            else
            {
                this.eventTrackerView.Focus();
            }
        }

        /// <summary>
        /// Determines if the event tracker can be displayed
        /// </summary>
        /// <returns>Always true</returns>
        public bool CanDisplayEventsTracker()
        {
            return true;
        }

        /// <summary>
        /// Displays the Event Notifications window
        /// </summary>
        public void DisplayEventNotificationsWindow()
        {
            this.eventNotificationsView = new EventNotificationWindow();
            this.Container.ComposeParts(this.eventNotificationsView);
            this.eventNotificationsView.Show();
        }

        /// <summary>
        /// Determines if the Event Notifications window can be displayed
        /// </summary>
        /// <returns>Always true</returns>
        public bool CanDisplayEventNotificationsWindow()
        {
            return true;
        }

        /// <summary>
        /// Toggles whether or not the events tracker is visible
        /// </summary>
        private void ToggleEventsTracker()
        {
            if (this.eventTrackerView == null || !this.eventTrackerView.IsVisible)
            {
                this.DisplayEventsTracker();
            }
            else
            {
                this.eventTrackerView.Close();
            }
        }
    }
}

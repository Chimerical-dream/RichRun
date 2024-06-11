#if UNITY_ANDROID
using System;
using Unity.Notifications.Android;
using UnityEngine;

namespace Game.Core {

    public enum NotificationsIds {
        GameReturn1 = 100,
        GameReturn2 = 101,

        // add here custom notification ids
    }

    /// <summary>
    /// Application notification manager
    /// </summary>
    public class NotificationManager {
        private static NotificationManager instance;
        public static NotificationManager Instance {
            get {
                if (instance == null) {
                    instance = new NotificationManager();
                }
                return instance;
            }
        }

        private AndroidNotificationChannel scheduledNotificationChannel;

        private static readonly string GameSmallIcon = "icon_0";
        private static readonly string GameLargeIcon = "icon_1";

        // todo: NOTIFICATION change title, text, icon and etc!
        private NotificationData[] _gameReturnNotifications = new NotificationData[] {
            new NotificationData() { Title = "Try Neon Rush",
                                     Text = "Reach the ultimate speed!",
                                     HasIcons = true,
                                     SmallIcon = "icon_0",
                                     LargeIcon = "icon_1" },
        };

        /// <summary>
        /// Call this method on start of Application to init notifications
        /// </summary>
        public void Init() {
            CreateScheduledNotificationChannel();
            UpdateGameReturnNotifications();
        }

        private void CreateScheduledNotificationChannel() {
            scheduledNotificationChannel = new AndroidNotificationChannel() {
                Id = $"scheduled_{Application.productName}_notifications",
                Name = "Scheduled Notifications",
                Importance = Importance.Default,
                Description = "Notifications to remind the player about the game.",
                EnableLights = true,
            };
            AndroidNotificationCenter.RegisterNotificationChannel(scheduledNotificationChannel);
        }

        private void UpdateGameReturnNotifications() {
            AndroidNotificationCenter.CancelNotification((int)NotificationsIds.GameReturn1);
            AndroidNotificationCenter.CancelNotification((int)NotificationsIds.GameReturn2);

            // here add notifications on game start
            SendScheduledNotification_GameReturn();
        }

        public void CancelNotification(NotificationsIds id) {
            var notificationStatus = AndroidNotificationCenter.CheckScheduledNotificationStatus((int)id);

            if (notificationStatus == NotificationStatus.Scheduled) {
                AndroidNotificationCenter.CancelScheduledNotification((int)id);
            }
            else if (notificationStatus == NotificationStatus.Delivered) {
                // Remove the previously shown notification from the status bar.
                AndroidNotificationCenter.CancelNotification((int)id);
            }
        }

        public void SendScheduledNotification(string title, string text, DateTime invokeTime, NotificationsIds id) {
            AndroidNotification notification = new AndroidNotification();
            var data = new NotificationData() {
                Title = title,
                Text = text,

                HasIcons = true,
                SmallIcon = GameSmallIcon,
                LargeIcon = GameLargeIcon
            };
            SetupNotification(ref notification, data);

            notification.FireTime = invokeTime;
            AndroidNotificationCenter.SendNotificationWithExplicitID(notification, scheduledNotificationChannel.Id, (int)id);
        }

        private void SendScheduledNotification_GameReturn(int waitDaysCount = 2, int fireHourFirst = 12,
                                                          int fireHourSecond = -1, int repeatInterfalDays = 7) {
            var notificationData = _gameReturnNotifications[UnityEngine.Random.Range(0, _gameReturnNotifications.Length)];

            TimeSpan repeatInterval = new TimeSpan(repeatInterfalDays, 0, 0, 0);
            AndroidNotification notification = new AndroidNotification();
            SetupNotification(ref notification, notificationData);

            // set notification fire time
            DateTime fireTime = DateTime.Now.AddDays(waitDaysCount);
            fireTime = new DateTime(fireTime.Year, fireTime.Month, fireTime.Day,
                hour: fireHourFirst, minute: 30, second: 0, DateTimeKind.Local);
            notification.FireTime = fireTime;
            notification.RepeatInterval = repeatInterval;
            notification.ShouldAutoCancel = true;

            AndroidNotificationCenter.SendNotificationWithExplicitID(notification, scheduledNotificationChannel.Id, (int)NotificationsIds.GameReturn1);

            if (fireHourSecond > 0) {
                notificationData = _gameReturnNotifications[UnityEngine.Random.Range(0, _gameReturnNotifications.Length)];
                SetupNotification(ref notification, notificationData);

                fireTime = new DateTime(fireTime.Year, fireTime.Month, fireTime.Day,
                    hour: fireHourSecond, minute: 0, second: 0, DateTimeKind.Local);
                notification.FireTime = fireTime;
                notification.RepeatInterval = repeatInterval;

                AndroidNotificationCenter.SendNotificationWithExplicitID(notification, scheduledNotificationChannel.Id, (int)NotificationsIds.GameReturn2);
            }
        }

        private void SetupNotification(ref AndroidNotification notification, NotificationData notificationData) {
            notification.Title = notificationData.Title;
            notification.Text = notificationData.Text;

            if (notificationData.HasIcons) {
                notification.SmallIcon = notificationData.SmallIcon;
                notification.LargeIcon = notificationData.LargeIcon;
            }
        }

        /// <summary>
        /// Notification info
        /// </summary>
        private struct NotificationData {
            public string Title;
            public string Text;

            public bool HasIcons;
            public string SmallIcon;
            public string LargeIcon;
        }
    }
}
#endif
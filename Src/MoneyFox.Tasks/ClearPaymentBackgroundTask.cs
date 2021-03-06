﻿using Windows.ApplicationModel.Background;
using Microsoft.HockeyApp;
using MoneyFox.Shared;
using MoneyFox.Shared.Constants;
using MoneyFox.Shared.DataAccess;
using MoneyFox.Shared.Manager;
using MoneyFox.Shared.Repositories;
using MoneyFox.Windows.Services;
using MoneyFox.Windows.Shortcuts;
using MvvmCross.Plugins.File.WindowsCommon;
using MvvmCross.Plugins.Sqlite.WindowsUWP;

namespace MoneyFox.Tasks
{
    public sealed class ClearPaymentBackgroundTask : IBackgroundTask
    {
        private readonly PaymentManager paymentManager;

        public ClearPaymentBackgroundTask() {

#if !DEBUG
            HockeyClient.Current.Configure(ServiceConstants.HOCKEY_APP_WINDOWS_ID,
                new TelemetryConfiguration {EnableDiagnostics = true});
#endif

            HockeyClient.Current.TrackEvent("Ctror Background Task");

            var unitOfWork = new UnitOfWork(new DatabaseManager(new WindowsSqliteConnectionFactory(),
                new MvxWindowsCommonFileStore()));

            var notificationService = new NotificationService();
            paymentManager = new PaymentManager(unitOfWork, null);
        }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            HockeyClient.Current.TrackEvent("Run BackgroundTask");

            paymentManager.ClearPayments();
            Tile.UpdateMainTile();
        }
    }
}
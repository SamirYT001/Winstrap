using System.Windows;

using Windows.Win32;
using Windows.Win32.Foundation;

using Bloxstrap.UI.Elements.Dialogs;
using Bloxstrap.Enums;

namespace Bloxstrap
{
    public static class LaunchHandler
    {
        public static void ProcessNextAction(NextAction action, bool isUnfinishedInstall = false)
        {
            const string LOG_IDENT = "LaunchHandler::ProcessNextAction";

            switch (action)
            {
                case NextAction.LaunchSettings:
                    App.Logger.WriteLine(LOG_IDENT, "Opening settings");
                    LaunchSettings();
                    break;

                case NextAction.LaunchRoblox:
                    App.Logger.WriteLine(LOG_IDENT, "Opening Roblox");
                    LaunchRoblox(LaunchMode.Player);
                    break;

                case NextAction.LaunchRobloxStudio:
                    App.Logger.WriteLine(LOG_IDENT, "Opening Roblox Studio");
                    LaunchRoblox(LaunchMode.Studio);
                    break;

                default:
                    App.Logger.WriteLine(LOG_IDENT, "Closing");
                    App.Terminate(isUnfinishedInstall ? ErrorCode.ERROR_INSTALL_USEREXIT : ErrorCode.ERROR_SUCCESS);
                    break;
            }
        }

        public static void ProcessLaunchArgs()
        {
            const string LOG_IDENT = "LaunchHandler::ProcessLaunchArgs";

            if (App.LaunchSettings.UninstallFlag.Active)
            {
                App.Logger.WriteLine(LOG_IDENT, "Opening uninstaller");
                LaunchUninstaller();
            }
            else if (App.LaunchSettings.MenuFlag.Active)
            {
                App.Logger.WriteLine(LOG_IDENT, "Opening settings");
                LaunchSettings();
            }
            else if (App.LaunchSettings.WatcherFlag.Active)
            {
                App.Logger.WriteLine(LOG_IDENT, "Opening watcher");
                LaunchWatcher();
            }
            else if (App.LaunchSettings.MultiInstanceWatcherFlag.Active)
            {
                App.Logger.WriteLine(LOG_IDENT, "Opening multi-instance watcher");
                LaunchMultiInstanceWatcher();
            }
            else if (App.LaunchSettings.BackgroundUpdaterFlag.Active)
            {
                App.Logger.WriteLine(LOG_IDENT, "Opening background updater");
                LaunchBackgroundUpdater();
            }
            // === ДОБАВЬ ЭТОТ БЛОК ===
            else if (App.LaunchSettings.OverlayFlag.Active)
            {
                App.Logger.WriteLine(LOG_IDENT, "Opening overlay");
                LaunchOverlay();
            }
            // ========================
            else if (App.LaunchSettings.RobloxLaunchMode != LaunchMode.None)
            {
                App.Logger.WriteLine(LOG_IDENT, $"Opening bootstrapper ({App.LaunchSettings.RobloxLaunchMode})");
                LaunchRoblox(App.LaunchSettings.RobloxLaunchMode);
            }
            else if (!App.LaunchSettings.QuietFlag.Active)
            {
                App.Logger.WriteLine(LOG_IDENT, "Opening menu");
                LaunchMenu();
            }
            else
            {
                App.Logger.WriteLine(LOG_IDENT, "Closing - quiet flag active");
                App.Terminate();
            }
        }

        public static void LaunchInstaller()
        {
            using var interlock = new InterProcessLock("Installer");

            if (!interlock.IsAcquired)
            {
                Frontend.ShowMessageBox(Strings.Dialog_AlreadyRunning_Installer, MessageBoxImage.Stop);
                App.Terminate();
                return;
            }

            if (App.LaunchSettings.UninstallFlag.Active)
            {
                Frontend.ShowMessageBox(Strings.Bootstrapper_FirstRunUninstall, MessageBoxImage.Error);
                App.Terminate(ErrorCode.ERROR_INVALID_FUNCTION);
                return;
            }

            if (App.LaunchSettings.QuietFlag.Active)
            {
                var installer = new Installer();

                if (!installer.CheckInstallLocation())
                    App.Terminate(ErrorCode.ERROR_INSTALL_FAILURE);

                installer.DoInstall();

                interlock.Dispose();

                ProcessLaunchArgs();
            }
            else
            {
#if QA_BUILD
                Frontend.ShowMessageBox("You are about to install a QA build of Bloxstrap. The red window border indicates that this is a QA build.\n\nQA builds are handled completely separately of your standard installation, like a virtual environment.", MessageBoxImage.Information);
#endif

                new LanguageSelectorDialog().ShowDialog();

                var installer = new UI.Elements.Installer.MainWindow();
                installer.ShowDialog();

                interlock.Dispose();

                ProcessNextAction(installer.CloseAction, !installer.Finished);
            }

        }

        public static void LaunchUninstaller()
        {
            using var interlock = new InterProcessLock("Uninstaller");

            if (!interlock.IsAcquired)
            {
                Frontend.ShowMessageBox(Strings.Dialog_AlreadyRunning_Uninstaller, MessageBoxImage.Stop);
                App.Terminate();
                return;
            }

            bool confirmed = false;
            bool keepData = true;

            if (App.LaunchSettings.QuietFlag.Active)
            {
                confirmed = true;
            }
            else
            {
                var dialog = new UninstallerDialog();
                dialog.ShowDialog();

                confirmed = dialog.Confirmed;
                keepData = dialog.KeepData;
            }

            if (!confirmed)
            {
                App.Terminate();
                return;
            }

            Installer.DoUninstall(keepData);

            Frontend.ShowMessageBox(Strings.Bootstrapper_SuccessfullyUninstalled, MessageBoxImage.Information);

            App.Terminate();
        }

        public static void LaunchSettings()
        {
            const string LOG_IDENT = "LaunchHandler::LaunchSettings";

            using var interlock = new InterProcessLock("Settings");

            if (interlock.IsAcquired)
            {
                bool showAlreadyRunningWarning = Process.GetProcessesByName(App.ProjectName).Length > 1;

                var window = new UI.Elements.Settings.MainWindow(showAlreadyRunningWarning);

                // typically we'd use Show(), but we need to block to ensure IPL stays in scope
                window.ShowDialog();
            }
            else
            {
                App.Logger.WriteLine(LOG_IDENT, "Found an already existing menu window");

                var process = Utilities.GetProcessesSafe().Where(x => x.MainWindowTitle == Strings.Menu_Title).FirstOrDefault();

                if (process is not null)
                    PInvoke.SetForegroundWindow((HWND)process.MainWindowHandle);

                App.Terminate();
            }
        }

        public static void LaunchMenu()
        {
            var dialog = new LaunchMenuDialog();
            dialog.ShowDialog();

            ProcessNextAction(dialog.CloseAction);
        }

        public static void LaunchRoblox(LaunchMode launchMode)
        {
            const string LOG_IDENT = "LaunchHandler::LaunchRoblox";

            if (launchMode == LaunchMode.None)
                throw new InvalidOperationException("No Roblox launch mode set");

            if (!File.Exists(Path.Combine(Paths.System, "mfplat.dll")))
            {
                Frontend.ShowMessageBox(Strings.Bootstrapper_WMFNotFound, MessageBoxImage.Error);

                if (!App.LaunchSettings.QuietFlag.Active)
                    Utilities.ShellExecute("https://support.microsoft.com/en-us/topic/media-feature-pack-list-for-windows-n-editions-c1c6fffa-d052-8338-7a79-a4bb980a700a");

                App.Terminate(ErrorCode.ERROR_FILE_NOT_FOUND);
            }

            if (App.Settings.Prop.ConfirmLaunches && Mutex.TryOpenExisting("ROBLOX_singletonMutex", out var _) && !App.Settings.Prop.MultiInstanceLaunching)
            {
                // this currently doesn't work very well since it relies on checking the existence of the singleton mutex
                // which often hangs around for a few seconds after the window closes
                // it would be better to have this rely on the activity tracker when we implement IPC in the planned refactoring

                var result = Frontend.ShowMessageBox(Strings.Bootstrapper_ConfirmLaunch, MessageBoxImage.Warning, MessageBoxButton.YesNo);

                if (result != MessageBoxResult.Yes)
                {
                    App.Terminate();
                    return;
                }
            }

            // Запуск оверлея как отдельного процесса
            if (App.Settings.Prop.OverlayEnabled)
            {
                App.Logger.WriteLine(LOG_IDENT, "Overlay is enabled, launching as separate process...");
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = Paths.Process,
                        Arguments = "-overlay",
                        UseShellExecute = true
                    });
                    App.Logger.WriteLine(LOG_IDENT, "Overlay process started");
                }
                catch (Exception ex)
                {
                    App.Logger.WriteException(LOG_IDENT, ex);
                }
            }

            // start bootstrapper and show the bootstrapper modal if we're not running silently
            App.Logger.WriteLine(LOG_IDENT, "Initializing bootstrapper");
            App.Bootstrapper = new Bootstrapper(launchMode);
            IBootstrapperDialog? dialog = null;

            if (!App.LaunchSettings.QuietFlag.Active)
            {
                App.Logger.WriteLine(LOG_IDENT, "Initializing bootstrapper dialog");
                dialog = App.Settings.Prop.BootstrapperStyle.GetNew();
                App.Bootstrapper.Dialog = dialog;
                dialog.Bootstrapper = App.Bootstrapper;
            }

            Task.Run(App.Bootstrapper.Run).ContinueWith(t =>
            {
                App.Logger.WriteLine(LOG_IDENT, "Bootstrapper task has finished");

                if (t.IsFaulted)
                {
                    App.Logger.WriteLine(LOG_IDENT, "An exception occurred when running the bootstrapper");

                    if (t.Exception is not null)
                        App.FinalizeExceptionHandling(t.Exception);
                }

                App.Terminate();
            });

            dialog?.ShowBootstrapper();

            App.Logger.WriteLine(LOG_IDENT, "Exiting");
        }

        public static void LaunchWatcher()
        {
            const string LOG_IDENT = "LaunchHandler::LaunchWatcher";

            // this whole topology is a bit confusing, bear with me:
            // main thread: strictly UI only, handles showing of the notification area icon, context menu, server details dialog
            // - server information task: queries server location, invoked if either the explorer notification is shown or the server details dialog is opened
            // - discord rpc thread: handles rpc connection with discord
            //    - discord rich presence tasks: handles querying and displaying of game information, invoked on activity watcher events
            // - watcher task: runs activity watcher + waiting for roblox to close, terminates when it has

            var watcher = new Watcher();

            Task.Run(watcher.Run).ContinueWith(t => 
            {
                App.Logger.WriteLine(LOG_IDENT, "Watcher task has finished");

                watcher.Dispose();

                if (t.IsFaulted)
                {
                    App.Logger.WriteLine(LOG_IDENT, "An exception occurred when running the watcher");

                    if (t.Exception is not null)
                        App.FinalizeExceptionHandling(t.Exception);
                }

                App.Terminate();
            });
        }

        public static void LaunchMultiInstanceWatcher()
        {
            const string LOG_IDENT = "LaunchHandler::LaunchMultiInstanceWatcher";

            App.Logger.WriteLine(LOG_IDENT, "Starting multi-instance watcher");

            Task.Run(MultiInstanceWatcher.Run).ContinueWith(t =>
            {
                App.Logger.WriteLine(LOG_IDENT, "Multi instance watcher task has finished");

                if (t.IsFaulted)
                {
                    App.Logger.WriteLine(LOG_IDENT, "An exception occurred when running the multi-instance watcher");

                    if (t.Exception is not null)
                        App.FinalizeExceptionHandling(t.Exception);
                }

                App.Terminate();
            });
        }

        public static void LaunchBackgroundUpdater()
        {
            const string LOG_IDENT = "LaunchHandler::LaunchBackgroundUpdater";

            // Activate some LaunchFlags we need
            App.LaunchSettings.QuietFlag.Active = true;
            App.LaunchSettings.NoLaunchFlag.Active = true;

            App.Logger.WriteLine(LOG_IDENT, "Initializing bootstrapper");

            App.Bootstrapper = new Bootstrapper(LaunchMode.Player)
            {
                MutexName = "Bloxstrap-BackgroundUpdater",
                QuitIfMutexExists = true
            };

            CancellationTokenSource cts = new CancellationTokenSource();

            Task.Run(() =>
            {
                App.Logger.WriteLine(LOG_IDENT, "Started event waiter");
                using (EventWaitHandle handle = new EventWaitHandle(false, EventResetMode.AutoReset, "Bloxstrap-BackgroundUpdaterKillEvent"))
                    handle.WaitOne();

                App.Logger.WriteLine(LOG_IDENT, "Received close event, killing it all!");
                App.Bootstrapper.Cancel();
            }, cts.Token);

            Task.Run(App.Bootstrapper.Run).ContinueWith(t =>
            {
                App.Logger.WriteLine(LOG_IDENT, "Bootstrapper task has finished");
                cts.Cancel(); // stop event waiter

                if (t.IsFaulted)
                {
                    App.Logger.WriteLine(LOG_IDENT, "An exception occurred when running the bootstrapper");

                    if (t.Exception is not null)
                        App.FinalizeExceptionHandling(t.Exception);
                }

                App.Terminate();
            });

            App.Logger.WriteLine(LOG_IDENT, "Exiting");
        }
        public static void LaunchOverlay()
        {
            const string LOG_IDENT = "LaunchHandler::LaunchOverlay";
            App.Logger.WriteLine(LOG_IDENT, "=== STARTING OVERLAY-ONLY MODE ===");

            var overlayWindow = new Bloxstrap.UI.Elements.Overlay.OverlayWindow();
            overlayWindow.Show();
            App.Logger.WriteLine(LOG_IDENT, "Overlay window shown. UI thread is active.");

            Task.Run(() =>
            {
                try
                {
                    App.Logger.WriteLine(LOG_IDENT, "Monitoring thread started. Waiting for Roblox to appear...");

                    bool windowWasSeen = false; // Флаг: видели ли мы игровое окно хотя бы раз
                    int noWindowCount = 0;
                    int waitCount = 0;

                    while (waitCount < 60) // Ждем максимум 60 секунд (на случай долгих загрузок)
                    {
                        var processes = Process.GetProcessesByName("RobloxPlayerBeta")
                                               .Concat(Process.GetProcessesByName("RobloxStudioBeta"))
                                               .ToArray();

                        // ВАРИАНТ А: Процесс полностью исчез из системы
                        if (processes.Length == 0)
                        {
                            if (windowWasSeen)
                            {
                                App.Logger.WriteLine(LOG_IDENT, "SUCCESS: Roblox process completely closed. Closing overlay.");
                                break;
                            }
                            // Если окно еще не было видно, просто продолжаем ждать его появления
                        }
                        else
                        {
                            // ВАРИАНТ Б: Процесс есть, проверяем наличие окна
                            bool hasActiveGameWindow = false;
                            foreach (var p in processes)
                            {
                                try
                                {
                                    if (p.MainWindowHandle != IntPtr.Zero)
                                    {
                                        hasActiveGameWindow = true;
                                        break;
                                    }
                                }
                                catch { /* Игнорируем ошибки доступа к закрывающемуся процессу */ }
                            }

                            if (hasActiveGameWindow)
                            {
                                windowWasSeen = true; // Мы увидели игру!
                                noWindowCount = 0;    // Сбрасываем счетчик исчезновения
                            }
                            else
                            {
                                // Окна нет. Но считаем это только если мы УЖЕ видели игру ранее
                                if (windowWasSeen)
                                {
                                    noWindowCount++;
                                    if (noWindowCount >= 3) // 3 секунды без окна = выход
                                    {
                                        App.Logger.WriteLine(LOG_IDENT, "SUCCESS: Roblox window closed (efficiency mode or exited). Closing overlay.");
                                        break;
                                    }
                                }
                            }
                        }

                        Thread.Sleep(1000); // Проверяем каждую секунду
                        waitCount++;
                    }

                    if (waitCount >= 60)
                    {
                        App.Logger.WriteLine(LOG_IDENT, "WARNING: Timeout reached (60s). Closing overlay.");
                    }

                    // Закрываем окно из UI-потока
                    overlayWindow.Dispatcher.Invoke(() =>
                    {
                        App.Logger.WriteLine(LOG_IDENT, "Executing overlayWindow.Close()");
                        overlayWindow.Close();
                    });
                }
                catch (Exception ex)
                {
                    App.Logger.WriteException(LOG_IDENT, ex);
                }
            });

            overlayWindow.Closed += (_, _) =>
            {
                App.Logger.WriteLine(LOG_IDENT, "Overlay window Closed event fired. Terminating overlay process.");
                App.Terminate();
            };
        }
    }
}

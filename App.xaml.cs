using System.Linq;
using System.Threading;
using System.Windows;

namespace BranchTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Static Fields

        // variable for mutex created by the application
        private static Mutex mutex;

        #endregion

        #region Public Methods and Operators

        public static bool CanRun()
        {
            if (mutex == null) mutex = new Mutex(false, "DE45C487-F75A-4EED-86F6-BA4D0FF1FE6E");
            var mutexIsNew = false;
            try
            {
                // Signal Mutex and return true if the Mutex did not exist yet.
                mutexIsNew = mutex.WaitOne(0, true);
                if (!mutexIsNew) mutex = null;
            }
            catch (AbandonedMutexException)
            {
                // Mutex was not released properly
                // if this results in this exception, the mutex owner now is this process
                mutexIsNew = true;
            }
            return mutexIsNew;
        }

        public static void ReleaseMutex()
        {
            if (mutex != null)
            {
                mutex.ReleaseMutex();
                mutex = null;
            }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override void OnExit(ExitEventArgs e)
        {
            ReleaseMutex();
            base.OnExit(e);
        }

        /// <inheritdoc />
        protected override void OnStartup(StartupEventArgs e)
        {
            if (!CanRun()) return;

            var lowerArgs = e.Args.Select(a => a.ToLowerInvariant());

            if (!e.Args.Any() || lowerArgs.Contains("branch"))
            {
                var mw = new BranchWindow();
                mw.Show();
            }
            else if (lowerArgs.Contains("stash"))
            {
                var mw = new StashWindow();
                mw.Show();
            }
        }

        #endregion
    }
}
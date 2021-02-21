using projektlabor.noah.planmeldung.Properties.langs;
using System.Windows;

namespace projektlabor.noah.planmeldung.windows
{
    public partial class MainWindow : Window
    {

        /// <summary>
        /// Holds the task that backups all data and executes all background-processes
        /// </summary>
        private BackupTask backupTask;

        #region Event-handlers

        /// <summary>
        /// Executes when the backup task updates the loading infos
        /// </summary>
        /// <param name="info">The information that should be displayed</param>
        private void OnBackupInfo(string info) 
        {
            // Displays the loading animation
            this.DisplayLoading(info);
        }

        /// <summary>
        /// Executes when the backup task sucessfully finishes
        /// </summary>
        private void OnBackupEnd()
        {
            // Hides the loading window
            this.CloseOverlay();
        }

        /// <summary>
        /// Executes when the backup task failes to update
        /// </summary>
        /// <param name="error">The error</param>
        private void OnBackupError(string error)
        {
            // Checks if the error is fatal
            if(error == null)
            {
                this.DisplayFatalError();
                return;
            }

            // Displays the error
            this.DisplayInfo(Lang.backup_error, Lang.backup_error_disclaimer + error, () =>
            {
                // Closes the loading to not distract the main checker
                this.CloseOverlay();

                // Retries the task
                this.backupTask.RetrieTask();
            },Lang.backup_error_retry);
        }

        #endregion

        #region Actions

        /// <summary>
        /// Executes when the backup handler asks to run the upload tasks
        /// </summary>
        /// <returns>If the main window is ready for the backup handler</returns>
        private bool IsWindowReadyForBackup()
        {
            // Checks if the overlay is visible
            return !this.Overlay.Visibility.Equals(Visibility.Visible);
        }

        #endregion

    }
}

using projektlabor.noah.planmeldung.database;
using projektlabor.noah.planmeldung.Properties.langs;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using projektlabor.noah.planmeldung.utils;
using MySql.Data.MySqlClient;

namespace projektlabor.noah.planmeldung
{
    class BackupTask
    {

        /// <summary>
        /// Function to check if the main window is idling and therefore if
        /// the background tasks can run
        /// </summary>
        private readonly Func<bool> checkForAvailability;

        /// <summary>
        /// Holds the info and end callback.
        /// This callbacks are executed when the background task update the infos and
        /// stops successfully
        /// </summary>
        private readonly Action<string> onInfo;
        private readonly Action onEnd;

        /// <summary>
        /// Holds the error handler for when the backup task fails to execute
        /// </summary>
        private readonly Action<string> onError;

        /// <summary>
        /// The time when the backup task was last executed
        /// </summary>
        private DateTime lastExecuted;

        /// <summary>
        /// If the task has stopped
        /// </summary>
        private bool stopped;

        public BackupTask(Func<bool> checkForAvailability, Action<string> onInfo, Action onEnd, Action<string> onError)
        {
            this.checkForAvailability = checkForAvailability;
            this.onInfo = onInfo;
            this.onEnd = onEnd;
            this.onError = onError;
        }

        public void Start()
        {
            // Resets the last execution
            lastExecuted = DateTime.Now;

            // Goes as long as the program runs
            while (true)
            {
                // Checks if the task got stopped because of an error check
                if (this.stopped)
                {
                    Thread.Sleep(50);
                    continue;
                }

                // Checks if the last execution has passed the time limit
                if (DateTime.Now.Subtract(lastExecuted).TotalSeconds < Config.BACKUP_SCHEDULE_SECONDS)
                {
                    // Waits recheck
                    Thread.Sleep(1000 * 10);
                    continue;
                }

                // Checks if the main window is ready for a backup
                if (!this.checkForAvailability())
                {
                    // Waits to recheck
                    Thread.Sleep(1000 * 10);
                    continue;
                }

                // Executes the backup tasks and checks if it executed successfully
                if (this.Execute())
                    // Resets the time
                    lastExecuted = DateTime.Now;
            }
        }

        /// <summary>
        /// Restarts the task after failure
        /// </summary>
        public void RetrieTask()
        {
            // Starts the task
            this.stopped = false;
        }

        /// <summary>
        /// Executes when the backup task fires
        /// </summary>
        private bool Execute()
        {
            try
            {
                // Displays the next loading
                this.onInfo(Lang.backup_logout);

                // Logs out all persons
                Database.Instance.LogoutAllUsers();

                // Displays the next loading
                this.onInfo(Lang.backup_processing);

                // Deletes all user-accounts that wern't present for 4 weeks and wanted their accounts deleted. Removes all time spent data that has been collected more than 4 weeks ago
                Database.Instance.AutoDeleteAccounts();

                // Displays the next loading
                this.onInfo(Lang.backup_backup);

                // Grabs a backup from the database and send it to the backup-email
                var backup = Database.Instance.GetBackupAsString();

                // Name of the backup file
                string name = $"Backup-{DateTime.Now.ToString(@"dd\.MM\.yyyy HH\:mm")}";

                // Displays the next loading
                this.onInfo(Lang.backup_upload);

                // Creates the smtp client
                var smtpClient = new SmtpClient(Config.SMTP_ADDRESS)
                {
                    Port = Config.SMTP_PORT,
                    Credentials = new NetworkCredential(Config.SMTP_EMAIL, Config.SMTP_PASSWORD),
                    EnableSsl = true
                };
                // Creates the message
                MailMessage mm = new MailMessage(Config.SMTP_EMAIL, Config.SMTP_EMAIL, name, string.Empty);
                // Attaches the backup
                mm.Attachments.Add(new Attachment(backup.ToStream(), name + ".sql"));
                // Sends the email
                smtpClient.Send(mm);

                // Executes the sucessfull ended event
                this.onEnd();
                return true;
            }
            catch (SmtpException e)
            {
                // Stops the task execution to wait until the user approves of restarting
                this.stopped = true;

                // Checks if the connection failed
                if (e.StatusCode.Equals(SmtpStatusCode.GeneralFailure))
                    this.onError(Lang.backup_error_upload);
                // Checks if the credentials were invalid
                else if (e.StatusCode.Equals(SmtpStatusCode.TransactionFailed))
                    this.onError(Lang.backup_error_authenticate);
                else
                    // Displays the fatal error
                    this.DisplayInfo(Lang.main_error_fatal_title, Lang.main_error_fatal_text);
            }
            catch (MySqlException)
            {
                // Stops the task execution to wait until the user approves of restarting
                this.stopped = true;
                // Displays the error
                this.onError(Lang.main_database_error_connect_text);
            }
            catch
            {
                // Stops the task execution to wait until the user approves of restarting
                this.stopped = true;
                // Handles the fatal error
                this.onError(null);
            }

            return false;
        }

        private void DisplayInfo(string main_error_fatal_title, string main_error_fatal_text)
        {
            throw new NotImplementedException();
        }
    }
}

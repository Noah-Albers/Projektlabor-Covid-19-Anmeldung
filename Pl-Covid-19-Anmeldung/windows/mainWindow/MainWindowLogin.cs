using MySql.Data.MySqlClient;
using projektlabor.noah.planmeldung.database;
using projektlabor.noah.planmeldung.database.entities;
using projektlabor.noah.planmeldung.Properties.langs;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace projektlabor.noah.planmeldung.windows
{
    public partial class MainWindow : Window
    {

        /// <summary>
        /// Holds the currently selected user
        /// </summary>
        private UserEntity selectedLoginUser;

        /// <summary>
        /// Holds the currently selected timespan
        /// </summary>
        private TimeSpentEntity selectedLoginTime;

        /// <summary>
        /// Holds all form field elements that are used at the login form.
        /// This is used to autodelete all data from these forms.
        /// </summary>
        private readonly TextBox[] loginFieldGroup;

        #region Event-handlers

        /// <summary>
        /// Executes when the database fails to deliver the user informations for the available users
        /// </summary>
        private void OnLoginUserSelectError(Exception ex)
        {
            // Checks if the exception is a mysql exception
            if (ex.GetType() == typeof(MySqlException))
                // Displays the error
                this.DisplayInfo(
                    Lang.main_database_error_connect_title,
                    Lang.main_database_error_connect_user_text,
                    this.CloseOverlay,
                    Lang.main_popup_close
                );
            else
                // Displays the error
                this.DisplayFatalError();
        }

        /// <summary>
        /// Executes when the user selects another user with the login form
        /// </summary>
        /// <param name="user">The selected user</param>
        private void OnLoginUserSelectSelect(UserEntity user) => Task.Run(() =>
        {
            // Displays the loading
            this.DisplayLoading(Lang.main_login_select_loading);
            try
            {
                // Gets the remaining data
                var ts = Database.Instance.GetOpenTimeSpentFromUser(user.Id);

                // Checks if no unclosed sessions have been found
                if (ts == null)
                    // Creates a new login
                    ts = new TimeSpentEntity()
                    {
                        Start = DateTime.Now
                    };
                else
                    // Updates the stop-time
                    ts.Stop = DateTime.Now;

                // Updates the form
                this.Dispatcher.Invoke(() =>
                {
                    // Resets the form
                    this.LoginResetForm();

                    // Displays the login form
                    this.LoginDisplayUser(user, ts);

                    // Closes the loading
                    this.CloseOverlay();
                });
            }
            catch (MySqlException)
            {
                // Displays the error
                this.DisplayInfo(
                    Lang.main_database_error_connect_title,
                    Lang.main_database_error_connect_user_text,
                    this.CloseOverlay,
                    Lang.main_popup_close
                );
            }
            catch
            {
                // Displays the error
                this.DisplayFatalError();
            }
        });

        /// <summary>
        /// Executes when the user clicks the login button
        /// </summary>
        private void OnLoginButtonClick(object sender, RoutedEventArgs e) => Task.Run(() =>
        {
            // Displays the loading animation
            this.DisplayLoading(Lang.main_login_loading);
            try
            {
                //Log the user in
                Database.Instance.LoginUser(this.selectedLoginUser, this.selectedLoginTime);

                this.Dispatcher.Invoke(() =>
                {
                    // Closes the animation
                    this.CloseOverlay();
                    // Clears the form
                    this.LoginResetForm();
                });
            }
            catch (MySqlException)
            {
                // Displays the error
                this.DisplayInfo(
                    Lang.main_database_error_connect_title,
                    Lang.main_database_error_connect_user_text,
                    this.CloseOverlay,
                    Lang.main_popup_close
                );
            }
            catch
            {
                // Displays the error
                this.DisplayFatalError();
            }
        });

        /// <summary>
        /// Executes when the user clicks the logout button
        /// </summary>
        private void OnLogoutButtonClick(object sender, RoutedEventArgs e) => Task.Run(() =>
        {
            // Displays the loading animation
            this.DisplayLoading(Lang.main_login_loading_logout);
            try
            {
                //Log the user out
                Database.Instance.LogoutUser(this.selectedLoginTime);

                // Closes the overlay and clears the field
                this.Dispatcher.Invoke(() =>
                {
                    this.LoginResetForm();
                    this.CloseOverlay();
                });
            }
            catch (MySqlException)
            {
                // Displays the error
                this.DisplayInfo(
                    Lang.main_database_error_connect_title,
                    Lang.main_database_error_connect_user_text,
                    this.CloseOverlay,
                    Lang.main_popup_close
                );
            }
            catch
            {
                // Displays the error
                this.DisplayFatalError();
            }
        });

        /// <summary>
        /// Executes when the user clicks the button that should delete the login form data
        /// </summary>
        private void OnLoginDeleteButtonClick(object sender, RoutedEventArgs e) => this.LoginResetForm();

        #endregion

        #region Actions

        /// <summary>
        /// Tries to login the user using his rfid
        /// </summary>
        /// <param name="rfid">The rfid</param>
        private void LoginUserFromRFID(string rfid)
        {
            // Tries to get a user by it's rfid
            Task.Run(() =>
            {
                try
                {
                    // Tries to find a user
                    Tuple<UserEntity, TimeSpentEntity> fetchedUser = Database.Instance.GetUserByRFIDCode(rfid);

                    // Gets the values
                    var user = fetchedUser.Item1;
                    var time = fetchedUser.Item2;

                    // Checks if a user got found
                    if (user == null)
                    {
                        // Sends the error
                        this.Dispatcher.Invoke(() => this.DisplayInfo(
                            Lang.main_rfid_error_loading_title,
                            Lang.main_rfid_error_loading_text,
                            () => this.CloseOverlay(),
                            Lang.main_popup_close
                        ));
                        return;
                    }

                    // If the user was already logged in
                    bool isLogin = time == null;

                    // Checks if the user must be logged in or logged out
                    if (isLogin)
                    {
                        // Creates a new time
                        time = new TimeSpentEntity
                        {
                            Start = DateTime.Now
                        };

                        // Logs in the user
                        Database.Instance.LoginUser(user,time);
                    }
                    else
                    {
                        // Sets the stop time
                        time.Stop = DateTime.Now;

                        // Logs out the user
                        Database.Instance.LogoutUser(time);
                    }

                    // Closes the overlay and clears the fields
                    this.Dispatcher.Invoke(() => this.LoginResetForm());

                    // Displays the short animation
                    this.LoginStartDisplayCountdown(isLogin);
                }
                catch (MySqlException)
                {
                    // Displays the error
                    this.DisplayInfo(
                        Lang.main_database_error_connect_title,
                        Lang.main_database_error_connect_user_text,
                        this.CloseOverlay,
                        Lang.main_popup_close
                    );
                }
                catch
                {
                    // Displays the error
                    this.DisplayFatalError();
                }
            });
        }

        /// <summary>
        /// Displays an animation to indicate that the user has successfull been logged out or logged in
        /// </summary>
        /// <param name="loggedIn">If the user got logged in or out</param>
        private void LoginStartDisplayCountdown(bool loggedIn)
        {
            // Starts a new task to not block the main application
            Task.Run(() =>
            {
                // Iterates over the animation ten times with a delay of .2s
                for(int i = 0; i < 5; i++)
                {
                    // Displays the animation
                    this.DisplayInfo(loggedIn? Lang.main_login_rfid_success_title_login : Lang.main_login_rfid_success_title_logout, new string('.',i), null, null, false);
                    // Waits for a short while
                    Thread.Sleep(500);
                }

                // Closes the overlay
                this.CloseOverlay();
            });
        }

        /// <summary
        /// Displays the given user in the current login form
        /// </summary>
        /// <param name="user">The user that should be displayed</param>
        /// <param name="spenttime">The time span entity that should be used</param>
        private void LoginDisplayUser(UserEntity user, TimeSpentEntity spenttime)
        {
            // Sets the selected user and spenttime
            this.selectedLoginUser = user;
            this.selectedLoginTime = spenttime;

            // Updates all fields
            this.FieldFirstname.Text = user.Firstname;
            this.FieldLastname.Text = user.Lastname;
            this.FieldStart.Text = spenttime.Start.ToString();
            this.FieldEnd.Text = spenttime.Stop == default ? string.Empty : spenttime.Stop.ToString();

            // Checks if the user can login or logout
            if (spenttime.Stop == default)
                // Aktives the login button
                this.ButtonLogin.IsEnabled = true;
            else
            {
                // Shows the logout button and hides the login button
                this.ButtonLogout.Visibility = Visibility.Visible;
                this.ButtonLogin.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Resets the login form back to the default login screen
        /// </summary>
        private void LoginResetForm()
        {

            // Resets the buttons
            this.ButtonLogin.Visibility = Visibility.Visible;
            this.ButtonLogin.IsEnabled = false;
            this.ButtonLogout.Visibility = Visibility.Collapsed;

            // Resets all login fields
            foreach (TextBox field in this.loginFieldGroup)
                field.Text = string.Empty;

            // Unselects anything
            this.selectedLoginTime = null;
            this.selectedLoginUser = null;
        }

        #endregion
    }
}

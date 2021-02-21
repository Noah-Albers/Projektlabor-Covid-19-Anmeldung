using MySql.Data.MySqlClient;
using projektlabor.noah.planmeldung.database.entities;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows;
using projektlabor.noah.planmeldung.utils;
using projektlabor.noah.planmeldung.database;
using projektlabor.noah.planmeldung.Properties.langs;

namespace projektlabor.noah.planmeldung.windows
{
    public partial class MainWindow : Window
    {

        /// <summary>
        /// Holds the currently stored user in the edit form of the admin panel
        /// </summary>
        private ExtendedUserEntity AdminPanelStoredUser;

        #region Event-handlers

        /// <summary>
        /// Executes when the button to open the admin panel gets clicked
        /// </summary>
        private void OnAdminPanelOpenButtonClick(object sender, RoutedEventArgs e)
        {
            // Resets the form
            this.ButtonPassacceptAdminPanel.IsEnabled = false;
            this.PassinputAdminPanel.Clear();

            // Hides the other overlays
            this.CloseOverlay();

            // Shows the adminpanel-login overlay
            this.Overlay.Visibility =
            this.OverlayAdminPanelLogin.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Executes when the user types the password into the password field for the admin panel
        /// </summary>
        private void OnAdminPanelLoginPassinputChange(object sender, RoutedEventArgs e)
        {
            // Updates the enabled state of the button depending on if the password was correct
            this.ButtonPassacceptAdminPanel.IsEnabled = this.PassinputAdminPanel.Password.Equals(Config.ADMIN_PASSWORD);
        }

        /// <summary>
        /// Executes when the user has successfully input the password and clicked the open admin panel button.
        /// </summary>
        private void OnAdminPanelLoginClick(object sender, RoutedEventArgs e)
        {
            this.CloseOverlay();

            // Shows the adminpanel overlay
            this.Overlay.Visibility =
            this.OverlayAdminPanel.Visibility = Visibility.Visible;

            // Clears any previously input profile data
            this.AdminPanelStoredUser = null;
            this.FormEditProfile.ResetForm();
            this.FormEditProfile.IsEnabled = false;
        }

        /// <summary>
        /// Executes when the user search request fails
        /// </summary>
        /// <param name="e">The exception</param>
        private void OnAdminPanelEditProfileError(Exception ex)
        {
            // Checks the type
            if (ex.GetType() == typeof(MySqlException))
                this.DisplayInfo
                (
                    Lang.main_database_error_connect_title,
                    Lang.main_database_error_connect_text,
                    () => this.DisplayAdminPanel(),
                    Lang.main_popup_close
                );
            else
                this.DisplayFatalError();
        }

        /// <summary>
        /// Executes when the user selects another user to edit the profile
        /// </summary>
        /// <param name="selected">The selected user</param>
        private void OnAdminPanelEditProfileSelectUser(UserEntity selected) => Task.Run(() =>
        {
            // Displays the loading
            this.DisplayLoading(Lang.main_admin_edituser_select_loading);

            try
            {
                // Gets the user
                var user = Database.Instance.GetUser(selected);

                // Checks if no user got found
                if (user == null)
                {
                    // Displays the error
                    this.DisplayInfo(
                        Lang.main_admin_edituser_select_error_not_found_title,
                        Lang.main_admin_edituser_select_error_not_found_text,
                        () => this.DisplayAdminPanel(),
                        Lang.main_popup_close
                    );
                    return;
                }

                this.Dispatcher.Invoke(() =>
                {
                    // Shows the admin panel
                    this.DisplayAdminPanel();

                    // Inserts the user
                    this.FormEditProfile.UserInput = user;

                    // Enables the edit form
                    this.FormEditProfile.IsEnabled = this.ButtonEditProfileSave.IsEnabled = true;

                    // Sets the selected user
                    this.AdminPanelStoredUser = user;
                });
            }
            catch (MySqlException)
            {
                // Displays the error
                this.DisplayInfo(
                    Lang.main_database_error_connect_title,
                    Lang.main_database_error_connect_text,
                    ()=>this.DisplayAdminPanel(),
                    Lang.main_popup_close
                );
            }
            catch {
                // Displays the error
                this.DisplayFatalError();
            }
        });

        /// <summary>
        /// Executes once the user clicks on the button to save the changes on the profile
        /// </summary>
        private void OnAdminPanelEditProfileFinishClicked(object sender, RoutedEventArgs e)
        {
            // Updates and checks if all field got set
            if (!this.FormEditProfile.UpdateFields())
                return;

            // Gets the user
            var user = this.FormEditProfile.UserInput;
            user.Id = this.AdminPanelStoredUser.Id;

            // Displays the loading
            this.DisplayLoading(Lang.main_admin_edituser_loading);

            Task.Run(() =>
            {
                try
                {
                    // Updates the user
                    int status = Database.Instance.EditUser(user);

                    // Checks if the name already existed
                    if (status != 0)
                        this.DisplayInfo(
                            Lang.main_register_error_title,
                            status == 1 ? Lang.main_register_error_name : Lang.main_register_error_rfid,
                            ()=>this.DisplayAdminPanel(),
                            Lang.main_popup_close
                        );
                    else
                        // Closes the loading
                        this.Dispatcher.Invoke(()=>this.DisplayAdminPanel());
                }
                catch(MySqlException)
                {
                    // Displays the error
                    this.DisplayInfo(
                        Lang.main_database_error_connect_title,
                        Lang.main_database_error_connect_text,
                        () => this.DisplayAdminPanel(false),
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

        #endregion

        #region Actions

        /// <summary>
        /// Displays the admin panel
        /// </summary>
        private void DisplayAdminPanel(bool resetStoredUser = true) => this.Dispatcher.Invoke(() =>
        {
            this.CloseOverlay();
            this.OverlayAdminPanel.Visibility = this.Overlay.Visibility = Visibility.Visible;
            // Checks if the stored user should be reset
            if (resetStoredUser)
            {
                // Resets the form
                this.FormEditProfile.ResetForm();
                this.AdminPanelStoredUser = null;
                // Disables the form and button
                this.FormEditProfile.IsEnabled = this.ButtonEditProfileSave.IsEnabled = false;
            }
        });

        /// <summary>
        /// Tries to grab the user and display it inside the admin-edit user panel
        /// </summary>
        /// <param name="rfid">The rfid that shall be searched for</param>
        private void AdminPanelSelectUserByRFID(string rfid)
        {
            // Tries to get a user by it's rfid
            Task.Run(() =>
            {
                try
                {
                    // Gets the user
                    var user = Database.Instance.GetUser(rfid);

                    // Checks if a user got found
                    if (user == null)
                    {
                        // Shows the error
                        this.DisplayInfo(
                           Lang.main_rfid_error_loading_title,
                           Lang.main_rfid_error_loading_text,
                           () => this.DisplayAdminPanel(),
                           Lang.main_popup_close
                       );
                        return;
                    }

                    // Updates the user
                    this.Dispatcher.Invoke(() =>
                    {
                        // Shows the admin panel
                        this.DisplayAdminPanel();
                        // Inserts the user
                        this.AdminPanelStoredUser = user;
                        this.FormEditProfile.UserInput = user;
                        // Enables the edit form
                        this.FormEditProfile.IsEnabled = this.ButtonEditProfileSave.IsEnabled = true;
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
        }

        #endregion
    }
}

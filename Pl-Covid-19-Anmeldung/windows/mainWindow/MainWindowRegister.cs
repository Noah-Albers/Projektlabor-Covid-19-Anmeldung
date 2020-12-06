using System.Threading.Tasks;
using System.Windows;
using projektlabor.noah.planmeldung.database;
using projektlabor.noah.planmeldung.Properties.langs;
using MySql.Data.MySqlClient;

namespace projektlabor.noah.planmeldung.windows
{
    public partial class MainWindow : Window
    {

        #region Event-handlers

        /// <summary>
        /// Executes when the user checks or unchecks the checkbox to accept the rules.
        /// 
        /// Enables or diables the button in that context
        /// </summary>
        private void OnRegistrationRuleCheckboxChanged(object sender, RoutedEventArgs e) => this.ButtonRegisterRegister.IsEnabled = this.CheckboxRegAcceptRules.IsChecked.Value;

        /// <summary>
        /// Executes when the user clicks the register button
        /// </summary>
        private void OnRegistrationButtonClicked(object sender, RoutedEventArgs e)
        {
            // Checks if all register field match their case
            if (!this.FormRegister.UpdateFields())
                return;

            // Displays the loading
            this.DisplayLoading(Lang.main_register_loading);

            // Gets the inserted user
            var user = this.FormRegister.UserInput;
            
            Task.Run(() =>
            {
                try
                {
                    // Tries to register the user
                    int status = Database.Instance.RegisterUser(user);
                    
                    this.Dispatcher.Invoke(() =>
                    {
                        // Checks if the register was successful
                        if (status != 0)
                            this.DisplayInfo(
                                Lang.main_register_error_title,
                                status==1?Lang.main_register_error_name : Lang.main_register_error_rfid,
                                this.CloseOverlay,
                                Lang.main_popup_close
                            );
                        else
                        {
                            // Resets the form
                            this.FormRegister.ResetForm();
                            this.CheckboxRegAcceptRules.IsChecked = false;
                            this.ButtonRegisterRegister.IsEnabled = false;
                            // Displays the info
                            this.DisplayInfo(
                                Lang.main_register_success_title,
                                Lang.main_register_success_text,
                                this.CloseOverlay,
                                Lang.main_popup_close,
                                false
                            );
                        }
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

        /// <summary>
        /// Executes when the register form gets reset
        /// </summary>
        private void OnRegistrationResetClicked(object sender, RoutedEventArgs e)
        {
            // Updates the button and checkboxs
            this.ButtonRegisterRegister.IsEnabled = false;
            this.CheckboxRegAcceptRules.IsChecked = false;

            // Resets the form
            this.FormRegister.ResetForm();
        }

        #endregion
    }
}

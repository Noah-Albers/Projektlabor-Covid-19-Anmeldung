using MySql.Data.MySqlClient;
using projektlabor.noah.planmeldung.Properties.langs;
using projektlabor.noah.planmeldung.database;
using projektlabor.noah.planmeldung.database.entities;
using projektlabor.noah.planmeldung.utils;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

/// <summary>
/// Different parts of the class have been outsources to other files inside "mainWindow/":
///     Login => MainWindowLogin.cs
///     Registration => MainWindowRegister.cs
///
/// All methods and variables in this outsources files will have something in their name that clarifies their origin.
/// 
/// Events from the form and external events will start with On<MethodName>
/// </summary>

namespace projektlabor.noah.planmeldung.windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Is used to select the serial port with the required esp32-rfid-scanner
        /// </summary>
        private RFIDReader rfidreader = new RFIDReader(".");

        /// <summary>
        /// Amount of times the logo has rotated. Used to animate an easter egg
        /// </summary>
        private int RotatedLogoTimes = 0;

        public MainWindow()
        {
            InitializeComponent();

            // Gets the login group
            this.loginFieldGroup = new TextBox[] { this.FieldStart, this.FieldEnd, this.FieldFirstname,this.FieldLastname };

            // Connects to the database using a new thread
            this.LoginToDatabase();
        }

        #region Event-handlers

        /// <summary>
        /// Executes when an rfid gets recevied from the esp32-rfid-scanner
        /// </summary>
        /// <param name="rfid">The received rfid</param>
        private void OnRFIDReceiv(string rfid) => this.Dispatcher.Invoke(() =>
        {
            // Gets which interfaces are visible
            bool over = this.Overlay.Visibility.Equals(Visibility.Visible);
            bool adm = this.OverlayAdminPanel.Visibility.Equals(Visibility.Visible);

            // Checks if an overlay is displayed and its not the admin panel
            if (over && !adm)
                return;

            // Checks if the register form handles the rfid
            if (this.FormRegister.OnRFIDReceive(rfid))
                return;

            // Checks if the admin panels edit form handles the rfid
            if (this.FormEditProfile.OnRFIDReceive(rfid))
                return;

            // Shows the loading window
            this.DisplayLoading(Lang.main_rfid_loading);

            // Tries to get a user by it's rfid
            Task.Run(() =>
            {
                try
                {

                    // Checks if the user should be inserted into the admin panel or the normal login
                    if (adm)
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
                    else
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

                        // Sets the new time if no previous time has been recorded
                        if (time == null)
                            time = new TimeSpentEntity
                            {
                                Start = DateTime.Now
                            };
                        else
                            time.Stop = DateTime.Now;

                        this.Dispatcher.Invoke(() =>
                        {
                            // Resets the login form
                            this.LoginResetForm();

                            // Hides the overlay
                            this.CloseOverlay();

                            // Displays the user
                            this.LoginDisplayUser(user, time);
                        });
                    }

                }
                catch(MySqlException)
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
        });

        /// <summary>
        /// Executes when the button that should connect the rfid-scanner gets clicked
        /// </summary>
        private void OnRFIDButtonClicked(object sender, RoutedEventArgs e) => this.StartRFIDReader();

        /// <summary>
        /// Executes when the logo gets clicked
        /// </summary>
        private void OnLogoClick(object sender, RoutedEventArgs e)
        {
            // Updates the rotated times
            this.RotatedLogoTimes++;

            // Checks if the easter-easter-egg got activated
            if (this.RotatedLogoTimes >= 41)
            {
                // Resets the rotated times
                this.RotatedLogoTimes = 0;

                // Creates the transformer
                SkewTransform skew = new SkewTransform();
                // Starts the animation
                this.ButtonLogo.RenderTransform = skew;
                skew.BeginAnimation(
                    SkewTransform.AngleXProperty,
                    new DoubleAnimation(0,180, new Duration(TimeSpan.FromMilliseconds(2000)))
                );
            }
            else
            {
                // Creates the transformer
                RotateTransform rt = new RotateTransform();

                // Gets the rotate direction
                bool forward = this.RotatedLogoTimes % 2 == 0;

                // Gets the max amount of rotations
                int max = 360 + 2;

                // Starts the animation
                this.ButtonLogo.RenderTransform = rt;
                rt.BeginAnimation(
                    RotateTransform.AngleProperty,
                    new DoubleAnimation(forward ? max : 0, forward ? 0 : max, new Duration(TimeSpan.FromMilliseconds(200)))
                );
            }

        }

        /// <summary>
        /// Event to hook when a button should close the overly
        /// </summary>
        private void OnButtonCloseClicked(object sender, RoutedEventArgs e) => this.CloseOverlay();

        #endregion

        #region Actions

        /// <summary>
        /// Used by seperated thread to start the background-rfid reader.
        /// Handles the successful connection to the rfid scanner and also the visuals.
        /// </summary>
        private void StartRFIDReader()
        {
            Task.Run(() =>
            {
                // Starts the rfid-reader
                this.rfidreader.Start(() =>
                // If no port was found
                {
                    // Displays the error
                    this.Dispatcher.Invoke(() =>
                        this.DisplayInfo(
                            Lang.main_rfid_error_port_not_found_title,
                            Lang.main_rfid_error_port_not_found_text,
                            this.CloseOverlay,
                            Lang.main_popup_close
                        )
                    );
                }, () =>
                // Once a port was found
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        // Removes the overlay
                        this.CloseOverlay();
                        // Hides the error button for the disconnected rfid-scanner
                        this.ButtonErrorRFID.Visibility = Visibility.Collapsed;
                    });
                }, () =>
                // Once the port got disconnected again
                {
                    // Displays the error
                    this.Dispatcher.Invoke(() =>
                        this.ButtonErrorRFID.Visibility = Visibility.Visible
                    );
                }, this.OnRFIDReceiv);
            });
        }

        /// <summary>
        /// Tries to log the program into the database.
        /// It will start all the visual effects for that.
        /// Once the login was successful, the onContinue method will be called.
        /// </summary>
        /// <param name="onContinue">Method to call when the login has successfully been done.</param>
        private void LoginToDatabase()
        {
            // Starts the loading animation
            this.Dispatcher.Invoke(() => this.DisplayLoading(Lang.main_database_connecting));

            // Starts a new thread to prevent blocking the wpf thread
            Task.Run(() =>
            {
                try
                {
                    // Connects to the database
                    new Database();

                    // Closes the loading
                    this.Dispatcher.Invoke(() => this.CloseOverlay());
                }
                catch (MySqlException e)
                {
                    // Defines if the error happend because of wrong credentials or because of a failed connection
                    bool wrongCredentials = e.Number == 0;

                    // Displays the error
                    this.DisplayInfo(
                        wrongCredentials?
                            Lang.main_database_error_credentials_title :
                            Lang.main_database_error_connect_title,
                        wrongCredentials?
                            Lang.main_database_error_credentials_text :
                            Lang.main_database_error_connect_text,
                        this.LoginToDatabase,
                        Lang.main_popup_retrie
                    );
                }
                // Catches any fatal exception
                catch
                {
                    // Displays the error
                    this.DisplayFatalError();
                }
            });
        }
        
        /// <summary>
        /// Displays the fatal error message to the user
        /// </summary>
        private void DisplayFatalError() => this.DisplayInfo
        (
            Lang.main_error_fatal_title,
            Lang.main_error_fatal_text
        );

        /// <summary>
        /// Shows the given text with the title.
        /// </summary>
        /// <param name="title">The overlay title</param>
        /// <param name="text">The overlay text</param>
        /// <param name="buttonText">The text on the button</param>
        /// <param name="clickevent">The event handler that should execute when the user clicks on the button</param>
        /// <param name="errorColor">If the color should indicate an error</param>
        private void DisplayInfo(string title, string text, Action clickevent = null, string buttonText = null,bool errorColor = true) => this.Dispatcher.Invoke(() =>
        {
            // Shows the info and text
            this.OverlayInfoText.Text = text;
            this.OverlayInfoTitle.Text = title;
            this.OverlayInfoTitle.Foreground = new SolidColorBrush(errorColor?Colors.Red:Colors.Cyan);

            // Makes the info visible
            this.CloseOverlay();
            this.OverlayInfo.Visibility = this.Overlay.Visibility = Visibility.Visible;

            // Checks if the button should be useable
            bool displayButton = clickevent != null && buttonText != null;

            // Updats the button visiblity
            this.ButtonOverlayClose.Visibility = displayButton ? Visibility.Visible : Visibility.Collapsed;

            // Updates the button
            if (displayButton)
            {
                // Removes all previous events
                this.ButtonOverlayClose.RemoveRoutedEventHandlers(Button.ClickEvent);

                // Appends the new one
                this.ButtonOverlayClose.Click += (handler, args) => clickevent();

                // Appends the text
                this.ButtonOverlayClose.Content = buttonText;
            }
        });

        /// <summary>
        /// Shows the given error with the title
        /// </summary>
        /// <param name="text">The text that should be displayed at the title</param>
        private void DisplayLoading(string text) => this.Dispatcher.Invoke(() =>
        {
            // Shows the title
            this.OverlayLoadingTitle.Text = text;
            // Makes the error visible
            this.CloseOverlay();
            this.OverlayLoading.Visibility = this.Overlay.Visibility = Visibility.Visible;
        });

        /// <summary>
        /// Closes the overlay by hidding it
        /// </summary>
        private void CloseOverlay() => this.Dispatcher.Invoke(() =>
        {
            // Hides the global overlay
            this.Overlay.Visibility = Visibility.Collapsed;

            // Hides the single parts
            this.OverlayInfo.Visibility =
            this.OverlayLoading.Visibility =
            this.OverlayAdminPanelLogin.Visibility =
            this.OverlayAdminPanel.Visibility = Visibility.Collapsed;
        });

        #endregion
    }
}

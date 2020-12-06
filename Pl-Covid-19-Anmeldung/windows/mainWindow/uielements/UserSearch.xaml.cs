using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using projektlabor.noah.planmeldung.database.entities;
using projektlabor.noah.planmeldung.database;

namespace projektlabor.noah.planmeldung.windows.mainWindow
{
    /// <summary>
    /// Interaction logic for UserSearch.xaml
    /// </summary>
    public partial class UserSearch : UserControl
    {

        /// <summary>
        /// Event handler that executes when the fetching for users fails
        /// </summary>
        public Action<Exception> OnError { get; set; }

        /// <summary>
        /// Event handler that executes when the a user got selected
        /// </summary>
        public Action<UserEntity> OnSelect { get; set; }
        
        /// <summary>
        /// Holds the current running task of fetching users from the database.
        /// Prevents the program form starting multiple tasks to do the same thing.
        /// </summary>
        private Task taskFetching;

        public UserSearch()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Executes when the button to search users gets clicked
        /// </summary>
        private void OnOpenButtonClick(object sender, RoutedEventArgs e)
        {
            // Resets the search bar
            this.FieldSearch.Text = string.Empty;

            // Shows the loading animation
            this.ViewLoading.Visibility = Visibility.Visible;

            // Hides the list view
            this.ViewList.Visibility = Visibility.Collapsed;

            // Checks if a request for all users is already running
            if (taskFetching != null)
                return;

            // Starts the task
            this.taskFetching = Task.Run(() =>
            {
                try
                {
                    // Gets the users
                    var users = Database.Instance.GetUsers();

                    // Updates the ui
                    this.Dispatcher.Invoke(() =>
                    {
                        // Removes all previous users
                        this.List.Items.Clear();

                        // Appends the new users
                        foreach (var u in users)
                        {
                            // Creates the list item
                            ListViewItem lvi = new ListViewItem
                            {
                                // Creates the display
                                Content = u,
                                FontSize = 20,
                                Foreground = new SolidColorBrush(Colors.White)
                            };

                            // Appends the user
                            this.List.Items.Add(lvi);
                        }
                        // Shows the user list
                        this.ViewList.Visibility = Visibility.Visible;
                        // Hides the loading animation
                        this.ViewLoading.Visibility = Visibility.Collapsed;

                        this.FieldSearch.Focus();
                    });
                }
                catch (Exception ex)
                {
                    // Displays the error
                    this.Dispatcher.Invoke(() =>
                    {
                        // Hides the popup
                        this.ButtonSelect.IsChecked = false;

                        // Executes the error handler
                        this.OnError(ex);
                    });
                }

                // Resets the task
                this.taskFetching = null;
            });
        }
    
        /// <summary>
        /// Executes when the user selects another user
        /// </summary>
        private void OnSelectUser(object server, SelectionChangedEventArgs e)
        {
            // Checks that an item got selected
            if (this.List.SelectedItem == null)
                return;

            // Closes the popup
            this.Popup.IsOpen = false;

            // Gets the selected user
            // Updates the selected user
            UserEntity user = (this.List.SelectedItem as ListViewItem).Content as UserEntity;

            // Executes the handler
            this.OnSelect(user);
        }

        /// <summary>
        /// Executes when the user types to search
        /// </summary>
        private void OnSearch(object sender, TextChangedEventArgs e)
        {
            // Iterates over all listed users
            foreach (var userItem in this.List.Items)
            {
                // Gets the listitem
                var itm = userItem as ListViewItem;

                // Checks if the user matches
                // Shows or hides the user
                itm.Visibility = (itm.Content as UserEntity).isMatching(this.FieldSearch.Text) ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}

using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows;
using projektlabor.noah.planmeldung.Properties.langs;

namespace projektlabor.noah.planmeldung.windows.mainWindow
{
    /// <summary>
    /// Interaction logic for LoginInput.xaml
    /// </summary>
    public partial class RegisterInput : UserControl
    {
        /// <summary>
        /// Bindings for the settings of the uc
        /// </summary>
        public string Title { get; set; }
        public int MaxLength { get; set; }
        public bool Optional { get; set; } = false;
        public string VerifyRegex { get; set; }
        public string TypeRegex { get; set; }
        public string ErrorOnOptional { get; set; } = Lang.ui_registerinput_opterror_default;
        public string ErrorOnRegex { get; set; } = Lang.ui_registerinput_regexerror_default;
        public bool IsWriteable { get; set; } = true;

        public string Text
        {
            get => (string)GetValue(dependencyProperty);
            set
            {
                this.previousText = value;
                SetValue(dependencyProperty, value);
            }
        }

        /// <summary>
        /// Dependency for the text of the fields
        /// </summary>
        public static readonly DependencyProperty dependencyProperty =
            DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(RegisterInput),
                new PropertyMetadata(string.Empty, new PropertyChangedCallback((obj,args)=> (obj as RegisterInput).InputField.Text = args.NewValue as string)));

        /// <summary>
        /// Holds the text before the user input to prevent invalid inputs
        /// </summary>
        private string previousText = string.Empty;


        public RegisterInput()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        /// <summary>
        /// Resets the input
        /// </summary>
        public void Reset()
        {
            this.Text = string.Empty;
            // Hides the error
            this.ErrorLabel.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Checks if all creteria are getting matched with the users current input.
        /// If an error occures the error field gets displayed
        /// </summary>
        /// <returns>
        /// Returns if all creteria are getting matched.
        /// 0 = Everything is fine
        /// 1 = Not optional and the field is empty
        /// 2 = Regex does not match
        /// </returns>
        public int UpdateResolvable()
        {
            // Checks if the the text has not been set and the optional is not required
            if (this.Optional && (this.Text==null || this.Text.Length == 0))
                return 0;

            // Checks if the field is not optional and the text has not been set
            if (!this.Optional && this.Text.Length == 0)
            {
                //Displays the error
                this.ErrorLabel.Content = this.ErrorOnOptional;
                this.ErrorLabel.Visibility = Visibility.Visible;
                return 1;
            }

            // If the regex matches or isn't set
            if (VerifyRegex != null && !new Regex(this.VerifyRegex).IsMatch(this.Text))
            {
                //Displays the error
                this.ErrorLabel.Content = this.ErrorOnRegex;
                this.ErrorLabel.Visibility = Visibility.Visible;
                return 2;
            }

            return 0;
        }

        /// <summary>
        /// Event that executes after the user typed a character
        /// </summary>
        private void OnType(object sender, TextChangedEventArgs e)
        {
            // If the new text is valid with the regex
            bool match = this.TypeRegex == null ? true : (new Regex(this.TypeRegex).IsMatch(this.InputField.Text));

            // Hides the error
            this.ErrorLabel.Visibility = Visibility.Collapsed;

            // Checks if the regex matches
            if (match)
                // Updates the text
                this.previousText = this.InputField.Text;
            else
            {
                // Gets the current cursor position
                int c = this.InputField.SelectionStart -1;

                // Resets the text
                this.InputField.Text = this.previousText;
                
                // Sets the cursor position
                this.InputField.SelectionStart=c;
            }
        }
    }
}

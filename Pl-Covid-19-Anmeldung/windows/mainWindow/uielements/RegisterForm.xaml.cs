using projektlabor.noah.planmeldung.database.entities;
using projektlabor.noah.planmeldung.uiElements;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace projektlabor.noah.planmeldung.windows.mainWindow
{
    /// <summary>
    /// Interaction logic for RegisterForm.xaml
    /// </summary>
    public partial class RegisterForm : UserControl
    {
        /// <summary>
        /// Data-bindings for all text-values on the form element
        /// </summary>
        public string DataFirstname { get => this.FieldFirstname.Text; set => this.FieldFirstname.Text=value; }
        public string DataLastname { get => this.FieldLastname.Text; set => this.FieldLastname.Text = value; }
        public string DataStreet { get => this.FieldStreet.Text; set => this.FieldStreet.Text = value; }
        public string DataStreetnumber { get => this.FieldStreetNumber.Text; set => this.FieldStreetNumber.Text = value; }
        public string DataLocation { get => this.FieldLocation.Text; set => this.FieldLocation.Text = value; }
        public string DataPlz { get => this.FieldPLZ.Text; set => this.FieldPLZ.Text = value; }
        public string DataTelephone { get => this.FieldTelephone.Text; set => this.FieldTelephone.Text = value; }
        public string DataEmail { get => this.FieldEmail.Text; set => this.FieldEmail.Text = value; }
        public string DataRFID { get => this.FieldRFID.Text; set => this.FieldRFID.Text = value; }
        public bool DataAutoDeleteAccount { get => this.CheckboxRegDelAccount.IsChecked.Value; set => this.CheckboxRegDelAccount.IsChecked = value; }

        /// <summary>
        /// All field value using the extended user entity
        /// </summary>
        public ExtendedUserEntity UserInput
        {
            get
            {
                return new ExtendedUserEntity
                {
                    AutoDeleteAccount = this.DataAutoDeleteAccount,
                    Email = this.DataEmail,
                    Firstname = this.DataFirstname,
                    Lastname=this.DataLastname,
                    TelephoneNumber=this.DataTelephone,
                    Location=this.DataLocation,
                    PLZ = this.DataPlz.Length > 0 ? int.Parse(this.DataPlz) : null as int?,
                    StreetNumber = this.DataStreetnumber,
                    Street=this.DataStreet,
                    RFID=this.DataRFID
                };
            }
            set
            {
                this.DataAutoDeleteAccount = value.AutoDeleteAccount;
                this.DataEmail = value.Email;
                this.DataFirstname = value.Firstname;
                this.DataLastname = value.Lastname;
                this.DataTelephone = value.TelephoneNumber;
                this.DataLocation = value.Location;
                this.DataPlz = value.PLZ.ToString();
                this.DataStreetnumber = value.StreetNumber;
                this.DataStreet = value.Street;
                this.DataRFID = value.RFID;
            }
        }

        /// <summary>
        /// Holds all form field elements that are used at the registration form.
        /// This is used to autodelete all data from these forms.
        /// </summary>
        private readonly RegisterInput[] fieldGroup;

        public RegisterForm()
        {
            InitializeComponent();
            this.DataContext = this;
            this.fieldGroup = new RegisterInput[] {
                this.FieldRFID,
                this.FieldTelephone,
                this.FieldEmail,
                this.FieldFirstname,
                this.FieldLastname,
                this.FieldLocation,
                this.FieldStreetNumber,
                this.FieldPLZ,
                this.FieldStreet
            };
        }


        /// <summary>
        /// Checks if every field has its value set if not optional and if all field match their case.
        /// If a field does not match their case, it displays the error
        /// </summary>
        public bool UpdateFields() => this.fieldGroup.Select(field => field.UpdateResolvable() == 0).Aggregate((a, b) => a && b);

        /// <summary>
        /// Resets the register form back to the default register screen
        /// </summary>
        public void ResetForm()
        {
            // Updates the button and checkboxs
            this.CheckboxRegDelAccount.IsChecked = false;

            // Clears all fields
            foreach (var field in this.fieldGroup)
                field.Reset();
        }
        
        /// <summary>
        /// Executes once an rfid gets received from the sensor
        /// </summary>
        /// <param name="rfid">The rfid</param>
        /// <returns>If the from has an open request and therefor can use the rfid</returns>
        public bool OnRFIDReceive(string rfid)
        {
            // Checks if the register overlay is open
            if (this.Popup.IsOpen)
            {
                // Inserts the rfid into the registration form
                this.FieldRFID.Text = rfid;

                // Closes the popup
                this.ButtonOpenRFID.IsChecked = false;

                return true;
            }
            return false;
        }

        /// <summary>
        /// Executes when the user clicks on the button to delete the rfid
        /// </summary>
        private void OnDeleteRFIDClick(object sender, RoutedEventArgs e)
        {
            this.FieldRFID.Text = string.Empty;
        }
    }
}

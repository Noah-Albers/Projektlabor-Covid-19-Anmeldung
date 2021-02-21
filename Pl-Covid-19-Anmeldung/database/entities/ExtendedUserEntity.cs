namespace projektlabor.noah.planmeldung.database.entities
{

    /// <summary>
    /// Holds the extended informations from a user like living place and email
    /// </summary>
    public class ExtendedUserEntity
    {
        public int? Id { get; set; } = null;
        public string Firstname { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int? PLZ { get; set; } = null;
        public string Street { get; set; } = string.Empty;
        public string StreetNumber { get; set; } = null;
        public string telephoneNumber { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string rfid { get; set; } = string.Empty;
        public bool AutoDeleteAccount { get; set; } = false;

        public string Email
        {
            get
            {
                return this.email;
            }
            set
            {
                this.email = value;
                if (value != null && value.Length <= 0)
                    this.email = null;
            }
        }
        public string TelephoneNumber
        {
            get
            {
                return this.telephoneNumber;
            }
            set
            {
                this.telephoneNumber = value;
                if (value != null && value.Length <= 0)
                    this.telephoneNumber = null;
            }
        }
        public string RFID
        {
            get
            {
                return this.rfid;
            }
            set
            {
                this.rfid = value;
                if (value != null && value.Length <= 0)
                    this.rfid = null;
            }
        }
    }
}

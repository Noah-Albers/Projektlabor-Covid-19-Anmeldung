namespace projektlabor.noah.planmeldung.database.entities
{
    class ContactInfoEntity
    {
        public int Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public int PLZ { get; set; }
        public string Location { get; set; }
        public string Street { get; set; }
        public int Housenumber { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
    }
}

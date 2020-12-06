using System;

namespace projektlabor.noah.planmeldung.database.entities
{
    class ContactEntity
    {
        public int InfectTimeId { get; set; }
        public DateTime InfectStarttime { get; set;  }
        public DateTime InfectEndtime { get; set; }
        public int ContactId { get; set; }
        public DateTime ContactStarttime { get; set; }
        public DateTime ContactEndtime { get; set; }
    }
}

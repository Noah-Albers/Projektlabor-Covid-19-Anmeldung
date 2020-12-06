using System;

namespace projektlabor.noah.planmeldung.database.entities
{
    class TimeSpentEntity
    {

        /// <summary>
        /// THe unique id of the timespent entity
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// When the user started to work
        /// </summary>
        public DateTime Start { get; set; }
        /// <summary>
        /// When the user ended his work
        /// Nullable
        /// </summary>
        public DateTime Stop { get; set; }
        /// <summary>
        /// If the day-end stopped the work
        /// </summary>
        public bool Enddisconnect { get; set; }
        /// <summary>
        /// The user's id this timespententity belongs to
        /// </summary>
        public int UserId { get; set; }
    }
}

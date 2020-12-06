namespace projektlabor.noah.planmeldung.database.entities
{
    public class UserEntity
    {
        /// <summary>
        /// The users unique id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The users firstname
        /// </summary>
        public string Firstname { get; set; }

        /// <summary>
        /// The users lastname
        /// </summary>
        public string Lastname  { get; set; }

        /// <summary>
        /// Checks if the user matches the search
        /// </summary>
        /// <param name="search"></param>
        /// <returns>True if the user matches; else false</returns>
        public bool isMatching(string search)
        {
            // Checks if the firstname or lastname matches
            return this.ToString().ToLower().Contains(search.ToLower());
        }
        public override string ToString()
        {
            return $"{this.Firstname} {this.Lastname}";
        }
    }
}

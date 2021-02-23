using MySql.Data.MySqlClient;
using projektlabor.noah.planmeldung.database.entities;
using System;
using System.Collections.Generic;
using System.Data;

namespace projektlabor.noah.planmeldung.database
{
    class Database
    {
        /// <summary>
        /// Holds the singleton instance
        /// </summary>
        public static Database Instance { get; private set; }

        /// <summary>
        /// Holds the connection to the database
        /// </summary>
        private MySqlConnection connection;

        public Database()
        {
            // Sets the singleton instance
            Instance = this;

            // Creates the database connection
            this.connection = new MySqlConnection($"Server=\"{Config.DB_ADDRESS}\"; database=\"{Config.DB_NAME}\"; UID=\"{Config.DB_USERNAME}\"; Password=\"{Config.DB_PASSWORD}\";");
            // Opens the connection
            this.connection.Open();
        }

        /// <summary>
        /// Searches a user by it's rfid code
        /// </summary>
        /// <param name="rfid">The rfid code that should be used to search the user</param>
        /// <returns>Null if no user got found; else the found user</returns>
        public Tuple<UserEntity,TimeSpentEntity> GetUserByRFIDCode(string rfid)
        {
            this.EnsureOpenConnection();
            // Gets the user
            UserEntity user;
            {
                // Create the query to get the user
                var query = new MySqlCommand("SELECT `firstname`,`lastname`,`id` FROM `user` WHERE `rfidcode`=@rfidcode", this.connection);
                query.Parameters.AddWithValue("@rfidcode", rfid);
                query.Prepare();

                // Gets the result
                var reader = query.ExecuteReader();

                // Checks if no user got found
                if (!reader.Read())
                {
                    // Exits and returns no user
                    reader.Close();
                    return Tuple.Create<UserEntity,TimeSpentEntity>(null,null);
                }

                // Creates the user
                user = new UserEntity
                {
                    Id = reader.GetInt32("id"),
                    Firstname = reader.GetString("firstname"),
                    Lastname = reader.GetString("lastname")
                };
                // Closes the query
                reader.Close();
            }

            // Gets the spenttimeentity
            TimeSpentEntity entity = this.GetOpenTimeSpentFromUser(user.Id);

            return Tuple.Create(user,entity);
        }

        /// <summary>
        /// Requests all users from the database
        /// </summary>
        /// <returns>all users</returns>
        public UserEntity[] GetUsers()
        {
            this.EnsureOpenConnection();

            // List with all users
            List<UserEntity> users = new List<UserEntity>();
            // Create the query
            var query = new MySqlCommand("SELECT `id`,`firstname`,`lastname` FROM `user`", this.connection);
            
            using(var reader = query.ExecuteReader())
            {
                // Maps every row to the user
                while (reader.Read())
                    // Creates the entity
                    users.Add(new UserEntity()
                    {
                        Id = reader.GetInt32("id"),
                        Firstname = reader.GetString("firstname"),
                        Lastname = reader.GetString("lastname")
                    });

                // Returns all users
                return users.ToArray();
            }
        }

        /// <summary>
        /// Searches the database for any open timespent entitys of the user.
        /// </summary>
        /// <param name="userId">The id of that user which spenttime entity should be searched for</param>
        /// <returns>The last spent time entity that hasn't been closed; if not found null</returns>
        public TimeSpentEntity GetOpenTimeSpentFromUser(int userId)
        {
            this.EnsureOpenConnection();
            // Creates the query
            var query = new MySqlCommand("SELECT * FROM `timespent` WHERE `userid`=@val1 AND `stop` IS NULL LIMIT 1", this.connection);
            // Inserts the values
            query.Parameters.AddWithValue("@val1", userId);
            query.Prepare();

            // Gets the result
            var reader = query.ExecuteReader();

            // Checks if a query got found
            if (!reader.Read())
            {
                reader.Close();
                return null;
            }

            // Creates the entity
            var timespent = new TimeSpentEntity()
            {
                Id=reader.GetInt32("id"),
                Enddisconnect=reader.GetBoolean("enddisconnect"),
                UserId=reader.GetInt32("userid"),
                Start=reader.GetDateTime("start")
            };


            // Closes the reader
            reader.Close();

            return timespent;
        }

        /// <summary>
        /// Sends the user to the database. Doesnt uses a userentity because this has not all properties that a registered user needs.
        /// This is done for dataprotection
        /// </summary>
        /// <returns>
        /// Retuns a status code that resembels the following
        /// 0 = Success
        /// 1 = The name has already been used
        /// 2 = The rfidCode has already been used
        /// </returns>
        public int RegisterUser(ExtendedUserEntity user)
        {
            this.EnsureOpenConnection();

            // Registers the user
            try
            {
                // Creates the query
                var query = new MySqlCommand("INSERT INTO `user` (`id`, `firstname`, `lastname`, `plz`, `location`, `street`, `housenumber`,`email`,`telephone`,`rfidcode`,`autodeleteaccount`,`createdate`) VALUES (NULL, @firstname, @lastname, @plz, @location, @street, @housenumber,@email,@telephone,@rfidcode,@autodelete,@createdate);", this.connection);

                // Inserts the values
                query.Parameters.AddWithValue("@firstname", user.Firstname);
                query.Parameters.AddWithValue("@lastname", user.Lastname);
                query.Parameters.AddWithValue("@plz", user.PLZ);
                query.Parameters.AddWithValue("@location", user.Location);
                query.Parameters.AddWithValue("@street", user.Street);
                query.Parameters.AddWithValue("@housenumber", user.StreetNumber);
                query.Parameters.AddWithValue("@email", user.Email);
                query.Parameters.AddWithValue("@telephone", user.TelephoneNumber);
                query.Parameters.AddWithValue("@rfidcode", user.RFID);
                query.Parameters.AddWithValue("@autodelete", user.AutoDeleteAccount);
                query.Parameters.AddWithValue("@createdate", DateTime.Now);
                query.Prepare();

                // Gets the result
                query.ExecuteNonQuery();

                // Exits without any error
                return 0;
            }catch(MySqlException e)
            {
                // Checks if the exception is a duplicated entry
                if (e.Number == 1062)
                {
                    // The identifier to seperate the column name
                    string identifier = "key '";

                    // Gets the message and the index of the identifier
                    string msg = e.Message;
                    int identifierPos = msg.LastIndexOf(identifier);

                    // Gets the column name
                    string colName = msg.Substring(identifierPos + identifier.Length, msg.Length - identifier.Length - identifierPos - 1);
                    
                    // Checks if the duplicated column was the name combination
                    if (colName.ToLower().Equals("uq_name"))
                        return 1;

                    // Checks if the duplicated column was the rfidcode
                    if (colName.ToLower().Equals("rfidcode"))
                        return 2;
                }

                // Parses on the exception
                throw e;
            }
        }

        /// <summary>
        /// Edits a users profile with the given changes
        /// </summary>
        /// <param name="user">The user and his new profile settings</param>
        /// /// <returns>
        /// Retuns a status code that resembels the following
        /// 0 = Success
        /// 1 = The name has already been used
        /// 2 = The rfidCode has already been used
        /// </returns>
        public int EditUser(ExtendedUserEntity user)
        {
            this.EnsureOpenConnection();

            // Updates the user
            try {
                // Creates the query
                var query = new MySqlCommand(@"
                    UPDATE `user` SET
                      `email`=@email,
                      `telephone`=@telephone,
                      `rfidcode`=@rfidcode,
                      `autodeleteaccount`=@autodelete,
                      `firstname`=@firstname,
                      `lastname`=@lastname,
                      `plz`=@plz,
                      `location`=@location,
                      `street`=@street,
                      `housenumber`=@housenumber
                    WHERE
                      `user`.`id` = @userid;
                ", this.connection);
                // Inserts the values
                query.Parameters.AddWithValue("@firstname", user.Firstname);
                query.Parameters.AddWithValue("@lastname", user.Lastname);
                query.Parameters.AddWithValue("@plz", user.PLZ);
                query.Parameters.AddWithValue("@location", user.Location);
                query.Parameters.AddWithValue("@street", user.Street);
                query.Parameters.AddWithValue("@housenumber", user.StreetNumber);
                query.Parameters.AddWithValue("@email", user.Email);
                query.Parameters.AddWithValue("@telephone", user.TelephoneNumber);
                query.Parameters.AddWithValue("@rfidcode", user.RFID);
                query.Parameters.AddWithValue("@autodelete", user.AutoDeleteAccount);
                query.Parameters.AddWithValue("@userid", user.Id);
                query.Prepare();

                // Executes the update
                query.ExecuteNonQuery();

                return 0;
            }catch(MySqlException e)
            {
                // Checks if the exception is a duplicated entry
                if (e.Number == 1062)
                {
                    // The identifier to seperate the column name
                    string identifier = "key '";

                    // Gets the message and the index of the identifier
                    string msg = e.Message;
                    int identifierPos = msg.LastIndexOf(identifier);

                    // Gets the column name
                    string colName = msg.Substring(identifierPos + identifier.Length, msg.Length - identifier.Length - identifierPos - 1);

                    // Checks if the duplicated column was the name combination
                    if (colName.ToLower().Equals("uq_name"))
                        return 1;

                    // Checks if the duplicated column was the rfidcode
                    if (colName.ToLower().Equals("rfidcode"))
                        return 2;
                }

                // Parses on the exception
                throw e;
            }
        }

        /// <summary>
        /// Gets extended profile of the user
        /// </summary>
        /// <param name="user">The user</param>
        /// <returns>The user's extended profile if found; else null</returns>
        public ExtendedUserEntity GetUser(UserEntity user)
        {
            this.EnsureOpenConnection();

            // Creates the query
            var query = new MySqlCommand(@"
                SELECT * FROM `user` WHERE `id`=@user LIMIT 1;
            ", this.connection);
            query.Parameters.AddWithValue("@user", user.Id);
            query.Prepare();

            // Executes the query
            using(var reader = query.ExecuteReader())
            {
                // Checks if the user got found
                if (!reader.Read())
                    return null;

                // Gets the user
                return new ExtendedUserEntity
                {
                    AutoDeleteAccount = reader.GetBoolean("autodeleteaccount"),
                    Email = reader.IsDBNull(reader.GetOrdinal("email")) ? null : reader.GetString("email"),
                    RFID = reader.IsDBNull(reader.GetOrdinal("rfidcode")) ? null : reader.GetString("rfidcode"),
                    TelephoneNumber = reader.IsDBNull(reader.GetOrdinal("telephone")) ? null : reader.GetString("telephone"),
                    Firstname = reader.GetString("firstname"),
                    Lastname = reader.GetString("lastname"),
                    Location = reader.GetString("location"),
                    StreetNumber = reader.GetString("housenumber"),
                    PLZ = reader.GetInt32("plz"),
                    Street = reader.GetString("street"),
                    Id=user.Id
                };

            }
        }

        /// <summary>
        /// Gets an extended profile of a user by its rfid
        /// </summary>
        /// <param name="rfid">The rfid</param>
        /// <returns>Null if no user got found</returns>
        public ExtendedUserEntity GetUser(string rfid)
        {
            this.EnsureOpenConnection();

            // Creates the query
            var query = new MySqlCommand(@"
                SELECT * FROM `user` WHERE `rfidcode`=@rfidcode LIMIT 1;
            ", this.connection);
            query.Parameters.AddWithValue("@rfidcode", rfid);
            query.Prepare();

            // Executes the query
            using (var reader = query.ExecuteReader())
            {
                // Checks if the user got found
                if (!reader.Read())
                    return null;

                // Gets the user
                return new ExtendedUserEntity
                {
                    AutoDeleteAccount = reader.GetBoolean("autodeleteaccount"),
                    Email = reader.IsDBNull(reader.GetOrdinal("email")) ? null : reader.GetString("email"),
                    RFID = reader.IsDBNull(reader.GetOrdinal("rfidcode")) ? null : reader.GetString("rfidcode"),
                    TelephoneNumber = reader.IsDBNull(reader.GetOrdinal("telephone")) ? null : reader.GetString("telephone"),
                    Firstname = reader.GetString("firstname"),
                    Lastname = reader.GetString("lastname"),
                    Location = reader.GetString("location"),
                    StreetNumber = reader.GetString("housenumber"),
                    PLZ = reader.GetInt32("plz"),
                    Street = reader.GetString("street"),
                    Id = reader.GetInt32("id")
                };

            }
        }

        /// <summary>
        /// Logs in the user with the timespent entity
        /// </summary>
        /// <param name="user">The user to log in</param>
        /// <param name="spent">The spent time entity</param>
        public void LoginUser(UserEntity user,TimeSpentEntity spent)
        {
            this.EnsureOpenConnection();
            // Creates the query
            var query = new MySqlCommand("INSERT INTO `timespent` (`id`, `start`, `stop`, `enddisconnect`, `userid`) VALUES (NULL, @start, NULL, '0', @userid)", this.connection);

            // Appends all values
            query.Parameters.AddWithValue("@start", spent.Start);
            query.Parameters.AddWithValue("@userid", user.Id);
            query.Prepare();

            // Executes the query
            query.ExecuteNonQuery();
        }

        /// <summary>
        /// Log out the user with the timespent entity
        /// </summary>
        /// <param name="spent">The spent time entity</param>
        public void LogoutUser(TimeSpentEntity spent)
        {
            // Creates the query
            var query = new MySqlCommand("UPDATE `timespent` SET `stop`=@stopped WHERE `id`=@id", this.connection);

            // Appends all values
            query.Parameters.AddWithValue("@stopped", spent.Stop);
            query.Parameters.AddWithValue("@id", spent.Id);
            query.Prepare();

            // Executes the query
            query.ExecuteNonQuery();
        }

        /// <summary>
        /// Logs out all users
        /// </summary>
        public void LogoutAllUsers()
        {
            this.EnsureOpenConnection();

            // Creates the query
            var query = new MySqlCommand(@"
                    UPDATE `timespent`
                    SET `stop`=@end,`enddisconnect`=1
                    WHERE
                        `stop` IS NULL
                        AND TIMESTAMPDIFF(hour,start,@end) >= 24;"
            , this.connection);

            // Appends all values
            query.Parameters.AddWithValue("@end", DateTime.Now);
            query.Prepare();

            // Executes the query
            query.ExecuteNonQuery();
        }

        /// <summary>
        /// Autodeletes all accounts of the user that have selected that checkbox and have not logged in
        /// in the past four weeks
        /// </summary>
        public void AutoDeleteAccounts()
        {
            this.EnsureOpenConnection();

            // Gets the date from 4 weeks ago
            DateTime preDate = DateTime.Now.AddDays(-7 * 4);

            // Deletes all users that have not logged in the past four weeks an
            {

                // Creates the query
                var query = new MySqlCommand(@"
                    CREATE TEMPORARY TABLE IF NOT EXISTS `oldUsers` AS (
	                    SELECT
		                    `o`.`userid` as `id`
		                    FROM
		                    `timespent` `o`
		                    LEFT JOIN `timespent` `b` ON
		                    `o`.`userid` = `b`.`userid` AND `o`.`stop` < `b`.`stop`
		                    JOIN `user` ON `o`.`userid` = `user`.`id`
		                    WHERE
		                    `b`.`stop` IS NULL AND `o`.`stop` IS NOT NULL AND `o`.`stop` AND `o`.`stop` < @date
                    );

                    DELETE FROM `user`
	                    WHERE  `user`.`autodeleteaccount` AND(
			                    `user`.`id` IN(SELECT `id` FROM `oldUsers`) OR(
                                    (
                                        SELECT COUNT(*) FROM `timespent`


                                        WHERE
					                    `user`.`id` = `timespent`.`userid`
				                    ) = 0

                                    and `user`.`createdate` < @date
                                )
		                    );

                DROP TABLE IF EXISTS `oldUsers`;
                ", this.connection);

                // Prepares the value
                query.Parameters.AddWithValue("@date", preDate);
                query.Prepare();

                // Executes the query
                query.ExecuteNonQuery();
            }

            // Deletes all time spent entries that are older than 4 weeks
            {
                // Creates the query
                var query = new MySqlCommand(@"
                    DELETE FROM `timespent` WHERE `stop` < @date;
                ", this.connection);

                // Prepares the value
                query.Parameters.AddWithValue("@date", preDate);
                query.Prepare();

                // Executes the query
                query.ExecuteNonQuery();
            }

            // Deletes all time spent entries that have no user (Unsually overkill, but just a safty measure)
            {
                var query = new MySqlCommand(@"
                    CREATE TEMPORARY TABLE IF NOT EXISTS  `oldTimes` AS (
	                    SELECT 
			                    `timespent`.`id`
		                    FROM
			                    `timespent` LEFT JOIN
			                    `user` ON `timespent`.`userid` = `user`.`id`
		
		                    WHERE
			                    `user`.`id` IS NULL
                    );

                    DELETE FROM `timespent`
                    WHERE
                        `timespent`.`id` IN (SELECT 
                            *
                        FROM
                            `oldTimes`);
    
                    DROP TABLE IF EXISTS `oldTimes`;
                ", this.connection);

                // Executes the query
                query.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Returns the exportet database as a string
        /// </summary>
        public string GetBackupAsString()
        {
            this.EnsureOpenConnection();

            // Creates the mysql command
            using (MySqlCommand cmd = new MySqlCommand(string.Empty,this.connection))
                // Creates the backup con
                using (MySqlBackup mb = new MySqlBackup(cmd))
                    return mb.ExportToString();
        }

        /// <summary>
        /// Searches all persons that had at some point contact with the infected person (Or up to 15 minutes after the infected person had left)
        /// </summary>
        /// <param name="infected">The infected person</param>
        public List<ContactInfoEntity> GetContactPersons(UserEntity infected)
        {
            this.EnsureOpenConnection();

            // Query to get all persons that had in some way contact with the infected person
            var query = new MySqlCommand(@"
                SELECT DISTINCT
                    u.id, u.firstname, u.lastname, u.plz, u.location, u.street, u.housenumber, u.telephone, u.email
                FROM
                    timespent i
                JOIN timespent c ON
                    i.userid != c.userid AND ADDTIME(
                        CASE WHEN i.stop IS NULL THEN @end ELSE i.stop
                    END,
                    '1500'
                ) >= c.start AND i.start <=(
                    CASE WHEN c.stop IS NULL THEN @end ELSE c.stop
                END
                )
                JOIN user u ON u.id=c.userid
                WHERE
                    i.userid = @user;
            ", this.connection);

            query.Parameters.AddWithValue("@end", DateTime.Now);
            query.Parameters.AddWithValue("@user", infected.Id);
            query.Prepare();

            // Holds all persons that had contact with the infected person
            var contacts = new List<ContactInfoEntity>();

            // Gets all timestemps
            using (var reader = query.ExecuteReader())
                while (reader.Read())
                    contacts.Add(new ContactInfoEntity
                    {
                        Id = reader.GetInt32("id"),
                        Email = reader.IsDBNull(reader.GetOrdinal("email")) ? null : reader.GetString("email"),
                        Telephone = reader.IsDBNull(reader.GetOrdinal("telephone")) ? null : reader.GetString("telephone"),
                        Firstname = reader.GetString("firstname"),
                        Lastname = reader.GetString("lastname"),
                        Housenumber = reader.GetInt32("housenumber"),
                        Location = reader.GetString("location"),
                        PLZ = reader.GetInt32("plz"),
                        Street = reader.GetString("street")
                    });

            return contacts;
        }

        /// <summary>
        /// Searches all contacts that a person had at some point with the infected person (Or up to 15 minutes after the infected person had left)
        /// </summary>
        /// <param name="infected">The infected person</param>
        public List<ContactEntity> GetContacts(UserEntity infected)
        {
            this.EnsureOpenConnection();

            // Query to get all overlapping timespans with the infected person.
            // The id is a unique id for a specific timespan
            var query = new MySqlCommand(@"
                SELECT
                    i.id AS 'Id',
                    i.start AS 'Istart',
                    (CASE WHEN i.stop IS NULL THEN @end ELSE i.stop END) AS 'Istop',
                    c.userid AS 'Cuserid',
                    c.start AS 'Cstart',
                    (CASE WHEN c.stop IS NULL THEN @end ELSE c.stop END) AS 'CStop'
                FROM
                    timespent i
                JOIN timespent c ON
                    i.userid != c.userid AND ADDTIME(CASE WHEN i.stop IS NULL THEN @end ELSE i.stop END, '1500') >= c.start AND i.start <= (CASE WHEN c.stop IS NULL THEN @end ELSE c.stop END)
                WHERE
                    i.userid = @user;    
            ", this.connection);
            query.Parameters.AddWithValue("@end", DateTime.Now);
            query.Parameters.AddWithValue("@user", infected.Id);
            query.Prepare();

            // Holds all contacts that a person had contact with the infected person 
            var contacts = new List<ContactEntity>();

            // Gets all contacts
            using (var reader = query.ExecuteReader())
                while (reader.Read())
                    contacts.Add(new ContactEntity
                    {
                        ContactId=reader.GetInt32("Cuserid"),
                        InfectTimeId=reader.GetInt32("Id"),
                        InfectStarttime=reader.GetDateTime("Istart"),
                        InfectEndtime=reader.IsDBNull(reader.GetOrdinal("Istop")) ? DateTime.Now : reader.GetDateTime("Istop"),
                        ContactEndtime= reader.IsDBNull(reader.GetOrdinal("Cstop")) ? DateTime.Now : reader.GetDateTime("Cstop"),
                        ContactStarttime=reader.GetDateTime("Cstart")
                    });

            return contacts;
        }



        /// <summary>
        /// Reconnects with the database if the connection has been closed
        /// </summary>
        private void EnsureOpenConnection()
        {
            // Ensures an open connection
            if (!this.connection.State.Equals(ConnectionState.Open))
                this.connection.Open();
        }
    }
}

-- Creates the user table
CREATE TABLE `user` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `firstname` VARCHAR(30) NOT NULL,
  `lastname` VARCHAR(30) NOT NULL,
  `plz` INT(5) NOT NULL,
  `location` VARCHAR(100) NOT NULL,
  `street` VARCHAR(50) NOT NULL,
  `housenumber` INT(10) NOT NULL,
  `telephone` INT(30),
  `email` VARCHAR(69),
  `rfidcode` VARCHAR(10),
  `autodeleteaccount` BOOLEAN NOT NULL,
  `createdate` DATE NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE = InnoDB;

-- Creates the time spent table
CREATE TABLE `timespent` ( `id` INT NOT NULL AUTO_INCREMENT , `start` DATETIME NOT NULL , `stop` DATETIME NULL DEFAULT NULL , `enddisconnect` BOOLEAN NOT NULL , `userid` INT NOT NULL , PRIMARY KEY (`id`)) ENGINE = InnoDB;

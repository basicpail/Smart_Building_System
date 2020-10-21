CREATE TABLE `final_data`.`tempandhumid` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `Curr_Time` datetime NULL,
  `FirstFloorTemp` FLOAT NULL,
  `FirstFloorHumid` FLOAT NULL,
  `SecondFloorTemp` FLOAT NULL,
  `SecondFloorHumid` FLOAT NULL,
  `ThirdFloorTemp` FLOAT NULL,
  `ThirdFloorHumid` FLOAT NULL,
  PRIMARY KEY (`Id`));

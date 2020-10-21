CREATE TABLE `final_data`.`member` (
  `Num` INT NOT NULL AUTO_INCREMENT,
  `MemName` VARCHAR(45) NOT NULL,
  `CarModel` VARCHAR(45) NULL,
  `CarNumber` VARCHAR(45) NULL,
  `Telephone` VARCHAR(45) NOT NULL,
  `Entered` VARCHAR(45) NULL,
  `EnteredDate` DATETIME NULL,
  PRIMARY KEY (`Num`))
COMMENT = '주차 관련 테이블';

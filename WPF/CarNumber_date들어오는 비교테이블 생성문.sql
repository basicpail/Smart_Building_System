CREATE TABLE `final_data`.`membercar` (
  `CarNumber` VARCHAR(15) NOT NULL,
  `EnteredDate` DATETIME NOT NULL,
  PRIMARY KEY (`CarNumber`))
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8
COMMENT = '차번호, 시간들어오는 테이블(비교하기위함)';

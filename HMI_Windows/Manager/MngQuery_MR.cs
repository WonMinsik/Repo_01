using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMI_Windows.Manager
{
    class MngQuery_MR
    {

		/// <summary> 숙소동 기계실 센서 취득 데이터명 </summary>
		private static string[] ARR_ACCOMMODATION_DATA_ROWS = { "temp_1", "temp_2", "temp_3", "temp_4", "temp_5", "temp_6", "temp_7", "temp_8", "temp_9", "temp_10", "temp_11", "press_1", "press_2", "press_3", "press_4", "press_5", "press_6", "press_7", "press_8", "press_9", "flow_1", "flow_2", "flow_3", "flow_4", "valve_1", "valve_2", "valve_3", "valve_4" };
		/// <summary> 연구동 기계실 센서 취득 데이터명 </summary>
		private static string[] ARR_LABORATORY_DATA_ROWS = { "temp_1", "temp_2", "temp_3", "temp_4", "temp_5", "temp_6", "temp_7", "temp_8", "press_1", "press_2", "press_3", "press_4", "press_5", "press_6", "flow_1", "flow_2" };

		/// <summary>
		/// 연구동_기계실센서 계측 데이터 테이블 생성
		/// </summary>
		/// <param name="dtRegTime">등록일시</param>
		/// <returns>생성 쿼리문</returns>
		public static string GetCreateQuery_LABORATORY(DateTime dtRegTime)
		{
			return @"CREATE TABLE IF NOT EXISTS `laboratory" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR + MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) + @"` (
	`no` BIGINT(20) NOT NULL AUTO_INCREMENT,
	`regdate` TIMESTAMP NULL DEFAULT NULL,
	`temp_1` FLOAT SIGNED NULL DEFAULT NULL,
	`temp_2` FLOAT SIGNED NULL DEFAULT NULL,
	`temp_3` FLOAT SIGNED NULL DEFAULT NULL,
	`temp_4` FLOAT SIGNED NULL DEFAULT NULL,
	`temp_5` FLOAT SIGNED NULL DEFAULT NULL,
	`temp_6` FLOAT SIGNED NULL DEFAULT NULL,
	`temp_7` FLOAT SIGNED NULL DEFAULT NULL,
	`temp_8` FLOAT SIGNED NULL DEFAULT NULL,
	`press_1` FLOAT NULL DEFAULT NULL,
	`press_2` FLOAT NULL DEFAULT NULL,
	`press_3` FLOAT NULL DEFAULT NULL,
	`press_4` FLOAT NULL DEFAULT NULL,
	`press_5` FLOAT NULL DEFAULT NULL,
	`press_6` FLOAT NULL DEFAULT NULL,
	`press_7` FLOAT NULL DEFAULT NULL,
	`press_8` FLOAT NULL DEFAULT NULL,
	`flow_1` FLOAT SIGNED NULL DEFAULT NULL,
	`flow_2` FLOAT SIGNED NULL DEFAULT NULL,
	PRIMARY KEY(`no`) USING BTREE
)
COMMENT = '" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR) + "년" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) + @"월 연구동_기계실센서 계측 데이터'
COLLATE = 'utf8_general_ci'
ENGINE = InnoDB
AUTO_INCREMENT = 1;";
		}

		/// <summary>
		/// 숙소동 기계실 센서 계측 데이터 테이블 생성 쿼리문 취득
		/// </summary>
		/// <param name="dtRegTime">등록일시</param>
		/// <returns>테이블 생성 쿼리문</returns>
		public static string GetCreateQuery_ACCOMMODATION(DateTime dtRegTime)
		{
			return @"CREATE TABLE IF NOT EXISTS `accommodation" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR + MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) + @"` (
	`no` BIGINT(20) NOT NULL AUTO_INCREMENT,
	`regdate` TIMESTAMP NULL DEFAULT NULL,
	`temp_1` FLOAT SIGNED NULL DEFAULT NULL,
	`temp_2` FLOAT SIGNED NULL DEFAULT NULL,
	`temp_3` FLOAT SIGNED NULL DEFAULT NULL,
	`temp_4` FLOAT SIGNED NULL DEFAULT NULL,
	`temp_5` FLOAT SIGNED NULL DEFAULT NULL,
	`temp_6` FLOAT SIGNED NULL DEFAULT NULL,
	`temp_7` FLOAT SIGNED NULL DEFAULT NULL,
	`temp_8` FLOAT SIGNED NULL DEFAULT NULL,
	`temp_9` FLOAT SIGNED NULL DEFAULT NULL,
	`temp_10` FLOAT SIGNED NULL DEFAULT NULL,
	`temp_11` FLOAT SIGNED NULL DEFAULT NULL,
	`press_1` FLOAT NULL DEFAULT NULL,
	`press_2` FLOAT NULL DEFAULT NULL,
	`press_3` FLOAT NULL DEFAULT NULL,
	`press_4` FLOAT NULL DEFAULT NULL,
	`press_5` FLOAT NULL DEFAULT NULL,
	`press_6` FLOAT NULL DEFAULT NULL,
	`press_7` FLOAT NULL DEFAULT NULL,
	`press_8` FLOAT NULL DEFAULT NULL,
	`press_9` FLOAT NULL DEFAULT NULL,
	`flow_1` FLOAT SIGNED NULL DEFAULT NULL,
	`flow_2` FLOAT SIGNED NULL DEFAULT NULL,
	`flow_3` FLOAT SIGNED NULL DEFAULT NULL,
	`flow_4` FLOAT SIGNED NULL DEFAULT NULL,
	`valve_1` FLOAT SIGNED NULL DEFAULT NULL,
	`valve_2` FLOAT SIGNED NULL DEFAULT NULL,
	`valve_3` FLOAT SIGNED NULL DEFAULT NULL,
	`valve_4` FLOAT SIGNED NULL DEFAULT NULL,
	PRIMARY KEY(`no`) USING BTREE
)
COMMENT = '" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR) + "년" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) + @"월숙소동_기계실센서 계측데이타'
COLLATE = 'utf8_general_ci'
ENGINE = InnoDB
AUTO_INCREMENT = 1;";
		}

		/// <summary>
		/// 밸브 제어 이력 월별 테이블 생성
		/// </summary>
		/// <param name="dtRegTime">생성년월</param>
		/// <returns>쿼리문</returns>
		public static string GetCreateQuery_VALVE_CONTROLRECORD(DateTime dtRegTime)
		{
			string strCreateQuery = @"CREATE TABLE IF NOT EXISTS `valve_controlrecord" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR + MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) + @"` (
	`no` BIGINT(20) NOT NULL AUTO_INCREMENT,
	`regdate` DATETIME NOT NULL,
	`valve_name` CHAR(50) NOT NULL COLLATE 'utf8_general_ci',
	`value` INT(11) NOT NULL DEFAULT '0',
	PRIMARY KEY (`no`) USING BTREE
)
COMMENT='" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR) + "년" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) + @"월 밸브제어 이력 테이블'
COLLATE='utf8_general_ci'
ENGINE=InnoDB
AUTO_INCREMENT=1;";

			return strCreateQuery;
		}


		/// <summary>
		/// 숙소동 기계실 센서 데이터 이력 추가 쿼리문 취득
		/// </summary>
		/// <param name="dtRegDate"></param>
		/// <param name="dtTmp"></param>
		/// <param name="dtPrs"></param>
		/// <param name="dtFlw"></param>
		/// <param name="dtVlv"></param>
		/// <returns></returns>
		public static string GetInsertQuery_ACCOMMODATION(DateTime dtRegDate, System.Data.DataTable dtTmp, System.Data.DataTable dtPrs, System.Data.DataTable dtFlw, System.Data.DataTable dtVlv)
		{
			string strInsertQuery = "INSERT INTO accommodation" + dtRegDate.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR + MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH)
				+ " (regdate," + string.Join(MngQuery_Main.STR_SEPARATOR, ARR_ACCOMMODATION_DATA_ROWS) + ")" +
				" VALUES (\'" 
				+ dtRegDate.ToString(MngQuery_Main.STR_DATE_FORMAT_DATA_ROW) + "\', " 
				+ ((float)dtTmp.Rows[0].ItemArray[1]).ToString("F4") + ", "
				+ ((float)dtTmp.Rows[0].ItemArray[2]).ToString("F4") + ", "
				+ ((float)dtTmp.Rows[0].ItemArray[3]).ToString("F4") + ", "
				+ ((float)dtTmp.Rows[0].ItemArray[4]).ToString("F4") + ", "
				+ ((float)dtTmp.Rows[0].ItemArray[5]).ToString("F4") + ", "
				+ ((float)dtTmp.Rows[0].ItemArray[6]).ToString("F4") + ", "
				+ ((float)dtTmp.Rows[0].ItemArray[7]).ToString("F4") + ", "
				+ ((float)dtTmp.Rows[0].ItemArray[8]).ToString("F4") + ", "
				+ ((float)dtTmp.Rows[0].ItemArray[9]).ToString("F4") + ", "
				+ ((float)dtTmp.Rows[0].ItemArray[10]).ToString("F4") + ", "
				+ ((float)dtTmp.Rows[0].ItemArray[11]).ToString("F4") + ", "
				+ ((float)dtPrs.Rows[0].ItemArray[1]).ToString("F4") + ", "
				+ ((float)dtPrs.Rows[0].ItemArray[2]).ToString("F4") + ", "
				+ ((float)dtPrs.Rows[0].ItemArray[3]).ToString("F4") + ", "
				+ ((float)dtPrs.Rows[0].ItemArray[4]).ToString("F4") + ", "
				+ ((float)dtPrs.Rows[0].ItemArray[5]).ToString("F4") + ", "
				+ ((float)dtPrs.Rows[0].ItemArray[6]).ToString("F4") + ", "
				+ ((float)dtPrs.Rows[0].ItemArray[7]).ToString("F4") + ", "
				+ ((float)dtPrs.Rows[0].ItemArray[8]).ToString("F4") + ", "
				+ ((float)dtPrs.Rows[0].ItemArray[9]).ToString("F4") + ", "
				+ ((float)dtFlw.Rows[0].ItemArray[1]).ToString("F4") + ", "
				+ ((float)dtFlw.Rows[0].ItemArray[2]).ToString("F4") + ", "
				+ ((float)dtFlw.Rows[0].ItemArray[3]).ToString("F4") + ", "
				+ ((float)dtFlw.Rows[0].ItemArray[4]).ToString("F4") + ", "
				+ ((float)dtVlv.Rows[0].ItemArray[1]).ToString("F4") + ", "
				+ ((float)dtVlv.Rows[0].ItemArray[2]).ToString("F4") + ", "
				+ ((float)dtVlv.Rows[0].ItemArray[3]).ToString("F4") + ", "
				+ ((float)dtVlv.Rows[0].ItemArray[4]).ToString("F4") + ");";

			return strInsertQuery;
		}

		public static string GetUpdateQuery_ACCOMMODATION_CUR(DateTime dateTime, System.Data.DataTable dtTmp, System.Data.DataTable dtPrs, System.Data.DataTable dtFlw, System.Data.DataTable dtVlv)
		{
			string strUpdateQuery = "UPDATE accommodation_cur SET "
				+ "regdate = \'" + dateTime.ToString(MngQuery_Main.STR_DATE_FORMAT_DATA_ROW) + "\', "
				+ "temp_1 = " + ((float)dtTmp.Rows[0].ItemArray[1]).ToString("F4") + ", "
				+ "temp_2 = " + ((float)dtTmp.Rows[0].ItemArray[2]).ToString("F4") + ", "
				+ "temp_3 = " + ((float)dtTmp.Rows[0].ItemArray[3]).ToString("F4") + ", "
				+ "temp_4 = " + ((float)dtTmp.Rows[0].ItemArray[4]).ToString("F4") + ", "
				+ "temp_5 = " + ((float)dtTmp.Rows[0].ItemArray[5]).ToString("F4") + ", "
				+ "temp_6 = " + ((float)dtTmp.Rows[0].ItemArray[6]).ToString("F4") + ", "
				+ "temp_7 = " + ((float)dtTmp.Rows[0].ItemArray[7]).ToString("F4") + ", "
				+ "temp_8 = " + ((float)dtTmp.Rows[0].ItemArray[8]).ToString("F4") + ", "
				+ "temp_9 = " + ((float)dtTmp.Rows[0].ItemArray[9]).ToString("F4") + ", "
				+ "temp_10 = " + ((float)dtTmp.Rows[0].ItemArray[10]).ToString("F4") + ", "
				+ "temp_11 = " + ((float)dtTmp.Rows[0].ItemArray[11]).ToString("F4") + ", "
				+ "press_1 = " + ((float)dtPrs.Rows[0].ItemArray[1]).ToString("F4") + ", "
				+ "press_2 = " + ((float)dtPrs.Rows[0].ItemArray[2]).ToString("F4") + ", "
				+ "press_3 = " + ((float)dtPrs.Rows[0].ItemArray[3]).ToString("F4") + ", "
				+ "press_4 = " + ((float)dtPrs.Rows[0].ItemArray[4]).ToString("F4") + ", "
				+ "press_5 = " + ((float)dtPrs.Rows[0].ItemArray[5]).ToString("F4") + ", "
				+ "press_6 = " + ((float)dtPrs.Rows[0].ItemArray[6]).ToString("F4") + ", "
				+ "press_7 = " + ((float)dtPrs.Rows[0].ItemArray[7]).ToString("F4") + ", "
				+ "press_8 = " + ((float)dtPrs.Rows[0].ItemArray[8]).ToString("F4") + ", "
				+ "press_9 = " + ((float)dtPrs.Rows[0].ItemArray[9]).ToString("F4") + ", "
				+ "flow_1 = " + ((float)dtFlw.Rows[0].ItemArray[1]).ToString("F4") + ", "
				+ "flow_2 = " + ((float)dtFlw.Rows[0].ItemArray[2]).ToString("F4") + ", "
				+ "flow_3 = " + ((float)dtFlw.Rows[0].ItemArray[3]).ToString("F4") + ", "
				+ "flow_4 = " + ((float)dtFlw.Rows[0].ItemArray[4]).ToString("F4") + ", "
				+ "valve_1 = " + ((float)dtVlv.Rows[0].ItemArray[1]).ToString("F4") + ", "
				+ "valve_2 = " + ((float)dtVlv.Rows[0].ItemArray[2]).ToString("F4") + ", "
				+ "valve_3 = " + ((float)dtVlv.Rows[0].ItemArray[3]).ToString("F4") + ", "
				+ "valve_4 = " + ((float)dtVlv.Rows[0].ItemArray[4]).ToString("F4")
				+ " WHERE No = \'1\';";

			return strUpdateQuery;
		}

		/// <summary>
		/// 연구동 기계실 센서 데이터 이력 추가 쿼리문 취득
		/// </summary>
		/// <param name="dtRegDate">등록일시</param>
		/// <param name="dtTmpData">연구동 기계실 온도 데이터</param>
		/// <param name="dtPrsFlwData">연구동 기계실 압력 및 유량 데이터</param>
		/// <returns>입력 쿼리문</returns>
		public static string GetInsertQuery_LABORATORY(DateTime dtRegDate, System.Data.DataTable dtTmpData, System.Data.DataTable dtPrsFlwData)
		{
			System.Data.DataRow drTmp = dtTmpData.Rows[0];
			System.Data.DataRow drPrs = dtPrsFlwData.Rows[0];
			
			string strInsertQuery = "INSERT INTO laboratory" + dtRegDate.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR + MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH)
				+ " (regdate, " + string.Join(MngQuery_Main.STR_SEPARATOR, ARR_LABORATORY_DATA_ROWS) + ")" +
				" VALUES (\'" + dtRegDate.ToString(MngQuery_Main.STR_DATE_FORMAT_DATA_ROW) + "\', " 
				+ ((float)drTmp.ItemArray[1]).ToString("F4") + ", "
				+ ((float)drTmp.ItemArray[2]).ToString("F4") + ", "
				+ ((float)drTmp.ItemArray[3]).ToString("F4") + ", "
				+ ((float)drTmp.ItemArray[4]).ToString("F4") + ", "
				+ ((float)drTmp.ItemArray[5]).ToString("F4") + ", "
				+ ((float)drTmp.ItemArray[6]).ToString("F4") + ", "
				+ ((float)drTmp.ItemArray[7]).ToString("F4") + ", "
				+ ((float)drTmp.ItemArray[8]).ToString("F4") + ", "
				+ ((float)drPrs.ItemArray[1]).ToString("F4") + ", "
				+ ((float)drPrs.ItemArray[2]).ToString("F4") + ", "
				+ ((float)drPrs.ItemArray[3]).ToString("F4") + ", "
				+ ((float)drPrs.ItemArray[4]).ToString("F4") + ", "
				+ ((float)drPrs.ItemArray[5]).ToString("F4") + ", "
				+ ((float)drPrs.ItemArray[6]).ToString("F4") + ", "
				+ ((float)drPrs.ItemArray[7]).ToString("F4") + ", "
				+ ((float)drPrs.ItemArray[8]).ToString("F4")
				+ ");";

			return strInsertQuery;
		}

		/// <summary>
		/// 연구동 기계실 현재값 갱신 쿼리문 취득
		/// </summary>
		/// <param name="dtRegDate">등록일시</param>
		/// <param name="dtTmpData">연구동 기계실 온도 데이터</param>
		/// <param name="dtPrsFlwData">연구동 기계실 압력 및 유량 데이터</param>
		/// <returns></returns>
		public static string GetUpdateQuery_LABORATORY_CUR(DateTime dtRegDate, System.Data.DataTable dtTmpData, System.Data.DataTable dtPrsFlwData)
		{
			System.Data.DataRow drTmp = dtTmpData.Rows[0];
			System.Data.DataRow drPrs = dtPrsFlwData.Rows[0];

			string strUpdateQuery = "UPDATE laboratory_cur SET "
				+ "regdate = \'" + dtRegDate.ToString(MngQuery_Main.STR_DATE_FORMAT_DATA_ROW) + "\', "
				+ "temp_1 = " + ((float)drTmp.ItemArray[1]).ToString("F4") + ", "
				+ "temp_2 = " + ((float)drTmp.ItemArray[2]).ToString("F4") + ", "
				+ "temp_3 = " + ((float)drTmp.ItemArray[3]).ToString("F4") + ", "
				+ "temp_4 = " + ((float)drTmp.ItemArray[4]).ToString("F4") + ", "
				+ "temp_5 = " + ((float)drTmp.ItemArray[5]).ToString("F4") + ", "
				+ "temp_6 = " + ((float)drTmp.ItemArray[6]).ToString("F4") + ", "
				+ "temp_7 = " + ((float)drTmp.ItemArray[7]).ToString("F4") + ", "
				+ "temp_8 = " + ((float)drTmp.ItemArray[8]).ToString("F4") + ", "
				+ "press_1 = " + ((float)drPrs.ItemArray[1]).ToString("F4") + ", "
				+ "press_2 = " + ((float)drPrs.ItemArray[2]).ToString("F4") + ", "
				+ "press_3 = " + ((float)drPrs.ItemArray[3]).ToString("F4") + ", "
				+ "press_4 = " + ((float)drPrs.ItemArray[4]).ToString("F4") + ", "
				+ "press_5 = " + ((float)drPrs.ItemArray[5]).ToString("F4") + ", "
				+ "press_6 = " + ((float)drPrs.ItemArray[6]).ToString("F4") + ", "
				+ "flow_1 = " + ((float)drPrs.ItemArray[7]).ToString("F4") + ", "
				+ "flow_2 = " + ((float)drPrs.ItemArray[8]).ToString("F4")
				+ " WHERE no = 1;";

			return strUpdateQuery;
		}


		/// <summary>
		/// valve_control 테이블에서 밸브제어목록 취득 쿼리문 취득
		/// </summary>
		/// <returns>밸브제어 목록 취득 쿼리문</returns>
		public static string GetSelectQuery_VALVE_CONTORL()
		{
			string strSelectQuery = "SELECT valve_name,	value, command_flag FROM valve_control WHERE command_flag = 1 ORDER BY valve_name asc LIMIT 5;";

			return strSelectQuery;
		}

		/// <summary>
		/// valve_control 테이블의 command_flag를 변경하는 쿼리문 목록 취득 
		/// </summary>
		/// <param name="strValve_name">제어한 밸브 이름</param>
		/// <returns>밸브 제어 쿼리문 목록</returns>
		public static string GetUpdateQuery_VALVE_CONTROL(string strValve_name)
		{
			string strUpdateQuery = "UPDATE valve_control SET command_flag = 0 WHERE valve_name = \'" + strValve_name + "\';";

			return strUpdateQuery;
		}


		/// <summary>
		/// 밸브 제어이력 테이블에 데이터 추가 쿼리문 작성
		/// </summary>
		/// <param name="dtRegDate">제어일시</param>
		/// <param name="strValve_name">제어 밸브명</param>
		/// <param name="strValue">제어값</param>
		/// <returns>제어쿼리문</returns>
		public static string GetInsertQuery_VALVE_CONTROLRECORD(DateTime dtRegDate, string strValve_name, string strValue)
		{
			string strInsertQueryHeadder = "INSERT INTO `valve_controlrecord`( `regdate`, `valve_name`, `value`) VALUES ";
			string strValues = "(\'" + dtRegDate.ToString(MngQuery_Main.STR_DATE_FORMAT_DATA_ROW) + "\', \'" + strValve_name + "\', " + strValue + ")";

			string strInsertQuery = strInsertQueryHeadder + strValues + ";";


			return strInsertQuery;
		}
		

		/// <summary>
		/// 밸브 제어이력 월별테이블에 데이터 추가 쿼리문 작성
		/// </summary>
		/// <param name="dtRegDate">제어일시</param>
		/// <param name="strValve_name">제어 밸브명</param>
		/// <param name="strValue">제어값</param>
		/// <returns>제어쿼리문</returns>
		public static string GetInsertQuery_VALVE_CONTROLRECORDYearMonth(DateTime dtRegDate, string strValve_name, string strValue)
		{
			string strInsertQueryHeadder = "INSERT INTO `valve_controlrecord"+ dtRegDate.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR + MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) + "`( `regdate`, `valve_name`, `value`) VALUES ";
			string strValues = "(\'" + dtRegDate.ToString(MngQuery_Main.STR_DATE_FORMAT_DATA_ROW) + "\', \'" + strValve_name + "\', " + strValue + ")";

			string strInsertQuery = strInsertQueryHeadder + strValues + ";";


			return strInsertQuery;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMI_Windows.Manager
{
    class MngQuery_UDP
    {
		/// <summary> B동 객실 난방 센서 취득 데이터명 </summary>
		private static string[] ARR_ROOMB_TABLE_ROWS = { "Floor", "Ho", "InsideTemp", "SetTemp", "InHeating", "OutHeating", "NowControlValue", "SetControlValue", "RegDate", "NowFlow", "TotalFlow", "OnOff", "TotalHeat", "SetDate", "DeltaTTemp", "FloorTemp" };
		/// <summary> 외기센서 취득 데이터명 </summary>
		private static string[] ARR_WEATHER_TABLE_ROWS = { "reg_date", " dev_status", " wind_dir", " wind_speed", " air_temp", " air_humi", " air_press", " weather", " rainfall", " rainfall_acc", " rain_unit", " radiation_acc", " radiation" };


		/// <summary>
		/// B동 객실 제어 이력 테이블 생성 쿼리문 취득
		/// </summary>
		/// <param name="dtRegTime">생성년월</param>
		/// <returns>생성쿼리문</returns>
		public static string GetCreateQuery_CONTROLRECORD(DateTime dtRegTime)
		{string strCreateQuery = @"CREATE TABLE IF NOT EXISTS `controlrecord" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR + MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) + @"` (
	`No` INT(11) NOT NULL AUTO_INCREMENT,
	`Dong` CHAR(1) NOT NULL COMMENT '동(B,C)' COLLATE 'utf8_general_ci',
	`Floor` SMALLINT(6) NOT NULL COMMENT '층',
	`Ho` SMALLINT(6) NOT NULL COMMENT '호',
	`NowTemp` FLOAT NOT NULL,
	`SetTemp` FLOAT NULL DEFAULT NULL COMMENT '설정온도',
	`NowControlValue` SMALLINT(6) NOT NULL,
	`SetControlValue` FLOAT NULL DEFAULT NULL,
	`RegDate` TIMESTAMP NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
	`OnOff` SMALLINT(6) NULL DEFAULT NULL,
	`DiffTemp` FLOAT NULL DEFAULT NULL COMMENT '난방수온도차',
	`OutTemp` FLOAT NULL DEFAULT NULL COMMENT '환수온도',
	`flag` SMALLINT(6) NULL DEFAULT NULL COMMENT '읽기유무(0,1)',
	`SetDate` TIMESTAMP NULL DEFAULT NULL COMMENT '읽기일시',
	`FlowReset` SMALLINT(1) NULL DEFAULT NULL,
	PRIMARY KEY (`No`) USING BTREE
)
COMMENT='" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR) + "년" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) + @"월 객실 제어 이력 테이블'
COLLATE='utf8_general_ci'
ENGINE=InnoDB
AUTO_INCREMENT=1
;";
            return strCreateQuery;
        }




		public static string GetCreateQuery_WEATHER(DateTime dtRegTime)
		{
			string strCreateQuery = @"CREATE TABLE IF NOT EXISTS `weather_data" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR + MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) + @"` (
	`No` BIGINT(20) NOT NULL AUTO_INCREMENT,
	`reg_date` TIMESTAMP NOT NULL DEFAULT current_timestamp(),
	`dev_status` INT(11) NULL DEFAULT NULL,
	`wind_dir` INT(11) NULL DEFAULT NULL,
	`wind_speed` FLOAT NULL DEFAULT NULL,
	`air_temp` FLOAT NULL DEFAULT NULL,
	`air_humi` FLOAT NULL DEFAULT NULL,
	`air_press` FLOAT NULL DEFAULT NULL,
	`weather` INT(11) NULL DEFAULT NULL,
	`rainfall` FLOAT NULL DEFAULT NULL,
	`rainfall_acc` FLOAT NULL DEFAULT NULL,
	`rain_unit` FLOAT NULL DEFAULT NULL,
	`radiation` FLOAT NULL DEFAULT NULL,
	`radiation_acc` FLOAT NULL DEFAULT NULL,
	PRIMARY KEY(`No`) USING BTREE
)
COMMENT = '" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR) + "년" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) + @"월 외기센서 계측 데이터'
COLLATE = 'utf8_general_ci'
ENGINE = InnoDB
AUTO_INCREMENT = 1;";
			return strCreateQuery;
		}

		/// <summary>
		/// roomb 테이블 생성 쿼리 
		/// </summary>
		/// <returns>테이블 생성 쿼리문</returns>
		public static string GetCreateQuery_ROOMB(DateTime dtRegTime)
		{
			string strCreateQuery =  @"CREATE TABLE IF NOT EXISTS `roomb" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR + MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) + @"` (
	`No` BIGINT(20) NOT NULL AUTO_INCREMENT,
	`Floor` SMALLINT(6) NOT NULL,
	`Ho` SMALLINT(6) NOT NULL,
	`InsideTemp` FLOAT NOT NULL,
	`SetTemp` FLOAT NULL DEFAULT NULL,
	`InHeating` FLOAT NOT NULL,
	`OutHeating` FLOAT NOT NULL,
	`NowControlValue` SMALLINT(6) NOT NULL,
	`SetControlValue` FLOAT NULL DEFAULT NULL,
	`RegDate` TIMESTAMP NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
	`NowFlow` FLOAT NOT NULL,
	`TotalFlow` FLOAT NOT NULL,
	`OnOff` SMALLINT(6) NOT NULL,
	`TotalHeat` FLOAT NOT NULL,
	`SetDate` TIMESTAMP NOT NULL DEFAULT '0000-00-00 00:00:00',
	`DeltaTTemp` SMALLINT(6) NULL DEFAULT NULL,
	`FloorTemp` SMALLINT(6) NULL DEFAULT NULL,
	PRIMARY KEY(`No`) USING BTREE
)
COMMENT = '" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR) + "년" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) + @"월 B동객실 계측제어 데이터'
COLLATE='utf8_general_ci'
ENGINE=InnoDB
AUTO_INCREMENT = 1;";

			return strCreateQuery;
		}

		/// <summary>
		/// B동 객실 제어 명령 취득 쿼리문 취득
		/// </summary>
		/// <returns>객실 제어 명령 목록 취득 쿼리문</returns>
		public static string GetSelectQuery_CONTROLRECORD()
		{
			string strSelectQuery = @"SELECT 
LSERVER.`No` AS 'ID'
, LSERVER.Dong AS 'Dong'
, LSERVER.Floor AS 'Floor'
, LSERVER.Ho AS 'Ho'
, LSERVER.ip_addr AS 'Ip_Addr'
, CRECORD.No AS 'No'
, CRECORD.OnOff AS 'On_off'
, CRECORD.SetTemp AS 'Set_temp'
, CRECORD.DiffTemp AS 'Diff_temp'
, CRECORD.flag AS 'Flag'
, CRECORD.FlowReset AS 'Flow_reset'
, CRECORD.RegDate AS 'Reg_date'
, CRECORD.SetDate AS 'Set_date'
FROM controlrecord AS CRECORD 
INNER JOIN (SELECT NO, Dong, FLoor, Ho, ip_addr FROM lookserver AS LKSVR WHERE LKSVR.room_type = 0 AND LKSVR.Dong = 'B' ) AS LSERVER 
ON LSERVER.Dong = CRECORD.Dong AND LSERVER.Floor = CRECORD.Floor AND LSERVER.Ho = CRECORD.Ho
WHERE CRECORD.flag = 0 ORDER BY  `No`, `Ho` ASC;";
			return strSelectQuery;
		}

		/// <summary>
		/// LookServer테이블에서 B동 객실 UDP장치에 대한 IP정보를 취득한다.
		/// </summary>
		/// <returns></returns>
		public static string GetSelectQuery_LOOKSERVER()
		{
			string strSelectQuery = "SELECT No, Dong, Floor, Ho, ip_addr AS Addr, connet AS Connect FROM LookServer WHERE Dong = \'B\' AND room_type = 0 AND Ho != 0 ORDER BY No;";

			return strSelectQuery;
		}

		/// <summary>
		/// LookServer테이블의 B동 객실 연결상태 갱신 쿼리문 취득
		/// </summary>
		/// <param name="strAddr">연결대상 IP 주소</param>
		/// <param name="isConnected">연결 상태</param>
		/// <returns>갱신 쿼리문</returns>
		public static string GetUpdateQuery_LOOKSERVER(string strAddr, bool isConnected)
		{
			string strUpdateQuery = "UPDATE LookServer SET connet = " + (isConnected ? "1" : "0")
					+ " WHERE ip_addr = \'" + strAddr + "\';";

			return strUpdateQuery;
		}

		/// <summary>
		/// 알람이력 추가 쿼리문 취득
		/// </summary>
		/// <param name="dtRegDate">알람발생일</param>
		/// <param name="strAlarmContents">알람내용</param>
		/// <param name="mdlRoomInfo">알람발생장소</param>
		/// <returns></returns>
		public static string GetInsertQuery_ALARMRECORD(DateTime dtRegDate, string strAlarmContents, Model.MdlRoomInfo mdlRoomInfo)
		{
			string strAlarmPlace = mdlRoomInfo.strDong + mdlRoomInfo.intFloor.ToString() + mdlRoomInfo.intHo.ToString();

			string sqlQueary = "INSERT INTO AlarmRecord (IsRead, AlarmTime, AlarmContents, AlarmPlace ) VALUES (1, "
				+ dtRegDate.ToString(MngQuery_Main.STR_DATE_FORMAT_DATA_ROW) + ", '" + strAlarmContents + "', '" + strAlarmPlace + "');";

			return sqlQueary;
		}

		/// <summary>
		/// B동 객실 제어 명령 열람상태 변경 쿼리문 취득
		/// </summary>
		/// <param name="intControlOrderNo">제어명령번호</param>
		/// <param name="mdlRoomInfo">객실 정보</param>
		/// <returns>객실 제어 명령 쿼리문</returns>
		public static string GetUpdateQuery_CONTROLRECORD(int intControlOrderNo, Model.MdlRoomInfo mdlRoomInfo)
		{
			string strUpdateQuery = "UPDATE ControlRecord SET flag = 1"
				+ " WHERE No = " + intControlOrderNo
				+ " AND dong = \'" + mdlRoomInfo.strDong
				+ "\' AND floor = " + mdlRoomInfo.intFloor.ToString()
				+ " AND ho = " + mdlRoomInfo.intHo + ";";
			return strUpdateQuery;
		}


		/// <summary>
		/// B동 객실 센서 데이터 이력 추가 쿼리문 취득
		/// </summary>
		/// <param name="dtRegTime">등록일시</param>
		/// <param name="mdlOutPacket">센서 계측 데이터 배열</param>
		/// <returns>쿼리문배열</returns>
		public static string GetInsertQuery_ROOMB(DateTime dtRegTime, Model.MdlOutPacketData mdlOutPacket)
		{
			string strInsertQuery = "INSERT INTO roomb" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR + MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH)
				+ "(" + string.Join(MngQuery_Main.STR_SEPARATOR, ARR_ROOMB_TABLE_ROWS)
				+ ") VALUES (" + mdlOutPacket.mdlRoomInfo.intFloor.ToString() + "," + mdlOutPacket.mdlRoomInfo.intHo.ToString()
				+ "," + mdlOutPacket.fltInsideTemp + "," + mdlOutPacket.fltHeatSetTemp + "," + mdlOutPacket.fltInHeating + "," + mdlOutPacket.fltOutHeating
				+ "," + mdlOutPacket.shtNowControlValue + "," + mdlOutPacket.shtSetControlValue
				+ ", " + "\'" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_DATA_ROW) + "\'"
				+ ", " + mdlOutPacket.fltNowFlow + "," + mdlOutPacket.fltTotalFlow
				+ "," + mdlOutPacket.shtHeatOnOff + "," + mdlOutPacket.fltTotalHeat
				+ ", " + "\'" + DateTime.Now.ToString(MngQuery_Main.STR_DATE_FORMAT_DATA_ROW) + "\'"
				+ ", " + mdlOutPacket.intDeltaTTemp + "," + mdlOutPacket.intFloorTemp + ");";
			return strInsertQuery;
		}

		/// <summary>
		/// B동 객실센서 현재값 변경 쿼리문 취득
		/// </summary>
		/// <param name="dtRegTime">취득 시간</param>
		/// <param name="mdlOutPacket"></param>
		/// <returns></returns>
		public static string GetUpdateQuery_ROOMB_CUR(DateTime dtRegTime, Model.MdlOutPacketData mdlOutPacket)
		{
			string strUpdateQuery = "UPDATE roomb_cur SET "
				+ ARR_ROOMB_TABLE_ROWS[2] + " = " + mdlOutPacket.fltInsideTemp.ToString() + MngQuery_Main.STR_SEPARATOR
				+ ARR_ROOMB_TABLE_ROWS[3] + " = " + mdlOutPacket.fltHeatSetTemp.ToString() + MngQuery_Main.STR_SEPARATOR
				+ ARR_ROOMB_TABLE_ROWS[4] + " = " + mdlOutPacket.fltInHeating.ToString() + MngQuery_Main.STR_SEPARATOR
				+ ARR_ROOMB_TABLE_ROWS[5] + " = " + mdlOutPacket.fltOutHeating.ToString() + MngQuery_Main.STR_SEPARATOR
				+ ARR_ROOMB_TABLE_ROWS[6] + " = " + mdlOutPacket.shtNowControlValue.ToString() + MngQuery_Main.STR_SEPARATOR
				+ ARR_ROOMB_TABLE_ROWS[7] + " = " + mdlOutPacket.shtSetControlValue.ToString() + MngQuery_Main.STR_SEPARATOR
				+ ARR_ROOMB_TABLE_ROWS[8] + " = " + "\'" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_DATA_ROW) + "\'" + MngQuery_Main.STR_SEPARATOR
				+ ARR_ROOMB_TABLE_ROWS[9] + " = " + mdlOutPacket.fltNowFlow.ToString() + MngQuery_Main.STR_SEPARATOR
				+ ARR_ROOMB_TABLE_ROWS[10] + " = " + mdlOutPacket.fltTotalFlow.ToString() + MngQuery_Main.STR_SEPARATOR
				+ ARR_ROOMB_TABLE_ROWS[11] + " = " + mdlOutPacket.shtHeatOnOff.ToString() + MngQuery_Main.STR_SEPARATOR
				+ ARR_ROOMB_TABLE_ROWS[12] + " = " + mdlOutPacket.fltTotalHeat.ToString() + MngQuery_Main.STR_SEPARATOR
				+ ARR_ROOMB_TABLE_ROWS[13] + " = " + "\'" + DateTime.Now.ToString(MngQuery_Main.STR_DATE_FORMAT_DATA_ROW) + "\'" + MngQuery_Main.STR_SEPARATOR
				+ ARR_ROOMB_TABLE_ROWS[14] + " = " + mdlOutPacket.intDeltaTTemp.ToString() + MngQuery_Main.STR_SEPARATOR
				+ ARR_ROOMB_TABLE_ROWS[15] + " = " + mdlOutPacket.intFloorTemp.ToString()
				+ " WHERE "
				+ ARR_ROOMB_TABLE_ROWS[0] + " = " + mdlOutPacket.mdlRoomInfo.intFloor + " AND "
				+ ARR_ROOMB_TABLE_ROWS[1] + " = " + mdlOutPacket.mdlRoomInfo.intHo + ";";

			return strUpdateQuery;
		}

		/// <summary>
		/// 외기 센서 데이터 이력 추가 쿼리문 취득
		/// </summary>
		/// <param name="dtRegDate">등록일시</param>
		/// <param name="WeatherData">센서 계측 데이터</param>
		/// <returns>쿼리문</returns>
		public static string GetInserQuery_WEATHER(DateTime dtRegDate, Model.MdlWeatherData WeatherData)
		{
			string strInsertQuery = "INSERT INTO weather_data" + dtRegDate.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR + MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH)
				+ " (" + string.Join(MngQuery_Main.STR_SEPARATOR, ARR_WEATHER_TABLE_ROWS) + ") Values ( \'"
				+ dtRegDate.ToString(MngQuery_Main.STR_DATE_FORMAT_DATA_ROW) + "\', " + WeatherData.intDevStatus + ", " + WeatherData.intWindDir + ", " + WeatherData.dblWindSpeed + ", " + WeatherData.dblAirTemp
				+ ", " + WeatherData.dblAirHumi + ", " + WeatherData.dblAirPress + "," + WeatherData.intWeather + ", " + WeatherData.dblRainFall + "," + WeatherData.dblRainFallAcc
				+ "," + WeatherData.intRainUnit + ", " + WeatherData.dblRadiationAcc + ", " + WeatherData.dblRadiation + ");";

			return strInsertQuery;
		}

		/// <summary>
		/// 외기센서 현재값 변경 쿼리문 취득
		/// </summary>
		/// <param name="dtRegDate">데이터 취득 시간</param>
		/// <param name="WeatherData">외기센서 정보</param>
		/// <returns>쿼리문</returns>
		public static string GetUpdateQuery_WEATHER_CUR(DateTime dtRegDate, Model.MdlWeatherData WeatherData)
		{
			string strUpdateQuery = "UPDATE weather_data_cur "
				+ ARR_WEATHER_TABLE_ROWS[0] + " = " + "\'" + dtRegDate.ToString(MngQuery_Main.STR_DATE_FORMAT_DATA_ROW) + "\'" + MngQuery_Main.STR_SEPARATOR
				+ ARR_WEATHER_TABLE_ROWS[1] + " = " + WeatherData.intDevStatus + MngQuery_Main.STR_SEPARATOR
				+ ARR_WEATHER_TABLE_ROWS[2] + " = " + WeatherData.intWindDir + MngQuery_Main.STR_SEPARATOR
				+ ARR_WEATHER_TABLE_ROWS[3] + " = " + WeatherData.dblWindSpeed + MngQuery_Main.STR_SEPARATOR
				+ ARR_WEATHER_TABLE_ROWS[4] + " = " + WeatherData.dblAirTemp + MngQuery_Main.STR_SEPARATOR
				+ ARR_WEATHER_TABLE_ROWS[5] + " = " + WeatherData.dblAirHumi + MngQuery_Main.STR_SEPARATOR
				+ ARR_WEATHER_TABLE_ROWS[6] + " = " + WeatherData.dblAirPress + MngQuery_Main.STR_SEPARATOR
				+ ARR_WEATHER_TABLE_ROWS[7] + " = " + WeatherData.intWeather + MngQuery_Main.STR_SEPARATOR
				+ ARR_WEATHER_TABLE_ROWS[8] + " = " + WeatherData.dblRainFall + MngQuery_Main.STR_SEPARATOR
				+ ARR_WEATHER_TABLE_ROWS[9] + " = " + WeatherData.dblRainFallAcc + MngQuery_Main.STR_SEPARATOR
				+ ARR_WEATHER_TABLE_ROWS[10] + " = " + WeatherData.intRainUnit + MngQuery_Main.STR_SEPARATOR
				+ ARR_WEATHER_TABLE_ROWS[11] + " = " + WeatherData.dblRadiationAcc + MngQuery_Main.STR_SEPARATOR
				+ ARR_WEATHER_TABLE_ROWS[12] + " = " + WeatherData.dblRadiation + " WHERE No = 1;";
			
			return strUpdateQuery;
		}
	}
}

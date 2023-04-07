using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMI_Windows.Manager
{
    class MngQuery_IoT7
    {
		/// <summary> 객실 IOT장비 계측 데이터 항목명 </summary>
		private static string[] ARR_ROOM_IOT_DATA_ROWS = { "Sensor_no", "Aircon_state", "Temp", "Humidity", "VOCs", "Co2", "Noise", "Illumiance", "Movement", "RegDate", "Dong", "Ho", "RSSI" };

		/// <summary>
		/// 객실 IOT 센서 제어 이력 테이블 생성 쿼리문 취득 
		/// </summary>
		/// <param name="strDong">동 정보</param>
		/// <param name="dtRegTime">등록일시</param>
		/// <returns>테이블 생성 쿼리문</returns>
		public static string GetCreateQuery_ROOM_IOT(string strDong, DateTime dtRegTime)
		{
			return @"CREATE TABLE IF NOT EXISTS `room" + strDong.ToLower() + "iot" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR + MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) + @"` (
	`No` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '데이터 번호',
	`Sensor_no` int(11) DEFAULT NULL COMMENT '일련번호',
	`Aircon_state` smallint(6) DEFAULT NULL COMMENT '에어컨동작상태(on:1 / off : 0)',
	`Temp` smallint(6) DEFAULT NULL COMMENT '온도',
	`Humidity` smallint(6) DEFAULT NULL COMMENT '습도',
	`VOCs` smallint(6) DEFAULT NULL COMMENT '휘발성 유기 화합물',
	`Co2` smallint(6) DEFAULT NULL COMMENT '이산화탄소',
	`Noise` smallint(6) DEFAULT NULL COMMENT '소음',
	`Illumiance` smallint(6) DEFAULT NULL COMMENT '조도',
	`Movement` bigint(20) DEFAULT 0 COMMENT '모션감지',
	`RegDate` timestamp NULL DEFAULT current_timestamp() ON UPDATE current_timestamp() COMMENT '통신시각',
	`Dong` char(1) DEFAULT '' COMMENT '동',
	`Ho` smallint(6) DEFAULT NULL COMMENT '호실',
	`RSSI` bigint(20) DEFAULT 0,
	PRIMARY KEY (`No`) USING BTREE
) 
COMMENT='" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR) + "년" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) + "월 " + strDong.ToUpper() + @"동 이력 테이블'
COLLATE='utf8_general_ci'
ENGINE=InnoDB DEFAULT CHARSET=utf8
AUTO_INCREMENT=1;";
		}


		/// <summary>
		/// IOT7 DCU 별 정보(IP, DCU번호, 연결 센서 수) 취득쿼리문
		/// </summary>
		/// <returns>쿼리문</returns>
		public static string GetSelectQuery_IoT7DCUInfo_LOOKSERVER_IOT()
		{
			string strSelectQuery = @"SELECT `ip_addr`, `iot_no`, COUNT(*) AS `sensor_amount` FROM lookserver_iot GROUP BY ip_addr, iot_no ORDER BY ip_addr, iot_no;";
			return strSelectQuery;
		}

		/// <summary>
		/// DCU IP를 기준으로 연결된 객실 정보의 목록 취득 쿼리문 생성
		/// </summary>
		/// <param name="strIpAddr">DCU IP주소</param>
		/// <returns>쿼리문</returns>
		public static string GetSelectRoomListFromAddr_LOOKSERVER_IOT(string strIpAddr)
		{
			string strSelectQuery = @"SELECT `Dong`, `Ho`, `sensor_no`, `multi_sensor_no`, `connet` FROM lookserver_iot WHERE ip_addr = '" + strIpAddr + @"' ORDER BY multi_sensor_no ASC;";
			return strSelectQuery;
		}

		/// <summary>
		/// 객실의 IoT 센서 연결상태 갱신 쿼리문 취득.
		/// </summary>
		/// <param name="strDong"> 동 정보 </param>
		/// <param name="intHo"> 호 정보</param>
		/// <param name="isConnected"> 연결상태</param>
		/// <returns>쿼리문</returns>
		public static string GetUpdateQuery_LOOKSERVER_IOT(string strDong, int intHo, bool isConnected)
		{
			string strUpdateQuery = "UPDATE LookServer_iot SET connet = " + (isConnected ? "1" : "0")
					+ " WHERE Dong = \'" + strDong + "\' AND Ho = " + intHo + ";";

			return strUpdateQuery;
		}



		/// <summary>
		/// 객실 IoT센서 데이터 이력 추가 쿼리문 취득
		/// </summary>
		/// <param name="dtRegTIme">등록일시</param>
		/// <param name="mdlSensorData">등록할 센서 정보</param>
		/// <returns>데이터 입력 쿼리문</returns>
		public static string GetInsertQuery_ROOM_IOT(DateTime dtRegTIme, Model.MdlIOT7SensorData mdlSensorData)
		{
			string strValues = "(" + mdlSensorData.mdlRoomInfo.intSensorNo + ", " + mdlSensorData.intIRBtnCount
				+ ", " + mdlSensorData.dcmTemp + ", " + mdlSensorData.dcmHumidity + ", " + mdlSensorData.intVOCs
				+ ", " + mdlSensorData.intCo2 + ", " + mdlSensorData.intReserve + ", " + mdlSensorData.intIllumiance + ", " + mdlSensorData.intMovement
				+ ", \'" + dtRegTIme.ToString(MngQuery_Main.STR_DATE_FORMAT_DATA_ROW) + "\',\'"
				+ mdlSensorData.mdlRoomInfo.strDong.ToUpper() + "\', " + mdlSensorData.mdlRoomInfo.intHo.ToString() + ", " + mdlSensorData.intRSSI + ")";

			string strInsetQuery = "INSERT INTO `room" + mdlSensorData.mdlRoomInfo.strDong.ToLower() + "iot" + dtRegTIme.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR + MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) + "` ("
				+ string.Join(MngQuery_Main.STR_SEPARATOR, ARR_ROOM_IOT_DATA_ROWS) + ") VALUES " + strValues + ";";

			return strInsetQuery;
		}

		/// <summary>
		/// 객실 IoT센서 데이터 상태 갱신 쿼리문 취득
		/// </summary>
		/// <param name="dtRegTIme">등록일시</param>
		/// <param name="mdlSensorData">등록할 센서 정보</param>
		/// <returns>갱신 쿼리문</returns>
		public static string GetUpdateQuery_ROOM_IOT_CUR(DateTime dtRegTIme, Model.MdlIOT7SensorData mdlSensorData)
		{
			string strUpdateQuery = "UPDATE `room"+ mdlSensorData.mdlRoomInfo.strDong.ToLower() + "iot_cur`"
			+ " SET Aircon_state = " + mdlSensorData.intIRBtnCount 
			+ ", Temp = " + mdlSensorData.dcmTemp
			+ ", Humidity = " + mdlSensorData.dcmHumidity
			+ ", VOCs = " + mdlSensorData.intVOCs
			+ ", Co2 = " + mdlSensorData.intCo2 
			+ ", Noise = " + mdlSensorData.intReserve
			+ ", Illumiance = " + mdlSensorData.intIllumiance
			+ ", Movement = " + mdlSensorData.intMovement
			+ ", RegDate =\'" + dtRegTIme.ToString(Manager.MngQuery_Main.STR_DATE_FORMAT_DATA_ROW) + "\'"
			+ ", rssi = " + mdlSensorData.intRSSI
			+ " WHERE Dong =\'" + mdlSensorData.mdlRoomInfo.strDong.ToUpper() + "\'"
			+ " AND Ho=" + mdlSensorData.mdlRoomInfo.intHo+";";

			return strUpdateQuery;
		}
	}
}

using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMI_Windows.Manager
{
	class MngQuery_GreenHub
	{
		/// <summary> 변온소 계측데이터 항목명 </summary>
		private static string[] ARR_CENTER_DATA_ROWS = { "regdate", "temp_1", "temp_2", "temp_3", "temp_4", "temp_5", "temp_6"
				, "press_1", "press_2", "press_3", "press_4", "press_5", "press_6", "flow_1", "flow_2", "flow_3"
				, "pafc_n_v_amount", "pafc_t_v_amount", "pafc_y_v_amount", "pafc_n_h_amount", "pafc_t_h_amount", "pafc_y_h_amount"
				, "orc_n_v_amount", "orc_t_v_amount", "orc_y_v_amount", "hc_n_h_amount", "hc_t_h_amount", "hc_y_h_amount"
				, "pv_n_v_amount", "pv_t_v_amount", "pv_y_v_amount" };

		/// <summary> 변온소 외기데이터 항목명(YYYYMM테이블 기준) </summary>
		private static string[] ARR_WEATHER_DATA_ROWS = { "reg_date", "air_temp", "air_humi" };

		/// <summary> 이기종 ORC 계측데이터 항목명(YYYYMM테이블 기준) </summary>
		private static string[] ARR_ORC_DATA_ROWS = { "reg_date", "pms_power", "pms_pwr_acc" };

		/// <summary> 이기종 PNC 계측데이터 항목명(YYYYMM테이블 기준) </summary>
		private static string[] ARR_PNC_DATA_ROWS = { "reg_date", "pwr_trans_out", "pwr_trans_acc", "pwr_recv_acc" };

		/// <summary> 이기종 태양광발전기 계측데이터 항목명(YYYYMM테이블 기준) </summary>
		private static string[] ARR_PV_DATA_ROWS = { "reg_date", "pv_output", "pv_day_acc", "pv_total_acc", "inosolation", "panel_temp" };

		/// <summary> 이기종 수차 계측데이터 항목명(YYYYMM테이블 기준) </summary>
		private static string[] ARR_WHEEL_DATA_ROWS = { "reg_date", "elec_output", "elec_permin" };

		/// <summary> 이기종 열량계 계측데이터 항목명(YYYYMM테이블 기준) </summary>
		private static string[] ARR_METER_DATA_ROWS = { "reg_date", "acc_calorie", "cur_calorie", "acc_flux", "cur_flux", "sup_temp", "rec_temp", "cur_press" };

		/// <summary> 날짜 시작 서식 </summary>
		public const string STR_DATE_FORMAT_DAY_START = "yyyy-MM-dd 00:00:00";

		/// <summary> 날짜 시작 서식 </summary>
		public const string STR_DATE_FORMAT_DAY_END = "yyyy-MM-dd 23:59:59";

		/// <summary>
		/// 적산계산 시작시간의 DB문장형 데이터를 취득
		/// </summary>
		/// <param name="dtTargetTime">대상날짜정보</param>
		/// <returns>String형 데이터</returns>
		private static string GetDayAccStartTime(DateTime dtTargetTime)
		{
			//[E2S_TEST_0722_BK] 적산값 계산간격 1
			return dtTargetTime.ToString(STR_DATE_FORMAT_DAY_START);
			//[E2S_TEST_0722] 적산값 계산간격 1시간화
			// return dtTargetTime.ToString("yyyy-MM-dd HH:00:00");
		}

		/// <summary>
		/// 적산계산 끝시간의 DB문장형 데이터를 취득
		/// </summary>
		/// <param name="dtTargetTime">대상날짜정보</param>
		/// <returns>String형 데이터</returns>
		private static string GetDayAccEndTime(DateTime dtTargetTime)
		{
			//[E2S_TEST_0722_BK] 적산값 계산간격 1일
			return dtTargetTime.ToString(STR_DATE_FORMAT_DAY_END);
			//[E2S_TEST_0722] 적산값 계산간격 1시간화
			// return dtTargetTime.ToString("yyyy-MM-dd HH:59:59");
		}
		/// <summary>
		/// 그린허브 이기종 ORC데이터 테이블 생성 쿼리문 취득
		/// </summary>
		/// <param name="dtRegTime">테이블 생성일자</param>
		/// <returns></returns>
		public static string GetCreateQuery_ORC(DateTime dtRegTime)
		{
			string strCreateQuery = @"CREATE TABLE IF NOT EXISTS `gh_orc" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR + MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) + @"` (
	`no` BIGINT(20) NOT NULL AUTO_INCREMENT,
	`reg_date` TIMESTAMP NULL DEFAULT NULL,
	`pms_power` DECIMAL(10,2) NULL DEFAULT NULL,
	`pms_pwr_acc` DOUBLE(10,4) NULL DEFAULT NULL,
	PRIMARY KEY (`no`) USING BTREE
)
COLLATE='utf8_general_ci'
ENGINE=INNODB
AUTO_INCREMENT=1
COMMENT='" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR) + " 년 " + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) + @" ORC 측정값';";

			return strCreateQuery;
		}

		/// <summary>
		/// 그린허브 이기종 PNC
		/// </summary>
		/// <param name="dtRegTime"></param>
		/// <returns></returns>
		public static string GetCreateQuery_PNC(DateTime dtRegTime)
		{
			string strCreateQuery = @"CREATE TABLE IF NOT EXISTS `gh_pnc" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR + MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) + @"` (
	`no` BIGINT(20) NOT NULL AUTO_INCREMENT,
	`reg_date` TIMESTAMP NULL DEFAULT NULL,
	`pwr_trans_out` DECIMAL(10, 2) NULL DEFAULT NULL,
	`pwr_trans_acc` DECIMAL(10, 2) NULL DEFAULT NULL,
	`pwr_recv_acc` DECIMAL(10, 2) NULL DEFAULT NULL,
	PRIMARY KEY (`no`) USING BTREE
)
COLLATE='utf8_general_ci'
ENGINE=INNODB
AUTO_INCREMENT=1
COMMENT='" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR) + " 년 " + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) + @" 송전량 측정값';";

			return strCreateQuery;
		}


		/// <summary>
		/// 그린허브 이기종 태양광 발전기 월별 데이터 테이블 생성 쿼리문
		/// </summary>
		/// <param name="dtRegTime">등록월 정보</param>
		/// <returns>쿼리문</returns>
		public static string GetCreateQuery_PV(DateTime dtRegTime)
		{
			string strCreateQuery = @"CREATE TABLE IF NOT EXISTS `gh_pv" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR + MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) + @"` (
	`no` BIGINT(20) NOT NULL AUTO_INCREMENT,
	`reg_date` TIMESTAMP NULL DEFAULT NULL,
	`pv_output` SMALLINT(6) UNSIGNED NULL DEFAULT NULL,
	`pv_day_acc` SMALLINT(6) UNSIGNED NULL DEFAULT NULL,
	`pv_total_acc` INT(11) UNSIGNED NULL DEFAULT NULL,
	`inosolation` SMALLINT(6) UNSIGNED NULL DEFAULT NULL,
	`panel_temp` DECIMAL(6,1) NULL DEFAULT NULL,
	PRIMARY KEY (`no`) USING BTREE
)
COLLATE='utf8_general_ci'
ENGINE=INNODB
AUTO_INCREMENT=1
COMMENT='" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR) + " 년 " + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) + 
@"월 태양광 발전기 측정값'
;";
			return strCreateQuery;
		}

		/// <summary>
		/// 그린허브 이기종 수차 데이터 테이블 생성 쿼리문 취득
		/// </summary>
		/// <param name="dtRegTime">생성일자</param>
		/// <returns></returns>
		public static string GetCreateQuery_WaterWheel(DateTime dtRegTime)
		{
			string strCreateQuery = @"CREATE TABLE IF NOT EXISTS `gh_wwhl" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR + MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) + @"` (
	`no` BIGINT(20) NOT NULL AUTO_INCREMENT,
	`reg_date` TIMESTAMP NULL DEFAULT NULL,
	`elec_output` DECIMAL(10, 2) NULL DEFAULT NULL,
	`elec_permin` DECIMAL(10, 2) NULL DEFAULT NULL,
	PRIMARY KEY (`no`) USING BTREE
)
COLLATE='utf8_general_ci'
ENGINE=INNODB
AUTO_INCREMENT=1
COMMENT='" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR) + " 년 " + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) + @" 수차 측정값';";

			return strCreateQuery;
		}

		/// <summary>
		/// 그린허브 이기종 열량계 데이터 테이블 생성 쿼리문 취득
		/// </summary>
		/// <param name="dtRegTime">생성일자</param>
		/// <returns></returns>
		public static string GetCreateQuery_CalorieMeter(DateTime dtRegTime)
		{
			string strCreateQuery = @"CREATE TABLE IF NOT EXISTS `gh_meter" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR + MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) + @"` (
	`no` BIGINT(20) NOT NULL AUTO_INCREMENT,
	`reg_date` TIMESTAMP NULL DEFAULT NULL,
	`acc_calorie` DECIMAL(10, 2) NULL DEFAULT NULL,
	`cur_calorie` DECIMAL(10, 2) NULL DEFAULT NULL,
	`acc_flux` DECIMAL(10, 2) NULL DEFAULT NULL,
	`cur_flux` DECIMAL(10, 2) NULL DEFAULT NULL,
	`sup_temp` DECIMAL(10, 2) NULL DEFAULT NULL,
	`rec_temp` DECIMAL(10, 2) NULL DEFAULT NULL,
	`cur_press` DECIMAL(10, 2) NULL DEFAULT NULL,
	PRIMARY KEY (`no`) USING BTREE
)
COLLATE='utf8_general_ci'
ENGINE=INNODB
AUTO_INCREMENT=1
COMMENT='" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR) + " 년 " + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) + @" 열량계 측정값'
;";
			return strCreateQuery;
		}

		/// <summary>
		/// IoT7Smart_lab 변온소 측정값 테이블 생성 쿼리문 취득
		/// </summary>
		/// <param name="dtRegTime"></param>
		/// <returns></returns>
		public static string GetCreateQuery_SMART_LAB(DateTime dtRegTime)
		{
			return @"CREATE TABLE IF NOT EXISTS `smart_lab" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR + MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) + @"` (
	`no` BIGINT(20) NOT NULL AUTO_INCREMENT,
	`regdate` DATETIME NULL DEFAULT NULL COMMENT '측정시각',
	`temp_1` FLOAT NULL DEFAULT NULL,
	`temp_2` FLOAT NULL DEFAULT NULL,
	`temp_3` FLOAT NULL DEFAULT NULL,
	`temp_4` FLOAT NULL DEFAULT NULL,
	`temp_5` FLOAT NULL DEFAULT NULL,
	`temp_6` FLOAT NULL DEFAULT NULL,
	`press_1` FLOAT NULL DEFAULT NULL,
	`press_2` FLOAT NULL DEFAULT NULL,
	`press_3` FLOAT NULL DEFAULT NULL,
	`press_4` FLOAT NULL DEFAULT NULL,
	`press_5` FLOAT NULL DEFAULT NULL,
	`press_6` FLOAT NULL DEFAULT NULL,
	`flow_1` FLOAT NULL DEFAULT NULL,
	`flow_2` FLOAT NULL DEFAULT NULL,
	`flow_3` FLOAT NULL DEFAULT NULL,
	`pafc_n_v_amount` FLOAT NULL DEFAULT NULL,
	`pafc_t_v_amount` FLOAT NULL DEFAULT NULL,
	`pafc_y_v_amount` FLOAT NULL DEFAULT NULL,
	`pafc_n_h_amount` FLOAT NULL DEFAULT NULL,
	`pafc_t_h_amount` FLOAT NULL DEFAULT NULL,
	`pafc_y_h_amount` FLOAT NULL DEFAULT NULL,
	`orc_n_v_amount` FLOAT NULL DEFAULT NULL,
	`orc_t_v_amount` FLOAT NULL DEFAULT NULL,
	`orc_y_v_amount` FLOAT NULL DEFAULT NULL,
	`hc_n_h_amount` FLOAT NULL DEFAULT NULL,
	`hc_t_h_amount` FLOAT NULL DEFAULT NULL,
	`hc_y_h_amount` FLOAT NULL DEFAULT NULL,
	`pv_n_v_amount` FLOAT NULL DEFAULT NULL,
	`pv_t_v_amount` FLOAT NULL DEFAULT NULL,
	`pv_y_v_amount` FLOAT NULL DEFAULT NULL,
	PRIMARY KEY (`no`) USING BTREE
)
COMMENT='스마트변온소 " + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR) + " 년 " + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) + @" 월 데이터'
COLLATE='utf8_general_ci'
ENGINE=InnoDB
AUTO_INCREMENT=1;";
		}

		/// <summary>
		/// 그린허브 변온소 외기센서 월별 계측 데이터 테이블 생성
		/// </summary>
		/// <param name="dtRegDate">등록일자</param>
		/// <returns>테이블 생성 쿼리문</returns>
		public static string GetCreateQuery_SMART_WEATHER(DateTime dtRegDate)
		{
			string strCreateQuery = @"CREATE TABLE IF NOT EXISTS `smart_weather_data" + dtRegDate.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR + MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) + @"` (
	`no` BIGINT(20) NOT NULL AUTO_INCREMENT,
	`reg_date` TIMESTAMP NULL DEFAULT NULL,
	`air_temp` FLOAT NULL DEFAULT NULL,
	`air_humi` FLOAT NULL DEFAULT NULL,
	PRIMARY KEY (`No`) USING BTREE
)
COLLATE='utf8_general_ci'
ENGINE=INNODB
COMMENT='변온소 외기 " + dtRegDate.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR) + " 년 " + dtRegDate.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) + @" 월 데이터'
AUTO_INCREMENT=1;";

			return strCreateQuery;
		}

		/// <summary>
		/// ORC 현재값 테이블에서 누적전력량 취득
		/// </summary>
		/// <returns></returns>
		public static string GetSelectQuery_CurrentPwrAcc_GreenHub_ORC_CUR() 
		{ 
			string strSelectQuery = @"SELECT pms_pwr_acc FROM `gh_orc_cur` WHERE `no` = 1";

			return strSelectQuery;
		}

		/// <summary>
		/// 수차 현재값 테이블에서 누적 전력량 취득
		/// </summary>
		/// <returns></returns>
		public static string GetSelectQuery_CurrentPwrAcc_GreenHub_WWL_CUR()
		{
			string strSelectQuery = @"SELECT elec_acc FROM `gh_wwhl_cur` WHERE `no` = 1";

			return strSelectQuery;
		}

		public static string GetQuery_PwrDayAcc_LastData()
		{
			string strSelectQuery = @"SELECT
	`reg_date`			AS `DATE`
	,`orc_day_acc`		AS ORC_DAY
	,`pnc_tran_day_acc`	AS PNC_SEND
	,`pnc_recv_day_acc`	AS PNC_RECV
	,`pv_day_acc`		AS PV_DAY
	,`pv_total_acc`		AS PV_TOTAL
	,`wheel_day_acc`	AS WATAR_DAY
FROM
	`gh_pwr_dayacc`
ORDER BY `reg_date` DESC LIMIT 1;";
			return strSelectQuery;
		}

		/// <summary>
		/// 일일누적 전력량 데이터 로우 추가 쿼리문 취득
		/// </summary>
		/// <param name="dtRegTime">일일 적산량 계산대상날짜</param>
		/// <returns>추가 쿼리문</returns>
		public static string GetQuery_PwrDayAcc_RowAdd(DateTime dtRegTime)
		{
			string strQuery = @"REPLACE INTO `gh_pwr_dayAcc` ( `reg_date`,`orc_day_acc`,`pnc_tran_day_acc`,	`pnc_recv_day_acc`,`pv_day_acc`,`pv_total_acc`, `wheel_day_acc`) VALUES ('"
			+ GetDayAccStartTime(dtRegTime) + @"',0, 0, 0, 0, 0, 0);";

			return strQuery;
		}
		
		/// <summary>
		///  태양광 현재 총 누적 발전량값 쿼리문 
		/// </summary>
		/// <returns>태양광 에너지 총 누적 발전량 취득 쿼리문</returns>
		public static string GetQuery_PwrDayAcc_PvTotalAcc()
		{
			string strSelectQuery = @"SELECT IFNUll(MAX(pv_total_acc), 0) AS pv_total_acc FROM gh_pwr_dayacc;";

			return strSelectQuery;
		}

		/// <summary>
		/// 일일누적 전력량 데이터의 일일적산값 갱신 쿼리문 취득
		/// </summary>
		/// <param name="dtRegTime">일일 적산량 계산대상날짜</param>
		/// <returns>갱신 쿼리문</returns>
		public static string GetQuery_PwrDayAcc_Update(DateTime dtRegTime, int intPrevPVTotalAcc)
		{
			string strQuery = @"UPDATE 
	hmi.`gh_pwr_dayacc` AS DAYACC, 
	( SELECT IFNull(AVG(`pms_pwr_acc`) *" + 3600 / Frm_HMI_Main.intThreadGreenHubInterval + "*24, 0) AS orc_day_acc FROM gh_orc" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR + MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH)
	+@" WHERE reg_date BETWEEN '" + GetDayAccStartTime(dtRegTime) + @"' AND '" + GetDayAccEndTime(dtRegTime) + @"' ) AS ORC,
	( SELECT IFNull(MAX(pwr_trans_acc) - MIN(pwr_trans_acc), 0) AS pnc_tran_day_acc, IFNull(MAX(pwr_recv_acc) - MIN(pwr_recv_acc), 0) AS pnc_recv_day_acc FROM gh_pnc" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR + MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH)
	+@" WHERE reg_date BETWEEN '" + GetDayAccStartTime(dtRegTime) + @"' AND '" + GetDayAccEndTime(dtRegTime) + @"' ORDER BY reg_date DESC LIMIT 1) AS PNC,
	( SELECT IFNull(MAX(pv_day_acc), 0) AS pv_day_acc, IFNULL(MAX(pv_total_acc), " + intPrevPVTotalAcc + ") AS pv_total_acc FROM gh_pv" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR + MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH)
	+@" WHERE reg_date BETWEEN '" + GetDayAccStartTime(dtRegTime) + @"' AND '" + GetDayAccEndTime(dtRegTime) + @"' ORDER BY reg_date DESC LIMIT 1) AS PV,
	( SELECT IFNull(AVG(`elec_acc_permin`) *" + 3600 / Frm_HMI_Main.intThreadGreenHubInterval + "*24, 0)  AS wheel_day_acc FROM gh_wwhl" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR + MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH)
	+@" WHERE reg_date BETWEEN '" + GetDayAccStartTime(dtRegTime) + @"' AND '" + GetDayAccEndTime(dtRegTime) + @"' ) AS WATAR
SET
	DAYACC.reg_date = '" + GetDayAccStartTime(dtRegTime) + @"',
	DAYACC.orc_day_acc = ORC.orc_day_acc,
	DAYACC.pnc_tran_day_acc = PNC.pnc_tran_day_acc,
	DAYACC.pnc_recv_day_acc = PNC.pnc_recv_day_acc,
	DAYACC.pv_day_acc = PV.pv_day_acc,
	DAYACC.pv_total_acc = PV.pv_total_acc,
	DAYACC.wheel_day_acc = WATAR.wheel_day_acc
	WHERE DAYACC.reg_date = '" + GetDayAccStartTime(dtRegTime) + @"';";

			return strQuery;
		}

		/// <summary>
		/// 그린허브 이기종 ORC 데이터 삽입 쿼리문 취득
		/// </summary>
		/// <param name="dtRegTime">등록일시</param>
		/// <param name="dtORCData">ORC측정데이터</param>
		/// <returns>INSERT쿼리문</returns>
		public static string GetInsertQuery_GreenHub_ORC(DateTime dtRegTime, DataTable dtORCData)
		{
			string strInsertQuery = @"INSERT INTO `gh_orc" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR + MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) +
				@"` (" + string.Join(MngQuery_Main.STR_SEPARATOR, ARR_ORC_DATA_ROWS) + @") VALUES ('" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_DATA_ROW) + "\', "
				+ dtORCData.Rows[0].ItemArray[1] + MngQuery_Main.STR_SEPARATOR + dtORCData.Rows[0].ItemArray[2] +");";

			return strInsertQuery;
		}

		/// <summary>
		/// 그린허브 이기종 PNC 데이터 삽입 쿼리문 취득
		/// </summary>
		/// <param name="dtRegTime">등록일시</param>
		/// <param name="dtPNCData">PNC측정데이터</param>
		/// <returns>INSERT쿼리문</returns>
		public static string GetInsertQuery_GreenHub_PNC(DateTime dtRegTime, DataTable dtPNCData)
		{
			DataRow dr = dtPNCData.Rows[0];
			string strInsertQuery = @"INSERT INTO `gh_pnc" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR + MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) +
				@"` (" + string.Join(MngQuery_Main.STR_SEPARATOR, ARR_PNC_DATA_ROWS) + @") VALUES ('" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_DATA_ROW) + "\', "
				+ dr.ItemArray[1] + ", "
				+ dr.ItemArray[2] + ", "
				+ dr.ItemArray[3] + ");";

			return strInsertQuery;
		}

		/// <summary>
		/// 그린허브 이기종 태양광 발전기 계측데이터 삽입 쿼리문
		/// </summary>
		/// <param name="dtRegTime">태양광 발전기 데이터 등록 일시</param>
		/// <param name="dtPVGenData">태양광발전기 계측 데이터</param>
		/// <returns>데이터 삽입 쿼리문</returns>
		public static string GetInsertQuery_GreenHub_PVGen(DateTime dtRegTime, DataTable dtPVGenData)
		{
			DataRow dr = dtPVGenData.Rows[0];
			string strInsertQuery = @"INSERT INTO `gh_pv" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR + MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) +
				@"` (" + string.Join(MngQuery_Main.STR_SEPARATOR, ARR_PV_DATA_ROWS) + @") VALUES ('" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_DATA_ROW) + "\', "
				+ dr.ItemArray[1].ToString() + ", "
				+ dr.ItemArray[2].ToString() + ", "
				+ dr.ItemArray[3].ToString() + ", "
				+ dr.ItemArray[4].ToString() + ", "
				+ dr.ItemArray[5].ToString() + ");";

			return strInsertQuery;
		}

		/// <summary>
		/// 그린허브 이기종 수차 데이터 삽입 쿼리문 취득
		/// </summary>
		/// <param name="dtRegTime">수차 데이터 취득 일시</param>
		/// <param name="dtWWheelData">수차 데이터</param>
		/// <returns></returns>
		public static string GetInsertQuery_GreenHub_WaterWheel(DateTime dtRegTime, DataTable dtWWheelData)
		{
			DataRow dr = dtWWheelData.Rows[0];
			string strInsertQuery = @"INSERT INTO `gh_wwhl"+ dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR + MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) +
				@"` (" + string.Join(MngQuery_Main.STR_SEPARATOR, ARR_WHEEL_DATA_ROWS) + @") VALUES ('" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_DATA_ROW) + "\', "
				+ dr.ItemArray[1] + ", " + dr.ItemArray[2] + ");";

			return strInsertQuery;
		}

		/// <summary>
		/// 그린허브 이기종 열량계 데이터 삽입 쿼리문 취득
		/// </summary>
		/// <param name="dtRegTime">열량계 데이터 취득 일시</param>
		/// <param name="dtMeterData">열량계 데이터</param>
		/// <returns></returns>
		public static string GetInsertQuery_GreenHub_CalorieMeter(DateTime dtRegTime, DataTable dtMeterData)
		{
			DataRow dr = dtMeterData.Rows[0];
			string strInsertQuery = @"INSERT INTO `gh_meter" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR + MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) + 
				@"` (" + string.Join(MngQuery_Main.STR_SEPARATOR, ARR_METER_DATA_ROWS) + @") VALUES ('" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_DATA_ROW) + "\', "
				+ dr.ItemArray[1] + ", "
				+ dr.ItemArray[2] + ", "
				+ dr.ItemArray[3] + ", "
				+ dr.ItemArray[4] + ", "
				+ dr.ItemArray[5] + ", "
				+ dr.ItemArray[6] + ", "
				+ dr.ItemArray[7] + ");";

			return strInsertQuery;
		}

		/// <summary>
		/// 변온소 센터 측정값 저장 쿼리문 취득
		/// </summary>
		/// <param name="dtRegDate">측정시간</param>
		/// <param name="dtInsertData">취득한 측정값 데이터</param>
		/// <returns>쿼리문</returns>
		public static string GetInsertQuery_SMART_LAB(DateTime dtRegDate, System.Data.DataTable dtInsertData)
		{
			System.Data.DataRow dr = dtInsertData.Rows[0];
			object[] arrObjBuffer = new object[(dr.ItemArray.Length - 1)];
			Array.Copy(dr.ItemArray, 1, arrObjBuffer, 0, arrObjBuffer.Length);

			string strQuery = @"INSERT INTO `smart_lab" + dtRegDate.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR + MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH) 
				+ @"` (" + string.Join(MngQuery_Main.STR_SEPARATOR, ARR_CENTER_DATA_ROWS)
				+ @") VALUES ('" + dtRegDate.ToString(MngQuery_Main.STR_DATE_FORMAT_DATA_ROW) + @"', " + string.Join(MngQuery_Main.STR_SEPARATOR, arrObjBuffer) + ");";

			return strQuery;
		}

		/// <summary>
		/// 변온소 외기 측정값 저장 쿼리문 취득
		/// </summary>
		/// <param name="dtRegDate">측정시간</param>
		/// <param name="dtInsertData">취득한 측정값 데이터</param>
		/// <returns>쿼리문</returns>
		public static string GetInsertQuery_SMART_WEATHER(DateTime dtRegDate, System.Data.DataTable dtWeatherData)
		{
			System.Data.DataRow dr = dtWeatherData.Rows[0];
			object[] arrObjBuffer = new object[(dr.ItemArray.Length - 1)];
			Array.Copy(dr.ItemArray, 1, arrObjBuffer, 0, arrObjBuffer.Length);

			string strQuery = @"INSERT INTO `smart_weather_data" + dtRegDate.ToString(MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_YEAR + MngQuery_Main.STR_DATE_FORMAT_TABLE_NAME_MONTH)
				+ @"` (" + string.Join(MngQuery_Main.STR_SEPARATOR, ARR_WEATHER_DATA_ROWS)
				+ @") VALUES ('" + dtRegDate.ToString(MngQuery_Main.STR_DATE_FORMAT_DATA_ROW) + @"', " + string.Join(MngQuery_Main.STR_SEPARATOR, arrObjBuffer) + ");";

			return strQuery;
		}

		/// <summary>
		/// ORC현재값 테이블에서 총 적산량 취득 쿼리문 취득
		/// </summary>
		/// <returns>SELECT 쿼리문</returns>
		public static string GetPwrTotalAcc_GreenHub_ORC_Cur()
		{
			string strSelectQuery = @"SELECT pms_pwr_acc FROM gh_orc_cur ORDER BY no DESC Limit 1;";

			return strSelectQuery;
		}

		/// <summary>
		/// ORC 계측데이터 현재값 갱신 쿼리문
		/// </summary>
		/// <param name="dtRegTime"> ORC 계측데이터 갱신일시</param>
		/// <param name="dtORCData"> ORC 계측데이터</param>
		/// <returns>갱신 쿼리문</returns>
		public static string GetUpdateQuery_GreenHub_ORC_Cur(DateTime dtRegTime, DataTable dtORCData, Double? dblPwrAcc)
		{
			Double dblCurPwrAccValue = 0;
			Double dblCurValue = Convert.ToDouble(dtORCData.Rows[0].ItemArray[2].ToString());
			if (dblPwrAcc.HasValue)
			{
				dblCurPwrAccValue = dblPwrAcc.Value + dblCurValue;
			}
			else
			{
				dblCurPwrAccValue = dblCurValue;
			}
			string strUpdateQuery = @"UPDATE `gh_orc_cur` SET "
			+ ARR_ORC_DATA_ROWS[0] + " = \'" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_DATA_ROW) + "\', "
			+ ARR_ORC_DATA_ROWS[1] + " = " + dtORCData.Rows[0].ItemArray[1] + MngQuery_Main.STR_SEPARATOR
			+ ARR_ORC_DATA_ROWS[2] + " = " + dblCurPwrAccValue.ToString("F4")
			+ " WHERE no = 1";

			return strUpdateQuery;
		}

		/// <summary>
		/// PNC 계측데이터 현재값 갱신 쿼리문
		/// </summary>
		/// <param name="dtRegTime">PNC 데이터 갱신일시</param>
		/// <param name="dtPNCData">PNC 데이터</param>
		/// <returns>갱신 쿼리문</returns>
		public static string GetUpdateQuery_GreenHub_PNC_Cur(DateTime dtRegTime, DataTable dtPNCData)
		{
			string strUpdateQuery = @"UPDATE `gh_pnc_cur` SET "
			+ ARR_PNC_DATA_ROWS[0] + " = \'" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_DATA_ROW) + "\', "
			+ ARR_PNC_DATA_ROWS[1] + " = " + dtPNCData.Rows[0].ItemArray[1] + ", "
			+ ARR_PNC_DATA_ROWS[2] + " = " + dtPNCData.Rows[0].ItemArray[2] + ", "
			+ ARR_PNC_DATA_ROWS[3] + " = " + dtPNCData.Rows[0].ItemArray[3]
			+ " WHERE no = 1";

			return strUpdateQuery;
		}

		/// <summary>
		/// 태양광발전기 계측데이터 현재값 갱신 쿼리문
		/// </summary>
		/// <param name="dtRegTime">태양광 발전기 계측데이터 갱신일시</param>
		/// <param name="dtPVGenData">태양광발전기 계측데이터</param>
		/// <returns>갱신 쿼리문</returns>
		public static string GetUpdateQuery_GreenHub_PVGen_Cur(DateTime dtRegTime, DataTable dtPVGenData)
		{
			string strUpdateQuery = @"UPDATE `gh_pv_cur` SET "
			+ ARR_PV_DATA_ROWS[0] + " = \'" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_DATA_ROW) + "\', "
			+ ARR_PV_DATA_ROWS[1] + " = " + dtPVGenData.Rows[0].ItemArray[1] + ", "
			+ ARR_PV_DATA_ROWS[2] + " = " + dtPVGenData.Rows[0].ItemArray[2] + ", "
			+ ARR_PV_DATA_ROWS[3] + " = " + dtPVGenData.Rows[0].ItemArray[3] + ", "
			+ ARR_PV_DATA_ROWS[4] + " = " + dtPVGenData.Rows[0].ItemArray[4] + ", "
			+ ARR_PV_DATA_ROWS[5] + " = " + dtPVGenData.Rows[0].ItemArray[5]
			+ " WHERE no = 1";

			return strUpdateQuery;
		}


		/// <summary>
		/// 수차 계측데이터 현재값 갱신 쿼리문
		/// </summary>
		/// <param name="dtRegTime"> 수차 계측데이터 갱신일시</param>
		/// <param name="dtData"> 수차 계측데이터</param>
		/// <param name="dcmCurrentAccValue">현재 총 적산값</param>
		/// <returns>갱신 쿼리문</returns>
		public static string GetUpdateQuery_GreenHub_WWHL_Cur(DateTime dtRegTime, DataTable dtData, Decimal dcmCurrentAccValue)
		{
			string strUpdateQuery = @"UPDATE `gh_wwhl_cur` SET "
			+ "`reg_date`" + " = \'" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_DATA_ROW) + "\', "
			+ "`elec_output`" + " = " + dtData.Rows[0].ItemArray[1] + MngQuery_Main.STR_SEPARATOR
			+ "`elec_acc`" + " = " + (dcmCurrentAccValue + Convert.ToDecimal(dtData.Rows[0].ItemArray[2])).ToString("F2")
			+ " WHERE no = 1";

			return strUpdateQuery;
		}

		/// <summary>
		/// 열량계 계측데이터 현재값 갱신 쿼리문
		/// </summary>
		/// <param name="dtRegTime"> 열량계 계측데이터 갱신일시</param>
		/// <param name="dtData"> 열량계 계측데이터</param>
		/// <returns>갱신 쿼리문</returns>
		public static string GetUpdateQuery_GreenHub_METER_Cur(DateTime dtRegTime, DataTable dtData)
		{
			string strUpdateQuery = @"UPDATE `gh_meter_cur` SET "
			+ ARR_METER_DATA_ROWS[0] + " = \'" + dtRegTime.ToString(MngQuery_Main.STR_DATE_FORMAT_DATA_ROW) + "\', "
			+ ARR_METER_DATA_ROWS[1] + " = " + dtData.Rows[0].ItemArray[1] + ", "
			+ ARR_METER_DATA_ROWS[2] + " = " + dtData.Rows[0].ItemArray[2] + ", "
			+ ARR_METER_DATA_ROWS[3] + " = " + dtData.Rows[0].ItemArray[3] + ", "
			+ ARR_METER_DATA_ROWS[4] + " = " + dtData.Rows[0].ItemArray[4] + ", "
			+ ARR_METER_DATA_ROWS[5] + " = " + dtData.Rows[0].ItemArray[5] + ", "
			+ ARR_METER_DATA_ROWS[6] + " = " + dtData.Rows[0].ItemArray[6] + ", "
			+ ARR_METER_DATA_ROWS[7] + " = " + dtData.Rows[0].ItemArray[7]
			+ " WHERE no = 1";

			return strUpdateQuery;
		}

		/// <summary>
		/// 변온소 현재값 갱신 쿼리문 취득
		/// </summary>
		/// <param name="dtRegDate">등록일시</param>
		/// <param name="dtInsertData">추가할 데이터 (데이터 테이블)</param>
		/// <returns>쿼리문</returns>
		public static string GetUpdateQuery_SMART_LAB_CUR(DateTime dtRegDate, System.Data.DataTable dtInsertData)
		{

			string strQuery = "UPDATE `smart_lab_cur` SET regdate = \'" + dtRegDate.ToString(MngQuery_Main.STR_DATE_FORMAT_DATA_ROW) + "\'" + MngQuery_Main.STR_SEPARATOR;

			System.Data.DataRow dr = dtInsertData.Rows[0];

			for (int intCnt = 1; intCnt < ARR_CENTER_DATA_ROWS.Length; intCnt++)
			{
				strQuery += ARR_CENTER_DATA_ROWS[intCnt] + " = " + dr[intCnt] + " ";

				if (intCnt < ARR_CENTER_DATA_ROWS.Length - 1)
				{
					strQuery += MngQuery_Main.STR_SEPARATOR;
				}
			}

			strQuery += "WHERE No = 1;";

			return strQuery;
		}

		/// <summary>
		/// 변온소 외기 현재값 갱신 쿼리문 취득
		/// </summary>
		/// <param name="dtRegDate">등록일시</param>
		/// <param name="dtUpdateData">추가할 데이터 (데이터 테이블)</param>
		/// <returns>쿼리문</returns>
		public static string GetUpdateQuery_SMART_WEATHER_CUR(DateTime dtRegDate, System.Data.DataTable dtUpdateData)
		{

			string strQuery = "UPDATE `smart_weather_data_cur` SET "+ ARR_WEATHER_DATA_ROWS[0] + " = \'" + dtRegDate.ToString(MngQuery_Main.STR_DATE_FORMAT_DATA_ROW) + "\'" + MngQuery_Main.STR_SEPARATOR
				+ ARR_WEATHER_DATA_ROWS[1] + " = " + dtUpdateData.Rows[0].ItemArray[1].ToString() + MngQuery_Main.STR_SEPARATOR
				+ ARR_WEATHER_DATA_ROWS[2] + " = " + dtUpdateData.Rows[0].ItemArray[2].ToString() + ";";

			return strQuery;
		}

	}
}

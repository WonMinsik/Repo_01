using System;
using System.Data;

namespace HMI_Windows.Manager
{
    class Mng_DataTable
    {
        public static DataTable GetDataTableTemplate_WeatherData()
        {
            DataTable dtTable = new DataTable("WeatherData");
            dtTable.Columns.Add("ConnectTime", typeof(DateTime));
            dtTable.Columns.Add("AddrBlock", typeof(string));
            dtTable.Columns.Add("FuncCode", typeof(string));
            dtTable.Columns.Add("RecvByteLength", typeof(string));
            dtTable.Columns.Add("Status", typeof(int));
            dtTable.Columns.Add("Wind_Dir", typeof(int));
            dtTable.Columns.Add("Wind_Spd", typeof(double));
            dtTable.Columns.Add("Temp", typeof(double));
            dtTable.Columns.Add("Humi", typeof(double));
            dtTable.Columns.Add("AirPress", typeof(double));
            dtTable.Columns.Add("Weather", typeof(int));
            dtTable.Columns.Add("Rain_Fall", typeof(double));
            dtTable.Columns.Add("Rain_FallAcc", typeof(double));
            dtTable.Columns.Add("Rain_Unit", typeof(int));
            dtTable.Columns.Add("Radiation_Acc", typeof(double));
            dtTable.Columns.Add("Radiation", typeof(double));
            dtTable.Columns.Add("CheckBlock", typeof(int));
            return dtTable;
        }

        public static DataTable GetDataTableTemplate_ConnectionByteData(string strTableName)
        {
            DataTable dtTable = new DataTable(strTableName);
            dtTable.Columns.Add("RoomCd", typeof(string));
            dtTable.Columns.Add("ConnectTime", typeof(DateTime));
            dtTable.Columns.Add("SendData", typeof(string));
            dtTable.Columns.Add("RecvData", typeof(string));

            dtTable.Columns["SendData"].DefaultValue = DBNull.Value;
            dtTable.Columns["RecvData"].DefaultValue = DBNull.Value;

            return dtTable;
        }

        public static DataTable GetDataTableTemplate_RoomB_UDP(string strTableName)
        {
            DataTable dtTable = new DataTable(strTableName);
            dtTable.Columns.Add("RoomCd", typeof(string));
            dtTable.Columns.Add("ConnectTime", typeof(DateTime));
            dtTable.Columns.Add("InsideTemp", typeof(float));
            dtTable.Columns.Add("SetTemp", typeof(float));
            dtTable.Columns.Add("InHeating", typeof(float));
            dtTable.Columns.Add("OutHeating", typeof(float));
            dtTable.Columns.Add("NowCtrlValue", typeof(Int16));
            dtTable.Columns.Add("SetCtrlValue", typeof(float));
            dtTable.Columns.Add("NowFlow", typeof(float));
            dtTable.Columns.Add("TotalFlow", typeof(float));
            dtTable.Columns.Add("On_Off", typeof(Int16));
            dtTable.Columns.Add("TotalHeat", typeof(float));
            dtTable.Columns.Add("DeltaTTemp", typeof(Int16));
            dtTable.Columns.Add("FloorTemp", typeof(Int16));
            dtTable.Columns.Add("ProcTime", typeof(double));
            return dtTable;
        }

        /// <summary>
        /// 숙소동 기계실 온도 테이블
        /// </summary>
        /// <returns>테이블</returns>
        public static DataTable GetDataTableTemplate_MR_Acc_Tmp()
        {
            DataTable dtTable = new DataTable(Connect.CntMR.STR_TABLE_NAME_MR_ACC_TMP);

            dtTable.Columns.Add("DateTime",typeof(DateTime));
            dtTable.Columns.Add("T121_Tmp01", typeof(Single));
            dtTable.Columns.Add("T122_Tmp02", typeof(Single));
            dtTable.Columns.Add("T123_Tmp03", typeof(Single));
            dtTable.Columns.Add("T124_Tmp04", typeof(Single));
            dtTable.Columns.Add("T125_Tmp05", typeof(Single));
            dtTable.Columns.Add("T221_Tmp06", typeof(Single));
            dtTable.Columns.Add("T222_Tmp07", typeof(Single));
            dtTable.Columns.Add("T223_Tmp08", typeof(Single));
            dtTable.Columns.Add("T224_Tmp09", typeof(Single));
            dtTable.Columns.Add("T225_Tmp10", typeof(Single));
            dtTable.Columns.Add("T226_Tmp11", typeof(Single));
            

            return dtTable;
        }

        /// <summary>
        /// 숙소동 기계실 압력 테이블
        /// </summary>
        /// <returns>테이블</returns>
        public static DataTable GetDataTableTemplate_MR_Acc_Prs()
        {
            DataTable dtTable = new DataTable(Connect.CntMR.STR_TABLE_NAME_MR_ACC_PRS);

            dtTable.Columns.Add("DateTime", typeof(DateTime));
            dtTable.Columns.Add("P121_Prs01", typeof(Single));
            dtTable.Columns.Add("P122_Prs02", typeof(Single));
            dtTable.Columns.Add("P123_Prs03", typeof(Single));
            dtTable.Columns.Add("P124_Prs04", typeof(Single));
            dtTable.Columns.Add("P125_Prs05", typeof(Single));
            dtTable.Columns.Add("P221_Prs06", typeof(Single));
            dtTable.Columns.Add("P222_Prs07", typeof(Single));
            dtTable.Columns.Add("P223_Prs08", typeof(Single));
            dtTable.Columns.Add("P224_Prs09", typeof(Single));

            return dtTable;
        }

        /// <summary>
        /// 숙소동 기계실 유량 테이블
        /// </summary>
        /// <returns>테이블</returns>
        public static DataTable GetDataTableTemplate_MR_Acc_Flw()
        {
            DataTable dtTable = new DataTable(Connect.CntMR.STR_TABLE_NAME_MR_ACC_FLW);

            dtTable.Columns.Add("DateTime", typeof(DateTime));
            dtTable.Columns.Add("M123_Flw01", typeof(Single));
            dtTable.Columns.Add("M124_Flw02", typeof(Single));
            dtTable.Columns.Add("M125_Flw03", typeof(Single));
            dtTable.Columns.Add("M223_Flw04", typeof(Single));

            return dtTable;
        }

        /// <summary>
        /// 숙소동 기계실 밸브 테이블
        /// </summary>
        /// <returns>테이블</returns>
        public static DataTable GetDataTableTemplate_MR_Acc_Vlv()
        {
            DataTable dtTable = new DataTable(Connect.CntMR.STR_TABLE_NAME_MR_ACC_VLV);

            dtTable.Columns.Add("DateTime", typeof(DateTime));
            dtTable.Columns.Add("V122_Vlv01", typeof(Single));
            dtTable.Columns.Add("V123_Vlv02", typeof(Single));
            dtTable.Columns.Add("V124_Vlv03", typeof(Single));
            dtTable.Columns.Add("V125_Vlv04", typeof(Single));

            return dtTable;
        }

        /// <summary>
        /// 연구동 기계실 온도 테이블
        /// </summary>
        /// <returns>테이블</returns>
        public static DataTable GetDataTableTemplate_MR_Lab_Tmp()
        {
            DataTable dtTable = new DataTable(Connect.CntMR.STR_TABLE_NAME_MR_LAB_TMP);

            dtTable.Columns.Add("DateTime", typeof(DateTime));
            dtTable.Columns.Add("T111_Tmp01", typeof(Single));
            dtTable.Columns.Add("T112_Tmp02", typeof(Single));
            dtTable.Columns.Add("T113_Tmp03", typeof(Single));
            dtTable.Columns.Add("T114_Tmp04", typeof(Single));
            dtTable.Columns.Add("T211_Tmp05", typeof(Single));
            dtTable.Columns.Add("T212_Tmp06", typeof(Single));
            dtTable.Columns.Add("T213_Tmp07", typeof(Single));
            dtTable.Columns.Add("T214_Tmp08", typeof(Single));

            return dtTable;
        }

        /// <summary>
        /// 연구동 기계실 압력 유량
        /// </summary>
        /// <returns>테이블</returns>
        public static DataTable GetDataTableTemplate_MR_Lab_Prs_Flw()
        {
            DataTable dtTable = new DataTable(Connect.CntMR.STR_TABLE_NAME_MR_LAB_PRS_FLW);

            dtTable.Columns.Add("DateTime", typeof(DateTime));
            dtTable.Columns.Add("P111_Prs01", typeof(Single));
            dtTable.Columns.Add("P112_Prs02", typeof(Single));
            dtTable.Columns.Add("P113_Prs03", typeof(Single));
            dtTable.Columns.Add("P114_Prs04", typeof(Single));
            dtTable.Columns.Add("P211_Prs05", typeof(Single));
            dtTable.Columns.Add("P212_Prs06", typeof(Single));
            dtTable.Columns.Add("M113_Flw01", typeof(Single));
            dtTable.Columns.Add("M211_Flw02", typeof(Single));

            return dtTable;
        }

        /// <summary>
        /// IoT 탭 리스트박스 갱신
        /// </summary>
        /// <param name="dtOrigin">원본데이터</param>
        /// <param name="lstbxTarget">갱신대상</param>
        public static void SetIOTListBoxDataSource(DataTable dtOrigin, ref System.Windows.Forms.ListBox lstbxTarget)
        {
            lstbxTarget.Items.Clear();
            lstbxTarget.Items.Add("ALL");
            foreach (DataRow dr in dtOrigin.Rows)
            {
                lstbxTarget.Items.Add(dr["ROOM"].ToString());
            }
        }

        /// <summary>
        /// UDP 탭 리스트박스 갱신
        /// </summary>
        /// <param name="dtOrigin">원본데이터</param>
        /// <param name="lstbxTarget">갱신대상</param>
        public static void SetUDPListBoxDataSource(DataTable dtOrigin, ref System.Windows.Forms.ListBox lstbxTarget)
        {
            lstbxTarget.Items.Clear();
            lstbxTarget.Items.Add("ALL");
            foreach (DataRow dr in dtOrigin.Rows)
            {
                lstbxTarget.Items.Add(dr["DONG"].ToString() + dr["FLOOR"].ToString() + dr["ROOM"].ToString());
            }
        }


        public static DataTable GetDataTableTemplate_FlowMeters()
        {
            DataTable dt = new DataTable("FlowMeter");
            dt.Columns.Add("FlowMeter_1", typeof(double));
            dt.Columns.Add("FlowMeter_2", typeof(double));
            dt.Columns.Add("FlowMeter_3", typeof(double));
            dt.Columns.Add("FlowMeter_4", typeof(double));

            return dt;
        }

        /// <summary>
        /// IOT객실 데이터 테이블 템플릿 취득
        /// </summary>
        /// <param name="strTableName">테이블명</param>
        /// <returns></returns>
        public static DataTable GetDataTableTemplate_Room_IOT(string strTableName)
        {
            DataTable dt = new DataTable(strTableName);
            dt.Columns.Add("LastConnectDate", typeof(DateTime));
            dt.Columns.Add("Dong", typeof(string));
            dt.Columns.Add("Ho", typeof(int));
            dt.Columns.Add("Sensor_no", typeof(int));
            dt.Columns.Add("Connect_Count", typeof(uint));
            dt.Columns.Add("Temp", typeof(int));
            dt.Columns.Add("Humidity", typeof(uint));
            dt.Columns.Add("Reserve", typeof(uint));
            dt.Columns.Add("VOCs", typeof(uint));
            dt.Columns.Add("Co2", typeof(uint));
            dt.Columns.Add("Illumiance", typeof(uint));
            dt.Columns.Add("Movement", typeof(uint));
            dt.Columns.Add("IRBtn_Count", typeof(uint));
            dt.Columns.Add("RSSI", typeof(uint));

            return dt;
        }

        /// <summary>
        /// 변온소 데이터 테이블 템플릿 취득
        /// </summary>
        /// <returns></returns>
        public static DataTable GetDataTableTemplate_SPCenterData()
        {
            DataTable dt = new DataTable(Connect.CntGreenHub.STR_GREENHUB_CENTER_DATA_TABLE_NAME);
            dt.Columns.Add("ConnectDate", typeof(DateTime));
            dt.Columns.Add("Temp_1", typeof(Double));
            dt.Columns.Add("Temp_2", typeof(Double));
            dt.Columns.Add("Temp_3", typeof(Double));
            dt.Columns.Add("Temp_4", typeof(Double));
            dt.Columns.Add("Temp_5", typeof(Double));
            dt.Columns.Add("Temp_6", typeof(Double));
            dt.Columns.Add("Press_1", typeof(Double));
            dt.Columns.Add("Press_2", typeof(Double));
            dt.Columns.Add("Press_3", typeof(Double));
            dt.Columns.Add("Press_4", typeof(Double));
            dt.Columns.Add("Press_5", typeof(Double));
            dt.Columns.Add("Press_6", typeof(Double));
            dt.Columns.Add("Flow_1", typeof(Double));
            dt.Columns.Add("Flow_2", typeof(Double));
            dt.Columns.Add("Flow_3", typeof(Double));
            dt.Columns.Add("Pafc_n_v_amount", typeof(Double));
            dt.Columns.Add("Pafc_t_v_amount", typeof(Double));
            dt.Columns.Add("Pafc_y_v_amount", typeof(Double));
            dt.Columns.Add("Pafc_n_h_amount", typeof(Double));
            dt.Columns.Add("Pafc_t_h_amount", typeof(Double));
            dt.Columns.Add("Pafc_y_h_amount", typeof(Double));
            dt.Columns.Add("orc_n_v_amount", typeof(Double));
            dt.Columns.Add("orc_t_v_amount", typeof(Double));
            dt.Columns.Add("orc_y_v_amount", typeof(Double));
            dt.Columns.Add("hc_n_h_amount", typeof(Double));
            dt.Columns.Add("hc_t_h_amount", typeof(Double));
            dt.Columns.Add("hc_y_h_amount", typeof(Double));
            dt.Columns.Add("pv_n_v_amount", typeof(Double));
            dt.Columns.Add("pv_t_v_amount", typeof(Double));
            dt.Columns.Add("pv_y_v_amount", typeof(Double));
            return dt;
        }

        /// <summary>
        /// 변온소 데이터 테이블 템플릿 취득
        /// </summary>
        /// <returns></returns>
        public static DataTable GetDataTableTemplate_SPWeatherData()
        {
            DataTable dt = new DataTable(Connect.CntGreenHub.STR_GREENHUB_WEATHER_DATA_TABLE_NAME);
            dt.Columns.Add("LastTime", typeof(DateTime));
            dt.Columns.Add("AirTemp", typeof(Double));
            dt.Columns.Add("AirHumi", typeof(Double));

            return dt;
        }

        /// <summary>
        /// 이기종 누적 전력량 데이터 테이블 취득
        /// </summary>
        /// <returns></returns>
        public static DataTable GetDataTableTemplate_PwrDayAccData()
        {
            DataTable dt = new DataTable(Connect.CntGreenHub.STR_GREENHUB_DAYPWR_DATA_TABLE_NAME);
            dt.Columns.Add("DATE", typeof(DateTime));
            dt.Columns.Add("ORC_DAY", typeof(Double));
            dt.Columns.Add("PNC_SEND", typeof(Decimal));
            dt.Columns.Add("PNC_RECV", typeof(Decimal));
            dt.Columns.Add("PV_DAY", typeof(Int16));
            dt.Columns.Add("PV_TOTAL", typeof(Int32));
            dt.Columns.Add("WATAR_DAY", typeof(Decimal));

            return dt;
        }

        /// <summary>
        /// 이기종 - ORC 데이터 템플릿 취득
        /// </summary>
        /// <returns></returns>
        public static DataTable GetDataTableTemplate_ORCData()
        {
            DataTable dt = new DataTable(Connect.CntGreenHub.STR_GREENHUB_ORC_DATA_TABLE_NAME);
            dt.Columns.Add("ConnectDate", typeof(DateTime));
            dt.Columns.Add("pms_pwr", typeof(Decimal));
            dt.Columns.Add("pms_pwr_acc", typeof(Double));

            return dt;
        }


        /// <summary>
        /// 이기종 - PNC(송전량) 데이터 템플릿 취득
        /// </summary>
        /// <returns></returns>
        public static DataTable GetDataTableTemplate_PNCData()
        {
            DataTable dt = new DataTable(Connect.CntGreenHub.STR_GREENHUB_PNC_DATA_TABLE_NAME);
            dt.Columns.Add("ConnectDate", typeof(DateTime));
            dt.Columns.Add("pwr_trans_out", typeof(Decimal));
            dt.Columns.Add("pwr_trans_acc", typeof(Decimal));
            dt.Columns.Add("pwr_recv_acc", typeof(Decimal));

            return dt;
        }

        /// <summary>
        /// 이기종 - PV(태양광발전기) 데이터 템플릿 취득
        /// </summary>
        /// <returns></returns>
        public static DataTable GetDataTableTemplate_PVGenData()
        {
            DataTable dt = new DataTable(Connect.CntGreenHub.STR_GREENHUB_PVGen_DATA_TABLE_NAME);
            dt.Columns.Add("ConnectDate", typeof(DateTime));
            dt.Columns.Add("pv_output", typeof(Int16));
            dt.Columns.Add("pv_day_acc", typeof(Int16));
            dt.Columns.Add("pv_total_acc", typeof(Int32));
            dt.Columns.Add("inosolation", typeof(Int16));
            dt.Columns.Add("panel_temp", typeof(Decimal));

            return dt;
        }

        /// <summary>
        /// 이기종 - 수차 데이터 테이블 템플릿 취득
        /// </summary>
        /// <returns></returns>
        public static DataTable GetDataTableTemplate_WheelData()
        {
            DataTable dt = new DataTable(Connect.CntGreenHub.STR_GREENHUB_WHEEL_DATA_TABLE_NAME);
            dt.Columns.Add("ConnectDate", typeof(DateTime));
            dt.Columns.Add("elec_output", typeof(Decimal));
            dt.Columns.Add("elec_acc_permin", typeof(Decimal));

            return dt;
        }

        /// <summary>
        /// 이기종 - 열량계 데이터 테이블 템플릿 취득
        /// </summary>
        /// <returns></returns>
        public static DataTable GetDataTableTemplate_MeterData()
        {
            DataTable dt = new DataTable(Connect.CntGreenHub.STR_GREENHUB_METER_DATA_TABLE_NAME);
            dt.Columns.Add("ConnectDate", typeof(DateTime));
            dt.Columns.Add("acc_calorie", typeof(Decimal));
            dt.Columns.Add("cur_calorie", typeof(Decimal));
            dt.Columns.Add("acc_flux", typeof(Decimal));
            dt.Columns.Add("cur_flux", typeof(Decimal));
            dt.Columns.Add("sup_temp", typeof(Decimal));
            dt.Columns.Add("rec_temp", typeof(Decimal));
            dt.Columns.Add("cur_press", typeof(Decimal));

            return dt;
        }


        /// <summary>
        /// 연결시간 칼럼을 가장 앞에 추가
        /// </summary>
        /// <param name="dt">대상 데이터 테이블</param>
        /// <param name="dtRegTime">추가할 시간정보</param>
        /// <returns>1열에 시간데이터를 추가한 데이터 테이블</returns>
        public static DataTable AddColumn_ConnectTime(DataTable dt, DateTime dtRegTime)
        {
            dt.Columns.Add("ConnectTime", typeof(DateTime));
            dt.Rows[0]["ConnectTime"] = dtRegTime;
            dt.Columns["ConnectTime"].SetOrdinal(0);

            return dt;
        }
 
    }

    
}

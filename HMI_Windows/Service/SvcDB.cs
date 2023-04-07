using System;
using System.Data;
using MySql.Data.MySqlClient;
using HMI_Windows.Model;
using HMI_Windows.Manager;

namespace HMI_Windows.Service
{
    /// <summary>
    /// 데이터베이스 입출력 관리 클래스
    /// </summary>
    class SvcDB
    {
        /// <summary> DB주소 </summary>
        // private static string STR_MYSQL_DB_DEFAULT_HOST = "192.168.0.1";
        private static string STR_MYSQL_DB_DEFAULT_HOST = "127.0.0.1";
        /// <summary> DB접속 포트 </summary>
        private static string STR_MYSQL_DB_DEFAULT_PORT = "3306";
        /// <summary> DB사용자 </summary>
        private static string STR_HMI_USER = "hmi";
        // private string STR_HMI_USER = "root";
        /// <summary> DB이름 </summary>
        private static string STR_HMI_DB_NAME = "hmi";
        /// <summary> DB패스워드 </summary>
        private static string STR_HMI_PASSWORD = "hmi";
        // private string STR_HMI_PASSWORD = "e2s48914";
        /// <summary>  </summary>
        private static string STR_CHARSET = "utf8";
        /// <summary> DB쿼리 실패시 재시도 횟수 </summary>
        private int INT_DB_QUEARY_RETRY_COUNT = 3;
        /// <summary> DB연결정보 </summary>
        private string STR_DB_CONNECTION_INFO_STRING;

        /// <summary>
        /// IoT7 객실동 유형 목록
        /// </summary>
        public enum enmRoomType
        {
            /// <summary> A동 </summary>
            DONG_A = 0,
            /// <summary> B동 </summary>
            DONG_B = 1,
            /// <summary> C동 </summary>
            DONG_C = 2,
        }

        /// <summary>
        /// 객실 연결상태 
        /// </summary>
        public enum enmConnectionStatus
        {
            CONNECTED = 1,
            DISCONNECTED = 0,
        }

        /// <summary>
        /// 리스트박스 객실동 유형
        /// </summary>
        public enum enmListBoxValuesTypes
        {
            UDP_ROOM_B = 0,
            IOT_ROOM_A = 1,
            IOT_ROOM_B = 2,
            IOT_ROOM_C = 3
        }


        public SvcDB()
        {
            STR_DB_CONNECTION_INFO_STRING = SetDBConnectionInfoString();
        }

        
        /// <summary>
        /// 접속정보 설정
        /// </summary>
        private string SetDBConnectionInfoString(params string[] arg)
        {
            return "Server=" + STR_MYSQL_DB_DEFAULT_HOST
                    + ";Port=" + STR_MYSQL_DB_DEFAULT_PORT
                    + ";Database=" + STR_HMI_DB_NAME
                    + ";Uid=" + STR_HMI_USER
                    + ";Pwd=" + STR_HMI_PASSWORD
                    + ";CharSet=" + STR_CHARSET;
        }



        /// <summary>
        /// 숙소동 밸브제어 값 취득
        /// </summary>
        /// <param name="intRetryCount">재시도 횟수</param>
        /// <returns>밸브제어값</returns>
        public DataTable GetValveControl(int intRetryCount = 0)
        {
            MySqlConnection msqlConn = null;
            DataTable dtReturnDataTable = new DataTable();
            if (intRetryCount > INT_DB_QUEARY_RETRY_COUNT)
            {
                return null;
            }
            try
            {
                string strQuery = MngQuery_MR.GetSelectQuery_VALVE_CONTORL();
                DataSet ds = new DataSet();
                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(strQuery, msqlConn);
                    mySqlDataAdapter.Fill(ds);
                    dtReturnDataTable = ds.Tables[0];

                    msqlConn.Close();
                }

                return dtReturnDataTable;
            }
            catch (DBConcurrencyException)
            {
                return GetValveControl(intRetryCount + 1);
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                if ((msqlConn != null) && msqlConn.State != ConnectionState.Closed)
                {
                    msqlConn.Close();
                    msqlConn.Dispose();
                }
            }
        }

        /// <summary>
        /// 밸브제어 결과 반영
        /// </summary>
        /// <param name="strValveName">밸브이름</param>
        /// <returns>처리 성공여부</returns>
        public bool UpdateValveControl_Flag(string strValveName)
        {
            MySqlConnection msqlConn = null;

            try
            {
                string strUpdateQuery = MngQuery_MR.GetUpdateQuery_VALVE_CONTROL(strValveName);

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlCommand mySqlCommand = new MySqlCommand(strUpdateQuery, msqlConn);
                    mySqlCommand.ExecuteReader();

                    msqlConn.Close();
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if ((msqlConn != null) && msqlConn.State != ConnectionState.Closed)
                {
                    msqlConn.Close();
                    msqlConn.Dispose();
                }

            }
        }

        /// <summary>
        /// 밸브 제어 이력 추가
        /// </summary>
        /// <param name="dtRegTime">제어 일시</param>
        /// <param name="strValve_name">제어 밸브명</param>
        /// <param name="strValue">제어값</param>
        /// <returns>제어 이력 추가 성공 여부</returns>
        public bool InsertValveControlRecord(DateTime dtRegTime, string strValve_name, string strValue)
        {
            MySqlConnection msqlConn = null;

            try
            {
                string strInsertQuery = MngQuery_MR.GetInsertQuery_VALVE_CONTROLRECORD(dtRegTime, strValve_name, strValue);

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlCommand mySqlCommUpd = new MySqlCommand(strInsertQuery, msqlConn);
                    mySqlCommUpd.ExecuteReader();

                    msqlConn.Close();
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if ((msqlConn != null) && msqlConn.State != ConnectionState.Closed)
                {
                    msqlConn.Close();
                    msqlConn.Dispose();
                }
            }
        }

        /// <summary>
        /// 객실 제어 명령 취득
        /// </summary>
        /// <param name="intRetryCount">재시도 횟수</param>
        /// <returns>미설정 제어 기록</returns>
        public DataTable GetControlRecord(int intRetryCount = 0)
        {
            MySqlConnection msqlConn = null;
            DataTable dtReturnDataTable = new DataTable();
            if (intRetryCount > INT_DB_QUEARY_RETRY_COUNT)
            {
                return null;
            }
            try
            {
                string strQuery = MngQuery_UDP.GetSelectQuery_CONTROLRECORD();
                DataSet ds = new DataSet();
                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(strQuery, msqlConn);
                    mySqlDataAdapter.Fill(ds);
                    if (ds.Tables.Count > 0)
                    {
                        dtReturnDataTable = ds.Tables[0];
                    }

                    msqlConn.Close();
                }

                return dtReturnDataTable;
            }
            catch (DBConcurrencyException)
            {
                return GetControlRecord(intRetryCount + 1);
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                if ((msqlConn != null) && msqlConn.State != ConnectionState.Closed)
                {
                    msqlConn.Close();
                    msqlConn.Dispose();
                }
            }
        }

        /// <summary>
        /// 제어기록 테이블 데이터 읽음/제어성공 상태로 전환
        /// </summary>
        /// <param name="mdlControlData">제어 명령 및 객실 정보</param>
        /// <returns>갱신 결과</returns>
        public bool UpdateControlRecord_Flag(MdlInPacketData mdlControlData)
        {
            MySqlConnection msqlConn = null;
            int intRetryCount = 0;
            bool isSQLOk = false;
            try
            {
                string strQuery = MngQuery_UDP.GetUpdateQuery_CONTROLRECORD(mdlControlData.intControlOrderNo, mdlControlData.mdlRoomInfo);

                do
                {
                    using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                    {
                        msqlConn.Open();

                        MySqlCommand mySqlCommand = new MySqlCommand(strQuery, msqlConn);
                        mySqlCommand.ExecuteReader();

                        msqlConn.Close();
                    }

                    isSQLOk = true;

                } while (!isSQLOk && intRetryCount < 3);


                if (isSQLOk)
                {
                    return true;
                }
                else
                {
                    return false;
                }
                
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if ((msqlConn != null) && msqlConn.State != ConnectionState.Closed)
                {
                    msqlConn.Close();
                    msqlConn.Dispose();
                }
            }
        }

        /// <summary>
        /// 객실 IoT 센서 취득 데이터 입력
        /// </summary>
        /// <param name="dtRegTime">등록일시</param>
        /// <param name="mdlSensorData">객실별 센서 취득 정보</param>
        /// <returns></returns>
        public bool InsertIoT7Data_ROOM_IOT(DateTime dtRegTime, Model.MdlIOT7SensorData mdlSensorData)
        {
            MySqlConnection msqlConn = null;
            try
            {
                string strCreateQuery = MngQuery_IoT7.GetCreateQuery_ROOM_IOT(mdlSensorData.mdlRoomInfo.strDong, dtRegTime);

                string strInsertQuery = MngQuery_IoT7.GetInsertQuery_ROOM_IOT(dtRegTime,  mdlSensorData);

                string strUpdateQuery = MngQuery_IoT7.GetUpdateQuery_ROOM_IOT_CUR(dtRegTime, mdlSensorData);

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlCommand mySqlCommand = new MySqlCommand(strCreateQuery, msqlConn);
                    mySqlCommand.ExecuteReader();

                    msqlConn.Close();
                }

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlCommand mySqlCommand = new MySqlCommand(strInsertQuery, msqlConn);
                    mySqlCommand.ExecuteReader();

                    msqlConn.Close();
                }

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlCommand mySqlCommand = new MySqlCommand(strUpdateQuery, msqlConn);
                    mySqlCommand.ExecuteReader();

                    msqlConn.Close();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if ((msqlConn != null) && msqlConn.State != ConnectionState.Closed)
                {
                    msqlConn.Close();
                    msqlConn.Dispose();
                }
            }
        }

        /// <summary>
        /// 숙소동 밸브제어 값 취득
        /// </summary>
        /// <returns>밸브제어값</returns>
        public DataTable GetRoomsUDPConnectTarget()
        {
            MySqlConnection msqlConn = null;
            DataTable dtReturnDataTable = new DataTable();

            try
            {
                string strQuery = MngQuery_UDP.GetSelectQuery_LOOKSERVER();
                DataSet ds = new DataSet();
                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(strQuery, msqlConn);
                    mySqlDataAdapter.Fill(ds);
                    dtReturnDataTable = ds.Tables[0];

                    msqlConn.Close();
                }

                return dtReturnDataTable;
            }
            catch (Exception)
            {
                return new DataTable();
            }
            finally
            {
                if ((msqlConn != null) && msqlConn.State != ConnectionState.Closed)
                {
                    msqlConn.Close();
                    msqlConn.Dispose();
                }
            }
        }

        /// <summary>
        /// gh_orc_cur 테이블에서 현재 누적값 취득.
        /// </summary>
        /// <param name="dblReturnValue">어플리케이션 기동후 현재 누적값</param>
        /// <returns>누적값 취득 성공여부</returns>
        public bool SelectGreenHubData_ORC_Cur_PwrAcc(out Double dblReturnValue)
        {
            MySqlConnection msqlConn = null;
            DataSet ds = new DataSet();
            try
            {
                string strSelectQuery = Manager.MngQuery_GreenHub.GetPwrTotalAcc_GreenHub_ORC_Cur();

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();
                    MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(strSelectQuery, msqlConn);
                    mySqlDataAdapter.Fill(ds);
                    msqlConn.Close();
                }

                dblReturnValue = Double.Parse(ds.Tables[0].Rows[0].ItemArray[0].ToString());

                return true;
            }
            catch (Exception)
            {
                dblReturnValue = Double.MinValue;
                return false;
            }
        }

        /// <summary>
        /// 이기종 - ORC 계측데이터 입력
        /// </summary>
        /// <param name="dtRegTime">데이터취득 시간</param>
        /// <param name="dtORCData">ORC 데이터</param>
        /// <param name="dblCurPwrAcc">ORC현재 누적 전력량</param>
        /// <returns>데이터 등록처리 성공여부</returns>
        public bool InsertGreenHubData_ORC(DateTime dtRegTime, DataTable dtORCData, Double? dblCurPwrAcc) 
        {
            MySqlConnection msqlConn = null;

            try
            {
                string strCreateQuery = Manager.MngQuery_GreenHub.GetCreateQuery_ORC(dtRegTime);

                string strInsertQuery = Manager.MngQuery_GreenHub.GetInsertQuery_GreenHub_ORC(dtRegTime,dtORCData);

                string strUpdateQuery = Manager.MngQuery_GreenHub.GetUpdateQuery_GreenHub_ORC_Cur(dtRegTime, dtORCData, dblCurPwrAcc);

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlCommand mySqlCommand = new MySqlCommand(strCreateQuery, msqlConn);
                    mySqlCommand.ExecuteReader();

                    msqlConn.Close();
                }

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlCommand mySqlCommand = new MySqlCommand(strInsertQuery, msqlConn);
                    mySqlCommand.ExecuteReader();

                    msqlConn.Close();
                }

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlCommand mySqlCommand = new MySqlCommand(strUpdateQuery, msqlConn);
                    mySqlCommand.ExecuteReader();

                    msqlConn.Close();
                }

                return true;
            }
            catch (Exception)
            {
                if (msqlConn.State != ConnectionState.Closed)
                {
                    msqlConn.Close();
                }
                msqlConn.Dispose();
            }

            return false;
        }


        /// <summary>
        /// 이기종 - PNC 데이터 입력
        /// </summary>
        /// <param name="dtRegTime">데이터취득 시간</param>
        /// <param name="dtPNCData">PNC 데이터</param>
        /// <returns></returns>
        public bool InsertGreenHubData_PNC(DateTime dtRegTime, DataTable dtPNCData) 
        {
            MySqlConnection msqlConn = null;

            try
            {
                string strCreateQuery = Manager.MngQuery_GreenHub.GetCreateQuery_PNC(dtRegTime);

                string strInsertQuery = Manager.MngQuery_GreenHub.GetInsertQuery_GreenHub_PNC(dtRegTime, dtPNCData);

                string strUpdateQuery = Manager.MngQuery_GreenHub.GetUpdateQuery_GreenHub_PNC_Cur(dtRegTime, dtPNCData);

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlCommand mySqlCommand = new MySqlCommand(strCreateQuery, msqlConn);
                    mySqlCommand.ExecuteReader();

                    msqlConn.Close();
                }

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlCommand mySqlCommand = new MySqlCommand(strInsertQuery, msqlConn);
                    mySqlCommand.ExecuteReader();

                    msqlConn.Close();
                }

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlCommand mySqlCommand = new MySqlCommand(strUpdateQuery, msqlConn);
                    mySqlCommand.ExecuteReader();

                    msqlConn.Close();
                }

                return true;
            }
            catch (Exception)
            {
                if (msqlConn.State != ConnectionState.Closed)
                {
                    msqlConn.Close();
                }
                msqlConn.Dispose();
            }

            return false;
        }

        /// <summary>
        /// 이기종 - 태양광발전기 계측 데이터 입력
        /// </summary>
        /// <param name="dtRegTime">등록일시</param>
        /// <param name="dtPVGData">발전기 데이터</param>
        /// <returns> 등록처리 성공여부 </returns>
        public bool InsertGreenHubData_PV(DateTime dtRegTime, DataTable dtPVGData)
        {
            MySqlConnection msqlConn = null;

            try
            {
                //2022.06.02 추가
                string strCreateQuery = Manager.MngQuery_GreenHub.GetCreateQuery_PV(dtRegTime);

                string strInsertQuery = Manager.MngQuery_GreenHub.GetInsertQuery_GreenHub_PVGen(dtRegTime, dtPVGData);

                string strUpdateQuery = Manager.MngQuery_GreenHub.GetUpdateQuery_GreenHub_PVGen_Cur(dtRegTime, dtPVGData);

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlCommand mySqlCommand = new MySqlCommand(strCreateQuery, msqlConn);
                    mySqlCommand.ExecuteReader();

                    msqlConn.Close();
                }

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlCommand mySqlCommand = new MySqlCommand(strInsertQuery, msqlConn);
                    mySqlCommand.ExecuteReader();

                    msqlConn.Close();
                }

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlCommand mySqlCommand = new MySqlCommand(strUpdateQuery, msqlConn);
                    mySqlCommand.ExecuteReader();

                    msqlConn.Close();
                }
            }
            catch (Exception)
            {
                if (msqlConn.State != ConnectionState.Closed)
                {
                    msqlConn.Close();
                }
                msqlConn.Dispose();
            }

            return true;
        }

        /// <summary>
        /// 이기종 일일누적전력량 계산
        /// </summary>
        /// <param name="dtRegTime">계산 대상 날짜</param>
        /// <returns> 등록처리후 최신 적산 데이터 (테이블) </returns>
        /// 스토어 프로시저로 교체 요망
        public DataTable AddCalcData_GreenHubData_PwrDayAcc(DateTime dtRegTime)
        {
            MySqlConnection msqlConn = null;
            DataSet dsPVTotalAcc = new DataSet();
            DataSet dsLastData = new DataSet();
            int intPrevPVTotalAcc = 0;
            try
            {
               //데이터 생선전 기존 데이터에서 태양광 누적 적산 발전량을 취득
                string strSelectTotalAccQuery = Manager.MngQuery_GreenHub.GetQuery_PwrDayAcc_PvTotalAcc();

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();
                    MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(strSelectTotalAccQuery, msqlConn);
                    mySqlDataAdapter.Fill(dsPVTotalAcc);
                    msqlConn.Close();
                }

                try
                {
                    DataTable dtBuffer = dsPVTotalAcc.Tables[0];
                    string strPvTotalAcc = dtBuffer.Rows[0].ItemArray[0].ToString();

                    intPrevPVTotalAcc = Convert.ToInt32(strPvTotalAcc);
                }
                catch
                {
                    intPrevPVTotalAcc = 0;                
                }

                string strInsertQuery = Manager.MngQuery_GreenHub.GetQuery_PwrDayAcc_RowAdd(dtRegTime);

                string strUpdateQuery = Manager.MngQuery_GreenHub.GetQuery_PwrDayAcc_Update(dtRegTime , intPrevPVTotalAcc);

                string strSelectQuery = Manager.MngQuery_GreenHub.GetQuery_PwrDayAcc_LastData();

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlCommand mySqlCommand = new MySqlCommand(strInsertQuery, msqlConn);
                    mySqlCommand.ExecuteReader();

                    msqlConn.Close();
                }

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlCommand mySqlCommand = new MySqlCommand(strUpdateQuery, msqlConn);
                    mySqlCommand.ExecuteReader();

                    msqlConn.Close();
                }

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();
                    MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(strSelectQuery, msqlConn);
                    mySqlDataAdapter.Fill(dsLastData);
                    msqlConn.Close();
                }

                return dsLastData.Tables[0].Copy();
            }
            catch (Exception)
            {
                if (msqlConn.State != ConnectionState.Closed)
                {
                    msqlConn.Close();
                }
                msqlConn.Dispose();

                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtRegTime"></param>
        /// <param name="dtWaterWheelData"></param>
        /// <returns></returns>
        public bool InsertGreenHubData_WaterWheel(DateTime dtRegTime, DataTable dtWaterWheelData)
        {
            MySqlConnection msqlConn = null;

            try
            {
                string strCreateQuery = Manager.MngQuery_GreenHub.GetCreateQuery_WaterWheel(dtRegTime);

                string strInsertQuery = Manager.MngQuery_GreenHub.GetInsertQuery_GreenHub_WaterWheel(dtRegTime,dtWaterWheelData);

                string strSelectQuery = Manager.MngQuery_GreenHub.GetSelectQuery_CurrentPwrAcc_GreenHub_WWL_CUR();

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlCommand mySqlCommand = new MySqlCommand(strCreateQuery, msqlConn);
                    mySqlCommand.ExecuteReader();

                    msqlConn.Close();
                }

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlCommand mySqlCommand = new MySqlCommand(strInsertQuery, msqlConn);
                    mySqlCommand.ExecuteReader();

                    msqlConn.Close();
                }

                DataSet ds = new DataSet();
                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();
                    MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(strSelectQuery, msqlConn);
                    mySqlDataAdapter.Fill(ds);
                    msqlConn.Close();
                }

                Decimal dcmAccValue = Decimal.Parse(ds.Tables[0].Rows[0].ItemArray[0].ToString());


                string strUpdateQuery = Manager.MngQuery_GreenHub.GetUpdateQuery_GreenHub_WWHL_Cur(dtRegTime, dtWaterWheelData, dcmAccValue);

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlCommand mySqlCommand = new MySqlCommand(strUpdateQuery, msqlConn);
                    mySqlCommand.ExecuteReader();

                    msqlConn.Close();
                }

                return true;
            }
            catch (Exception)
            {
                if (msqlConn.State != ConnectionState.Closed)
                {
                    msqlConn.Close();
                }
                msqlConn.Dispose();
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtRegTime"></param>
        /// <param name="dtMeterData"></param>
        /// <returns></returns>
        public bool InsertGreenHubData_CalorieMeter(DateTime dtRegTime, DataTable dtMeterData)
        {
            MySqlConnection msqlConn = null;

            try
            {
                string strCreateQuery = Manager.MngQuery_GreenHub.GetCreateQuery_CalorieMeter(dtRegTime);

                string strInsertQuery = Manager.MngQuery_GreenHub.GetInsertQuery_GreenHub_CalorieMeter(dtRegTime, dtMeterData);

                string strUpdateQuery = Manager.MngQuery_GreenHub.GetUpdateQuery_GreenHub_METER_Cur(dtRegTime, dtMeterData);

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlCommand mySqlCommand = new MySqlCommand(strCreateQuery, msqlConn);
                    mySqlCommand.ExecuteReader();

                    msqlConn.Close();
                }

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlCommand mySqlCommand = new MySqlCommand(strInsertQuery, msqlConn);
                    mySqlCommand.ExecuteReader();

                    msqlConn.Close();
                }

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlCommand mySqlCommand = new MySqlCommand(strUpdateQuery, msqlConn);
                    mySqlCommand.ExecuteReader();

                    msqlConn.Close();
                }

                return true;
            }
            catch (Exception)
            {
                if (msqlConn.State != ConnectionState.Closed)
                {
                    msqlConn.Close();
                }
                msqlConn.Dispose();
            }

            return false;
        }

        /// <summary>
        /// 변온소 데이터 입력
        /// </summary>
        /// <param name="dtRegTIme">등록일시</param>
        /// <param name="dtSPCData">변온소 데이터</param>
        /// <returns>등록처리 성공여부</returns>
        public bool InsertData_SMART_LAB(DateTime dtRegTIme, DataTable dtSPCData)
        {
            MySqlConnection msqlConn = null;

            try
            {
                string strCreateQuery = Manager.MngQuery_GreenHub.GetCreateQuery_SMART_LAB(dtRegTIme);

                string strInsertQuery = Manager.MngQuery_GreenHub.GetInsertQuery_SMART_LAB(dtRegTIme, dtSPCData);

                string strUpdateQuery = Manager.MngQuery_GreenHub.GetUpdateQuery_SMART_LAB_CUR(dtRegTIme, dtSPCData);

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlCommand mySqlCommand = new MySqlCommand(strCreateQuery, msqlConn);
                    mySqlCommand.ExecuteReader();

                    msqlConn.Close();
                }

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlCommand mySqlCommand = new MySqlCommand(strInsertQuery, msqlConn);
                    mySqlCommand.ExecuteReader();

                    msqlConn.Close();
                }

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlCommand mySqlCommand = new MySqlCommand(strUpdateQuery, msqlConn);
                    mySqlCommand.ExecuteReader();

                    msqlConn.Close();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (msqlConn.State != ConnectionState.Closed)
                {
                    msqlConn.Close();
                }
                msqlConn.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtRegTIme"></param>
        /// <param name="dtSPCData"></param>
        /// <returns></returns>
        public bool InsertData_SMART_WEATHER(DateTime dtRegTIme, DataTable dtSPWData)
        {
            MySqlConnection msqlConn = null;

            try
            {
                string strCreateQuery = Manager.MngQuery_GreenHub.GetCreateQuery_SMART_WEATHER(dtRegTIme);

                string strInsertQuery = Manager.MngQuery_GreenHub.GetInsertQuery_SMART_WEATHER(dtRegTIme, dtSPWData);

                string strUpdateQuery = Manager.MngQuery_GreenHub.GetUpdateQuery_SMART_WEATHER_CUR(dtRegTIme, dtSPWData);

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlCommand mySqlCommand = new MySqlCommand(strCreateQuery, msqlConn);
                    mySqlCommand.ExecuteReader();

                    msqlConn.Close();
                }

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlCommand mySqlCommand = new MySqlCommand(strInsertQuery, msqlConn);
                    mySqlCommand.ExecuteReader();

                    msqlConn.Close();
                }

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlCommand mySqlCommand = new MySqlCommand(strUpdateQuery, msqlConn);
                    mySqlCommand.ExecuteReader();

                    msqlConn.Close();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (msqlConn.State != ConnectionState.Closed)
                {
                    msqlConn.Close();
                }
                msqlConn.Dispose();
            }
        }

        /// <summary>
        /// DCU IP를 바탕으로 연결된 센서번호와 객실정보를 취득
        /// </summary>
        /// <param name="strIpAddr">DCU IP</param>
        /// <returns>객실정보 목록</returns>
        public DataTable GetIoT7SensorDataFromDCU(string strIpAddr)
        {
            MySqlConnection msqlConn = null;
            DataSet ds = new DataSet();

            try
            {
                string strQuery = MngQuery_IoT7.GetSelectRoomListFromAddr_LOOKSERVER_IOT(strIpAddr);

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();
                    MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(strQuery, msqlConn);
                    mySqlDataAdapter.Fill(ds);
                    msqlConn.Close();
                }

                return ds.Tables[0];
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if ((msqlConn != null) && msqlConn.State != ConnectionState.Closed)
                {
                    msqlConn.Close();
                    msqlConn.Dispose();
                }
            }
        }

        /// <summary>
        /// LookServer_IoT에서 DCU IP주소 목록 취득
        /// </summary>
        /// <returns>DCU 주소 목록</returns>
        public DataTable GetDCUData()
        {
            MySqlConnection msqlConn = null;
            DataSet dsReturnData = new DataSet();
            try
            {
                string strQuery = MngQuery_IoT7.GetSelectQuery_IoT7DCUInfo_LOOKSERVER_IOT();

                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();
                    MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(strQuery, msqlConn);
                    mySqlDataAdapter.Fill(dsReturnData);
                    msqlConn.Close();
                }

                return dsReturnData.Tables[0];
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if ((msqlConn != null) && msqlConn.State != ConnectionState.Closed)
                {
                    msqlConn.Close();
                    msqlConn.Dispose();
                }
            }
        }

        /// <summary>
        /// UDP센서 연결상태  갱신 
        /// </summary>
        /// <param name="strIPAddr">센서 연결 주소</param>
        /// <param name="enmConnect">설정할 연결상태</param>
        /// <param name="intRetryCount"> 재시도 횟수</param>
        /// <returns>갱신 처리 성공여부</returns>
        public bool UpdateLookServer_ConnectionStatus(string strIPAddr, enmConnectionStatus enmConnect, int intRetryCount = 0)
        {
            MySqlConnection msqlConn = null;
            if (intRetryCount > INT_DB_QUEARY_RETRY_COUNT)
            {
                return false;
            }
            try
            {
                string strQuery = MngQuery_UDP.GetUpdateQuery_LOOKSERVER(strIPAddr, (enmConnect == enmConnectionStatus.CONNECTED));

                DataSet ds = new DataSet();
                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlCommand mySqlCommand = new MySqlCommand(strQuery, msqlConn);
                    mySqlCommand.ExecuteReader();

                    msqlConn.Close();
                }

                return true;
            }
            catch (Exception)
            {
                return UpdateLookServer_ConnectionStatus(strIPAddr, enmConnect, intRetryCount + 1); ;
            }
            finally
            {
                if ((msqlConn != null) && msqlConn.State != ConnectionState.Closed)
                {
                    msqlConn.Close();
                    msqlConn.Dispose();
                }
            }
        }

        /// <summary>
        /// 각 객실별 IOT센서 연결상태 갱신 
        /// </summary>
        /// <param name="strDong">객실 동 정보</param>
        /// <param name="bolConnetLog">객실 호 정보</param>
        /// <param name="bolConnetLog">연결상태</param>
        /// <returns>갱신 처리 성공여부</returns>
        public bool UpdateLookServerIot_ConnectionStatus(string strDong, int intHo, bool bolConnetLog)
        {
            MySqlConnection msqlConn = null;

            try
            {
                string strQuery = MngQuery_IoT7.GetUpdateQuery_LOOKSERVER_IOT(strDong, intHo, bolConnetLog);

                DataSet ds = new DataSet();
                using (msqlConn = new MySqlConnection(STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlConn.Open();

                    MySqlCommand mySqlCommand = new MySqlCommand(strQuery, msqlConn);
                    mySqlCommand.ExecuteReader();

                    msqlConn.Close();
                }
                return true;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {

                if ((msqlConn != null) && msqlConn.State != ConnectionState.Closed)
                {
                    msqlConn.Close();
                    msqlConn.Dispose();
                }
            }
        }

        /// <summary>
        /// B동 객실 UDP데이터 입력
        /// </summary>
        /// <param name="dtRegTime"></param>
        /// <param name="mdlOutPacket"></param>
        /// <returns></returns>
        public bool InsertRoomUDPData(DateTime dtRegTime, MdlOutPacketData mdlOutPacket)
        {
            MySqlConnection msqlCon = null;
            try
            {
                string strCreateQuery = MngQuery_UDP.GetCreateQuery_ROOMB(dtRegTime);

                string strInsertQuery = MngQuery_UDP.GetInsertQuery_ROOMB(dtRegTime, mdlOutPacket);

                string strUpdateQuery = MngQuery_UDP.GetUpdateQuery_ROOMB_CUR(dtRegTime, mdlOutPacket);

                using (msqlCon = new MySqlConnection(this.STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlCon.Open();

                    //이력테이블 INSERT
                    MySqlCommand msCommand = new MySqlCommand(strCreateQuery, msqlCon);
                    msCommand.ExecuteReader();

                    msqlCon.Close();
                }

                using (msqlCon = new MySqlConnection(this.STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlCon.Open();

                    //이력테이블 INSERT
                    MySqlCommand msCommand = new MySqlCommand(strInsertQuery, msqlCon);
                    msCommand.ExecuteReader();

                    msqlCon.Close();
                }

                using (msqlCon = new MySqlConnection(this.STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlCon.Open();

                    //이력테이블 INSERT
                    MySqlCommand msCommand = new MySqlCommand(strUpdateQuery, msqlCon);
                    msCommand.ExecuteReader();

                    msqlCon.Close();
                }

                return true;
            }
            catch (Exception excep)
            {
                Console.Error.WriteLine(excep);
                return false;
            }
            finally
            {
                if (msqlCon.State != ConnectionState.Closed)
                {
                    msqlCon.Close();
                }
                msqlCon.Dispose();
            }
        }

        /// <summary>
        /// 외기정보 삽입/갱신(외기데이터 클래스)
        /// </summary>
        /// <returns>데이터 삽입 성공여부</returns>
        public bool InsertWeatherData(DateTime dtRegDate, MdlWeatherData WeatherData)
        {
            MySqlConnection msqlCon = null;
            try
            {
                string strCreateQuery = MngQuery_UDP.GetCreateQuery_WEATHER(dtRegDate);

                string strInsertQuery = MngQuery_UDP.GetInserQuery_WEATHER(dtRegDate, WeatherData);

                string strUpdateQuery = MngQuery_UDP.GetUpdateQuery_WEATHER_CUR(dtRegDate, WeatherData);

                using (msqlCon = new MySqlConnection(this.STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlCon.Open();

                    //테이블 확인
                    MySqlCommand mySqlCommand = new MySqlCommand(strCreateQuery, msqlCon);
                    mySqlCommand.ExecuteReader();

                    msqlCon.Close();
                }

                using (msqlCon = new MySqlConnection(this.STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlCon.Open();

                    //데이터 입력
                    MySqlCommand mySqlCommand = new MySqlCommand(strInsertQuery, msqlCon);
                    mySqlCommand.ExecuteReader();

                    msqlCon.Close();
                }

                using (msqlCon = new MySqlConnection(this.STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlCon.Open();

                    //데이터 갱신
                    MySqlCommand mySqlCommand = new MySqlCommand(strUpdateQuery, msqlCon);
                    mySqlCommand.ExecuteReader();

                    msqlCon.Close();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (msqlCon.State != ConnectionState.Closed)
                {
                    msqlCon.Close();
                }
                msqlCon.Dispose();
            }
        }

        /// <summary>
        /// 알람이력 테이블 삽입
        /// </summary>
        /// <param name="dtRegDate">알람발생일</param>
        /// <param name="strAlarmContents">알람내용</param>
        /// <param name="mdlRoomInfo">알람 사항 발생 장소</param>
        /// <returns>삽입처리 성공여부</returns>
        public bool InsertAlarmRecord(DateTime dtRegDate, string strAlarmContents, MdlRoomInfo mdlRoomInfo)
        {
            MySqlConnection msqlCon = null;
            try
            {
                string sqlQueary = MngQuery_UDP.GetInsertQuery_ALARMRECORD(dtRegDate, strAlarmContents, mdlRoomInfo);

                using (msqlCon = new MySqlConnection(this.STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlCon.Open();

                    MySqlCommand msCommand = new MySqlCommand(sqlQueary, msqlCon);
                    MySqlDataReader msDataReader = msCommand.ExecuteReader();
                }
                return true;
            }
            catch (Exception excep)
            {
                Console.Error.WriteLine(excep);
                return false;
            }
            finally
            {
                if (msqlCon.State != ConnectionState.Closed)
                {
                    msqlCon.Close();
                }
                msqlCon.Dispose();
            }
        }



        /// <summary>
        /// 숙소동 기계실 취득데이터 DB입력
        /// </summary>
        /// <param name="dtRegDate">등록일시</param>
        /// <param name="lstSensedData">계측데이터</param>
        public void InsertMachineRoomData_Accommodation(DateTime dtRegDate, DataTable dtAccTmp, DataTable dtAccPrs, DataTable dtAccFlw, DataTable dtAccVlv)
        {
            MySqlConnection msqlCon = null;
            try
            {
                //데이터 삽입날짜의 년월일을 취득하여 로그 데이터 테이블 생성 쿼리 및 데이터 삽입 쿼리 취득
                string strCreateQuery = MngQuery_MR.GetCreateQuery_ACCOMMODATION(dtRegDate);

                string strInsertQuery = MngQuery_MR.GetInsertQuery_ACCOMMODATION(dtRegDate, dtAccTmp, dtAccPrs, dtAccFlw, dtAccVlv);

                string strUpdateQuery = MngQuery_MR.GetUpdateQuery_ACCOMMODATION_CUR(dtRegDate, dtAccTmp, dtAccPrs, dtAccFlw, dtAccVlv);

                //연결정보를 사용하여 DB와 연결
                using (msqlCon = new MySqlConnection(this.STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlCon.Open();

                    //테이블 생성/확인
                    MySqlCommand msCommand = new MySqlCommand(strCreateQuery, msqlCon);
                    MySqlDataReader msDataReader = msCommand.ExecuteReader();

                    msqlCon.Close();
                }

                using (msqlCon = new MySqlConnection(this.STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlCon.Open();

                    //데이터 삽입
                    MySqlCommand msCommand = new MySqlCommand(strInsertQuery, msqlCon);
                    MySqlDataReader msDataReader = msCommand.ExecuteReader();

                    msqlCon.Close();
                }

                using (msqlCon = new MySqlConnection(this.STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlCon.Open();

                    //데이터 갱신
                    MySqlCommand msCommand = new MySqlCommand(strUpdateQuery, msqlCon);
                    MySqlDataReader msDataReader = msCommand.ExecuteReader();

                    msqlCon.Close();
                }
            }
            catch (MySqlException)
            {
                return;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (msqlCon.State != ConnectionState.Closed)
                {
                    msqlCon.Close();
                }
                msqlCon.Dispose();
            }
        }

        /// <summary>
        /// 연구동 기계실 취득데이터 삽입
        /// </summary>
        /// <param name="regDate">데이터 취득일시</param>
        /// <param name="lstSensedData">취득 데이터 리스트</param>
        public void InsertMachineRoomData_Laboratory(DateTime regDate, DataTable dtLabTmp, DataTable dtLabPrsFlw)
        {
            MySqlConnection msqlCon = null;
            try
            {

                //취득데이터를 쿼리문으로 변환
                string strCreateQuery = MngQuery_MR.GetCreateQuery_LABORATORY(regDate);

                string strInsertQuery = MngQuery_MR.GetInsertQuery_LABORATORY(regDate, dtLabTmp, dtLabPrsFlw);

                string strUpdateQuery = MngQuery_MR.GetUpdateQuery_LABORATORY_CUR(regDate, dtLabTmp, dtLabPrsFlw);

                //연결정보를 사용하여 DB에 연결
                using (msqlCon = new MySqlConnection(this.STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlCon.Open();

                    //로그데이터 테이블 생성
                    MySqlCommand msCommandCreate = new MySqlCommand(strCreateQuery, msqlCon);
                    msCommandCreate.ExecuteReader();

                    msqlCon.Close();
                }

                using (msqlCon = new MySqlConnection(this.STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlCon.Open();

                    //로그데이터 추가
                    MySqlCommand msCommandCreate = new MySqlCommand(strInsertQuery, msqlCon);
                    msCommandCreate.ExecuteReader();

                    msqlCon.Close();
                }

                using (msqlCon = new MySqlConnection(this.STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlCon.Open();

                    //상태 데이터 갱신
                    MySqlCommand msCommandCreate = new MySqlCommand(strUpdateQuery, msqlCon);
                    msCommandCreate.ExecuteReader();

                    msqlCon.Close();
                }
            }
            catch (MySqlException mysqlex)
            {
                switch (mysqlex.Number)
                {
                    default: return;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (msqlCon.State != ConnectionState.Closed)
                {
                    msqlCon.Close();
                }
                msqlCon.Dispose();
            }
        }

        /// <summary>
        /// 선택박스 값 취득
        /// </summary>
        /// <param name="enmTypes"></param>
        /// <returns></returns>
        public DataTable SelectListBoxValues(enmListBoxValuesTypes enmTypes)
        {
            MySqlConnection msqlCon = null;
            try
            {
                string strQuery = "";
                int intSelected = (int)enmTypes;
                switch (intSelected)
                {
                    case (int)enmListBoxValuesTypes.UDP_ROOM_B:
                        strQuery = MngQuery_Main.GetListBoxValueQuery_LOOKSERVER();
                        break;
                    case (int)enmListBoxValuesTypes.IOT_ROOM_A:
                        strQuery = MngQuery_Main.GetListBoxValueQuery_LOOKSERVER_IOT("A");
                        break;
                    case (int)enmListBoxValuesTypes.IOT_ROOM_B:
                        strQuery = MngQuery_Main.GetListBoxValueQuery_LOOKSERVER_IOT("B");
                        break;
                    case (int)enmListBoxValuesTypes.IOT_ROOM_C:
                        strQuery = MngQuery_Main.GetListBoxValueQuery_LOOKSERVER_IOT("C");
                        break;
                    default:
                        break;
                }

                //연결정보를 사용하여 DB에 연결
                using (msqlCon = new MySqlConnection(this.STR_DB_CONNECTION_INFO_STRING))
                {
                    msqlCon.Open();
                    DataSet ds = new DataSet();
                    MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(strQuery, msqlCon);

                    mySqlDataAdapter.Fill(ds);

                    msqlCon.Close();

                    return ds.Tables[0];
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

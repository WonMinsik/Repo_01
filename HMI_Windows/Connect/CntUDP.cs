using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Sockets;

namespace HMI_Windows.Connect
{
    class CntUDP
    {
        /// <summary> 취득 처리 동작중 여부 </summary>
        public bool isRunning = false;
        /// <summary> 발생한 알림 </summary>
        public string strMessage = "";
        /// <summary> HMI UDP 통신 포트 </summary>
        private int INT_SERVER_PORT = 9998;
        /// <summary> 외기센서 중계기 연결 IP </summary>
        private string STR_WEATHER_ADDR = "192.168.0.250";
        /// <summary> 외기센서측 HMI 대상 연결 포트 </summary>
        private int INT_WEATHER_CONNECT_POINT_PORT = 9999;
        /// <summary> B동 객실 센서측 HMI 대상 연결 포트 </summary>
        private int INT_ROOMB_CONNECT_POINT_PORT = 9999;
        /// <summary> 연결시간제한 </summary>
        private int INT_TIME_OUT_MILISECOND = 2000;
        /// <summary> 최대 통신 재시도 횟수 </summary>
        private int INT_MAX_RETRY = 2;

        public const string STR_WEATHER_DATA_TABLE_NAME = "WeatherData";

        public const string STR_WEATHER_BYTE_TABLE_NAME = "WeatherBytes";

        public const string STR_DONG_B_DATA_TABLE_NAME = "DongB";

        /// <summary> 루틴 종료 여부 </summary>
        public bool isConnectionFinished { get; set; }

        /// <summary> DB연결 클래스 </summary>
        private Service.SvcDB svcDB;

        /// <summary> UDP통신 바이트 데이터 로그 </summary>
        public DataSet dsUDPLogData { get; }

        /// <summary> 객실B동 난방 통신데이터 테이블 </summary>
        private DataTable dtRoomBConnectionDataLog { get; set; }

        /// <summary>
        /// CntUDP클래스 생성자
        /// </summary>
        public CntUDP() 
        {
            this.isConnectionFinished = false;
            this.svcDB = new Service.SvcDB();
            this.dsUDPLogData = new DataSet();
            this.dtRoomBConnectionDataLog = Manager.Mng_DataTable.GetDataTableTemplate_RoomB_UDP(STR_DONG_B_DATA_TABLE_NAME);
        }

        /// <summary>
        /// UDP 통신데이터 테이블 취득
        /// </summary>
        /// <param name="strTableName">테이블명</param>
        /// <returns>UDP 통신데이터 테이블 템플릿</returns>
        private DataTable GetUDPLogDataTable(string strTableName) 
        {
            DataTable dtTable = new DataTable(strTableName);
            dtTable.Columns.Add("RoomCd", typeof(string));
            dtTable.Columns.Add("ConnectTime", typeof(DateTime));
            dtTable.Columns.Add("SendData", typeof(string));
            dtTable.Columns.Add("RecvData", typeof(string));

            return dtTable;
        }

        /// <summary>
        /// 외기데이터 테이블 취득
        /// </summary>
        /// <returns>외기 데이터 테이블 템플릿</returns>
        private DataTable GetWeatherDataTable()
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

        /// <summary>
        /// 외기 데이터 UDP통신
        /// </summary>
        private void GetWeatherData(DateTime dtRegDate)
        {
            int intRetryCount =0;
            Manager.MngPacket mngPacket = new Manager.MngPacket();
            IPEndPoint ipep = new IPEndPoint(System.Net.IPAddress.Parse(STR_WEATHER_ADDR), INT_WEATHER_CONNECT_POINT_PORT);
            UdpClient udpClient = new UdpClient(INT_SERVER_PORT);
            udpClient.Client.ReceiveTimeout = INT_TIME_OUT_MILISECOND;
            udpClient.Client.SendTimeout = INT_TIME_OUT_MILISECOND;

            byte[] arrSendData = mngPacket.SetWeatherSensorSendData();
            byte[] arrRecvData = new byte[75];


            while (intRetryCount < INT_MAX_RETRY)
            {                
                try
                {
                    int intSendCount = 0;
                    
                    DateTime dtSendStart = DateTime.Now;
                    TimeSpan tsSendTime = DateTime.Now - dtSendStart;
                    do
                    {
                        intSendCount = udpClient.Send(arrSendData, arrSendData.Length, ipep);

                        tsSendTime = DateTime.Now - dtSendStart;

                    } while (intSendCount < arrSendData.Length && tsSendTime.TotalMilliseconds < INT_TIME_OUT_MILISECOND);


                    DateTime dtRecvStart = DateTime.Now;
                    TimeSpan tsRecvTime = DateTime.Now - dtSendStart;

                    System.Collections.ArrayList lstRecv = new System.Collections.ArrayList();

                    do
                    {
                        byte[] arrBuffer = new byte[0];

                        arrBuffer = udpClient.Receive(ref ipep);

                        foreach (byte btdata in arrBuffer)
                        {
                            lstRecv.Add(btdata);
                        }

                        tsRecvTime = DateTime.Now - dtRecvStart;
                    } while (lstRecv.Count < arrRecvData.Length && tsRecvTime.TotalMilliseconds < INT_TIME_OUT_MILISECOND);

                    object[] arrObjBuffer = lstRecv.ToArray();

                    Array.Copy(arrObjBuffer, arrRecvData, arrRecvData.Length);

                    udpClient.Close();
                    break;
                }
                catch (Exception)
                {
                    intRetryCount++;
                }                
            }

            udpClient.Close();
            udpClient.Dispose();

            //통신 데이터 전환 후 DB입력, 데이터 로그작성
            try
            {
                if (intRetryCount > INT_MAX_RETRY)
                {
                    svcDB.UpdateLookServer_ConnectionStatus(STR_WEATHER_ADDR, Service.SvcDB.enmConnectionStatus.DISCONNECTED);
                    DataTable dtWeatherData = GetWeatherDataTable();
                }
                else
                {
                    svcDB.UpdateLookServer_ConnectionStatus(STR_WEATHER_ADDR, Service.SvcDB.enmConnectionStatus.CONNECTED);
                    DataTable dtWeatherData = GetWeatherDataTable();
                    Model.MdlWeatherData mdlWeather = mngPacket.GetWeatherSensorReceiveData(arrRecvData, dtRegDate, ref dtWeatherData);
                    this.dsUDPLogData.Tables.Add(dtWeatherData);
                    svcDB.InsertWeatherData(dtRegDate, mdlWeather);
                }

                DataTable dtWeatherByteLog = GetUDPLogDataTable(STR_WEATHER_BYTE_TABLE_NAME);
                dtWeatherByteLog.Rows.Add("Weather", dtRegDate, BitConverter.ToString(arrSendData), BitConverter.ToString(arrRecvData));
                this.dsUDPLogData.Tables.Add(dtWeatherByteLog);
                
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        /// <summary>
        /// B동 객실 센서 데이터 통신
        /// </summary>
        /// <param name="strTargetAddress">대상IP</param>
        /// <param name="arrSendData">송신데이터</param>
        /// <param name="arrReceiveData">수신데이터</param>
        /// <returns>통신 성공여부</returns>
        private bool SendReceiveUDP_RoomB(string strTargetAddress, ref byte[] arrSendData, ref byte[] arrReceiveData) 
        {
            int intRetryCount = 0;
            bool isConnectOk = false;
            UdpClient udpClient = new UdpClient(INT_SERVER_PORT);
            IPEndPoint ipep = new IPEndPoint(System.Net.IPAddress.Parse(strTargetAddress), INT_ROOMB_CONNECT_POINT_PORT);

            udpClient.Client.ReceiveTimeout = INT_TIME_OUT_MILISECOND;
            udpClient.Client.SendTimeout = INT_TIME_OUT_MILISECOND;

            do
            {
                try
                {
                    udpClient.Send(arrSendData, arrSendData.Length, ipep);

                    System.Threading.Thread.Sleep(500);

                    DateTime dtRecvStartTime = DateTime.Now;

                    TimeSpan tsRecv = new TimeSpan();

                    System.Collections.ArrayList lstRecv = new System.Collections.ArrayList();

                    object[] arrRecvBuffer = new object[22];


                    do
                    {
                        byte[] arrBuffer = new byte[0];

                        arrBuffer = udpClient.Receive(ref ipep);

                        foreach (byte btdata in arrBuffer)
                        {
                            lstRecv.Add(btdata);
                        }

                        tsRecv = DateTime.Now - dtRecvStartTime;
                    } while (lstRecv.Count < arrReceiveData.Length && tsRecv.TotalMilliseconds < INT_TIME_OUT_MILISECOND);

                    arrRecvBuffer = lstRecv.ToArray();

                    if (Convert.ToInt32(arrRecvBuffer[0]) == 0x02 && Convert.ToInt32(arrRecvBuffer[arrRecvBuffer.Length - 1]) == 0x03)
                    {
                        isConnectOk = true;
                        Array.Copy(arrRecvBuffer, arrReceiveData, arrReceiveData.Length);

                        break;
                    }
                    else
                    {
                        intRetryCount++;
                        Array.Copy(arrRecvBuffer, arrReceiveData, arrReceiveData.Length);
                        isConnectOk = false;
                    }

                    System.Threading.Thread.Sleep(200);
                }
                catch (Exception)
                {
                    intRetryCount++;
                    isConnectOk = false;
                }
            } while (!isConnectOk && intRetryCount < INT_MAX_RETRY);

            udpClient.Close();
            udpClient.Dispose();

            try
            {
                if (!isConnectOk)
                {
                    svcDB.UpdateLookServer_ConnectionStatus(strTargetAddress, Service.SvcDB.enmConnectionStatus.DISCONNECTED);
                    return false;
                }
                else
                {
                    svcDB.UpdateLookServer_ConnectionStatus(strTargetAddress, Service.SvcDB.enmConnectionStatus.CONNECTED);
                    return true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// B동 객실 제어
        /// </summary>
        private void SetCommand_RoomB()
        {
            

            try
            {
                //객실제어정보 확인 후 B동 객실에 대한 제어를 실행
                DataTable dtControlRecord = svcDB.GetControlRecord();

                //제어명령이 없는 경우
                if (dtControlRecord.Rows.Count == 0)
                {
                    return;
                }

                //각 객실제어 명령에 대한 실행
                foreach (DataRow drControlRecord in dtControlRecord.Rows)
                {

                    Manager.MngPacket mngPacket = new Manager.MngPacket();
                    byte[] arrControlData = new byte[11];
                    byte[] arrTempData = new byte[22];
                    Model.MdlRoomInfo mdlControlTargetRoomInfo = new Model.MdlRoomInfo();
                    Model.MdlInPacketData mdlControlData = new Model.MdlInPacketData();

                    //제어명령을 위한 방정보
                    mdlControlTargetRoomInfo.intID = Convert.ToInt32(drControlRecord.ItemArray[0]);
                    mdlControlTargetRoomInfo.strDong = Convert.ToString(drControlRecord.ItemArray[1]);
                    mdlControlTargetRoomInfo.intFloor = Convert.ToInt32(drControlRecord.ItemArray[2]);
                    mdlControlTargetRoomInfo.intHo = Convert.ToInt32(drControlRecord.ItemArray[3]);
                    string strTargetIPAddress = Convert.ToString(drControlRecord.ItemArray[4]);

                    //제어 명령 정보
                    mdlControlData.mdlRoomInfo = mdlControlTargetRoomInfo;
                    mdlControlData.isReadOnly = false;
                    mdlControlData.intControlOrderNo = Convert.ToInt32(drControlRecord.ItemArray[5]);
                    mdlControlData.intHeatOnOff = Convert.ToInt32(drControlRecord.ItemArray[6]);
                    mdlControlData.intSetTemp = Convert.ToInt32(drControlRecord.ItemArray[7]);
                    mdlControlData.intDiffTemp = Convert.ToInt32(drControlRecord.ItemArray[8]);

                    Int16 intFlowReset = Convert.ToInt16(dtControlRecord.Rows[0].ItemArray[10]);
                    mdlControlData.isTotalReset = (intFlowReset > 0 ? true : false);

                    //제어정보를 바이트 데이터로 전환하여 제어통신 준비
                    arrControlData = mngPacket.WriteInPacket(mdlControlData);

                    //준비된 데이터로 통신
                    bool isControlSuccess = this.SendReceiveUDP_RoomB(strTargetIPAddress, ref arrControlData, ref arrTempData);

                    //제어가 성공 하면 명령테이블을 읽음상태로 전환
                    if (isControlSuccess && arrTempData.Length > 0)
                    {
                        this.svcDB.UpdateControlRecord_Flag(mdlControlData);
                    }

                    System.Threading.Thread.Sleep(10);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// B동 객실 데이터 취득
        /// </summary>
        /// <param name="drRoomBData"></param>
        /// <param name="dtTime"></param>
        private void GetData_RoomB(DataRow drRoomBData, DateTime dtTime)
        {
            DateTime dtGetDataStart = DateTime.Now;

            //패킷 관리 클래스
            Manager.MngPacket mngPacket = new Manager.MngPacket();

            //객실에대한 데이터 취득 명령어를 위한 데이터 모델
            Model.MdlInPacketData mdlInPacket = new Model.MdlInPacketData();
            Model.MdlRoomInfo mdlRoomInfo = new Model.MdlRoomInfo();

            //
            string strRoomBCd = null;
            string strTargetIPAddress = null;
            try
            {
                //객실 데이터 정리
                strTargetIPAddress = drRoomBData.ItemArray[4].ToString();
                mdlRoomInfo.intID = Convert.ToInt32(drRoomBData.ItemArray[0]);
                mdlRoomInfo.strDong = Convert.ToString(drRoomBData.ItemArray[1]);
                mdlRoomInfo.intFloor = Convert.ToInt32(drRoomBData.ItemArray[2]);
                mdlRoomInfo.intHo = Convert.ToInt32(drRoomBData.ItemArray[3]);
                strRoomBCd = Convert.ToString(drRoomBData.ItemArray[1]) + Convert.ToInt32(drRoomBData.ItemArray[2]) + Convert.ToInt32(drRoomBData.ItemArray[3]);
            }
            catch (Exception)
            {
                //객실 데이터가 무효한 경우 작업 취소
                return;
            }

            try
            {
                //객실에 보낼 데이터 취득용 인패킷 데이터 정의
                mdlInPacket.mdlRoomInfo = mdlRoomInfo;
                mdlInPacket.isReadOnly = true;
                mdlInPacket.intHeatOnOff = 0;
                mdlInPacket.intSetTemp = 0;
                mdlInPacket.intDiffTemp = 0;
                mdlInPacket.isTotalReset = false;

                //패킷 데이터를 바이트 배열로 변환
                byte[] arrSendData = mngPacket.WriteInPacket(mdlInPacket);
                //객실 센서와 통신(읽기)
                byte[] arrRecievData = new byte[22];
                bool isConnectSuccess = SendReceiveUDP_RoomB(strTargetIPAddress, ref arrSendData, ref arrRecievData);
                //통신 정상 종료 후 Control 테이블 갱신

                Model.MdlOutPacketData mdlOutPacket = null;

                if (isConnectSuccess)
                {
                    // 아웃패킷 DB 입력
                    mdlOutPacket = mngPacket.ReadOutPacket(dtTime, arrRecievData, ref mdlRoomInfo);

                    svcDB.InsertRoomUDPData(dtTime, mdlOutPacket);

                    //LookServer테이블 통신상태 갱신
                    svcDB.UpdateLookServer_ConnectionStatus(strTargetIPAddress, Service.SvcDB.enmConnectionStatus.CONNECTED);                    
                }
                else
                {
                    //LookServer테이블 통신상태 갱신
                    svcDB.UpdateLookServer_ConnectionStatus(strTargetIPAddress, Service.SvcDB.enmConnectionStatus.DISCONNECTED);
                }


                TimeSpan tsGetDataTime = DateTime.Now - dtGetDataStart;

                //객실 통신 기록을 데이터 테이블에 저장 [객실코드(동+층+호 ex: B동 1층 108호 -> B1108), 취득시간, 송신데이터, 수신데이터]
                if (mdlOutPacket != null)
                {
                    this.dtRoomBConnectionDataLog.Rows.Add(strRoomBCd, dtTime,
                        mdlOutPacket.fltInsideTemp
                        , mdlOutPacket.fltHeatSetTemp
                        , mdlOutPacket.fltInHeating
                        , mdlOutPacket.fltOutHeating
                        , mdlOutPacket.shtNowControlValue
                        , mdlOutPacket.shtSetControlValue
                        , mdlOutPacket.fltNowFlow
                        , mdlOutPacket.fltTotalFlow
                        , mdlOutPacket.shtHeatOnOff
                        , mdlOutPacket.fltTotalHeat
                        , mdlOutPacket.intDeltaTTemp
                        , mdlOutPacket.intFloorTemp
                        , tsGetDataTime.TotalSeconds
                    );
                }
                else
                {
                    this.dtRoomBConnectionDataLog.Rows.Add(strRoomBCd, dtTime,
                        0
                        , 0
                        , 0
                        , 0
                        , 0
                        , 0
                        , 0
                        , 0
                        , 0
                        , 0
                        , 0
                        , 0
                        , tsGetDataTime.TotalSeconds
                    );
                }




                return;
            }
            catch (Exception)
            {
                this.strMessage += "Error Occuer in Connection.\r\n";
                return;
            }
        }

        /// <summary>
        /// UDP (B동 객실 및 외기센서) 통신 프로세스
        /// </summary>
        /// 통신시간을 약 4000 ~ 4500 msec이 되도록 조정
        public void UDPConnectionProcess()
        {
            this.isRunning = true;
            try
            {
                DateTime dtNowTime = DateTime.Now;
                DateTime dtProcStartTime = new DateTime(dtNowTime.Year, dtNowTime.Month, dtNowTime.Day, dtNowTime.Hour, dtNowTime.Minute, 0);
                this.isConnectionFinished = false;
                this.strMessage = string.Empty;

                this.dsUDPLogData.Reset();
                this.dsUDPLogData.Clear();
                this.dtRoomBConnectionDataLog.Clear();

                DataTable dtRoomBList = svcDB.GetRoomsUDPConnectTarget();

                DateTime dtCtrlProcTime = DateTime.Now;

                //매 호실 취득시퀀스 전에 제어 데이터 반영
                foreach (DataRow drRoomBData in dtRoomBList.Rows)
                {
                    this.GetData_RoomB(drRoomBData, dtProcStartTime);

                    System.Threading.Thread.Sleep(500);

                    TimeSpan tsCtrlSpan = DateTime.Now - dtCtrlProcTime;

                    if (tsCtrlSpan.TotalSeconds > 3)
                    {
                        this.SetCommand_RoomB();

                        dtCtrlProcTime = DateTime.Now;
                    }   
                }

                this.dsUDPLogData.Tables.Add(this.dtRoomBConnectionDataLog);

                //외기센서 데이터 취득
                this.GetWeatherData(dtProcStartTime);

                TimeSpan tsProcTIme = DateTime.Now - dtProcStartTime;

                TimeSpan tsSpan = DateTime.Now - dtNowTime;

                while (tsSpan.TotalSeconds % Frm_HMI_Main.intThreadUDPInterval < (Frm_HMI_Main.intThreadUDPInterval - Frm_HMI_Main.intThreadCheckInterval * 2))
                {
                    SetCommand_RoomB();

                    System.Threading.Thread.Sleep(Frm_HMI_Main.intThreadCheckInterval * 1000);

                    tsSpan = DateTime.Now - dtNowTime;
                }

                this.strMessage += "UDP Thread: Start at " + dtProcStartTime.ToLongTimeString() + " / " + tsProcTIme.TotalSeconds.ToString("F2") + "/" + tsSpan.TotalSeconds.ToString("F2") + " Sec";
                this.isRunning = false;
            }
            catch (Exception e)
            {
                this.strMessage += e.Message + "\r\n";
                this.isRunning = false;
                throw;
            }
        }

    }
}

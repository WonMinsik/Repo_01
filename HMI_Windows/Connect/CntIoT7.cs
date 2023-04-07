using System;
using System.Data;
using System.Collections.Generic;
using EasyModbus;
namespace HMI_Windows.Connect
{
    class CntIoT7
    {
        /// <summary> 처리동작중 플래그 </summary>
        public bool isRunning = false;
        /// <summary> 발생한 알림 </summary>
        public string strMessage = "";

        /// <summary> A동 객실 IOT 정보 테이블 명 </summary>
        public const string STR_ROOM_A_IOT_TABLE_NAME = "rooma_iot";
        /// <summary> B동 객실 IOT 정보 테이블 명 </summary>
        public const string STR_ROOM_B_IOT_TABLE_NAME = "roomb_iot";
        /// <summary> C동 객실 IOT 정보 테이블 명 </summary>
        public const string STR_ROOM_C_IOT_TABLE_NAME = "roomc_iot";

        /// <summary> DCU의 ReadInputRegister에서의 시각정보 시작점 (DCU IoT 시각 정보 길이) </summary>
        private const int INT_DCU_TIME_START_ADDR = 8;

        /// <summary> DCU 시각 데이터 길이 </summary>
        private const int INT_DCU_TIME_DATA_LENGTH = 6;

        /// <summary> 멀티센서당 데이터 길이 </summary>
        private const int INT_SENSOR_DATA_LENGTH = 10;

        /// <summary> 숙소동 Modbus 통신 포트 </summary>
        private const int INT_MODBUS_PORT = 502;

        /// <summary> 연결시간 타임아웃 설정치 </summary>
        private const int INT_MODBUS_CONNECTION_TIMEOUT = 3000;

        /// <summary> 그린허브 데이터 스케일 0.1 </summary>
        private const float FLT_SCALE_0D1 = 0.1f;
        /// <summary> 그린허브 데이터 스케일 0.01 </summary>
        private const float FLT_SCALE_0D01 = 0.01f;


        /// <summary> 객실 A동 IoT7 통신 데이터 </summary>
        private DataTable dtRoomA_IoT7Log;
        /// <summary> 객실 B동 IoT7 통신 데이터 </summary>
        private DataTable dtRoomB_IoT7Log;
        /// <summary> 객실 C동 IoT7 통신 데이터 </summary>
        private DataTable dtRoomC_IoT7Log;

        /// <summary> IoT7 modbus통신 바이트 데이터 로그 </summary>
        public DataSet dsIoT7Log { get; }

        /// <summary> IoT7 클래스 DB 접속 관리자</summary>
        private Service.SvcDB svc;

        /// <summary>
        /// CntIoT7클래스 생성자
        /// </summary>
        public CntIoT7()
        {
            this.isRunning = false;
            this.strMessage = string.Empty;

            this.svc = new Service.SvcDB();

            this.dsIoT7Log = new DataSet();

            this.dtRoomA_IoT7Log = Manager.Mng_DataTable.GetDataTableTemplate_Room_IOT(STR_ROOM_A_IOT_TABLE_NAME);
            this.dtRoomB_IoT7Log = Manager.Mng_DataTable.GetDataTableTemplate_Room_IOT(STR_ROOM_B_IOT_TABLE_NAME);
            this.dtRoomC_IoT7Log = Manager.Mng_DataTable.GetDataTableTemplate_Room_IOT(STR_ROOM_C_IOT_TABLE_NAME);
        }


        /// <summary>
        /// DCU로부터 각 객실의 센서정보를 취득한다.
        /// </summary>
        /// <param name="strTargetAddr"> 대상 DCU IP주소 </param>
        /// <returns>센서 정보 취득 성공여부</returns>
        private bool GetSensorDataFromDCU(string strTargetAddr)
        {
            try
            {
                DataTable dtRoomInfo = svc.GetIoT7SensorDataFromDCU(strTargetAddr);

                ModbusClient modbusClient = new ModbusClient(strTargetAddr, INT_MODBUS_PORT);
                modbusClient.ConnectionTimeout = INT_MODBUS_CONNECTION_TIMEOUT;
                modbusClient.Connect();

                int[] intDCU_TimeValue = modbusClient.ReadHoldingRegisters(INT_DCU_TIME_START_ADDR, INT_DCU_TIME_DATA_LENGTH);
                DateTime dtNowTime = DateTime.Now;
                DateTime dtDCUSystemTime = new DateTime(
                        Convert.ToInt32(intDCU_TimeValue[0])    // DCU 시스템 시각 (년)
                        , Convert.ToInt32(intDCU_TimeValue[1])  // DCU 시스템 시각 (월)
                        , Convert.ToInt32(intDCU_TimeValue[2])  // DCU 시스템 시각 (일)
                        , Convert.ToInt32(intDCU_TimeValue[3])  // DCU 시스템 시각 (시)
                        , Convert.ToInt32(intDCU_TimeValue[4])  // DCU 시스템 시각 (분)
                        , Convert.ToInt32(intDCU_TimeValue[5])  // DCU 시스템 시각 (초)
                    );

                foreach (DataRow drRoomInfo in dtRoomInfo.Rows)
                {

                    Model.MdlRoomInfo mdlRoomInfo = new Model.MdlRoomInfo();
                    mdlRoomInfo.strDong = drRoomInfo.ItemArray[0].ToString(); //drRoomInfo["Dong"].ToString();
                    mdlRoomInfo.intHo = Convert.ToInt32(drRoomInfo.ItemArray[1].ToString()); //Convert.ToInt32(drRoomInfo["Ho"]);
                    mdlRoomInfo.intSensorNo = Convert.ToInt32(drRoomInfo.ItemArray[2].ToString()); //Convert.ToInt32(drRoomInfo["sensor_no"]);
                    int intMultiSensorNo = Convert.ToInt32(drRoomInfo.ItemArray[3].ToString()); //Convert.ToInt32(drRoomInfo["multi_sensor_no"]);
                    
                    int[] arrRIR = modbusClient.ReadInputRegisters((INT_SENSOR_DATA_LENGTH * (intMultiSensorNo - 1 )), INT_SENSOR_DATA_LENGTH); // 펑션코드: 04, RIR의 30001번지부터 10번지씩 읽기 -->멀티센서번호( DCU당 할당번호)

                    if (arrRIR[0] != 0)
                    {
                        Model.MdlIOT7SensorData mdlSensorData = new Model.MdlIOT7SensorData();

                        mdlSensorData.mdlRoomInfo = mdlRoomInfo;
                        mdlSensorData.dtConnectTime = dtNowTime;
                        byte[] arrConnCountbuffer = BitConverter.GetBytes(arrRIR[0]);
                        mdlSensorData.intConnectionCount = BitConverter.ToUInt16(arrConnCountbuffer,0);
                        mdlSensorData.dcmTemp = Convert.ToDecimal((Convert.ToSingle(arrRIR[1].ToString()) * 0.1f).ToString("F1"));
                        mdlSensorData.dcmHumidity = Convert.ToDecimal((Convert.ToSingle(arrRIR[2].ToString()) * 0.1f).ToString("F1"));
                        byte[] arrVOCsbuffer = BitConverter.GetBytes(arrRIR[3]);
                        mdlSensorData.intVOCs = BitConverter.ToUInt16(arrVOCsbuffer,0);
                        byte[] arrCo2buffer = BitConverter.GetBytes(arrRIR[4]);
                        mdlSensorData.intCo2 = BitConverter.ToUInt16(arrCo2buffer, 0);
                        byte[] arrReservebuffer = BitConverter.GetBytes(arrRIR[5]);
                        mdlSensorData.intReserve = BitConverter.ToUInt16(arrReservebuffer, 0);
                        byte[] arrIllumiancebuffer = BitConverter.GetBytes(arrRIR[6]);
                        mdlSensorData.intIllumiance = BitConverter.ToUInt16(arrIllumiancebuffer, 0);
                        byte[] arrMovementbuffer = BitConverter.GetBytes(arrRIR[7]);
                        mdlSensorData.intMovement = BitConverter.ToUInt16(arrMovementbuffer, 0);
                        
                        byte[] arrIRBbuffer = BitConverter.GetBytes(arrRIR[8]);
                        mdlSensorData.intIRBtnCount = BitConverter.ToUInt16(arrIRBbuffer, 0);
                        byte[] arrRSSIbuffer = BitConverter.GetBytes(arrRIR[9]);
                        mdlSensorData.intRSSI = BitConverter.ToUInt16(arrRSSIbuffer, 0);


                        switch (mdlRoomInfo.strDong)
                        {
                            case "A":
                                this.dtRoomA_IoT7Log.Rows.Add(mdlSensorData.dtConnectTime
                            , mdlRoomInfo.strDong
                            , mdlRoomInfo.intHo
                            , mdlRoomInfo.intSensorNo
                            , mdlSensorData.intConnectionCount
                            , mdlSensorData.dcmTemp
                            , mdlSensorData.dcmHumidity
                            , mdlSensorData.intReserve
                            , mdlSensorData.intVOCs
                            , mdlSensorData.intCo2
                            , mdlSensorData.intIllumiance
                            , mdlSensorData.intMovement
                            , mdlSensorData.intIRBtnCount
                            , mdlSensorData.intRSSI
                        ); ;
                                break;
                            case "B":
                                this.dtRoomB_IoT7Log.Rows.Add(mdlSensorData.dtConnectTime
                            , mdlRoomInfo.strDong
                            , mdlRoomInfo.intHo
                            , mdlRoomInfo.intSensorNo
                            , mdlSensorData.intConnectionCount
                            , mdlSensorData.dcmTemp
                            , mdlSensorData.dcmHumidity
                            , mdlSensorData.intReserve
                            , mdlSensorData.intVOCs
                            , mdlSensorData.intCo2
                            , mdlSensorData.intIllumiance
                            , mdlSensorData.intMovement
                            , mdlSensorData.intIRBtnCount
                            , mdlSensorData.intRSSI
                        ); ;
                                break;
                            case "C":
                                this.dtRoomC_IoT7Log.Rows.Add(mdlSensorData.dtConnectTime
                            , mdlRoomInfo.strDong
                            , mdlRoomInfo.intHo
                            , mdlRoomInfo.intSensorNo
                            , mdlSensorData.intConnectionCount
                            , mdlSensorData.dcmTemp
                            , mdlSensorData.dcmHumidity
                            , mdlSensorData.intReserve
                            , mdlSensorData.intVOCs
                            , mdlSensorData.intCo2
                            , mdlSensorData.intIllumiance
                            , mdlSensorData.intMovement
                            , mdlSensorData.intIRBtnCount
                            , mdlSensorData.intRSSI
                        );
                                break;
                            default:
                                break;
                        }

                        svc.UpdateLookServerIot_ConnectionStatus(mdlRoomInfo.strDong, mdlRoomInfo.intHo, true);

                        svc.InsertIoT7Data_ROOM_IOT(dtNowTime, mdlSensorData);
                    }
                    else 
                    {
                        svc.UpdateLookServerIot_ConnectionStatus(mdlRoomInfo.strDong, mdlRoomInfo.intHo, false);
                    }
                }

                modbusClient.Disconnect();
            }
            catch (Exception e)
            {
                this.strMessage += "Error : (" + strTargetAddr + ")" + e.Message + "\r\n";
                return false;
            }

            return true;
        }

        /// <summary>
        /// DCU로부터 객실 IoT센서 정보 취득
        /// </summary>
        /// <returns>객실 데이터 취득 성공여부</returns>
        private bool GetRoomIoTData()
        {
            bool isDataOk = false;
            try
            {
                DataTable dtDCUAddrInfo = svc.GetDCUData();

                foreach (DataRow dr in dtDCUAddrInfo.Rows)
                {
                    string strIPAddr = dr.ItemArray[0].ToString();
                    isDataOk = GetSensorDataFromDCU(strIPAddr);
                }
                

                return isDataOk;
            }
            catch (Exception)
            {
                this.strMessage += "Failed to Connect DCU \n";
                return false;
            }
        }


        /// <summary>
        /// IOT7 프로세스
        /// </summary>
        public void IoT7Data_Process()
        {
            try
            {
                this.isRunning = true;

                DateTime dtNowTime = DateTime.Now;
                DateTime dtStartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);

                this.strMessage = string.Empty;

                this.dsIoT7Log.Reset();
                this.dsIoT7Log.Clear();

                this.dtRoomA_IoT7Log = Manager.Mng_DataTable.GetDataTableTemplate_Room_IOT(STR_ROOM_A_IOT_TABLE_NAME);
                this.dtRoomB_IoT7Log = Manager.Mng_DataTable.GetDataTableTemplate_Room_IOT(STR_ROOM_B_IOT_TABLE_NAME);
                this.dtRoomC_IoT7Log = Manager.Mng_DataTable.GetDataTableTemplate_Room_IOT(STR_ROOM_C_IOT_TABLE_NAME);

                bool isRoomOk = this.GetRoomIoTData();

                if (isRoomOk)
                {
                    this.dsIoT7Log.Tables.Add(this.dtRoomA_IoT7Log);
                    this.dsIoT7Log.Tables.Add(this.dtRoomB_IoT7Log);
                    this.dsIoT7Log.Tables.Add(this.dtRoomC_IoT7Log);
                }

                //다음 실행시간까지 슬립
                TimeSpan tsSpan = DateTime.Now - dtNowTime;

                if (tsSpan.TotalSeconds % 60 < (Frm_HMI_Main.intThreadIoT7Interval - Frm_HMI_Main.intThreadCheckInterval *2))
                {
                    int intRestSec = (Frm_HMI_Main.intThreadIoT7Interval - Frm_HMI_Main.intThreadCheckInterval * 2) - Convert.ToInt32((tsSpan.TotalSeconds % 60));

                    System.Threading.Thread.Sleep(intRestSec * 1000);
                }

                TimeSpan tsProcTIme = DateTime.Now - dtNowTime;
                int intProcMin = tsProcTIme.Minutes;
                int intProcSec = tsProcTIme.Seconds;
                int intProcMil = tsProcTIme.Milliseconds;
                float fltProcTime = intProcMin * 60 + intProcSec + (float)(intProcMil / 1000);
                if (this.strMessage.Length == 0)
                {
                    this.strMessage = "IoT7 Thread: Start at " + dtStartTime.ToLongTimeString() + " / " + fltProcTime.ToString() + " Sec";
                }
                else
                {
                    this.strMessage += "IoT7 Thread: End at " + DateTime.Now.ToLongTimeString();
                }

                this.isRunning = false;

            }
            catch (Exception)
            {
                this.isRunning = false;
                return;
            }
        }
    }
}


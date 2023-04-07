using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Sockets;
using EasyModbus;

namespace HMI_Windows.Connect
{
    /// <summary>
    /// 변온소 및 태양광발전기 연결 관리 클래스
    /// </summary>
    class CntGreenHub
    {
        /// <summary> 처리동작중 플래그 </summary>
        public bool isRunning = false;
        /// <summary> 발생한 알림 </summary>
        public string strMessage = "";

        /// <summary> 그린허브 변온소 데이터 테이블명 </summary>
        public const string STR_GREENHUB_CENTER_DATA_TABLE_NAME = "GreenHub_LabCenter_Data";

        /// <summary> 그린허브 외기센서 데이터 테이블명 </summary>
        public const string STR_GREENHUB_WEATHER_DATA_TABLE_NAME = "GreenHub_Weather_Data";

        /// <summary> 그린허브 일일적산 전력량 데이터 테이블명 </summary>
        public const string STR_GREENHUB_DAYPWR_DATA_TABLE_NAME = "GreenHub_Pwr_Data";

        /// <summary> 그린허브 이기종- ORC 데이터 테이블명 </summary>
        public const string STR_GREENHUB_ORC_DATA_TABLE_NAME = "GreenHub_ORC_Data";

        /// <summary> 그린허브 이기종- PNC 데이터 테이블명 </summary>
        public const string STR_GREENHUB_PNC_DATA_TABLE_NAME = "GreenHub_PNC_Data";

        /// <summary> 그린허브 이기종- PV 데이터 테이블명 </summary>
        public const string STR_GREENHUB_PVGen_DATA_TABLE_NAME = "GreenHub_PVGen_Data";

        /// <summary> 그린허브 이기종- 수차 데이터 테이블명 </summary>
        public const string STR_GREENHUB_WHEEL_DATA_TABLE_NAME = "GreenHub_Wheel_Data";

        /// <summary> 그린허브 이기종- 열량계 데이터 테이블명 </summary>
        public const string STR_GREENHUB_METER_DATA_TABLE_NAME = "GreenHub_Meter_Data";

        /// <summary> 그린허브 변온소 IP주소 </summary>
        private const string STR_GREENHUB_CENTER_IP = "192.168.0.245";

        /// <summary> 변온소 통신 포트 </summary>
        private const int INT_GREENHUB_CENTER_PORT = 502;

        /// <summary> 그린허브 ORC 연결IP주소 </summary>
        private const string STR_GREENHUB_ORC_IP = "192.168.0.246";

        /// <summary> 그린허브 ORC 연결포트번호 </summary>
        private const int INT_GREENHUB_ORC_PORT = 502;

        /// <summary> 그린허브 이기종 연결용 485 to 이더넷 변환/중계기 IP주소 </summary>
        private const string STR_GREENHUB_485TE_IP = "192.168.0.210";

        /// <summary> 그린허브 이기종 연결용 485 to 이더넷 변환/중계기 통신 포트 </summary>
        private const int INT_GREENHUB_485TE_PORT = 9999;

        // /// <summary> 이기종 - ORC 정보 요청 </summary>
        //private string[] ARR_REQUEST_HEX_ORC = { "01", "04", "00", "10", "00", "01", "D0", "3D" };

        // /// <summary> 이기종 - ORC 응답데이터 길이 </summary>
        // private int INT_RESP_DATA_LENGTH_ORC = 7;

        /// <summary> 이기종 - ORC 정보요청 어드레스 </summary>
        private int INT_ORC_REQ_START_ADDR = 470; //22.07.27 16에서 16 -> 470로 변경

        /// <summary> 이기종 - ORC 요청정보 길이 </summary>
        private int INT_ORC_REQ_DATA_LENGTH = 1;

        /// <summary> 이기종 - PNC(송전) 정보 요청 </summary>
        private string[] ARR_REQUEST_HEX_PNC = { "03", "04", "14", "70", "00", "1E", "75", "CB" };

        /// <summary> 이기종 - PNC 응답데이터 길이 </summary>
        private int INT_RESP_DATA_LENGTH_PNC = 65;

        /// <summary> 이기종 - PV(태양광발전기) 정보 요청 </summary>
        private string[] ARR_REQUEST_HEX_PV = { "04", "04", "00", "00", "00", "05", "30", "5C" };

        /// <summary> 이기종 - PV 응답데이터 길이 </summary>
        private int INT_RESP_DATA_LENGTH_PV = 15;

        /// <summary> 이기종 - 수차(서호) 정보 요청 </summary>
        private string[] ARR_REQUEST_HEX_WHEEL = { "05", "04", "00", "08", "00", "01", "B1", "8C" };

        /// <summary> 이기종 - 수차 응답데이터 길이 </summary>
        private int INT_RESP_DATA_LENGTH_WHEEL = 7;

        /// <summary> 이기종 - 신진계전_열량계 검침정보 요청 </summary>
        private string[] ARR_REQUEST_HEX_METER = { "10", "4B", "01", "5C", "16" };

        /// <summary> 이기종 - 열량계 검침요청 응답 길이 </summary>
        private int INT_RESP_DATA_LENGTH_METER = 47; // 수차가 정상동작중인 경우, 열량계 검침정보에 대하여 5byte의 비정상응답을 먼저 보내므로, 열량계 실제 데이터는 42자

        /// <summary> 통신에러 완화를 위한 버퍼 길이 보정 </summary>
        private int INT_BUFFER_LENGTH_CALIB = 4;

        /// <summary> 연결시간 타임아웃 설정치 </summary>
        private const int INT_CONNECTION_TIMEOUT = 3000;

        /// <summary> 그린허브 데이터 스케일 0.00625 </summary>
        private const float FLT_SCALE_D00625 = 0.00625f;

        /// <summary> 그린허브 데이터 스케일 0.00063 </summary>
        private const float FLT_SCALE_D00063 = 0.00063f;

        /// <summary> 그린허브 데이터 스케일 0.00125 </summary>
        private const float FLT_SCALE_D00125 = 0.00125f;

        /// <summary> 그린허브 데이터 스케일 0.1 </summary>
        private const float FLT_SCALE_D1 = 0.1f;
        
        /// <summary> 그린허브 데이터 스케일 0.01 </summary>
        private const float FLT_SCALE_D01 = 0.01f;

        /// <summary> 그린허브 데이터 스케일 0.001 </summary>
        private const float FLT_SCALE_D001 = 0.001f;

        /// <summary> 그린허브 변온소 데이터 </summary>
        private DataTable dtSPCenterDataLog;
        /// <summary> 그린허브 외기 데이터 </summary>
        private DataTable dtSPWeatherDataLog;

        /// <summary> ORC 취득 데이터 </summary>
        private DataTable dtORCDataLog;
        /// <summary> PNC 취득 데이터 </summary>
        private DataTable dtPNCDataLog;
        /// <summary> 태양광 발전기 취득 데이터 </summary>
        private DataTable dtPVDataLog;
        /// <summary> 수차 취득 데이터 </summary>
        private DataTable dtWheelDataLog;
        /// <summary> 열량계 취득 데이터 </summary>
        private DataTable dtMeterDataLog;

        /// <summary> 그린허브 modbus통신 바이트 데이터 로그 </summary>
        public DataSet dsGreenHubLog { get; }

        /// <summary> 그린허브 클래스 DB 접속 관리자</summary>
        private Service.SvcDB svc;


        public CntGreenHub() 
        {
            this.isRunning = false;
            this.strMessage = string.Empty;

            this.dsGreenHubLog = new DataSet();

            this.dtSPCenterDataLog = Manager.Mng_DataTable.GetDataTableTemplate_SPCenterData();
            this.dtSPWeatherDataLog = Manager.Mng_DataTable.GetDataTableTemplate_SPWeatherData();

            this.dtORCDataLog = Manager.Mng_DataTable.GetDataTableTemplate_ORCData();
            this.dtPNCDataLog = Manager.Mng_DataTable.GetDataTableTemplate_PNCData();
            this.dtPVDataLog = Manager.Mng_DataTable.GetDataTableTemplate_PVGenData();
            this.dtWheelDataLog = Manager.Mng_DataTable.GetDataTableTemplate_WheelData();
            this.dtMeterDataLog = Manager.Mng_DataTable.GetDataTableTemplate_MeterData();

            this.svc = new Service.SvcDB();
        }

        /// <summary>
        /// 이기종 ORC 데이터 취득 (easyModbus)
        /// </summary>
        /// <param name="dtStartTime">처리시작 시간</param>
        /// <returns>데이터 취득 성공여부</returns>
        private bool GetORCData_ByEasyModbus(DateTime dtStartTime)
        {
            bool isConnSucc = false;

            Double dblOrcPwrAcc = Double.MinValue;

            bool isPwrAccGetSuc = svc.SelectGreenHubData_ORC_Cur_PwrAcc(out dblOrcPwrAcc);

            Double dblCurTotalPwrAcc = Convert.ToDouble(dblOrcPwrAcc.ToString("F4"));
            try
            {
                EasyModbus.ModbusClient modbusCli = new ModbusClient(STR_GREENHUB_ORC_IP, INT_GREENHUB_ORC_PORT);

                modbusCli.ConnectionTimeout = INT_CONNECTION_TIMEOUT;

                modbusCli.UnitIdentifier = 1;

                modbusCli.Connect();

                //전압값으로 추정 [E2S]
                int[] arrReadRegisters = modbusCli.ReadInputRegisters(INT_ORC_REQ_START_ADDR, INT_ORC_REQ_DATA_LENGTH);

                modbusCli.Disconnect();

                //취득값을 기반으로 계산 - 전력값 (kW)
                float fltCurValue = ((float)arrReadRegisters[0] / 10);

                //현재 순간 전력
                Double dblCurrentPwr = Double.Parse(fltCurValue.ToString("F2"));
                
                //ORC 이전 주기까지의 전체 누적전력량
                Double dblPrvTotalPwrAcc = dblOrcPwrAcc;
                
                //현재 분단위 전력량
                Double dblCurPrwAcc = dblCurrentPwr * Frm_HMI_Main.intThreadGreenHubInterval / 3600;

                //이번주기까지의 전체 누적 전력량
                dblCurTotalPwrAcc = dblPrvTotalPwrAcc + dblCurPrwAcc;

                this.dtORCDataLog.Rows.Add(dtStartTime                      //취득시간
                    , Convert.ToDecimal(dblCurrentPwr.ToString("F2"))       //ORC 전력(kW)
                    , Convert.ToDouble(dblCurPrwAcc.ToString("F4"))        // 분단위 전력량
                );

                isConnSucc = true;
            }
            catch (Exception)
            {
                this.dtORCDataLog.Rows.Add(dtStartTime, 0, 0);
                isConnSucc = false;
            }

            try
            {

                if (isConnSucc)
                {
                    

                    if (isPwrAccGetSuc)
                    {
                        svc.InsertGreenHubData_ORC(dtStartTime, dtORCDataLog, dblCurTotalPwrAcc);
                    }
                    else
                    {
                        svc.InsertGreenHubData_ORC(dtStartTime, dtORCDataLog, null);
                    }
                }
                

                return isConnSucc;
            }
            catch (Exception excep)
            {
                this.strMessage = excep.Message;
            }

            return false;
        }


        /// <summary>
        /// 이기종 PNC(송전)데이터 취득 (485To이더넷)
        /// </summary>
        /// <param name="dtStartTime"></param>
        /// <returns></returns>
        private bool GetPNCData( DateTime dtStartTime)
        {
            byte[] arrRequestBytes = Manager.MngHex.ConvertHexToByte(ARR_REQUEST_HEX_PNC);
            byte[] arrRecieveBytes = new byte[INT_RESP_DATA_LENGTH_PNC];
            bool isPNCOk = false;

            try
            {
                int intTryCount = 0;
                bool isConnectionSuccessed = false;
                bool isDataCollect = false;
                do
                {
                    intTryCount++;
                    isConnectionSuccessed = GetConnectData485ToEthernetConverter(ref arrRequestBytes, ref arrRecieveBytes);

                    //수신데이터가 0304로 시작하는지 확인
                    if (isConnectionSuccessed)
                    {
                        isDataCollect = (arrRecieveBytes[0] == 0x03 && arrRecieveBytes[1] == 0x04);
                    }
                    
                } while (intTryCount < 3 && !isDataCollect);

                if (isConnectionSuccessed && isDataCollect)
                {
                    this.dtPNCDataLog.Rows.Add(dtStartTime//취득시간
                        , Manager.MngHex.Convert4ByteToDecimal_465TE_PNC(arrRecieveBytes, 3)//송전 출력값
                        , Manager.MngHex.Convert4ByteToDecimal_465TE_PNC(arrRecieveBytes, 55)//송전 전력량값
                        , Manager.MngHex.Convert4ByteToDecimal_465TE_PNC(arrRecieveBytes, 59)//수전 전력량값
                    );
                    isPNCOk = true;
                }
                else
                {
                    this.dtPNCDataLog.Rows.Add(dtStartTime//취득시간
                        , 0//송전 출력값
                        , 0//송전 전력량값
                        , 0//수전 전력량값
                    );

                    isPNCOk = false;
                }
            }
            catch (Exception excep)
            {
                this.dtPNCDataLog.Rows.Add(dtStartTime//취득시간
                        , 0//송전 출력값
                        , 0//송전 전력량값
                        , 0//수전 전력량값
                    );
                this.strMessage = excep.Message;

                isPNCOk = false;
            }

            try
            {
                if (isPNCOk)
                {
                    svc.InsertGreenHubData_PNC(dtStartTime, dtPNCDataLog);
                }
                
                return isPNCOk;
            }
            catch (Exception excep)
            {
                this.strMessage = excep.Message;

                return false;
            }       
        }

        /// <summary>
        /// 이기종 태양광발전기 데이터 취득 (485To이더넷)
        /// </summary>
        /// <param name="dtStartTime"></param>
        /// <returns></returns>
        private bool GetSolorPVData(DateTime dtStartTime) 
        {            
            byte[] arrRequestBytes = Manager.MngHex.ConvertHexToByte(ARR_REQUEST_HEX_PV);
            byte[] arrRecieveBytes = new byte[INT_RESP_DATA_LENGTH_PV];
            bool isPVOk = false;
            try
            {
                int intTryCount = 0;
                bool isDataCollect = false;
                bool isConnectionSuccessed = false;

                do
                {
                    intTryCount++;
                    isConnectionSuccessed = GetConnectData485ToEthernetConverter(ref arrRequestBytes, ref arrRecieveBytes);

                    //PV수신데이터의응답이0404로 시작하는지 확인
                    if (isConnectionSuccessed)
                    {
                        isDataCollect = (arrRecieveBytes[0] == 0x04 && arrRecieveBytes[1] == 0x04);
                    }

                } while (intTryCount < 3 && !isDataCollect);

                if (isConnectionSuccessed && isDataCollect)
                {
                    //태양광 발전출력
                    short shtPVGenValue1 = Manager.MngHex.Convert2ByteToInt16_485TE(arrRecieveBytes, 3).Value;

                    //일일발전량
                    short shtPVGenValue2 = Manager.MngHex.Convert2ByteToInt16_485TE(arrRecieveBytes, 5).Value;

                    //누적발전량
                    short shtPVGenValue3 = Manager.MngHex.Convert2ByteToInt16_485TE(arrRecieveBytes, 7).Value;

                    //일사량
                    short shtPVGenValue4 = Manager.MngHex.Convert2ByteToInt16_485TE(arrRecieveBytes, 9).Value;

                    //태양광 패널 온도
                    decimal dcmTempValue = decimal.Parse(((float)(Manager.MngHex.Convert2ByteToInt16_485TE(arrRecieveBytes, 11) / 10)).ToString("F1"));

                    this.dtPVDataLog.Rows.Add(dtStartTime
                        , shtPVGenValue1    //태양광 발전출력
                        , shtPVGenValue2    //일일발전량
                        , shtPVGenValue3    //누적발전량
                        , shtPVGenValue4    //일사량
                        , dcmTempValue      //태양광 패널 온도
                    );
                    isPVOk = true;
                }
                else
                {
                    this.dtPVDataLog.Rows.Add(dtStartTime
                        , 12    //태양광 발전출력
                        , 34    //일일발전량
                        , 56    //누적발전량
                        , 78    //일사량
                        , 32.5    //태양광 패널 온도
                    );

                    isPVOk = false;
                }
            }
            catch (Exception excep)
            {
                this.dtPVDataLog.Rows.Add(dtStartTime
                        , 0    //태양광 발전출력
                        , 0    //일일발전량
                        , 0    //누적발전량
                        , 0    //일사량
                        , 0    //태양광 패널 온도
                    );
                this.strMessage = excep.Message;

                isPVOk = false;
            }

            try
            {
                if (isPVOk)
                {
                    svc.InsertGreenHubData_PV(dtStartTime, dtPVDataLog);
                }

                return isPVOk;
            }
            catch ( Exception excep)
            {
                this.strMessage = excep.Message;

                return false;
            }
        }


        /// <summary>
        /// 이기종 수차 (서호) 데이터 취득 (485To이더넷) 
        /// </summary>
        /// <param name="dtStartTime"></param>
        /// <returns></returns>
        private bool GetWheelData(DateTime dtStartTime)
        {
            byte[] arrRequestBytes = Manager.MngHex.ConvertHexToByte(ARR_REQUEST_HEX_WHEEL);
            byte[] arrRecieveBytes = new byte[INT_RESP_DATA_LENGTH_WHEEL];
            bool isWhlOk = false;

            try
            {
                int intRetryCount = 0;
                bool isDataCollect = false;
                bool isConnectionSuccessed = false;
                do
                {
                    intRetryCount++;
                    isConnectionSuccessed = GetConnectData485ToEthernetConverter(ref arrRequestBytes, ref arrRecieveBytes);

                    if (isConnectionSuccessed)
                    {
                        isDataCollect = (arrRecieveBytes[0] == 0x05 && arrRecieveBytes[1] == 0x04);
                    }
                    
                } while (intRetryCount < 3 && !isDataCollect);

                if (isConnectionSuccessed && isDataCollect)
                {
                    //전력
                    decimal dcmWWlValue = Manager.MngHex.Convert2ByteToDecimal_465TE_Wheel(arrRecieveBytes, 3, 1).Value / 10;
                    //분단위 전력량
                    decimal dcmWWlAcc = dcmWWlValue * Frm_HMI_Main.intThreadGreenHubInterval / 3600;

                    this.dtWheelDataLog.Rows.Add(dtStartTime    //날짜
                        , dcmWWlValue   // 전력
                        , dcmWWlAcc     // 분단위 전력량
                    );

                    isWhlOk = true;
                }
                else
                {
                    this.dtWheelDataLog.Rows.Add(dtStartTime    //날짜
                        , 0   //전력
                        , 0     // 분단위 전력량
                    );

                    isWhlOk = false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            try {
                if (isWhlOk)
                {
                    svc.InsertGreenHubData_WaterWheel(dtStartTime, dtWheelDataLog);
                }
                return isWhlOk;
            }
            catch (Exception e)
            {
                this.strMessage += e.Message;
                return false;
            }
        }

        /// <summary>
        /// 이기종 열량계 데이터 취득 (485To이더넷)
        /// </summary>
        /// <param name="dtStartTime">처리시작시간</param>
        /// <param name="isWheelOk">수차 통신 성공여부</param>
        /// <returns></returns>
        private bool GetCalorieMeterData(DateTime dtStartTime, bool isWheelOk)
        {            
            byte[] arrRequestBytes = Manager.MngHex.ConvertHexToByte(ARR_REQUEST_HEX_METER);
            byte[] arrRecieveBytes;
            bool isCMOk = false;

            if (isWheelOk)
            {
                arrRecieveBytes = new byte[INT_RESP_DATA_LENGTH_METER];
            }
            else
            {
                arrRecieveBytes = new byte[INT_RESP_DATA_LENGTH_METER - 5];
            }

            try
            {
                int intTryCount = 0;
                bool isRespChkOk = false;
                bool isConnectionSuccessed = false;

                do
                {
                    intTryCount++;
                    isConnectionSuccessed = GetConnectData485ToEthernetConverter(ref arrRequestBytes, ref arrRecieveBytes);

                    if (isWheelOk && isConnectionSuccessed)
                    {
                        isRespChkOk = (arrRecieveBytes[5] == 0x68 && arrRecieveBytes[8] == 0x68);
                    }
                    else
                    {
                        isRespChkOk = (arrRecieveBytes[0] == 0x68 && arrRecieveBytes[3] == 0x68);
                    }

                } while (intTryCount < 3 && !isRespChkOk);

                if (isConnectionSuccessed && isRespChkOk)
                {

                    if (isWheelOk)
                    {

                        //수차가 정상동작하여 현재검침 요구에 대하여 비정상응답을 할 경우
                        //누적열량
                        decimal dcmBuffer1 = Manager.MngHex.Convert2ByteToDecimal_485TE_Meter(arrRecieveBytes, 20).Value;

                        //순간열량
                        decimal dcmBuffer2 = Manager.MngHex.Convert2ByteToDecimal_485TE_Meter(arrRecieveBytes, 24).Value;

                        //누적유량
                        decimal dcmBuffer3 = Decimal.Parse(arrRecieveBytes[28].ToString("X2"));

                        //순간유량
                        decimal dcmBuffer4 = Manager.MngHex.Convert2ByteToDecimal_485TE_Meter(arrRecieveBytes, 31).Value;

                        //순간공급온도
                        decimal dcmBuffer5 = Manager.MngHex.Convert2ByteToDecimal_485TE_Meter(arrRecieveBytes, 35).Value;

                        //순간회수온도
                        decimal dcmBuffer6 = Manager.MngHex.Convert2ByteToDecimal_485TE_Meter(arrRecieveBytes, 39).Value;

                        //순간압력
                        decimal dcmBuffer7 = Manager.MngHex.Convert2ByteToDecimal_485TE_Meter(arrRecieveBytes, 43).Value;

                        this.dtMeterDataLog.Rows.Add(dtStartTime
                            , dcmBuffer1    //누적열량
                            , dcmBuffer2    //순간열량
                            , dcmBuffer3    //누적유량
                            , dcmBuffer4    //순간유량
                            , dcmBuffer5    //순간공급온도
                            , dcmBuffer6    //순간회수온도
                            , dcmBuffer7    //순간압력
                        );
                    }
                    else
                    {
                        //수차가 동작하지 않아 비정상응답이 포함되지 않을 경우
                        //누적열량
                        decimal dcmBuffer1 = Manager.MngHex.Convert2ByteToDecimal_485TE_Meter(arrRecieveBytes, 15).Value;

                        //순간열량
                        decimal dcmBuffer2 = Manager.MngHex.Convert2ByteToDecimal_485TE_Meter(arrRecieveBytes, 19).Value;

                        //누적유량
                        decimal dcmBuffer3 = Decimal.Parse(arrRecieveBytes[23].ToString("X2"));

                        //순간유량
                        decimal dcmBuffer4 = Manager.MngHex.Convert2ByteToDecimal_485TE_Meter(arrRecieveBytes, 26).Value;

                        //순간공급온도
                        decimal dcmBuffer5 = Manager.MngHex.Convert2ByteToDecimal_485TE_Meter(arrRecieveBytes, 30).Value;

                        //순간회수온도
                        decimal dcmBuffer6 = Manager.MngHex.Convert2ByteToDecimal_485TE_Meter(arrRecieveBytes, 34).Value;

                        //순간압력
                        decimal dcmBuffer7 = Manager.MngHex.Convert2ByteToDecimal_485TE_Meter(arrRecieveBytes, 38).Value;

                        this.dtMeterDataLog.Rows.Add(dtStartTime
                            , dcmBuffer1    //누적열량
                            , dcmBuffer2    //순간열량
                            , dcmBuffer3    //누적유량
                            , dcmBuffer4    //순간유량
                            , dcmBuffer5    //순간공급온도
                            , dcmBuffer6    //순간회수온도
                            , dcmBuffer7    //순간압력
                        );
                    }

                    isCMOk = true;
                }
                else
                {
                    this.dtMeterDataLog.Rows.Add(dtStartTime
                            , 0    //누적열량
                            , 0    //순간열량
                            , 0    //누적유량
                            , 0    //순간유량
                            , 0    //순간공급온도
                            , 0    //순간회수온도
                            , 0    //순간압력
                        );

                    isCMOk = false;
                }
            }
            catch (Exception excep)
            {
                this.strMessage = excep.Message;
                return false;
            }

            try {

                if (isCMOk)
                {
                    svc.InsertGreenHubData_CalorieMeter(dtStartTime, dtMeterDataLog);
                }

                return isCMOk;
            }
            catch (Exception excep)
            {
                this.strMessage = excep.Message;
                return false;
            }
        }



        /// <summary>
        /// 그린허브 이기종 연결용 485To이더넷 컨버터와 통신
        /// </summary>
        /// <param name="arrSendData">송신 데이터</param>
        /// <param name="arrRecvData">수신 데이터</param>
        /// <returns>통신 성공/실패 여부</returns>
        private bool GetConnectData485ToEthernetConverter(ref byte[] arrSendData, ref byte[] arrRecvData)
        {
            TcpClient tcpCli = null;
            NetworkStream nsStream = null;
            byte[] arrRecvBuffer = new byte[arrRecvData.Length + INT_BUFFER_LENGTH_CALIB];
            int intTryCount = 0;
            try
            {
                tcpCli = new TcpClient(STR_GREENHUB_485TE_IP, INT_GREENHUB_485TE_PORT);

                tcpCli.SendTimeout = INT_CONNECTION_TIMEOUT;
                tcpCli.ReceiveTimeout = INT_CONNECTION_TIMEOUT;

                nsStream = tcpCli.GetStream();

                int intSendOffSet = 0;
                int intRecvOffSet = 0;

                intTryCount++;

                //Send Data
                nsStream.Write(arrSendData, intSendOffSet, arrSendData.Length);
                System.Threading.Thread.Sleep(50);

                if (nsStream.CanRead)
                {
                    intRecvOffSet = 0;

                    DateTime dtRecvStartTime = DateTime.Now;

                    TimeSpan tsRecv = new TimeSpan();

                    do
                    {
                        intRecvOffSet += nsStream.Read(arrRecvBuffer, intRecvOffSet, 8); //한번에 10 byte 이상 읽을경우 통신실패 혹은 잘못된 데이터로 받을 확률 급격히 증가

                        tsRecv = DateTime.Now - dtRecvStartTime;
                    
                    } while (intRecvOffSet < arrRecvData.Length && tsRecv.TotalMilliseconds < INT_CONNECTION_TIMEOUT);
                }

                System.Threading.Thread.Sleep(50);

                Array.Copy(arrRecvBuffer,arrRecvData, arrRecvData.Length);
                
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                try
                {
                    if (tcpCli != null)
                    {
                        if (nsStream != null)
                        {
                            nsStream.Close();
                            nsStream.Dispose();
                        }

                        tcpCli.Close();
                        tcpCli.Dispose();
                    }
                }
                catch (Exception)
                {
                    tcpCli = null;
                    nsStream = null;
                }
            }

            return true;
        }

        /// <summary>
        /// 변온소 IoT7정보 취득
        /// </summary>
        /// <param name="dtRegTime">등록처리 시작 일시</param>
        /// <returns>처리 성공여부</returns>
        private bool GetGreenHubCenterData(DateTime dtRegTime)
        {
            ModbusClient modbusClient = null;

            //각 데이터 초기값 정의
            float fltTemp_1 = 0.0f;
            float fltTemp_2 = 0.0f;
            float fltTemp_3 = 0.0f;
            float fltTemp_4 = 0.0f;
            float fltTemp_5 = 0.0f;
            float fltTemp_6 = 0.0f;

            float fltPress_1 = 0.0f;
            float fltPress_2 = 0.0f;
            float fltPress_3 = 0.0f;
            float fltPress_4 = 0.0f;
            float fltPress_5 = 0.0f;
            float fltPress_6 = 0.0f;

            float fltFlow_1 = 0.0f;
            float fltFlow_2 = 0.0f;
            float fltFlow_3 = 0.0f;

            float fltNowFCV = 0;
            float fltCurDayFCV = 0;
            float fltPrvDayFCV = 0;

            float fltNowFCH = -100;
            float fltCurDayFCH = -1000;
            float fltPrvDayFCH = -1000;

            float fltNowORC = 0;
            float fltCurDayORC = 0;
            float fltPrvDayORC = 0;

            float fltNowHC = 0;
            float fltCurDayHC = 0;
            float fltPrvDayHC = 0;

            float fltNowPV = 0;
            float fltCurDayPV = 0;
            float fltPrvDayPV = 0;

            float fltAirTemp = -20.0f;
            float fltAirHumi = 0.0f;

            try
            {
                modbusClient = new ModbusClient(STR_GREENHUB_CENTER_IP, INT_GREENHUB_CENTER_PORT);

                modbusClient.ConnectionTimeout = INT_CONNECTION_TIMEOUT;
                modbusClient.Connect();

                int[] arrRecvBuffer1 = modbusClient.ReadInputRegisters(0, 50); // 펑션코드: 03, RHR의 30000~30049 읽기
                //온도1 30004
                ConvertGreenHubDataScale(arrRecvBuffer1[4], FLT_SCALE_D1, ref fltTemp_1);
                //온도2 30005
                ConvertGreenHubDataScale(arrRecvBuffer1[5], FLT_SCALE_D1, ref fltTemp_2);
                //온도3 30007
                ConvertGreenHubDataScale(arrRecvBuffer1[7], FLT_SCALE_D1, ref fltTemp_3);
                //온도4 30006
                ConvertGreenHubDataScale(arrRecvBuffer1[6], FLT_SCALE_D1, ref fltTemp_4);

                //유량1 (미확인) 30008
                ConvertGreenHubDataScale(arrRecvBuffer1[8], FLT_SCALE_D1, ref fltFlow_1);

                // PAFC 현재 발전량 30012
                ConvertGreenHubDataScale(arrRecvBuffer1[12], FLT_SCALE_D1, ref fltNowFCV);

                //외기 온도 30016
                ConvertGreenHubDataScale(arrRecvBuffer1[16], FLT_SCALE_D1, ref fltAirTemp);
                //외기 습도 30017
                ConvertGreenHubDataScale(arrRecvBuffer1[17], FLT_SCALE_D1, ref fltAirHumi);

                //압력1 30021
                ConvertGreenHubDataScale(arrRecvBuffer1[21], FLT_SCALE_D1, ref fltPress_1);
                //압력2 30020
                ConvertGreenHubDataScale(arrRecvBuffer1[20], FLT_SCALE_D1, ref fltPress_2);
                //압력3 30024
                ConvertGreenHubDataScale(arrRecvBuffer1[24], FLT_SCALE_D1, ref fltPress_3);
                //압력4 30025
                ConvertGreenHubDataScale(arrRecvBuffer1[25], FLT_SCALE_D1, ref fltPress_4);

                //유량2 지역난방_환수유량계 30030~30031
                int[] arrFlow2Value = new int[2];
                Array.Copy(arrRecvBuffer1, 30, arrFlow2Value, 0, 2);
                ConvertGreenHubDataScale(ModbusClient.ConvertRegistersToInt(arrFlow2Value), FLT_SCALE_D1, ref fltFlow_2);
                // PAFC 현재 열생산량 30034
                ConvertGreenHubDataScale(arrRecvBuffer1[34], FLT_SCALE_D01, ref fltNowFCH);
                // ORC 현재발전량 30035
                ConvertGreenHubDataScale(arrRecvBuffer1[35], FLT_SCALE_D1, ref fltNowORC);
                // HC(냉난방) 현재 공급량 30038
                ConvertGreenHubDataScale(arrRecvBuffer1[38], FLT_SCALE_D1, ref fltNowHC);
                // 태양광 현재 발전량 30039
                ConvertGreenHubDataScale(arrRecvBuffer1[39], FLT_SCALE_D1, ref fltNowPV);


                int[] arrRecvBuffer2 = modbusClient.ReadInputRegisters(50, 50); // 펑션코드: 03, RHR의 30050~30099 읽기
                //금일 전일값
                int[] bufferCurFCGReg = new int[2];
                int[] bufferPrvFCGReg = new int[2];
                int[] bufferCurFCHReg = new int[2];
                int[] bufferPrvFCHReg = new int[2];
                Array.Copy(arrRecvBuffer2, 0, bufferCurFCGReg, 0, 2);
                Array.Copy(arrRecvBuffer2, 2, bufferPrvFCGReg, 0, 2);
                Array.Copy(arrRecvBuffer2, 6, bufferCurFCHReg, 0, 2);
                Array.Copy(arrRecvBuffer2, 8, bufferPrvFCHReg, 0, 2);
                //PAFC 금일 발전량값 30050 ~ 30051
                ConvertGreenHubDataScale(ModbusClient.ConvertRegistersToInt(bufferCurFCGReg), FLT_SCALE_D001, ref fltCurDayFCV);
                //PAFC 전일 발전량값 30052 ~ 30053
                ConvertGreenHubDataScale(ModbusClient.ConvertRegistersToInt(bufferPrvFCGReg), FLT_SCALE_D001, ref fltPrvDayFCV);
                //PAFC 금일 열생산량값 30056 ~ 30057
                ConvertGreenHubDataScale(ModbusClient.ConvertRegistersToInt(bufferCurFCHReg), FLT_SCALE_D1, ref fltCurDayFCH);
                //PAFC 전일 열생산량값 30058 ~ 30059
                ConvertGreenHubDataScale(ModbusClient.ConvertRegistersToInt(bufferPrvFCHReg), FLT_SCALE_D1, ref fltPrvDayFCH);

                int[] bufferCurORCReg = new int[2];
                int[] bufferPrvORCReg = new int[2];
                Array.Copy(arrRecvBuffer2, 18, bufferCurORCReg, 0, 2);
                Array.Copy(arrRecvBuffer2, 20, bufferPrvORCReg, 0, 2);
                // ORC 금일 발전량값 30068 ~ 30069
                ConvertGreenHubDataScale(ModbusClient.ConvertRegistersToInt(bufferCurORCReg), FLT_SCALE_D1, ref fltCurDayORC);
                // ORC 전일 발전량값 30070 ~ 30071
                ConvertGreenHubDataScale(ModbusClient.ConvertRegistersToInt(bufferPrvORCReg), FLT_SCALE_D1, ref fltPrvDayORC);

                int[] bufferCurHCReg = new int[2];
                int[] bufferPrvHCReg = new int[2];
                int[] bufferCurPVReg = new int[2];
                int[] bufferPrvPVReg = new int[2];
                Array.Copy(arrRecvBuffer2, 30, bufferCurHCReg, 0, 2);
                Array.Copy(arrRecvBuffer2, 32, bufferPrvHCReg, 0, 2);
                Array.Copy(arrRecvBuffer2, 36, bufferCurPVReg, 0, 2);
                Array.Copy(arrRecvBuffer2, 38, bufferPrvPVReg, 0, 2);
                // HC 금일 열공급량값 30080 ~ 30081
                ConvertGreenHubDataScale(ModbusClient.ConvertRegistersToInt(bufferCurHCReg), FLT_SCALE_D1, ref fltCurDayHC);
                // HC 전일 열공급량값 30082 ~ 30083
                ConvertGreenHubDataScale(ModbusClient.ConvertRegistersToInt(bufferPrvHCReg), FLT_SCALE_D1, ref fltPrvDayHC);
                // 태양광 금일 발전량 값 30086 ~ 30087
                ConvertGreenHubDataScale(ModbusClient.ConvertRegistersToInt(bufferCurPVReg), FLT_SCALE_D1, ref fltCurDayPV);
                // 태양광 전일 발전량 값 30088 ~ 30089
                ConvertGreenHubDataScale(ModbusClient.ConvertRegistersToInt(bufferPrvPVReg), FLT_SCALE_D1, ref fltPrvDayPV);

                int[] arrRecvBuffer3 = modbusClient.ReadInputRegisters(301, 11); // 펑션코드: 03, RHR의 30301~30311 읽기
                //온도5 30301
                ConvertGreenHubDataScale(arrRecvBuffer3[0], FLT_SCALE_D00625, ref fltTemp_5);
                //온도6 30302
                ConvertGreenHubDataScale(arrRecvBuffer3[1], FLT_SCALE_D00625, ref fltTemp_6);
                //압력5 30303
                ConvertGreenHubDataScale(arrRecvBuffer3[2], FLT_SCALE_D00063, ref fltPress_5);
                //압력6 30304
                ConvertGreenHubDataScale(arrRecvBuffer3[3], FLT_SCALE_D00125, ref fltPress_6);
                //유량3 M201 급탕순환유량 30310 ~ 30311
                int[] arrFlow3Value = new int[2];
                Array.Copy(arrRecvBuffer3, 9, arrFlow3Value, 0, 2);
                ConvertGreenHubDataScale(ModbusClient.ConvertRegistersToInt(arrFlow3Value), FLT_SCALE_D1, ref fltFlow_3);

                modbusClient.Disconnect();
            }
            catch (Exception)
            {
                modbusClient.Disconnect();
                //통신에 실패할 경우 DB미입력
                return false;
            }


            //DB에 데이터 입력
            try
            {
                //변온소 데이터 입력
                this.dtSPCenterDataLog.Rows.Add(dtRegTime
                    , fltTemp_1, fltTemp_2, fltTemp_3, fltTemp_4, fltTemp_5, fltTemp_6          // Temp 6종
                    , fltPress_1, fltPress_2, fltPress_3, fltPress_4, fltPress_5, fltPress_6    // Press 6종
                    , fltFlow_1, fltFlow_2, fltFlow_3                                           // Flow 3종
                    , fltNowFCV, fltCurDayFCV, fltPrvDayFCV                                     // FC 발전량 3종
                    , fltNowFCH, fltCurDayFCH, fltPrvDayFCH                                     // FC 열생산량 3종
                    , fltNowORC, fltCurDayORC, fltPrvDayORC                                     // ORC 발전량 3종
                    , fltNowHC, fltCurDayHC, fltPrvDayHC                                        // HC 공급량 3종
                    , fltNowPV, fltCurDayPV, fltPrvDayPV                                        // PV 발전량 3종
                );

                //외기 데이터 입력
                this.dtSPWeatherDataLog.Rows.Add(dtRegTime, fltAirTemp, fltAirHumi);  //외기온도, 외기 습도

                svc.InsertData_SMART_LAB(dtRegTime, this.dtSPCenterDataLog);
                svc.InsertData_SMART_WEATHER(dtRegTime, this.dtSPWeatherDataLog);
            }
            catch (Exception)
            {
                //DB에는 미입력
                return false;
            }

            return true;
        }

        /// <summary>
        /// 그린허브 데이터 스케일 적용 (float형)
        /// </summary>
        /// <param name="intValue">데이터 값</param>
        /// <param name="fltScale">스케일</param>
        /// <param name="fltData">스케일 적용 데이터 값</param>
        private void ConvertGreenHubDataScale(int intValue, float fltScale, ref float fltData)
        {
            fltData = intValue * fltScale;
        }

        /// <summary>
        /// 그린허브 데이터 스케일 적용 (Int형)
        /// </summary>
        /// <param name="intValue">데이터 값</param>
        /// <param name="fltScale">스케일</param>
        /// <param name="intData">스케일 적용 데이터 값</param>
        private void ConvertGreenHubDataScale(int intValue, float fltScale, ref Int32 intData)
        {
            intData = Convert.ToInt32(intValue * fltScale);
        }

        /// <summary>
        /// 그린허브 데이터 취득 프로세스
        /// </summary>
        public void GreenHub_Process()
        {
            try
            {
                this.isRunning = true;

                DateTime dtNowTime = DateTime.Now;
                DateTime dtStartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);

                this.strMessage = string.Empty;

                this.dsGreenHubLog.Reset();
                this.dsGreenHubLog.Clear();

                this.dtORCDataLog = Manager.Mng_DataTable.GetDataTableTemplate_ORCData();
                this.dtPNCDataLog = Manager.Mng_DataTable.GetDataTableTemplate_PNCData();
                this.dtPVDataLog = Manager.Mng_DataTable.GetDataTableTemplate_PVGenData();
                this.dtWheelDataLog = Manager.Mng_DataTable.GetDataTableTemplate_WheelData();
                this.dtMeterDataLog = Manager.Mng_DataTable.GetDataTableTemplate_MeterData();
                //[e2s] mswon - 2022.07.20 동작 제외
                this.dtSPCenterDataLog = Manager.Mng_DataTable.GetDataTableTemplate_SPCenterData();
                this.dtSPWeatherDataLog = Manager.Mng_DataTable.GetDataTableTemplate_SPWeatherData();

                //변온소 데이터 취득
                bool isCenterDataOk = this.GetGreenHubCenterData(dtStartTime);

                System.Threading.Thread.Sleep(1000);

                //이기종 5종 데이터 취득
                bool isORCDataOk = this.GetORCData_ByEasyModbus(dtStartTime);

                System.Threading.Thread.Sleep(1000);

                bool isPNCDataOk = this.GetPNCData(dtStartTime);

                System.Threading.Thread.Sleep(1000);

                bool isSolDataOk = this.GetSolorPVData(dtStartTime);

                System.Threading.Thread.Sleep(1000);

                bool isWhlDataOk = this.GetWheelData(dtStartTime);

                System.Threading.Thread.Sleep(1000);

                bool isCMTDataOk = this.GetCalorieMeterData(dtStartTime, isWhlDataOk);


                //변온소 데이터를 데이터셋에 추가
                if (isCenterDataOk)
                {
                    this.dsGreenHubLog.Tables.Add(this.dtSPCenterDataLog);
                    this.dsGreenHubLog.Tables.Add(this.dtSPWeatherDataLog);
                }
                else this.strMessage += "변온소 ";

                //이기종 - ORC 데이터를 데이터셋에 추가
                if (isORCDataOk) this.dsGreenHubLog.Tables.Add(this.dtORCDataLog);
                else this.strMessage += "ORC ";
                
                //이기종 - PNC 데이터를 데이터셋에 추가
                if (isPNCDataOk) this.dsGreenHubLog.Tables.Add(this.dtPNCDataLog);
                else this.strMessage += "PNC ";

                //이기종 - PV 데이터를 데이터셋에 추가
                if (isSolDataOk) this.dsGreenHubLog.Tables.Add(this.dtPVDataLog);
                else this.strMessage += "PV ";

                //이기종 - 수차 데이터를 데이터셋에 추가
                if (isWhlDataOk) this.dsGreenHubLog.Tables.Add(this.dtWheelDataLog);
                else this.strMessage += "수차 ";

                //이기종 - 열량계 데이터를 데이터셋에 추가
                if (isCMTDataOk) this.dsGreenHubLog.Tables.Add(this.dtMeterDataLog);
                else this.strMessage += "열량계 ";

                if (!isORCDataOk || !isPNCDataOk || !isSolDataOk || !isWhlDataOk)
                {
                    this.strMessage += " 통신 오류. \r\n";
                }


                DateTime dtPrevProcTime = dtStartTime.AddSeconds(-Frm_HMI_Main.intThreadGreenHubInterval);
                //날짜가 바뀐 경우 [E2S_TEST_0722_BK] 적산값 계산간격 1일
                if (dtPrevProcTime.Day != dtStartTime.Day)
                //시간이 바뀐 경우 [E2S_TEST_0722]  적산값 계산간격 1시간
                // if (dtPrevProcTime.Hour != dtStartTime.Hour)
                {
                    //전일 날짜의 시간보정( 00:00:00 )데이터형을 취득 [E2S_TEST_0722_BK]  적산값 계산간격 1일
                    DateTime dtDayAccCalcDate = new DateTime(dtPrevProcTime.Year, dtPrevProcTime.Month, dtPrevProcTime.Day, 0, 0, 0);

                    //전일 날짜의 시간보정( 00:00:00 )데이터형을 취득 [E2S_TEST_0722]  적산값 계산간격 1시간
                    //DateTime dtDayAccCalcDate = new DateTime(dtPrevProcTime.Year, dtPrevProcTime.Month, dtPrevProcTime.Day, dtPrevProcTime.Hour, 0, 0);

                    //전일 발전량 적산치를 계산하여 적산 테이블에 데이터 추가 후 최근 입력데이터를 취득
                    DataTable dtDayPwrAcc = svc.AddCalcData_GreenHubData_PwrDayAcc(dtDayAccCalcDate);

                    if (dtDayPwrAcc != null)
                    {
                        dtDayPwrAcc.TableName = STR_GREENHUB_DAYPWR_DATA_TABLE_NAME;

                        this.dsGreenHubLog.Tables.Add(dtDayPwrAcc);
                    }
                    else
                    {
                        this.strMessage += "적산값계산 실패.\r\n";
                        this.dsGreenHubLog.Tables.Add(Manager.Mng_DataTable.GetDataTableTemplate_PwrDayAccData());
                    }
                }

                TimeSpan tsProcTIme = DateTime.Now - dtNowTime;
                int intProcMin = tsProcTIme.Minutes;
                int intProcSec = tsProcTIme.Seconds;
                int intProcMil = tsProcTIme.Milliseconds;
                float fltProcTime = intProcMin * 60 + intProcSec + (float)(intProcMil / 1000);

                if (this.strMessage.Length == 0)
                {
                    this.strMessage = "GreenHub Thread: Start at " + dtStartTime.ToLongTimeString() + " / " + fltProcTime.ToString() + " Sec";
                }
                else 
                {
                    this.strMessage += dtStartTime.ToLongTimeString() + " / " + fltProcTime.ToString() + " Sec";
                }

                //다음 실행시간까지 슬립
                TimeSpan tsSpan = DateTime.Now - dtNowTime;

                if (tsSpan.TotalSeconds % 60 < (Frm_HMI_Main.intThreadGreenHubInterval - Frm_HMI_Main.intThreadCheckInterval * 2))
                {
                    int intRestSec = (Frm_HMI_Main.intThreadGreenHubInterval - Frm_HMI_Main.intThreadCheckInterval * 2) - Convert.ToInt32((tsSpan.TotalSeconds % 60));

                    System.Threading.Thread.Sleep(intRestSec * 1000);
                }

                this.isRunning = false;

            }
            catch (Exception excep)
            {
                this.strMessage += excep.Message;
                this.isRunning = false;
                return;
            }
        }
    }
}

using System;
using System.Data;
using System.Threading;
using System.Windows.Forms;

namespace HMI_Windows
{
    public partial class Frm_HMI_Main : Form
    {
        #region HMI 메인 클래스 전역변수 정의
        /// <summary> 스레드 상태 확인 작업 실행 간격(초) </summary>
        public static int intThreadCheckInterval = 6;
        /// <summary> 기계실 데이터 취득 작업 실행 간격(초) </summary>
        public static int intThreadMachinRoomInterval = 60; // 1분
        /// <summary> UDP 데이터 취득 작업 실행 간격(초) </summary>
        public static int intThreadUDPInterval = 60; // 3분
        /// <summary> IoT7(Modbus) 데이터 취득 작업 실행 간격(초) </summary>
        public static int intThreadIoT7Interval = 60; // 1분
        /// <summary> 그린허브 및 변온소 및 태양광발전기 데이터 취득작업 실행간격(초) </summary>
        public static int intThreadGreenHubInterval = 60; // 1분
        /// <summary> 데이터 그리드, 스레드 상태창 등의 로그 정보에서 보유가능한 포인트당 이전 통신 정보 수 </summary>
        public const int intMaxRowCount = 5;

        /// <summary> UDP B동 데이터그리드 버퍼용 배열 </summary>
        private System.Collections.ArrayList lstUDPDataCount;

        /// <summary> IOT A동 데이터그리드 버퍼용 배열 </summary>
        private System.Collections.ArrayList lstRoomAIoTCount;
        /// <summary> IOT B동 데이터그리드 버퍼용 배열 </summary>
        private System.Collections.ArrayList lstRoomBIoTCount;
        /// <summary> IOT C동 데이터그리드 버퍼용 배열 </summary>
        private System.Collections.ArrayList lstRoomCIoTCount;

        /// <summary> HMI 화면 갱신용 타이머 </summary>
        System.Windows.Forms.Timer tmMainTimer;

        /// <summary> UDP (B동 객실 및 외기센서) 통신 스레드 </summary>
        Thread trdUDPConnect = null;
        /// <summary> 기계실 (연구동, 숙소동, 유량계) 통신 스레드 </summary>
        Thread trdMachineRoom = null;
        ///// <summary> 객실 I0T 통신 스레드 </summary>
        Thread trdIoT7 = null;
        /// <summary> 그린허브 변온소 및 태양광 발전기 통신 스레드 </summary>
        Thread trdGreenHub = null;

        /// <summary> UDP 통신 구조체 </summary>
        structUDPConnect structUDP;
        /// <summary> 기계실 통신 구조체 </summary>
        structMachineRoom structMR;
        /// <summary> I0T7 (modbus) 통신 구조체 </summary>
        structIoT7Connect structI0T7;
        /// <summary> 그린허브 통신 구조체 </summary>
        structGreenHubConnect structGreenHub;

        #endregion

        #region 구조체 정의 구역
        /// <summary>
        /// UDP 통신 (외기센서 및 B동 난방) 구조체 정의
        /// </summary>
        struct structUDPConnect
        {
            /// <summary> UDP 제어관리 클래스 </summary>
            public Connect.CntUDP cnt;
            /// <summary> 메시지 </summary>
            public string strMessage;
            /// <summary> 데이터 갱신 플래그 </summary>
            public bool isUpdated;

            /// <summary> 연구동 기계실 데이터 </summary>
            public DataSet dsUDPData;

            public structUDPConnect trdProcUDP(structUDPConnect structUDP)
            {
                if (!cnt.isRunning)
                {
                    cnt.UDPConnectionProcess();
                    structUDP.dsUDPData = cnt.dsUDPLogData;
                    structUDP.strMessage = cnt.strMessage;
                    structUDP.isUpdated = true;
                }
                
                return structUDP;
            }
        }

        /// <summary>
        /// 기계실(NI + 유량계 통신) 스레드 구조체 정의
        /// </summary>
        struct structMachineRoom
        {
            /// <summary> 기계실 제어연결 클래스 </summary>
            public Connect.CntMR cnt;
            /// <summary> 메시지 </summary>
            public string strMessage;
            /// <summary> 데이터 갱신 플래그 </summary>
            public bool isUpdated;

            /// <summary> 연구동 기계실 데이터 </summary>
            public DataSet dsMRLogData;

            public structMachineRoom trdProcMachineRoom(structMachineRoom structMachine)
            {
                if (!cnt.isRunning)
                {
                    cnt.MachineRoomProcess();
                    structMachine.dsMRLogData = cnt.dsMRLogData;
                    structMachine.strMessage = cnt.strMessage;
                    structMachine.isUpdated = true;
                }
                return structMachine;
            }
        }

        /// <summary>
        /// IoT7 (모드버스 통신) 스레드 구조체 정의
        /// </summary>
        struct structIoT7Connect
        {
            /// <summary> IoT7 제어연결 클래스 </summary>
            public Connect.CntIoT7 cnt;
            /// <summary> IoT7 메시지 </summary>
            public string strMessage;
            /// <summary> IoT7 데이터 갱신 플래그 </summary>
            public bool isUpdated;

            public DataSet dsIoT7Data;

            public structIoT7Connect trdProcIoT7(structIoT7Connect strctIoT7)
            {
                if (!cnt.isRunning)
                {
                    cnt.IoT7Data_Process();
                    strctIoT7.dsIoT7Data = cnt.dsIoT7Log.Copy();
                    strctIoT7.strMessage = cnt.strMessage;
                    strctIoT7.isUpdated = true;
                }

                return strctIoT7;
            }
        }

        /// <summary>
        /// GreenHub (모드버스 통신) 스레드 구조체 정의
        /// </summary>
        struct structGreenHubConnect
        {
            /// <summary> GreenHub 제어연결 클래스 </summary>
            public Connect.CntGreenHub cnt;
            /// <summary> GreenHub 메시지 </summary>
            public string strMessage;
            /// <summary> GreenHub 데이터 갱신 플래그 </summary>
            public bool isUpdated;

            public DataSet dsGreenHubLog;

            public structGreenHubConnect trdProcGreenHub(structGreenHubConnect strctGH)
            {
                if (!cnt.isRunning)
                {
                    cnt.GreenHub_Process();
                    strctGH.dsGreenHubLog = cnt.dsGreenHubLog.Copy();
                    strctGH.strMessage = cnt.strMessage;
                    strctGH.isUpdated = true;
                }

                return strctGH;
            }
        }

        #endregion

        #region 생성자와 소멸자

        /// <summary>
        /// 메인프레임 클래스 생성자
        /// </summary>
        public Frm_HMI_Main()
        {
            InitializeComponent();

            try
            {
                //화면 갱신 타이머 설정
                this.tmMainTimer = new System.Windows.Forms.Timer();
                this.tmMainTimer.Enabled = true;

                InitDataTables();

                InitStruct();

                InitHMIEventHander();

                InitListBox();

                

                //첫 통신 1회 즉시 시작 후 감시타이머 설정
                //[E2S] 스레드 경량화
                SetThreadUDP();
                SetThreadMR();
                SetThreadIoT();
                SetThreadGreenHub();
                SetHMITimerInterval();
                StartHMITimers();

                btnConnectionStart.Enabled = false;
                btnConnectionStop.Enabled = true;
            }
            catch (Exception excep)
            {
                DialogResult dr = MessageBox.Show(excep.Message);
            }
        }

        /// <summary>
        /// 메인프레임 클래스 소멸자
        /// </summary>
        ~Frm_HMI_Main() 
        {
            this.tmMainTimer.Stop();
            this.tmMainTimer.Dispose();

            this.tmUDP.Stop();
            this.tmUDP.Dispose();

            this.tmMR.Stop();
            this.tmMR.Dispose();

            this.tmIoT.Stop();
            this.tmIoT.Dispose();

            this.tmGH.Stop();
            this.tmGH.Dispose();


            if (trdUDPConnect.ThreadState == ThreadState.Running)
                this.trdUDPConnect.Interrupt();

            if (trdMachineRoom.ThreadState == ThreadState.Running)
                this.trdMachineRoom.Interrupt();

            if (trdIoT7.ThreadState == ThreadState.Running)
                this.trdIoT7.Interrupt();

            if (trdGreenHub.ThreadState == ThreadState.Running)
                this.trdGreenHub.Interrupt();
        }

        #endregion

        #region HMI 프로그램 메인 폼 이벤트

        /// <summary>
        /// 데이터 그리드 업데이트 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgView_Update(object sender, EventArgs e) { ((DataGridView)sender).AutoResizeColumns(); ((DataGridView)sender).Update(); }

        #endregion

        #region 각 변수의 초기화 및 정의

        /// <summary>
        /// 데이터소스로 사용할 테이블 정의
        /// </summary>
        private void InitDataTables()
        {
            //외기센서 데이터소스 테이블 초기화
            this.dtUDPWeatherHex = Manager.Mng_DataTable.GetDataTableTemplate_ConnectionByteData(Connect.CntUDP.STR_WEATHER_BYTE_TABLE_NAME);
            this.dtUDPWeatherData = Manager.Mng_DataTable.GetDataTableTemplate_WeatherData();

            //B동객실 난방 데이터소스 테이블 초기화
            this.dtUDPDongBHex = Manager.Mng_DataTable.GetDataTableTemplate_RoomB_UDP(Connect.CntUDP.STR_DONG_B_DATA_TABLE_NAME);
            this.lstUDPDataCount = new System.Collections.ArrayList();
            //연구동 데이터소스 테이블 초기화
            this.dtLogLabTmp = Manager.Mng_DataTable.GetDataTableTemplate_MR_Lab_Tmp();
            this.dtLogLabPrsFlw = Manager.Mng_DataTable.GetDataTableTemplate_MR_Lab_Prs_Flw();

            //숙소동 데이터 소스 테이블 초기화
            this.dtLogAccTmp = Manager.Mng_DataTable.GetDataTableTemplate_MR_Acc_Tmp();
            this.dtLogAccPrs = Manager.Mng_DataTable.GetDataTableTemplate_MR_Acc_Prs();
            this.dtLogAccFlw = Manager.Mng_DataTable.GetDataTableTemplate_MR_Acc_Flw();
            this.dtLogAccVlv = Manager.Mng_DataTable.GetDataTableTemplate_MR_Acc_Vlv();

            //IoT 객실 데이터 소스 테이블 초기화
            this.dtRoomAI0T = Manager.Mng_DataTable.GetDataTableTemplate_Room_IOT(Connect.CntIoT7.STR_ROOM_A_IOT_TABLE_NAME);
            this.lstRoomAIoTCount = new System.Collections.ArrayList();
            this.dtRoomBI0T = Manager.Mng_DataTable.GetDataTableTemplate_Room_IOT(Connect.CntIoT7.STR_ROOM_B_IOT_TABLE_NAME);
            this.lstRoomBIoTCount = new System.Collections.ArrayList();
            this.dtRoomCI0T = Manager.Mng_DataTable.GetDataTableTemplate_Room_IOT(Connect.CntIoT7.STR_ROOM_C_IOT_TABLE_NAME);
            this.lstRoomCIoTCount = new System.Collections.ArrayList();

            //그린허브탭 데이터소스 테이블 초기화
            this.dtSPCenterData = Manager.Mng_DataTable.GetDataTableTemplate_SPCenterData();
            this.dtSPWeatherData = Manager.Mng_DataTable.GetDataTableTemplate_SPWeatherData();
            this.dtGHDayAccData = Manager.Mng_DataTable.GetDataTableTemplate_PwrDayAccData();

            //이기종탭 데이터소스 테이블 초기화
            this.dtORCData = Manager.Mng_DataTable.GetDataTableTemplate_ORCData();
            this.dtPNCData = Manager.Mng_DataTable.GetDataTableTemplate_PNCData();
            this.dtPVData = Manager.Mng_DataTable.GetDataTableTemplate_PVGenData();
            this.dtWatarWheelData = Manager.Mng_DataTable.GetDataTableTemplate_WheelData();
            this.dtCalorieMeterData = Manager.Mng_DataTable.GetDataTableTemplate_MeterData();

            //외기센서 탭 데이터 그리드 데이터소스 지정
            this.dgvWeatherBytes.DataSource = this.dtUDPWeatherHex;
            this.dgvWeatherData.DataSource = this.dtUDPWeatherData;
            //연구동 탭 데이터 그리드 데이터소스 지정
            this.dgvLabTmp.DataSource = this.dtLogLabTmp;
            this.dgvLabPrsFlw.DataSource = this.dtLogLabPrsFlw;
            //숙소동 탭 데이터 그리드 데이터소스 지정
            this.dgvAccTmp.DataSource = this.dtLogAccTmp;
            this.dgvAccPrs.DataSource = this.dtLogAccPrs;
            this.dgvAccFlw.DataSource = this.dtLogAccFlw;
            this.dgvAccVlv.DataSource = this.dtLogAccVlv;
            //[UDP 난방] B동 객실 탭 데이터 그리드 데이터소스 지정
            this.dgvRoomB_UDP.DataSource = this.dtUDPDongBHex;

            //[GreenHub] 변온소 탭 데이터 그리드 데이터 소스 지정
            this.dgvSPCenter.DataSource = this.dtSPCenterData;
            this.dgvSPWeather.DataSource = this.dtSPWeatherData;
            this.dgvDayAcc.DataSource = this.dtGHDayAccData;

            //[GreenHub] 이기종 탭 데이터 그리드 데이터 소스 지정
            this.dgvORC.DataSource = this.dtORCData;
            this.dgvPNC.DataSource = this.dtPNCData;
            this.dgvPV.DataSource = this.dtPVData;
            this.dgvWatarWheel.DataSource = this.dtWatarWheelData;
            this.dgvCalorieMeter.DataSource = this.dtCalorieMeterData;

            //[IoT7] 각 숙소동 탭 데이터 그리드 데이터 소스 지정
            this.dgvRoomA.DataSource = this.dtRoomAI0T;
            this.dgvRoomB.DataSource = this.dtRoomBI0T;
            this.dgvRoomC.DataSource = this.dtRoomCI0T;
        }

        /// <summary>
        /// 구조체 초기화
        /// </summary>
        private void InitStruct()
        {
            try
            {
                //UDP 통신 구조체
                this.structUDP = new structUDPConnect();
                this.structUDP.cnt = new Connect.CntUDP();
                this.structUDP.strMessage = "";
                this.structUDP.isUpdated = false;
                this.structUDP.dsUDPData = new DataSet();

                //기계실 통신 구조체
                this.structMR = new structMachineRoom();
                this.structMR.cnt = new Connect.CntMR();
                this.structMR.strMessage = "";
                this.structMR.isUpdated = false;
                this.structMR.dsMRLogData = new DataSet();

                //IOT7 통신 구조체
                this.structI0T7 = new structIoT7Connect();
                this.structI0T7.cnt = new Connect.CntIoT7();
                this.structI0T7.strMessage = "";
                this.structMR.isUpdated = false;
                this.structI0T7.dsIoT7Data = new DataSet();

                //그린허브 통신 구조체
                this.structGreenHub = new structGreenHubConnect();
                this.structGreenHub.cnt = new Connect.CntGreenHub();
                this.structGreenHub.strMessage = "";
                this.structGreenHub.isUpdated = false;
                this.structGreenHub.dsGreenHubLog = new DataSet();
            }
            catch (Exception excep)
            {
                MessageBox.Show(excep.Message + "\r\n" + excep);
            }
        }

        /// <summary>
        /// HMI 프로그램 스레드 타이머 설정
        /// </summary>
        private void SetHMITimerInterval()
        {
            //간격 설정
            this.tmMainTimer.Interval = 1000;

            this.tmUDP.Interval = intThreadCheckInterval * 1000;

            this.tmMR.Interval = intThreadCheckInterval * 1000;

            this.tmIoT.Interval = intThreadCheckInterval * 1000;

            this.tmGH.Interval = intThreadCheckInterval * 1000;
        }

        /// <summary>
        /// HMI 프로그램 데이터소스 변경 이벤트 정의
        /// </summary>
        private void InitHMIEventHander()
        {
            //타이머 이벤트 설정
            this.tmMainTimer.Tick += new EventHandler(this.Tick_MainTimer);

            this.tmUDP.Tick += new System.EventHandler(this.UDPProc_Start);

            this.tmMR.Tick += new System.EventHandler(this.MRProc_Start);

            this.tmIoT.Tick += new System.EventHandler(this.IOTProc_Start);

            this.tmGH.Tick += new System.EventHandler(this.GHProc_Start);


            //외기센서 탭 이벤트 설정
            this.dgvWeatherBytes.DataSourceChanged += new EventHandler(this.dgView_Update);
            this.dgvWeatherData.DataSourceChanged += new EventHandler(this.dgView_Update);

            //B동(난방) 탭 이벤트 설정
            this.dgvRoomB_UDP.DataSourceChanged += new EventHandler(this.dgView_Update);

            //연구동 기계실 탭 이벤트 설정
            this.dgvLabTmp.DataSourceChanged += new EventHandler(this.dgView_Update);
            this.dgvLabTmp.DataSourceChanged += new EventHandler(this.dgView_Update);

            //숙소동 기계실 탭 이벤트 설정
            this.dgvAccTmp.DataSourceChanged += new EventHandler(this.dgView_Update);
            this.dgvAccPrs.DataSourceChanged += new EventHandler(this.dgView_Update);
            this.dgvAccFlw.DataSourceChanged += new EventHandler(this.dgView_Update);
            this.dgvAccVlv.DataSourceChanged += new EventHandler(this.dgView_Update);

            //IoT 객실탭 이벤트 설정
            this.dgvRoomA.DataSourceChanged += new EventHandler(this.dgView_Update);
            this.dgvRoomB.DataSourceChanged += new EventHandler(this.dgView_Update);
            this.dgvRoomC.DataSourceChanged += new EventHandler(this.dgView_Update);

            //그린허브 변온소탭 이벤트 설정
            this.dgvSPCenter.DataSourceChanged += new EventHandler(this.dgView_Update);
            this.dgvSPWeather.DataSourceChanged += new EventHandler(this.dgView_Update);
            this.dgvDayAcc.DataSourceChanged += new EventHandler(this.dgView_Update);

            //그린허브 이기종탭 이벤트 설정
            this.dgvORC.DataSourceChanged += new EventHandler(this.dgView_Update);
            this.dgvPNC.DataSourceChanged += new EventHandler(this.dgView_Update);
            this.dgvPV.DataSourceChanged += new EventHandler(this.dgView_Update);
            this.dgvWatarWheel.DataSourceChanged += new EventHandler(this.dgView_Update);
            this.dgvCalorieMeter.DataSourceChanged += new EventHandler(this.dgView_Update);
        }

        /// <summary>
        /// HMI 메인 함수 (리스트박스 초기화)
        /// </summary>
        private void InitListBox()
        {
            try
            {
                Service.SvcDB svc = new Service.SvcDB();

                // 난방객실탭의 객실 리스트 취득
                Manager.Mng_DataTable.SetUDPListBoxDataSource(svc.SelectListBoxValues(Service.SvcDB.enmListBoxValuesTypes.UDP_ROOM_B), ref this.lstbxRoomB_UDP);

                // IOT객실탭의 객실 리스트 취득
                Manager.Mng_DataTable.SetIOTListBoxDataSource(svc.SelectListBoxValues(Service.SvcDB.enmListBoxValuesTypes.IOT_ROOM_A), ref this.lstbxRoomA);
                Manager.Mng_DataTable.SetIOTListBoxDataSource(svc.SelectListBoxValues(Service.SvcDB.enmListBoxValuesTypes.IOT_ROOM_B), ref this.lstbxRoomB);
                Manager.Mng_DataTable.SetIOTListBoxDataSource(svc.SelectListBoxValues(Service.SvcDB.enmListBoxValuesTypes.IOT_ROOM_C), ref this.lstbxRoomC);

            }
            catch (Exception excep)
            {
                MessageBox.Show(excep.Message);
            }
        }

        #endregion

        #region 각 통신별 스레드 생성

        /// <summary>
        /// UDP 통신 스레드 설정 정의
        /// </summary>
        private void SetThreadUDP() 
        {
            try
            {
                if ((this.trdUDPConnect == null) ? true : this.trdUDPConnect.ThreadState == ThreadState.Stopped)
                {
                    if (this.trdUDPConnect == null)
                    {
                        this.trdUDPConnect = new Thread(delegate ()
                        {
                            this.structUDP = structUDP.trdProcUDP(structUDP);
                        });

                        this.trdUDPConnect.Start();

                        string strSetMsg = "Set Thread [UDP].";
                        UpdateThreadStateTextBox(ref strSetMsg, ref this.tbxUDPThreadState);

                        return;
                    }
                    else
                    {
                        //작업이 완료된 상태인 경우 스레드 제거
                        this.trdUDPConnect = null;

                        // string strRmvMsg = "Remove Thread [UDP].";
                        // UpdateThreadStateTextBox(ref strRmvMsg, ref this.tbxUDPThreadState);

                        return;
                    }
                }
                else
                {
                    // this.trdUDPConnect.Interrupt();
                    // string strTrdChkMsg = "[UDP]`s " + this.trdUDPConnect.ThreadState + ", " + DateTime.Now.ToShortTimeString();
                    // UpdateThreadStateTextBox(ref strTrdChkMsg, ref this.tbxUDPThreadState);
                    // this.structUDP.cnt.isRunning = false;

                    //스레드 작업 중지
                    return;
                }
            }
            catch (Exception e)
            {
                string strErrMsg = "Failed to Threading [UDP]. \r\n" + e.Message;
                UpdateThreadStateTextBox(ref strErrMsg, ref this.tbxUDPThreadState);
                this.trdUDPConnect = null;
            }
        }

        /// <summary>
        /// 기계실 스레드 (연구동 기계실, 숙소동 기계실, 숙소동 유량계) 정의
        /// </summary>
        private void SetThreadMR()
        {
            try
            {
                if ((this.trdMachineRoom == null) ? true : this.trdMachineRoom.ThreadState == ThreadState.Stopped)
                {
                    if (this.trdMachineRoom == null)
                    {
                        this.trdMachineRoom = new Thread(delegate ()
                        {
                            this.structMR = structMR.trdProcMachineRoom(structMR);
                        });

                        this.trdMachineRoom.Start();

                        string strSetMsg = "Set Thread [MR].";
                        UpdateThreadStateTextBox(ref strSetMsg, ref this.tbxMRThreadState);

                        return;
                    }
                    else
                    {
                        //완료된 스레드 제거
                        this.trdMachineRoom = null;

                        //string strRmvMsg = "Remove Thread [MR].";
                        //UpdateThreadStateTextBox(ref strRmvMsg, ref this.tbxMRThreadState);

                        return;
                    }
                }
                else
                {
                    // this.trdMachineRoom.Interrupt();
                    // string strBreakMsg= "[MR]`s " + this.trdMachineRoom.ThreadState + ", " + DateTime.Now.ToShortTimeString();
                    // UpdateThreadStateTextBox(ref strBreakMsg, ref this.tbxMRThreadState);
                    // this.structMR.cnt.isRunning = false;

                    //스레드가 동작중이면 작업 중지
                    return;
                }
            }
            catch (Exception e)
            {
                string strErrMsg = "Failed to Set Thread [MR]. \r\n" + e.Message;
                UpdateThreadStateTextBox(ref strErrMsg, ref this.tbxMRThreadState);
                this.trdMachineRoom = null;
            }
        }

        /// <summary>
        /// IOT통신 스레드 정의
        /// </summary>
        private void SetThreadIoT() 
        {
            try
            {
                if ((this.trdIoT7 == null) ? true : this.trdIoT7.ThreadState == ThreadState.Stopped)
                {
                    if (this.trdIoT7 == null)
                    {
                        this.trdIoT7 = new Thread(delegate ()
                        {
                            this.structI0T7 = structI0T7.trdProcIoT7(structI0T7);
                        });

                        this.trdIoT7.Start();

                        string strSetMsg = "Set Thread [IoT].";
                        UpdateThreadStateTextBox(ref strSetMsg, ref this.tbxIoT7ThreadState);

                        return;
                    }
                    else
                    {
                        //완료된 스레드 제거
                        this.trdIoT7 = null;

                        //string strRmvMsg = "Remove Thread [IoT].";
                        //UpdateThreadStateTextBox(ref strRmvMsg, ref this.tbxMRThreadState);

                        return;
                    }
                }
                else
                {
                    //스레드를 정지하고 재생성
                    //this.trdIoT7.Interrupt();
                    //string strMessage= "[IoT]`s " + this.trdIoT7.ThreadState + ", " + DateTime.Now.ToShortTimeString();
                    //UpdateThreadStateTextBox(ref strMessage, ref this.tbxIoT7ThreadState);

                    //스레드가 동작중이면 재시작 작업 중지
                    return;
                }                
            }
            catch (Exception e)
            {
                string strErrMsg = "Failed to Set Thread [IoT]. \r\n" + e.Message;
                UpdateThreadStateTextBox(ref strErrMsg, ref this.tbxIoT7ThreadState);
                this.trdIoT7 = null;
            }
        }

        /// <summary>
        /// 그린허브통신 스레드 정의
        /// </summary>
        private void SetThreadGreenHub() 
        {
            try
            {
                if ((this.trdGreenHub == null) ? true : this.trdGreenHub.ThreadState == ThreadState.Stopped)
                {
                    if (this.trdGreenHub == null)
                    {
                        this.trdGreenHub = new Thread(delegate ()
                        {
                            this.structGreenHub = structGreenHub.trdProcGreenHub(structGreenHub);
                        });

                        this.trdGreenHub.Start();

                        string strSetMsg = "Set Thread [Green Hub].";
                        UpdateThreadStateTextBox(ref strSetMsg, ref this.tbxGHThreadState);

                        return;
                    }
                    else
                    {
                        //완료된 스레드 제거
                        this.trdGreenHub = null;

                        //string strRmvMsg = "Remove Thread [Green Hub].";
                        //UpdateThreadStateTextBox(ref strRmvMsg, ref this.tbxGHThreadState);

                        return;
                    }
                }
                else
                {
                    //스레드 정지하고 재생성
                    //this.trdGreenHub.Interrupt();
                    //string strMessage = "Break Thread [Green Hub]`s " + this.trdGreenHub.ThreadState + ", " + DateTime.Now.ToShortTimeString();
                    //UpdateThreadStateTextBox(ref strMessage, ref this.tbxGHThreadState);
                    //this.structGreenHub.cnt.isRunning = false;

                    //스레드가 동작중이면 재시작 작업 중지
                    return;
                }
            }
            catch (Exception e)
            {
                string strErrMsg = "Failed to Set Thread [Green Hub]. \r\n" + e.Message;
                UpdateThreadStateTextBox(ref strErrMsg, ref this.tbxGHThreadState);
                this.trdGreenHub = null;
            }
        }

        #endregion

        #region 각 타이머 이벤트 내용

        /// <summary>
        /// 메인 타이머 이벤트 (초당 1회 화면 갱신)
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="e"></param>
        private void Tick_MainTimer(object Sender, EventArgs e)
        {
            MachineRoomDataUpdate();

            UDPDataTabUpdate();

            IOT7DataTableUpdate();

            GreenHubDataTableUpdate();
        }

        /// <summary>
        /// UDP 통신 타이머 이벤트
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        void UDPProc_Start(object Sender, EventArgs e)
        {
            //[E2S] 스레드 경량화
            SetThreadUDP();
        }

        /// <summary>
        /// 기계실 통신 타이머 이벤트
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="e"></param>
        void MRProc_Start(object Sender, EventArgs e)
        {
            //[E2S] 스레드 경량화
            SetThreadMR();
        }

        /// <summary>
        /// IOT장비(중계기) 통신 타이머 이벤트
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="e"></param>
        void IOTProc_Start(object Sender, EventArgs e)
        {
            //[E2S] 스레드 경량화
            SetThreadIoT();
        }

        /// <summary>
        /// 그린허브 통신 타이머 이벤트
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        void GHProc_Start(object obj, EventArgs e)
        {
            //[E2S] 스레드 경량화
            SetThreadGreenHub();
        }

        /// <summary>
        /// 스레드 상태창 갱신
        /// </summary>
        /// <param name="strNewMessage"></param>
        /// <param name="tbxThreadState"></param>
        void UpdateThreadStateTextBox(ref string strNewMessage, ref TextBox tbxThreadState) 
        {
            if (strNewMessage.Length > 0)
            {
                string[] arrTrdState = tbxThreadState.Lines;

                if (arrTrdState.Length < intMaxRowCount)
                {
                    Array.Resize(ref arrTrdState, arrTrdState.Length + 1);
                }

                for (int intReverseCount = arrTrdState.Length - 1; intReverseCount > 0; intReverseCount--)
                {
                    arrTrdState[intReverseCount] = arrTrdState[intReverseCount - 1];
                }
                arrTrdState[0] = strNewMessage;

                tbxThreadState.Lines = arrTrdState;

                strNewMessage = string.Empty;
            }
        }



        #endregion

        #region 데이터 소스용 데이터 및 그리드 데이터 업데이트
        
        /// <summary>
        /// 버퍼배열로 부터 현재 표시/보유해야할 행의 갯수 취득
        /// </summary>
        /// <param name="lstArray"></param>
        /// <returns>현재 표시해야할 데이터의 갯수</returns>
        int GetCurrentDataCountFromArrayList(System.Collections.ArrayList lstArray) 
        {
            try 
            {
                int lstArrayLength = lstArray.Count;

                int intReturnCount = 0;

                if (lstArrayLength > intMaxRowCount)
                {
                    //첫번째열의 갯수(삭제 대상) 제거
                    lstArray.RemoveAt(0);
                }

                foreach (object objCount in lstArray)
                {
                    intReturnCount += Convert.ToInt32(objCount);
                }

                return intReturnCount;
            }
            catch(Exception)
            {
                return 0;
            }
        }


        /// <summary>
        /// 기계실 데이터 테이블 갱신
        /// </summary>
        private void MachineRoomDataUpdate()
        {
            //기계실 취득 데이터의 화면반영
            if (this.structMR.isUpdated)
            {
                foreach (DataTable dt in this.structMR.dsMRLogData.Tables)
                {
                    if (dt.Rows.Count <= 0)
                    {
                        continue;
                    }
                    switch (dt.TableName)
                    {
                        case Connect.CntMR.STR_TABLE_NAME_MR_ACC_TMP:

                            foreach (DataRow dr in dt.Rows)
                            {
                                if (this.dtLogAccTmp.Rows.Count >= intMaxRowCount) { this.dtLogAccTmp.Rows.RemoveAt(0); }
                                else if (this.dtLogAccTmp.Rows.Count == 0) { this.dtLogAccTmp = dt.Clone(); }

                                this.dtLogAccTmp.ImportRow(dr);
                            }

                            this.dgvAccTmp.DataSource = this.dtLogAccTmp;

                            break;
                        case Connect.CntMR.STR_TABLE_NAME_MR_ACC_PRS:

                            foreach (DataRow dr in dt.Rows)
                            {
                                if (this.dtLogAccPrs.Rows.Count >= intMaxRowCount) { this.dtLogAccPrs.Rows.RemoveAt(0); }
                                else if (this.dtLogAccPrs.Rows.Count == 0) { this.dtLogAccPrs = dt.Clone(); }

                                this.dtLogAccPrs.ImportRow(dr);
                            }

                            this.dgvAccPrs.DataSource = this.dtLogAccPrs;

                            break;
                        case Connect.CntMR.STR_TABLE_NAME_MR_ACC_FLW:

                            foreach (DataRow dr in dt.Rows)
                            {
                                if (this.dtLogAccFlw.Rows.Count >= intMaxRowCount) { this.dtLogAccFlw.Rows.RemoveAt(0); }
                                else if (this.dtLogAccFlw.Rows.Count == 0) { this.dtLogAccFlw = dt.Clone(); }

                                this.dtLogAccFlw.ImportRow(dr);
                            }

                            this.dgvAccFlw.DataSource = this.dtLogAccFlw;

                            break;
                        case Connect.CntMR.STR_TABLE_NAME_MR_ACC_VLV:

                            foreach (DataRow dr in dt.Rows)
                            {
                                if (this.dtLogAccVlv.Rows.Count >= intMaxRowCount) { this.dtLogAccVlv.Rows.RemoveAt(0); }
                                else if (this.dtLogAccVlv.Rows.Count == 0) { this.dtLogAccVlv = dt.Clone(); }

                                this.dtLogAccVlv.ImportRow(dr);
                            }

                            this.dgvAccVlv.DataSource = this.dtLogAccVlv;

                            break;
                        case Connect.CntMR.STR_TABLE_NAME_MR_LAB_TMP:

                            foreach (DataRow dr in dt.Rows)
                            {
                                if (this.dtLogLabTmp.Rows.Count >= intMaxRowCount) { this.dtLogLabTmp.Rows.RemoveAt(0); }
                                else if (this.dtLogLabTmp.Rows.Count == 0) { this.dtLogLabTmp = dt.Clone(); }

                                this.dtLogLabTmp.ImportRow(dr);
                            }

                            this.dgvLabTmp.DataSource = this.dtLogLabTmp;

                            break;
                        case Connect.CntMR.STR_TABLE_NAME_MR_LAB_PRS_FLW:

                            foreach (DataRow dr in dt.Rows)
                            {
                                if (this.dtLogLabPrsFlw.Rows.Count >= intMaxRowCount) { this.dtLogLabPrsFlw.Rows.RemoveAt(0); }
                                else if (this.dtLogLabPrsFlw.Rows.Count == 0) { this.dtLogLabPrsFlw = dt.Clone(); }

                                this.dtLogLabPrsFlw.ImportRow(dr);
                            }

                            this.dgvLabPrsFlw.DataSource = this.dtLogLabPrsFlw;

                            break;
                    }
                }

                UpdateThreadStateTextBox(ref this.structMR.strMessage, ref this.tbxMRThreadState);

                this.structMR.isUpdated = false;
            }
        }

        /// <summary>
        ///  UDP (B동 객실 및 외기센서) 데이터 테이블 및 그리드 데이터 업데이트
        /// </summary>
        private void UDPDataTabUpdate()
        {
            try
            {
                if (structUDP.isUpdated)
                {
                    foreach (DataTable dtData in this.structUDP.dsUDPData.Tables)
                    {
                        if (dtData.Rows.Count <= 0)
                        {
                            continue;
                        }

                        switch (dtData.TableName)
                        {
                            case Connect.CntUDP.STR_WEATHER_BYTE_TABLE_NAME:
                                foreach (DataRow dr in dtData.Rows)
                                {
                                    if (this.dtUDPWeatherHex.Rows.Count > intMaxRowCount) { this.dtUDPWeatherHex.Rows.RemoveAt(0); }
                                    else if (this.dtUDPWeatherHex.Rows.Count == 0) { this.dtUDPWeatherHex = dtData.Clone(); }
                                    this.dtUDPWeatherHex.ImportRow(dr);
                                }
                                this.dgvWeatherBytes.DataSource = this.dtUDPWeatherHex;
                                break;
                            case Connect.CntUDP.STR_WEATHER_DATA_TABLE_NAME:
                                foreach (DataRow dr in dtData.Rows)
                                {
                                    if (this.dtUDPWeatherData.Rows.Count > intMaxRowCount) { this.dtUDPWeatherData.Rows.RemoveAt(0); }
                                    else if (this.dtUDPWeatherData.Rows.Count == 0) { this.dtUDPWeatherData = dtData.Clone(); }
                                    this.dtUDPWeatherData.ImportRow(dr);
                                }
                                this.dgvWeatherData.DataSource = this.dtUDPWeatherData;
                                break;
                            case Connect.CntUDP.STR_DONG_B_DATA_TABLE_NAME:

                                this.lstUDPDataCount.Add(dtData.Rows.Count);
                                int intDataSourceRowMax = GetCurrentDataCountFromArrayList(lstUDPDataCount);

                                foreach (DataRow dr in dtData.Rows)
                                {
                                    if (this.dtUDPDongBHex.Rows.Count == 0) { this.dtUDPDongBHex = dtData.Clone(); }
                                    this.dtUDPDongBHex.ImportRow(dr);
                                }

                                while (dtUDPDongBHex.Rows.Count > intDataSourceRowMax)
                                {
                                    this.dtUDPDongBHex.Rows.RemoveAt(0);
                                }

                                this.dgvRoomB_UDP.DataSource = this.dtUDPDongBHex;
                                break;
                            default:
                                break;
                        }
                    }

                    UpdateThreadStateTextBox(ref this.structUDP.strMessage, ref this.tbxUDPThreadState);

                    this.structUDP.isUpdated = false;
                
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        /// <summary>
        /// IoT7 데이터 테이블 및 그리드 데이터 업데이트
        /// </summary>
        private void IOT7DataTableUpdate()
        {
            try
            {
                if (!this.structI0T7.isUpdated)
                {
                    return;
                }
                else
                {
                    int intDataSourceRowMax;
                    foreach (DataTable dt in this.structI0T7.dsIoT7Data.Tables)
                    {
                        switch (dt.TableName)
                        {
                            case Connect.CntIoT7.STR_ROOM_A_IOT_TABLE_NAME:
                                this.lstRoomAIoTCount.Add(dt.Rows.Count);
                                intDataSourceRowMax = GetCurrentDataCountFromArrayList(lstRoomAIoTCount);

                                foreach (DataRow dr in dt.Rows)
                                {
                                    this.dtRoomAI0T.ImportRow(dr);
                                }

                                while (dtRoomAI0T.Rows.Count > intDataSourceRowMax)
                                {
                                    this.dtRoomAI0T.Rows.RemoveAt(0);
                                }

                                this.dgvRoomA.DataSource = this.dtRoomAI0T;
                                break;
                            case Connect.CntIoT7.STR_ROOM_B_IOT_TABLE_NAME:
                                this.lstRoomBIoTCount.Add(dt.Rows.Count);
                                intDataSourceRowMax = GetCurrentDataCountFromArrayList(lstRoomBIoTCount);

                                foreach (DataRow dr in dt.Rows)
                                {
                                    this.dtRoomBI0T.ImportRow(dr);
                                }

                                while (dtRoomBI0T.Rows.Count > intDataSourceRowMax)
                                {
                                    this.dtRoomBI0T.Rows.RemoveAt(0);
                                }

                                this.dgvRoomB.DataSource = this.dtRoomBI0T;
                                break;
                            case Connect.CntIoT7.STR_ROOM_C_IOT_TABLE_NAME:
                                this.lstRoomCIoTCount.Add(dt.Rows.Count);
                                intDataSourceRowMax = GetCurrentDataCountFromArrayList(lstRoomCIoTCount);

                                foreach (DataRow dr in dt.Rows)
                                {
                                    this.dtRoomCI0T.ImportRow(dr);
                                }

                                while (dtRoomCI0T.Rows.Count > intDataSourceRowMax)
                                {
                                    this.dtRoomCI0T.Rows.RemoveAt(0);
                                }

                                this.dgvRoomC.DataSource = this.dtRoomCI0T;
                                break;
                            default:
                                break;
                        }
                    }

                    UpdateThreadStateTextBox(ref this.structI0T7.strMessage, ref this.tbxIoT7ThreadState);

                    this.structI0T7.isUpdated = false;
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        /// <summary>
        /// 그린허브 통신이력테이블 업데이트
        /// </summary>
        private void GreenHubDataTableUpdate()
        {
            try
            {
                if (!this.structGreenHub.isUpdated)
                {
                    return;
                }
                else
                {
                    int intDataSourceRowMax = intMaxRowCount;
                    foreach (DataTable dt in this.structGreenHub.dsGreenHubLog.Tables)
                    {
                        switch (dt.TableName)
                        {
                            case Connect.CntGreenHub.STR_GREENHUB_CENTER_DATA_TABLE_NAME:
                                foreach (DataRow dr in dt.Rows)
                                {
                                    if (this.dtSPCenterData.Rows.Count > intDataSourceRowMax) { this.dtSPCenterData.Rows.RemoveAt(0); }
                                    this.dtSPCenterData.ImportRow(dr);
                                }
                                this.dgvSPCenter.DataSource = this.dtSPCenterData;
                                break;
                            case Connect.CntGreenHub.STR_GREENHUB_WEATHER_DATA_TABLE_NAME:
                                foreach (DataRow dr in dt.Rows)
                                {
                                    if (this.dtSPWeatherData.Rows.Count > intDataSourceRowMax) { this.dtSPWeatherData.Rows.RemoveAt(0); }
                                    this.dtSPWeatherData.ImportRow(dr);
                                }
                                this.dgvSPWeather.DataSource = this.dtSPWeatherData;
                                break;
                            case Connect.CntGreenHub.STR_GREENHUB_DAYPWR_DATA_TABLE_NAME:
                                foreach (DataRow dr in dt.Rows)
                                {
                                    if (this.dtGHDayAccData.Rows.Count > intDataSourceRowMax) { this.dtGHDayAccData.Rows.RemoveAt(0); }
                                    this.dtGHDayAccData.ImportRow(dr);
                                }
                                this.dgvDayAcc.DataSource = this.dtGHDayAccData;
                                break;
                            case Connect.CntGreenHub.STR_GREENHUB_ORC_DATA_TABLE_NAME:
                                foreach (DataRow dr in dt.Rows)
                                {
                                    if (this.dtORCData.Rows.Count > intDataSourceRowMax) { this.dtORCData.Rows.RemoveAt(0); }
                                    this.dtORCData.ImportRow(dr);
                                }
                                this.dgvORC.DataSource = this.dtORCData;
                                break;
                            case Connect.CntGreenHub.STR_GREENHUB_PNC_DATA_TABLE_NAME:
                                foreach (DataRow dr in dt.Rows)
                                {
                                    if (this.dtPNCData.Rows.Count > intDataSourceRowMax) { this.dtPNCData.Rows.RemoveAt(0); }
                                    this.dtPNCData.ImportRow(dr);
                                }
                                this.dgvPNC.DataSource = this.dtPNCData;
                                break;
                            case Connect.CntGreenHub.STR_GREENHUB_PVGen_DATA_TABLE_NAME:
                                foreach (DataRow dr in dt.Rows)
                                {
                                    if (this.dtPVData.Rows.Count > intDataSourceRowMax) { this.dtPVData.Rows.RemoveAt(0); }
                                    this.dtPVData.ImportRow(dr);
                                }
                                this.dgvPV.DataSource = this.dtPVData;
                                break;
                            case Connect.CntGreenHub.STR_GREENHUB_WHEEL_DATA_TABLE_NAME:
                                foreach (DataRow dr in dt.Rows)
                                {
                                    if (this.dtWatarWheelData.Rows.Count > intDataSourceRowMax) { this.dtWatarWheelData.Rows.RemoveAt(0); }
                                    this.dtWatarWheelData.ImportRow(dr);
                                }
                                this.dgvWatarWheel.DataSource = this.dtWatarWheelData;
                                break;
                            case Connect.CntGreenHub.STR_GREENHUB_METER_DATA_TABLE_NAME:
                                foreach (DataRow dr in dt.Rows)
                                {
                                    if (this.dtCalorieMeterData.Rows.Count > intDataSourceRowMax) { this.dtCalorieMeterData.Rows.RemoveAt(0); }
                                    this.dtCalorieMeterData.ImportRow(dr);
                                }
                                this.dgvCalorieMeter.DataSource = this.dtCalorieMeterData;
                                break;
                            default:
                                break;
                        }
                    }

                    UpdateThreadStateTextBox(ref this.structGreenHub.strMessage, ref this.tbxGHThreadState);

                    this.structGreenHub.isUpdated = false;
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        #endregion

        #region 각 타이머 시작/종료 이벤트

        /// <summary>
        /// HMI프로그램 타이머 시작
        /// </summary>
        private void StartHMITimers()
        {
            if (!this.tmMainTimer.Enabled)
            {
                this.tmMainTimer.Start();
            }

            if (!this.tmUDP.Enabled)
            {
                this.tmUDP.Start();
            }

            if (!this.tmMR.Enabled)
            {
                this.tmMR.Start();
            }

            if (!this.tmIoT.Enabled)
            {
                this.tmIoT.Start();
            }

            if (!this.tmGH.Enabled)
            {
                this.tmGH.Start();
            }
        }

        /// <summary>
        /// HMI타이머 종료
        /// </summary>
        private void StopHMIThreads()
        {
            if (this.tmMainTimer.Enabled)
            {
                this.tmMainTimer.Stop();
            }

            if (this.tmUDP.Enabled)
            {
                this.tmUDP.Stop();
            }

            if (this.tmMR.Enabled)
            {
                this.tmMR.Stop();
            }

            if (this.tmIoT.Enabled)
            {
                this.tmIoT.Stop();
            }

            if (this.tmGH.Enabled)
            {
                this.tmGH.Stop();
            }
        }

        #endregion

        #region 버튼 이벤트

        /// <summary>
        /// 시작버튼 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, EventArgs e)
        {
            //각 타이머 시작
            try
            {
                StartHMITimers();
                btnConnectionStart.Enabled = false;
                btnConnectionStop.Enabled = true;
            }
            catch (Exception exception)
            {

                StopHMIThreads();
                btnConnectionStart.Enabled = true;
                btnConnectionStop.Enabled = false;

                MessageBox.Show(exception.Message);
            }
        }

        /// <summary>
        /// 중지 버튼 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                //타이머 중지
                StopHMIThreads();
                btnConnectionStart.Enabled = true;
                btnConnectionStop.Enabled = false;
            }
            catch (Exception excep)
            {
                StopHMIThreads();

                btnConnectionStart.Enabled = true;
                btnConnectionStop.Enabled = false;

                MessageBox.Show(excep.Message);
            }
        }

        #endregion

        #region UDP 및 IOT 통신 탭의 객실 선택박스 관련 이벤트
        /// <summary>
        /// IOT A동 탭 객실 선택 변경 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstbx_A_IOT_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                ListBox lstbx = (ListBox)sender;
                if (lstbx.SelectedIndex == 0)
                {
                    this.dtRoomAI0T.DefaultView.RowFilter = string.Empty;
                }
                else
                {
                    string strSelectedRoom = Convert.ToString(lstbx.SelectedItem);
                    if (!strSelectedRoom.Equals(""))
                        this.dtRoomAI0T.DefaultView.RowFilter = "[Ho] = " + strSelectedRoom;
                }

            }
            catch (Exception excep)
            {
                MessageBox.Show(excep.Message);
                return;
            }
        }
        /// <summary>
        /// IOT B동 탭 객실 선택 변경 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstbx_B_IOT_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                ListBox lstbx = (ListBox)sender;
                if (lstbx.SelectedIndex == 0)
                {
                    this.dtRoomBI0T.DefaultView.RowFilter = string.Empty;
                }
                else
                {
                    string strSelectedRoom = Convert.ToString(lstbx.SelectedItem);
                    if (!strSelectedRoom.Equals(""))
                        this.dtRoomBI0T.DefaultView.RowFilter = "[Ho] = " + strSelectedRoom;
                }

            }
            catch (Exception excep)
            {
                MessageBox.Show(excep.Message);
                return;
            }
        }
        /// <summary>
        /// IOT C동 탭 객실 선택 변경 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstbx_C_IOT_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                ListBox lstbx = (ListBox)sender;

                if (lstbx.SelectedIndex == 0)
                {
                    this.dtRoomCI0T.DefaultView.RowFilter = string.Empty;
                }
                else
                {
                    string strSelectedRoom = Convert.ToString(lstbx.SelectedItem);
                    if (!strSelectedRoom.Equals(""))
                        this.dtRoomCI0T.DefaultView.RowFilter = "[Ho] = " + strSelectedRoom;
                }

            }
            catch (Exception excep)
            {
                MessageBox.Show(excep.Message);
                return;
            }
        }

        /// <summary>
        /// B동(난방) 탭 객실 선택 변경 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstbx_B_UDP_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                ListBox lstbx = (ListBox)sender;
                if (lstbx.SelectedIndex == 0)
                {
                    this.dtUDPDongBHex.DefaultView.RowFilter = string.Empty;
                }
                else
                {
                    string strSelectedRoom = Convert.ToString(lstbx.SelectedItem);
                    if (!strSelectedRoom.Equals(""))
                        this.dtUDPDongBHex.DefaultView.RowFilter = "[RoomCd] LIKE \'" + strSelectedRoom + "\'";
                }
                this.dgvRoomB_UDP.Update();
            }
            catch (Exception excep)
            {
                MessageBox.Show(excep.Message);
                this.lstbxRoomC.Update();
                return;
            }
        }

       

        /// <summary>
        /// 각 호실 탭의 호실 리스트 박스 초기화 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbconHMI_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strSelectedTabName = ((TabControl)sender).SelectedTab.Name;

            try
            {

                //A동 탭을 선택한 경우
                if (strSelectedTabName.Equals(this.tabRoomA.Name))
                {
                    this.lstbxRoomA.SelectedItem = "";
                    this.lstbxRoomA.Update();
                    this.lstbxRoomB.SelectedIndex = -1;
                    this.lstbxRoomC.SelectedIndex = -1;
                    this.lstbxRoomB_UDP.SelectedIndex = -1;
                }
                //B동 탭을 선택한 경우
                else if (strSelectedTabName.Equals(this.tabRoomB_UDP.Name))
                {
                    this.lstbxRoomB.SelectedItem = "";
                    this.lstbxRoomA.SelectedIndex = -1;
                    this.lstbxRoomB.Update();
                    this.lstbxRoomC.SelectedIndex = -1;
                    this.lstbxRoomB_UDP.SelectedIndex = -1;
                }
                //C동 탭을 선택한 경우
                else if (strSelectedTabName.Equals(this.tabRoomC.Name))
                {
                    this.lstbxRoomC.SelectedItem = "";
                    this.lstbxRoomA.SelectedIndex = -1;
                    this.lstbxRoomB.SelectedIndex = -1;
                    this.lstbxRoomC.Update();
                    this.lstbxRoomB_UDP.SelectedIndex = -1;
                }
                else if (strSelectedTabName.Equals(this.tabRoomB_UDP.Name))
                {
                    this.lstbxRoomB_UDP.SelectedItem = "";
                    this.lstbxRoomA.SelectedIndex = -1;
                    this.lstbxRoomB.SelectedIndex = -1;
                    this.lstbxRoomC.SelectedIndex = -1;
                    this.lstbxRoomB_UDP.Update();
                }
                else
                {
                    //객실탭 외의 탭을 선택한 경우 객실탭의 선택박스를 미선택 상태로 전환
                    this.lstbxRoomA.SelectedIndex = -1;
                    this.lstbxRoomB.SelectedIndex = -1;
                    this.lstbxRoomC.SelectedIndex = -1;
                    this.lstbxRoomB_UDP.SelectedIndex = -1;

                }

            }
            catch (Exception excep)
            {
                MessageBox.Show(excep.Message);
            }
        }

        #endregion
    }
}

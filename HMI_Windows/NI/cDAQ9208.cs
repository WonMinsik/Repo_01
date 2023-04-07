using System;
using System.Collections;
using System.Data;
using NationalInstruments.DAQmx;

namespace HMI_Windows.NI
{
    class cDAQ9208
    {
        private DaqSystem daqSys;
        private ArrayList lstAIChannelPhysicalNames = new ArrayList();
        private AnalogMultiChannelReader amcr;
        private DataColumn[] dataColumn = null;

        /// <summary> 기계실 연구동 네트워크 디바이스 - 디바이스 이름 </summary>
        private const string STR_NI_NETWORK_DEVICE_NAME_LAB = "cDAQ9185-1E5EA6B";
        /// <summary> 기계실 연구동 네트워크 디바이스 - 디바이스 주소 </summary>
        private const string STR_NI_NETWORK_DEVICE_ADDR_LAB = "192.168.0.201";
        /// <summary> 기계실 숙소동 네트워크 디바이스 - 디바이스 이름 </summary>
        private const string STR_NI_NETWORK_DEVICE_NAME_ACC = "cDAQ9185-1E5EAF7";
        /// <summary> 기계실 숙소동 네트워크 디바이스 - 디바이스 주소 </summary>
        private const string STR_NI_NETWORK_DEVICE_ADDR_ACC = "192.168.0.200";
        /// <summary> 기계실 NI 장비 타임아웃 </summary>
        private const double DBL_NI_DEVICE_TIMEOUT = 3000.0f;

        /// <summary> NI-9208 숙소동 장치식별자 </summary>
        public const string STR_NI9208_RESIDENCE_IDENTIFIER = "cDAQ9208";
        /// <summary> NI-9208 숙소동 사용 채널 갯수 </summary>
        public const int INT_NI9208_RESIDENCE_CHANNEL_LENGTH = 14; //채널 2,3번의 유무 확인 필요 20220408
        /// <summary> NI-9208 연구동 장치식별자 </summary>
        public const string STR_NI9208_RESEARCH_IDENTIFIER = "cDAQ9208_2";
        /// <summary> NI-9208 연구동 사용 채널 갯수 </summary>
        public const int INT_NI9208_RESEARCH_CHANNEL_LENGTH = 8;
        /// <summary> NI-9208 채널식별자 </summary>
        public const string STR_NI9208_CHANNEL_IDENTIFIER = "ai";

        /// <summary> NI-9208 계측전류 최소값 </summary>
        private const double DBL_MIN_VALUE = 0; //0mA??
        /// <summary> NI-9208 계측전류 최대값 </summary>
        private const double DBL_MAX_VALUE = 20.0e-3; //20mA? 
        /// <summary> NI-9208 션트저항 값 </summary>
        private const double DBL_SHUNT_RESISTOR_VALUE = 34.01;
        /// <summary> 샘플링 갯수 </summary>
        /// 최소 샘플 수 1
        private const int INT_FINITE_SAMPLES = 1;
        /// <summary> 클럭 설정수치 </summary>
        /// 2ms속도로 측정
        private const int INT_SAMPLECLOCK = 500;


        /// <summary> 장치식별자 </summary>
        private String strDeviceIdentifier = String.Empty;
        /// <summary> 배치채널명 </summary>
        private String strNameToAssignChannel = String.Empty;

        /// <summary>
        /// cDAQ9208 제어 클래스 생성자
        /// </summary>
        /// <param name="strDeviceIdentifier">장치식별자</param>
        /// <param name="strNameToAssignChannel">배치채널명</param>
        /// <param name="dblMinValue">최소값</param>
        /// <param name="dblMaxValue">최대값</param>
        /// <param name="dblShuntResistorValue">션트 저항값 (채널 접근방식이 External인 경우)</param>
        /// <param name="intSamples"></param>
        /// <param name="intClock"></param>
        public cDAQ9208(String strDeviceIdentifier, String strNameToAssignChannel = "",
            double dblMinValue = DBL_MIN_VALUE, double dblMaxValue = DBL_MAX_VALUE, double dblShuntResistorValue = DBL_SHUNT_RESISTOR_VALUE,
            int intSamples = INT_FINITE_SAMPLES, int intClock = INT_SAMPLECLOCK)
        {
            this.strDeviceIdentifier = strDeviceIdentifier;
            this.strNameToAssignChannel = strNameToAssignChannel;

            this.daqSys = DaqSystem.Local;
        }

        /// <summary>
        /// NI-9208 디바이스 전류 데이터 취득
        /// </summary>
        /// <returns>전류데이터 1샘플 테이블</returns>
        public DataTable GetAmpDataFromNI9208() 
        {
            //테스크 생성
            NationalInstruments.DAQmx.Task tsk_NI9208 = new NationalInstruments.DAQmx.Task();
            try
            {

                //물리채널목록 생성
                bool isChannelExist = GetPhysicalChannelList(strDeviceIdentifier, out lstAIChannelPhysicalNames);

                //Error
                if (!isChannelExist)
                {
                    tsk_NI9208.Dispose();
                    return null;
                }
                Console.WriteLine("Program:: CREATE CURRENT CHANNEL");
                // 가상채널 생성
                if (true)
                {
                    //션트저항 위치 Internal 고정, 채널은 외부채널 고정
                    foreach (string strPhysicalChannel in lstAIChannelPhysicalNames)
                    {
                        tsk_NI9208.AIChannels.CreateCurrentChannel(strPhysicalChannel, strNameToAssignChannel,
                            AITerminalConfiguration.Rse,
                            DBL_MIN_VALUE,
                            DBL_MAX_VALUE,
                            AICurrentUnits.Amps
                        );
                    }
                }

                tsk_NI9208.AIChannels.All.AdcTimingMode = AIAdcTimingMode.HighResolution;

                Console.WriteLine("Program:: SET SAMPLE CLOCK");

                tsk_NI9208.Timing.ConfigureSampleClock("", Convert.ToDouble(INT_FINITE_SAMPLES),
                    SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, INT_SAMPLECLOCK);


                // Verify the Task
                tsk_NI9208.Control(TaskAction.Verify);

                Console.WriteLine("Program:: RUN TASK");

                // acquisitionDataGrid.DataSource = dataTable;

                Console.WriteLine("Program::AnalogMultiChannelReader run");
                amcr = new AnalogMultiChannelReader(tsk_NI9208.Stream);
                amcr.SynchronizeCallbacks = true;
                double[] arrDblValues = amcr.ReadSingleSample();
                return dataToDataTable(arrDblValues);
            }
            catch (Exception)
            {
                tsk_NI9208.Dispose();
                throw;
            }
            finally
            {
                tsk_NI9208.Dispose();
                this.daqSys = null;
            }
        }

        /// <summary>
        /// 물리적채널리스트 취득
        /// </summary>
        /// <param name="strDeviceIdentifier">장치식별자</param>
        /// <returns>물리적채널 리스트</returns>
        private bool GetPhysicalChannelList(String strDeviceIdentifier, out System.Collections.ArrayList lstExternalPChannels)
        {
            string[] arrExternalAIPhysicalChannels;
            int intChannelLength = 0;
            try
            {
                string strPhysicalChannelTemplate = strDeviceIdentifier + "/" + STR_NI9208_CHANNEL_IDENTIFIER;
                if (strDeviceIdentifier.Equals(STR_NI9208_RESEARCH_IDENTIFIER))//연구동인 경우
                {
                    // Device daqDevice = daqSys.AddNetworkDevice(STR_NI_NETWORK_DEVICE_ADDR_LAB, STR_NI_NETWORK_DEVICE_NAME_LAB, DBL_NI_DEVICE_TIMEOUT);
                    arrExternalAIPhysicalChannels = daqSys.GetPhysicalChannels(PhysicalChannelTypes.AI, PhysicalChannelAccess.External);
                    intChannelLength = INT_NI9208_RESEARCH_CHANNEL_LENGTH;
                }
                else
                {
                    // Device daqDevice = daqSys.AddNetworkDevice(STR_NI_NETWORK_DEVICE_ADDR_ACC, STR_NI_NETWORK_DEVICE_NAME_ACC, DBL_NI_DEVICE_TIMEOUT);
                    arrExternalAIPhysicalChannels = daqSys.GetPhysicalChannels(PhysicalChannelTypes.AI, PhysicalChannelAccess.External);
                    intChannelLength = INT_NI9208_RESIDENCE_CHANNEL_LENGTH;
                }

                if (arrExternalAIPhysicalChannels.Length <= 0)
                {
                    lstExternalPChannels = null;

                    return false;
                }
                else
                {
                    lstExternalPChannels = new ArrayList();

                    //External AI채널 확인
                    if (arrExternalAIPhysicalChannels.Length > 0)
                    {
                        System.Collections.ArrayList lstPhysicalChannelBuffer = new System.Collections.ArrayList();

                        //cDAQ9208의 외부채널만 추출
                        foreach (string strPhysicalChannelName in arrExternalAIPhysicalChannels)
                        {
                            if (strPhysicalChannelName.Contains(strPhysicalChannelTemplate))
                            {
                                lstPhysicalChannelBuffer.Add(strPhysicalChannelName);
                            }
                        }

                        //cDAQ9208의 외부채널이 존재하는 경우
                        if (lstPhysicalChannelBuffer.Count > 0)
                        {
                            Console.WriteLine("Program:: Get External PhysicalChannels \n[" + string.Join(",", (string[])lstPhysicalChannelBuffer.ToArray(Type.GetType("System.String"))) + "]");
                            
                            //cDAQ9208의 사용채널을 목록에 추가
                            for (int intCount = 0; intCount < intChannelLength; intCount++)
                            {
                                lstExternalPChannels.Add(lstPhysicalChannelBuffer[intCount]);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Program:: External AI PhysicalChannel does not exist");
                        }
                    }
                    else
                    {
                        lstExternalPChannels = new System.Collections.ArrayList();
                    }
                }
                return true;
            }
            catch (DaqException)
            {
                throw;
            }
        }

        /// <summary>
        /// 데이터 테이블 초기화
        /// </summary>
        /// <param name="dtTable">데이터 테이블</param>
        public void InitializeDataTable(out DataTable dtTable)
        {
            int numOfChannels = lstAIChannelPhysicalNames.Count;
            dtTable = new DataTable();
            dataColumn = new DataColumn[numOfChannels];

            for (int currentChannelIndex = 0; currentChannelIndex < numOfChannels; currentChannelIndex++)
            {
                dataColumn[currentChannelIndex] = new DataColumn();
                dataColumn[currentChannelIndex].DataType = typeof(double);
                dataColumn[currentChannelIndex].ColumnName = Convert.ToString(lstAIChannelPhysicalNames[currentChannelIndex]);
            }
            dtTable.Columns.AddRange(dataColumn);
        }

        /// <summary>
        /// 아날로그 데이터를 데이터 테이블로 변환
        /// </summary>
        /// <param name="sourceArray">아날로그 데이터</param>
        /// <returns>데이터 테이블</returns>
        private DataTable dataToDataTable(double[] arrData)
        {
            
            try
            {
                object[] objData = new object[arrData.Length];
                for (int intCnt = 0; intCnt < arrData.Length; intCnt++) 
                {
                    objData[intCnt] = arrData[intCnt];
                }

                // Prepare the table for Data
                InitializeDataTable(out DataTable dataTable);
                dataTable.Rows.Add(objData);


                return dataTable;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

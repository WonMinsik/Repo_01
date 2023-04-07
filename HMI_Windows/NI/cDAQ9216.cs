using System;
using System.Collections;
using System.Data;
using NationalInstruments.DAQmx;

namespace HMI_Windows.NI
{
    class cDAQ9216
    {
        private DaqSystem daqSys;
        private ArrayList lstAIChannelPhysicalNames = new ArrayList();
        private AnalogMultiChannelReader amcr;
        private DataColumn[] dataColumn;

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

        /// <summary> NI-9216 숙소동 장치식별자 </summary>
        public const string STR_NI9216_RESIDENCE_IDENTIFIER = "cDAQ9216";
        /// <summary> NI-9216 연구동 장치식별자 </summary>
        public const string STR_NI9216_RESEARCH_IDENTIFIER = "cDAQ9216_2";
        /// <summary> NI-9216 사용 채널 갯수 </summary>
        public const int INT_NI9216_CHANNEL_LENGTH = 8;
        /// <summary> NI-9216 채널식별자 </summary>
        public const string STR_NI9216_CHANNEL_IDENTIFIER = "ai";

        /// <summary> NI-9216 Temp RTD 최소값 </summary>
        private const decimal DCM_MIN_VALUE = 0;
        /// <summary> NI-9216 Temp RTD 최대값 </summary>
        private const decimal DCM_MAX_VALUE = 100;
        /// <summary> NI-9216 ro값 </summary>
        private const decimal DCM_RO_VALUE = 100;
        /// <summary> NI-9216 현재 여자값 </summary>
        private const double DBL_CURRENT_EXCITATION_VALUE = 0.001;
        /// <summary> 샘플링 갯수 </summary>
        /// 총 8채널 200ms 걸리도록 10샘플로 설정?
        private const int INT_FINITE_SAMPLES = 2;
        /// <summary> 클럭 설정수치 </summary>
        /// 400Hz --2.5ms속도로 측정
        private const int INT_SAMPLECLOCK = 400;


        /// <summary> 장치식별자 </summary>
        private String strDeviceIdentifier = String.Empty;
        /// <summary> 배치채널명 </summary>
        private String strNameToAssignChannel = String.Empty;

        public cDAQ9216(String strDeviceIdentifier, String strNameToAssignChannel = "",
            decimal dcmMinValue = 0, decimal dcmMaxValue = 100, decimal dcmROValue = 100,
            double dblCurrentExcitationValue = 0.001, int intFiniteSamples = 10, int intSampleClock = 400)
        {
            this.strDeviceIdentifier = strDeviceIdentifier;
            this.strNameToAssignChannel = strNameToAssignChannel;

            this.daqSys = DaqSystem.Local;
        }

        /// <summary>
        /// NI-9216에서 RTD온도 취득
        /// </summary>
        /// <returns>취득 온도 데이터 테이블</returns>
        public DataTable GetRTDTempFromNI9216()
        {
            NationalInstruments.DAQmx.Task tsk_NI9216 = new NationalInstruments.DAQmx.Task();
            try
            {
                bool isChannelExist = GetListPhysicalChannels(strDeviceIdentifier, out lstAIChannelPhysicalNames);
                if (!isChannelExist)
                {
                    Console.WriteLine("Program::" + strDeviceIdentifier + " has No Channels");
                    return null;
                }

                if (true) // 채널 생성
                {
                    foreach (string strPhysicalChannel in lstAIChannelPhysicalNames)
                    {
                        tsk_NI9216.AIChannels.CreateRtdChannel(strPhysicalChannel, strNameToAssignChannel,
                            Convert.ToDouble(DCM_MIN_VALUE),
                            Convert.ToDouble(DCM_MAX_VALUE),
                            AITemperatureUnits.DegreesC,
                            AIRtdType.Pt3750,
                            AIResistanceConfiguration.FourWire,
                            AIExcitationSource.Internal,
                            DBL_CURRENT_EXCITATION_VALUE,
                            Convert.ToDouble(DCM_RO_VALUE)
                        );
                    }
                }


                tsk_NI9216.AIChannels.All.AdcTimingMode = AIAdcTimingMode.HighResolution;

                tsk_NI9216.Timing.ConfigureSampleClock("", Convert.ToDouble(INT_SAMPLECLOCK),
                    SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, INT_FINITE_SAMPLES);

                tsk_NI9216.Control(TaskAction.Verify);

                amcr = new AnalogMultiChannelReader(tsk_NI9216.Stream);
                amcr.SynchronizeCallbacks = true;
                double[] arrData = amcr.ReadSingleSample();
                //IAsyncResult iarResult = amcr.BeginReadWaveform(INT_FINITE_SAMPLES, cbkAsync, tempTask);

                //data = amcr.EndReadWaveform(iarResult);
                tsk_NI9216.Dispose();
                return dataToDataTable(arrData);
            }
            catch (Exception)
            {
                tsk_NI9216.Dispose();
                throw;
            }
            finally
            {
                tsk_NI9216.Dispose();
                this.daqSys = null;
            }
        }

        /// <summary>
        /// 물리적채널목록 취득
        /// </summary>
        /// <param name="strDeviceIdentifier">장치식별자</param>
        /// <param name="lstExternalPChannels">External 물리채널 목록</param>
        /// <returns>채널 목록 취득 여부</returns>
        private bool GetListPhysicalChannels(String strDeviceIdentifier, out System.Collections.ArrayList lstExternalPChannels)
        {
            string[] arrExternalAIPhysicalChannels;
            int intChannelLength = INT_NI9216_CHANNEL_LENGTH;
            try
            {
                string strPhysicalChannelTemplate = strDeviceIdentifier + "/" + STR_NI9216_CHANNEL_IDENTIFIER;
                if (strDeviceIdentifier.Equals(STR_NI9216_RESEARCH_IDENTIFIER))//연구동인 경우
                {
                    // Device daqDevice = daqSys.AddNetworkDevice(STR_NI_NETWORK_DEVICE_ADDR_LAB, STR_NI_NETWORK_DEVICE_NAME_LAB, DBL_NI_DEVICE_TIMEOUT);
                    arrExternalAIPhysicalChannels = daqSys.GetPhysicalChannels(PhysicalChannelTypes.AI, PhysicalChannelAccess.External);
                }
                else
                {
                    // Device daqDevice = daqSys.AddNetworkDevice(STR_NI_NETWORK_DEVICE_ADDR_ACC, STR_NI_NETWORK_DEVICE_NAME_ACC, DBL_NI_DEVICE_TIMEOUT);
                    arrExternalAIPhysicalChannels = daqSys.GetPhysicalChannels(PhysicalChannelTypes.AI, PhysicalChannelAccess.External);
                }

                intChannelLength = INT_NI9216_CHANNEL_LENGTH;

                if (arrExternalAIPhysicalChannels.Length <= 0 )
                {
                    lstExternalPChannels = null;

                    return false;
                }
                else
                {
                    lstExternalPChannels = new System.Collections.ArrayList();

                    //External AI채널 확인
                    if (arrExternalAIPhysicalChannels.Length > 0)
                    {
                        System.Collections.ArrayList lstPhysicalChannelBuffer = new System.Collections.ArrayList();

                        //로컬에 존재하는 채널에서 cDAQ9216 채널만 추출
                        foreach (string strPhysicalChannelName in arrExternalAIPhysicalChannels)
                        {
                            if (strPhysicalChannelName.Contains(strPhysicalChannelTemplate))
                            {
                                lstPhysicalChannelBuffer.Add(strPhysicalChannelName);
                            }
                        }

                        //cDAQ9216의 외부채널이 존재하는 경우
                        if (lstPhysicalChannelBuffer.Count > 0)
                        {
                            //외부채널에서 사용 설정이 된 채널 수 만큼 추출
                            for (int intCount = 0; intCount < intChannelLength; intCount++)
                            {
                                lstExternalPChannels.Add(lstPhysicalChannelBuffer[intCount]);
                            }
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
        /// 아날로그 데이터 배열를 데이터 테이블 형태로 변환
        /// </summary>
        /// <param name="arrData">아날로그 데이터</param>
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
                throw;
            }
        }
    }
}

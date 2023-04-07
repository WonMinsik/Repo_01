using System;
using System.Collections;
using System.Data;
using NationalInstruments.DAQmx;

namespace HMI_Windows.NI
{
    class cDAQ9217
    {
        private DaqSystem daqSys;
        private ArrayList lstAIChannelPhysicalNames;
        private AnalogMultiChannelReader amcr;
        private DataColumn[] dataColumn;

        /// <summary> 기계실 숙소동 네트워크 디바이스 - 디바이스 이름 </summary>
        private const string STR_NI_NETWORK_DEVICE_NAME_ACC = "cDAQ9185-1E5EAF7";
        /// <summary> 기계실 숙소동 네트워크 디바이스 - 디바이스 주소 </summary>
        private const string STR_NI_NETWORK_DEVICE_ADDR_ACC = "192.168.0.200";
        /// <summary> 기계실 NI 장비 타임아웃 </summary>
        private const double DBL_NI_DEVICE_TIMEOUT = 3000.0f;

        /// <summary> NI-9217 장치식별자 </summary>
        public const string STR_NI9217_IDENTIFIER = "cDAQ9217";
        /// <summary> NI-9217 사용 채널 갯수 </summary>
        public const int INT_NI9217_CHANNEL_LENGTH = 3;
        /// <summary> NI-9217 채널식별자 </summary>
        public const string STR_NI9217_CHANNEL_IDENTIFIER = "ai";

        /// <summary> NI-9217 Temp RTD 최소값 </summary>
        private const decimal DCM_MIN_VALUE = 0;
        /// <summary> NI-9217 Temp RTD 최대값 </summary>
        private const decimal DCM_MAX_VALUE = 100;
        /// <summary> NI-9217 ro값 </summary>
        private const decimal DCM_RO_VALUE = 100;
        /// <summary> NI-9217 현재 여자값 </summary>
        private const Double DBL_CURRENT_EXCITATION_VALUE = 0.001;
        /// <summary> 샘플링 갯수 </summary>
        private const int INT_FINITE_SAMPLES = 2;
        /// <summary> 클럭 설정수치 </summary>
        /// 2.5ms속도로 측정?
        private const int INT_SAMPLECLOCK = 400;


        /// <summary> 장치식별자 </summary>
        private String strDeviceIdentifier = String.Empty;
        /// <summary> 배치채널명 </summary>
        private String strNameToAssignChannel = String.Empty;


        public cDAQ9217(String strDeviceIdentifier, String strNameToAssignChannel = "",
            decimal dcmMinValue = 0, decimal dcmMaxValue = 100, decimal dcmROValue = 100,
            double dblCurrentExcitationValue = 0.001, int intFiniteSamples = 2, int intSampleClock = 400)
        {
            this.strDeviceIdentifier = strDeviceIdentifier;
            this.strNameToAssignChannel = strNameToAssignChannel;

            this.daqSys = DaqSystem.Local;
        }

        /// <summary>
        /// NI-9217 RTD온도 취득
        /// </summary>
        /// <returns>취득데이터</returns>
        public DataTable GetRTDTempFromNI9217()
        {
            NationalInstruments.DAQmx.Task tsk_NI9217 = new NationalInstruments.DAQmx.Task();
            try
            {
                bool isPhysicalChannelExist = GetListPhysicalChannels(strDeviceIdentifier, out lstAIChannelPhysicalNames);

                if (!isPhysicalChannelExist)
                {
                    Console.WriteLine("Program::" + strDeviceIdentifier + " has No Channels");
                    return null;
                }

                //외부 채널
                foreach (string strPhysicalChannel in lstAIChannelPhysicalNames)
                {
                    tsk_NI9217.AIChannels.CreateRtdChannel(strPhysicalChannel, strNameToAssignChannel,
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

                tsk_NI9217.AIChannels.All.AdcTimingMode = AIAdcTimingMode.HighResolution;

                tsk_NI9217.Timing.ConfigureSampleClock("", Convert.ToDouble(INT_SAMPLECLOCK),
                    SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, INT_FINITE_SAMPLES);

                tsk_NI9217.Control(TaskAction.Verify);

                amcr = new AnalogMultiChannelReader(tsk_NI9217.Stream);

                amcr.SynchronizeCallbacks = true;
                double[] dblvalues = amcr.ReadSingleSample();

                tsk_NI9217.Dispose();

                return dataToDataTable(dblvalues);
            }
            catch (Exception)
            {
                tsk_NI9217.Dispose();
                throw;
            }
            finally
            {
                tsk_NI9217.Dispose();
                this.daqSys = null;
            }
        }

        /// <summary>
        /// 물리적채널리스트 취득
        /// </summary>
        /// <param name="strDeviceIdentifier">장치식별자</param>
        /// <returns>물리적채널 리스트</returns>
        private bool GetListPhysicalChannels(String strDeviceIdentifier, out System.Collections.ArrayList lstExternalPChannels)
        {
            string[] arrExternalAIPhysicalChannels;
            try
            {
                string strPhysicalChannelTemplate = strDeviceIdentifier + "/" + STR_NI9217_CHANNEL_IDENTIFIER;

                // Device daqDevice = daqSys.AddNetworkDevice(STR_NI_NETWORK_DEVICE_ADDR_ACC, STR_NI_NETWORK_DEVICE_NAME_ACC, DBL_NI_DEVICE_TIMEOUT);
                arrExternalAIPhysicalChannels = daqSys.GetPhysicalChannels(PhysicalChannelTypes.AI, PhysicalChannelAccess.External);
                int intChannelLength = INT_NI9217_CHANNEL_LENGTH;

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

                        foreach (string strPhysicalChannelName in arrExternalAIPhysicalChannels)
                        {
                            if (strPhysicalChannelName.Contains(strPhysicalChannelTemplate))
                            {
                                lstPhysicalChannelBuffer.Add(strPhysicalChannelName);
                            }
                        }
    
                        for (int intCount = 0; intCount < intChannelLength; intCount++)
                        {
                            lstExternalPChannels.Add(lstPhysicalChannelBuffer[intCount]);
                        }

                        if (lstPhysicalChannelBuffer.Count > 0)
                        {
                            Console.WriteLine("Program:: Get External PhysicalChannels \n[" + string.Join(",", (string[])lstPhysicalChannelBuffer.ToArray(Type.GetType("System.String"))) + "]");
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
        /// 배열데이터를 데이터 테이블형으로 전환
        /// </summary>
        /// <param name="arrData">전환할 데이터</param>
        /// <returns>데이터 테이블</returns>
        private DataTable dataToDataTable(double[] arrData)
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
    }
}

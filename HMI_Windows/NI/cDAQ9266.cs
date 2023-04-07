using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Data;
using NationalInstruments.DAQmx;

namespace HMI_Windows.NI
{
    class MdlCdData
    {
        public string strValveCd { get; set; }

        public string strChannel { get; set; }

        public MdlCdData(string strValve, string strChnl) 
        {
            this.strValveCd = strValve;
            this.strChannel = strChnl;
        }
    }

    /// <summary>
    /// 숙소동 cDAQ9266 디바이스 클래스
    /// </summary>
    class cDAQ9266
    {
        private DaqSystem daqSys;
        private ArrayList lstNI9266AOChannels = new ArrayList();
        private AnalogSingleChannelWriter amcw;

        /// <summary> 기계실 숙소동 네트워크 디바이스 - 디바이스 이름 </summary>
        private const string STR_NI_NETWORK_DEVICE_NAME_ACC = "cDAQ9185-1E5EAF7";
        /// <summary> 기계실 숙소동 네트워크 디바이스 - 디바이스 주소 </summary>
        private const string STR_NI_NETWORK_DEVICE_ADDR_ACC = "192.168.0.200";
        /// <summary> 기계실 NI 장비 타임아웃 </summary>
        private const double DBL_NI_DEVICE_TIMEOUT = 3000.0f;

        /// <summary> NI-9266 숙소동 장치식별자 </summary>
        public const string STR_NI9266_IDENTIFIER = "cDAQ9266"; //"cDAQ9185-1E5EAF7Mod4";
        /// <summary> NI-9266 숙소동 채널 갯수 </summary>
        public const int INT_NI9266_CHANNEL_LENGTH = 5;
        /// <summary> NI-9266 채널식별자 </summary>
        public const string STR_NI9266_CHANNEL_IDENTIFIER = "ao";

        /// <summary> NI-9266 숙소동 밸브 코드 목록 </summary>
        private static string[] ARR_NI9266_VALVECD = { "v121", "v122", "v123", "v124", "v125" };

        /// <summary> NI-9266 숙소동 채널 이름 목록 </summary>
        private static string[] ARR_NI9266_CHANNEL = { STR_NI9266_IDENTIFIER + "/"+STR_NI9266_CHANNEL_IDENTIFIER+"0"
                , STR_NI9266_IDENTIFIER + "/" + STR_NI9266_CHANNEL_IDENTIFIER + "1"
                , STR_NI9266_IDENTIFIER + "/" + STR_NI9266_CHANNEL_IDENTIFIER + "2"
                , STR_NI9266_IDENTIFIER + "/" + STR_NI9266_CHANNEL_IDENTIFIER + "3"
                , STR_NI9266_IDENTIFIER + "/" + STR_NI9266_CHANNEL_IDENTIFIER + "4" };

        /// <summary> 채널-밸브코드 정보 목록 </summary>
        private ArrayList lstChannelValveCdData = new ArrayList();

        /// <summary> NI-9266 전류 최소값 </summary>
        private const double DBL_MIN_VALUE = 4.0e-3;
        /// <summary> NI-9266 계측전류 최대값 </summary>
        private const double DBL_MAX_VALUE = 20.0e-3;
        /// <summary> 샘플링 갯수 </summary>
        /// 총 16채널 320ms 걸리도록 10샘플로 설정
        private const int INT_FINITE_SAMPLES = 10;
        /// <summary> 클럭 설정수치 </summary>
        /// 2ms속도로 측정
        private const int INT_SAMPLECLOCK = 500;


        /// <summary> 장치식별자 </summary>
        private String strDeviceIdentifier = String.Empty;
        /// <summary> 배치채널명 </summary>
        private String strNameToAssignChannel = String.Empty;

        /// <summary>
        /// cDAQ9266생성자
        /// </summary>
        /// <param name="strDeviceIdentifier">장치 식별자</param>
        public cDAQ9266(String strDeviceIdentifier)
        {
            this.strDeviceIdentifier = strDeviceIdentifier;
            this.strNameToAssignChannel = "CDAQ9266_ValveControl";
            this.daqSys = DaqSystem.Local;
            CreateChannelValveCdData();
        }
        /// <summary>
        /// cDAQ9266생성자
        /// </summary>
        /// <param name="strDeviceIdentifier">장비식별자</param>
        /// <param name="strNameToAssignChannel">배치채널명</param>
        /// <param name="dblMinValue">최소값</param>
        /// <param name="dblMaxValue">최대값</param>
        /// <param name="intFiniteSamples">샘플링갯수</param>
        /// <param name="intSampleClock">클럭설정수치</param>
        public cDAQ9266(String strDeviceIdentifier, String strNameToAssignChannel = "CDAQ9266_ValveControl", double dblMinValue = 4.0e-3, double dblMaxValue = 20.0e-3, int intFiniteSamples = 10, int intSampleClock = 500)
        {
            this.strDeviceIdentifier = strDeviceIdentifier;
            this.strNameToAssignChannel = strNameToAssignChannel;
            CreateChannelValveCdData();
        }

        /// <summary>
        /// NI-9266의 채널명과 밸브명을 매치한 데이터를 작성
        /// </summary>
        private void CreateChannelValveCdData() 
        {
            for(int intCnt = 0; intCnt < ARR_NI9266_VALVECD.Length; intCnt++)
            {
                MdlCdData mdlData = new MdlCdData(ARR_NI9266_VALVECD[intCnt], ARR_NI9266_CHANNEL[intCnt]);
                this.lstChannelValveCdData.Add(mdlData);
            }
        }

        /// <summary>
        /// 문자열 형식의 DB취득값을 실제 NI장비에 적용할 값으로 변환
        /// </summary>
        /// <param name="strWriteValue">문자열형식의 DB취득값</param>
        /// <returns>double형 장비입력값</returns>
        private double ConvertWriteValueToInputValue(string strWriteValue)
        {
            float fltValveControlPercentage = 0;
            try
            {
                fltValveControlPercentage = (float)(Convert.ToSingle(strWriteValue) / 100);
            }
            catch (Exception)
            {
                if (int.TryParse(strWriteValue, out int intWriteValue))
                {
                    fltValveControlPercentage = ((float)(intWriteValue)) / 100;
                }
                else
                {
                    fltValveControlPercentage = 0;
                }
            }
            
            
            double dblTempVal = DBL_MAX_VALUE - DBL_MIN_VALUE; //입력값 변위폭

            double dblValue = (double)fltValveControlPercentage * dblTempVal + DBL_MIN_VALUE;

            return dblValue;
        }

        
        /// <summary>
        /// NI-9266 디바이스 제어 (채널)
        /// </summary>
        /// <param name="strTargetValveCd">제어대상 밸브코드</param>
        /// <param name="strWriteableValue">제어값</param>
        /// <param name="strErrMsg">에러 메시지</param>
        /// <returns></returns>
        public bool WriteValveValue(string strTargetValveCd, string strWriteableValue, ref string strErrMsg)
        {
            bool isVlvCtrlOk = false;
            string strTargetChannel = string.Empty;
            strErrMsg = string.Empty;
            try
            {
                System.Collections.ArrayList arrPhysicalChannels = new System.Collections.ArrayList();
                System.Collections.ArrayList arrAOChannels = new System.Collections.ArrayList();

                // Device daqDevice = daqSys.AddNetworkDevice(STR_NI_NETWORK_DEVICE_ADDR_ACC, STR_NI_NETWORK_DEVICE_NAME_ACC, DBL_NI_DEVICE_TIMEOUT);
                string[] arrLocalPhysicalChannels = daqSys.GetPhysicalChannels(PhysicalChannelTypes.AO, PhysicalChannelAccess.All);
                
                

                //채널 - 밸브 매치 데이터에서 일치하는 밸브코드가 있는지 확인
                foreach (MdlCdData mdlCdData in this.lstChannelValveCdData)
                {
                    //밸브코드가 일치하는지 확인
                    if (strTargetValveCd.Equals(mdlCdData.strValveCd))
                    {
                        //밸브코드가 일치하는 경우, 채널명을 취득하고 밸브개폐값을 변환한다
                        strTargetChannel = mdlCdData.strChannel;
                        double dblValue = ConvertWriteValueToInputValue(strWriteableValue);

                        if (!arrLocalPhysicalChannels.Contains(strTargetChannel))
                        {
                            //일치하는 밸브를 못찾은 경우 false반환
                            strErrMsg = strTargetValveCd + "의 제어 채널을 찾을 수 없습니다. : " + strTargetChannel;
                            isVlvCtrlOk = false;
                            return isVlvCtrlOk;
                        }

                        using (NationalInstruments.DAQmx.Task tskWriteVavleValue = new NationalInstruments.DAQmx.Task())
                        {
                            tskWriteVavleValue.AOChannels.CreateCurrentChannel(
                                strTargetChannel,
                                strNameToAssignChannel,
                                DBL_MIN_VALUE,
                                DBL_MAX_VALUE,
                                AOCurrentUnits.Amps);
                            amcw = new AnalogSingleChannelWriter(tskWriteVavleValue.Stream);
                            amcw.WriteSingleSample(true, dblValue);
                        }

                        isVlvCtrlOk = true;
                    }
                    else
                    {
                        //일치하지않으면 다음 검색상대로
                        continue;
                    }
                }


                
                return isVlvCtrlOk;
            }
            catch (Exception)
            {
                string errbuffer = string.Join(",", daqSys.Devices);

                strErrMsg += errbuffer;

                throw;
            }
        }
    }
}

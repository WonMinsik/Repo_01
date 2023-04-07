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
    class cDAQ9265
    {
        private DaqSystem daqSys;
        private AnalogSingleChannelWriter amcw;

        /// <summary> 기계실 연구동 네트워크 디바이스 - 디바이스 이름 </summary>
        private const string STR_NI_NETWORK_DEVICE_NAME_LAB = "cDAQ9185-1E5EA6B";
        /// <summary> 기계실 연구동 네트워크 디바이스 - 디바이스 주소 </summary>
        private const string STR_NI_NETWORK_DEVICE_ADDR_LAB = "192.168.0.201";
        /// <summary> 기계실 NI 장비 타임아웃 </summary>
        private const double DBL_NI_DEVICE_TIMEOUT = 3000.0f;

        /// <summary> NI-9265 연구동 장치식별자 </summary>
        public const string STR_NI9265_IDENTIFIER = "cDAQ9185-1E5EA6BMod3";
        /// <summary> NI-9265 연구동 채널 갯수 </summary>
        public const int INT_NI9265_CHANNEL_LENGTH = 5;
        /// <summary> NI-9265 / NI-9265 채널식별자 </summary>
        public const string STR_NI9265_CHANNEL_IDENTIFIER = "ao";

        /// <summary> 채널-밸브코드 정보 목록 </summary>
        private ArrayList lstChannelValveCdData = new ArrayList();

        /// <summary> NI-9265 밸브 코드 목록 </summary>
        private static string[] ARR_NI9265_VALVECD = { "v999" };

        /// <summary> NI-9265 채널 이름 목록 </summary>
        private static string[] ARR_NI9265_CHANNEL = { STR_NI9265_IDENTIFIER + "/ao0" };


        /// <summary> NI-9265 / NI-9266 전류 최소값 </summary>
        private const double DBL_MIN_VALUE = 4.0e-3;
        /// <summary> NI-9265 / NI-9266 계측전류 최대값 </summary>
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
        /// cDAQ9265 생성자
        /// </summary>
        /// <param name="strDeviceIdentifier">장비 식별자</param>
        public cDAQ9265(String strDeviceIdentifier)
        {
            this.strDeviceIdentifier = strDeviceIdentifier;
            this.strNameToAssignChannel = "cDAQ9265_WriteTask";

            this.daqSys = DaqSystem.Local;
        }

        /// <summary>
        /// cDAQ9265생성자
        /// </summary>
        /// <param name="strDeviceIdentifier">장비식별자</param>
        /// <param name="strNameToAssignChannel">배치채널명</param>
        /// <param name="dblMinValue">최소값</param>
        /// <param name="dblMaxValue">최대값</param>
        /// <param name="intFiniteSamples">샘플링갯수</param>
        /// <param name="intSampleClock">클럭설정수치</param>
        public cDAQ9265(String strDeviceIdentifier, String strNameToAssignChannel = "cDAQ9265_WriteTask", double dblMinValue = 4.0e-3, double dblMaxValue = 20.0e-3, int intFiniteSamples = 10, int intSampleClock = 500)
        {
            this.strDeviceIdentifier = strDeviceIdentifier;
            this.strNameToAssignChannel = strNameToAssignChannel;
        }

        /// <summary>
        /// NI-9266의 채널명과 밸브명을 매치한 데이터를 작성
        /// </summary>
        private void CreateChannelValveCdData()
        {
            for (int intCnt = 0; intCnt < ARR_NI9265_VALVECD.Length; intCnt++)
            {
                MdlCdData mdlData = new MdlCdData(ARR_NI9265_VALVECD[intCnt], ARR_NI9265_CHANNEL[intCnt]);
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
        /// 디바이스 제어값 송신
        /// </summary>
        /// <param name="strTargetValveCd">대상 밸브명(밸브코드)</param>
        /// <param name="intWriteableValue">대상 밸브개폐율</param>
        /// <returns>송신 성공여부</returns>
        public bool WriteValveValue(string strTargetValveCd, string strWriteableValue)
        {
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
                        string strChannel = mdlCdData.strChannel;
                        double dblValue = ConvertWriteValueToInputValue(strWriteableValue);

                        using (NationalInstruments.DAQmx.Task testTask = new NationalInstruments.DAQmx.Task())
                        {
                            testTask.AOChannels.CreateCurrentChannel(
                                strChannel,
                                strNameToAssignChannel,
                                DBL_MIN_VALUE,
                                DBL_MAX_VALUE,
                                AOCurrentUnits.Amps);
                            amcw = new AnalogSingleChannelWriter(testTask.Stream);
                            amcw.WriteSingleSample(true, dblValue);
                            Console.WriteLine("::Write [" + dblValue + "] to " + strChannel);
                        }

                        return true;
                    }
                    else
                    {
                        //일치하지않으면 다음 검색상대로
                        continue;
                    }
                }


                //일치하는 밸브를 못찾은 경우 false반환
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

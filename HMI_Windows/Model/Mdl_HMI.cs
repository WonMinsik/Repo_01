using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace HMI_Windows.Model
{
    /// <summary>
    /// IoT7 멀티센서 제어 정보
    /// </summary>
    class MdlIOT7SensorData
    {        
        /// <summary> 객실정보 </summary>
        public MdlRoomInfo mdlRoomInfo { get; set; }

        /// <summary> 통신 시간 </summary>
        public DateTime dtConnectTime { get; set; }

        /// <summary> 통신회수 </summary>
        public uint intConnectionCount { get; set; }

        /// <summary> 온도 </summary>
        public decimal dcmTemp { get; set; }

        /// <summary> 습도 </summary>
        public decimal dcmHumidity { get; set; }

        /// <summary> 휘발성 유기화합물 </summary>
        public uint intVOCs { get; set; }

        /// <summary> 이산화탄소 </summary>
        public uint intCo2 { get; set; }

        /// <summary> 조도 (단위 : Lux) </summary>
        public uint intIllumiance { get; set; }

        /// <summary> 모션감지 </summary>
        public uint intMovement { get; set; }

        /// <summary> Reserve값 </summary>
        public uint intReserve { get; set; }

        /// <summary> IR버튼 동작 횟수 </summary>
        public uint intIRBtnCount { get; set; }

        /// <summary> 수신감도 </summary>
        public uint intRSSI { get; set; }
    }

    /// <summary>
    /// 객실 정보 모델
    /// </summary>
    class MdlRoomInfo 
    {
        /// <summary>[B동] 시스템 내 객실ID </summary>
        public int intID { get; set; }

        /// <summary> 객실 동 정보 </summary>
        public string strDong { get; set; }

        /// <summary> 객실 층 정보 </summary>
        public int intFloor { get; set; }

        /// <summary> 객실 방번호 정보 </summary>
        public int intHo { get; set; }

        /// <summary> 객실 센서 번호 </summary>
        public int intSensorNo { get; set; }
    }
    /// <summary>
    /// 외기계측 데이터 모델 클래스
    /// </summary>
    class MdlWeatherData
    {
        /// <summary> 주소 </summary>
        public byte BYTE_ADDRESS_BLOCK { get; set; }

        /// <summary> Function Code </summary>
        public byte BYTE_FUNC_CODE { get; set; }

        /// <summary> 수신 바이트 수 </summary>
        public byte BYTE_RECV_BYTE { get; set; }

        /// <summary> 기기 상태 </summary>
        public Int32 intDevStatus { get; set; }

        /// <summary> 풍향 </summary>
        public Int32 intWindDir { get; set; }
        /// <summary> 풍속 </summary>
        public double dblWindSpeed { get; set; }

        /// <summary> 기온 </summary>
        public double dblAirTemp { get; set; }

        /// <summary> 습도 </summary>
        public double dblAirHumi { get; set; }

        /// <summary> 기압 </summary>
        public double dblAirPress { get; set; }

        /// <summary> 날씨 </summary>
        public Int32 intWeather { get; set; }

        /// <summary> 강우량 </summary>
        public double dblRainFall { get; set; }

        /// <summary> 강우량 누적 </summary>
        public double dblRainFallAcc { get; set; }

        /// <summary> 단위 </summary>
        public Int32 intRainUnit { get; set; }

        /// <summary> 방사선 누적 </summary>
        public double dblRadiationAcc { get; set; }

        /// <summary> 방사선 </summary>
        public double dblRadiation { get; set; }

        /// <summary> 전체CRC값 </summary>
        public Int32 intCheckBlock { get; set; }
    }

    /// <summary>
    /// 시스템용 객실 인패킷 데이터 모델 클래스
    /// </summary>
    class MdlInPacketData
    {
        /// <summary> 객실정보 </summary>
        public MdlRoomInfo mdlRoomInfo { get; set; }
        /// <summary> 제어명령번호 </summary>
        public int intControlOrderNo { get; set; }
        /// <summary> 쓰기허용여부 </summary>
        public bool isReadOnly { get; set; }
        /// <summary> 난방설정 </summary>
        public int intHeatOnOff { get; set; }
        /// <summary> 설정온도 </summary>
        public int intSetTemp { get; set; }
        /// <summary> 온도차 </summary>
        public int intDiffTemp { get; set; }
        /// <summary> 유량적산 초기화여부 </summary>
        public bool isTotalReset { get; set; }
    }

    /// <summary>
    /// 객실 아웃패킷 데이터 모델 클래스
    /// </summary>
    class MdlOutPacketData
    {
        /// <summary> 객실정보 </summary>
        public MdlRoomInfo mdlRoomInfo { get; set; }
        /// <summary> 난방ON/OFF </summary>
        public short shtHeatOnOff { get; set; }
        /// <summary> 난방 설정 온도 </summary>
        public float fltHeatSetTemp { get; set; }
        /// <summary> 객실온도 </summary>
        public float fltInsideTemp { get; set; }
        /// <summary> 공급온도 </summary>
        public float fltInHeating { get; set; }
        /// <summary> 환수온도 </summary>
        public float fltOutHeating { get; set; }
        /// <summary> 순시유량 </summary>
        public float fltNowFlow { get; set; }
        /// <summary> 적산유량 </summary>
        public float fltTotalFlow { get; set; }
        /// <summary> 현재 밸브 개도율 </summary>
        public short shtNowControlValue { get; set; }
        
        /// <summary> 델타온도값 </summary>
        public int intDeltaTTemp { get; set; }
        /// <summary> 객실바닥(중계기 센서)온도 </summary>
        public int intFloorTemp { get; set; }
        /// <summary> 설정 밸브 개도율 </summary>
        public short shtSetControlValue { get; set; }
        /// <summary> 난방 적산 </summary>
        public float fltTotalHeat { get; set; }
    }
   

    /// <summary>
    /// NI디바이스 채널별 데이터 모델 (cDAQ-9266용)
    /// </summary>
    class MdlDeviceChannelLog
    {
        /// <summary> 데이터 모델 생성자 </summary>
        public MdlDeviceChannelLog() { }

        /// <summary> 포인트명 </summary>
        public string strPointName { get; set; }

        /// <summary> 채널명 </summary>
        public string strChannelName { get; set; }

        /// <summary> double형 값 </summary>
        public double dblValue { get; set; }
    }
}

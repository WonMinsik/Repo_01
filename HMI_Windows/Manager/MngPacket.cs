using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using HMI_Windows.Model;
using HMI_Windows.Service;

namespace HMI_Windows.Manager
{
    class MngPacket
    {
        //인패킷 바이트 데이터 길이
        private static int intInPacketLength = 11;
        //
        public static int INT_INPACKET_RESERVE_DEFAULT_VALUE = 15;

        //인패킷 LEN 데이터 기본값
        private static int INT_INPACKET_DATA_LEN = 7;

        /// <summary> 인패킷 STX </summary>
        private static byte BYTE_INPACKET_STX = 0x02;

        /// <summary> 인패킷 ETX </summary>
        private static byte BYTE_INPACKET_ETX = 0x03;

        /// <summary> 인패킷 읽기/쓰기 </summary>
        private static byte BYTE_INPACKET_READWRITE = 1;

        /// <summary> 인패킷 읽기전용 </summary>
        private static byte BYTE_INPACKET_READONLY = 0;

        /// <summary> 인패킷 리저브 고정값 </summary>
        private static byte BYTE_INPACKET_STATIC_RESERVE = 0x11;

        /// <summary> 실내온도 제어OFF시 온도 </summary>
        private static byte BYTE_CTRL_SET_TEMP_ZERO = 0x00;

        /// <summary> 실내온도 수동 제어ON </summary>
        private static byte BYTE_CTRL_MANU_HEAT_ON = 0x01;

        /// <summary> 실내온도 자동 제어 ON </summary>
        private static byte BYTE_CTRL_AUTO_HEAT_ON = 0x11;

        /// <summary> 실내온도 제어 OFF </summary>
        private static byte BYTE_CTRL_HEAT_OFF = 0x00;

        /// <summary> 유량적산치 초기화 </summary>
        private static byte BYTE_FLOWTOTAL_RESET = 0x01;

        /// <summary> 유량적산치 정상 </summary>
        private static byte BYTE_FLOWTOTAL_NORMAL = 0x00;

        private SvcDB svcDB = new SvcDB();


        /// <summary>
        /// 외기센서에 전송할 데이터 생성
        /// </summary>
        /// <returns>외기센서 송신 데이터 </returns>
        public byte[] SetWeatherSensorSendData() 
        {
            byte[] arrReturnData = new byte[8];
            //addr_block
            arrReturnData[0] = 0x01;
            //func_code
            arrReturnData[1] = 0x03;
            //Reserve1
            arrReturnData[2] = 0x00;
            //Reserve2
            arrReturnData[3] = 0x00;
            //Reserve3
            arrReturnData[4] = 0x00;
            //Reserve4
            arrReturnData[5] = 0x23;
            //Reserve5
            arrReturnData[6] = 0x04;
            //Reserve6
            arrReturnData[7] = 0x13;

            return arrReturnData;
        }

        /// <summary>
        /// 외기센서 패킷 데이터 변환
        /// </summary>
        /// <param name="btData">패킷 데이터 배열</param>
        /// <returns>외기센서 수집 데이터</returns>
        public MdlWeatherData GetWeatherSensorReceiveData(byte[] btData,DateTime dtProcDate, ref DataTable dtWeatherData)
        {

            MdlWeatherData wdWeather = new MdlWeatherData();
            try
            {
                //Get Base Data
                //addr_block
                byte btAddrBlock = btData[0];
                wdWeather.BYTE_ADDRESS_BLOCK = btAddrBlock;
                //func_cod
                byte btFuncCd = btData[1];
                wdWeather.BYTE_FUNC_CODE = btFuncCd;
                //Number of Recieve Bytes
                byte btRecvByte = btData[2];
                wdWeather.BYTE_RECV_BYTE = btRecvByte;

                //장치로부터 입력받은 외기정보 변환
                //장치상태
                Int32 intDevStatus = MngHex.ReadWeatherDataFrom2Byte(btData, 3);
                wdWeather.intDevStatus = intDevStatus;
                //풍향
                Int32 intWindDir = MngHex.ReadWeatherDataFrom2Byte(btData, 5);
                wdWeather.intWindDir = intWindDir;
                //풍속
                double dblWindSpd = (double) MngHex.ReadWeatherDataFrom4Byte(btData, 7);
                wdWeather.dblWindSpeed = dblWindSpd;
                //온도
                double dblAirTmp = (double) MngHex.ReadWeatherDataFrom4Byte(btData, 11);
                wdWeather.dblAirTemp = dblAirTmp;
                //습도
                double dblAirHumi = (double)MngHex.ReadWeatherDataFrom4Byte(btData, 15);
                wdWeather.dblAirHumi = dblAirHumi;
                //기압
                double dblAirPress = (double)MngHex.ReadWeatherDataFrom4Byte(btData, 19);
                wdWeather.dblAirPress = dblAirPress;

                //기상 ( 비 / 눈 / 우박 )
                Int32 intWeather = MngHex.ReadWeatherDataFrom2Byte(btData, 25);
                wdWeather.intWeather = intWeather;
                //강우량
                double dblRainFall = (double) MngHex.ReadWeatherDataFrom4Byte(btData, 27);
                wdWeather.dblRainFall = dblRainFall;
                //시간단위 강우량
                double dblRainFallAcc = (double) MngHex.ReadWeatherDataFrom4Byte(btData, 31);
                wdWeather.dblRainFallAcc = dblRainFallAcc;
                //강우량 단위
                Int32 intRainUnit = MngHex.ReadWeatherDataFrom2Byte(btData, 35);
                wdWeather.intRainUnit = intRainUnit;
                //방사선누적
                double dblRadAcc = (double) MngHex.ReadWeatherDataFrom4Byte(btData, 37);
                wdWeather.dblRadiationAcc = dblRadAcc;
                //방사선
                double dblRad = (double) MngHex.ReadWeatherDataFrom4Byte(btData, 41);
                wdWeather.dblRadiation = dblRad;
                //전체CRC값
                Int32 intChkBlk = MngHex.ReadWeatherDataFrom2Byte(btData, 43);
                wdWeather.intCheckBlock = intChkBlk;

                dtWeatherData.Rows.Add(dtProcDate
                    , btAddrBlock.ToString(), btFuncCd.ToString(), btRecvByte.ToString()
                    , intDevStatus, intWindDir, dblWindSpd
                    , dblAirTmp, dblAirHumi, dblAirPress
                    , intWeather
                    , dblRainFall, dblRainFallAcc, intRainUnit
                    , dblRadAcc, dblRad
                    , intChkBlk
                );

                return wdWeather;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 객실 아웃패킷 데이터 변환
        /// </summary>
        /// <param name="dtRegDate">데이터 취득 일시</param>
        /// <param name="btOutPacket">패킷 바이트 데이터</param>
        /// <param name="mdlRoomInfo">객실 정보</param>
        /// <returns>아웃패킷 데이터 클래스</returns>
        public MdlOutPacketData ReadOutPacket(DateTime dtRegDate,byte[] btOutPacket, ref MdlRoomInfo mdlRoomInfo)
        {
            string strErrorValue = null;
            try
            {
                //ID 취득
                int intID = int.TryParse(btOutPacket[2].ToString("X2"), out intID) ? intID : -1;

                if (mdlRoomInfo == null)
                {
                    Console.Error.WriteLine("not match room info {0} InsertData() : id ", intID);
                    return null;
                }

                //난방 ON /OFF
                // 난방 ON / OFF
                short shtOnOff = Convert.ToInt16(btOutPacket[3]);
                // 설정 온도
                float fltSetTemp = Convert.ToSingle(btOutPacket[4]);

                //실내온도
                float? fltInsideTemp = MngHex.Convert2ByteToFloatData(btOutPacket[5], btOutPacket[6]);

                //공급온도
                float? fltInHeating = MngHex.Convert2ByteToFloatData(btOutPacket[7], btOutPacket[8]);

                // 환수온도
                float? fltOutHeating = MngHex.Convert2ByteToFloatData(btOutPacket[9], btOutPacket[10]);
                // 중계기 센서 온도 ? NowFlow? 유량 순시?
                float? fltNowFlow = MngHex.Convert2ByteToFloatData(btOutPacket[11], btOutPacket[12]);
                // 유량 적산
                float? fltTotalFlow = MngHex.Convert3ByteToUInt32(btOutPacket, 13);
                // 현재 밸브 개도율
                short shtNowControlValue = Convert.ToInt16(btOutPacket[16]);
                // 에러 값
                strErrorValue = MngHex.GetErrorValueFromSingleByte(btOutPacket[17]);

                // deltaTTemp?
                int intDeltaTTemp = Convert.ToInt32(btOutPacket[18]);
                // 현재바닥온도
                int intFloorTemp = Convert.ToInt32(btOutPacket[19]);
                //설정 밸브 개도율
                short shtSetControlValue = shtNowControlValue;

                // 총 온도
                float fltTotalHeat = fltInHeating.Value - fltOutHeating.Value;

                if (strErrorValue != null)
                {
                    svcDB.InsertAlarmRecord(dtRegDate, strErrorValue, mdlRoomInfo);
                }
                
                MdlOutPacketData mdlOutPacket = new MdlOutPacketData();

                mdlOutPacket.mdlRoomInfo = mdlRoomInfo;
                mdlOutPacket.fltInsideTemp = fltInsideTemp.HasValue ? fltInsideTemp.Value : 0;
                mdlOutPacket.fltHeatSetTemp = fltSetTemp;
                mdlOutPacket.fltInHeating = fltInHeating.HasValue ? fltInHeating.Value : 0;
                mdlOutPacket.fltOutHeating = fltOutHeating.HasValue ? fltOutHeating.Value : 0;
                mdlOutPacket.shtNowControlValue = shtNowControlValue;
                mdlOutPacket.shtSetControlValue = shtSetControlValue;
                mdlOutPacket.fltNowFlow = fltNowFlow.HasValue ? fltNowFlow.Value : 0;
                mdlOutPacket.fltTotalFlow = fltTotalFlow.HasValue ? fltTotalFlow.Value : 0;
                mdlOutPacket.shtHeatOnOff = shtOnOff;
                mdlOutPacket.fltTotalHeat = Convert.ToSingle(fltTotalHeat.ToString("F1"));
                mdlOutPacket.intDeltaTTemp = intDeltaTTemp;
                mdlOutPacket.intFloorTemp = intFloorTemp;

                return mdlOutPacket;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// B동 객실용 인패킷데이터 배열 작성
        /// </summary>
        /// <param name="mdlInPacket">패킷데이터 내용 데이터 모델</param>
        /// <returns>전송할 패킷 데이터 바이트 배열</returns>
        public byte[] WriteInPacket(MdlInPacketData mdlInPacket)
        {
            byte[] btInPacketBuffer = new byte[intInPacketLength];

            try
            {
                //STX, 데이터 길이, 객실ID
                btInPacketBuffer[0] = BYTE_INPACKET_STX;
                btInPacketBuffer[1] = Convert.ToByte(INT_INPACKET_DATA_LEN);
                btInPacketBuffer[2] = Convert.ToByte(mdlInPacket.mdlRoomInfo.intID);


                if (mdlInPacket.isReadOnly)
                {
                    btInPacketBuffer[3] = BYTE_INPACKET_READONLY;
                    btInPacketBuffer[4] = BYTE_CTRL_HEAT_OFF;
                    btInPacketBuffer[5] = BYTE_CTRL_SET_TEMP_ZERO;
                    btInPacketBuffer[6] = BYTE_INPACKET_STATIC_RESERVE;
                }
                else
                {
                    switch (mdlInPacket.intHeatOnOff)
                    {
                        case 0: // 쓰기 ON, 난방제어 OFF
                            btInPacketBuffer[3] = BYTE_INPACKET_READWRITE;
                            btInPacketBuffer[4] = BYTE_CTRL_HEAT_OFF;
                            btInPacketBuffer[5] = BYTE_CTRL_SET_TEMP_ZERO;
                            break;
                        case 1: //쓰기 ON, 수동난방
                            btInPacketBuffer[3] = BYTE_INPACKET_READWRITE;
                            btInPacketBuffer[4] = BYTE_CTRL_MANU_HEAT_ON;
                            btInPacketBuffer[5] = Convert.ToByte(mdlInPacket.intSetTemp);
                            break;
                        default : // 쓰기 ON, 자동난방
                            btInPacketBuffer[3] = BYTE_INPACKET_READWRITE;
                            btInPacketBuffer[4] = BYTE_CTRL_AUTO_HEAT_ON;
                            btInPacketBuffer[5] = Convert.ToByte(mdlInPacket.intSetTemp);
                            break;
                    }

                    //온도차
                    btInPacketBuffer[6] = Convert.ToByte(mdlInPacket.intDiffTemp);
                }
                if (mdlInPacket.isTotalReset)
                {
                    btInPacketBuffer[7] = BYTE_FLOWTOTAL_RESET;
                }
                else
                {
                    btInPacketBuffer[7] = BYTE_FLOWTOTAL_NORMAL;
                }

                btInPacketBuffer[8] = BYTE_INPACKET_STATIC_RESERVE;

                AddCheckSum(ref btInPacketBuffer);

                btInPacketBuffer[10] = BYTE_INPACKET_ETX;

                return btInPacketBuffer;
            }
            catch (Exception)
            {
                return new byte[intInPacketLength];
            }
        }

        /// <summary>
        /// 체크썸값 추가
        /// </summary>
        /// <param name="arrInPacketData">B동 인패킷 데이터</param>
        private void AddCheckSum(ref byte[] arrInPacketData)
        {
            byte btCheckSum = 0x00;
            ///ID~Reserve값까지의 데이터로 Checksum 작성
            for (int intCnt = 2; intCnt < 9; intCnt++)
            {
                btCheckSum ^= arrInPacketData[intCnt];
            }
            btCheckSum &= 0x7F;

            arrInPacketData[9] = btCheckSum;
        }
    }
}

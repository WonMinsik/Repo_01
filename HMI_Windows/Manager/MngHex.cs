using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HMI_Windows.Model;

namespace HMI_Windows.Manager
{
    /// <summary>
    /// 헥사값 데이터 전환 매니저
    /// </summary>
    class MngHex
    {
        /// <summary>
        /// 헥사코드를 바이트배열로 변환
        /// </summary>
        /// <param name="arrHex">문자열형 헥사코드</param>
        /// <returns>변환된 바이트배열</returns>
        static public byte[] ConvertHexToByte(string[] arrHex)
        {
            try
            {
                byte[] arrConvert = new byte[arrHex.Length];

                for (int intCount = 0; intCount < arrHex.Length;intCount++)
                {
                    arrConvert[intCount] = Convert.ToByte(arrHex[intCount], 16);
                }

                return arrConvert;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 유량계 통신데이터에서 float형 값 추출
        /// </summary>
        /// <param name="arrByte">유량계통신데이터</param>
        /// <returns>유량계값</returns>
        static public float? ConvertFlowMeterDataToFloat(byte[] arrByte)
        {
            try
            {
                if(arrByte.Length < 6)
                {
                    return (float?) 0f;
                }
                //byte[] arrByteBuffer = { arrByte[5], arrByte[6], arrByte[3], arrByte[4] };
                byte[] arrByteBuffer = { arrByte[3], arrByte[4], arrByte[5], arrByte[6] };
                Array.Reverse(arrByteBuffer);

                return BitConverter.ToSingle(arrByteBuffer, 0);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 2개의 바이트데이터를 지수와 가수로 하나의 float데이터형으로 통합
        /// </summary>
        /// <param name="btDataExp">데이터의 지수부분 바이트 데이터</param>
        /// <param name="btDataRad">데이터의 가수부분 바이트 데이터</param>
        /// <returns></returns>
        static public float? Convert2ByteToFloatData(byte btDataExp, byte btDataRad)
        {
            try
            {
                if ((btDataExp.GetType() != typeof(byte)) || (btDataRad.GetType() != typeof(byte)))
                {
                    return null;
                }

                decimal dcmReturnValue = decimal.Parse(((float)btDataExp + ((float)btDataRad * 0.1f)).ToString("F1"));

                return Convert.ToSingle(dcmReturnValue);
            }
            catch (Exception excp)
            {
                Console.Error.WriteLine(excp);
                throw;
            }
        }

        /// <summary>
        /// 2바이트 빅엔디안 데이터의 UInt16형 데이터 전환
        /// </summary>
        /// <param name="btData">데이터 배열</param>
        /// <param name="intStartWith">변환데이터 시작 위치</param>
        /// <returns></returns>
        static public UInt16? Convert2ByteToUInt16(byte[] btData, int intStartWith)
        {
            try
            {
                if (btData.Length < (intStartWith + 1))
                {
                    return null;
                }

                UInt16 uint16ReturnValue = 0;
                byte[] btDataBuffer = { btData[intStartWith + 1], btData[intStartWith] };
                uint16ReturnValue = BitConverter.ToUInt16(btDataBuffer, 0);
                return uint16ReturnValue;

            }
            catch (Exception excp)
            {
                Console.Error.WriteLine(excp);
                throw;
            }
        }

        /// <summary>
        /// 2바이트 빅엔디안 데이터의 UInt16형 데이터 전환 (485To이더넷)
        /// </summary>
        /// <param name="btData">데이터 배열</param>
        /// <param name="intStartWith">변환데이터 시작 위치</param>
        /// <returns></returns>
        static public Int16? Convert2ByteToInt16_485TE(byte[] btData, int intStartWith)
        {
            try
            {
                if (btData.Length < (intStartWith + 1))
                {
                    return null;
                }

                short int16ReturnValue = 0;
                byte[] btDataBuffer = { btData[intStartWith + 1], btData[intStartWith + 0] };
                int16ReturnValue = BitConverter.ToInt16(btDataBuffer, 0);
                return int16ReturnValue;

            }
            catch (Exception excp)
            {
                Console.Error.WriteLine(excp);
                throw;
            }
        }

        /// <summary>
        /// 3바이트데이터의 UInt32형 데이터 전환
        /// </summary>
        /// <param name="btData">바이트 데이터 배열</param>
        /// <param name="intStartWith">데이터 시작위치</param>
        /// <returns>UInt32형 데이터</returns>
        static public UInt32? Convert3ByteToUInt32(byte[] btData, int intStartWith)
        {
            try
            {
                if (btData.Length < (intStartWith + 2))
                {
                    return null;
                }

                UInt32 intReturnValue = 0;
                byte[] btDataBuffer = { btData[intStartWith + 2], btData[intStartWith + 1], btData[intStartWith], 0x00 };
                intReturnValue = BitConverter.ToUInt32(btDataBuffer, 0);
                return intReturnValue;
            }
            catch (Exception excp)
            {
                Console.Error.WriteLine(excp);
                throw;
            }
        }


        /// <summary>
        /// 4바이트데이터의 Int32형 데이터 전환 (485To이더넷)
        /// </summary>
        /// <param name="btData">바이트 데이터 배열</param>
        /// <param name="intStartWith">데이터 시작위치</param>
        /// <returns>UInt32형 데이터</returns>
        static public Int32? Convert4ByteToInt32_465TE(byte[] btData, int intStartWith)
        {
            try
            {
                if (btData.Length < (intStartWith + 3))
                {
                    return null;
                }

                Int32 intReturnValue = 0;
                byte[] btDataBuffer = { btData[intStartWith + 1], btData[intStartWith + 0], btData[intStartWith + 3], btData[intStartWith + 2] };
                intReturnValue = BitConverter.ToInt32(btDataBuffer, 0);
                return intReturnValue;
            }
            catch (Exception excp)
            {
                Console.Error.WriteLine(excp);
                throw;
            }
        }

        /// <summary>
        /// 4바이트데이터의 Decimal형 데이터 전환 (485To이더넷)
        /// </summary>
        /// <param name="btData">바이트 데이터 배열</param>
        /// <param name="intStartWith">데이터 시작위치</param>
        /// <returns>Decimal형 데이터</returns>
        static public Decimal? Convert4ByteToDecimal_465TE(byte[] btData, int intStartWith)
        {
            try
            {
                if (btData.Length < (intStartWith + 3))
                {
                    return null;
                }

                float fltReturnValue = 0.0f;
                byte[] btDataBuffer = { btData[intStartWith + 1], btData[intStartWith + 0], btData[intStartWith + 3], btData[intStartWith + 2] };
                fltReturnValue = BitConverter.ToSingle(btDataBuffer, 0);

                return Convert.ToDecimal(fltReturnValue.ToString("F2"));
            }
            catch (Exception excp)
            {
                Console.Error.WriteLine(excp);
                throw;
            }
        }

        /// <summary>
        /// 4바이트데이터의 Decimal형 PNC데이터 전환 (485To이더넷)
        /// </summary>
        /// <param name="btData">바이트 데이터 배열</param>
        /// <param name="intStartWith">데이터 시작위치</param>
        /// <returns>Decimal형 데이터</returns>
        static public Decimal? Convert4ByteToDecimal_465TE_PNC(byte[] btData, int intStartWith)
        {
            try
            {
                if (btData.Length < (intStartWith + 3))
                {
                    return null;
                }

                float fltReturnValue = 0.0f;
                byte[] btDataBuffer = { btData[intStartWith + 3], btData[intStartWith + 2], btData[intStartWith + 1], btData[intStartWith + 0] };
                fltReturnValue = BitConverter.ToSingle(btDataBuffer, 0);

                return Convert.ToDecimal(fltReturnValue.ToString("F2"));
            }
            catch (Exception excp)
            {
                Console.Error.WriteLine(excp);
                throw;
            }
        }

        /// <summary>
        /// 2바이트데이터의 Decimal형 데이터 전환 (485To이더넷 수차)
        /// </summary>
        /// <param name="btData">바이트 데이터 배열</param>
        /// <param name="intStartWith">데이터 시작위치</param>
        /// <returns>Decimal형 데이터</returns>
        static public Decimal? Convert2ByteToDecimal_465TE_Wheel(byte[] btData, int intStartWith, float fltScale)
        {
            try
            {
                if (btData.Length < (intStartWith + 1))
                {
                    return null;
                }

                float fltReturnValue = 0.0f;
                byte[] btDataBuffer = { btData[intStartWith + 1], btData[intStartWith + 0]};
                fltReturnValue = (BitConverter.ToUInt16(btDataBuffer, 0) * fltScale);

                return Convert.ToDecimal(fltReturnValue);
            }
            catch (Exception excp)
            {
                Console.Error.WriteLine(excp);
                throw;
            }
        }


        /// <summary>
        /// 4바이트 데이터의 float형 데이터 전환
        /// </summary>
        /// <param name="btData">바이트 데이터 배열</param>
        /// <param name="intStartWith">데이터 시작지점</param>
        /// <returns>float형 데이터</returns>
        static public Single? Convert4ByteToFloat(byte[] btData, int intStartWith)
        {
            try
            {
                if (btData.Length < (intStartWith + 3))
                {
                    return null;
                }

                Single fltReturnValue = 0;
                byte[] btDataBuffer = { btData[intStartWith + 3], btData[intStartWith + 2], btData[intStartWith + 1], btData[intStartWith] };
                fltReturnValue = BitConverter.ToSingle(btDataBuffer, 0);
                
                return fltReturnValue;
            }
            catch (Exception excp)
            {
                Console.Error.WriteLine(excp);
                throw;
            }
        }


        /// <summary>
        /// 2바이트 데이터의 Decimal형 데이터 전환 (485To이더넷 열량계)
        /// </summary>
        /// <param name="btData">바이트 배열</param>
        /// <param name="intStartWith">데이터 시작지점</param>
        /// <returns>float형 데이터</returns>
        static public Decimal? Convert2ByteToDecimal_485TE_Meter(byte[] btData, int intStartWith)
        {
            try
            {
                if (btData.Length < (intStartWith + 1))
                {
                    return null;
                }

                Int16 intRad = Int16.Parse(btData[intStartWith].ToString("X2"));
                Int16 intExp = Int16.Parse(btData[intStartWith + 1].ToString("X2"));

                float fltRad = (float)(intRad * 0.01);
                float fltBUffer = (float)intExp + fltRad;
                Decimal dcmReturnValue = Decimal.Parse(fltBUffer.ToString("F2"));

                return dcmReturnValue;
            }
            catch (Exception excp)
            {
                Console.Error.WriteLine(excp);
                throw;
            }
        }

        /// <summary>
        /// 2바이트 데이터로부터 외기데이터를 취득하여 UInt32형으로 반환
        /// </summary>
        /// <param name="btData">바이트 데이터 배열</param>
        /// <param name="intStartWith">데이터 시작점</param>
        /// <returns>UInt32형 데이터</returns>
        static public Int32 ReadWeatherDataFrom2Byte(byte[] btData, int intStartWith)
        {
            try
            {
                if (btData.Length < (intStartWith + 1))
                {
                    return Int32.MinValue;
                }

                Int32 Int32ReturnValue = 0;
                byte[] btDataBuffer = { btData[intStartWith +1], btData[intStartWith], 0x00, 0x00 };
                bool isParsable = Int32.TryParse((BitConverter.ToInt32(btDataBuffer, 0).ToString()), out Int32ReturnValue);

                if (!isParsable)
                {
                    return Int32.MinValue;
                }
                else
                {
                    return Int32ReturnValue;
                }
            }
            catch (Exception excp)
            {
                Console.Error.WriteLine(excp);
                throw;
            }
        }

        /// <summary>
        /// 4바이트 데이터로부터 외기정보 취득하여
        /// </summary>
        /// <param name="btData">바이트 데이터 배열</param>
        /// <param name="intStartWith">데이터 시작점</param>
        /// <returns>float형 데이터</returns>
        static public Double ReadWeatherDataFrom4Byte(byte[] btData, int intStartWith)
        {
            try
            {
                if (btData.Length < (intStartWith + 3))
                {
                    return Double.MinValue;
                }

                byte[] btDataBuffer = { btData[intStartWith + 1], btData[intStartWith + 0], btData[intStartWith + 3], btData[intStartWith + 2] };

                return (Double) BitConverter.ToSingle(btDataBuffer, 0);
            }
            catch (Exception excp)
            {
                Console.Error.WriteLine(excp);
                throw;
            }
        }

        /// <summary>
        /// 에러검출바이트로부터 에러값을 읽어낸다.
        /// </summary>
        /// <param name="btData">에러검출바이트 데이터</param>
        /// <returns>에러 발생 문장</returns>
        static public string GetErrorValueFromSingleByte(byte btData)
        {
            string strErrStr = null;

            try
            {
                //에러검출바이트데이터를 2바이트 배열로 전환
                byte[] btDataBuffer = { 0x00, btData };
                
                //전환된 배열을 short형 데이터로 전환
                //short shtData = BitConverter.ToInt16(btDataBuffer, 0);
                
                bool isParsable = short.TryParse(btData.ToString("X2"), out short shtErrorNo);

                if (!isParsable) {
                    return null;
                }

                //short형 데이터로부터 에러 발생유형 및 로그 데이터 취득
                switch (shtErrorNo)
                {
                    case 1: //공급온도 센서 에러
                        strErrStr = "InHeating sensor error";
                        break;

                    case 2: //환수온도 센서 에러
                        strErrStr = "OutHeating sensor error";
                        break;

                    case 3:// 공급/환수온도 센서 에러
                        strErrStr = "InHeating/OutHeating sensor error";
                        break;

                    case 4:// 
                        strErrStr = "actuator error";
                        break;

                    case 5:
                        strErrStr = "InHeating sensor error and actuator error";
                        break;

                    case 6:
                        strErrStr = "OutHeating sensor error and actuator error";
                        break;

                    case 7:
                        strErrStr = "InHeating/OutHeating sensor error and actuator error";
                        break;

                    default:
                        strErrStr = null;
                        break;
                }

                //검출덴 에러 발생문을 AlarmRecord테이블에 삽입하기위해 반환

                return strErrStr;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

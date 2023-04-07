using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using System.Collections;
using System.Net.Sockets;

namespace HMI_Windows.Manager
{
    /// <summary>
    /// 유량계 시리얼통신 클래스
    /// </summary>
    class MngMR_Meter
    {
        /// <summary> 유량계 항목명 </summary>
        public const string STR_FLOW_METER_TABLE_NAME = "FlowMeter";
        /// <summary> 기계실 유량계 연결 Serial - TCP 컨버터 IP주소 </summary>
        private string STR_FLOW_METER_ADDR = "192.168.0.40";
        /// <summary> 기계실 유량계 연결 Serial - TCP 컨버터 포트 </summary>
        private string STR_FLOW_METER_PORT = "9999";
        /// <summary>  타임아웃 설정 </summary>
        private int INT_TIME_OUT_MILISECOND = 5000;

        private int INT_SCALE_SEC_TO_HOUR = 3600;

        private TcpClient tcpcli = null;

        private NetworkStream nsStream = null;

        /// <summary>
        /// 기계실유량계 데이터 취득
        /// </summary>
        /// <returns>기계실유량계 1~4번 취득 데이터</returns>
        public DataTable GetDataFromFlowMeter() 
        {
            DataTable dtFlowMeter = new DataTable();

            dtFlowMeter.Columns.Add("FlowMeter_1", typeof(double));
            dtFlowMeter.Columns.Add("FlowMeter_2", typeof(double));
            dtFlowMeter.Columns.Add("FlowMeter_3", typeof(double));
            dtFlowMeter.Columns.Add("FlowMeter_4", typeof(double));

            dtFlowMeter.TableName = STR_FLOW_METER_TABLE_NAME;
            try
            {
                this.tcpcli = new TcpClient(STR_FLOW_METER_ADDR, Convert.ToInt32(STR_FLOW_METER_PORT));

                this.tcpcli.ReceiveTimeout = INT_TIME_OUT_MILISECOND;
                this.tcpcli.SendTimeout = INT_TIME_OUT_MILISECOND;

                this.nsStream = this.tcpcli.GetStream();

                ArrayList lstSendBytes = new ArrayList();

                //유량계에 보낼 헥사 데이터의 문자열 배열
                //유량계 1번
                string[] arrSendDataFlowMeter_1 = { "01", "03", "00", "ED", "00", "02", "54", "3E" };
                //유량계 2번
                string[] arrSendDataFlowMeter_2 = { "02", "03", "00", "ED", "00", "02", "54", "0D" };
                //유량계 3번
                string[] arrSendDataFlowMeter_3 = { "03", "03", "00", "ED", "00", "02", "55", "DC" };
                //유량계 4번
                string[] arrSendDataFlowMeter_4 = { "04", "03", "00", "ED", "00", "02", "54", "6B" };

                //유량계에 보낼 헥사데이터의 바이트 배열 묶음 생성
                //1번 유량계
                byte[] arrSendData_1 = Manager.MngHex.ConvertHexToByte(arrSendDataFlowMeter_1);
                byte[] arrReceivData_1 = new byte[9];
                SendReceiveFlowMeter(ref arrSendData_1, out arrReceivData_1);

                //2번 유량계
                byte[] arrSendData_2 = Manager.MngHex.ConvertHexToByte(arrSendDataFlowMeter_2);
                byte[] arrReceivData_2 = new byte[9];
                SendReceiveFlowMeter(ref arrSendData_2, out arrReceivData_2);


                //3번 유량계
                byte[] arrSendData_3 = Manager.MngHex.ConvertHexToByte(arrSendDataFlowMeter_3);
                byte[] arrReceivData_3 = new byte[9];
                SendReceiveFlowMeter(ref arrSendData_3, out arrReceivData_3);

                //4번 유량계
                byte[] arrSendData_4 = Manager.MngHex.ConvertHexToByte(arrSendDataFlowMeter_4);
                byte[] arrReceivData_4 = new byte[9];
                SendReceiveFlowMeter(ref arrSendData_4, out arrReceivData_4);

                //수신하는 값이 m^3/s, 표기데이터는 m^3/h이므로 변환
                float fltMtrValue1 = Manager.MngHex.ConvertFlowMeterDataToFloat(arrReceivData_1).Value * INT_SCALE_SEC_TO_HOUR;
                float fltMtrValue2 = Manager.MngHex.ConvertFlowMeterDataToFloat(arrReceivData_2).Value * INT_SCALE_SEC_TO_HOUR;
                float fltMtrValue3 = Manager.MngHex.ConvertFlowMeterDataToFloat(arrReceivData_3).Value * INT_SCALE_SEC_TO_HOUR;
                float fltMtrValue4 = Manager.MngHex.ConvertFlowMeterDataToFloat(arrReceivData_4).Value * INT_SCALE_SEC_TO_HOUR;

                decimal dcmFlowMeter_1 = decimal.Parse(fltMtrValue1.ToString("F4"));
                decimal dcmFlowMeter_2 = decimal.Parse(fltMtrValue2.ToString("F4"));
                decimal dcmFlowMeter_3 = decimal.Parse(fltMtrValue3.ToString("F4"));
                decimal dcmFlowMeter_4 = decimal.Parse(fltMtrValue4.ToString("F4"));

                dtFlowMeter.Rows.Add(Convert.ToSingle(dcmFlowMeter_1.ToString("F4"))
                    , Convert.ToSingle(dcmFlowMeter_2.ToString("F4"))
                    , Convert.ToSingle(dcmFlowMeter_3.ToString("F4"))
                    , Convert.ToSingle(dcmFlowMeter_4.ToString("F4"))
                );

                //통신 종료
                return dtFlowMeter;
            }
            catch (Exception)
            {
                dtFlowMeter.Rows.Add(
                    0, 0, 0, 0
                );

                return dtFlowMeter;
            }
            finally
            {
                if (this.nsStream != null)
                {
                    this.nsStream.Dispose();
                }

                if (this.tcpcli != null)
                {
                    this.tcpcli.Dispose();
                }

            }

        }

        /// <summary>
        /// 유량계 통신
        /// </summary>
        /// <param name="arrSendData"></param>
        /// <param name="arrRecvData"></param>
        private void SendReceiveFlowMeter(ref byte[] arrSendData, out byte[] arrRecvData)
        {
            arrRecvData = new byte[9];
            byte[] arrRecvBuffer = new byte[arrRecvData.Length + 5];
            int intTryCount = 0;
            int intOffSet = 0;
            bool isSuccess = false;
            try
            {
                do
                {
                    intOffSet = 0;
                    intTryCount++;

                    //Send Data
                    nsStream.Write(arrSendData, 0, arrSendData.Length);
                    System.Threading.Thread.Sleep(50);

                    DateTime dtRecvStartTime = DateTime.Now;

                    TimeSpan tsRecv = new TimeSpan();

                    do
                    {

                        if (nsStream.CanRead)
                        {
                            intOffSet += nsStream.Read(arrRecvBuffer, intOffSet, 4);
                        }

                        tsRecv = DateTime.Now - dtRecvStartTime;

                    } while (nsStream.CanRead && intOffSet < arrRecvData.Length && tsRecv.TotalMilliseconds < INT_TIME_OUT_MILISECOND);

                    if (arrSendData[0] == arrRecvBuffer[0] && arrSendData[1] == arrRecvBuffer[1])
                    {
                        isSuccess = true;
                        Array.Copy(arrRecvBuffer, arrRecvData, arrRecvData.Length);
                    }

                } while (isSuccess && intTryCount < 3);

            }
            catch (Exception)
            {
                return;
            }
        }
    }
}

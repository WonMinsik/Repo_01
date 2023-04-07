using System;
using System.Data;

using HMI_Windows.NI;
using HMI_Windows.Service;
using HMI_Windows.Manager;

namespace HMI_Windows.Connect
{
    /// <summary>
    /// 기계실 센서 정보 취득 관리 클래스
    /// </summary>
    class CntMR
    {
        /// <summary> 처리동작중 플래그 </summary>
        public bool isRunning { get; set; }

        /// <summary> 발생한 알림 </summary>
        public string strMessage { get; set; }

        /// <summary> 기계실 통신데이터 로그 테이블 </summary>
        public DataSet dsMRLogData { get;}

        /// <summary> 숙소동 온도 테이블명 </summary>
        public const string STR_TABLE_NAME_MR_ACC_TMP = "McRmAccTemp";

        /// <summary> 숙소동 온도 데이터 </summary>
        private DataTable dtAccTmpData { get; set; }

        /// <summary> 숙소동 압력 테이블명 </summary>
        public const string STR_TABLE_NAME_MR_ACC_PRS = "McRmAccPress";

        /// <summary> 숙소동 압력 데이터 </summary>
        private DataTable dtAccPrsData { get; set; }

        /// <summary> 숙소동 유량 테이블명 </summary>
        public const string STR_TABLE_NAME_MR_ACC_FLW = "McRmAccFlow";

        /// <summary> 숙소동 유량 데이터 </summary>
        private DataTable dtAccFlwData { get; set; }

        /// <summary> 숙소동 밸브 테이블명 </summary>
        public const string STR_TABLE_NAME_MR_ACC_VLV = "McRmAccValve";

        /// <summary> 숙소동 밸브 데이터 </summary>
        private DataTable dtAccVlvData { get; set; }

        /// <summary> 연구동 온도 테이블명 </summary>
        public const string STR_TABLE_NAME_MR_LAB_TMP = "McRmLabTemp";

        /// <summary> 연구동 DB입력용 데이터 </summary>
        private DataTable dtLabTmpData { get; set; }

        /// <summary> 연구동 압력 및 유량 테이블명 </summary>
        public const string STR_TABLE_NAME_MR_LAB_PRS_FLW = "McRmLabPressAndFlow";
        /// <summary> 연구동 압력 및 유량 </summary>
        private DataTable dtLabPrsFlwData { get; set; }

        /// <summary> 스케일값 1000 </summary>
        private int INT_SCALE_1000 = 1000;

        /// <summary>
        /// 밸브제어 플래그 : 미제어
        /// </summary>
        private string STR_COMMAND_FLAG_NOT_CONTROLED = "1";
        ///// <summary>
        ///// 밸브제어 플래그 : 제어
        ///// </summary>
        //private string STR_COMMAND_FLAG_HAS_CONTROLED = "0";

        /// <summary>
        /// CntMR클래스 생성자
        /// </summary>
        public CntMR() {
            this.isRunning = false;
            this.strMessage = string.Empty;

            this.dsMRLogData = new DataSet();

            this.dtLabTmpData = Manager.Mng_DataTable.GetDataTableTemplate_MR_Lab_Tmp();
            this.dtLabPrsFlwData = Manager.Mng_DataTable.GetDataTableTemplate_MR_Lab_Prs_Flw();

            this.dtAccTmpData = Manager.Mng_DataTable.GetDataTableTemplate_MR_Acc_Tmp();
            this.dtAccPrsData = Manager.Mng_DataTable.GetDataTableTemplate_MR_Acc_Prs();
            this.dtAccFlwData = Manager.Mng_DataTable.GetDataTableTemplate_MR_Acc_Flw();
            this.dtAccVlvData = Manager.Mng_DataTable.GetDataTableTemplate_MR_Acc_Vlv();
        }

        /// <summary>
        /// 연구동 데이터 수신
        /// </summary>
        /// <param name="dtRegTime">데이터 수신 일시</param>
        private void GetDataFromLaboratory(DateTime dtRegTime) 
        {
            try
            {
                DataTable dtReturnValue_9208_Lab = new DataTable();
                DataTable dtReturnValue_9216_Lab = new DataTable();

                dtReturnValue_9208_Lab.Clear();
                dtReturnValue_9216_Lab.Clear();


                try
                {
                    cDAQ9208 ni9208 = new cDAQ9208(cDAQ9208.STR_NI9208_RESEARCH_IDENTIFIER);
                    dtReturnValue_9208_Lab = ni9208.GetAmpDataFromNI9208();
                }
                catch (Exception)
                {
                    this.strMessage += "연구동 " + cDAQ9208.STR_NI9208_RESEARCH_IDENTIFIER + " 접속 에러 \r\n";
                }
                dtReturnValue_9208_Lab.TableName = cDAQ9208.STR_NI9208_RESEARCH_IDENTIFIER;


                try
                {
                    cDAQ9216 ni9216 = new cDAQ9216(cDAQ9216.STR_NI9216_RESEARCH_IDENTIFIER);
                    dtReturnValue_9216_Lab = ni9216.GetRTDTempFromNI9216();
                }
                catch (Exception)
                {
                    this.strMessage += "연구동 " + cDAQ9216.STR_NI9216_RESEARCH_IDENTIFIER + " 접속 에러 \r\n";
                }
                dtReturnValue_9216_Lab.TableName = cDAQ9216.STR_NI9216_RESEARCH_IDENTIFIER;
                


                //HMI DB laboratory 테이블에 맞춰 데이터 편집
                this.dtLabTmpData.Rows.Add(dtRegTime,
                    dtReturnValue_9216_Lab.Rows[0].ItemArray[0],
                    dtReturnValue_9216_Lab.Rows[0].ItemArray[1],
                    dtReturnValue_9216_Lab.Rows[0].ItemArray[2],
                    dtReturnValue_9216_Lab.Rows[0].ItemArray[3],
                    dtReturnValue_9216_Lab.Rows[0].ItemArray[4],
                    dtReturnValue_9216_Lab.Rows[0].ItemArray[5],
                    dtReturnValue_9216_Lab.Rows[0].ItemArray[6],
                    dtReturnValue_9216_Lab.Rows[0].ItemArray[7]
                );

                this.dtLabPrsFlwData.Rows.Add(dtRegTime, 
                    Convert.ToSingle(dtReturnValue_9208_Lab.Rows[0].ItemArray[0]) * INT_SCALE_1000,
                    Convert.ToSingle(dtReturnValue_9208_Lab.Rows[0].ItemArray[1]) * INT_SCALE_1000,
                    Convert.ToSingle(dtReturnValue_9208_Lab.Rows[0].ItemArray[2]) * INT_SCALE_1000,
                    Convert.ToSingle(dtReturnValue_9208_Lab.Rows[0].ItemArray[3]) * INT_SCALE_1000,
                    Convert.ToSingle(dtReturnValue_9208_Lab.Rows[0].ItemArray[4]) * INT_SCALE_1000,
                    Convert.ToSingle(dtReturnValue_9208_Lab.Rows[0].ItemArray[5]) * INT_SCALE_1000,
                    Convert.ToSingle(dtReturnValue_9208_Lab.Rows[0].ItemArray[6]) * INT_SCALE_1000,
                    Convert.ToSingle(dtReturnValue_9208_Lab.Rows[0].ItemArray[7]) * INT_SCALE_1000
                );

                SvcDB mngDB = new SvcDB();
                mngDB.InsertMachineRoomData_Laboratory(dtRegTime, this.dtLabTmpData, this.dtLabPrsFlwData);

                //각 디바이스별 테이블을 로그 정보에 추가
                this.dsMRLogData.Tables.Add(this.dtLabTmpData);
                this.dsMRLogData.Tables.Add(this.dtLabPrsFlwData);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 숙소동 NI9266 (밸브 제어 장치) 제어
        /// </summary>
        /// <param name="dtRegTime">제어 일시</param>
        /// <returns>제어 결과</returns>
        private void SetValveValueToAccommodation(DateTime dtRegTime) 
        {
            try
            {
                SvcDB svcDB = new SvcDB();

                DataTable dtControlValue = new DataTable();
                try
                {
                    //NI9266 카드 쓰기 및 레코드 업데이트
                    dtControlValue = svcDB.GetValveControl();
                }
                catch (Exception)
                {
                    this.strMessage += "숙소동 " + cDAQ9266.STR_NI9266_IDENTIFIER + " 제어명령 취득 에러\r\n";
                    return;
                }


                if (dtControlValue.Rows.Count > 0)
                {
                    cDAQ9266 ni9266 = new cDAQ9266(cDAQ9266.STR_NI9266_IDENTIFIER);

                    foreach (DataRow dr in dtControlValue.Rows)
                    {
                        string strValve_name = dr.ItemArray[0].ToString();
                        string strValue = dr.ItemArray[1].ToString();
                        string strCommand_flag = dr.ItemArray[2].ToString();

                        if (strCommand_flag.Equals(STR_COMMAND_FLAG_NOT_CONTROLED))
                        {
                            bool isValveControlOk = false;
                            bool isUpdateOk = false;
                            bool isInsertOk = false;
                            string strErrMsg = string.Empty;
                            try
                            {
                                isValveControlOk = ni9266.WriteValveValue(strValve_name, strValue, ref strErrMsg);
                                System.Threading.Thread.Sleep(50);
                                if (isValveControlOk)
                                {
                                    isUpdateOk = svcDB.UpdateValveControl_Flag(strValve_name);
                                    System.Threading.Thread.Sleep(50);
                                    isInsertOk = svcDB.InsertValveControlRecord(dtRegTime, strValve_name, strValue);
                                    System.Threading.Thread.Sleep(50);
                                }
                                else
                                {
                                    if(!strMessage.Contains(strErrMsg))
                                        this.strMessage += strErrMsg + "\r\n";
                                }
                            }
                            catch (Exception)
                            {
                                if (!isValveControlOk)
                                {
                                    this.strMessage += "밸브 " + strValve_name + "접속 에러\r\n";
                                }
                                else if (isValveControlOk && !(isUpdateOk && isInsertOk))
                                {
                                    this.strMessage += "밸브 " + strValve_name + " SQL 에러\r\n";
                                }

                            }
                        }
                    }
                }

                return;
            }
            catch (Exception)
            {
                this.strMessage += "숙소동 NI9266 제어 에러 \r\n";
                return;
            }
        }


        /// <summary>
        /// 숙소동 데이터 수신
        /// </summary>
        /// <param name="dtRegTime">수신일시</param>
        private void GetDataFromAccommodation(DateTime dtRegTime)
        {
            try
            {
                SvcDB svcDB = new SvcDB();

                DataTable dtReturnValue_9208_Accommon = new DataTable();
                DataTable dtReturnValue_9216_Accommon = new DataTable();
                DataTable dtReturnValue_9217_Accommon = new DataTable();

                DataTable dtFlowMeter = new DataTable();

                try
                {
                    MngMR_Meter svcSerialTCP = new MngMR_Meter();

                    dtFlowMeter = svcSerialTCP.GetDataFromFlowMeter();
                }
                catch (Exception)
                {
                    this.strMessage += "숙소동 유량계 접속 에러\r\n";
                }
                
                try
                {
                    cDAQ9208 ni9208 = new cDAQ9208(cDAQ9208.STR_NI9208_RESIDENCE_IDENTIFIER);
                    dtReturnValue_9208_Accommon = ni9208.GetAmpDataFromNI9208();
                }
                catch (Exception)
                {
                    this.strMessage += "숙소동 " + cDAQ9208.STR_NI9208_RESIDENCE_IDENTIFIER + " 접속 에러\r\n";
                }
                dtReturnValue_9208_Accommon.TableName = cDAQ9208.STR_NI9208_RESIDENCE_IDENTIFIER;


                
                try
                {
                    cDAQ9216 ni9216 = new cDAQ9216(cDAQ9216.STR_NI9216_RESIDENCE_IDENTIFIER);
                    dtReturnValue_9216_Accommon = ni9216.GetRTDTempFromNI9216();
                }
                catch (Exception)
                {
                    this.strMessage += "숙소동 " + cDAQ9216.STR_NI9216_RESIDENCE_IDENTIFIER + " 접속 에러\r\n";
                }
                dtReturnValue_9216_Accommon.TableName = cDAQ9216.STR_NI9216_RESIDENCE_IDENTIFIER;

                
                try
                {
                    cDAQ9217 ni9217 = new cDAQ9217(cDAQ9217.STR_NI9217_IDENTIFIER);
                    dtReturnValue_9217_Accommon = ni9217.GetRTDTempFromNI9217();  
                }
                catch (Exception)
                {
                    this.strMessage += "숙소동 " + cDAQ9217.STR_NI9217_IDENTIFIER + " 접속 에러\r\n";
                }
                dtReturnValue_9217_Accommon.TableName = cDAQ9217.STR_NI9217_IDENTIFIER;


                //HMI DB accommandation 테이블에 맞춰 데이터 편집
                //온도
                this.dtAccTmpData.Rows.Add(dtRegTime
                    , dtReturnValue_9216_Accommon.Rows[0].ItemArray[0]
                    , dtReturnValue_9216_Accommon.Rows[0].ItemArray[1]
                    , dtReturnValue_9216_Accommon.Rows[0].ItemArray[2]
                    , dtReturnValue_9216_Accommon.Rows[0].ItemArray[3]
                    , dtReturnValue_9216_Accommon.Rows[0].ItemArray[4]
                    , dtReturnValue_9216_Accommon.Rows[0].ItemArray[5]
                    , dtReturnValue_9216_Accommon.Rows[0].ItemArray[6]
                    , dtReturnValue_9216_Accommon.Rows[0].ItemArray[7]
                    , dtReturnValue_9217_Accommon.Rows[0].ItemArray[0]
                    , dtReturnValue_9217_Accommon.Rows[0].ItemArray[1]
                    , dtReturnValue_9217_Accommon.Rows[0].ItemArray[2]
                );

                //압력
                this.dtAccPrsData.Rows.Add(dtRegTime
                    , Convert.ToSingle(dtReturnValue_9208_Accommon.Rows[0].ItemArray[0]) * INT_SCALE_1000
                    , Convert.ToSingle(dtReturnValue_9208_Accommon.Rows[0].ItemArray[1]) * INT_SCALE_1000
                    , Convert.ToSingle(dtReturnValue_9208_Accommon.Rows[0].ItemArray[2]) * INT_SCALE_1000
                    , Convert.ToSingle(dtReturnValue_9208_Accommon.Rows[0].ItemArray[3]) * INT_SCALE_1000
                    , Convert.ToSingle(dtReturnValue_9208_Accommon.Rows[0].ItemArray[4]) * INT_SCALE_1000
                    , Convert.ToSingle(dtReturnValue_9208_Accommon.Rows[0].ItemArray[5]) * INT_SCALE_1000
                    , Convert.ToSingle(dtReturnValue_9208_Accommon.Rows[0].ItemArray[6]) * INT_SCALE_1000
                    , Convert.ToSingle(dtReturnValue_9208_Accommon.Rows[0].ItemArray[7]) * INT_SCALE_1000
                    , Convert.ToSingle(dtReturnValue_9208_Accommon.Rows[0].ItemArray[8]) * INT_SCALE_1000
                );

                //유량
                this.dtAccFlwData.Rows.Add(dtRegTime,
                    dtFlowMeter.Rows[0].ItemArray[0]
                    , dtFlowMeter.Rows[0].ItemArray[1]
                    , dtFlowMeter.Rows[0].ItemArray[2]
                    , dtFlowMeter.Rows[0].ItemArray[3]
                );

                //밸브
                this.dtAccVlvData.Rows.Add(dtRegTime,
                    Convert.ToSingle(dtReturnValue_9208_Accommon.Rows[0].ItemArray[9]) * INT_SCALE_1000
                    , Convert.ToSingle(dtReturnValue_9208_Accommon.Rows[0].ItemArray[10]) *INT_SCALE_1000
                    , Convert.ToSingle(dtReturnValue_9208_Accommon.Rows[0].ItemArray[11]) *INT_SCALE_1000
                    , Convert.ToSingle(dtReturnValue_9208_Accommon.Rows[0].ItemArray[12]) *INT_SCALE_1000
                );


                //Accommodation 테이블에 Data추가
                svcDB.InsertMachineRoomData_Accommodation(dtRegTime, this.dtAccTmpData, this.dtAccPrsData, this.dtAccFlwData, this.dtAccVlvData);

                //각 테이블을 로그에 저장
                this.dsMRLogData.Tables.Add(this.dtAccTmpData);
                this.dsMRLogData.Tables.Add(this.dtAccPrsData);
                this.dsMRLogData.Tables.Add(this.dtAccFlwData);
                this.dsMRLogData.Tables.Add(this.dtAccVlvData);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 기계실 계측제어 프로세스
        /// </summary>
        public void MachineRoomProcess()
        {
            this.isRunning = true;
            try
            {
                DateTime dtNowTime = DateTime.Now; 
                DateTime dtProcStartTime = new DateTime(dtNowTime.Year, dtNowTime.Month, dtNowTime.Day, dtNowTime.Hour, dtNowTime.Minute, 0);

                this.strMessage = "";
                this.dsMRLogData.Reset();
                this.dsMRLogData.Clear();

                this.dtLabTmpData.Clear();
                this.dtLabPrsFlwData.Clear();

                this.dtAccTmpData.Clear();
                this.dtAccPrsData.Clear();
                this.dtAccFlwData.Clear();
                this.dtAccVlvData.Clear();

                SetValveValueToAccommodation(dtNowTime);

                DateTime dtSetValve = DateTime.Now;

                this.GetDataFromLaboratory(dtProcStartTime);

                System.Threading.Thread.Sleep(1000);

                this.GetDataFromAccommodation(dtProcStartTime);

                TimeSpan tsProcTIme = DateTime.Now - dtNowTime;

                //밸브제어를 3초마다 실행 시킬 경우

                TimeSpan tsSpan = DateTime.Now - dtNowTime;

                while (tsSpan.TotalSeconds % 60 < (Frm_HMI_Main.intThreadMachinRoomInterval - Frm_HMI_Main.intThreadCheckInterval*2))
                {
                    SetValveValueToAccommodation(DateTime.Now);

                    System.Threading.Thread.Sleep(Frm_HMI_Main.intThreadCheckInterval * 1000);

                    tsSpan = DateTime.Now - dtNowTime;

                }

                this.strMessage += "MR Thread: " + dtProcStartTime.ToLongTimeString() + " / " + tsProcTIme.TotalSeconds.ToString("F2") + "/ " + tsSpan.TotalSeconds.ToString("F2") + " Sec";
                this.isRunning = false;
            }
            catch (Exception e)
            {
                this.strMessage += e.Message + "\r\n";
                this.isRunning = false;
            }
        }
    }
}

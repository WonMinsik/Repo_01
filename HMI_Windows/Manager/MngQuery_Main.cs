using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMI_Windows.Manager
{
    class MngQuery_Main
    {
		/// <summary> 테이블명 날짜 연도 서식 </summary>
		public const string STR_DATE_FORMAT_TABLE_NAME_YEAR = "yyyy";
		/// <summary>  테이블명 날짜 월 서식  </summary>
		public const string STR_DATE_FORMAT_TABLE_NAME_MONTH = "MM";
		/// <summary> 데이터 날짜 서식 </summary>
		public const string STR_DATE_FORMAT_DATA_ROW = "yyyy-MM-dd HH:mm:ss";
		/// <summary>  </summary>
		public const string STR_SEPARATOR = ", ";

		/// <summary>
		/// LookServer선택박스 값 취득 쿼리문 취득
		/// </summary>
		/// <returns>쿼리문</returns>
		public static string GetListBoxValueQuery_LOOKSERVER()
		{ 
			string strQuery = "SELECT Dong AS DONG, FLOOR AS FLOOR, Ho AS ROOM, ip_addr AS IPADDR FROM lookserver WHERE Dong = 'B' AND room_type = 0 ORDER BY ROOM ASC;";

			return strQuery;
		}

		/// <summary>
		/// LookServer_iot 선택박스 값 취득 쿼리문 취득
		/// </summary>
		/// <returns>쿼리문</returns>
		public static string GetListBoxValueQuery_LOOKSERVER_IOT(string strDong)
		{
			string strQuery = "SELECT Dong AS DONG, Ho AS ROOM, ip_addr AS IPADDR, multi_sensor_no AS SENSOR_NO FROM lookserver_iot	WHERE Dong = \'"
				+ strDong.ToUpper()
				+ "\' ORDER BY ROOM, SENSOR_NO asc;";

			return strQuery;
		}
	}
}

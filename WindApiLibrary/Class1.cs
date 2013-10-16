using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace WindApiLibrary
{
    public delegate int IEventHandler(WQEvent quantdata, IntPtr parameter);

    [StructLayout(LayoutKind.Sequential , CharSet=CharSet.Unicode)]
    public struct WQAUTH_INFO
    {
        public  int bSilentLogin; ///@param  是否显示登陆对话框，0：显示 1：不显示
	    //TCHAR strUserName[SMALLSTRING_SIZE]; ///登录用户名,当bSilentLogin=true时有效
        [MarshalAs(UnmanagedType.ByValTStr,  SizeConst = 64)]
        public string strUserName;
	    //TCHAR strPassword[SMALLSTRING_SIZE];///登录密码,当bSilentLogin=true时有效
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string strPassword;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public class WQEvent
    {
	public int Version;							// 版本号，以备今后扩充
	public eWQEventType EventType;              // Event类型
	public eWQErr ErrCode;					    // 错误码
	public Int64 RequestID;					    // 对应的request
    public Int64 EventID;						// 流水号
	public IntPtr pQuantData;      			    // 包含的数据
    } 

    public enum eWQErr
		:int
	{
		// 操作成功
		OK = 0
		,

		BASE = -40520000
		,

		// 一般性错误
		GENERAL_CLASS = BASE
		,

		// 未知错误 
		UNKNOWN = GENERAL_CLASS - 1
		,

		// 内部错误
		INTERNAL_ERROR = GENERAL_CLASS - 2
		,

		// 操作系统原因
		SYSTEM_REASON = GENERAL_CLASS - 3
		,

		// 登陆失败
		LOGON_FAILED = GENERAL_CLASS - 4
		,

		// 无登陆权限
		LOGON_NOAUTH = GENERAL_CLASS - 5
		,

		// 用户取消
		USER_CANCEL = GENERAL_CLASS - 6
		,

		// 没有可用数据
		NO_DATA_AVALIABLE = GENERAL_CLASS - 7
		,

		// 请求超时
		TIMEOUT = GENERAL_CLASS - 8
		,

		// Wbox错误
		LOST_WBOX = GENERAL_CLASS - 9
		,

		// 未找到相关内容
		ITEM_NOT_FOUND = GENERAL_CLASS - 10
		,

		// 未找到相关服务
		SERVICE_NOT_FOUND = GENERAL_CLASS - 11
		,

		// 未找到相关ID
		ID_NOT_FOUND = GENERAL_CLASS - 12
		,

		// 已在本机使用其他账号登录万得其他产品，故无法使用指定账号登录
		LOGON_CONFLICT = GENERAL_CLASS - 13
		,

		// 未登录使用WIM工具，故无法登录
		LOGON_NO_WIM = GENERAL_CLASS - 14
		,

		// 连续登录失败次数过多
		TOO_MANY_LOGON_FAILURE = GENERAL_CLASS - 15
		,

		// 网络数据存取错误
		IOERROR_CLASS = BASE - 1000
		,
		// IO操作错误
		IO_ERROR = IOERROR_CLASS - 1
		,

		// 后台服务器不可用
		SERVICE_NOT_AVAL = IOERROR_CLASS - 2
		,

		// 网络连接失败
		CONNECT_FAILED = IOERROR_CLASS - 3
		,

		// 请求发送失败
		SEND_FAILED = IOERROR_CLASS - 4
		,

		// 数据接收失败
		RECEIVE_FAILED = IOERROR_CLASS - 5
		,

		// 网络错误
		NETWORK_ERROR = IOERROR_CLASS - 6
		,

		// 服务器拒绝请求
		SERVER_REFUSED = IOERROR_CLASS - 7
		,

		// 错误的应答
		SVR_BAD_RESPONSE = IOERROR_CLASS - 8
		,

		// 数据解码失败
		DECODE_FAILED = IOERROR_CLASS - 9
		,

		// 网络超时
		INTERNET_TIMEOUT = IOERROR_CLASS - 10
		,

		// 频繁访问
		ACCESS_FREQUENTLY = IOERROR_CLASS - 11
		,


		// 请求输入错误
		INVALID_CLASS = BASE - 2000
		,
		// 无合法会话
		ILLEGAL_SESSION = INVALID_CLASS - 1
		,

		// 非法数据服务
		ILLEGAL_SERVICE = INVALID_CLASS - 2
		,

		// 非法请求
		ILLEGAL_REQUEST = INVALID_CLASS - 3
		,

		// 万得代码语法错误
		WINDCODE_SYNTAX_ERR = INVALID_CLASS - 4
		,

		// 不支持的万得代码
		ILLEGAL_WINDCODE = INVALID_CLASS - 5
		,

		// 指标语法错误
		INDICATOR_SYNTAX_ERR = INVALID_CLASS - 6
		,

		// 不支持的指标
		ILLEGAL_INDICATOR = INVALID_CLASS - 7
		,

		// 指标参数语法错误
		OPTION_SYNTAX_ERR = INVALID_CLASS - 8
		,

		// 不支持的指标参数
		ILLEGAL_OPTION = INVALID_CLASS - 9
		,

		// 日期与时间语法错误
		DATE_TIME_SYNTAX_ERR = INVALID_CLASS - 10
		,

		// 不支持的日期与时间
		INVALID_DATE_TIME = INVALID_CLASS - 11
		,

		// 不支持的请求参数
		ILLEGAL_ARG = INVALID_CLASS - 12
		,

		// 数组下标越界
		INDEX_OUT_OF_RANGE = INVALID_CLASS - 13
		,

		// 重复的int
		DUPLICATE_WQID = INVALID_CLASS - 14
		,

		// 请求无相应权限
		UNSUPPORTED_NOAUTH = INVALID_CLASS - 15
		,

		// 不支持的数据类型
		UNSUPPORTED_DATA_TYPE = INVALID_CLASS - 16
		,

		// 数据提取量超限
		DATA_QUOTA_EXCEED = INVALID_CLASS - 17
		,
	}
    public enum eWQEventType
    {
        eWQLogin = 1,                       // 登录相关信息
        eWQResponse,                        // 数据返回
        eWQPartialResponse,                 // 部分数据返回（订阅时适用）
        eWQErrorReport,                     // 出错信息
        eWQOthers                           // 其他信息
    } 
    public enum eWQLangType
    {
        eENG = 0,
        eCHN,
    }
    public class WindOriginalApi
    {
        [DllImport("WindQuantData.dll", CharSet = CharSet.Unicode)]
        public static extern eWQErr SetEventHandler(IEventHandler eventHandler);

        ///Wind认证登录，必须成功登录方可调用接口函数
        [DllImport("WindQuantData.dll", CharSet = CharSet.Unicode)]
        public static extern eWQErr WDataAuthorize(ref WQAUTH_INFO argr_AuthInfo);

        ///Wind认证登出
        [DllImport("WindQuantData.dll", CharSet = CharSet.Unicode)]
        public static extern eWQErr WDataAuthQuit();

        // Wind函数使用说明：
        // 以下所有的取数据的函数都是异步函数。
        // 一般性的共通语法要求如下：
        // reqEventHandler参数：如果设置了这个参数，则调用此回调函数返回数据；否则使用预先指定的主回调函数返回数据。
        // lpReqParam参数：用户参数，回调时原样返回。
        // windcode参数：函数能够接受的windcode。如果有多个，用半角逗号隔开。例如"000001.SZ,000002.SZ"。
        // indicators参数：函数能够接受的指标名称。如果有多个，用半角逗号隔开。例如"high,open,low,close"。
        // beginTime与endTime参数：函数能够接受的时间和日期字符串。可接受的字符串必须形如"yyyymmdd"，"yyyy-mm-dd"，"yyyymmdd HHMMSS"或者"yyyy-mm-dd HH:MM:SS"
        // params参数：函数能够接受的参数。形如"param1=value1;param2=value2"，多个参数用半角分号隔开。
        

        /// <summary>
        ///  WSD函数，取时间序列数据，支持单品种多指标多时间
        /// </summary>
        /// <param name="windcode">函数能够接受的windcode。如果有多个，用半角逗号隔开。例如"000001.SZ,000002.SZ"。</param>
        /// <param name="indicators">函数能够接受的指标名称。如果有多个，用半角逗号隔开。例如"high,open,low,close"。</param>
        /// <param name="beginTime">函数能够接受的时间和日期字符串。可接受的字符串必须形如"yyyymmdd"，"yyyy-mm-dd"，"yyyymmdd HHMMSS"或者"yyyy-mm-dd HH:MM:SS"</param>
        /// <param name="endTime">函数能够接受的时间和日期字符串。可接受的字符串必须形如"yyyymmdd"，"yyyy-mm-dd"，"yyyymmdd HHMMSS"或者"yyyy-mm-dd HH:MM:SS"</param>
        /// <param name="para">函数能够接受的参数。形如"param1=value1;param2=value2"，多个参数用半角分号隔开。</param>
        /// <param name="reqEventHandler">如果设置了这个参数，则调用此回调函数返回数据；否则使用预先指定的主回调函数返回数据。</param>
        /// <param name="lpReqParam"> 用户参数，回调时原样返回。</param>
        /// <returns></returns>
        [DllImport("WindQuantData.dll", CharSet = CharSet.Unicode)]
        public static extern int WSD(string windcode, string indicators, string beginTime, string endTime, string para, IEventHandler reqEventHandler, IntPtr lpReqParam);
        
        /// <summary>
        /// WSS函数，取快照数据，支持多品种多指标单时间
        /// </summary>
        /// <param name="windcodes"></param>
        /// <param name="indicators"></param>
        /// <param name="para"></param>
        /// <param name="reqEventHandler"></param>
        /// <param name="lpReqParam"></param>
        /// <returns></returns>
        [DllImport("WindQuantData.dll", CharSet = CharSet.Unicode)]
        public static extern int WSS(string windcodes, string indicators, string para, IEventHandler reqEventHandler, IntPtr lpReqParam);
        // WST函数，取日内跳价数据，现为单品种的当日数据
        [DllImport("WindQuantData.dll", CharSet = CharSet.Unicode)]
        public static extern int WST(string windcode, string indicators, string beginTime, string endTime, string para, IEventHandler reqEventHandler, IntPtr lpReqParam);
        // WSI函数，取分钟序列数据，现支持单品种最近一年的数据（单次限制为三个月）
        [DllImport("WindQuantData.dll", CharSet = CharSet.Unicode)]
        public static extern int WSI(string windcode, string indicators, string beginTime, string endTime, string para, IEventHandler reqEventHandler, IntPtr lpReqParam);
        // WSQ函数，取实时行情数据，支持多品种与多指标，可订阅，可取快照
        [DllImport("WindQuantData.dll", CharSet = CharSet.Unicode)]
        public static extern int WSQ(string windcodes, string indicators, string para, IEventHandler reqEventHandler, IntPtr lpReqParam);

        // WSQ函数，取实时行情数据，支持多品种与多指标，可订阅，可取快照
        [DllImport("WindQuantData.dll", CharSet = CharSet.Unicode)]
        public static extern int WSQ2(string windcodes, string indicators, string para, IntPtr reqEventHandler, IntPtr lpReqParam);
        // WSET函数，取相关数据集数据，如指数成分等
        [DllImport("WindQuantData.dll", CharSet = CharSet.Unicode)]
        public static extern int WSET(string reportName, string para, IEventHandler reqEventHandler, IntPtr lpReqParam);
        // WPF函数，取组合管理数据
        [DllImport("WindQuantData.dll", CharSet = CharSet.Unicode)]
        public static extern int WPF(string portfolioName, string viewName, string para, IEventHandler reqEventHandler, IntPtr lpReqParam);
        // WEQS函数，完成证券筛选功能。需要在万得终端里预先定义筛选方案
        [DllImport("WindQuantData.dll", CharSet = CharSet.Unicode)]
        public static extern int WEQS(string filterName, string para, IEventHandler reqEventHandler, IntPtr lpReqParam);
      
        // TDays函数，生成指定的日期序列
        [DllImport("WindQuantData.dll", CharSet = CharSet.Unicode)]
        public static extern int TDays(string beginTime, string endTime, string para, IEventHandler reqEventHandler, IntPtr lpReqParam);
        // TDaysOffset函数，给定一个日期及一个偏移量，计算另一个日期
        [DllImport("WindQuantData.dll", CharSet = CharSet.Unicode)]
        public static extern int TDaysOffset(string beginTime, string offset, string para, IEventHandler reqEventHandler, IntPtr lpReqParam);
        // TDaysCount函数，计算两个给定日期的间隔
        [DllImport("WindQuantData.dll", CharSet = CharSet.Unicode)]
        public static extern int TDaysCount(string beginTime, string endTime, string para, IEventHandler reqEventHandler, IntPtr lpReqParam);
        // 取消订阅请求
        [DllImport("WindQuantData.dll", CharSet = CharSet.Unicode)]
        public static extern eWQErr CancelRequest(int id);
        // 取消所有请求
        [DllImport("WindQuantData.dll", CharSet = CharSet.Unicode)]
        public static extern eWQErr CancelAllRequest();
        // 取错误码的说明信息
        [DllImport("WindQuantData.dll", CharSet = CharSet.Unicode)]
        public static extern string WErr(eWQErr errCode, eWQLangType lang);

        // Internal Use
        [DllImport("WindQuantData.dll", CharSet = CharSet.Unicode)]
        public static extern int GetInternalStatus();
    }
}

#pragma once
#include "stdafx.h"

using namespace System;
namespace WindOriginalApiLibrary {	

	public enum class eWQEventType
		:int
	{
		eWQLogin = 1,                       // 登录相关信息
		eWQResponse,                        // 数据返回
		eWQPartialResponse,                 // 部分数据返回（订阅时适用）
		eWQErrorReport,                     // 出错信息
		eWQOthers                           // 其他信息
	};

	public enum class eWQErr
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

		// 重复的WQID
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
	};

	public enum class eLang
		:int
	{
		eENG = 0,
		eCHN,
	};

	public ref class QuantData
	{
	public:
		array<DateTime>^ ArrDateTime;        // 时间列表（表头）
		array<String^>^ ArrWindCode;         // 品种列表（表头）
		array<String^>^ ArrWindFields;       // 指标列表（表头）
		Object^ MatrixData;                  // 数据
	public:
		static QuantData^ FromIntPtr(IntPtr arg_NCQuantData);
	};

	public ref class QuantEvent
	{
	public:
		int Version;							// 版本号，以备今后扩充
		eWQEventType EventType;                 // Event类型
		eWQErr ErrCode;							// 错误码
		INT64 RequestID;					    // 对应的request
		INT64 EventID;						    // 流水号
		QuantData quantData;      			    // 包含的数据
	};

}
// WindOriginalApiLibrary.h

#pragma once

#include "stdafx.h"
#include "WindCLI.hpp"
#include "WindQuantAPI.h"

#include <windows.h>

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;

namespace WindOriginalApiLibrary {

	public delegate int NATIVEQUANTCALLBACK(WQEvent* pEvent, LPVOID pParam);
	public delegate int  MANAGEDQUANTCALLBACK(QuantEvent^ quantEvent);	
	ref class QuantCallbackHelper
	{
	public:
		delegate int ONQUANTCALLBACKHELPER(QuantCallbackHelper^ helper, QuantEvent^ quantEvent);
	public:
		//arg_nMaxCallCount:n>0表示调用n次后注销, n<=0表示一直不注销直到类实例被回收
		QuantCallbackHelper(ONQUANTCALLBACKHELPER^ arg_helperCallback, MANAGEDQUANTCALLBACK^ arg_mangedCallback, int arg_nMaxCallCount);
		~QuantCallbackHelper();
	public:
		property int MaxCallCount
		{
			int get(){ return _nMaxCallCount; }
		}
		property int CallCount
		{
			int get(){ return _nCallCount; }
		}
	public:
		IEventHandler GetNativeCallback();
		initonly MANAGEDQUANTCALLBACK^ ManagedQuantCallback;
	private:
		///_NativeCallback是把来自Wind的Native数据结构转化为Managed数据结构的一个Native函数
		int _NativeCallback(WQEvent* pEvent, LPVOID pParam);
	private:
		initonly ONQUANTCALLBACKHELPER^ _fnHelperCallback;
		NATIVEQUANTCALLBACK^ _delegateNativeCallback;		
		int _nMaxCallCount;
		int _nCallCount;
	};

	public ref class WindQuantOriginalApi
	{
	public:
		WindQuantOriginalApi();
		eWQErr Open(MANAGEDQUANTCALLBACK^ fnCallback);
		eWQErr Authorize(String^ strUser, String^ strPass, bool bSilentLogin);
		int WindWSD(String^ strWindcode, String^ strIndicators, DateTime dtimeBegin, DateTime dtimeEnd, String^ strParams, MANAGEDQUANTCALLBACK^ callback);
		int WindWSQ(String^ strWindcodes, String^ indicators, bool bSnapOnly, MANAGEDQUANTCALLBACK^ callback);
		///WSET函数，取相关数据集数据，如指数成分等
		int WindWSET(String^ strReportName, String^ params, MANAGEDQUANTCALLBACK^ callback);
		eWQErr WindQuantOriginalApi::WindCancelRequest(int arg_nWQID);
		eWQErr WindQuantOriginalApi::WindCancelAllRequest();
		String^ WindQuantOriginalApi::WindWErr(eWQErr arg_eWQErr, eLang arg_eLang);	
	private:
		///记录每次调用，用于日后回调函数的释放
		void _RecordOneCallback(int arg_nWQId, QuantCallbackHelper^ arg_callbackhelper);
		bool _CancelSubCallback(int arg_nWQId);

		//void _RecordSubCallback(int arg_nWQId, QuantCallbackHelper^ arg_callbackhelper);
		///尝试删除一个回调函数,返回True表示删除了arg_nWQId对应的回调函数,False表示没有找到arg_nWQId对应的回调函数
		//bool _CancelSubCallback(int arg_nWQId);
		void _ClearSubCallback();
		int _OnHelperCallback(QuantCallbackHelper^ callbackhelper, QuantEvent^ arg_QuantData);
	private:
		///Open配置此值,用于全局的回调函数
		QuantCallbackHelper^ _GlobalCallbackHelper;
		///管理一次性回调
		initonly  Dictionary<int, QuantCallbackHelper^>^ _dtOneCallbackHelper;
		///管理订阅回调
		initonly  Dictionary<int, QuantCallbackHelper^>^ _dtSubCallbackHelper;
	};
	
	Object^ SafeArrayToObject(VARTYPE arg_vartp, SAFEARRAY& safearr);
	Object^ PVariantToObject(PVOID arg_pVariant);	
}

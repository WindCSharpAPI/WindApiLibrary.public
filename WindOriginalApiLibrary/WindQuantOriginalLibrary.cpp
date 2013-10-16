// This is the main DLL file.

#include "stdafx.h"
#include "WindCLI.hpp"
#include "WindQuantOriginalLibrary.h"

#include <map>

using namespace std;

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;
using namespace System::Threading;

namespace WindOriginalApiLibrary
{	
	
	#pragma region 工具函数		
	generic <typename T>
	delegate T TESTCONVERT(PVOID pData);
	template<typename T>
	T _PVOIDToData(PVOID arg_pdata)
	{
		return *((T*) arg_pdata);
	}
	generic <typename T>
		Object^ _GetArray(SAFEARRAY& arg_safearr, TESTCONVERT<T>^ arg_fnConvert)
		{
			SAFEARRAY& safearr = arg_safearr;
			int nDim = safearr.cDims;
			int nTotalIndex = 0;
			array<array<T>^>^ matrix = gcnew array<array<T>^>(nDim);
			array<int>^ dimindexs = gcnew array<int>(nDim);
			int nTotalCount = 1;
			for (int i = 0; i < nDim; i++)
			{
				nTotalCount = (safearr.rgsabound + i)->cElements*nTotalCount;
			}

			array<T>^ arr = gcnew array<T>(nTotalCount);
			for (int i = 0; i < nTotalCount; i++)
			{
				arr[i] = arg_fnConvert((PVOID) ((PBYTE) safearr.pvData + i*safearr.cbElements));					
			}
			return arr;
			for (int i = 0; i < nDim; i++)
			{
				if (dimindexs[i] >= ((safearr.rgsabound + i)->cElements))
				{
					if (i + 1 >= nDim)
						break;
					//进位
					//高位+1
					dimindexs[i + 1]++;
					//低位置0
					for (int j = 0; j <= i; j++)
						dimindexs[j] = 0;
				}
				else
				{

				}
			}						
		}
	Object^ SafeArrayToObject(VARTYPE arg_vartp, SAFEARRAY& safearr)
	{
		Object^ oRet = nullptr;
		#pragma region Check vartp
		VARTYPE vartp = arg_vartp;
		if (!(vartp & VT_ARRAY))
		{
			throw gcnew NotImplementedException(String::Format("_SafeArrayToObject(..)vartp({0}) is not has flag:VT_ARRAY", vartp));
		}
		vartp &= (~VT_ARRAY);
		#pragma endregion
		int dims = safearr.cDims;

		if (vartp == VT_I4)
		{
			return  _GetArray<int>(safearr, gcnew TESTCONVERT<int>(_PVOIDToData<int>));			
		}
		else if (vartp == VT_VARIANT)
		{
			return _GetArray<Object^>(safearr, gcnew TESTCONVERT<Object^>(PVariantToObject));			
		}
		else if (vartp == VT_R8)
		{
			return _GetArray<double>(safearr, gcnew TESTCONVERT<double>(_PVOIDToData<double>));		
		}
		else
		{
			return String::Format(L"Unknown VT_TYPE:{0}", vartp);
		}
	}
	Object^ PVariantToObject(PVOID arg_pVariant)
	{
		VARIANT& data = *((VARIANT*) arg_pVariant);

		if ((data.vt & VT_ARRAY))
		{
			VARTYPE vtypeElem = data.vt & (~VT_ARRAY);
			SAFEARRAY& safearr = *data.parray;
			return SafeArrayToObject(data.vt, safearr);
		}
		else
		{
			switch (data.vt)
			{
			case VT_EMPTY:
				{
					return nullptr;
				}
			case VT_VARIANT:
				{
					return PVariantToObject(data.pvarVal);
					break;
				}
			case VT_BSTR:
				{
					return gcnew String(data.bstrVal);
					break;
				}
			case VT_R8:
				{
					return data.dblVal;
					break;
				}
			case VT_DATE:
				{
					return DateTime::FromOADate(data.date);
					break;
				}
			case VT_I4:
				{
					return data.intVal;
					break;
				}
			case VT_I8:
				{
					return data.llVal;
					break;
				}
			default:
				{
					return String::Format(gcnew String(L"Not Implement VTTYPE:{0}"), data.vt);
				}
				break;
			}
		}
	}

	generic <typename T>
	array<array<T>^>^ _WindGetArray(SAFEARRAY& arg_safearr, TESTCONVERT<T>^ arg_fnConvert, int arg_nCodenum, int arg_nIndnum, int arg_nTimenum)
	{
		SAFEARRAY& safearr = arg_safearr;
		int nDim = arg_nIndnum;//Wind is special
		int nTotalIndex = 0;
		array<array<T>^>^ matrix = gcnew array<array<T>^>(nDim);
		for (int i = 0; i < nDim; i++)
		{
			SAFEARRAYBOUND& bound = *(safearr.rgsabound + i);
			if (bound.lLbound != 0)
				throw gcnew NotImplementedException(String::Format(gcnew String("bound.lLbound({0})!=0"), bound.lLbound));
			array<T>^ arr = gcnew array<T>(bound.cElements);
			for (int j = 0; j < bound.cElements; j++)
			{
				arr[j] = arg_fnConvert((PVOID) ((PBYTE) safearr.pvData + nTotalIndex*safearr.cbElements));
				nTotalIndex++;
			}
			matrix[i] = arr;
		}
		return matrix;
	}
	Object^ _WindSafeArrayToObject(PVOID arg_pVariant,int arg_nCodenum,int arg_nIndnum, int arg_nTimenum)
	{
		VARIANT& variant = *(VARIANT*) arg_pVariant;
		int codenum = arg_nCodenum;
		int indnum = arg_nIndnum;
		int timenum = arg_nTimenum;
		if (NULL == arg_pVariant)
			throw gcnew Exception(String::Format("arg_pVariant is Null"));
		if (codenum != 1 && timenum != 1)
			throw gcnew Exception(String::Format("codenum:{0}\r\ntimenum:{1}"));
		if (!(variant.vt & VT_ARRAY))
			throw gcnew Exception(String::Format("Variant is not a SafeArray"));
		
		Object^ oRet = nullptr;		
		VARTYPE vartp = variant.vt;
		SAFEARRAY& safearr = *variant.parray;
		
		if (vartp == VT_I4)
		{
			array<array<int>^>^ matrix = _WindGetArray<int>(safearr, gcnew TESTCONVERT<int>(_PVOIDToData<int>), arg_nCodenum, arg_nIndnum, arg_nTimenum);
			oRet = matrix;
			return oRet;
		}
		else if (vartp == VT_VARIANT)
		{
			array<array<Object^>^>^ matrix = _WindGetArray<Object^>(safearr, gcnew TESTCONVERT<Object^>(PVariantToObject), arg_nCodenum, arg_nIndnum, arg_nTimenum);
			oRet = matrix;
			return oRet;
		}
		else if (vartp == VT_R8)
		{
			array<array<double>^>^ matrix = _WindGetArray<double>(safearr, gcnew TESTCONVERT<double>(_PVOIDToData<double>), arg_nCodenum, arg_nIndnum, arg_nTimenum);
			oRet = matrix;
			return oRet;
		}
		else
		{
			return String::Format(L"Unknown VT_TYPE:{0}", vartp);
		}

	}

	QuantData^ QuantData::FromIntPtr(IntPtr arg_NCQuantData)
	{
		if (IntPtr::Zero == arg_NCQuantData)
			return nullptr;
		WQEvent wqevent;
		wqevent.pQuantData = (WQData*) arg_NCQuantData.ToPointer();
		WQEvent* pEvent = &wqevent;
		QuantData^ quantData = gcnew QuantData();
	
		int codenum = pEvent->pQuantData->ArrWindCode.arrLen;
		int indnum = pEvent->pQuantData->ArrWindFields.arrLen;
		int timenum = pEvent->pQuantData->ArrDateTime.arrLen;

		quantData->ArrWindCode = gcnew array<String^>(codenum);
		quantData->ArrWindFields = gcnew array<String^>(indnum);
		quantData->ArrDateTime = gcnew array<DateTime>(timenum);

		for (int i = 0; i < codenum; i++)
		{
			String^ clistr = gcnew String(pEvent->pQuantData->ArrWindCode.codeArray[i]);
			quantData->ArrWindCode[i] = clistr;
		}
		for (int i = 0; i < indnum; i++)
		{
			String^ clistr = gcnew String(pEvent->pQuantData->ArrWindFields.fieldsArray[i]);
			quantData->ArrWindFields[i] = clistr;
		}
		for (int i = 0; i < timenum; i++)
		{
			DateTime clidatetime = DateTime::FromOADate(pEvent->pQuantData->ArrDateTime.timeArray[i]);
			quantData->ArrDateTime[i] = clidatetime;
		}

		VARIANT& variant = pEvent->pQuantData->MatrixData;
		quantData->MatrixData = PVariantToObject(&variant);
		return quantData;
	}
	#pragma endregion

	//以下代码作废
	//应当使用P/Invoke自动转换机制
	//详细参阅WindApiLibrary(c#)
	#pragma region WindQuantOriginalApi实现函数
	WindQuantOriginalApi::WindQuantOriginalApi()
	{				
		_dtOneCallbackHelper = gcnew Dictionary<int, QuantCallbackHelper^>();
	}
	eWQErr WindQuantOriginalApi::Open(MANAGEDQUANTCALLBACK^ arg_fnCallback)
	{			
		_GlobalCallbackHelper = gcnew QuantCallbackHelper(gcnew QuantCallbackHelper::ONQUANTCALLBACKHELPER(this, &WindQuantOriginalApi::_OnHelperCallback), arg_fnCallback, -1);
		WQErr wqerr = SetEventHandler(_GlobalCallbackHelper->GetNativeCallback());//SetEventHandler(_QuantCallback);
		return (eWQErr) wqerr;
	}
	eWQErr WindQuantOriginalApi::Authorize(String^ strUser, String^ strPass, bool bSilentLogin)
	{		
		WQAUTH_INFO info;
		info.bSilentLogin = bSilentLogin?1:0;

		pin_ptr<const wchar_t> wchUser = PtrToStringChars(strUser);
		pin_ptr<const wchar_t> wchPass = PtrToStringChars(strPass);
		_tcscpy_s((PTSTR) info.strUserName, sizeof(info.strUserName) / sizeof(TCHAR), (PCTSTR) wchUser);
		_tcscpy_s((PTSTR) info.strPassword, sizeof(info.strPassword) / sizeof(TCHAR), (PCTSTR) wchPass);
		WQErr wqerr = WDataAuthorize(&info);


		return (eWQErr)wqerr;
	}
	int WindQuantOriginalApi::WindWSD(String^ strWindcode, String^ strIndicators, DateTime dtimeBegin, DateTime dtimeEnd, String^ strParams, MANAGEDQUANTCALLBACK^ arg_callback)
	{
		pin_ptr<const TCHAR> tchWindCode = PtrToStringChars(strWindcode);
		pin_ptr<const TCHAR> tchIndicators = PtrToStringChars(strIndicators);		
		String^ strBegin = dtimeBegin.ToString("yyyy-MM-dd");
		pin_ptr<const TCHAR> tchBegin = PtrToStringChars(strBegin);
		String^ strEnd = dtimeEnd.ToString("yyyy-MM-dd");
		pin_ptr<const TCHAR> tchEnd = PtrToStringChars(strEnd);
		pin_ptr<const TCHAR> tchParams = PtrToStringChars(strParams);

		IEventHandler fnNative = NULL;
		QuantCallbackHelper^ helper = nullptr;
		IntPtr para = IntPtr::Zero;
		if (arg_callback != nullptr)
		{
			helper = gcnew QuantCallbackHelper(gcnew QuantCallbackHelper::ONQUANTCALLBACKHELPER(this, &WindQuantOriginalApi::_OnHelperCallback), arg_callback, 1);
			fnNative = helper->GetNativeCallback();
		}

		//"000001.SZ", "low,amt", new DateTime(2013, 8, 1), new DateTime(2013, 8, 13)
		int wqid = WSD(tchWindCode, tchIndicators, tchBegin, tchEnd, tchParams, fnNative, NULL);
		_RecordOneCallback(wqid, helper);
		
		return wqid;
	}
	int WindQuantOriginalApi::WindWSQ(String^ strWindcodes, String^ strIndicators, bool bSnapOnly, MANAGEDQUANTCALLBACK^ arg_callback)
	{
		pin_ptr<const TCHAR> tchWindCodes = PtrToStringChars(strWindcodes);
		pin_ptr<const TCHAR> tchIndicators = PtrToStringChars(strIndicators);		
		pin_ptr<const TCHAR> tchParams = PtrToStringChars(String::Format("realtime={0}",bSnapOnly?"N":"Y"));
	
		GCHandle gch;
		IEventHandler nativeFn = NULL;	
		QuantCallbackHelper^ helper = nullptr;
		if (arg_callback != nullptr)
		{						
			helper = gcnew QuantCallbackHelper(gcnew QuantCallbackHelper::ONQUANTCALLBACKHELPER(this, &WindQuantOriginalApi::_OnHelperCallback), arg_callback, bSnapOnly ? 1 : -1);
			nativeFn = helper->GetNativeCallback();
		}

		//"000001.SZ", "low,amt", new DateTime(2013, 8, 1), new DateTime(2013, 8, 13)
		int wqid = WSQ(tchWindCodes, tchIndicators, tchParams, nativeFn, NULL);
		_RecordOneCallback(wqid, helper);
		return wqid;
	}	
	int WindQuantOriginalApi::WindWSET(String^ strReportName, String^ strParams, MANAGEDQUANTCALLBACK^ arg_callback)
	{
		pin_ptr<const TCHAR> tchReportName = PtrToStringChars(strReportName);		
		pin_ptr<const TCHAR> tchParams = PtrToStringChars(strParams);
		IEventHandler nativeFn = NULL;	
		QuantCallbackHelper^ helper = nullptr;
		if (arg_callback != nullptr)
		{		
			helper = gcnew QuantCallbackHelper(gcnew QuantCallbackHelper::ONQUANTCALLBACKHELPER(this,&WindQuantOriginalApi::_OnHelperCallback), arg_callback, 1);
			nativeFn = helper->GetNativeCallback();
		}

		int wqid = WSET(tchReportName, tchParams, nativeFn, NULL);
		_RecordOneCallback(wqid, helper);
		return wqid;
	}
	eWQErr WindQuantOriginalApi::WindCancelRequest(int arg_nWQID)
	{		
		eWQErr eRet =  (eWQErr)CancelRequest(arg_nWQID);
		_CancelSubCallback(arg_nWQID);
		return eRet;
	}
	eWQErr WindQuantOriginalApi::WindCancelAllRequest()
	{						
		eWQErr eRet =  (eWQErr) CancelAllRequest();
		_ClearSubCallback();
		return eRet;
	}
	String^ WindQuantOriginalApi::WindWErr(eWQErr arg_eWQErr, eLang arg_eLang)
	{
		return gcnew String(WErr((int) arg_eWQErr, (WQLangType)(int)arg_eLang));
	}

	
	void WindQuantOriginalApi::_RecordOneCallback(int arg_nWQId, QuantCallbackHelper^ arg_callbackhelper)
	{
		if (arg_nWQId > 0 && arg_callbackhelper != nullptr)
		{
			Monitor::Enter(_dtOneCallbackHelper);
			try
			{
				if (_dtOneCallbackHelper->ContainsKey(arg_nWQId))
				{
					throw gcnew Exception(String::Format("{0}已经是一个记录在案的RequestId", arg_nWQId));
				}
				_dtOneCallbackHelper->Add(arg_nWQId, arg_callbackhelper);
			}
			finally
			{
				Monitor::Exit(_dtOneCallbackHelper);
			}
		}
	}
	bool WindQuantOriginalApi::_CancelSubCallback(int arg_nWQId)
	{
		Monitor::Enter(_dtOneCallbackHelper);
		try
		{
			return _dtOneCallbackHelper->Remove(arg_nWQId);		
		}
		finally
		{
			Monitor::Exit(_dtOneCallbackHelper);
		}
	}
	void WindQuantOriginalApi::_ClearSubCallback()
	{
		Monitor::Enter(_dtOneCallbackHelper);
		try
		{
			_dtOneCallbackHelper->Clear();
		}
		finally
		{
			Monitor::Exit(_dtOneCallbackHelper);
		}
	}
	int  WindQuantOriginalApi::_OnHelperCallback(QuantCallbackHelper^ callbackhelper, QuantEvent^ arg_QuantData)
	{
		if (callbackhelper != nullptr)
		{
			if (callbackhelper->MaxCallCount > 0 && callbackhelper->MaxCallCount <= callbackhelper->CallCount)
			{
				//_CancelSubCallback();
			}
			return callbackhelper->ManagedQuantCallback(arg_QuantData);
		}
		else
		{
			return -1;
		}		
	}
	#pragma endregion

	#pragma region QuantCallbackHelper
	QuantCallbackHelper::QuantCallbackHelper(QuantCallbackHelper::ONQUANTCALLBACKHELPER^ arg_helpercallback , MANAGEDQUANTCALLBACK^ arg_mangedCallback, int arg_nMaxCallCount)
	{		
		_fnHelperCallback = arg_helpercallback;
		_nMaxCallCount = arg_nMaxCallCount;
		_nCallCount = 0;
		ManagedQuantCallback = arg_mangedCallback;

		//把本类的_NativeCallback作为传出的IEventHandler
		//使用局部变量,保证_delegateNativeCallback不被回收
		_delegateNativeCallback = gcnew NATIVEQUANTCALLBACK(this, &QuantCallbackHelper::_NativeCallback);	
	}
	QuantCallbackHelper::~QuantCallbackHelper()
	{				
	}
	IEventHandler QuantCallbackHelper::GetNativeCallback()
	{
		IntPtr ip = Marshal::GetFunctionPointerForDelegate(_delegateNativeCallback);
		return static_cast<IEventHandler>(ip.ToPointer());		
	}
	int QuantCallbackHelper::_NativeCallback(WQEvent* pEvent, LPVOID pParam)
	{
		_nCallCount++;
		if (NULL == pEvent)
			return 0;
		QuantEvent^ quantEvent = gcnew QuantEvent;
		quantEvent->EventType = (eWQEventType) pEvent->EventType;
		quantEvent->ErrCode = (eWQErr) pEvent->ErrCode;
		quantEvent->RequestID = pEvent->RequestID;
		quantEvent->EventID = pEvent->EventID;
		if (pEvent->pQuantData != NULL)
		{
			int codenum = pEvent->pQuantData->ArrWindCode.arrLen;
			int indnum = pEvent->pQuantData->ArrWindFields.arrLen;
			int timenum = pEvent->pQuantData->ArrDateTime.arrLen;

			quantEvent->quantData.ArrWindCode = gcnew array<String^>(codenum);
			quantEvent->quantData.ArrWindFields = gcnew array<String^>(indnum);
			quantEvent->quantData.ArrDateTime = gcnew array<DateTime>(timenum);

			for (int i = 0; i < codenum; i++)
			{
				String^ clistr = gcnew String(pEvent->pQuantData->ArrWindCode.codeArray[i]);
				quantEvent->quantData.ArrWindCode[i] = clistr;
			}
			for (int i = 0; i < indnum; i++)
			{
				String^ clistr = gcnew String(pEvent->pQuantData->ArrWindFields.fieldsArray[i]);
				quantEvent->quantData.ArrWindFields[i] = clistr;
			}
			for (int i = 0; i < timenum; i++)
			{
				DateTime clidatetime = DateTime::FromOADate(pEvent->pQuantData->ArrDateTime.timeArray[i]);
				quantEvent->quantData.ArrDateTime[i] = clidatetime;
			}

			VARIANT& variant = pEvent->pQuantData->MatrixData;
			quantEvent->quantData.MatrixData = PVariantToObject(&variant);
		}

		//调用Managed回调函数
		return this->_fnHelperCallback(this, quantEvent);
	}
	#pragma endregion

}


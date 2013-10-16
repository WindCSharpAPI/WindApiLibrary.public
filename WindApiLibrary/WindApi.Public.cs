using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using WindOriginalApiLibrary;

namespace WindApiLibrary
{
    public class WindApi
    {
        public WindApi()
        {
            Opened = false;
            GCHandle.Alloc(Marshal.GetFunctionPointerForDelegate(_fnOnOriginlCommonCallback = new IEventHandler(_OnOriginlCommonCallback)), GCHandleType.Pinned);
            GCHandle.Alloc(Marshal.GetFunctionPointerForDelegate(_fnOnOriginalCallbackAlways = new IEventHandler(_OnOriginalCallbackAlways)), GCHandleType.Pinned);
            GCHandle.Alloc(Marshal.GetFunctionPointerForDelegate(_fnOnOriginalCallbackOneTime = new IEventHandler(_OnOriginalCallbackOneTime)), GCHandleType.Pinned);
        }
        public string UserName { get; private set; }
        public string Password { get; private set; }
        public bool Opened { get; private set; }

        public delegate void ONMCCALLBACK(eWQErr a, QuantData b);

        public eWQErr OpenAsync(string arg_strUserName, string arg_strPassword, Action<eWQErr> arg_fn)
        {
            return _OpenAsync(arg_strUserName ?? "", arg_strPassword ?? "", true , false, arg_fn);          
        }
        public eWQErr OpenAsync(Action<eWQErr> arg_fn)
        {
            return _OpenAsync( "", "", false , false, arg_fn);     
        }
        public eWQErr WSDAsync(string arg_strCode, string arg_strIndicators, DateTime arg_dateStart, DateTime arg_dateEnd, ONMCCALLBACK arg_fn)
        {
            var strDateStart = _DateToWindString(arg_dateStart);
            var strDateEnd = _DateToWindString(arg_dateEnd);
            IntPtr para;
            //fnNC which will be called by native codes  will not be collected or relocated by GC until GCHandle.Free is called.
            var fnNC = _CreateOriginalCallbackWrapper(arg_fn, out para, true);
            var eo = _WQIdToWQErr(WindOriginalApi.WSD(arg_strCode, arg_strIndicators, strDateStart, strDateEnd, "", fnNC , para));          
            return eo;
        }
        public eWQErr WSQAsync(string arg_strCodes, string arg_strIndicators, bool arg_bSnapOnly,ONMCCALLBACK arg_fn)
        {
            IntPtr para;
            //fnNC which will be called by native codes  will not be collected or relocated by GC until GCHandle.Free is called.
            var fnNC = _CreateOriginalCallbackWrapper(arg_fn, out para , arg_bSnapOnly);
            return _WQIdToWQErr(WindOriginalApi.WSQ(arg_strCodes, arg_strIndicators, string.Format("realtime={0}", arg_bSnapOnly ? "N" : "Y"), fnNC, para));           
        }
        public eWQErr WSSAsync(string arg_strCodes, string arg_strIndicators, string arg_strParas, ONMCCALLBACK arg_fn)
        {
            IntPtr para;         
            var fnNC = _CreateOriginalCallbackWrapper(arg_fn, out para, true);
            var err = _WQIdToWQErr(WindOriginalApi.WSS(arg_strCodes, arg_strIndicators, arg_strParas, fnNC, para));
            return err;
        }        


        /// <summary>
        /// 打开Api
        /// </summary>
        /// <param name="arg_strUserName"></param>
        /// <param name="arg_strPassword"></param>
        /// <param name="arg_bSilentLogin"></param>
        /// <param name="arg_bForceOpen">
        /// 忽略Open状态,强行打开Api
        /// </param>
        /// <param name="arg_fn">Long-time-life callback</param>
        /// <returns></returns>
        eWQErr _OpenAsync(string arg_strUserName, string arg_strPassword, bool arg_bSilentLogin , bool arg_bForceOpen , Action<eWQErr> arg_fn)
        {
            var eErr = eWQErr.OK;           
            if( !arg_bForceOpen && Opened)
            {
                return eErr;//不重复登录
            }
            _fnOnOpen = arg_fn;         
            UserName = arg_strUserName;
            Password = arg_strPassword;
            eErr = WindOriginalApi.SetEventHandler(_fnOnOriginlCommonCallback);
            if (eWQErr.OK != eErr)
                return eErr;

            var info = new WQAUTH_INFO();
            info.strUserName = UserName = arg_bSilentLogin ? arg_strUserName : "";
            info.strPassword = Password = arg_bSilentLogin ? arg_strPassword : "";
            info.bSilentLogin = arg_bSilentLogin?1:0;
            eErr = WindOriginalApi.WDataAuthorize(ref info);           
            return eErr;
        }       
        /// <summary>
        /// 创建一个给Native Codes调用的Callback的Wrapper
        /// 保证函数对象不被GC回收或者Relocation
        /// </summary>
        /// <param name="arg_fnOriginalCallback"></param>
        /// <returns></returns>
        IEventHandler _CreateOriginalCallbackWrapper(ONMCCALLBACK arg_fnMCCallback, out IntPtr argo_para, bool arg_bCallOneTime = true)
        {
            var hMCCallback = GCHandle.Alloc(Marshal.GetFunctionPointerForDelegate(arg_fnMCCallback), GCHandleType.Pinned);
            argo_para = GCHandle.ToIntPtr(hMCCallback);
            if (arg_bCallOneTime)
                return _fnOnOriginalCallbackOneTime;
            else           
                return _fnOnOriginalCallbackAlways;            
        }
        int _OnOriginlCommonCallback(WQEvent quantdata, IntPtr parameter)
        {
            if ( 0 == quantdata.EventID || eWQEventType.eWQLogin == quantdata.EventType)
            {
                Opened = !quantdata.ErrCode.IsFalse();
                if(null != _fnOnOpen)
                    _fnOnOpen(quantdata.ErrCode);
            }           
            return 0;
        }
        int _OnOriginalCallbackOneTime(WQEvent arg_wqevent, IntPtr arg_para)
        {          
            var hFnMCCallback = GCHandle.FromIntPtr(arg_para);
            var fnMCCallback = (ONMCCALLBACK)Marshal.GetDelegateForFunctionPointer((IntPtr)hFnMCCallback.Target, typeof(ONMCCALLBACK));
            //告知GC可以清除这个对象
            hFnMCCallback.Free();
            fnMCCallback(arg_wqevent.ErrCode, QuantData.FromIntPtr(arg_wqevent.pQuantData));
            return 0;
        }       
        int _OnOriginalCallbackAlways(WQEvent arg_wqevent, IntPtr arg_para)
        {
            if (null == arg_wqevent)
            {
                int b = 3;
            }
            if (arg_para == null)
            {

                int a = 3;
            }
            var hFnMCCallback = GCHandle.FromIntPtr(arg_para);
            var fnMCCallback = (ONMCCALLBACK)Marshal.GetDelegateForFunctionPointer((IntPtr)hFnMCCallback.Target, typeof(ONMCCALLBACK));
            //由于是推送的回调函数,所以不需要告知GC可以清除这个对象,直到CancelRequest被调用
            //hFnMCCallback.Free();
            fnMCCallback(arg_wqevent.ErrCode, QuantData.FromIntPtr(arg_wqevent.pQuantData));
            return 0;
        }
        /// <summary>
        /// 规则化一个时间转换
        /// </summary>
        /// <param name="arg_datetime"></param>
        /// <returns></returns>
        string _DateToWindString(DateTime arg_datetime)
        {
            return arg_datetime.ToString("yyyy-MM-dd");
        }
        eWQErr _WQIdToWQErr(int arg_nWQId)
        {
            if (arg_nWQId <= 0)
                return eWQErr.UNKNOWN;//无法转换nWQId到eWQErr
            else
                return eWQErr.OK;
        }
      
        Action<eWQErr> _fnOnOpen;
        readonly IEventHandler _fnOnOriginlCommonCallback;
        readonly IEventHandler _fnOnOriginalCallbackOneTime;
        readonly IEventHandler _fnOnOriginalCallbackAlways;
    }

    public static class ApiHelper
    {
        public static eWQErr Open(this WindApi arg_api, string arg_strUserName, string arg_strPassword, int arg_nTimeOutMS)
        {
            var smph = new Semaphore(0, 1);
            var eErr = eWQErr.OK;
            eErr = arg_api.OpenAsync(arg_strUserName, arg_strPassword, (errI) =>
            {
                eErr = errI;
                smph.Release();
            });
            //直接返回了错误,无须等待Callback返回
            if (eErr.IsFalse())
            {
                return eErr;
            }
            //等待异步返回
            if (smph.WaitOne(arg_nTimeOutMS))
            {
                return eErr;
            }
            else
            {
                return eErr = eWQErr.TIMEOUT;
            }
        }
        public static eWQErr Open(this WindApi arg_api, int arg_nTimeOutMS)
        {
            var smph = new Semaphore(0, 1);
            var eErr = eWQErr.OK;
            eErr = arg_api.OpenAsync((errI) =>
            {
                eErr = errI;
                smph.Release();
            });
            //直接返回了错误,无须等待Callback返回
            if (eErr.IsFalse())
            {
                return eErr;
            }
            //等待异步返回
            if (smph.WaitOne(arg_nTimeOutMS))
            {
                return eErr;
            }
            else
            {
                return eErr = eWQErr.TIMEOUT;
            }
        }
        public static QuantData WSD(this WindApi arg_api, string arg_strCode, string arg_strIndicators, DateTime arg_dateStart, DateTime arg_dateEnd, out eWQErr argo_eWQErr, int arg_nTimeOutMS)
        {
            QuantData oRet = null;
            var eErr = eWQErr.OK;
            var smph = new Semaphore(0, 1);
            var err = arg_api.WSDAsync(arg_strCode, arg_strIndicators, arg_dateStart, arg_dateEnd, (errI, o) =>
            {
                oRet = o;
                eErr = errI;
                smph.Release();
            });
            if (err.IsFalse())
            {
                argo_eWQErr = err;
            }
            else if (!smph.WaitOne(arg_nTimeOutMS))
            {
                argo_eWQErr = eWQErr.TIMEOUT;
            }
            else
            {
                argo_eWQErr = eErr;
            }
            return oRet;
        }
        public static QuantData WSQ(this WindApi arg_api, string arg_strCodes, string arg_strIndicators, out eWQErr argo_eWQErr, int arg_nTimeOutMS)
        {
            QuantData oRet = null;
            var eErr = eWQErr.OK;
            var smph = new Semaphore(0, 1);
            var err = arg_api.WSQAsync(arg_strCodes, arg_strIndicators, true, (errI, o) =>
            {
                oRet = o;
                eErr = errI;
                smph.Release();
            });
            if (err.IsFalse())
            {
                argo_eWQErr = err;
            }
            else if (!smph.WaitOne(arg_nTimeOutMS))
            {
                argo_eWQErr = eWQErr.TIMEOUT;
            }
            else
            {
                argo_eWQErr = eErr;
            }
            return oRet;
        }
        public static QuantData WSS(this WindApi arg_api, string arg_strCodes, string arg_strIndicators, string arg_strParas, out eWQErr argo_eWQErr, int arg_nTimeOutMS)
        {
            QuantData oRet = null;
            var eErr = eWQErr.OK;
            var smph = new Semaphore(0, 1);
            var err = arg_api.WSSAsync(arg_strCodes, arg_strIndicators, arg_strParas, (errI, o) =>
            {
                oRet = o;
                eErr = errI;
                smph.Release();
            });
            if (err.IsFalse())
            {
                argo_eWQErr = err;
            }
            else if (!smph.WaitOne(arg_nTimeOutMS))
            {
                argo_eWQErr = eWQErr.TIMEOUT;
            }
            else
            {
                argo_eWQErr = eErr;
            }
            return oRet;
        }
        public static bool IsFalse(this eWQErr arg_eWQErr)
        {
            return arg_eWQErr != eWQErr.OK;
        }
    }
}

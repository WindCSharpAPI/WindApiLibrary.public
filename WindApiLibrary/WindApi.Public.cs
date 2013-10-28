using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using WindOriginalApiLibrary;

namespace WindApiLibrary
{
    public class WindApi
    {
        public WindApi()
        {

        }

        public string UserName { get; private set; }
        public string Password { get; private set; }

        public ErrorObject OpenAsync(string arg_strUserName, string arg_strPassword, Action<ErrorObject> arg_fn)
        {
            var eErr = eWQErr.OK;
            _fnOpen = arg_fn;
            UserName = arg_strUserName;
            Password = arg_strPassword;
            eErr = WindOriginalApi.SetEventHandler(_OnOriginlCallback);
            if (eWQErr.OK != eErr)
                return ErrorObject.ReturnFalse("SetEventHandler错误:{0}", eErr);

            var info = new WQAUTH_INFO();
            info.strUserName = UserName;
            info.strPassword = Password;
            info.bSilentLogin = 1;
            eErr = WindOriginalApi.WDataAuthorize(ref info);

            if (eWQErr.OK != eErr)
                return ErrorObject.ReturnFalse("WDataAuthorize错误:{0}", eErr);
            return ErrorObject.True;
        }
        public ErrorObject OpenAsync(Action<ErrorObject> arg_fn)
        {
            var eErr = eWQErr.OK;
            _fnOpen = arg_fn;
            UserName = "";
            Password = "";
            eErr = WindOriginalApi.SetEventHandler(_OnOriginlCallback);
            if (eWQErr.OK != eErr)
                return ErrorObject.ReturnFalse("SetEventHandler错误:{0}", eErr);

            var info = new WQAUTH_INFO();
            info.strUserName = UserName;
            info.strPassword = Password;
            info.bSilentLogin = 0;
            eErr = WindOriginalApi.WDataAuthorize(ref info);

            if (eWQErr.OK != eErr)
                return ErrorObject.ReturnFalse("WDataAuthorize错误:{0}", eErr);
            return ErrorObject.True;
        }
        public ErrorObject WSDAsync(string arg_strCode, string arg_strIndicators, DateTime arg_dateStart, DateTime arg_dateEnd, Action<ErrorObject, QuantData> arg_fn)
        {
            var strDateStart = _DateToWindString(arg_dateStart);
            var strDateEnd = _DateToWindString(arg_dateEnd);
            IEventHandler fn = null;
            var eo = _WQIdToEO(WindOriginalApi.WSD(arg_strCode, arg_strIndicators, strDateStart, strDateEnd, "", fn = (qevent, para) =>
            {
                arg_fn(_ErrCodeToEO(qevent.ErrCode), QuantData.FromIntPtr(qevent.pQuantData));
                return 0;
            }, IntPtr.Zero));
            _vtMCCallback.Add(fn);
            return eo;
        }
        public ErrorObject WSQAsync(string arg_strCodes, string arg_strIndicators, bool arg_bSnapOnly, Action<ErrorObject, QuantData> arg_fn)
        {
            if (arg_bSnapOnly)
            {
                return _WQIdToEO(WindOriginalApi.WSQ(arg_strCodes, arg_strIndicators, string.Format("realtime={0}", arg_bSnapOnly ? "N" : "Y"), (qevent, para) =>
                {
                    arg_fn(_ErrCodeToEO(qevent.ErrCode), QuantData.FromIntPtr(qevent.pQuantData));
                    return 0;
                }, IntPtr.Zero));
            }
            else
            {
                var fn = new IEventHandler((qevent, para) =>
                {
                    arg_fn(_ErrCodeToEO(qevent.ErrCode), QuantData.FromIntPtr(qevent.pQuantData));
                    return 0;
                });
                _vtMCCallback.Add(fn);

                return _WQIdToEO(WindOriginalApi.WSQ(arg_strCodes, arg_strIndicators, string.Format("realtime={0}", arg_bSnapOnly ? "N" : "Y"), fn, IntPtr.Zero));
            }

        }
        public ErrorObject WSSAsync(string arg_strCodes, string arg_strIndicators, string arg_strParas, Action<ErrorObject, QuantData> arg_fn)
        {
            IEventHandler fn = null;
            var eo = _WQIdToEO(WindOriginalApi.WSS(arg_strCodes, arg_strIndicators, arg_strParas, fn = (qevent, para) =>
            {
                arg_fn(_ErrCodeToEO(qevent.ErrCode), QuantData.FromIntPtr(qevent.pQuantData));
                return 0;
            }, IntPtr.Zero));
            _vtMCCallback.Add(fn);
            return eo;
        }


        Action<ErrorObject> _fnOpen;
        readonly List<IEventHandler> _vtMCCallback = new List<IEventHandler>();

        int _OnOriginlCallback(WQEvent quantdata, IntPtr parameter)
        {
            if (quantdata.EventID == 0)
            {
                if (quantdata.ErrCode != eWQErr.OK)
                {
                    _fnOpen(ErrorObject.ReturnFalse("WDataAuthorize异步中错误:{0}", quantdata.ErrCode));
                }
            }
            else if (quantdata.EventType == eWQEventType.eWQLogin)
            {
                if (quantdata.ErrCode == eWQErr.OK)
                    _fnOpen(ErrorObject.True);
                else
                    _fnOpen( new ErrorObject(false, "WDataAuthorize失败:" + quantdata.ErrCode, null, QuantData.FromIntPtr(quantdata.pQuantData)));
            }
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
        ErrorObject _WQIdToEO(int arg_nWQId)
        {
            if (arg_nWQId <= 0)
                return ErrorObject.ReturnFalse("错误:{0}", arg_nWQId);
            else
                return ErrorObject.True;
        }
        ErrorObject _ErrCodeToEO(eWQErr arg_errcode)
        {
            if (arg_errcode.IsFlag(eWQErr.OK))
                return ErrorObject.True;
            else
                return ErrorObject.ReturnFalse("{0}", arg_errcode);
        }
    }

    public static class ApiHelper
    {
        public static ErrorObject Open(this WindApi arg_api, string arg_strUserName, string arg_strPassword, int arg_nTimeOutMS)
        {
            var smph = new Semaphore(0, 1);
            var eoRet = ErrorObject.True;
            var eo = arg_api.OpenAsync(arg_strUserName, arg_strPassword, (eoI) =>
            {
                eoRet = eoI;
                smph.Release();
            });
            if (eo.IsFalse)
            {
                return eoRet = eo;
            }

            if (smph.WaitOne(arg_nTimeOutMS))
            {
                return eoRet;
            }
            else
            {
                return eoRet = ErrorObject.ReturnFalse("WindApi.Open超时{0}毫秒", arg_nTimeOutMS);
            }
        }
        public static ErrorObject Open(this WindApi arg_api, int arg_nTimeOutMS)
        {
            var smph = new Semaphore(0, 1);
            var eoRet = ErrorObject.True;
            var eo = arg_api.OpenAsync((eoI) =>
            {
                eoRet = eoI;
                smph.Release();
            });
            if (eo.IsFalse)
            {
                return eoRet = eo;
            }

            if (smph.WaitOne(arg_nTimeOutMS))
            {
                return eoRet;
            }
            else
            {
                return eoRet = ErrorObject.ReturnFalse("WindApi.Open超时{0}毫秒", arg_nTimeOutMS);
            }
        }
        public static QuantData WSD(this WindApi arg_api, string arg_strCode, string arg_strIndicators, DateTime arg_dateStart, DateTime arg_dateEnd, out ErrorObject argo_eo, int arg_nTimeOutMS)
        {
            QuantData oRet = null;
            var eoRet = ErrorObject.True;
            var smph = new Semaphore(0, 1);
            var eo = arg_api.WSDAsync(arg_strCode, arg_strIndicators, arg_dateStart, arg_dateEnd, (eoI, o) =>
            {
                oRet = o;
                eoRet = eoI;
                smph.Release();
            });
            if (eo.IsFalse)
            {
                argo_eo = eo;
            }
            else if (!smph.WaitOne(arg_nTimeOutMS))
            {
                argo_eo = ErrorObject.ReturnFalse("WindApi.WSD超时{0}毫秒", arg_nTimeOutMS);
            }
            else
            {
                argo_eo = eoRet;
            }
            return oRet;
        }
        public static QuantData WSQ(this WindApi arg_api, string arg_strCodes, string arg_strIndicators, out ErrorObject argo_eo, int arg_nTimeOutMS)
        {
            QuantData oRet = null;
            var eoRet = ErrorObject.True;
            var smph = new Semaphore(0, 1);
            var eo = arg_api.WSQAsync(arg_strCodes, arg_strIndicators, true, (eoI, o) =>
            {
                oRet = o;
                eoRet = eoI;
                smph.Release();
            });
            if (eo.IsFalse)
            {
                argo_eo = eo;
            }
            else if (!smph.WaitOne(arg_nTimeOutMS))
            {
                argo_eo = ErrorObject.ReturnFalse("WindApi.WSQ超时{0}毫秒", arg_nTimeOutMS);
            }
            else
            {
                argo_eo = eoRet;
            }
            return oRet;
        }
        public static QuantData WSS(this WindApi arg_api, string arg_strCodes, string arg_strIndicators, string arg_strParas, out ErrorObject argo_eo, int arg_nTimeOutMS)
        {
            QuantData oRet = null;
            var eoRet = ErrorObject.True;
            var smph = new Semaphore(0, 1);
            var eo = arg_api.WSSAsync(arg_strCodes, arg_strIndicators, arg_strParas, (eoI, o) =>
            {
                oRet = o;
                eoRet = eoI;
                smph.Release();
            });
            if (eo.IsFalse)
            {
                argo_eo = eo;
            }
            else if (!smph.WaitOne(arg_nTimeOutMS))
            {
                argo_eo = ErrorObject.ReturnFalse("WindApi.WSS超时{0}毫秒", arg_nTimeOutMS);
            }
            else
            {
                argo_eo = eoRet;
            }
            return oRet;
        }
    }
}

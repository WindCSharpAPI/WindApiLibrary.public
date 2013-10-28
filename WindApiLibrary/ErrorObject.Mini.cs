using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindApiLibrary
{
    public class ErrorObject
    {   
        public ErrorObject(bool arg_bValue, string arg_strInfo, Exception arg_exception = null, object arg_objReturn = null)
        {
            Value = arg_bValue;
            Info = arg_strInfo;
            Tag = arg_objReturn;
        }
        public bool Value { get; private set; }
        public string Info { get; private set; }
        public object Tag { get; private set; }
        public bool IsFalse { get { return !Value; } }
        public bool IsTrue { get { return Value; } }

        public static ErrorObject ReturnFalse(string arg_strTemplate , params object[] arg_args)
        {
            return new ErrorObject(false, string.Format(arg_strTemplate, arg_args));
        }
        public readonly static ErrorObject True = new ErrorObject(true, null);
        public readonly static ErrorObject False = new ErrorObject(false, null);
    }
}

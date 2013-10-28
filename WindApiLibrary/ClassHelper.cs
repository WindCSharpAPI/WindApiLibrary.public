using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindApiLibrary
{
    public static class ClassHelper
    {
        public static bool IsFlag(this Enum arg_eThis, params Enum[] arg_eFlags)
        {
            foreach (var e in arg_eFlags)
                if (arg_eThis.Equals(e))
                    return true;
            return false;
        }
    }
}

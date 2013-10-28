using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using WindApiLibrary;


namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {
            var api = new WindApi();
            var auinfo = new WQAUTH_INFO();
            int test = Marshal.SizeOf(auinfo);
            var eo = api.Open(3000);//("W36783006", "843769676",3000);

            GC.Collect();    
            
            var strIndicators = "open,comp_name,comp_name_eng,founddate,fullname,windl1type,par,issueamount,outstandingbalance,carrydate,maturitydate,couponrate,couponrate2";
            var dateStart = new DateTime(2013, 09, 27);
            var dateEnd = new DateTime(2013, 9, 29);
            var strTFName = "TF1312.CFE";
            var nTimeOutMS = 1000000;    
            var quantdata = api.WSD(strTFName, "open,high,low,close,volume,amt,vwap,oi,settle", dateStart, dateEnd, out eo, nTimeOutMS);

            //open,high,low,close,volume,amt,vwap,oi,settle
            strIndicators = "open,comp_name,comp_name_eng,founddate,fullname,windl1type,par,issueamount,outstandingbalance,carrydate,maturitydate,couponrate,couponrate2";
            var datass = api.WSD("130015.IB", strIndicators, DateTime.Now.AddDays(-10), DateTime.Now, out eo, 5000);
            GC.Collect();     

            var strBondIds = "010004.IB";
            var date = new DateTime(2013, 09, 27);
            var strParas = string.Format("date={0};tradeDate={0};priceAdj=U;cycle=D", date.ToString("yyyyMMdd"));
            quantdata = api.WSS(strBondIds, "pre_close,open,high,low,close,volume,amt,chg,pct_chg,swing,vwap,yield_csi,net_csi,dirty_csi,accruedinterest_csi,modidura_csi,cnvxty_csi", strParas, out eo, nTimeOutMS);           
            var codes = quantdata.ArrWindCode;
            for (var i = 0; i < codes.Length; i++)
            {
                var code = codes[i];
                if (!(quantdata.MatrixData is double[]))
                {
                    Console.Write(string.Format("_api.WSS返回的数组不是double[]而是:{0}", quantdata.MatrixData));                    
                }
                var datas = quantdata.MatrixData as double[];
                var nCol = quantdata.ArrWindFields.Length;              
            }


            strIndicators = "rt_date,rt_time,rt_last,rt_pre_close,rt_vol";         
            api.WSQAsync("130015.IB,130013.IB,TF1312.CFE,IF1310.CFE", strIndicators, false, (err, qdata) =>
            {
                if (qdata != null)
                {
                    for (var i = 0; i < qdata.ArrWindCode.Length; i++)
                    {
                        var code = qdata.ArrWindCode[i];
                        if (null == qdata.ArrWindFields)
                            continue;
                        for (var j = 0; j < qdata.ArrWindFields.Length; j++)
                        {
                            Console.Write(string.Format("{0} {1}：{2}\r\n", code, qdata.ArrWindFields[j], (qdata.MatrixData is double[]) ? ((double[])qdata.MatrixData)[i * qdata.ArrWindFields.Length + j] : double.NaN));
                        }
                    }
                }
            });          

            GC.Collect();           
            while (true)
            {
                Thread.Sleep(1000);
            }          
        }       
    }
}

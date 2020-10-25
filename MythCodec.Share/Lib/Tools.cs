using System;
using System.Collections.Generic;
using System.Text;

namespace MythCodec.Lib
{
    static class Tools
    {
        /// <summary>
        /// 阶乘表
        /// </summary>
        public static ulong[] FactorialTable { get; } = new ulong[] {
            1,//0bit
            1,//0bit
            2,//1bit
            6,//2bit
            24,//4bit
            120,//6bit
            720,//9bit
            5040,//12bit
            40320,//15bit
            362880,//18bit
            3628800,//21bit
            39916800,//25bit
            479001600,//28bit
            6227020800,//32bit
            87178291200,//36bit
            1307674368000,//40bit
            20922789888000,//44bit
            355687428096000,//48bit
            6402373705728000,//52bit
            121645100408832000,//56bit
            2432902008176640000//61bit
            //51090942171709440000 = 21! >
            //18446744073709551615 可以容纳 64bit
        };
        /// <summary>
        /// 康托展开
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public static ulong Cantor(int[] order)
        {
            ulong result = 0;
            int size = order.Length;
            for (var i = 0; i < size; i++)
            {
                for (var j = i+1; j < size; j++)
                {
                    if(order[j]<order[i])
                        result += FactorialTable[size - i - 1];
                }
            }
            return result;
        }
        /// <summary>
        /// 逆康托展开
        /// </summary>
        /// <param name="index"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static int[] Decantor(ulong index, int n)
        {
            var order = new List<int>();
            for (var j = 1; j <= n; j++)
            {
                order.Add(j);
            }

            var ret = new List<int>();
            var mod = index;
            for(var i = n-1; i >= 0; i--)
            {
                var nMax = (int)(mod / FactorialTable[i]);
                mod %= FactorialTable[i];
                ret.Add(order[nMax]);
                order.RemoveAt(nMax);
            }
            return ret.ToArray();
        }
    }
}

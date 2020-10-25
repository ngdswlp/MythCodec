using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MythCodec.Lib
{ 
    /// <summary>
    /// 参考谜钥
    /// </summary>
    public class Model
    {
        public const string DefaultString = "关于这个问题，我就说两句，这种事情见得多了，我只想说懂得都懂，不懂的我也不多解释，毕竟自己知道就好，细细品吧。你们也别来问我怎么了，利益牵扯太大，说了对你我都没好处，当不知道就行了，其余的我只能说这里面水很深，牵扯到很多东西。详细情况你们自己是很难找的，网上大部分已经删除干净了，所以我只能说懂得都懂。懂的人已经基本都获利上岸什么的了，不懂的人永远不懂，关键懂的人都是自己悟的，你也不知道谁是懂的人也没法请教，大家都藏着掖着生怕别人知道自己懂事，懂了就能收割不懂的，你甚至都不知道自己不懂。只是在有些时候，某些人对某些事情不懂装懂，还以为别人不懂。其实自己才是不懂的，别人懂的够多了，不仅懂，还懂的超越了这个范围，但是某些不懂的人让这个懂的人完全教不懂，所以不懂的人永远不懂，只能不懂装懂，别人说懂的都懂，只要点点头就行了，其实你懂的我也懂，谁让我们都懂呢，不懂的话也没必要装懂，毕竟里面牵扣扯到很多懂不了的事。这种事懂的人也没必要访出来，不懂的人看见又来问七问八，最后跟他说了他也不一定能懂，就算懂了以后也对他不好，毕竟懂的太多了不是好事。所以大家最好是不懂就不要去了解，懂太多不好。";
        public static Model Default =>new Model(DefaultString);
        //子句分割符
        private const string seperators = "　？；：！。，、 ,.?;:'!\r\n";
        private readonly List<Clause> segments;
        public string Origin { private set; get; }
        /// <summary>
        /// 谜钥容量
        /// </summary>
        public int BitLength => segments.Sum((s) => s.BitLength);
        private int MaxCharNum => Tools.FactorialTable.Length;
        /// <summary>
        /// 解析文本谜钥
        /// </summary>
        /// <param name="model">谜钥文本，每一子句不超过20个字符，重复字符和超长部分不会用作载体</param>
        public Model(string model)
        {
            if (string.IsNullOrEmpty(model))
                model = DefaultString;
            if (seperators.IndexOf(model.Last()) == -1)//没有结尾符的处理自动加换行
            {
                model += "\n";
            }
            Origin = model;
            segments = new List<Clause>();
            var redundance = new StringBuilder();//冗余字符
            var payloadChars = new List<char>();//载体字符
            foreach (var c in model)
            {
                //分隔符处理
                if (seperators.IndexOf(c)!=-1)
                {
                    redundance.Append(c);
                    segments.Add(new Clause(payloadChars.ToList(), redundance.ToString()));
                    redundance.Clear();
                    payloadChars.Clear();
                }
                else//载体字符处理
                {
                    if (payloadChars.Contains(c) || payloadChars.Count >= MaxCharNum)//不超过阶乘表长度
                    {
                        redundance.Append(c);//冗余字符,放在载体之后
                    }
                    else if (!payloadChars.Contains(c))//不重复的字符作为载体
                    {
                        payloadChars.Add(c);
                    }
                }
            }
            if (redundance.Length > 0 || payloadChars.Count > 0)//剩余数据
            {
                segments.Add(new Clause(payloadChars.ToList(), redundance.ToString()));
            }
        }
        /// <summary>
        /// 加谜
        /// </summary>
        /// <param name="data">载荷数据</param>
        /// <param name="extended">扩展模式,根据数据长度裁切或增加载体</param>
        /// <returns>加谜文本</returns>
        public string Encode(byte[] data,bool extended)
        {
            if (BitLength == 0)
                throw new Exception("谜钥容量为0");
            var sb = new StringBuilder();
            data = AddHeader(data);
            //数据放入比特流
            var br = new BitReader(new MemoryStream(data));
            if (!br.EndOfStream)
            {
                if (extended)//扩展模式,超长部分循环秘钥或者数据写完就截断  
                {
                    var index = 0;
                    while (!br.EndOfStream)
                    {
                        sb.Append(segments[index].Encode(br));
                        index++;
                        index %= segments.Count;
                    }
                }
                else
                {
                    if (data.Length * 8 > BitLength)
                        throw new OverflowException($"数据{data.Length * 8}bits超出定长模式最大容量{BitLength}bits");
                    //把数据加载到子句上
                    foreach (var seg in segments)
                    {
                        sb.Append(seg.Encode(br));
                    }
                }   
            }
            return sb.ToString();
        }
        /// <summary>
        /// 第一个字节保存末尾的Segment超出实际数据的位数
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <returns>增加一个头部字节</returns>
        private byte[] AddHeader(byte[] data)
        {
            var ret = new byte[data.Length + 1];

            var segIndex= 0;
            var segCountSum= 0;
            var totalBitCount = ret.Length * 8;

            for(var i=0;i< ret.Length;i++)
            {
                while (segCountSum < totalBitCount)
                {
                    segCountSum += segments[segIndex%segments.Count].BitLength;
                    segIndex++;
                }
            }
            data.CopyTo(ret, 1);
            ret[0] = (byte)(segCountSum - totalBitCount);//超出比特数
            return ret;
        }
        /// <summary>
        /// 解谜
        /// </summary>
        /// <param name="cipher">待解谜文本</param>
        /// <param name="textMode">文本模式，解谜到0则可以提前退出</param>
        /// <returns></returns>
        public byte[] Decode(string cipher)
        {
            //解析已加谜文本谜钥
            var cypherModel = new Model(cipher);
            var bw = new BitWriter();
            var segIndex = 0;
            while(cypherModel.segments.Count > segIndex)//子句需要一一对应，否则无法解谜
            {
                //根据编解谜前后谜钥的序列顺序差异，得到载荷数据，并写入比特流
                segments[segIndex%segments.Count].Decode(bw,cypherModel.segments[segIndex]);
                segIndex++;
            }
            
            return ReadHeader(bw.GetBytes());
        }
        /// <summary>
        /// 根据头字节信息取出数据字节
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private byte[] ReadHeader(byte[] data)
        {
            var overBitCount = data[0];
            var ret = new byte[data.Length - 1 - (int)Math.Ceiling(overBitCount/8.0)];
            Array.Copy(data, 1, ret, 0, ret.Length);
            return ret;
        }
    }
}

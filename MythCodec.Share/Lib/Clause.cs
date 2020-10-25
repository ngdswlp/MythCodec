using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MythCodec.Lib
{
    /// <summary>
    /// 子句
    /// </summary>
    class Clause
    {
        /// <summary>
        /// 载体字符，存储数据
        /// </summary>
        private readonly List<char> payload;
        /// <summary>
        /// 冗余内容
        /// </summary>
        private readonly string redundance;
        /// <summary>
        /// 位容量
        /// </summary>
        public int BitLength { get; private set; }
        /// <summary>
        /// 子句
        /// </summary>
        /// <param name="payload">载体字符</param>
        /// <param name="redundance">剩余内容</param>
        public Clause(List<char> payload, string redundance)
        {
            this.payload = payload;
            this.redundance = redundance;
            BitLength = GetLength();
        }
        /// <summary>
        /// 计算本子句容量
        /// </summary>
        /// <returns></returns>
        private int GetLength()
        {
            if (payload == null) return 0;
            var bit = 0;
            if (payload.Count == Tools.FactorialTable.Length) return 64;//最大位数
            for (var i = 1; i < 64; i++)
            {
                if (1UL << i <= Tools.FactorialTable[payload.Count])
                {
                    bit = i;
                }
                else
                {
                    return bit;
                }
            }
            return bit;
        }
        /// <summary>
        /// 加谜此子句
        /// </summary>
        /// <param name="br">待加谜的比特流</param>
        /// <param name="bitCount">实际写入的比特数</param>
        /// <returns>加谜后的子句文本</returns>
        public string Encode(BitReader br)
        {
            //根据容量读取序号
            var index = br.EndOfStream ? 0 : br.ReadULong(BitLength);
            //逆展开为序列
            var order = Tools.Decantor(index, payload.Count);
            var sb = new StringBuilder();

            //把序列信息加载到载体上
            for (var i = 0; i < payload.Count; i++)
            {
                sb.Append(payload[order[i] - 1]);//order序列为1,2,3....故索引需-1
            }
            return sb.ToString() + redundance;
        }
        /// <summary>
        /// 用此子句序列来解谜对应谜文的子句
        /// </summary>
        /// <param name="bw">解谜比特流</param>
        /// <param name="cypher">已加谜的子句</param>
        public void Decode(BitWriter bw, Clause cypher)
        {
            if (!Enumerable.SequenceEqual(payload.OrderBy(_ => _), cypher.payload.OrderBy(_ => _)))//判断载体是否相同
            {
                throw new Exception("解谜失败，谜钥格式不匹配。");
            }
            var order = new List<int>();
            foreach (var c in cypher.payload)//取得谜文的载体序列
            {
                order.Add(payload.IndexOf(c) + 1);
            }
            var index = Tools.Cantor(order.ToArray());//康托展开
            bw.WriteULong(index, BitLength);//根据载体容量写入到比特流，取得载荷
        }
    }
}

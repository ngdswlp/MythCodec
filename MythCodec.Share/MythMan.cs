using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using MythCodec.Lib;
namespace MythCodec
{
    /// <summary>
    /// 谜码
    /// </summary>
    public class MythMan
    {
        public Model Model { protected set; get; }
        public bool Extended { get; set; } = true;
        public int BitLength => Model.BitLength;
        public MythMan():this(Model.Default)
        {

        }
        /// <summary>
        /// 初始化谜钥
        /// </summary>
        /// <param name="modelStr">使用原始文本初始化</param>
        public MythMan(string modelStr):this(new Model(modelStr))
        {
            
        }
        /// <summary>
        /// 初始化谜钥
        /// </summary>
        /// <param name="model">使用Model初始化</param>
        public MythMan(Model model)
        {
            Model = model;
        }
        /// <summary>
        /// 加谜字节数据
        /// </summary>
        /// <param name="plain">待加谜的字节数组</param>
        /// <returns>加谜后的文本</returns>
        public string Mytherialize(byte[] plain) 
        {
            //容量检测
            var result = Model.Encode(plain, Extended);
            return Extended ? EndOfSentence(result):result;
        }
        private string EndOfSentence(string result)
        {
            if (result.Length > 0)
            {
                switch (result.Last())
                {
                    case char c when "　？；：！。，、".IndexOf(c) != -1:
                        return result.Remove(result.Length - 1) + "。";
                    case char c when ",?;:'!".IndexOf(c) != -1:
                        return result.Remove(result.Length - 1) + ".";
                    default:
                        break;
                }
            }
            return result;
        }
        /// <summary>
        /// 解谜字节数据
        /// </summary>
        /// <param name="cipher">谜语文本</param>
        /// <returns></returns>
        public byte[] Demytherialize(string cipher)
        {
            return Model.Decode(cipher);
        }
        /// <summary>
        /// 加谜UTF8文本
        /// </summary>
        /// <param name="plain">待加谜的UTF8文本</param>
        /// <returns>谜文</returns>
        public string Surprise(string plain)
        {
            var bytes = Encoding.UTF8.GetBytes(plain);
            return Mytherialize(bytes);
        }
        /// <summary>
        /// 解谜UTF8文本
        /// </summary>
        /// <param name="cipher">UTF8谜语文本</param>
        /// <returns>明文</returns>
        public string Translate(string cipher)
        {
            var bytes = Demytherialize(cipher);
            var plain = Encoding.UTF8.GetString(bytes,0,bytes.Length);
            return plain.TrimEnd('\0');
        }
        /// <summary>
        /// 加谜base64串
        /// </summary>
        /// <param name="base64"></param>
        /// <returns></returns>
        public string EncodeFromBase64(string base64)
        {
            var bytes = Convert.FromBase64String(base64);
            return Mytherialize(bytes);
        }
        /// <summary>
        /// 解谜为base64串
        /// </summary>
        /// <param name="cipher"></param>
        /// <returns></returns>
        public string DecodeToBase64(string cipher)
        {
            var bytes = Demytherialize(cipher);
            return Convert.ToBase64String(bytes);
        }
    }
}

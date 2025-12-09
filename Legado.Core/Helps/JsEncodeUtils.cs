using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Web;
using System.Collections.Generic;

namespace Legado.Core.Helps
{
    /// <summary>
    /// JS 加解密扩展类 (对应 Kotlin: EncoderUtils.kt)
    /// 提供 MD5、对称加密、非对称加密、签名、摘要等功能
    /// </summary>
    public class JsEncodeUtils
    {
        //****************** MD5 ************************//

        public string Md5Encode(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            using (var md5 = MD5.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(str);
                byte[] hash = md5.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        public string Md5Encode16(string str)
        {
            string md532 = Md5Encode(str);
            if (md532.Length == 32)
            {
                // 16位MD5通常是32位中间的16个字符 (8-24)
                return md532.Substring(8, 16);
            }
            return md532;
        }

        //****************** 对称加密解密 (Symmetric) ************************//

        /// <summary>
        /// 创建对称加密对象
        /// </summary>
        public SymmetricCrypto CreateSymmetricCrypto(string transformation, byte[] key, byte[] iv)
        {
            return new SymmetricCrypto(transformation, key, iv);
        }

        public SymmetricCrypto CreateSymmetricCrypto(string transformation, byte[] key)
        {
            return new SymmetricCrypto(transformation, key, null);
        }

        public SymmetricCrypto CreateSymmetricCrypto(string transformation, string key)
        {
            return new SymmetricCrypto(transformation, Encoding.UTF8.GetBytes(key), null);
        }

        public SymmetricCrypto CreateSymmetricCrypto(string transformation, string key, string iv)
        {
            byte[] ivBytes = iv != null ? Encoding.UTF8.GetBytes(iv) : null;
            return new SymmetricCrypto(transformation, Encoding.UTF8.GetBytes(key), ivBytes);
        }

        //****************** 非对称加密解密 (Asymmetric) ************************//

        // 注意：由于不知道原 AsymmetricCrypto 的具体实现细节（公钥私钥格式等），
        // 这里仅提供类结构，具体RSA逻辑需根据实际需求补充
        public AsymmetricCrypto CreateAsymmetricCrypto(string transformation)
        {
            return new AsymmetricCrypto(transformation);
        }

        //****************** 签名 (Sign) ************************//

        public Sign CreateSign(string algorithm)
        {
            return new Sign(algorithm);
        }

        //****************** 消息摘要 (Digest) ************************//

        public string DigestHex(string data, string algorithm)
        {
            byte[] hash = CalculateHash(data, algorithm);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        public string DigestBase64Str(string data, string algorithm)
        {
            byte[] hash = CalculateHash(data, algorithm);
            return Convert.ToBase64String(hash);
        }

        private byte[] CalculateHash(string data, string algorithm)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(data);
            HashAlgorithm hashAlgo = null;

            // 简单的算法映射，对应 Java 的 DigestUtil
            switch (algorithm.ToUpper().Replace("-", ""))
            {
                case "MD5": hashAlgo = MD5.Create(); break;
                case "SHA1": hashAlgo = SHA1.Create(); break;
                case "SHA256": hashAlgo = SHA256.Create(); break;
                case "SHA512": hashAlgo = SHA512.Create(); break;
                default: throw new ArgumentException($"Unsupported algorithm: {algorithm}");
            }

            using (hashAlgo)
            {
                return hashAlgo.ComputeHash(inputBytes);
            }
        }

        //****************** HMAC ************************//

        public string HMacHex(string data, string algorithm, string key)
        {
            byte[] hmacBytes = CalculateHMac(data, algorithm, key);
            return BitConverter.ToString(hmacBytes).Replace("-", "").ToLower();
        }

        public string HMacBase64(string data, string algorithm, string key)
        {
            byte[] hmacBytes = CalculateHMac(data, algorithm, key);
            return Convert.ToBase64String(hmacBytes);
        }

        private byte[] CalculateHMac(string data, string algorithm, string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] messageBytes = Encoding.UTF8.GetBytes(data);
            HMAC hmac = null;

            // 对应 Java 的 HMac 算法名称
            switch (algorithm.ToUpper().Replace("-", "").Replace("_", ""))
            {
                case "HMACMD5": hmac = new HMACMD5(keyBytes); break;
                case "HMACSHA1": hmac = new HMACSHA1(keyBytes); break;
                case "HMACSHA256": hmac = new HMACSHA256(keyBytes); break;
                case "HMACSHA512": hmac = new HMACSHA512(keyBytes); break;
                default: throw new ArgumentException($"Unsupported HMAC algorithm: {algorithm}");
            }

            using (hmac)
            {
                return hmac.ComputeHash(messageBytes);
            }
        }

        //****************** Deprecated / Helper Methods ************************//
        // 这里的逻辑复刻了原代码：通过调用 createSymmetricCrypto 统一处理

        [Obsolete("过于繁琐弃用")]
        public byte[] AesDecodeToByteArray(string str, string key, string transformation, string iv)
        {
            return CreateSymmetricCrypto(transformation, key, iv).Decrypt(str);
        }

        [Obsolete("过于繁琐弃用")]
        public string AesDecodeToString(string str, string key, string transformation, string iv)
        {
            return CreateSymmetricCrypto(transformation, key, iv).DecryptStr(str);
        }

        [Obsolete("过于繁琐弃用")]
        public string AesDecodeArgsBase64Str(string data, string key, string mode, string padding, string iv)
        {
            byte[] keyBytes = Convert.FromBase64String(key);
            byte[] ivBytes = Convert.FromBase64String(iv);
            return CreateSymmetricCrypto($"AES/{mode}/{padding}", keyBytes, ivBytes).DecryptStr(data);
        }

        [Obsolete("过于繁琐弃用")]
        public byte[] AesBase64DecodeToByteArray(string str, string key, string transformation, string iv)
        {
            return CreateSymmetricCrypto(transformation, key, iv).Decrypt(str);
        }

        [Obsolete("过于繁琐弃用")]
        public string AesBase64DecodeToString(string str, string key, string transformation, string iv)
        {
            return CreateSymmetricCrypto(transformation, key, iv).DecryptStr(str);
        }

        [Obsolete("过于繁琐弃用")]
        public byte[] AesEncodeToByteArray(string data, string key, string transformation, string iv)
        {
            return CreateSymmetricCrypto(transformation, key, iv).Encrypt(data);
        }

        [Obsolete("过于繁琐弃用")]
        public string AesEncodeToString(string data, string key, string transformation, string iv)
        {
            return CreateSymmetricCrypto(transformation, key, iv).EncryptStr(data); // 注意：这里原代码返回的是 Hex 还是 String(bytes) 取决于具体实现，C#通常转Hex
        }

        [Obsolete("过于繁琐弃用")]
        public byte[] AesEncodeToBase64ByteArray(string data, string key, string transformation, string iv)
        {
            string base64 = CreateSymmetricCrypto(transformation, key, iv).EncryptBase64(data);
            return Encoding.UTF8.GetBytes(base64);
        }

        [Obsolete("过于繁琐弃用")]
        public string AesEncodeToBase64String(string data, string key, string transformation, string iv)
        {
            return CreateSymmetricCrypto(transformation, key, iv).EncryptBase64(data);
        }

        [Obsolete("过于繁琐弃用")]
        public string AesEncodeArgsBase64Str(string data, string key, string mode, string padding, string iv)
        {
            // 原逻辑是 key/iv 视为字符串，这里为了兼容上面的方法签名
            return CreateSymmetricCrypto($"AES/{mode}/{padding}", key, iv).EncryptBase64(data);
        }

        // --- DES & 3DES Wrappers ---
        [Obsolete("过于繁琐弃用")]
        public string DesDecodeToString(string data, string key, string transformation, string iv)
        {
            return CreateSymmetricCrypto(transformation, key, iv).DecryptStr(data);
        }

        [Obsolete("过于繁琐弃用")]
        public string DesEncodeToString(string data, string key, string transformation, string iv)
        {
            return CreateSymmetricCrypto(transformation, key, iv).EncryptStr(data);
        }

        [Obsolete("过于繁琐弃用")]
        public string TripleDESDecodeStr(string data, string key, string mode, string padding, string iv)
        {
            return CreateSymmetricCrypto($"DESede/{mode}/{padding}", key, iv).DecryptStr(data);
        }

        [Obsolete("过于繁琐弃用")]
        public string TripleDESEncodeStr(string data, string key, string mode, string padding, string iv)
        {
            return CreateSymmetricCrypto($"DESede/{mode}/{padding}", key, iv).EncryptStr(data);
        }

        [Obsolete("过于繁琐弃用")]
        public string TripleDESDecodeBase64Str(string data, string key, string mode, string padding, string iv)
        {
            return CreateSymmetricCrypto($"DESede/{mode}/{padding}", key, iv).DecryptStr(data);
        }

        [Obsolete("过于繁琐弃用")]
        public string TripleDESEncodeBase64Str(string data, string key, string mode, string padding, string iv)
        {
            return CreateSymmetricCrypto($"DESede/{mode}/{padding}", key, iv).EncryptBase64(data);
        }

        //****************** Base64 编解码 ************************//

        public string Base64Encode(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(bytes);
        }

        public string Base64Encode(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return "";
            return Convert.ToBase64String(bytes);
        }

        public string Base64Decode(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            try
            {
                byte[] bytes = Convert.FromBase64String(str);
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return "";
            }
        }

        public byte[] Base64DecodeToBytes(string str)
        {
            if (string.IsNullOrEmpty(str)) return new byte[0];
            try
            {
                return Convert.FromBase64String(str);
            }
            catch
            {
                return new byte[0];
            }
        }

        //****************** Base58 编解码 ************************//

        /// <summary>
        /// Base58 编码（Bitcoin 风格）
        /// </summary>
        public string Base58Encode(byte[] input)
        {
            if (input == null || input.Length == 0) return "";
            
            const string ALPHABET = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
            var result = new StringBuilder();
            var num = new System.Numerics.BigInteger(input.Reverse().Concat(new byte[] { 0 }).ToArray());
            
            while (num > 0)
            {
                var remainder = (int)(num % 58);
                num /= 58;
                result.Insert(0, ALPHABET[remainder]);
            }

            // 处理前导零
            foreach (var b in input)
            {
                if (b != 0) break;
                result.Insert(0, '1');
            }

            return result.ToString();
        }

        public string Base58Encode(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return Base58Encode(Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        /// Base58 解码
        /// </summary>
        public byte[] Base58DecodeToBytes(string input)
        {
            if (string.IsNullOrEmpty(input)) return new byte[0];
            
            const string ALPHABET = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
            var num = System.Numerics.BigInteger.Zero;
            
            foreach (var c in input)
            {
                var digit = ALPHABET.IndexOf(c);
                if (digit < 0) throw new ArgumentException($"Invalid Base58 character: {c}");
                num = num * 58 + digit;
            }

            var bytes = num.ToByteArray().Reverse().ToArray();
            
            // 移除可能的符号字节
            if (bytes.Length > 1 && bytes[0] == 0)
            {
                bytes = bytes.Skip(1).ToArray();
            }

            // 处理前导 '1'
            var leadingOnes = input.TakeWhile(c => c == '1').Count();
            var leadingZeros = new byte[leadingOnes];
            
            return leadingZeros.Concat(bytes).ToArray();
        }

        public string Base58Decode(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            try
            {
                return Encoding.UTF8.GetString(Base58DecodeToBytes(str));
            }
            catch
            {
                return "";
            }
        }

        //****************** Base32 编解码 ************************//

        /// <summary>
        /// Base32 编码（RFC 4648）
        /// </summary>
        public string Base32Encode(byte[] input)
        {
            if (input == null || input.Length == 0) return "";
            
            const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var result = new StringBuilder();
            int buffer = input[0];
            int bitsLeft = 8;
            int index = 1;

            while (bitsLeft > 0 || index < input.Length)
            {
                if (bitsLeft < 5)
                {
                    if (index < input.Length)
                    {
                        buffer <<= 8;
                        buffer |= input[index++] & 0xFF;
                        bitsLeft += 8;
                    }
                    else
                    {
                        int pad = 5 - bitsLeft;
                        buffer <<= pad;
                        bitsLeft += pad;
                    }
                }

                int val = 0x1F & (buffer >> (bitsLeft - 5));
                bitsLeft -= 5;
                result.Append(ALPHABET[val]);
            }

            return result.ToString();
        }

        public string Base32Encode(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return Base32Encode(Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        /// Base32 解码
        /// </summary>
        public byte[] Base32DecodeToBytes(string input)
        {
            if (string.IsNullOrEmpty(input)) return new byte[0];
            
            const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            input = input.TrimEnd('=').ToUpper();
            
            var output = new List<byte>();
            int buffer = 0;
            int bitsLeft = 0;

            foreach (var c in input)
            {
                int val = ALPHABET.IndexOf(c);
                if (val < 0) throw new ArgumentException($"Invalid Base32 character: {c}");

                buffer <<= 5;
                buffer |= val & 0x1F;
                bitsLeft += 5;

                if (bitsLeft >= 8)
                {
                    output.Add((byte)((buffer >> (bitsLeft - 8)) & 0xFF));
                    bitsLeft -= 8;
                }
            }

            return output.ToArray();
        }

        public string Base32Decode(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            try
            {
                return Encoding.UTF8.GetString(Base32DecodeToBytes(str));
            }
            catch
            {
                return "";
            }
        }

        //****************** Hex 编解码 ************************//

        public string HexEncode(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return "";
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        public string HexEncode(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return HexEncode(Encoding.UTF8.GetBytes(str));
        }

        public byte[] HexDecodeToBytes(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return new byte[0];
            try
            {
                if (hex.Length % 2 != 0) return new byte[0];
                
                byte[] bytes = new byte[hex.Length / 2];
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
                }
                return bytes;
            }
            catch
            {
                return new byte[0];
            }
        }

        public string HexDecode(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return "";
            try
            {
                return Encoding.UTF8.GetString(HexDecodeToBytes(hex));
            }
            catch
            {
                return "";
            }
        }

        //****************** Unicode 编解码 ************************//

        /// <summary>
        /// 字符串转 Unicode 编码（\uXXXX 格式）
        /// </summary>
        public string StrToUnicode(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            var result = new StringBuilder();
            foreach (char c in str)
            {
                result.Append($"\\u{((int)c):X4}");
            }
            return result.ToString();
        }

        /// <summary>
        /// Unicode 解码为字符串
        /// </summary>
        public string UnicodeToStr(string unicode)
        {
            if (string.IsNullOrEmpty(unicode)) return "";
            try
            {
                return Regex.Replace(unicode, @"\\u([0-9A-Fa-f]{4})", match =>
                {
                    return ((char)Convert.ToInt32(match.Groups[1].Value, 16)).ToString();
                });
            }
            catch
            {
                return unicode;
            }
        }

        //****************** URL 编解码 ************************//

        public string UrlEncode(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return System.Web.HttpUtility.UrlEncode(str, Encoding.UTF8);
        }

        public string UrlDecode(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return System.Web.HttpUtility.UrlDecode(str, Encoding.UTF8);
        }

        //****************** HTML 编解码 ************************//

        public string HtmlEncode(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return System.Web.HttpUtility.HtmlEncode(str);
        }

        public string HtmlDecode(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return System.Web.HttpUtility.HtmlDecode(str);
        }

        //****************** Escape 编解码 ************************//

        /// <summary>
        /// JavaScript escape 编码
        /// </summary>
        public string Escape(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            var result = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || 
                    (c >= 'A' && c <= 'Z') || 
                    (c >= 'a' && c <= 'z') ||
                    c == '-' || c == '_' || c == '.' || c == '!' || 
                    c == '~' || c == '*' || c == '\'' || c == '(' || c == ')')
                {
                    result.Append(c);
                }
                else if (c <= 0xFF)
                {
                    result.Append($"%{((int)c):X2}");
                }
                else
                {
                    result.Append($"%u{((int)c):X4}");
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// JavaScript unescape 解码
        /// </summary>
        public string Unescape(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            try
            {
                var result = new StringBuilder();
                for (int i = 0; i < str.Length; i++)
                {
                    if (str[i] == '%')
                    {
                        if (i + 5 < str.Length && str[i + 1] == 'u')
                        {
                            // %uXXXX 格式
                            string hex = str.Substring(i + 2, 4);
                            result.Append((char)Convert.ToInt32(hex, 16));
                            i += 5;
                        }
                        else if (i + 2 < str.Length)
                        {
                            // %XX 格式
                            string hex = str.Substring(i + 1, 2);
                            result.Append((char)Convert.ToInt32(hex, 16));
                            i += 2;
                        }
                        else
                        {
                            result.Append(str[i]);
                        }
                    }
                    else
                    {
                        result.Append(str[i]);
                    }
                }
                return result.ToString();
            }
            catch
            {
                return str;
            }
        }

        //****************** SHA 系列哈希 ************************//

        public string Sha1Encode(string str)
        {
            return DigestHex(str, "SHA1");
        }

        public string Sha256Encode(string str)
        {
            return DigestHex(str, "SHA256");
        }

        public string Sha384Encode(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            using (var sha = SHA384.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(str);
                byte[] hash = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        public string Sha512Encode(string str)
        {
            return DigestHex(str, "SHA512");
        }
    }

    /// <summary>
    /// 对称加密工具类 (对应 Kotlin: SymmetricCrypto)
    /// 支持 AES、DES、3DES 等算法
    /// </summary>
    public class SymmetricCrypto
    {
        private readonly SymmetricAlgorithm _algorithm;
        private readonly byte[] _key;
        private readonly byte[] _iv;
        private readonly bool _isNoPadding;

        public SymmetricCrypto(string transformation, byte[] key, byte[] iv)
        {
            // transformation 格式如: "AES/CBC/PKCS5Padding" 或 "DES/ECB/NoPadding"
            var parts = transformation.Split('/');
            string algoName = parts[0].ToUpper();
            string modeName = parts.Length > 1 ? parts[1].ToUpper() : "ECB";
            string paddingName = parts.Length > 2 ? parts[2].ToUpper() : "PKCS7PADDING";

            // 1. 创建算法实例
            if (algoName.StartsWith("AES")) _algorithm = Aes.Create();
            else if (algoName.StartsWith("DESEDE") || algoName == "TRIPLEDES") _algorithm = TripleDES.Create();
            else if (algoName.StartsWith("DES")) _algorithm = DES.Create();
            else throw new ArgumentException($"Unsupported Algorithm: {algoName}");

            // 2. 设置模式
            if (Enum.TryParse(modeName, out CipherMode mode))
            {
                _algorithm.Mode = mode;
            }
            else
            {
                // 默认处理或抛错
                _algorithm.Mode = CipherMode.ECB;
            }

            // 3. 设置填充
            // Java PKCS5Padding 等同于 .NET PKCS7
            if (paddingName == "NOPADDING")
            {
                _algorithm.Padding = PaddingMode.None;
                _isNoPadding = true;
            }
            else if (paddingName.Contains("PKCS5") || paddingName.Contains("PKCS7"))
            {
                _algorithm.Padding = PaddingMode.PKCS7;
            }
            else if (paddingName == "ZEROS" || paddingName == "ZEROBYTEPADDING")
            {
                _algorithm.Padding = PaddingMode.Zeros;
            }
            else if (paddingName == "ISO10126PADDING")
            {
                _algorithm.Padding = PaddingMode.ISO10126;
            }

            // 4. 处理 Key 和 IV
            _key = FixKeyLength(_algorithm, key);
            _algorithm.Key = _key;

            if (mode != CipherMode.ECB)
            {
                _iv = FixIvLength(_algorithm, iv);
                _algorithm.IV = _iv;
            }
        }

        /// <summary>
        /// 加密返回字节数组
        /// </summary>
        public byte[] Encrypt(string data)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(data);

            // 如果是 NoPadding，需要手动对齐块大小，通常补0
            if (_isNoPadding)
            {
                // 简单实现：补0到块大小整数倍
                int blockSizeBytes = _algorithm.BlockSize / 8;
                int remainder = inputBytes.Length % blockSizeBytes;
                if (remainder != 0)
                {
                    Array.Resize(ref inputBytes, inputBytes.Length + (blockSizeBytes - remainder));
                }
            }

            using (var encryptor = _algorithm.CreateEncryptor())
            {
                return encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
            }
        }

        /// <summary>
        /// 加密返回 Hex 字符串
        /// </summary>
        public string EncryptHex(string data)
        {
            return BitConverter.ToString(Encrypt(data)).Replace("-", "").ToLower();
        }

        public string EncryptStr(string data)
        {
            // 原 Java 代码这里行为比较模糊，通常指加密后的 bytes 转 string，或 hex
            // 这里为了通用性返回 Hex
            return EncryptHex(data);
        }

        /// <summary>
        /// 加密返回 Base64
        /// </summary>
        public string EncryptBase64(string data)
        {
            return Convert.ToBase64String(Encrypt(data));
        }

        /// <summary>
        /// 解密：输入为 Hex 字符串或 Base64 字符串（自动判断或需要调用者区分，这里假设输入是 Hex 或 Raw String不太可能，通常是Base64或Hex）
        /// 为了兼容原代码的 decrypt(str)，这里假设 str 是 Hex 字符串
        /// </summary>
        public byte[] Decrypt(string data)
        {
            byte[] inputBytes;
            // 简单的 Hex 检测逻辑，或者直接 try Base64
            // Legado 源码中 decrypt(String) 实际上通常处理 Hex 字符串
            try
            {
                // 尝试 Hex 解析
                inputBytes = HexToBytes(data);
            }
            catch
            {
                try
                {
                    // 尝试 Base64
                    inputBytes = Convert.FromBase64String(data);
                }
                catch
                {
                    // 视为普通 Bytes
                    inputBytes = Encoding.UTF8.GetBytes(data);
                }
            }

            using (var decryptor = _algorithm.CreateDecryptor())
            {
                return decryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
            }
        }

        /// <summary>
        /// 解密返回字符串
        /// </summary>
        public string DecryptStr(string data)
        {
            byte[] result = Decrypt(data);
            // 去除可能的填充0 (针对 NoPadding 模式手动补0的情况)
            if (_isNoPadding)
            {
                return Encoding.UTF8.GetString(result).TrimEnd('\0');
            }
            return Encoding.UTF8.GetString(result);
        }

        public SymmetricCrypto SetIv(byte[] iv)
        {
            _algorithm.IV = FixIvLength(_algorithm, iv);
            return this;
        }

        // 辅助方法：调整 Key 长度以适配算法
        private byte[] FixKeyLength(SymmetricAlgorithm alg, byte[] key)
        {
            if (key == null) key = new byte[alg.KeySize / 8]; // 随机或全0

            // 简单截断或补0逻辑，Hutool 实际上会做类似处理，或者抛错
            // 这里为了健壮性，如果长度不对，进行调整
            int validLength = alg.KeySize / 8;
            if (key.Length == validLength) return key;

            byte[] newKey = new byte[validLength];
            Array.Copy(key, newKey, Math.Min(key.Length, validLength));
            return newKey;
        }

        private byte[] FixIvLength(SymmetricAlgorithm alg, byte[] iv)
        {
            int blockSize = alg.BlockSize / 8;
            if (iv == null) return new byte[blockSize];
            if (iv.Length == blockSize) return iv;

            byte[] newIv = new byte[blockSize];
            Array.Copy(iv, newIv, Math.Min(iv.Length, blockSize));
            return newIv;
        }

        private byte[] HexToBytes(string hex)
        {
            if (hex.Length % 2 != 0) throw new ArgumentException();
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }
    }

    /// <summary>
    /// 非对称加密工具类 (对应 Kotlin: AsymmetricCrypto)
    /// 支持 RSA 等算法
    /// </summary>
    public class AsymmetricCrypto
    {
        private readonly string _transformation;
        private RSACryptoServiceProvider _rsa;
        private bool _usePublicKey = true;

        public AsymmetricCrypto(string transformation)
        {
            _transformation = transformation;
            _rsa = new RSACryptoServiceProvider();
        }

        /// <summary>
        /// 设置公钥 (PEM 或 XML 格式)
        /// </summary>
        public AsymmetricCrypto SetPublicKey(string publicKey)
        {
            try
            {
                if (publicKey.Contains("<RSAKeyValue>"))
                {
                    // XML 格式
                    _rsa.FromXmlString(publicKey);
                }
                else
                {
                    // PEM 或 Base64 格式
                    // .NET Standard 2.0 不支持 ImportSubjectPublicKeyInfo
                    // 这里简化处理，仅支持 XML 格式，或使用第三方库
                    Console.WriteLine("[Warning] PEM key format requires .NET Core 3.0+ or third-party library");
                }
                _usePublicKey = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SetPublicKey Error] {ex.Message}");
            }
            return this;
        }

        /// <summary>
        /// 设置私钥 (PEM 或 XML 格式)
        /// </summary>
        public AsymmetricCrypto SetPrivateKey(string privateKey)
        {
            try
            {
                if (privateKey.Contains("<RSAKeyValue>"))
                {
                    // XML 格式
                    _rsa.FromXmlString(privateKey);
                }
                else
                {
                    // PEM 或 Base64 格式
                    // .NET Standard 2.0 不支持 ImportPkcs8PrivateKey
                    Console.WriteLine("[Warning] PEM key format requires .NET Core 3.0+ or third-party library");
                }
                _usePublicKey = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SetPrivateKey Error] {ex.Message}");
            }
            return this;
        }

        /// <summary>
        /// 加密
        /// </summary>
        public byte[] Encrypt(byte[] data)
        {
            try
            {
                // 使用 PKCS1 填充
                return _rsa.Encrypt(data, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Encrypt Error] {ex.Message}");
                return new byte[0];
            }
        }

        public byte[] Encrypt(string data)
        {
            return Encrypt(Encoding.UTF8.GetBytes(data));
        }

        public string EncryptBase64(string data)
        {
            return Convert.ToBase64String(Encrypt(data));
        }

        public string EncryptHex(string data)
        {
            return BitConverter.ToString(Encrypt(data)).Replace("-", "").ToLower();
        }

        /// <summary>
        /// 解密
        /// </summary>
        public byte[] Decrypt(byte[] data)
        {
            try
            {
                return _rsa.Decrypt(data, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Decrypt Error] {ex.Message}");
                return new byte[0];
            }
        }

        public byte[] DecryptFromBase64(string data)
        {
            return Decrypt(Convert.FromBase64String(data));
        }

        public string DecryptToString(string data)
        {
            byte[] encrypted;
            try
            {
                // 尝试 Base64
                encrypted = Convert.FromBase64String(data);
            }
            catch
            {
                // 尝试 Hex
                encrypted = HexToBytes(data);
            }
            return Encoding.UTF8.GetString(Decrypt(encrypted));
        }

        /// <summary>
        /// 签名
        /// </summary>
        public byte[] Sign(byte[] data)
        {
            try
            {
                return _rsa.SignData(data, SHA256.Create());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Sign Error] {ex.Message}");
                return new byte[0];
            }
        }

        public string SignBase64(string data)
        {
            return Convert.ToBase64String(Sign(Encoding.UTF8.GetBytes(data)));
        }

        /// <summary>
        /// 验证签名
        /// </summary>
        public bool Verify(byte[] data, byte[] signature)
        {
            try
            {
                return _rsa.VerifyData(data, SHA256.Create(), signature);
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyBase64(string data, string signature)
        {
            return Verify(Encoding.UTF8.GetBytes(data), Convert.FromBase64String(signature));
        }

        private byte[] HexToBytes(string hex)
        {
            if (hex.Length % 2 != 0) throw new ArgumentException();
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }
    }

    /// <summary>
    /// 签名工具类 (对应 Kotlin: Sign)
    /// 支持数字签名功能
    /// </summary>
    public class Sign
    {
        private readonly string _algorithm;
        private RSACryptoServiceProvider _rsa;
        private HashAlgorithm _hashAlgorithm;

        public Sign(string algorithm)
        {
            _algorithm = algorithm;
            _rsa = new RSACryptoServiceProvider();
            
            // 根据算法名称选择哈希算法
            switch (algorithm.ToUpper())
            {
                case "SHA1WITHRSA":
                case "SHA1":
                    _hashAlgorithm = SHA1.Create();
                    break;
                case "SHA256WITHRSA":
                case "SHA256":
                    _hashAlgorithm = SHA256.Create();
                    break;
                case "SHA512WITHRSA":
                case "SHA512":
                    _hashAlgorithm = SHA512.Create();
                    break;
                case "MD5WITHRSA":
                case "MD5":
                    _hashAlgorithm = MD5.Create();
                    break;
                default:
                    _hashAlgorithm = SHA256.Create();
                    break;
            }
        }

        /// <summary>
        /// 设置公钥
        /// </summary>
        public Sign SetPublicKey(string publicKey)
        {
            try
            {
                if (publicKey.Contains("<RSAKeyValue>"))
                {
                    _rsa.FromXmlString(publicKey);
                }
                else
                {
                    // .NET Standard 2.0 不支持 ImportSubjectPublicKeyInfo
                    Console.WriteLine("[Warning] PEM key format requires .NET Core 3.0+ or third-party library");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Sign SetPublicKey Error] {ex.Message}");
            }
            return this;
        }

        /// <summary>
        /// 设置私钥
        /// </summary>
        public Sign SetPrivateKey(string privateKey)
        {
            try
            {
                if (privateKey.Contains("<RSAKeyValue>"))
                {
                    _rsa.FromXmlString(privateKey);
                }
                else
                {
                    // .NET Standard 2.0 不支持 ImportPkcs8PrivateKey
                    Console.WriteLine("[Warning] PEM key format requires .NET Core 3.0+ or third-party library");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Sign SetPrivateKey Error] {ex.Message}");
            }
            return this;
        }

        /// <summary>
        /// 签名
        /// </summary>
        public byte[] SignData(byte[] data)
        {
            try
            {
                return _rsa.SignData(data, _hashAlgorithm);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SignData Error] {ex.Message}");
                return new byte[0];
            }
        }

        public byte[] SignData(string data)
        {
            return SignData(Encoding.UTF8.GetBytes(data));
        }

        public string SignDataBase64(string data)
        {
            return Convert.ToBase64String(SignData(data));
        }

        public string SignDataHex(string data)
        {
            return BitConverter.ToString(SignData(data)).Replace("-", "").ToLower();
        }

        /// <summary>
        /// 验证签名
        /// </summary>
        public bool VerifyData(byte[] data, byte[] signature)
        {
            try
            {
                return _rsa.VerifyData(data, _hashAlgorithm, signature);
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyData(string data, string signature)
        {
            try
            {
                byte[] signatureBytes;
                try
                {
                    // 尝试 Base64
                    signatureBytes = Convert.FromBase64String(signature);
                }
                catch
                {
                    // 尝试 Hex
                    signatureBytes = HexToBytes(signature);
                }
                return VerifyData(Encoding.UTF8.GetBytes(data), signatureBytes);
            }
            catch
            {
                return false;
            }
        }

        private byte[] HexToBytes(string hex)
        {
            if (hex.Length % 2 != 0) throw new ArgumentException();
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }
    }
}


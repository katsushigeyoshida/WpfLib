using System;
using System.IO;

namespace WpfLib
{
    public class HashCode
    {
        const uint CRCPOLY1 = 0x1021U;          //	CRC16 右送り CCITT
        const uint CRCPOLY2 = 0x8408U;          //	CRC16 左送り
        const ulong CRCPOLY321 = 0x04C11DB7UL;  //	CRC32 右送り
        const ulong CRCPOLY322 = 0xEDB88320UL;  //	CRC32 左送り
        const uint UCHAR_MAX = 0xFF;            //	maximum unsigned char value
        const uint CHAR_BIT = 8;                //	number of bits in a char
        uint[] crctable1 = new uint[UCHAR_MAX + 1];
        uint[] crctable2 = new uint[UCHAR_MAX + 1];
        ulong[] crctable321 = new ulong[UCHAR_MAX + 1];
        ulong[] crctable322 = new ulong[UCHAR_MAX + 1];
        //		public FileInfo m_FileInfo;

        public enum HashType { CRC16R, CRC16L, CRC32R, CRC32L, MD5, SHA1, SHA256, SHA384, SHA512 };
        HashType m_HashType = HashType.CRC16L;

        public HashCode(HashType hashType)
        {
            SetHashType(hashType);
        }

        public void SetHashType(HashType hashType)
        {
            m_HashType = hashType;
            if (m_HashType == HashType.CRC16L)
                MakeCrcTable1();
            else if (m_HashType == HashType.CRC16R)
                MakeCrcTable2();
            else if (m_HashType == HashType.CRC32L)
                MakeCrcTable321();
            else if (m_HashType == HashType.CRC32R)
                MakeCrcTable322();

            //			m_FileInfo = new FileInfo(path);
        }

        //	CRC(cycric redndancy check)巡回冗長検査
        //	ﾁｪｯｸｻﾑに変わって使われている誤り検出法
        //	ﾃﾞｰﾀをあらかじめ定めた数で割り算し、その余りをﾁｪｯｸ用に使う
        //	割り算は途中の引き算の代わりにﾋﾞｯﾄごとの排他的論理和を使う
        //	例えばデータが10110というビット列で割る数が1101なら、割る数のビット数
        //	より1ビット少ない3ビットの0をデータに補って10110000としこれを割り算し
        //	あまりは割る数のビット数より少ない３ビットである。この余りがＣＲＣ値で
        //	ある。

        //	ＣＲＣ(cyclic redundacy check : 巡回冗長検査
        //	X~16+X~12+X~5+1
        //	左送り(CCITT)のCRCテーブル
        private void MakeCrcTable1()
        {
            uint i, j;
            uint r;

            for (i = 0; i <= UCHAR_MAX; i++) {
                r = i << (int)(16 - CHAR_BIT);
                for (j = 0; j < CHAR_BIT; j++) {
                    if ((r & 0x8000U) != 0)
                        r = (r << 1) ^ CRCPOLY1;
                    else
                        r <<= 1;
                }
                crctable1[i] = r & 0xFFFFU;
            }
        }

        //	右送りのCRCテーブル
        public void MakeCrcTable2()
        {
            uint i, j;
            uint r;

            for (i = 0; i <= UCHAR_MAX; i++) {
                r = i;
                for (j = 0; j < CHAR_BIT; j++) {
                    if ((r & 1) != 0)
                        r = (r >> 1) ^ CRCPOLY2;
                    else
                        r >>= 1;
                }
                crctable2[i] = r;
            }
        }

        //	左送り(CCITT)のCRC16を取り出す
        public uint CRC1(uint crc1, byte[] buf, int fs)
        {
            uint c;
            for (int i = 0; i < fs; i++) {
                c = (uint)(0xff & buf[i]);
                crc1 = (crc1 << (int)CHAR_BIT) ^ crctable1[(byte)(crc1 >> (int)(16 - CHAR_BIT) ^ (byte)c)];
            }
            return crc1;
        }

        //	右送りのCRC16を取り出す
        public uint CRC2(uint crc2, byte[] buf, int fs)
        {
            uint c;
            for (int i = 0; i < fs; i++) {
                c = (uint)(0xff & buf[i]);
                crc2 = (crc2 >> (int)CHAR_BIT) ^ crctable2[(byte)(crc2 ^ (byte)c)];
            }
            return crc2;
        }

        public uint GetCrcCCITT(string path)
        {
            uint crc = 0xFFFFU;
            byte[] byteArray = new byte[1024];
            FileStream sfs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            for (int i = 0; i < sfs.Length; i += 1024) {
                int size = sfs.Read(byteArray, 0, 1024);
                crc = CRC1(crc, byteArray, size);
            }
            crc = ~crc & 0xFFFFU;
            sfs.Close();
            return crc;
        }

        public uint GetCrc16R(string path)
        {
            uint crc = 0xFFFFU;
            byte[] byteArray = new byte[1024];
            FileStream sfs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            for (int i = 0; i < sfs.Length; i += 1024) {
                int size = sfs.Read(byteArray, 0, 1024);
                crc = CRC2(crc, byteArray, size);
            }
            crc = crc & 0xFFFFU;
            sfs.Close();
            return crc;
        }

        //	CRC32計算アルゴリズムはRFC2083
        //	http://www.ietf.org/rfc/rfc2083.txt
        //	CRC多項式X^32+X^26+X^23+X^22+X^16+X^12+X^11+X^10+X^8+X^7+X^5+X~4+X^2+X+1を
        //	使った32ﾋﾞｯﾄのCRC
        //
        //	CRC32 左送り CRCテーブル作成
        void MakeCrcTable321()
        {
            uint i, j;
            ulong r;

            for (i = 0; i <= UCHAR_MAX; i++) {
                r = i << (int)(32 - CHAR_BIT);
                for (j = 0; j < CHAR_BIT; j++) {
                    if ((r & 0x80000000UL) != 0)
                        r = (r << 1) ^ CRCPOLY321;
                    else
                        r <<= 1;
                }
                crctable321[i] = r & 0xffffffffUL;
            }
        }

        //	CRC32 右送り CRCテーブル作成
        void MakeCrcTable322()
        {
            uint i, j;
            ulong r;

            for (i = 0; i <= UCHAR_MAX; i++) {
                r = i;
                for (j = 0; j < CHAR_BIT; j++) {
                    if ((r & 1) != 0)
                        r = (r >> 1) ^ CRCPOLY322;
                    else
                        r >>= 1;
                }
                crctable322[i] = r;
            }
        }

        //	CRC32の左送り計算
        ulong CRC321(ulong crc321, byte[] buf, int fs)
        {
            ulong c;
            for (int i = 0; i < fs; i++) {
                c = (ulong)(0xff & buf[i]);
                crc321 = (crc321 << (int)CHAR_BIT) ^
                crctable321[(byte)(crc321 >> (int)(32 - CHAR_BIT) ^ (byte)c)];
            }
            return crc321;
        }

        //	CRC32の右送り計算
        ulong CRC322(ulong crc322, byte[] buf, int fs)
        {
            ulong c;
            for (int i = 0; i < fs; i++) {
                c = (ulong)0xff & buf[i];
                crc322 = (crc322 >> (int)CHAR_BIT) ^
                crctable322[(byte)(crc322 ^ (byte)c)];
            }
            return crc322;
        }

        public ulong GetCrc32L(string path)
        {
            ulong crc = 0xFFFFFFFFUL;
            byte[] byteArray = new byte[1024];
            FileStream sfs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            for (int i = 0; i < sfs.Length; i += 1024) {
                int size = sfs.Read(byteArray, 0, 1024);
                crc = CRC321(crc, byteArray, size);
            }
            crc = ~crc & 0xFFFFFFFFUL;
            sfs.Close();
            return crc;
        }

        //  CRC32
        public ulong GetCrc32R(string path)
        {
            ulong crc = 0xFFFFFFFFUL;
            byte[] byteArray = new byte[1024];
            FileStream sfs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            for (int i = 0; i < sfs.Length; i += 1024) {
                int size = sfs.Read(byteArray, 0, 1024);
                crc = CRC322(crc, byteArray, size);
            }
            crc = crc ^ 0xFFFFFFFFUL;
            sfs.Close();
            return crc;
        }

        public string GetMD5(string path)
        {
            System.IO.FileStream fs = new System.IO.FileStream(
                path, System.IO.FileMode.Open, System.IO.FileAccess.Read);

            //MD5CryptoServiceProviderオブジェクトを作成
            System.Security.Cryptography.MD5CryptoServiceProvider md5 =
                new System.Security.Cryptography.MD5CryptoServiceProvider();
            //または、次のようにもできる
            //System.Security.Cryptography.MD5 md5 =
            //    System.Security.Cryptography.MD5.Create();

            //ハッシュ値を計算する
            byte[] bs = md5.ComputeHash(fs);

            fs.Close();
            return BitConverter.ToString(bs).Replace("-", "");
            //byte型配列を16進数の文字列に変換
            //System.Text.StringBuilder result = new System.Text.StringBuilder();
            //foreach (byte b in bs) {
            //    result.Append(b.ToString("x2"));
            //}
            //ここの部分は次のようにもできる
            //string result = BitConverter.ToString(bs).ToLower().Replace("-","");
        }

        public string GetSHA1(string path)
        {
            System.IO.FileStream fs = new System.IO.FileStream(
                path, System.IO.FileMode.Open, System.IO.FileAccess.Read);

            //SHA1CryptoServiceProviderオブジェクトを作成
            System.Security.Cryptography.SHA1CryptoServiceProvider sha1 =
                new System.Security.Cryptography.SHA1CryptoServiceProvider();

            //ハッシュ値を計算する
            byte[] bs = sha1.ComputeHash(fs);

            fs.Close();
            return BitConverter.ToString(bs).Replace("-", "");
        }

        public string GetSHA256(string path)
        {
            System.IO.FileStream fs = new System.IO.FileStream(
                path, System.IO.FileMode.Open, System.IO.FileAccess.Read);

            //SHA1CryptoServiceProviderオブジェクトを作成
            System.Security.Cryptography.SHA256 sha256 =
                System.Security.Cryptography.SHA256.Create();

            //ハッシュ値を計算する
            byte[] bs = sha256.ComputeHash(fs);

            fs.Close();
            return BitConverter.ToString(bs).Replace("-", "");
        }

        public string GetSHA384(string path)
        {
            System.IO.FileStream fs = new System.IO.FileStream(
                path, System.IO.FileMode.Open, System.IO.FileAccess.Read);

            //SHA1CryptoServiceProviderオブジェクトを作成
            System.Security.Cryptography.SHA384 sha384 =
                System.Security.Cryptography.SHA384.Create();

            //ハッシュ値を計算する
            byte[] bs = sha384.ComputeHash(fs);

            fs.Close();
            return BitConverter.ToString(bs).Replace("-", "");
        }

        public string GetSHA512(string path)
        {
            System.IO.FileStream fs = new System.IO.FileStream(
                path, System.IO.FileMode.Open, System.IO.FileAccess.Read);

            //SHA1CryptoServiceProviderオブジェクトを作成
            System.Security.Cryptography.SHA512 sha512 =
                System.Security.Cryptography.SHA512.Create();

            //ハッシュ値を計算する
            byte[] bs = sha512.ComputeHash(fs);

            fs.Close();
            return BitConverter.ToString(bs).Replace("-", "");
        }

        //  ハッシュコードを取得する
        public string GetHashCode(string path)
        {
            if (m_HashType == HashType.CRC16L) {
                uint crc = GetCrcCCITT(path);
                return crc.ToString("X4");
            } else if (m_HashType == HashType.CRC16R) {
                uint crc = GetCrc16R(path);
                return crc.ToString("X4");
            } else if (m_HashType == HashType.CRC32L) {
                ulong crc = GetCrc32L(path);
                return crc.ToString("X8");
            } else if (m_HashType == HashType.CRC32R) {
                ulong crc = GetCrc32R(path);
                return crc.ToString("X8");
            } else if (m_HashType == HashType.MD5) {
                return GetMD5(path);
            } else if (m_HashType == HashType.SHA1) {
                return GetSHA1(path);
            } else if (m_HashType == HashType.SHA256) {
                return GetSHA256(path);
            } else if (m_HashType == HashType.SHA384) {
                return GetSHA384(path);
            } else if (m_HashType == HashType.SHA512) {
                return GetSHA512(path);
            }
            return "";
        }

        public string GetHashCode(HashType hashType, string path)
        {
            SetHashType(hashType);
            return GetHashCode(path);
        }
    }
}

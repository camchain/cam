#if !NET47
using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;

namespace Cam.Cryptography
{
    [ComVisible(true)]
    public class RIPEMD160Managed : HashAlgorithm
    {
        private byte[] _buffer;
        private long _count; // Number of bytes in the hashed message
        private uint[] _stateMD160;
        private uint[] _blockDWords;

        public override int HashSize => 160;




        public RIPEMD160Managed()
        {
            _stateMD160 = new uint[5];
            _blockDWords = new uint[16];
            _buffer = new byte[64];

            InitializeState();
        }




        public override void Initialize()
        {
            InitializeState();

            Array.Clear(_blockDWords, 0, _blockDWords.Length);
            Array.Clear(_buffer, 0, _buffer.Length);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        protected override void HashCore(byte[] rgb, int ibStart, int cbSize)
        {
            _HashData(rgb, ibStart, cbSize);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        protected override byte[] HashFinal()
        {
            return _EndHash();
        }




        private void InitializeState()
        {
            _count = 0;


            _stateMD160[0] = 0x67452301;
            _stateMD160[1] = 0xefcdab89;
            _stateMD160[2] = 0x98badcfe;
            _stateMD160[3] = 0x10325476;
            _stateMD160[4] = 0xc3d2e1f0;
        }

        [System.Security.SecurityCritical]  // auto-generated
        private unsafe void _HashData(byte[] partIn, int ibStart, int cbSize)
        {
            int bufferLen;
            int partInLen = cbSize;
            int partInBase = ibStart;

            /* Compute length of buffer */
            bufferLen = (int)(_count & 0x3f);

            /* Update number of bytes */
            _count += partInLen;

            fixed (uint* stateMD160 = _stateMD160)
            {
                fixed (byte* buffer = _buffer)
                {
                    fixed (uint* blockDWords = _blockDWords)
                    {
                        if ((bufferLen > 0) && (bufferLen + partInLen >= 64))
                        {
                            Buffer.BlockCopy(partIn, partInBase, _buffer, bufferLen, 64 - bufferLen);
                            partInBase += (64 - bufferLen);
                            partInLen -= (64 - bufferLen);
                            MDTransform(blockDWords, stateMD160, buffer);
                            bufferLen = 0;
                        }

                        /* Copy input to temporary buffer and hash */
                        while (partInLen >= 64)
                        {
                            Buffer.BlockCopy(partIn, partInBase, _buffer, 0, 64);
                            partInBase += 64;
                            partInLen -= 64;
                            MDTransform(blockDWords, stateMD160, buffer);
                        }

                        if (partInLen > 0)
                        {
                            Buffer.BlockCopy(partIn, partInBase, _buffer, bufferLen, partInLen);
                        }
                    }
                }
            }
        }

        [SecurityCritical]  // auto-generated
        private byte[] _EndHash()
        {
            byte[] pad;
            int padLen;
            long bitCount;
            byte[] hash = new byte[20];

            /* Compute padding: 80 00 00 ... 00 00 <bit count>
             */

            padLen = 64 - (int)(_count & 0x3f);
            if (padLen <= 8)
                padLen += 64;

            pad = new byte[padLen];
            pad[0] = 0x80;

            bitCount = _count * 8;

            pad[padLen - 1] = (byte)((bitCount >> 56) & 0xff);
            pad[padLen - 2] = (byte)((bitCount >> 48) & 0xff);
            pad[padLen - 3] = (byte)((bitCount >> 40) & 0xff);
            pad[padLen - 4] = (byte)((bitCount >> 32) & 0xff);
            pad[padLen - 5] = (byte)((bitCount >> 24) & 0xff);
            pad[padLen - 6] = (byte)((bitCount >> 16) & 0xff);
            pad[padLen - 7] = (byte)((bitCount >> 8) & 0xff);
            pad[padLen - 8] = (byte)((bitCount >> 0) & 0xff);

            /* Digest padding */
            _HashData(pad, 0, pad.Length);

            /* Store digest */
            DWORDToLittleEndian(hash, _stateMD160, 5);

            return hash;
        }

        [System.Security.SecurityCritical]  // auto-generated
        private static unsafe void MDTransform(uint* blockDWords, uint* state, byte* block)
        {
            uint aa = state[0];
            uint bb = state[1];
            uint cc = state[2];
            uint dd = state[3];
            uint ee = state[4];

            uint aaa = aa;
            uint bbb = bb;
            uint ccc = cc;
            uint ddd = dd;
            uint eee = ee;

            DWORDFromLittleEndian(blockDWords, 16, block);

            /*
                As we don't have macros in C# and we don't want to pay the cost of a function call
                (which BTW is quite important here as we would have to pass 5 args by ref in 
                16 * 10 = 160 function calls)
                we'll prefer a less compact code to a less performant code
            */


            aa += blockDWords[0] + F(bb, cc, dd);
            aa = (aa << 11 | aa >> (32 - 11)) + ee;
            cc = (cc << 10 | cc >> (32 - 10));

            ee += blockDWords[1] + F(aa, bb, cc);
            ee = (ee << 14 | ee >> (32 - 14)) + dd;
            bb = (bb << 10 | bb >> (32 - 10));

            dd += blockDWords[2] + F(ee, aa, bb);
            dd = (dd << 15 | dd >> (32 - 15)) + cc;
            aa = (aa << 10 | aa >> (32 - 10));

            cc += blockDWords[3] + F(dd, ee, aa);
            cc = (cc << 12 | cc >> (32 - 12)) + bb;
            ee = (ee << 10 | ee >> (32 - 10));

            bb += blockDWords[4] + F(cc, dd, ee);
            bb = (bb << 5 | bb >> (32 - 5)) + aa;
            dd = (dd << 10 | dd >> (32 - 10));

            aa += blockDWords[5] + F(bb, cc, dd);
            aa = (aa << 8 | aa >> (32 - 8)) + ee;
            cc = (cc << 10 | cc >> (32 - 10));

            ee += blockDWords[6] + F(aa, bb, cc);
            ee = (ee << 7 | ee >> (32 - 7)) + dd;
            bb = (bb << 10 | bb >> (32 - 10));

            dd += blockDWords[7] + F(ee, aa, bb);
            dd = (dd << 9 | dd >> (32 - 9)) + cc;
            aa = (aa << 10 | aa >> (32 - 10));

            cc += blockDWords[8] + F(dd, ee, aa);
            cc = (cc << 11 | cc >> (32 - 11)) + bb;
            ee = (ee << 10 | ee >> (32 - 10));

            bb += blockDWords[9] + F(cc, dd, ee);
            bb = (bb << 13 | bb >> (32 - 13)) + aa;
            dd = (dd << 10 | dd >> (32 - 10));

            aa += blockDWords[10] + F(bb, cc, dd);
            aa = (aa << 14 | aa >> (32 - 14)) + ee;
            cc = (cc << 10 | cc >> (32 - 10));

            ee += blockDWords[11] + F(aa, bb, cc);
            ee = (ee << 15 | ee >> (32 - 15)) + dd;
            bb = (bb << 10 | bb >> (32 - 10));

            dd += blockDWords[12] + F(ee, aa, bb);
            dd = (dd << 6 | dd >> (32 - 6)) + cc;
            aa = (aa << 10 | aa >> (32 - 10));

            cc += blockDWords[13] + F(dd, ee, aa);
            cc = (cc << 7 | cc >> (32 - 7)) + bb;
            ee = (ee << 10 | ee >> (32 - 10));

            bb += blockDWords[14] + F(cc, dd, ee);
            bb = (bb << 9 | bb >> (32 - 9)) + aa;
            dd = (dd << 10 | dd >> (32 - 10));

            aa += blockDWords[15] + F(bb, cc, dd);
            aa = (aa << 8 | aa >> (32 - 8)) + ee;
            cc = (cc << 10 | cc >> (32 - 10));


            ee += G(aa, bb, cc) + blockDWords[7] + 0x5a827999;
            ee = (ee << 7 | ee >> (32 - 7)) + dd;
            bb = (bb << 10 | bb >> (32 - 10));

            dd += G(ee, aa, bb) + blockDWords[4] + 0x5a827999;
            dd = (dd << 6 | dd >> (32 - 6)) + cc;
            aa = (aa << 10 | aa >> (32 - 10));

            cc += G(dd, ee, aa) + blockDWords[13] + 0x5a827999;
            cc = (cc << 8 | cc >> (32 - 8)) + bb;
            ee = (ee << 10 | ee >> (32 - 10));

            bb += G(cc, dd, ee) + blockDWords[1] + 0x5a827999;
            bb = (bb << 13 | bb >> (32 - 13)) + aa;
            dd = (dd << 10 | dd >> (32 - 10));

            aa += G(bb, cc, dd) + blockDWords[10] + 0x5a827999;
            aa = (aa << 11 | aa >> (32 - 11)) + ee;
            cc = (cc << 10 | cc >> (32 - 10));

            ee += G(aa, bb, cc) + blockDWords[6] + 0x5a827999;
            ee = (ee << 9 | ee >> (32 - 9)) + dd;
            bb = (bb << 10 | bb >> (32 - 10));

            dd += G(ee, aa, bb) + blockDWords[15] + 0x5a827999;
            dd = (dd << 7 | dd >> (32 - 7)) + cc;
            aa = (aa << 10 | aa >> (32 - 10));

            cc += G(dd, ee, aa) + blockDWords[3] + 0x5a827999;
            cc = (cc << 15 | cc >> (32 - 15)) + bb;
            ee = (ee << 10 | ee >> (32 - 10));

            bb += G(cc, dd, ee) + blockDWords[12] + 0x5a827999;
            bb = (bb << 7 | bb >> (32 - 7)) + aa;
            dd = (dd << 10 | dd >> (32 - 10));

            aa += G(bb, cc, dd) + blockDWords[0] + 0x5a827999;
            aa = (aa << 12 | aa >> (32 - 12)) + ee;
            cc = (cc << 10 | cc >> (32 - 10));

            ee += G(aa, bb, cc) + blockDWords[9] + 0x5a827999;
            ee = (ee << 15 | ee >> (32 - 15)) + dd;
            bb = (bb << 10 | bb >> (32 - 10));

            dd += G(ee, aa, bb) + blockDWords[5] + 0x5a827999;
            dd = (dd << 9 | dd >> (32 - 9)) + cc;
            aa = (aa << 10 | aa >> (32 - 10));

            cc += G(dd, ee, aa) + blockDWords[2] + 0x5a827999;
            cc = (cc << 11 | cc >> (32 - 11)) + bb;
            ee = (ee << 10 | ee >> (32 - 10));

            bb += G(cc, dd, ee) + blockDWords[14] + 0x5a827999;
            bb = (bb << 7 | bb >> (32 - 7)) + aa;
            dd = (dd << 10 | dd >> (32 - 10));

            aa += G(bb, cc, dd) + blockDWords[11] + 0x5a827999;
            aa = (aa << 13 | aa >> (32 - 13)) + ee;
            cc = (cc << 10 | cc >> (32 - 10));

            ee += G(aa, bb, cc) + blockDWords[8] + 0x5a827999;
            ee = (ee << 12 | ee >> (32 - 12)) + dd;
            bb = (bb << 10 | bb >> (32 - 10));


            dd += H(ee, aa, bb) + blockDWords[3] + 0x6ed9eba1;
            dd = (dd << 11 | dd >> (32 - 11)) + cc;
            aa = (aa << 10 | aa >> (32 - 10));

            cc += H(dd, ee, aa) + blockDWords[10] + 0x6ed9eba1;
            cc = (cc << 13 | cc >> (32 - 13)) + bb;
            ee = (ee << 10 | ee >> (32 - 10));

            bb += H(cc, dd, ee) + blockDWords[14] + 0x6ed9eba1;
            bb = (bb << 6 | bb >> (32 - 6)) + aa;
            dd = (dd << 10 | dd >> (32 - 10));

            aa += H(bb, cc, dd) + blockDWords[4] + 0x6ed9eba1;
            aa = (aa << 7 | aa >> (32 - 7)) + ee;
            cc = (cc << 10 | cc >> (32 - 10));

            ee += H(aa, bb, cc) + blockDWords[9] + 0x6ed9eba1;
            ee = (ee << 14 | ee >> (32 - 14)) + dd;
            bb = (bb << 10 | bb >> (32 - 10));

            dd += H(ee, aa, bb) + blockDWords[15] + 0x6ed9eba1;
            dd = (dd << 9 | dd >> (32 - 9)) + cc;
            aa = (aa << 10 | aa >> (32 - 10));

            cc += H(dd, ee, aa) + blockDWords[8] + 0x6ed9eba1;
            cc = (cc << 13 | cc >> (32 - 13)) + bb;
            ee = (ee << 10 | ee >> (32 - 10));

            bb += H(cc, dd, ee) + blockDWords[1] + 0x6ed9eba1;
            bb = (bb << 15 | bb >> (32 - 15)) + aa;
            dd = (dd << 10 | dd >> (32 - 10));

            aa += H(bb, cc, dd) + blockDWords[2] + 0x6ed9eba1;
            aa = (aa << 14 | aa >> (32 - 14)) + ee;
            cc = (cc << 10 | cc >> (32 - 10));

            ee += H(aa, bb, cc) + blockDWords[7] + 0x6ed9eba1;
            ee = (ee << 8 | ee >> (32 - 8)) + dd;
            bb = (bb << 10 | bb >> (32 - 10));

            dd += H(ee, aa, bb) + blockDWords[0] + 0x6ed9eba1;
            dd = (dd << 13 | dd >> (32 - 13)) + cc;
            aa = (aa << 10 | aa >> (32 - 10));

            cc += H(dd, ee, aa) + blockDWords[6] + 0x6ed9eba1;
            cc = (cc << 6 | cc >> (32 - 6)) + bb;
            ee = (ee << 10 | ee >> (32 - 10));

            bb += H(cc, dd, ee) + blockDWords[13] + 0x6ed9eba1;
            bb = (bb << 5 | bb >> (32 - 5)) + aa;
            dd = (dd << 10 | dd >> (32 - 10));

            aa += H(bb, cc, dd) + blockDWords[11] + 0x6ed9eba1;
            aa = (aa << 12 | aa >> (32 - 12)) + ee;
            cc = (cc << 10 | cc >> (32 - 10));

            ee += H(aa, bb, cc) + blockDWords[5] + 0x6ed9eba1;
            ee = (ee << 7 | ee >> (32 - 7)) + dd;
            bb = (bb << 10 | bb >> (32 - 10));

            dd += H(ee, aa, bb) + blockDWords[12] + 0x6ed9eba1;
            dd = (dd << 5 | dd >> (32 - 5)) + cc;
            aa = (aa << 10 | aa >> (32 - 10));


            cc += I(dd, ee, aa) + blockDWords[1] + 0x8f1bbcdc;
            cc = (cc << 11 | cc >> (32 - 11)) + bb;
            ee = (ee << 10 | ee >> (32 - 10));

            bb += I(cc, dd, ee) + blockDWords[9] + 0x8f1bbcdc;
            bb = (bb << 12 | bb >> (32 - 12)) + aa;
            dd = (dd << 10 | dd >> (32 - 10));

            aa += I(bb, cc, dd) + blockDWords[11] + 0x8f1bbcdc;
            aa = (aa << 14 | aa >> (32 - 14)) + ee;
            cc = (cc << 10 | cc >> (32 - 10));

            ee += I(aa, bb, cc) + blockDWords[10] + 0x8f1bbcdc;
            ee = (ee << 15 | ee >> (32 - 15)) + dd;
            bb = (bb << 10 | bb >> (32 - 10));

            dd += I(ee, aa, bb) + blockDWords[0] + 0x8f1bbcdc;
            dd = (dd << 14 | dd >> (32 - 14)) + cc;
            aa = (aa << 10 | aa >> (32 - 10));

            cc += I(dd, ee, aa) + blockDWords[8] + 0x8f1bbcdc;
            cc = (cc << 15 | cc >> (32 - 15)) + bb;
            ee = (ee << 10 | ee >> (32 - 10));

            bb += I(cc, dd, ee) + blockDWords[12] + 0x8f1bbcdc;
            bb = (bb << 9 | bb >> (32 - 9)) + aa;
            dd = (dd << 10 | dd >> (32 - 10));

            aa += I(bb, cc, dd) + blockDWords[4] + 0x8f1bbcdc;
            aa = (aa << 8 | aa >> (32 - 8)) + ee;
            cc = (cc << 10 | cc >> (32 - 10));

            ee += I(aa, bb, cc) + blockDWords[13] + 0x8f1bbcdc;
            ee = (ee << 9 | ee >> (32 - 9)) + dd;
            bb = (bb << 10 | bb >> (32 - 10));

            dd += I(ee, aa, bb) + blockDWords[3] + 0x8f1bbcdc;
            dd = (dd << 14 | dd >> (32 - 14)) + cc;
            aa = (aa << 10 | aa >> (32 - 10));

            cc += I(dd, ee, aa) + blockDWords[7] + 0x8f1bbcdc;
            cc = (cc << 5 | cc >> (32 - 5)) + bb;
            ee = (ee << 10 | ee >> (32 - 10));

            bb += I(cc, dd, ee) + blockDWords[15] + 0x8f1bbcdc;
            bb = (bb << 6 | bb >> (32 - 6)) + aa;
            dd = (dd << 10 | dd >> (32 - 10));

            aa += I(bb, cc, dd) + blockDWords[14] + 0x8f1bbcdc;
            aa = (aa << 8 | aa >> (32 - 8)) + ee;
            cc = (cc << 10 | cc >> (32 - 10));

            ee += I(aa, bb, cc) + blockDWords[5] + 0x8f1bbcdc;
            ee = (ee << 6 | ee >> (32 - 6)) + dd;
            bb = (bb << 10 | bb >> (32 - 10));

            dd += I(ee, aa, bb) + blockDWords[6] + 0x8f1bbcdc;
            dd = (dd << 5 | dd >> (32 - 5)) + cc;
            aa = (aa << 10 | aa >> (32 - 10));

            cc += I(dd, ee, aa) + blockDWords[2] + 0x8f1bbcdc;
            cc = (cc << 12 | cc >> (32 - 12)) + bb;
            ee = (ee << 10 | ee >> (32 - 10));


            bb += J(cc, dd, ee) + blockDWords[4] + 0xa953fd4e;
            bb = (bb << 9 | bb >> (32 - 9)) + aa;
            dd = (dd << 10 | dd >> (32 - 10));

            aa += J(bb, cc, dd) + blockDWords[0] + 0xa953fd4e;
            aa = (aa << 15 | aa >> (32 - 15)) + ee;
            cc = (cc << 10 | cc >> (32 - 10));

            ee += J(aa, bb, cc) + blockDWords[5] + 0xa953fd4e;
            ee = (ee << 5 | ee >> (32 - 5)) + dd;
            bb = (bb << 10 | bb >> (32 - 10));

            dd += J(ee, aa, bb) + blockDWords[9] + 0xa953fd4e;
            dd = (dd << 11 | dd >> (32 - 11)) + cc;
            aa = (aa << 10 | aa >> (32 - 10));

            cc += J(dd, ee, aa) + blockDWords[7] + 0xa953fd4e;
            cc = (cc << 6 | cc >> (32 - 6)) + bb;
            ee = (ee << 10 | ee >> (32 - 10));

            bb += J(cc, dd, ee) + blockDWords[12] + 0xa953fd4e;
            bb = (bb << 8 | bb >> (32 - 8)) + aa;
            dd = (dd << 10 | dd >> (32 - 10));

            aa += J(bb, cc, dd) + blockDWords[2] + 0xa953fd4e;
            aa = (aa << 13 | aa >> (32 - 13)) + ee;
            cc = (cc << 10 | cc >> (32 - 10));

            ee += J(aa, bb, cc) + blockDWords[10] + 0xa953fd4e;
            ee = (ee << 12 | ee >> (32 - 12)) + dd;
            bb = (bb << 10 | bb >> (32 - 10));

            dd += J(ee, aa, bb) + blockDWords[14] + 0xa953fd4e;
            dd = (dd << 5 | dd >> (32 - 5)) + cc;
            aa = (aa << 10 | aa >> (32 - 10));

            cc += J(dd, ee, aa) + blockDWords[1] + 0xa953fd4e;
            cc = (cc << 12 | cc >> (32 - 12)) + bb;
            ee = (ee << 10 | ee >> (32 - 10));

            bb += J(cc, dd, ee) + blockDWords[3] + 0xa953fd4e;
            bb = (bb << 13 | bb >> (32 - 13)) + aa;
            dd = (dd << 10 | dd >> (32 - 10));

            aa += J(bb, cc, dd) + blockDWords[8] + 0xa953fd4e;
            aa = (aa << 14 | aa >> (32 - 14)) + ee;
            cc = (cc << 10 | cc >> (32 - 10));

            ee += J(aa, bb, cc) + blockDWords[11] + 0xa953fd4e;
            ee = (ee << 11 | ee >> (32 - 11)) + dd;
            bb = (bb << 10 | bb >> (32 - 10));

            dd += J(ee, aa, bb) + blockDWords[6] + 0xa953fd4e;
            dd = (dd << 8 | dd >> (32 - 8)) + cc;
            aa = (aa << 10 | aa >> (32 - 10));

            cc += J(dd, ee, aa) + blockDWords[15] + 0xa953fd4e;
            cc = (cc << 5 | cc >> (32 - 5)) + bb;
            ee = (ee << 10 | ee >> (32 - 10));

            bb += J(cc, dd, ee) + blockDWords[13] + 0xa953fd4e;
            bb = (bb << 6 | bb >> (32 - 6)) + aa;
            dd = (dd << 10 | dd >> (32 - 10));


            aaa += J(bbb, ccc, ddd) + blockDWords[5] + 0x50a28be6;
            aaa = (aaa << 8 | aaa >> (32 - 8)) + eee;
            ccc = (ccc << 10 | ccc >> (32 - 10));

            eee += J(aaa, bbb, ccc) + blockDWords[14] + 0x50a28be6;
            eee = (eee << 9 | eee >> (32 - 9)) + ddd;
            bbb = (bbb << 10 | bbb >> (32 - 10));

            ddd += J(eee, aaa, bbb) + blockDWords[7] + 0x50a28be6;
            ddd = (ddd << 9 | ddd >> (32 - 9)) + ccc;
            aaa = (aaa << 10 | aaa >> (32 - 10));

            ccc += J(ddd, eee, aaa) + blockDWords[0] + 0x50a28be6;
            ccc = (ccc << 11 | ccc >> (32 - 11)) + bbb;
            eee = (eee << 10 | eee >> (32 - 10));

            bbb += J(ccc, ddd, eee) + blockDWords[9] + 0x50a28be6;
            bbb = (bbb << 13 | bbb >> (32 - 13)) + aaa;
            ddd = (ddd << 10 | ddd >> (32 - 10));

            aaa += J(bbb, ccc, ddd) + blockDWords[2] + 0x50a28be6;
            aaa = (aaa << 15 | aaa >> (32 - 15)) + eee;
            ccc = (ccc << 10 | ccc >> (32 - 10));

            eee += J(aaa, bbb, ccc) + blockDWords[11] + 0x50a28be6;
            eee = (eee << 15 | eee >> (32 - 15)) + ddd;
            bbb = (bbb << 10 | bbb >> (32 - 10));

            ddd += J(eee, aaa, bbb) + blockDWords[4] + 0x50a28be6;
            ddd = (ddd << 5 | ddd >> (32 - 5)) + ccc;
            aaa = (aaa << 10 | aaa >> (32 - 10));

            ccc += J(ddd, eee, aaa) + blockDWords[13] + 0x50a28be6;
            ccc = (ccc << 7 | ccc >> (32 - 7)) + bbb;
            eee = (eee << 10 | eee >> (32 - 10));

            bbb += J(ccc, ddd, eee) + blockDWords[6] + 0x50a28be6;
            bbb = (bbb << 7 | bbb >> (32 - 7)) + aaa;
            ddd = (ddd << 10 | ddd >> (32 - 10));

            aaa += J(bbb, ccc, ddd) + blockDWords[15] + 0x50a28be6;
            aaa = (aaa << 8 | aaa >> (32 - 8)) + eee;
            ccc = (ccc << 10 | ccc >> (32 - 10));

            eee += J(aaa, bbb, ccc) + blockDWords[8] + 0x50a28be6;
            eee = (eee << 11 | eee >> (32 - 11)) + ddd;
            bbb = (bbb << 10 | bbb >> (32 - 10));

            ddd += J(eee, aaa, bbb) + blockDWords[1] + 0x50a28be6;
            ddd = (ddd << 14 | ddd >> (32 - 14)) + ccc;
            aaa = (aaa << 10 | aaa >> (32 - 10));

            ccc += J(ddd, eee, aaa) + blockDWords[10] + 0x50a28be6;
            ccc = (ccc << 14 | ccc >> (32 - 14)) + bbb;
            eee = (eee << 10 | eee >> (32 - 10));

            bbb += J(ccc, ddd, eee) + blockDWords[3] + 0x50a28be6;
            bbb = (bbb << 12 | bbb >> (32 - 12)) + aaa;
            ddd = (ddd << 10 | ddd >> (32 - 10));

            aaa += J(bbb, ccc, ddd) + blockDWords[12] + 0x50a28be6;
            aaa = (aaa << 6 | aaa >> (32 - 6)) + eee;
            ccc = (ccc << 10 | ccc >> (32 - 10));


            eee += I(aaa, bbb, ccc) + blockDWords[6] + 0x5c4dd124;
            eee = (eee << 9 | eee >> (32 - 9)) + ddd;
            bbb = (bbb << 10 | bbb >> (32 - 10));

            ddd += I(eee, aaa, bbb) + blockDWords[11] + 0x5c4dd124;
            ddd = (ddd << 13 | ddd >> (32 - 13)) + ccc;
            aaa = (aaa << 10 | aaa >> (32 - 10));

            ccc += I(ddd, eee, aaa) + blockDWords[3] + 0x5c4dd124;
            ccc = (ccc << 15 | ccc >> (32 - 15)) + bbb;
            eee = (eee << 10 | eee >> (32 - 10));

            bbb += I(ccc, ddd, eee) + blockDWords[7] + 0x5c4dd124;
            bbb = (bbb << 7 | bbb >> (32 - 7)) + aaa;
            ddd = (ddd << 10 | ddd >> (32 - 10));

            aaa += I(bbb, ccc, ddd) + blockDWords[0] + 0x5c4dd124;
            aaa = (aaa << 12 | aaa >> (32 - 12)) + eee;
            ccc = (ccc << 10 | ccc >> (32 - 10));

            eee += I(aaa, bbb, ccc) + blockDWords[13] + 0x5c4dd124;
            eee = (eee << 8 | eee >> (32 - 8)) + ddd;
            bbb = (bbb << 10 | bbb >> (32 - 10));

            ddd += I(eee, aaa, bbb) + blockDWords[5] + 0x5c4dd124;
            ddd = (ddd << 9 | ddd >> (32 - 9)) + ccc;
            aaa = (aaa << 10 | aaa >> (32 - 10));

            ccc += I(ddd, eee, aaa) + blockDWords[10] + 0x5c4dd124;
            ccc = (ccc << 11 | ccc >> (32 - 11)) + bbb;
            eee = (eee << 10 | eee >> (32 - 10));

            bbb += I(ccc, ddd, eee) + blockDWords[14] + 0x5c4dd124;
            bbb = (bbb << 7 | bbb >> (32 - 7)) + aaa;
            ddd = (ddd << 10 | ddd >> (32 - 10));

            aaa += I(bbb, ccc, ddd) + blockDWords[15] + 0x5c4dd124;
            aaa = (aaa << 7 | aaa >> (32 - 7)) + eee;
            ccc = (ccc << 10 | ccc >> (32 - 10));

            eee += I(aaa, bbb, ccc) + blockDWords[8] + 0x5c4dd124;
            eee = (eee << 12 | eee >> (32 - 12)) + ddd;
            bbb = (bbb << 10 | bbb >> (32 - 10));

            ddd += I(eee, aaa, bbb) + blockDWords[12] + 0x5c4dd124;
            ddd = (ddd << 7 | ddd >> (32 - 7)) + ccc;
            aaa = (aaa << 10 | aaa >> (32 - 10));

            ccc += I(ddd, eee, aaa) + blockDWords[4] + 0x5c4dd124;
            ccc = (ccc << 6 | ccc >> (32 - 6)) + bbb;
            eee = (eee << 10 | eee >> (32 - 10));

            bbb += I(ccc, ddd, eee) + blockDWords[9] + 0x5c4dd124;
            bbb = (bbb << 15 | bbb >> (32 - 15)) + aaa;
            ddd = (ddd << 10 | ddd >> (32 - 10));

            aaa += I(bbb, ccc, ddd) + blockDWords[1] + 0x5c4dd124;
            aaa = (aaa << 13 | aaa >> (32 - 13)) + eee;
            ccc = (ccc << 10 | ccc >> (32 - 10));

            eee += I(aaa, bbb, ccc) + blockDWords[2] + 0x5c4dd124;
            eee = (eee << 11 | eee >> (32 - 11)) + ddd;
            bbb = (bbb << 10 | bbb >> (32 - 10));


            ddd += H(eee, aaa, bbb) + blockDWords[15] + 0x6d703ef3;
            ddd = (ddd << 9 | ddd >> (32 - 9)) + ccc;
            aaa = (aaa << 10 | aaa >> (32 - 10));

            ccc += H(ddd, eee, aaa) + blockDWords[5] + 0x6d703ef3;
            ccc = (ccc << 7 | ccc >> (32 - 7)) + bbb;
            eee = (eee << 10 | eee >> (32 - 10));

            bbb += H(ccc, ddd, eee) + blockDWords[1] + 0x6d703ef3;
            bbb = (bbb << 15 | bbb >> (32 - 15)) + aaa;
            ddd = (ddd << 10 | ddd >> (32 - 10));

            aaa += H(bbb, ccc, ddd) + blockDWords[3] + 0x6d703ef3;
            aaa = (aaa << 11 | aaa >> (32 - 11)) + eee;
            ccc = (ccc << 10 | ccc >> (32 - 10));

            eee += H(aaa, bbb, ccc) + blockDWords[7] + 0x6d703ef3;
            eee = (eee << 8 | eee >> (32 - 8)) + ddd;
            bbb = (bbb << 10 | bbb >> (32 - 10));

            ddd += H(eee, aaa, bbb) + blockDWords[14] + 0x6d703ef3;
            ddd = (ddd << 6 | ddd >> (32 - 6)) + ccc;
            aaa = (aaa << 10 | aaa >> (32 - 10));

            ccc += H(ddd, eee, aaa) + blockDWords[6] + 0x6d703ef3;
            ccc = (ccc << 6 | ccc >> (32 - 6)) + bbb;
            eee = (eee << 10 | eee >> (32 - 10));

            bbb += H(ccc, ddd, eee) + blockDWords[9] + 0x6d703ef3;
            bbb = (bbb << 14 | bbb >> (32 - 14)) + aaa;
            ddd = (ddd << 10 | ddd >> (32 - 10));

            aaa += H(bbb, ccc, ddd) + blockDWords[11] + 0x6d703ef3;
            aaa = (aaa << 12 | aaa >> (32 - 12)) + eee;
            ccc = (ccc << 10 | ccc >> (32 - 10));

            eee += H(aaa, bbb, ccc) + blockDWords[8] + 0x6d703ef3;
            eee = (eee << 13 | eee >> (32 - 13)) + ddd;
            bbb = (bbb << 10 | bbb >> (32 - 10));

            ddd += H(eee, aaa, bbb) + blockDWords[12] + 0x6d703ef3;
            ddd = (ddd << 5 | ddd >> (32 - 5)) + ccc;
            aaa = (aaa << 10 | aaa >> (32 - 10));

            ccc += H(ddd, eee, aaa) + blockDWords[2] + 0x6d703ef3;
            ccc = (ccc << 14 | ccc >> (32 - 14)) + bbb;
            eee = (eee << 10 | eee >> (32 - 10));

            bbb += H(ccc, ddd, eee) + blockDWords[10] + 0x6d703ef3;
            bbb = (bbb << 13 | bbb >> (32 - 13)) + aaa;
            ddd = (ddd << 10 | ddd >> (32 - 10));

            aaa += H(bbb, ccc, ddd) + blockDWords[0] + 0x6d703ef3;
            aaa = (aaa << 13 | aaa >> (32 - 13)) + eee;
            ccc = (ccc << 10 | ccc >> (32 - 10));

            eee += H(aaa, bbb, ccc) + blockDWords[4] + 0x6d703ef3;
            eee = (eee << 7 | eee >> (32 - 7)) + ddd;
            bbb = (bbb << 10 | bbb >> (32 - 10));

            ddd += H(eee, aaa, bbb) + blockDWords[13] + 0x6d703ef3;
            ddd = (ddd << 5 | ddd >> (32 - 5)) + ccc;
            aaa = (aaa << 10 | aaa >> (32 - 10));


            ccc += G(ddd, eee, aaa) + blockDWords[8] + 0x7a6d76e9;
            ccc = (ccc << 15 | ccc >> (32 - 15)) + bbb;
            eee = (eee << 10 | eee >> (32 - 10));

            bbb += G(ccc, ddd, eee) + blockDWords[6] + 0x7a6d76e9;
            bbb = (bbb << 5 | bbb >> (32 - 5)) + aaa;
            ddd = (ddd << 10 | ddd >> (32 - 10));

            aaa += G(bbb, ccc, ddd) + blockDWords[4] + 0x7a6d76e9;
            aaa = (aaa << 8 | aaa >> (32 - 8)) + eee;
            ccc = (ccc << 10 | ccc >> (32 - 10));

            eee += G(aaa, bbb, ccc) + blockDWords[1] + 0x7a6d76e9;
            eee = (eee << 11 | eee >> (32 - 11)) + ddd;
            bbb = (bbb << 10 | bbb >> (32 - 10));

            ddd += G(eee, aaa, bbb) + blockDWords[3] + 0x7a6d76e9;
            ddd = (ddd << 14 | ddd >> (32 - 14)) + ccc;
            aaa = (aaa << 10 | aaa >> (32 - 10));

            ccc += G(ddd, eee, aaa) + blockDWords[11] + 0x7a6d76e9;
            ccc = (ccc << 14 | ccc >> (32 - 14)) + bbb;
            eee = (eee << 10 | eee >> (32 - 10));

            bbb += G(ccc, ddd, eee) + blockDWords[15] + 0x7a6d76e9;
            bbb = (bbb << 6 | bbb >> (32 - 6)) + aaa;
            ddd = (ddd << 10 | ddd >> (32 - 10));

            aaa += G(bbb, ccc, ddd) + blockDWords[0] + 0x7a6d76e9;
            aaa = (aaa << 14 | aaa >> (32 - 14)) + eee;
            ccc = (ccc << 10 | ccc >> (32 - 10));

            eee += G(aaa, bbb, ccc) + blockDWords[5] + 0x7a6d76e9;
            eee = (eee << 6 | eee >> (32 - 6)) + ddd;
            bbb = (bbb << 10 | bbb >> (32 - 10));

            ddd += G(eee, aaa, bbb) + blockDWords[12] + 0x7a6d76e9;
            ddd = (ddd << 9 | ddd >> (32 - 9)) + ccc;
            aaa = (aaa << 10 | aaa >> (32 - 10));

            ccc += G(ddd, eee, aaa) + blockDWords[2] + 0x7a6d76e9;
            ccc = (ccc << 12 | ccc >> (32 - 12)) + bbb;
            eee = (eee << 10 | eee >> (32 - 10));

            bbb += G(ccc, ddd, eee) + blockDWords[13] + 0x7a6d76e9;
            bbb = (bbb << 9 | bbb >> (32 - 9)) + aaa;
            ddd = (ddd << 10 | ddd >> (32 - 10));

            aaa += G(bbb, ccc, ddd) + blockDWords[9] + 0x7a6d76e9;
            aaa = (aaa << 12 | aaa >> (32 - 12)) + eee;
            ccc = (ccc << 10 | ccc >> (32 - 10));

            eee += G(aaa, bbb, ccc) + blockDWords[7] + 0x7a6d76e9;
            eee = (eee << 5 | eee >> (32 - 5)) + ddd;
            bbb = (bbb << 10 | bbb >> (32 - 10));

            ddd += G(eee, aaa, bbb) + blockDWords[10] + 0x7a6d76e9;
            ddd = (ddd << 15 | ddd >> (32 - 15)) + ccc;
            aaa = (aaa << 10 | aaa >> (32 - 10));

            ccc += G(ddd, eee, aaa) + blockDWords[14] + 0x7a6d76e9;
            ccc = (ccc << 8 | ccc >> (32 - 8)) + bbb;
            eee = (eee << 10 | eee >> (32 - 10));


            bbb += F(ccc, ddd, eee) + blockDWords[12];
            bbb = (bbb << 8 | bbb >> (32 - 8)) + aaa;
            ddd = (ddd << 10 | ddd >> (32 - 10));

            aaa += F(bbb, ccc, ddd) + blockDWords[15];
            aaa = (aaa << 5 | aaa >> (32 - 5)) + eee;
            ccc = (ccc << 10 | ccc >> (32 - 10));

            eee += F(aaa, bbb, ccc) + blockDWords[10];
            eee = (eee << 12 | eee >> (32 - 12)) + ddd;
            bbb = (bbb << 10 | bbb >> (32 - 10));

            ddd += F(eee, aaa, bbb) + blockDWords[4];
            ddd = (ddd << 9 | ddd >> (32 - 9)) + ccc;
            aaa = (aaa << 10 | aaa >> (32 - 10));

            ccc += F(ddd, eee, aaa) + blockDWords[1];
            ccc = (ccc << 12 | ccc >> (32 - 12)) + bbb;
            eee = (eee << 10 | eee >> (32 - 10));

            bbb += F(ccc, ddd, eee) + blockDWords[5];
            bbb = (bbb << 5 | bbb >> (32 - 5)) + aaa;
            ddd = (ddd << 10 | ddd >> (32 - 10));

            aaa += F(bbb, ccc, ddd) + blockDWords[8];
            aaa = (aaa << 14 | aaa >> (32 - 14)) + eee;
            ccc = (ccc << 10 | ccc >> (32 - 10));

            eee += F(aaa, bbb, ccc) + blockDWords[7];
            eee = (eee << 6 | eee >> (32 - 6)) + ddd;
            bbb = (bbb << 10 | bbb >> (32 - 10));

            ddd += F(eee, aaa, bbb) + blockDWords[6];
            ddd = (ddd << 8 | ddd >> (32 - 8)) + ccc;
            aaa = (aaa << 10 | aaa >> (32 - 10));

            ccc += F(ddd, eee, aaa) + blockDWords[2];
            ccc = (ccc << 13 | ccc >> (32 - 13)) + bbb;
            eee = (eee << 10 | eee >> (32 - 10));

            bbb += F(ccc, ddd, eee) + blockDWords[13];
            bbb = (bbb << 6 | bbb >> (32 - 6)) + aaa;
            ddd = (ddd << 10 | ddd >> (32 - 10));

            aaa += F(bbb, ccc, ddd) + blockDWords[14];
            aaa = (aaa << 5 | aaa >> (32 - 5)) + eee;
            ccc = (ccc << 10 | ccc >> (32 - 10));

            eee += F(aaa, bbb, ccc) + blockDWords[0];
            eee = (eee << 15 | eee >> (32 - 15)) + ddd;
            bbb = (bbb << 10 | bbb >> (32 - 10));

            ddd += F(eee, aaa, bbb) + blockDWords[3];
            ddd = (ddd << 13 | ddd >> (32 - 13)) + ccc;
            aaa = (aaa << 10 | aaa >> (32 - 10));

            ccc += F(ddd, eee, aaa) + blockDWords[9];
            ccc = (ccc << 11 | ccc >> (32 - 11)) + bbb;
            eee = (eee << 10 | eee >> (32 - 10));

            bbb += F(ccc, ddd, eee) + blockDWords[11];
            bbb = (bbb << 11 | bbb >> (32 - 11)) + aaa;
            ddd = (ddd << 10 | ddd >> (32 - 10));

            ddd += cc + state[1];
            state[1] = state[2] + dd + eee;
            state[2] = state[3] + ee + aaa;
            state[3] = state[4] + aa + bbb;
            state[4] = state[0] + bb + ccc;
            state[0] = ddd;
        }

        private static uint F(uint x, uint y, uint z)
        {
            return (x ^ y ^ z);
        }

        private static uint G(uint x, uint y, uint z)
        {
            return ((x & y) | (~x & z));
        }

        private static uint H(uint x, uint y, uint z)
        {
            return ((x | ~y) ^ z);
        }

        private static uint I(uint x, uint y, uint z)
        {
            return ((x & z) | (y & ~z));
        }

        private static uint J(uint x, uint y, uint z)
        {
            return (x ^ (y | ~z));
        }

        [SecurityCritical]  // auto-generated
        private unsafe static void DWORDFromLittleEndian(uint* x, int digits, byte* block)
        {
            int i;
            int j;

            for (i = 0, j = 0; i < digits; i++, j += 4)
                x[i] = (uint)(block[j] | (block[j + 1] << 8) | (block[j + 2] << 16) | (block[j + 3] << 24));
        }

        private static void DWORDToLittleEndian(byte[] block, uint[] x, int digits)
        {
            int i;
            int j;

            for (i = 0, j = 0; i < digits; i++, j += 4)
            {
                block[j] = (byte)(x[i] & 0xff);
                block[j + 1] = (byte)((x[i] >> 8) & 0xff);
                block[j + 2] = (byte)((x[i] >> 16) & 0xff);
                block[j + 3] = (byte)((x[i] >> 24) & 0xff);
            }
        }
    }
}
#endif

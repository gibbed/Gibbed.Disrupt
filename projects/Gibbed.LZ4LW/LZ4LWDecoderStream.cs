#define CHECK_EOF

using System;
using System.IO;

namespace LZ4LW
{
    public class LZ4LWDecoderStream : Stream
    {
        public LZ4LWDecoderStream()
        {
        }

        public LZ4LWDecoderStream(Stream input, long inputLength)
        {
            Reset(input, inputLength);
        }

        public void Reset(Stream input, long inputLength)
        {
            this._OriginalInputLength = inputLength;
            this._InputLength = inputLength;
            this._Input = input;

            _Phase = DecodePhase.ReadToken;

            _DecodeBuffer = new byte[DecBufLen + _InBufLen];
            _DecodeBufferPos = 0;

            _LiteralLength = 0;
            _MatchLength = 0;
            _MatchDestination = 0;

            _InBufPos = DecBufLen;
            _InBufEnd = DecBufLen;
        }

        public override void Close()
        {
            this._Input = null;
        }

        private long _OriginalInputLength;
        private long _InputLength;
        private Stream _Input;

        //because we might not be able to match back across invocations,
        //we have to keep the last window's worth of bytes around for reuse
        //we use a circular buffer for this - every time we write into this
        //buffer, we also write the same into our output buffer

        private const int DecBufLen = 0x400000;
        private const int DecBufMask = 0x3FFFFF;

        private const int _InBufLen = 128;

        private byte[] _DecodeBuffer;
        private int _DecodeBufferPos;
        private int _InBufPos;
        private int _InBufEnd;

        //we keep track of which phase we're in so that we can jump right back
        //into the correct part of decoding

        private DecodePhase _Phase;

        private enum DecodePhase
        {
            ReadToken,
            ReadExLiteralLength,
            CopyLiteral,
            ReadOffset,
            ReadExOffset,
            ReadExMatchLength,
            CopyMatch,
        }

        //state within interruptable phases and across phase boundaries is
        //kept here - again, so that we can punt out and restart freely

        private int _LiteralLength;
        private int _MatchLength;
        private int _MatchDestination;

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (buffer.Length - count < offset)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (_Input == null)
            {
                throw new InvalidOperationException();
            }

            int nRead, nToRead = count;

            var decBuf = _DecodeBuffer;

            //the stringy gotos are obnoxious, but their purpose is to
            //make it *blindingly* obvious how the state machine transitions
            //back and forth as it reads - remember, we can yield out of
            //this routine in several places, and we must be able to re-enter
            //and pick up where we left off!

            switch (_Phase)
            {
                case DecodePhase.ReadToken: goto readToken;
                case DecodePhase.ReadExLiteralLength: goto readExLiteralLength;
                case DecodePhase.CopyLiteral: goto copyLiteral;
                case DecodePhase.ReadOffset: goto readOffset;
                case DecodePhase.ReadExOffset: goto readExOffset;
                case DecodePhase.ReadExMatchLength: goto readExMatchLength;
                case DecodePhase.CopyMatch: goto copyMatch;
            }

        readToken:
            int token;
            if (_InBufPos < _InBufEnd)
            {
                token = decBuf[_InBufPos++];
            }
            else
            {
                token = ReadByteCore();
#if CHECK_EOF
                if (token == -1)
                {
                    goto finish;
                }
#endif
            }

            //Console.WriteLine("{0:X2} - @{1} ({2})", token, _OriginalInputLength - (_InputLength + (_InBufEnd - _InBufPos)) - 1, _InputLength + (_InBufEnd - _InBufPos));

            if ((token == 0x07 && (_InputLength + (_InBufEnd - _InBufPos)) == 20) ||
              (token == 0x07 && (_InputLength + (_InBufEnd - _InBufPos)) == 2))
            {
                //goto finish;
            }

            _LiteralLength = token >> 4;
            _MatchLength = (token & 0xF) + 4;

            switch (_LiteralLength)
            {
                case 0:
                {
                    _Phase = DecodePhase.ReadOffset;
                    goto readOffset;
                }

                case 0xF:
                {
                    _Phase = DecodePhase.ReadExLiteralLength;
                    goto readExLiteralLength;
                }

                default:
                {
                    _Phase = DecodePhase.CopyLiteral;
                    goto copyLiteral;
                }
            }

        readExLiteralLength:
            int extendedLiteralLength;
            if (_InBufPos < _InBufEnd)
            {
                extendedLiteralLength = decBuf[_InBufPos++];
            }
            else
            {
                extendedLiteralLength = ReadByteCore();

#if CHECK_EOF
                if (extendedLiteralLength == -1)
                {
                    goto finish;
                }
#endif
            }

            _LiteralLength += extendedLiteralLength;
            if (extendedLiteralLength == 255)
            {
                goto readExLiteralLength;
            }

            _Phase = DecodePhase.CopyLiteral;
            goto copyLiteral;

        copyLiteral:
            int nReadLit = _LiteralLength < nToRead ? _LiteralLength : nToRead;
            if (nReadLit != 0)
            {
                if (_InBufPos + nReadLit <= _InBufEnd)
                {
                    int ofs = offset;

                    for (int c = nReadLit; c-- != 0;)
                        buffer[ofs++] = decBuf[_InBufPos++];

                    nRead = nReadLit;
                }
                else
                {
                    nRead = ReadCore(buffer, offset, nReadLit);
#if CHECK_EOF
                    if (nRead == 0)
                        goto finish;
#endif
                }

                offset += nRead;
                nToRead -= nRead;

                _LiteralLength -= nRead;

                if (_LiteralLength != 0)
                    goto copyLiteral;
            }

            if (nToRead == 0)
                goto finish;

            _Phase = DecodePhase.ReadOffset;
            goto readOffset;

        readOffset:
            if (_InBufPos + 1 < _InBufEnd)
            {
                _MatchDestination = (decBuf[_InBufPos + 1] << 8) | decBuf[_InBufPos];
                _InBufPos += 2;
            }
            else
            {
                _MatchDestination = ReadOffsetCore();
#if CHECK_EOF
                if (_MatchDestination == -1)
                    goto finish;
#endif
            }

            if (_MatchDestination >= 0xE000)
            {
                _Phase = DecodePhase.ReadExOffset;
                goto readExOffset;
            }

            if (_MatchLength == 15 + 4)
            {
                _Phase = DecodePhase.ReadExMatchLength;
                goto readExMatchLength;
            }
            else
            {
                _Phase = DecodePhase.CopyMatch;
                goto copyMatch;
            }

        readExOffset:
            int exOffset;
            if (_InBufPos < _InBufEnd)
            {
                exOffset = decBuf[_InBufPos++];
            }
            else
            {
                exOffset = ReadByteCore();
#if CHECK_EOF
                if (exOffset == -1)
                    goto finish;
#endif
            }

            _MatchDestination += exOffset << 13;

            if (_MatchLength == 15 + 4)
            {
                _Phase = DecodePhase.ReadExMatchLength;
                goto readExMatchLength;
            }
            else
            {
                _Phase = DecodePhase.CopyMatch;
                goto copyMatch;
            }

        readExMatchLength:
            int exMatLen;
            if (_InBufPos < _InBufEnd)
            {
                exMatLen = decBuf[_InBufPos++];
            }
            else
            {
                exMatLen = ReadByteCore();
#if CHECK_EOF
                if (exMatLen == -1)
                    goto finish;
#endif
            }

            _MatchLength += exMatLen;
            if (exMatLen == 255)
                goto readExMatchLength;

            _Phase = DecodePhase.CopyMatch;
            goto copyMatch;

        copyMatch:
            int nCpyMat = _MatchLength < nToRead ? _MatchLength : nToRead;
            if (nCpyMat != 0)
            {
                nRead = count - nToRead;

                int bufDst = _MatchDestination - nRead;
                if (bufDst > 0)
                {
                    //offset is fairly far back, we need to pull from the buffer

                    int bufSrc = _DecodeBufferPos - bufDst;
                    if (bufSrc < 0)
                        bufSrc += DecBufLen;
                    int bufCnt = bufDst < nCpyMat ? bufDst : nCpyMat;

                    for (int c = bufCnt; c-- != 0;)
                        buffer[offset++] = decBuf[bufSrc++];
                }
                else
                {
                    bufDst = 0;
                }

                int sOfs = offset - _MatchDestination;
                for (int i = bufDst; i < nCpyMat; i++)
                    buffer[offset++] = buffer[sOfs++];

                nToRead -= nCpyMat;
                _MatchLength -= nCpyMat;
            }

            if (nToRead == 0)
                goto finish;

            _Phase = DecodePhase.ReadToken;
            goto readToken;

        finish:
            nRead = count - nToRead;

            int nToBuf = nRead < DecBufLen ? nRead : DecBufLen;
            int repPos = offset - nToBuf;

            if (nToBuf == DecBufLen)
            {
                Buffer.BlockCopy(buffer, repPos, decBuf, 0, DecBufLen);
                _DecodeBufferPos = 0;
            }
            else
            {
                int decPos = _DecodeBufferPos;

                while (nToBuf-- != 0)
                    decBuf[decPos++] = buffer[repPos++];

                _DecodeBufferPos = decPos;
            }

            return nRead;
        }

        private int ReadByteCore()
        {
            var buf = _DecodeBuffer;

            if (_InBufPos == _InBufEnd)
            {
                int nRead = _Input.Read(buf, DecBufLen,
                  _InBufLen < _InputLength ? _InBufLen : (int)_InputLength);

#if CHECK_EOF
                if (nRead == 0)
                    return -1;
#endif

                _InputLength -= nRead;

                _InBufPos = DecBufLen;
                _InBufEnd = DecBufLen + nRead;
            }

            return buf[_InBufPos++];
        }

        private int ReadOffsetCore()
        {
            var buf = _DecodeBuffer;

            if (_InBufPos == _InBufEnd)
            {
                int nRead = _Input.Read(buf, DecBufLen,
                  _InBufLen < _InputLength ? _InBufLen : (int)_InputLength);

#if CHECK_EOF
                if (nRead == 0)
                    return -1;
#endif

                _InputLength -= nRead;

                _InBufPos = DecBufLen;
                _InBufEnd = DecBufLen + nRead;
            }

            if (_InBufEnd - _InBufPos == 1)
            {
                buf[DecBufLen] = buf[_InBufPos];

                int nRead = _Input.Read(buf, DecBufLen + 1,
                  _InBufLen - 1 < _InputLength ? _InBufLen - 1 : (int)_InputLength);

#if CHECK_EOF
                if (nRead == 0)
                {
                    _InBufPos = DecBufLen;
                    _InBufEnd = DecBufLen + 1;

                    return -1;
                }
#endif

                _InputLength -= nRead;

                _InBufPos = DecBufLen;
                _InBufEnd = DecBufLen + nRead + 1;
            }

            int ret = (buf[_InBufPos + 1] << 8) | buf[_InBufPos];
            _InBufPos += 2;

            return ret;
        }

        private int ReadCore(byte[] buffer, int offset, int count)
        {
            int nToRead = count;

            var buf = _DecodeBuffer;
            int inBufLen = _InBufEnd - _InBufPos;

            int fromBuf = nToRead < inBufLen ? nToRead : inBufLen;
            if (fromBuf != 0)
            {
                var bufPos = _InBufPos;

                for (int c = fromBuf; c-- != 0;)
                    buffer[offset++] = buf[bufPos++];

                _InBufPos = bufPos;
                nToRead -= fromBuf;
            }

            if (nToRead != 0)
            {
                int nRead;

                if (nToRead >= _InBufLen)
                {
                    nRead = _Input.Read(buffer, offset,
                      nToRead < _InputLength ? nToRead : (int)_InputLength);
                    nToRead -= nRead;
                }
                else
                {
                    nRead = _Input.Read(buf, DecBufLen,
                      _InBufLen < _InputLength ? _InBufLen : (int)_InputLength);

                    _InBufPos = DecBufLen;
                    _InBufEnd = DecBufLen + nRead;

                    fromBuf = nToRead < nRead ? nToRead : nRead;

                    var bufPos = _InBufPos;

                    for (int c = fromBuf; c-- != 0;)
                        buffer[offset++] = buf[bufPos++];

                    _InBufPos = bufPos;
                    nToRead -= fromBuf;
                }

                _InputLength -= nRead;
            }

            return count - nToRead;
        }

        #region Stream internals

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}

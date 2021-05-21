// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Yarp.ReverseProxy.Utilities
{
    internal class Crc32
    {
        // Table of CRCs of all 8-bit messages.
        private static readonly ulong[] _crcTable = new ulong[256];

        static Crc32()
        {
            _crcTable = MakeCrcTable();
        }

        // Update a running CRC with the bytes --the CRC
        // should be initialized to all 1's, and the transmitted value
        // is the 1's complement of the final running CRC (see the
        // crc() routine below)).
        public static ulong UpdateCRC(ulong crc, ReadOnlySpan<byte> buf)
        {
            var tmp = crc;
            for (var i = 0; i < buf.Length; i++)
            {
                tmp = _crcTable[(tmp ^ buf[i]) & 0xff] ^ (tmp >> 8);
            }
            return tmp;
        }

        public static ulong CalculateCRC(ReadOnlySpan<byte> buf) => UpdateCRC(0xffffffffL, buf) ^ 0xffffffffL;

        // Make the table for a fast CRC.
        // Derivative work of zlib -- https://github.com/madler/zlib/blob/master/crc32.c (hint: L108)
        private static ulong[] MakeCrcTable()
        {
            var result = new ulong[256];

            for (ulong i = 0; i < 256; i++)
            {
                var tmp = i;
                for (var k = 0; k < 8; k++)
                {
                    if ((tmp & 1) > 0)
                    {
                        tmp = 0xedb88320L ^ (tmp >> 1);
                    }
                    else
                    {
                        tmp = tmp >> 1;
                    }
                }
                result[i] = tmp;
            }

            return result;
        }
    }
}
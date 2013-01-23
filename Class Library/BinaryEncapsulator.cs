using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyFa.Properties;
using System.IO;
using System.Collections.ObjectModel;
using System.Runtime.Serialization.Formatters.Binary;

namespace MyFa
{
    public class BinaryEncapsulator
    {
		private static Byte[] CRCTable = {
			0, 94, 188, 226, 97, 63, 221, 131, 194, 156, 126, 32, 163, 253, 31, 65,
			157, 195, 33, 127, 252, 162, 64, 30, 95, 1, 227, 189, 62, 96, 130, 220,
			35, 125, 159, 193, 66, 28, 254, 160, 225, 191, 93, 3, 128, 222, 60, 98,
			190, 224, 2, 92, 223, 129, 99, 61, 124, 34, 192, 158, 29, 67, 161, 255,
			70, 24, 250, 164, 39, 121, 155, 197, 132, 218, 56, 102, 229, 187, 89, 7,
			219, 133, 103, 57, 186, 228, 6, 88, 25, 71, 165, 251, 120, 38, 196, 154,
			101, 59, 217, 135, 4, 90, 184, 230, 167, 249, 27, 69, 198, 152, 122, 36,
			248, 166, 68, 26, 153, 199, 37, 123, 58, 100, 134, 216, 91, 5, 231, 185,
			140, 210, 48, 110, 237, 179, 81, 15, 78, 16, 242, 172, 47, 113, 147, 205,
			17, 79, 173, 243, 112, 46, 204, 146, 211, 141, 111, 49, 178, 236, 14, 80,
			175, 241, 19, 77, 206, 144, 114, 44, 109, 51, 209, 143, 12, 82, 176, 238,
			50, 108, 142, 208, 83, 13, 239, 177, 240, 174, 76, 18, 145, 207, 45, 115,
			202, 148, 118, 40, 171, 245, 23, 73, 8, 86, 180, 234, 105, 55, 213, 139,
			87, 9, 235, 181, 54, 104, 138, 212, 149, 203, 41, 119, 244, 170, 72, 22,
			233, 183, 85, 11, 136, 214, 52, 106, 43, 117, 151, 201, 74, 20, 246, 168,
			116, 42, 200, 150, 21, 75, 169, 247, 182, 232, 10, 84, 215, 137, 107, 53 };
		private static Byte PacketSize = 128;
	
		public static Byte Encapsulate(Stream source, Stream destination)
		{
            BinaryReader src = new BinaryReader(source);
			BinaryWriter dst = new BinaryWriter(destination);
            Byte L = 23, ACK = 79, DL = 21, C = 1, CRC = 0, GeneralCRC = 0;
            Byte[] tmpBytes;
            int count = (int)Math.Ceiling(((double)source.Length - 20f) / (double)PacketSize) - 1;

            dst.Write(L);
            dst.Write(ACK);
            dst.Write(DL);
            dst.Write(C);

            tmpBytes = src.ReadBytes(20);
            CRC = CRCTable[0 ^ DL];
            CRC = CRCTable[CRC ^ C];
            foreach (Byte tmpByte in tmpBytes)
            {
                GeneralCRC = CRCTable[GeneralCRC ^ tmpByte];
                CRC = CRCTable[CRC ^ tmpByte];
                dst.Write(tmpByte);
            }
            dst.Write(CRC);

            L = (Byte)(PacketSize + 3);
            DL = (Byte)(L - 2);
            C = 2;

			for (int i = 0; i < count; ++i)
			{
                dst.Write(L);
                dst.Write(ACK);
                dst.Write(DL);
                dst.Write(C);

                tmpBytes = src.ReadBytes(PacketSize);
                CRC = CRCTable[0 ^ DL];
                CRC = CRCTable[CRC ^ C];
                foreach (Byte tmpByte in tmpBytes)
                {
                    GeneralCRC = CRCTable[GeneralCRC ^ tmpByte];
                    CRC = CRCTable[CRC ^ tmpByte];
                    dst.Write(tmpByte);
                }

                dst.Write(CRC);
			}

            L = (Byte)(source.Length - source.Position + 3);
            DL = (Byte)(L - 2);
            if (tmpBytes.Length < PacketSize) C = 5;

            dst.Write(L);
            dst.Write(ACK);
            dst.Write(DL);
            dst.Write(C);

            tmpBytes = src.ReadBytes((int)(source.Length - source.Position));
            CRC = CRCTable[0 ^ DL];
            CRC = CRCTable[CRC ^ C];
            foreach (Byte tmpByte in tmpBytes)
            {
                GeneralCRC = CRCTable[GeneralCRC ^ tmpByte];
                CRC = CRCTable[CRC ^ tmpByte];

                dst.Write(tmpByte);
            }

            if (C == 5)
            {
                CRC = CRCTable[CRC ^ GeneralCRC];
                dst.Write(GeneralCRC);

                dst.Write(CRC);
            }
            else
            {
                dst.Write(CRC);

                L = (Byte)(4);
                DL = (Byte)(2);
                C = 5;

                dst.Write(L);
                dst.Write(ACK);
                dst.Write(DL);
                dst.Write(C);

                CRC = CRCTable[0 ^ DL];
                CRC = CRCTable[CRC ^ C];
                CRC = CRCTable[CRC ^ GeneralCRC];
                dst.Write(GeneralCRC);

                dst.Write(CRC);
            }

            dst.Write((Byte)0);

            return GeneralCRC;
		}
	}
}
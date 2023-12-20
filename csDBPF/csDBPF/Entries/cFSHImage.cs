// Decompiled with JetBrains decompiler
// Type: RVT.SC4.FSHLib.cFSHImage
// Assembly: rvtvbFSH, Version=4.8.0.26200, Culture=neutral, PublicKeyToken=null
// MVID: 33702902-9135-4B05-B423-D8489700E80D
// Assembly location: C:\Program Files (x86)\SC4 Utilities\GoFSH Package\rvtvbFSH.dll

using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Xml.Linq;

namespace RVT.SC4.FSHLib {
    public class cFSHImage {
        private bool zIsQFSComp;
        private int[] zGlobalPalette;
        private bool zHaveGlobalPal;
        public byte[] zFSHData;
        private string zFSHFilePath;
        private cFSHImage.FSHFile zFSHFile;
        private string zSC4Type;
        private string zSC4Group;
        private string zSC4Instance;
        private int zUncompressedSize;
        private int zCompressedSize;
        private const int DXTC4 = 1;
        private const int DXTC3 = 2;
        private byte[] BestOne;

        public cFSHImage() {
            zUncompressedSize = 0;
            zCompressedSize = 0;
            BestOne = new byte[2048] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 8, 0, 0, 0, 4, 4, 0, 0, 8, 8, 0, 4, 4, 4, 0, 0, 8, 8, 0, 4, 4, 4, 4, 0, 8, 8, 0, 4, 4, 8, 0, 0, 8, 8, 0, 4, 8, 8, 4, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 12, 4, 8, 16, 16, 0, 8, 12, 16, 0, 8, 16, 16, 0, 12, 12, 16, 0, 8, 16, 16, 8, 12, 12, 12, 12, 8, 16, 16, 8, 12, 12, 20, 0, 8, 16, 16, 8, 12, 16, 20, 4, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 24, 4, 16, 24, 24, 8, 16, 20, 28, 0, 16, 24, 24, 8, 20, 20, 28, 0, 16, 24, 24, 16, 20, 20, 20, 20, 16, 24, 24, 16, 20, 20, 32, 0, 16, 24, 33, 0, 20, 24, 32, 4, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 36, 4, 24, 33, 41, 0, 24, 28, 40, 0, 24, 33, 41, 0, 28, 28, 40, 0, 24, 33, 41, 0, 28, 28, 28, 28, 24, 33, 41, 8, 28, 28, 44, 0, 24, 33, 41, 8, 28, 32, 44, 4, 24, 41, 49, 0, 32, 32, 32, 32, 24, 41, 49, 0, 32, 32, 32, 32, 33, 33, 33, 33, 32, 32, 48, 4, 33, 33, 33, 33, 32, 36, 52, 0, 24, 49, 49, 8, 36, 36, 52, 0, 24, 49, 49, 8, 36, 36, 36, 36, 33, 41, 57, 0, 36, 36, 56, 0, 33, 41, 57, 0, 36, 40, 56, 4, 24, 57, 57, 0, 40, 40, 40, 40, 24, 57, 41, 41, 40, 40, 40, 40, 41, 41, 41, 41, 40, 40, 60, 4, 41, 41, 57, 16, 40, 44, 60, 8, 41, 49, 57, 16, 44, 44, 65, 0, 41, 49, 66, 0, 44, 44, 44, 44, 41, 49, 66, 0, 44, 44, 48, 40, 41, 49, 66, 8, 44, 48, 69, 0, 41, 49, 66, 8, 48, 48, 69, 4, 49, 49, 49, 49, 48, 48, 48, 48, 49, 49, 49, 49, 48, 48, 52, 44, 49, 49, 49, 49, 48, 52, 73, 4, 49, 57, 57, 41, 52, 52, 77, 0, 49, 57, 74, 8, 52, 52, 52, 52, 49, 57, 82, 0, 52, 52, 56, 48, 49, 57, 82, 0, 52, 56, 81, 0, 49, 57, 66, 33, 56, 56, 81, 4, 57, 57, 66, 33, 56, 56, 56, 56, 57, 57, 57, 57, 56, 56, 60, 52, 57, 57, 57, 57, 56, 60, 85, 4, 57, 66, 90, 0, 60, 60, 89, 0, 57, 66, 90, 0, 60, 60, 60, 60, 57, 66, 90, 0, 60, 60, 60, 60, 57, 66, 90, 8, 60, 65, 93, 0, 57, 66, 74, 41, 60, 69, 93, 4, 57, 74, 90, 16, 60, 69, 97, 0, 57, 74, 90, 16, 65, 65, 65, 65, 66, 66, 66, 66, 60, 73, 97, 4, 66, 66, 99, 8, 65, 69, 101, 0, 57, 82, 99, 8, 60, 77, 101, 4, 57, 82, 99, 8, 69, 69, 69, 69, 66, 74, 107, 0, 60, 81, 105, 0, 66, 74, 107, 0, 69, 73, 105, 4, 57, 90, 107, 0, 60, 85, 109, 0, 57, 90, 90, 41, 73, 73, 73, 73, 74, 74, 74, 74, 60, 89, 109, 4, 74, 74, 115, 0, 73, 77, 113, 0, 74, 82, 115, 0, 60, 93, 113, 4, 74, 82, 99, 33, 77, 77, 77, 77, 74, 82, 99, 33, 60, 97, 117, 0, 74, 82, 115, 8, 77, 81, 117, 4, 74, 82, 115, 8, 60, 101, 121, 0, 82, 82, 82, 82, 81, 81, 81, 81, 82, 82, 82, 82, 60, 105, 121, 4, 82, 82, 123, 8, 81, 85, 125, 0, 82, 90, 123, 8, 60, 109, 125, 4, 82, 90, 107, 41, 85, 85, 85, 85, 82, 90, 107, 41, 60, 113, 130, 0, 82, 90, 132, 0, 85, 89, 125, 12, 82, 90, 132, 0, 60, 117, 130, 4, 90, 90, 132, 0, 89, 89, 89, 89, 90, 90, 90, 90, 60, 121, 134, 4, 90, 90, 90, 90, 89, 93, 125, 24, 90, 99, 140, 0, 60, 125, 138, 0, 90, 99, 140, 0, 93, 93, 93, 93, 90, 99, 140, 0, 93, 93, 142, 0, 90, 99, 123, 41, 93, 97, 125, 36, 90, 99, 123, 41, 97, 97, 142, 4, 90, 107, 148, 0, 97, 97, 97, 97, 90, 107, 148, 0, 97, 97, 146, 4, 99, 99, 99, 99, 97, 101, 125, 48, 99, 99, 99, 99, 101, 101, 150, 0, 90, 115, 148, 8, 101, 101, 101, 101, 90, 115, 148, 8, 101, 101, 154, 0, 99, 107, 156, 0, 101, 105, 125, 60, 99, 107, 156, 0, 105, 105, 154, 4, 90, 123, 156, 0, 105, 105, 105, 105, 90, 123, 156, 8, 105, 105, 158, 4, 107, 107, 107, 107, 105, 109, 125, 73, 107, 107, 156, 16, 109, 109, 162, 0, 107, 115, 156, 16, 109, 109, 109, 109, 107, 115, 165, 0, 109, 109, 166, 0, 107, 115, 165, 0, 109, 113, 125, 85, 107, 115, 165, 8, 113, 113, 166, 4, 107, 115, 165, 8, 113, 113, 113, 113, 115, 115, 115, 115, 113, 113, 170, 4, 115, 115, 115, 115, 113, 117, 125, 97, 115, 115, 156, 41, 117, 117, 174, 0, 115, 123, 156, 41, 117, 117, 117, 117, 115, 123, 173, 8, 117, 117, 178, 0, 115, 123, 173, 8, 117, 121, 125, 109, 115, 123, 181, 0, 121, 121, 178, 4, 115, 123, 165, 33, 121, 121, 121, 121, 123, 123, 123, 123, 121, 121, 182, 4, 123, 123, 123, 123, 121, 125, 125, 121, 123, 123, 123, 123, 125, 125, 186, 0, 123, 132, 189, 0, 125, 125, 125, 125, 123, 132, 189, 0, 125, 125, 190, 0, 123, 132, 189, 8, 125, 130, 130, 121, 123, 132, 189, 8, 125, 134, 190, 4, 123, 132, 173, 41, 125, 134, 190, 8, 123, 140, 173, 41, 125, 134, 130, 130, 123, 140, 189, 16, 125, 138, 195, 4, 132, 132, 132, 132, 130, 134, 199, 0, 132, 132, 198, 8, 125, 142, 190, 20, 123, 148, 198, 8, 134, 134, 134, 134, 123, 148, 198, 8, 125, 146, 203, 0, 132, 140, 206, 0, 134, 138, 203, 4, 132, 140, 206, 0, 125, 150, 190, 32, 123, 156, 206, 0, 138, 138, 138, 138, 123, 156, 189, 41, 125, 154, 207, 4, 140, 140, 140, 140, 138, 142, 211, 0, 140, 140, 214, 0, 125, 158, 190, 44, 140, 148, 214, 0, 142, 142, 142, 142, 140, 148, 198, 33, 125, 162, 215, 0, 140, 148, 198, 33, 142, 146, 215, 4, 140, 148, 214, 8, 125, 166, 190, 56, 140, 148, 214, 8, 146, 146, 146, 146, 148, 148, 148, 148, 125, 170, 219, 4, 148, 148, 148, 148, 146, 150, 223, 0, 148, 148, 222, 8, 125, 174, 190, 69, 148, 156, 222, 8, 150, 150, 150, 150, 148, 156, 206, 41, 125, 178, 227, 0, 148, 156, 206, 41, 150, 154, 227, 4, 148, 156, 222, 16, 125, 182, 195, 69, 148, 156, 231, 0, 154, 154, 154, 154, 156, 156, 156, 156, 125, 186, 231, 4, 156, 156, 156, 156, 154, 158, 235, 0, 156, 156, 156, 156, 158, 158, 203, 65, 156, 165, 239, 0, 158, 158, 158, 158, 156, 165, 239, 0, 158, 158, 239, 0, 156, 165, 222, 41, 158, 162, 239, 4, 156, 165, 222, 41, 162, 162, 207, 69, 156, 165, 239, 8, 162, 162, 162, 162, 156, 173, 239, 8, 162, 162, 243, 4, 156, 173, 247, 0, 162, 166, 247, 0, 165, 165, 165, 165, 166, 166, 215, 65, 165, 165, 247, 8, 166, 166, 166, 166, 156, 181, 247, 8, 166, 166, 251, 0, 156, 181, 247, 8, 166, 170, 251, 4, 165, 173, byte.MaxValue, 0, 170, 170, 219, 69, 165, 173, byte.MaxValue, 0, 170, 170, 170, 170, 156, 189, byte.MaxValue, 0, 170, 170, byte.MaxValue, 4, 156, 189, byte.MaxValue, 8, 170, 174, byte.MaxValue, 8, 173, 173, 173, 173, 174, 174, 227, 65, 173, 173, byte.MaxValue, 16, 174, 174, 174, 174, 173, 181, byte.MaxValue, 16, 174, 174, byte.MaxValue, 16, 173, 181, 231, 66, 174, 178, byte.MaxValue, 20, 173, 181, 231, 66, 178, 178, 231, 69, 173, 181, byte.MaxValue, 24, 178, 178, 178, 178, 173, 181, byte.MaxValue, 24, 178, 178, byte.MaxValue, 28, 181, 181, 181, 181, 178, 182, byte.MaxValue, 32, 181, 181, 181, 181, 182, 182, 239, 65, 181, 181, 181, 181, 182, 182, 182, 182, 181, 189, byte.MaxValue, 41, 182, 182, byte.MaxValue, 40, 181, 189, 239, 74, 182, 186, byte.MaxValue, 44, 181, 189, byte.MaxValue, 49, 186, 186, 243, 69, 181, 189, byte.MaxValue, 49, 186, 186, 186, 186, 181, 189, 231, 99, 186, 186, byte.MaxValue, 52, 189, 189, 231, 99, 186, 190, byte.MaxValue, 56, 189, 189, 189, 189, 190, 190, 247, 73, 189, 189, 189, 189, 190, 190, 190, 190, 189, 198, byte.MaxValue, 66, 190, 190, byte.MaxValue, 65, 189, 198, byte.MaxValue, 66, 190, 195, 219, 138, 189, 198, byte.MaxValue, 66, 190, 199, byte.MaxValue, 69, 189, 198, byte.MaxValue, 74, 190, 199, byte.MaxValue, 73, 189, 198, 239, 107, 195, 195, 195, 195, 189, 206, byte.MaxValue, 82, 190, 203, 227, 134, 189, 206, byte.MaxValue, 82, 195, 199, byte.MaxValue, 81, 198, 198, 198, 198, 190, 207, byte.MaxValue, 85, 198, 198, 198, 198, 199, 199, 199, 199, 189, 214, byte.MaxValue, 90, 190, 211, 235, 130, 189, 214, byte.MaxValue, 90, 199, 203, byte.MaxValue, 93, 198, 206, byte.MaxValue, 99, 190, 215, byte.MaxValue, 97, 198, 206, byte.MaxValue, 99, 203, 203, 203, 203, 189, 222, byte.MaxValue, 99, 190, 219, 239, 134, 189, 222, byte.MaxValue, 107, 203, 207, byte.MaxValue, 105, 206, 206, 206, 206, 190, 223, byte.MaxValue, 109, 206, 206, byte.MaxValue, 115, 207, 207, 207, 207, 206, 214, byte.MaxValue, 115, 190, 227, 247, 130, 206, 214, 231, 165, 207, 211, byte.MaxValue, 117, 206, 214, byte.MaxValue, 123, 190, 231, byte.MaxValue, 121, 206, 214, byte.MaxValue, 123, 211, 211, 211, 211, 206, 214, byte.MaxValue, 123, 190, 235, 251, 134, 214, 214, 214, 214, 211, 215, byte.MaxValue, 130, 214, 214, 214, 214, 190, 239, byte.MaxValue, 134, 214, 214, byte.MaxValue, 140, 215, 215, 215, 215, 214, 222, byte.MaxValue, 140, 190, 243, byte.MaxValue, 138, 214, 222, 239, 173, 215, 219, byte.MaxValue, 142, 214, 222, 239, 173, 190, 247, byte.MaxValue, 146, 214, 222, byte.MaxValue, 148, 219, 219, 219, 219, 214, 222, 231, 198, 190, 251, byte.MaxValue, 150, 222, 222, 222, 222, 219, 223, byte.MaxValue, 154, 222, 222, 222, 222, 190, byte.MaxValue, byte.MaxValue, 158, 222, 222, 222, 222, 223, 223, 223, 223, 222, 231, byte.MaxValue, 165, 223, 223, byte.MaxValue, 162, 222, 231, byte.MaxValue, 165, 223, 227, byte.MaxValue, 166, 222, 231, byte.MaxValue, 165, 227, 227, byte.MaxValue, 170, 222, 231, byte.MaxValue, 173, 227, 227, 227, 227, 222, 231, 239, 206, 227, 227, byte.MaxValue, 174, 222, 239, byte.MaxValue, 181, 227, 231, byte.MaxValue, 178, 222, 239, byte.MaxValue, 181, 231, 231, byte.MaxValue, 182, 231, 231, 231, 231, 231, 231, 231, 231, 231, 231, byte.MaxValue, 189, 231, 231, byte.MaxValue, 186, 222, 247, byte.MaxValue, 189, 231, 235, byte.MaxValue, 190, 222, 247, byte.MaxValue, 189, 235, 235, byte.MaxValue, 190, 231, 239, byte.MaxValue, 198, 235, 235, 235, 235, 231, 239, byte.MaxValue, 198, 235, 235, byte.MaxValue, 199, 222, byte.MaxValue, byte.MaxValue, 198, 235, 239, byte.MaxValue, 203, 222, byte.MaxValue, byte.MaxValue, 206, 239, 239, 239, 239, 239, 239, 239, 239, 239, 239, 239, 239, 239, 239, byte.MaxValue, 214, 239, 239, byte.MaxValue, 211, 239, 247, byte.MaxValue, 214, 239, 243, byte.MaxValue, 215, 239, 247, byte.MaxValue, 214, 243, 243, byte.MaxValue, 215, 239, 247, byte.MaxValue, 222, 243, 243, 243, 243, 239, 247, byte.MaxValue, 222, 243, 243, byte.MaxValue, 223, 239, 247, byte.MaxValue, 222, 243, 247, byte.MaxValue, 227, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, byte.MaxValue, 239, 247, 247, byte.MaxValue, 235, 247, byte.MaxValue, byte.MaxValue, 239, 247, 251, byte.MaxValue, 239, 247, byte.MaxValue, byte.MaxValue, 239, 251, 251, byte.MaxValue, 239, 247, byte.MaxValue, byte.MaxValue, 247, 251, 251, 251, 251, 247, byte.MaxValue, byte.MaxValue, 247, 251, 251, byte.MaxValue, 247, 247, byte.MaxValue, byte.MaxValue, 247, 251, byte.MaxValue, byte.MaxValue, 251, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue
            };
            ClassSetup();
        }

        private void ClassSetup() {
            zFSHFile.Directory = new cFSHImage.FSHDirEntry[1];
            ref cFSHImage.FSHHeader localFSHHeader = ref zFSHFile.Header;
            localFSHHeader.SHPI = "SHPI";
            localFSHHeader.FileSize = 0;
            localFSHHeader.numBmps = 0;
            localFSHHeader.DirID = "G264";
            ref cFSHImage.FSHDirEntry localFshDirEntry = ref zFSHFile.Directory[0];
            localFshDirEntry.EOffset = 0;
            localFshDirEntry.EName = "0000";
            ref cFSHImage.FSHEntryHdr localFSHEntryHdr = ref zFSHFile.Directory[0].EHdr;
            localFSHEntryHdr.code = 0;
            localFSHEntryHdr.Height = 256;
            localFSHEntryHdr.Width = 256;
            localFSHEntryHdr.nBMPs = 1;
            localFSHEntryHdr.NAttach = 0;
            localFSHEntryHdr.ColorBM = new cFSHImage.FSHBitMap[1];
            localFSHEntryHdr.AlphaBM = new cFSHImage.FSHBitMap[1];
            localFSHEntryHdr.ColorBM[0] = cFSHImage.AllocFSHBitmap(256, 256, 24, cFSHImage.BMP_PRESET.BMP_COLOR);
            localFSHEntryHdr.AlphaBM[0] = cFSHImage.AllocFSHBitmap(256, 256, 8, cFSHImage.BMP_PRESET.BMP_ALPHA);
        }

        public static string BMPType(int code) {
            string str;
            switch (code & sbyte.MaxValue) {
                case 96:
                    str = "DXT1";
                    break;
                case 97:
                    str = "DXT3";
                    break;
                case 109:
                    str = "16bit X4R4G4B4";
                    break;
                case 120:
                    str = "16bit R5G6B5";
                    break;
                case 123:
                    str = " 8bit PAL8";
                    break;
                case 125:
                    str = "32bit A8R8G8B8";
                    break;
                case 126:
                    str = "16bit A1R5G5B5";
                    break;
                case sbyte.MaxValue:
                    str = "24bit R8G8B8";
                    break;
                default:
                    str = "Unknown";
                    break;
            }
            if ((code & 128) == 128)
                str += " QFS Compressed";
            return str;
        }

        public cFSHImage.FSHDirEntry get_FSHDir(int DirIndex) {
            cFSHImage.FSHDirEntry fshDir = new cFSHImage.FSHDirEntry();
            if (Math.Abs(DirIndex) < zFSHFile.Header.numBmps)
                fshDir = zFSHFile.Directory[DirIndex];
            return fshDir;
        }

        public cFSHImage.FSHHeader FSHHead => zFSHFile.Header;

        public bool isCompr => zIsQFSComp;

        public bool Compress {
            set => zIsQFSComp = value;
        }

        public string LastPath => zFSHFilePath;

        public string SC4Type => zSC4Type;

        public string SC4Group => zSC4Group;

        public string SC4Instance => zSC4Instance;

        public string VendorAd => zFSHFile.VendorAd;

        public int Load(string FullFilePath) => LoadFSH(FullFilePath);

        public int Save(string FullFilePath) => SaveFSH(FullFilePath);

        public int GetCABitmaps(ref cFSHImage.FSHBitMap ColorBM, ref cFSHImage.FSHBitMap AlphaBM, int DirIndex, int BMIndex = 0) {
            int caBitmaps = -1;
            if (Math.Abs(DirIndex) < zFSHFile.Header.numBmps) {
                ref cFSHImage.FSHEntryHdr local = ref zFSHFile.Directory[Math.Abs(DirIndex)].EHdr;
                if (Math.Abs(BMIndex) < local.nBMPs) {
                    ColorBM = local.ColorBM[BMIndex];
                    AlphaBM = local.AlphaBM[BMIndex];
                    caBitmaps = 0;
                }
            }
            return caBitmaps;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public int SaveCABitmaps(int DirIndex, int BMIndex, string ColorBMFullPath, string AlphaBMFullPath) {
            int num1 = -1;
            if (Math.Abs(DirIndex) < zFSHFile.Header.numBmps) {
                ref cFSHImage.FSHEntryHdr local = ref zFSHFile.Directory[Math.Abs(DirIndex)].EHdr;
                if (Math.Abs(BMIndex) < local.nBMPs && cFSHImage.SaveBMP(ref local.ColorBM[BMIndex], ColorBMFullPath) == 0) {
                    if (cFSHImage.SaveBMP(ref local.AlphaBM[BMIndex], AlphaBMFullPath) == 0) {
                        num1 = 0;
                    } else {
                        FileSystem.Kill(ColorBMFullPath);
                        int num2 = (int) Interaction.MsgBox("Bitmaps were not saved");
                    }
                }
            }
            return num1;
        }

        public int InsertCABitmaps(int DirIndex, int MIPIndex, cFSHImage.FSHBitMap ColorBM, cFSHImage.FSHBitMap AlphaBM, cFSHImage.FSHBmpType EncodeMethod, bool isLotTexture, string DIRId = "G264", string BMPId = "0000") {
            if (DirIndex >= zFSHFile.Header.numBmps) {
                // ISSUE: variable of a reference type
                cFSHImage.FSHDirEntry[]&local;
                // ISSUE: explicit reference operation
                cFSHImage.FSHDirEntry[] fshDirEntryArray = (cFSHImage.FSHDirEntry[]) Utils.CopyArray((Array) ^ (local = ref zFSHFile.Directory), (Array) new cFSHImage.FSHDirEntry[zFSHFile.Header.numBmps + 1]);
                local = fshDirEntryArray;
                if (MIPIndex == 0) {
                    ++zFSHFile.Header.numBmps;
                    if (zFSHFile.Header.numBmps == 1)
                        zFSHFile.Header.DirID = DIRId;
                }
            }
            ref cFSHImage.FSHDirEntry local1 = ref zFSHFile.Directory[DirIndex];
            if (MIPIndex == 0) {
                local1.EName = Strings.Mid(BMPId, 1, 4);
                if (EncodeMethod == cFSHImage.FSHBmpType.BMP_DXTAUTO)
                    EncodeMethod = cFSHImage.DXTFormatByAlpha(AlphaBM, isLotTexture);
                local1.EHdr.code = (int) EncodeMethod;
                local1.EHdr.nBMPs = 1;
                local1.EHdr.Width = ColorBM.Width;
                local1.EHdr.Height = ColorBM.Height;
            }
            // ISSUE: variable of a reference type
            cFSHImage.FSHBitMap[]&local2;
            // ISSUE: explicit reference operation
            cFSHImage.FSHBitMap[] fshBitMapArray1 = (cFSHImage.FSHBitMap[]) Utils.CopyArray((Array) ^ (local2 = ref local1.EHdr.ColorBM), (Array) new cFSHImage.FSHBitMap[MIPIndex + 1]);
            local2 = fshBitMapArray1;
            local1.EHdr.ColorBM[MIPIndex] = ColorBM;
            // ISSUE: variable of a reference type
            cFSHImage.FSHBitMap[]&local3;
            // ISSUE: explicit reference operation
            cFSHImage.FSHBitMap[] fshBitMapArray2 = (cFSHImage.FSHBitMap[]) Utils.CopyArray((Array) ^ (local3 = ref local1.EHdr.AlphaBM), (Array) new cFSHImage.FSHBitMap[MIPIndex + 1]);
            local3 = fshBitMapArray2;
            local1.EHdr.AlphaBM[MIPIndex] = AlphaBM;
            return 0;
        }

        public void SetEncodeMethod(int DirIndex, int BMIndex, cFSHImage.FSHBmpType EncodeMethod, bool isLotTexture) {
            if (Math.Abs(DirIndex) >= zFSHFile.Header.numBmps)
                return;
            ref cFSHImage.FSHEntryHdr local = ref zFSHFile.Directory[Math.Abs(DirIndex)].EHdr;
            if (Math.Abs(BMIndex) >= local.nBMPs)
                return;
            if (EncodeMethod == cFSHImage.FSHBmpType.BMP_DXTAUTO)
                EncodeMethod = cFSHImage.DXTFormatByAlpha(local.AlphaBM[0], isLotTexture);
            local.code = (int) ((cFSHImage.FSHBmpType) (local.code & -256) | EncodeMethod);
        }

        public int DeleteCABitmaps(int DirIndex) {
            int num1 = -1;
            if (Math.Abs(DirIndex) < zFSHFile.Header.numBmps) {
                ref cFSHImage.FSHFile local1 = ref zFSHFile;
                int num2 = DirIndex;
                int num3 = Information.UBound(local1.Directory) - 1;
                for (int index = num2; index <= num3; ++index) {
                    local1.Directory[index] = local1.Directory[index + 1];
                    local1.Directory[index].EOffset = 0;
                }
                if (Information.UBound(local1.Directory) > 0) {
                    // ISSUE: variable of a reference type
                    cFSHImage.FSHDirEntry[]&local2;
                    // ISSUE: explicit reference operation
                    cFSHImage.FSHDirEntry[] fshDirEntryArray = (cFSHImage.FSHDirEntry[]) Utils.CopyArray((Array) ^ (local2 = ref local1.Directory), (Array) new cFSHImage.FSHDirEntry[Information.UBound(local1.Directory) - 1 + 1]);
                    local2 = fshDirEntryArray;
                }
                --zFSHFile.Header.numBmps;
                zFSHFile.Header.FileSize = 0;
                num1 = 0;
            }
            return num1;
        }

        public static cFSHImage.FSHBitMap AllocFSHColorBitmap(int Width, int Height) {
            return cFSHImage.AllocFSHBitmap(Width, Height, 24, cFSHImage.BMP_PRESET.BMP_COLOR);
        }

        public static cFSHImage.FSHBitMap AllocFSHAlphaBitmap(int Width, int Height) {
            return cFSHImage.AllocFSHBitmap(Width, Height, 8, cFSHImage.BMP_PRESET.BMP_ALPHA);
        }

        private int AllocFSHdata() {
            ref cFSHImage.FSHFile local1 = ref zFSHFile;
            int num1 = 16 + 24 * local1.Header.numBmps + 8;
            int num2 = local1.Header.numBmps - 1;
            for (int index1 = 0; index1 <= num2; ++index1) {
                ref cFSHImage.FSHEntryHdr local2 = ref local1.Directory[index1].EHdr;
                int num3 = Information.UBound(local2.ColorBM);
                for (int index2 = 0; index2 <= num3; ++index2)
                    num1 = num1 + local2.ColorBM[index2].PixBits.Length + local2.AlphaBM[index2].PixBits.Length;
            }
            zFSHData = new byte[num1 + ushort.MaxValue + 1];
            return 0;
        }

        private int GetFSHHeader(ref int pos) {
            string s = cFSHImage.GetString(ref zFSHData, pos, 4);
            int fshHeader;
            if (Operators.CompareString(s, "SHPI", false) != 0) {
                int num = (int) Interaction.MsgBox("File not a valid FSH file");
                fshHeader = -1;
            } else {
                ref cFSHImage.FSHHeader local = ref zFSHFile.Header;
                local.SHPI = s;
                local.FileSize = cFSHImage.GetLong(ref zFSHData, pos + 4);
                local.numBmps = cFSHImage.GetLong(ref zFSHData, pos + 8);
                local.DirID = cFSHImage.GetString(ref zFSHData, pos + 12, 4);
                fshHeader = 0;
            }
            pos = 16;
            return fshHeader;
        }

        private int PutFSHHeader(ref int pos) {
            cFSHImage.PutString(ref zFSHData, pos, 4, "SHPI");
            cFSHImage.PutLong(ref zFSHData, pos + 4, zFSHFile.Header.FileSize);
            cFSHImage.PutLong(ref zFSHData, pos + 8, zFSHFile.Header.numBmps);
            cFSHImage.PutString(ref zFSHData, pos + 12, 4, zFSHFile.Header.DirID);
            pos += 16;
            return 0;
        }

        private int GetDirectory(ref int pos) {
            ref cFSHImage.FSHFile local1 = ref zFSHFile;
            int num1 = local1.Header.numBmps - 1;
            local1.Directory = new cFSHImage.FSHDirEntry[num1 + 1];
            int num2 = num1;
            for (int index = 0; index <= num2; ++index) {
                ref cFSHImage.FSHDirEntry local2 = ref local1.Directory[index];
                local2.EName = cFSHImage.GetString(ref zFSHData, pos, 4);
                local2.EOffset = cFSHImage.GetLong(ref zFSHData, pos + 4);
                local2.EHdr = GetEntryHdr(local2.EOffset);
                pos += 8;
            }
            return 0;
        }

        private void CheckforVendorAd(int pos) {
            if (pos != zFSHFile.Directory[0].EOffset)
                zFSHFile.VendorAd = cFSHImage.GetString(ref zFSHData, pos, zFSHFile.Directory[0].EOffset - pos);
            else
                zFSHFile.VendorAd = "";
        }

        private void PutDirectory(ref int pos) {
            ref cFSHImage.FSHFile local1 = ref zFSHFile;
            int num = local1.Header.numBmps - 1;
            for (int index = 0; index <= num; ++index) {
                ref cFSHImage.FSHDirEntry local2 = ref local1.Directory[index];
                cFSHImage.PutString(ref zFSHData, pos, 4, local2.EName);
                cFSHImage.PutLong(ref zFSHData, pos + 4, local2.EOffset);
                pos += 8;
            }
            cFSHImage.PutString(ref zFSHData, pos, 8, Strings.Format(DateAndTime.Now, "yyyyMMdd"));
            pos += 8;
        }

        private cFSHImage.FSHEntryHdr GetEntryHdr(int Offset) {
            cFSHImage.FSHEntryHdr entryHdr = new cFSHImage.FSHEntryHdr();
            if (Offset < 0 | Offset > Information.UBound(zFSHData) - 15) {
                if (Offset != zFSHFile.Header.FileSize) {
                    int num = (int) Interaction.MsgBox("Error: Runaway offset=" + Conversions.ToString(Offset) + " in GetEntryHdr, check output");
                }
            } else {
                entryHdr.code = cFSHImage.GetLong(ref zFSHData, Offset);
                entryHdr.Width = cFSHImage.GetShort(ref zFSHData, Offset + 4);
                entryHdr.Height = cFSHImage.GetShort(ref zFSHData, Offset + 6);
                entryHdr.Misc0 = cFSHImage.GetShort(ref zFSHData, Offset + 8);
                entryHdr.Misc1 = cFSHImage.GetShort(ref zFSHData, Offset + 10);
                entryHdr.Misc2 = cFSHImage.GetShort(ref zFSHData, Offset + 12);
                entryHdr.Misc3 = cFSHImage.GetShort(ref zFSHData, Offset + 14);
            }
            return entryHdr;
        }

        private void PutEntryHdr(ref int Offset, ref cFSHImage.FSHEntryHdr Hdr) {
            cFSHImage.PutLong(ref zFSHData, Offset, Hdr.code);
            cFSHImage.PutShort(ref zFSHData, Offset + 4, Hdr.Width);
            cFSHImage.PutShort(ref zFSHData, Offset + 6, Hdr.Height);
            cFSHImage.PutShort(ref zFSHData, Offset + 8, Hdr.Misc0);
            cFSHImage.PutShort(ref zFSHData, Offset + 10, Hdr.Misc1);
            cFSHImage.PutShort(ref zFSHData, Offset + 12, Hdr.Misc2);
            cFSHImage.PutShort(ref zFSHData, Offset + 14, Hdr.Misc3);
            Offset += 16;
        }

        private static int GetLong(ref byte[] data, int Offset) {
            int num = data[Offset] + data[Offset + 1] * 256 + data[Offset + 2] * 65536 + (data[Offset + 3] & sbyte.MaxValue) * 16777216;
            if ((data[Offset + 3] & 128) != 0)
                num |= int.MinValue;
            return num;
        }

        private static void PutLong(ref byte[] data, int Offset, int value) {
            int num = 0;
            do {
                data[Offset] = (byte) (value & byte.MaxValue);
                value /= 256;
                ++Offset;
                ++num;
            }
            while (num <= 3);
        }

        private static string GetString(ref byte[] data, int Offset, int length) {
            string s = "";
            int num = length;
            for (int index = 1; index <= num; ++index) {
                s += Conversions.ToString(Strings.ChrW(data[Offset]));
                ++Offset;
            }
            return s;
        }

        private static void PutString(ref byte[] data, int Offset, int length, string s) {
            int num = length;
            for (int index = 1; index <= num; ++index) {
                if (Strings.Len(s) == 0) {
                    data[Offset] = 0;
                } else {
                    data[Offset] = (byte) Strings.AscW(s);
                    s = Strings.Mid(s, 2);
                }
                ++Offset;
            }
        }

        private static int GetShort(ref byte[] data, int Offset) {
            return data[Offset] + 256 * data[Offset + 1] & ushort.MaxValue;
        }

        private static void PutShort(ref byte[] data, int Offset, int value) {
            int num = 0;
            do {
                data[Offset] = (byte) (value & byte.MaxValue);
                value /= 256;
                ++Offset;
                ++num;
            }
            while (num <= 1);
        }

        private static bool isBMPCode(int code) {
            return code == 120 | code == 123 | code == 125 | code == 126 | code == sbyte.MaxValue | code == 109 | code == 97 | code == 96;
        }

        private static bool isNFSCode(int code) {
            return code == 34 | code == 36 | code == 41 | code == 42 | code == 45;
        }

        private static cFSHImage.FSHBitMap AllocFSHBitmap(int Width, int Height, int BitsPP, cFSHImage.BMP_PRESET Preset) {
            cFSHImage.FSHBitMap fshBitMap = new cFSHImage.FSHBitMap();
            fshBitMap.Width = Width;
            fshBitMap.Height = Height;
            if (BitsPP == 8) {
                fshBitMap.FSHFormat = cFSHImage.FSHBmpType.BMP_PAL8;
                fshBitMap.Palette = new int[256];
                if (Preset == cFSHImage.BMP_PRESET.BMP_ALPHA) {
                    int index = 0;
                    do {
                        fshBitMap.Palette[index] = Information.RGB(index, index, index);
                        ++index;
                    }
                    while (index <= byte.MaxValue);
                }
            } else
                fshBitMap.FSHFormat = cFSHImage.FSHBmpType.BMP_R8G8B8;
            fshBitMap.RowMod = cFSHImage.BMPRowModulo(Width, BitsPP);
            ref BITMAPINFOHEADER local1 = ref fshBitMap.BMIH;
            local1.biSize = 40;
            local1.biWidth = Width;
            local1.biHeight = Height;
            local1.biPlanes = 1;
            local1.biBitCount = (short) BitsPP;
            if (BitsPP == 8)
                local1.biClrUsed = 256;
            else
                local1.biClrUsed = 0;
            local1.biSizeImage = fshBitMap.RowMod * Height;
            local1.biXPelsPerMeter = 0;
            local1.biXPelsPerMeter = 0;
            if (BitsPP == 8)
                local1.biClrUsed = 256;
            else
                local1.biClrUsed = 0;
            local1.biClrImportant = 0;
            ref BITMAPFILEHEADER local2 = ref fshBitMap.BMFH;
            local2.bfType = 19778;
            local2.bfSize = 54 + fshBitMap.BMIH.biSizeImage;
            if (BitsPP == 8)
                local2.bfSize += 1024;
            local2.bfReserved1 = 0;
            local2.bfReserved2 = 0;
            local2.bfOffBits = 54;
            if (BitsPP == 8)
                local2.bfOffBits += 1024;
            if (BitsPP > 8 & BitsPP <= 16) {
                local2.bfSize += 12;
                local2.bfOffBits += 12;
            }
            fshBitMap.PixBits = new byte[fshBitMap.BMIH.biSizeImage - 1 + 1];
            return fshBitMap;
        }

        private static int BMPRowModulo(int Width, int BitsPP) {
            return (Width * BitsPP + 31 & -32) / 8;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private static int SaveBMP(ref cFSHImage.FSHBitMap BM, string FilePath) {
            int num1;
            int num2;
            int num3;
            try {
                bool flag = false;
                int FileNumber;
                int num4;
                if (Strings.Len(FilePath) == 0) {
                    int num5 = (int) Interaction.MsgBox("Need a filename to save BMP file to");
                    goto label_13;
                } else {
                    FileNumber = FileSystem.FreeFile();
                    num4 = -1;
                    ProjectData.ClearProjectError();
                    num1 = 2;
                    if (File.Exists(FilePath)) {
                        FileSystem.FileCopy(FilePath, FilePath + ".bu");
                        flag = true;
                        FileSystem.Kill(FilePath);
                    }
                }
            label_4:
                ProjectData.ClearProjectError();
                num1 = 3;
                FileNumber = FileSystem.FreeFile();
                FileSystem.FileOpen(FileNumber, FilePath, OpenMode.Binary, OpenAccess.Write);
                FileSystem.FilePut(FileNumber, BM.BMFH, -1L);
                FileSystem.FilePut(FileNumber, BM.BMIH, -1L);
                if (BM.BMIH.biBitCount == 8)
                    FileSystem.FilePut(FileNumber, BM.Palette, -1L, false, false);
                FileSystem.FilePut(FileNumber, BM.PixBits, -1L, false, false);
                num2 = 0;
                FileSystem.FileClose(FileNumber);
                ProjectData.ClearProjectError();
                num1 = 0;
                if (flag) {
                    FileSystem.Kill(FilePath + ".bu");
                    goto label_13;
                } else
                    goto label_13;
                label_9:
                num3 = -1;
                switch (num1) {
                    case 2:
                        goto label_4;
                    case 3:
                        int num6 = (int) Interaction.MsgBox("File " + FilePath + " not saved! Err: " + Information.Err().Description);
                        FileSystem.FileClose(FileNumber);
                        ProjectData.ClearProjectError();
                        num2 = num4;
                        goto label_13;
                }
            }
            catch (Exception ex) when (ex is Exception & num1 != 0 & num3 == 0) {
                ProjectData.SetProjectError(ex);
                goto label_9;
            }
            throw ProjectData.CreateProjectError(-2146828237);
        label_13:
            if (num3 != 0)
                ProjectData.ClearProjectError();
            return num2;
        }

        private static cFSHImage.FSHBmpType DXTFormatByAlpha(cFSHImage.FSHBitMap AlphaBM, bool isLotTexture) {
            cFSHImage.FSHBmpType fshBmpType = cFSHImage.FSHBmpType.BMP_DXT1;
            if (isLotTexture) {
                int num1 = AlphaBM.Height - 1;
                for (int index1 = 0; index1 <= num1; ++index1) {
                    int index2 = index1 * AlphaBM.RowMod;
                    int num2 = AlphaBM.Width - 1;
                    for (int index3 = 0; index3 <= num2; ++index3) {
                        if (AlphaBM.PixBits[index2] != byte.MaxValue) {
                            fshBmpType = cFSHImage.FSHBmpType.BMP_DXT3;
                            goto label_17;
                        } else
                            ++index2;
                    }
                }
            } else {
                int num3 = AlphaBM.Height - 1;
                for (int index4 = 0; index4 <= num3; ++index4) {
                    int index5 = index4 * AlphaBM.RowMod;
                    int num4 = AlphaBM.Width - 1;
                    for (int index6 = 0; index6 <= num4; ++index6) {
                        if (AlphaBM.PixBits[index5] != byte.MaxValue) {
                            fshBmpType = cFSHImage.FSHBmpType.BMP_DXT3;
                            goto label_17;
                        } else
                            ++index5;
                    }
                }
            }
        label_17:
            return fshBmpType;
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public int LoadFSH(string FilePath) {
            int num1;
            try {
                zFSHData = File.ReadAllBytes(FilePath);
                zFSHFilePath = FilePath;
                num1 = UnpackFSH();
                goto label_3;
            }
            catch (Exception ex) {
                ProjectData.SetProjectError(ex);
                int num2 = (int) Interaction.MsgBox("File " + FilePath + " not found or Truncated, Err: " + Information.Err().Description);
                ProjectData.ClearProjectError();
            }
            num1 = -1;
        label_3:
            return num1;
        }

        public int UnpackFSH() {
            int num1;
            try {
                byte[] Array = QFSCompressor.DecompressBlock(zFSHData);
                if (Array != null)
                    zFSHData = cFSHImage.zCopyArray(Array);
                int num2 = 0;
                int pos = 0;
                if (num2 == 0)
                    num2 = GetFSHHeader(ref pos);
                if (num2 == 0)
                    num2 = GetDirectory(ref pos);
                if (num2 == 0)
                    CheckforVendorAd(pos);
                if (num2 == 0)
                    num2 = GetGlobalPalette();
                if (num2 == 0)
                    num2 = GetEntries();
                num1 = num2;
                goto label_15;
            }
            catch (Exception ex) {
                ProjectData.SetProjectError(ex);
                ProjectData.ClearProjectError();
            }
            num1 = -1;
        label_15:
            return num1;
        }

        private int GetEntries() {
            int num1 = zFSHFile.Header.numBmps - 1;
            int entries;
            for (int i = 0; i <= num1; ++i) {
                cFSHImage.FSHEntryHdr ehdr = zFSHFile.Directory[i].EHdr;
                int eoffset = zFSHFile.Directory[i].EOffset;
                int nextOffset = GetNextOffset(i, eoffset);
                int num2 = cFSHImage.isBMPCode(ehdr.code & sbyte.MaxValue) ? 1 : 0;
                bool flag1 = (ehdr.code & 128) == 128;
                if (num2 != 0) {
                    cFSHImage.FSHEntryHdr PalHdr = new cFSHImage.FSHEntryHdr();
                    int PalOffset;
                    int num3 = CheckForPalette(ref PalHdr, ref PalOffset, ref ehdr, eoffset, nextOffset);
                    int num4 = 0;
                    bool flag2;
                    if (!flag1) {
                        if ((ehdr.Misc3 & 4095) == 0)
                            num4 = ehdr.Misc3 >> 12 & 15;
                        if (ehdr.Width % (1 << num4) != 0)
                            num4 = 0;
                        if (ehdr.Height % (1 << num4) != 0)
                            num4 = 0;
                        if (num4 != 0) {
                            int num5;
                            switch (ehdr.code & sbyte.MaxValue) {
                                case 96:
                                    num5 = 1;
                                    break;
                                case 97:
                                case 123:
                                    num5 = 2;
                                    break;
                                case 125:
                                    num5 = 8;
                                    break;
                                case sbyte.MaxValue:
                                    num5 = 6;
                                    break;
                                default:
                                    num5 = 4;
                                    break;
                            }
                            int num6 = 0;
                            int num7 = 0;
                            int num8 = num4;
                            for (int index = 0; index <= num8; ++index) {
                                int num9 = ehdr.Width >> index;
                                int num10 = ehdr.Height >> index;
                                if ((ehdr.code & 126) == 96) {
                                    num9 += 4 - num9 & 3;
                                    num10 += 4 - num10 & 3;
                                }
                                num6 += num9 * num10 * num5 / 2;
                                num7 += num9 * num10 * num5 / 2;
                                if ((ehdr.code & 126) != 96) {
                                    num6 += (short) (16 - num6 & 15);
                                    if (index == num4)
                                        num7 += (short) (16 - num7 & 15);
                                }
                            }
                            flag2 = false;
                            int num11 = ehdr.code / 256;
                            if (num11 == 0) {
                                if (num6 + eoffset + 16 != nextOffset)
                                    flag2 = true;
                                if (num7 + eoffset + 16 != nextOffset)
                                    num4 = 0;
                            } else {
                                if (num11 != num6 + 16)
                                    flag2 = true;
                                if (num11 != num7 + 16)
                                    num4 = 0;
                            }
                        }
                    }
                    int num12 = eoffset + 16;
                    byte[] destinationArray;
                    if (flag1) {
                        destinationArray = new byte[1];
                        entries = mQFSCompression.QFS_Uncompress(zFSHData, num12, nextOffset - num12, ref destinationArray);
                    } else {
                        destinationArray = new byte[nextOffset - num12 + 1];
                        Array.Copy(zFSHData, num12, destinationArray, 0, nextOffset - num12);
                    }
                    int curoffs = 0;
                    int num13 = num4;
                    for (int MipIndex = 0; MipIndex <= num13; ++MipIndex) {
                        curoffs = GetBMP(ref ehdr, ref destinationArray, MipIndex, curoffs, PalOffset);
                        if (num4 != 0) {
                            int num14 = curoffs & 15;
                            if (num14 != 0 & !flag2)
                                curoffs = curoffs + 16 - num14;
                        }
                    }
                    if (flag1) {
                        destinationArray = null;
                        if (ehdr.code / 256 != 0)
                            curoffs = eoffset + ehdr.code / 256;
                        else
                            curoffs = nextOffset;
                    }
                    cFSHImage.FSHEntryHdr fshEntryHdr = ehdr;
                    int Offset = eoffset;
                    int num15 = num3;
                    while (num15 > 0) {
                        --num15;
                        Offset += fshEntryHdr.code / 256;
                        fshEntryHdr = GetEntryHdr(Offset);
                        int num16 = fshEntryHdr.code & byte.MaxValue;
                        if ((ehdr.code & sbyte.MaxValue) == 123) {
                            int num17;
                            if (num16 == 45 | num16 == 41)
                                num17 = 2;
                            else if (num16 == 42)
                                num17 = 4;
                            else
                                num17 = 3;
                            curoffs = Offset + 16 + fshEntryHdr.Width * num17;
                        } else if ((fshEntryHdr.code & byte.MaxValue) == 111)
                            curoffs = Offset + 8 + fshEntryHdr.Width;
                        else if ((fshEntryHdr.code & byte.MaxValue) == 105)
                            curoffs = Offset + 16 + fshEntryHdr.Width;
                        else if ((fshEntryHdr.code & byte.MaxValue) == 112) {
                            curoffs = Offset + 16;
                        } else {
                            int num18 = fshEntryHdr.code / 256;
                            if (num18 <= 0)
                                num18 = nextOffset - Offset;
                            curoffs = Offset + num18;
                        }
                    }
                    if (curoffs <= nextOffset)
                        ;
                } else {
                    int num19 = ehdr.code & byte.MaxValue;
                }
                zFSHFile.Directory[i].EHdr = ehdr;
            }
            return entries;
        }

        private int GetNextOffset(int i, int curoffs) {
            ref cFSHImage.FSHFile local1 = ref zFSHFile;
            int nextOffset = local1.Header.FileSize;
            int numBmps = local1.Header.numBmps;
            int num1 = numBmps - 1;
            for (int index = 0; index <= num1; ++index) {
                ref cFSHImage.FSHDirEntry local2 = ref local1.Directory[index];
                if (local2.EOffset < nextOffset & local2.EOffset > curoffs)
                    nextOffset = local2.EOffset;
            }
            bool flag = false;
            if (i == numBmps - 1) {
                if (nextOffset != local1.Header.FileSize) flag = true;
            } else if (nextOffset != local1.Directory[i + 1].EOffset) flag = true;
            int num2 = flag ? 1 : 0;
            return nextOffset;
        }

        private int GetBMP(ref cFSHImage.FSHEntryHdr Hdr, ref byte[] BMPData, int MipIndex, int curoffs, int paloffs) {
            cFSHImage.FSHBitMap fshBitMap1 = new cFSHImage.FSHBitMap();
            cFSHImage.FSHBitMap fshBitMap2 = new cFSHImage.FSHBitMap();
            int Width = Hdr.Width >> MipIndex;
            int num1 = Hdr.Height >> MipIndex;
            cFSHImage.FSHBitMap fshBitMap3 = cFSHImage.AllocFSHBitmap(Width, num1, 8, cFSHImage.BMP_PRESET.BMP_ALPHA);
            int bmp;
            switch (Hdr.code & sbyte.MaxValue) {
                case 96:
                    fshBitMap2 = cFSHImage.AllocFSHBitmap(Width, num1, 24, cFSHImage.BMP_PRESET.BMP_COLOR);
                    for (int index1 = num1 / 4 - 1; index1 >= 0; index1 += -1) {
                        int num2 = Width / 4 - 1;
                        for (int index2 = 0; index2 <= num2; ++index2) {
                            cFSHImage.color[,] Dst = new cFSHImage.color[4, 4];
                            bmp = cFSHImage.GetDXT1(ref BMPData, ref curoffs, ref Dst);
                            int index3 = 0;
                            do {
                                int index4 = (4 * index1 + index3) * fshBitMap3.RowMod + 4 * index2;
                                int index5 = (4 * index1 + index3) * fshBitMap2.RowMod + 12 * index2;
                                int index6 = 0;
                                do {
                                    fshBitMap3.PixBits[index4] = Dst[index3, index6].A;
                                    ++index4;
                                    fshBitMap2.PixBits[index5] = Dst[index3, index6].r;
                                    int index7 = index5 + 1;
                                    fshBitMap2.PixBits[index7] = Dst[index3, index6].g;
                                    int index8 = index7 + 1;
                                    fshBitMap2.PixBits[index8] = Dst[index3, index6].B;
                                    index5 = index8 + 1;
                                    ++index6;
                                }
                                while (index6 <= 3);
                                ++index3;
                            }
                            while (index3 <= 3);
                        }
                    }
                    break;
                case 97:
                    fshBitMap2 = cFSHImage.AllocFSHBitmap(Width, num1, 24, cFSHImage.BMP_PRESET.BMP_COLOR);
                    for (int index9 = num1 / 4 - 1; index9 >= 0; index9 += -1) {
                        int num3 = Width / 4 - 1;
                        for (int index10 = 0; index10 <= num3; ++index10) {
                            cFSHImage.color[,] Dst = new cFSHImage.color[4, 4];
                            bmp = cFSHImage.GetDXT3(ref BMPData, ref curoffs, ref Dst);
                            int index11 = 0;
                            do {
                                int index12 = (4 * index9 + index11) * fshBitMap3.RowMod + 4 * index10;
                                int index13 = (4 * index9 + index11) * fshBitMap2.RowMod + 12 * index10;
                                int index14 = 0;
                                do {
                                    fshBitMap3.PixBits[index12] = Dst[index11, index14].A;
                                    ++index12;
                                    fshBitMap2.PixBits[index13] = Dst[index11, index14].r;
                                    int index15 = index13 + 1;
                                    fshBitMap2.PixBits[index15] = Dst[index11, index14].g;
                                    int index16 = index15 + 1;
                                    fshBitMap2.PixBits[index16] = Dst[index11, index14].B;
                                    index13 = index16 + 1;
                                    ++index14;
                                }
                                while (index14 <= 3);
                                ++index11;
                            }
                            while (index11 <= 3);
                        }
                    }
                    break;
                case 109:
                    fshBitMap2 = cFSHImage.AllocFSHBitmap(Width, num1, 24, cFSHImage.BMP_PRESET.BMP_COLOR);
                    for (int index17 = num1 - 1; index17 >= 0; index17 += -1) {
                        int index18 = (num1 - 1 - index17) * fshBitMap3.RowMod;
                        int index19 = (num1 - 1 - index17) * fshBitMap2.RowMod;
                        int index20 = curoffs + index17 * 2 * Width;
                        int num4 = Width - 1;
                        for (int index21 = 0; index21 <= num4; ++index21) {
                            int num5 = BMPData[index20];
                            int index22 = index20 + 1;
                            fshBitMap2.PixBits[index19] = (byte) (17U * (uint) (short) (num5 & 15));
                            int index23 = index19 + 1;
                            fshBitMap2.PixBits[index23] = (byte) (17 * (num5 / 16));
                            int index24 = index23 + 1;
                            int num6 = BMPData[index22];
                            index20 = index22 + 1;
                            fshBitMap2.PixBits[index24] = (byte) (17U * (uint) (short) (num6 & 15));
                            index19 = index24 + 1;
                            fshBitMap3.PixBits[index18] = (byte) (17 * (num6 / 16));
                            ++index18;
                        }
                    }
                    curoffs += 2 * Width * num1;
                    break;
                case 120:
                    fshBitMap2 = cFSHImage.AllocFSHBitmap(Width, num1, 24, cFSHImage.BMP_PRESET.BMP_COLOR);
                    for (int index25 = num1 - 1; index25 >= 0; index25 += -1) {
                        int index26 = (num1 - 1 - index25) * fshBitMap3.RowMod;
                        int index27 = (num1 - 1 - index25) * fshBitMap2.RowMod;
                        int index28 = curoffs + index25 * 2 * Width;
                        int num7 = Width - 1;
                        for (int index29 = 0; index29 <= num7; ++index29) {
                            int num8 = BMPData[index28] | BMPData[index28 + 1] << 8;
                            index28 += 2;
                            byte num9 = (byte) (num8 & 31);
                            fshBitMap2.PixBits[index27] = (byte) ((byte) ((uint) num9 << 3) | num9 & 7U);
                            int index30 = index27 + 1;
                            byte num10 = (byte) ((num8 & 2016) >> 5);
                            fshBitMap2.PixBits[index30] = (byte) ((byte) ((uint) num10 << 2) | num10 & 3U);
                            int index31 = index30 + 1;
                            byte num11 = (byte) ((num8 & 63488) >> 11);
                            fshBitMap2.PixBits[index31] = (byte) ((byte) ((uint) num11 << 3) | num11 & 7U);
                            index27 = index31 + 1;
                            fshBitMap3.PixBits[index26] = byte.MaxValue;
                            ++index26;
                        }
                    }
                    curoffs += 2 * Width * num1;
                    break;
                case 123:
                    fshBitMap2 = cFSHImage.AllocFSHBitmap(Width, num1, 8, cFSHImage.BMP_PRESET.BMP_COLOR);
                    if (paloffs >= 0)
                        fshBitMap2.Palette = cFSHImage.GetPalette(ref zFSHData, paloffs);
                    else if (zHaveGlobalPal)
                        fshBitMap2.Palette = cFSHImage.zCopyArray(zGlobalPalette);
                    else
                        fshBitMap2.Palette = cFSHImage.zCopyArray(fshBitMap3.Palette);
                    object Right = Width;
                    for (int Left = num1 - 1; Left >= 0; Left += -1) {
                        int index32 = (num1 - 1 - Left) * fshBitMap3.RowMod;
                        int index33 = (num1 - 1 - Left) * fshBitMap2.RowMod;
                        int integer = Conversions.ToInteger(Operators.AddObject(curoffs, Operators.MultiplyObject(Left, Right)));
                        int num12 = Width - 1;
                        for (int index34 = 0; index34 <= num12; ++index34) {
                            fshBitMap2.PixBits[index33] = BMPData[integer];
                            ++index33;
                            ++integer;
                            fshBitMap3.PixBits[index32] = byte.MaxValue;
                            ++index32;
                        }
                    }
                    curoffs = Conversions.ToInteger(Operators.AddObject(curoffs, Operators.MultiplyObject(num1, Right)));
                    break;
                case 125:
                    fshBitMap2 = cFSHImage.AllocFSHBitmap(Width, num1, 24, cFSHImage.BMP_PRESET.BMP_COLOR);
                    for (int index35 = num1 - 1; index35 >= 0; index35 += -1) {
                        int index36 = (num1 - 1 - index35) * fshBitMap3.RowMod;
                        int index37 = (num1 - 1 - index35) * fshBitMap2.RowMod;
                        int index38 = curoffs + index35 * 4 * Width;
                        int num13 = Width - 1;
                        for (int index39 = 0; index39 <= num13; ++index39) {
                            fshBitMap2.PixBits[index37] = BMPData[index38];
                            int index40 = index37 + 1;
                            int index41 = index38 + 1;
                            fshBitMap2.PixBits[index40] = BMPData[index41];
                            int index42 = index40 + 1;
                            int index43 = index41 + 1;
                            fshBitMap2.PixBits[index42] = BMPData[index43];
                            index37 = index42 + 1;
                            int index44 = index43 + 1;
                            fshBitMap3.PixBits[index36] = BMPData[index44];
                            ++index36;
                            index38 = index44 + 1;
                        }
                    }
                    curoffs += 4 * Width * num1;
                    break;
                case 126:
                    fshBitMap2 = cFSHImage.AllocFSHBitmap(Width, num1, 24, cFSHImage.BMP_PRESET.BMP_COLOR);
                    for (int index45 = num1 - 1; index45 >= 0; index45 += -1) {
                        int index46 = (num1 - 1 - index45) * fshBitMap3.RowMod;
                        int index47 = (num1 - 1 - index45) * fshBitMap2.RowMod;
                        int index48 = curoffs + index45 * 2 * Width;
                        int num14 = Width - 1;
                        for (int index49 = 0; index49 <= num14; ++index49) {
                            int num15 = BMPData[index48] | BMPData[index48 + 1] << 8;
                            index48 += 2;
                            byte num16 = (byte) (num15 & 31);
                            fshBitMap2.PixBits[index47] = (byte) ((byte) ((uint) num16 << 3) | num16 & 7U);
                            int index50 = index47 + 1;
                            byte num17 = (byte) ((num15 & 992) >> 5);
                            fshBitMap2.PixBits[index50] = (byte) ((byte) ((uint) num17 << 3) | num17 & 7U);
                            int index51 = index50 + 1;
                            byte num18 = (byte) ((num15 & 31744) >> 10);
                            fshBitMap2.PixBits[index51] = (byte) ((byte) ((uint) num18 << 3) | num18 & 7U);
                            index47 = index51 + 1;
                            fshBitMap3.PixBits[index46] = (byte) (byte.MaxValue & -((num15 & 32768) == 32768 ? 1 : 0));
                            ++index46;
                        }
                    }
                    curoffs += 2 * Width * num1;
                    break;
                case sbyte.MaxValue:
                    fshBitMap2 = cFSHImage.AllocFSHBitmap(Width, num1, 24, cFSHImage.BMP_PRESET.BMP_COLOR);
                    cFSHImage.BMPRowModulo(Width, 24);
                    for (int index52 = num1 - 1; index52 >= 0; index52 += -1) {
                        int index53 = (num1 - 1 - index52) * fshBitMap3.RowMod;
                        int index54 = (num1 - 1 - index52) * fshBitMap2.RowMod;
                        int index55 = curoffs + index52 * 3 * Width;
                        int num19 = Width - 1;
                        for (int index56 = 0; index56 <= num19; ++index56) {
                            fshBitMap2.PixBits[index54] = BMPData[index55];
                            int index57 = index54 + 1;
                            int index58 = index55 + 1;
                            fshBitMap2.PixBits[index57] = BMPData[index58];
                            int index59 = index57 + 1;
                            int index60 = index58 + 1;
                            fshBitMap2.PixBits[index59] = BMPData[index60];
                            index54 = index59 + 1;
                            index55 = index60 + 1;
                            fshBitMap3.PixBits[index53] = byte.MaxValue;
                            ++index53;
                        }
                    }
                    curoffs += num1 * 3 * Width;
                    break;
                default:
                    int num20 = (int) Interaction.MsgBox("Unexpected bmp type in Decode(): " + Conversion.Hex(Hdr.code & sbyte.MaxValue));
                    bmp = -1;
                    break;
            }
            if (bmp == 0) {
                // ISSUE: variable of a reference type
                cFSHImage.FSHBitMap[]&local1;
                // ISSUE: explicit reference operation
                cFSHImage.FSHBitMap[] fshBitMapArray1 = (cFSHImage.FSHBitMap[]) Utils.CopyArray((Array) ^ (local1 = ref Hdr.AlphaBM), (Array) new cFSHImage.FSHBitMap[Hdr.nBMPs + 1]);
                local1 = fshBitMapArray1;
                Hdr.AlphaBM[Hdr.nBMPs] = fshBitMap3;
                // ISSUE: variable of a reference type
                cFSHImage.FSHBitMap[]&local2;
                // ISSUE: explicit reference operation
                cFSHImage.FSHBitMap[] fshBitMapArray2 = (cFSHImage.FSHBitMap[]) Utils.CopyArray((Array) ^ (local2 = ref Hdr.ColorBM), (Array) new cFSHImage.FSHBitMap[Hdr.nBMPs + 1]);
                local2 = fshBitMapArray2;
                Hdr.ColorBM[Hdr.nBMPs] = fshBitMap2;
                ++Hdr.nBMPs;
                bmp = curoffs;
            }
            return bmp;
        }

        private static int GetDXT1(ref byte[] data, ref int Offset, ref cFSHImage.color[,] Dst) {
            Dst = new cFSHImage.color[4, 4];
            int num1 = data[Offset] + 256 * data[Offset + 1];
            Offset += 2;
            int num2 = data[Offset] + 256 * data[Offset + 1];
            Offset += 2;
            int num3 = 8 * (short) (num1 & 31);
            int num4 = 4 * (short) (num1 / 32 & 63);
            int num5 = 8 * (num1 / 2048);
            int num6 = 8 * (short) (num2 & 31);
            int num7 = 4 * (short) (num2 / 32 & 63);
            int num8 = 8 * (num2 / 2048);
            int num9 = num3 | num3 >> 5;
            int num10 = num4 | num4 >> 6;
            int num11 = num5 | num5 >> 5;
            int num12 = num6 | num6 >> 5;
            int num13 = num7 | num7 >> 6;
            int num14 = num8 | num8 >> 5;
            int num15 = 3;
            do {
                int num16 = 64;
                int num17 = 3;
                do {
                    ref cFSHImage.color local = ref Dst.Address(num15, num17);
                    switch (data[Offset] / num16 & 3) {
                        case 0:
                            local.A = byte.MaxValue;
                            local.r = (byte) num9;
                            local.g = (byte) num10;
                            local.B = (byte) num11;
                            break;
                        case 1:
                            local.A = byte.MaxValue;
                            local.r = (byte) num12;
                            local.g = (byte) num13;
                            local.B = (byte) num14;
                            break;
                        case 2:
                            if (num1 > num2) {
                                local.A = byte.MaxValue;
                                local.r = (byte) ((2 * num9 + num12) / 3);
                                local.g = (byte) ((2 * num10 + num13) / 3);
                                local.B = (byte) ((2 * num11 + num14) / 3);
                                break;
                            }
                            local.A = byte.MaxValue;
                            local.r = (byte) ((num9 + num12) / 2);
                            local.g = (byte) ((num10 + num13) / 2);
                            local.B = (byte) ((num11 + num14) / 2);
                            break;
                        case 3:
                            if (num1 > num2) {
                                local.A = byte.MaxValue;
                                local.r = (byte) ((num9 + 2 * num12) / 3);
                                local.g = (byte) ((num10 + 2 * num13) / 3);
                                local.B = (byte) ((num11 + 2 * num14) / 3);
                                break;
                            }
                            local.A = 0;
                            local.r = 0;
                            local.g = 0;
                            local.B = 0;
                            break;
                    }
                    num16 /= 4;
                    num17 += -1;
                }
                while (num17 >= 0);
                ++Offset;
                num15 += -1;
            }
            while (num15 >= 0);
            return 0;
        }

        private static int GetDXT3(ref byte[] data, ref int Offset, ref cFSHImage.color[,] Dst) {
            Dst = new cFSHImage.color[4, 4];
            int index1 = 3;
            do {
                int index2 = 0;
                do {
                    Dst[index1, index2].A = (byte) (17U * (uint) (short) (data[Offset] & 15));
                    Dst[index1, index2 + 1].A = (byte) (17U * (uint) (short) (data[Offset] / 16 & 15));
                    ++Offset;
                    index2 += 2;
                }
                while (index2 <= 3);
                index1 += -1;
            }
            while (index1 >= 0);
            int num1 = data[Offset] + 256 * data[Offset + 1];
            Offset += 2;
            int num2 = data[Offset] + 256 * data[Offset + 1];
            Offset += 2;
            int num3 = 8 * (short) (num1 & 31);
            int num4 = 4 * (short) (num1 / 32 & 63);
            int num5 = 8 * (num1 / 2048);
            int num6 = 8 * (short) (num2 & 31);
            int num7 = 4 * (short) (num2 / 32 & 63);
            int num8 = 8 * (num2 / 2048);
            int num9 = num3 | num3 >> 5;
            int num10 = num4 | num4 >> 6;
            int num11 = num5 | num5 >> 5;
            int num12 = num6 | num6 >> 5;
            int num13 = num7 | num7 >> 6;
            int num14 = num8 | num8 >> 5;
            int num15 = 3;
            do {
                int num16 = 64;
                int num17 = 3;
                do {
                    ref cFSHImage.color local = ref Dst.Address(num15, num17);
                    switch (data[Offset] / num16 & 3) {
                        case 0:
                            local.r = (byte) num9;
                            local.g = (byte) num10;
                            local.B = (byte) num11;
                            break;
                        case 1:
                            local.r = (byte) num12;
                            local.g = (byte) num13;
                            local.B = (byte) num14;
                            break;
                        case 2:
                            if (num1 > num2) {
                                local.r = (byte) ((2 * num9 + num12) / 3);
                                local.g = (byte) ((2 * num10 + num13) / 3);
                                local.B = (byte) ((2 * num11 + num14) / 3);
                                break;
                            }
                            local.r = (byte) ((num9 + num12) / 2);
                            local.g = (byte) ((num10 + num13) / 2);
                            local.B = (byte) ((num11 + num14) / 2);
                            break;
                        case 3:
                            if (num1 > num2) {
                                local.r = (byte) ((num9 + 2 * num12) / 3);
                                local.g = (byte) ((num10 + 2 * num13) / 3);
                                local.B = (byte) ((num11 + 2 * num14) / 3);
                                break;
                            }
                            local.r = 0;
                            local.g = 0;
                            local.B = 0;
                            break;
                    }
                    num16 /= 4;
                    num17 += -1;
                }
                while (num17 >= 0);
                ++Offset;
                num15 += -1;
            }
            while (num15 >= 0);
            return 0;
        }

        private int CheckForPalette(ref cFSHImage.FSHEntryHdr PalHdr, ref int PalOffset, ref cFSHImage.FSHEntryHdr Hdr, int Offset, int NxtOffset) {
            cFSHImage.FSHEntryHdr fshEntryHdr = Hdr;
            int Offset1 = Offset;
            int num1 = 0;
            PalOffset = -1;
            while (fshEntryHdr.code / 256 > 0) {
                try {
                    Offset1 = Offset1 + fshEntryHdr.code / 256 & 16777215;
                }
                catch (Exception ex) {
                    ProjectData.SetProjectError(ex);
                    Offset1 = NxtOffset;
                    ProjectData.ClearProjectError();
                }
                ++num1;
                if (Offset1 > NxtOffset) {
                    int num2 = (int) Interaction.MsgBox("ERROR: incorrect attachment structure! I can't continue");
                    goto label_10;
                } else if (Offset1 != NxtOffset) {
                    fshEntryHdr = GetEntryHdr(Offset1);
                    int code = fshEntryHdr.code & byte.MaxValue;
                    if ((Hdr.code & sbyte.MaxValue) == 123 && cFSHImage.isNFSCode(code)) {
                        PalHdr = fshEntryHdr;
                        PalOffset = Offset1;
                    }
                } else
                    break;
            }
            int num3 = num1;
        label_10:
            return num3;
        }

        private int GetGlobalPalette() {
            int numBmps = zFSHFile.Header.numBmps;
            int index1 = -1;
            bool flag = false;
            int num1 = numBmps - 1;
            for (int index2 = 0; index2 <= num1; ++index2) {
                ref cFSHImage.FSHDirEntry local = ref zFSHFile.Directory[index2];
                int num2 = local.EHdr.code & byte.MaxValue;
                if (num2 == 34 | num2 == 36 | num2 == 45 | num2 == 42 | num2 == 41)
                    index1 = index2;
                if (Operators.CompareString(local.EName, "!pal", false) == 0) {
                    flag = true;
                    break;
                }
            }
            if (index1 >= 0)
                flag = true;
            if (flag)
                zGlobalPalette = cFSHImage.GetPalette(ref zFSHData, zFSHFile.Directory[index1].EOffset);
            zHaveGlobalPal = flag;
            return 0;
        }

        private static int[] GetPalette(ref byte[] data, int pOfs) {
            int num1 = cFSHImage.GetShort(ref data, pOfs + 4);
            int[] Array = new int[num1 - 1 + 1];
            int index1 = pOfs + 16;
            if (num1 == 256) {
                int index2 = 0;
                do {
                    Array[index2] = -16777216 | index2 << 16 | index2 << 8 | index2;
                    ++index2;
                }
                while (index2 <= byte.MaxValue);
            } else {
                int num2 = num1 - 1;
                for (int index3 = 0; index3 <= num2; ++index3) {
                    int num3 = index3 * byte.MaxValue / (num1 - 1);
                    Array[index3] = -16777216 | Information.RGB(num3, num3, num3);
                }
            }
            switch (cFSHImage.GetLong(ref data, pOfs) & byte.MaxValue) {
                case 34:
                    int num4 = num1 - 1;
                    for (int index4 = 0; index4 <= num4; ++index4) {
                        Array[index4] = Color.FromArgb(1020, 4 * data[index1 + 2], 4 * data[index1 + 1], 4 * data[index1]).ToArgb();
                        index1 += 3;
                    }
                    break;
                case 36:
                    int num5 = num1 - 1;
                    for (int index5 = 0; index5 <= num5; ++index5) {
                        Array[index5] = Color.FromArgb(byte.MaxValue, data[index1], data[index1 + 1], data[index1 + 2]).ToArgb();
                        index1 += 3;
                    }
                    break;
                case 41:
                    int num6 = num1 - 1;
                    for (int index6 = 0; index6 <= num6; ++index6) {
                        int num7 = data[index1] + 256 * data[index1 + 1];
                        Array[index6] = Color.FromArgb(byte.MaxValue, 8 * (short) (num7 / 2048 & 31), 4 * (short) (num7 / 32 & 63), 8 * (short) (num7 & 31)).ToArgb();
                        index1 += 2;
                    }
                    break;
                case 42:
                    int num8 = num1 - 1;
                    for (int index7 = 0; index7 <= num8; ++index7) {
                        Array[index7] = Color.FromArgb(data[index1 + 3], data[index1 + 2], data[index1 + 1], data[index1]).ToArgb();
                        index1 += 4;
                    }
                    break;
                case 45:
                    int num9 = num1 - 1;
                    for (int index8 = 0; index8 <= num9; ++index8) {
                        int num10 = data[index1] + 256 * data[index1 + 1];
                        Array[index8] = Information.RGB(8 * (short) (num10 / 1024 & 31), 8 * (short) (num10 / 32 & 63), 8 * (short) (num10 & 31));
                        if ((num10 & 32768) != 0)
                            Array[index8] = Array[index8] | -16777216;
                        int[] numArray = Array;
                        int index9 = index8;
                        int alpha;
                        if ((num10 & 32768) == 0)
                            alpha = 0;
                        else
                            alpha = byte.MaxValue;
                        int red = 8 * (short) (num10 / 1024 & 31);
                        int green = 8 * (short) (num10 / 32 & 63);
                        int blue = 8 * (short) (num10 & 31);
                        int argb = Color.FromArgb(alpha, red, green, blue).ToArgb();
                        numArray[index9] = argb;
                        index1 += 2;
                    }
                    break;
            }
            return cFSHImage.zCopyArray(Array);
        }

        private static byte[] zCopyArray(byte[] Array) {
            byte[] numArray = new byte[Information.UBound(Array) + 1];
            Array.CopyTo(numArray, 0);
            return numArray;
        }

        private static int[] zCopyArray(int[] Array) {
            int[] numArray = new int[Information.UBound(Array) + 1];
            Array.CopyTo(numArray, 0);
            return numArray;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public int SaveFSH(string FilePath) {
            int num1;
            if (Strings.Len(FilePath) == 0) {
                int num2 = (int) Interaction.MsgBox("Need a filename to save FSH file to");
                num1 = -1;
            } else {
                FileStream output = null;
                int num3 = -1;
                try {
                    if (MakeFSH() != 0) {
                        num1 = num3;
                        goto label_21;
                    } else {
                        bool flag = false;
                        if (File.Exists(FilePath)) {
                            FileSystem.FileCopy(FilePath, FilePath + ".bu");
                            flag = true;
                            FileSystem.Kill(FilePath);
                        }
                        output = new FileStream(FilePath, FileMode.CreateNew, FileAccess.Write);
                        using (BinaryWriter binaryWriter = new BinaryWriter(output)) {
                            if (zIsQFSComp)
                                binaryWriter.Write(4 + zFSHData.Length);
                            binaryWriter.Write(zFSHData);
                        }
                        if (flag)
                            FileSystem.Kill(FilePath + ".bu");
                        zFSHFilePath = FilePath;
                        output.Dispose();
                        num1 = 0;
                        goto label_21;
                    }
                }
                catch (Exception ex) {
                    ProjectData.SetProjectError(ex);
                    Exception exception = ex;
                    output?.Dispose();
                    int num4 = (int) Interaction.MsgBox("File " + FilePath + " not saved! Err: " + exception.InnerException.Message);
                    ProjectData.ClearProjectError();
                }
                num1 = num3;
            }
        label_21:
            return num1;
        }

        private int MakeFSH() {
            AllocFSHdata();
            int pos1 = 0;
            zFSHFile.Header.FileSize = -18097427;
            PutFSHHeader(ref pos1);
            PutDirectory(ref pos1);
            ref cFSHImage.FSHFile local1 = ref zFSHFile;
            int num1 = local1.Header.numBmps - 1;
            for (int index = 0; index <= num1; ++index) {
                local1.Directory[index].EOffset = pos1;
                int num2 = Information.UBound(local1.Directory[index].EHdr.ColorBM);
                if (num2 > 0)
                    local1.Directory[index].EHdr.Misc3 = num2 << 12;
                int num3 = num2;
                for (int MipIndex = 0; MipIndex <= num3; ++MipIndex) {
                    int num4 = PutBMP(ref local1.Directory[index].EHdr, MipIndex, pos1);
                    pos1 = num4 + (16 - num4 & 15);
                }
            }
            local1.Header.FileSize = pos1;
            int pos2 = 0;
            PutFSHHeader(ref pos2);
            PutDirectory(ref pos2);
            zUncompressedSize = zFSHFile.Header.FileSize;
            zCompressedSize = zUncompressedSize;
            int num5;
            if (zIsQFSComp) {
                int fileSize = zFSHFile.Header.FileSize;
                byte[] Outbuf = new byte[1];
                num5 = mQFSCompression.QFS_Compress(ref Outbuf, zFSHData, 0, ref fileSize);
                if (num5 == 0) {
                    zFSHData = new byte[Information.UBound(Outbuf) + 1];
                    Array.Copy(Outbuf, zFSHData, Outbuf.Length);
                    zCompressedSize = Outbuf.Length;
                }
            } else {
                // ISSUE: variable of a reference type
                byte[]&local2;
                // ISSUE: explicit reference operation
                byte[] numArray = (byte[]) Utils.CopyArray((Array) ^ (local2 = ref zFSHData), (Array) new byte[zFSHFile.Header.FileSize - 1 + 1]);
                local2 = numArray;
            }
            return num5;
        }

        private int PutBMP(ref cFSHImage.FSHEntryHdr Hdr, int MipIndex, int Offset) {
            bool flag1 = false;
            if (MipIndex == 0) {
                Hdr.code &= byte.MaxValue;
                if (Hdr.code == 125) {
                    bool flag2 = true;
                    ref cFSHImage.FSHBitMap local = ref Hdr.AlphaBM[0];
                    int num = Information.UBound(local.PixBits);
                    for (int index = 0; index <= num; ++index) {
                        if (local.PixBits[index] != byte.MaxValue) {
                            flag2 = false;
                            break;
                        }
                    }
                    if (flag2)
                        Hdr.code = sbyte.MaxValue;
                }
                PutEntryHdr(ref Offset, ref Hdr);
            }
            int num1 = Offset;
            cFSHImage.FSHBitMap fshBitMap1 = Hdr.AlphaBM[MipIndex];
            cFSHImage.FSHBitMap fshBitMap2 = Hdr.ColorBM[MipIndex];
            int num2 = Hdr.Width >> MipIndex;
            int Left1 = Hdr.Height >> MipIndex;
            switch (Hdr.code & sbyte.MaxValue) {
                case 96:
                case 97:
                    byte[] src = new byte[64];
                    for (int index1 = Left1 / 4 - 1; index1 >= 0; index1 += -1) {
                        int num3 = num2 / 4 - 1;
                        for (int index2 = 0; index2 <= num3; ++index2) {
                            int num4 = 0;
                            do {
                                int index3 = (4 * index1 + num4) * fshBitMap1.RowMod + 4 * index2;
                                int index4 = (4 * index1 + num4) * fshBitMap2.RowMod + 12 * index2;
                                int num5 = 3;
                                do {
                                    try {
                                        src[4 * (4 * num4 + num5) + 0] = fshBitMap1.PixBits[index3];
                                        ++index3;
                                        src[4 * (4 * num4 + num5) + 3] = fshBitMap2.PixBits[index4];
                                        ++index4;
                                        src[4 * (4 * num4 + num5) + 2] = fshBitMap2.PixBits[index4];
                                        ++index4;
                                        src[4 * (4 * num4 + num5) + 1] = fshBitMap2.PixBits[index4];
                                        ++index4;
                                    }
                                    catch (Exception ex) {
                                        ProjectData.SetProjectError(ex);
                                        src[4 * (4 * num4 + num5) + 0] = byte.MaxValue;
                                        src[4 * (4 * num4 + num5) + 3] = 0;
                                        src[4 * (4 * num4 + num5) + 2] = 0;
                                        src[4 * (4 * num4 + num5) + 1] = 0;
                                        ProjectData.ClearProjectError();
                                    }
                                    num5 += -1;
                                }
                                while (num5 >= 0);
                                ++num4;
                            }
                            while (num4 <= 3);
                            PutDXT(zFSHData, ref Offset, src, (cFSHImage.FSHBmpType) (Hdr.code & sbyte.MaxValue));
                        }
                    }
                    break;
                case 109:
                    for (int index5 = Left1 - 1; index5 >= 0; index5 += -1) {
                        int index6 = (Left1 - 1 - index5) * fshBitMap1.RowMod;
                        int index7 = (Left1 - 1 - index5) * fshBitMap2.RowMod;
                        int index8 = Offset + index5 * 2 * num2;
                        int num6 = num2 - 1;
                        for (int index9 = 0; index9 <= num6; ++index9) {
                            int num7 = fshBitMap2.PixBits[index7] / 16;
                            int index10 = index7 + 1;
                            int num8 = num7 | fshBitMap2.PixBits[index10] & 240;
                            int index11 = index10 + 1;
                            zFSHData[index8] = (byte) num8;
                            int index12 = index8 + 1;
                            int num9 = fshBitMap2.PixBits[index11] / 16;
                            index7 = index11 + 1;
                            int num10 = num9 | fshBitMap1.PixBits[index6] & 240;
                            ++index6;
                            zFSHData[index12] = (byte) num10;
                            index8 = index12 + 1;
                        }
                    }
                    Offset = num1 + 2 * num2 * Left1;
                    break;
                case 120:
                    for (int index13 = Left1 - 1; index13 >= 0; index13 += -1) {
                        int index14 = (Left1 - 1 - index13) * fshBitMap2.RowMod;
                        int index15 = Offset + index13 * 2 * num2;
                        int num11 = num2 - 1;
                        for (int index16 = 0; index16 <= num11; ++index16) {
                            int num12 = (byte) ((uint) fshBitMap2.PixBits[index14] >> 3);
                            int index17 = index14 + 1;
                            int num13 = num12 | fshBitMap2.PixBits[index17] >> 2 << 5;
                            int index18 = index17 + 1;
                            int num14 = num13 | fshBitMap2.PixBits[index18] >> 3 << 11;
                            index14 = index18 + 1;
                            zFSHData[index15] = (byte) (num14 & byte.MaxValue);
                            int index19 = index15 + 1;
                            zFSHData[index19] = (byte) ((num14 & 65280) >> 8);
                            index15 = index19 + 1;
                        }
                    }
                    Offset = num1 + 2 * num2 * Left1;
                    break;
                case 123:
                    object Right = num2;
                    for (int Left2 = Left1 - 1; Left2 >= 0; Left2 += -1) {
                        int index20 = (Left1 - 1 - Left2) * fshBitMap2.RowMod;
                        int integer = Conversions.ToInteger(Operators.AddObject(Offset, Operators.MultiplyObject(Left2, Right)));
                        int num15 = num2 - 1;
                        for (int index21 = 0; index21 <= num15; ++index21) {
                            zFSHData[integer] = cFSHImage.RGBtoGrey(fshBitMap2.PixBits[index20 + 2], fshBitMap2.PixBits[index20 + 1], fshBitMap2.PixBits[index20]);
                            index20 += 3;
                            ++integer;
                        }
                    }
                    Offset = Conversions.ToInteger(Operators.AddObject(num1, Operators.MultiplyObject(Left1, Right)));
                    flag1 = true;
                    break;
                case 125:
                    for (int index22 = Left1 - 1; index22 >= 0; index22 += -1) {
                        int index23 = (Left1 - 1 - index22) * fshBitMap1.RowMod;
                        int index24 = (Left1 - 1 - index22) * fshBitMap2.RowMod;
                        int index25 = Offset + index22 * 4 * num2;
                        int num16 = num2 - 1;
                        for (int index26 = 0; index26 <= num16; ++index26) {
                            zFSHData[index25] = fshBitMap2.PixBits[index24];
                            int index27 = index24 + 1;
                            int index28 = index25 + 1;
                            zFSHData[index28] = fshBitMap2.PixBits[index27];
                            int index29 = index27 + 1;
                            int index30 = index28 + 1;
                            zFSHData[index30] = fshBitMap2.PixBits[index29];
                            index24 = index29 + 1;
                            int index31 = index30 + 1;
                            zFSHData[index31] = fshBitMap1.PixBits[index23];
                            ++index23;
                            index25 = index31 + 1;
                        }
                    }
                    Offset = num1 + 4 * num2 * Left1;
                    break;
                case 126:
                    for (int index32 = Left1 - 1; index32 >= 0; index32 += -1) {
                        int index33 = (Left1 - 1 - index32) * fshBitMap1.RowMod;
                        int index34 = (Left1 - 1 - index32) * fshBitMap2.RowMod;
                        int index35 = Offset + index32 * 2 * num2;
                        int num17 = num2 - 1;
                        for (int index36 = 0; index36 <= num17; ++index36) {
                            int num18 = (byte) ((uint) fshBitMap2.PixBits[index34] >> 3);
                            int index37 = index34 + 1;
                            int num19 = num18 | fshBitMap2.PixBits[index37] >> 3 << 5;
                            int index38 = index37 + 1;
                            int num20 = num19 | fshBitMap2.PixBits[index38] >> 3 << 10;
                            index34 = index38 + 1;
                            int num21 = num20 | 32768 & -((fshBitMap1.PixBits[index33] & 128) == 128 ? 1 : 0);
                            ++index33;
                            zFSHData[index35] = (byte) (num21 & byte.MaxValue);
                            int index39 = index35 + 1;
                            zFSHData[index39] = (byte) ((num21 & 65280) / 256);
                            index35 = index39 + 1;
                        }
                    }
                    Offset = num1 + 2 * num2 * Left1;
                    break;
                case sbyte.MaxValue:
                    for (int index40 = Left1 - 1; index40 >= 0; index40 += -1) {
                        int index41 = (Left1 - 1 - index40) * fshBitMap2.RowMod;
                        int index42 = Offset + index40 * 3 * num2;
                        int num22 = num2 - 1;
                        for (int index43 = 0; index43 <= num22; ++index43) {
                            zFSHData[index42] = fshBitMap2.PixBits[index41];
                            int index44 = index41 + 1;
                            int index45 = index42 + 1;
                            zFSHData[index45] = fshBitMap2.PixBits[index44];
                            int index46 = index44 + 1;
                            int index47 = index45 + 1;
                            zFSHData[index47] = fshBitMap2.PixBits[index46];
                            index41 = index46 + 1;
                            index42 = index47 + 1;
                        }
                    }
                    Offset = num1 + Left1 * 3 * num2;
                    break;
            }
            if ((Hdr.code & 128) == 128) {
                int Buflen = Offset - num1;
                byte[] Outbuf = new byte[1];
                if (mQFSCompression.QFS_Compress(ref Outbuf, zFSHData, num1, ref Buflen) == 0) {
                    Array.Copy(Outbuf, 0, zFSHData, num1, Buflen);
                    Offset = num1 + Buflen;
                }
            }
            if (flag1) {
                int[] Pal = new int[256];
                Offset = (int) (Offset + 15L & 4294967280L);
                Hdr.code = (int) (Hdr.code & (long) byte.MaxValue | (long) (Offset - 32 << 8));
                Hdr.NAttach = 1;
                cFSHImage.PutPalette(ref zFSHData, ref Offset, ref Pal, 36);
                int Offset1 = 32;
                PutEntryHdr(ref Offset1, ref Hdr);
            }
            return Offset;
        }

        private static byte RGBtoGrey(int r, int g, int b) {
            return (byte) ((19595 * r + 38470 * g + 7471 * b) / 65536);
        }

        private static float RGBtoGrey(float r, float g, float b) {
            return (float) ((19595.0 * (double) r + 38470.0 * (double) g + 7471.0 * (double) b) / 65536.0);
        }

        private void PutDXT(byte[] dest, ref int destp, byte[] src, cFSHImage.FSHBmpType OutFormat) {
            if (OutFormat != cFSHImage.FSHBmpType.BMP_DXT3)
                OutFormat = cFSHImage.FSHBmpType.BMP_DXT1;
            int method;
            int index1;
            if (OutFormat == cFSHImage.FSHBmpType.BMP_DXT1) {
                method = 1;
                int num = 0;
                do {
                    index1 = 4 * num;
                    if ((src[index1] & 128) == 128) {
                        src[index1] = byte.MaxValue;
                    } else {
                        src[index1] = 0;
                        method = 2;
                    }
                    ++num;
                }
                while (num <= 15);
            } else {
                int num = 7;
                do {
                    index1 = 8 * num;
                    dest[destp] = (byte) (cFSHImage.TruncateX4(src[index1]) | cFSHImage.TruncateX4(src[index1 + 4]) >> 4);
                    ++destp;
                    num += -1;
                }
                while (num >= 0);
                method = 1;
            }
            int[] c = new int[12];
            BestColourPairA(src, c, ref method);
            int num1 = 0;
            do {
                c[num1 + 0] = cFSHImage.TruncateX5(c[num1 + 0]);
                c[num1 + 1] = cFSHImage.TruncateX6(c[num1 + 1]);
                c[num1 + 2] = cFSHImage.TruncateX5(c[num1 + 2]);
                num1 += 3;
            }
            while (num1 <= 3);
            int num2 = c[0] << 8 | c[1] << 3 | c[2] >> 3;
            int num3 = c[3] << 8 | c[4] << 3 | c[5] >> 3;
            if (num2 == num3)
                method = 2;
            if (num2 <= num3 ^ method == 2) {
                int num4 = num3;
                num3 = num2;
                num2 = num4;
                int index2 = 0;
                do {
                    int num5 = c[index2];
                    c[index2] = c[3 + index2];
                    c[3 + index2] = num5;
                    ++index2;
                }
                while (index2 <= 2);
            }
            int num6 = 0;
            do {
                c[num6 + 0] = cFSHImage.ExtendX5(c[num6 + 0]);
                c[num6 + 1] = cFSHImage.ExtendX6(c[num6 + 1]);
                c[num6 + 2] = cFSHImage.ExtendX5(c[num6 + 2]);
                num6 += 3;
            }
            while (num6 <= 3);
            int num7;
            if (method == 1) {
                num7 = 3;
                cFSHImage.C2C3fromC0C1DXTC4(c);
            } else {
                num7 = 2;
                cFSHImage.C2C3fromC0C1DXTC3(c);
            }
            int num8 = 0;
            int num9 = 0;
            int index3 = 0;
            do {
                if (OutFormat == cFSHImage.FSHBmpType.BMP_DXT1 && method == 2 && src[index3] == 0) {
                    index1 = 3;
                } else {
                    int num10 = 16777216;
                    int num11 = num7;
                    for (int index4 = 0; index4 <= num11; ++index4) {
                        int num12 = 0;
                        int num13 = 0;
                        do {
                            int num14 = src[index3 + num13 + 1] - c[3 * index4 + num13];
                            num12 += num14 * num14;
                            ++num13;
                        }
                        while (num13 <= 2);
                        if (num12 < num10) {
                            num10 = num12;
                            index1 = index4;
                            if (num10 == 0)
                                index4 = num7;
                        }
                    }
                }
                if (index3 < 32)
                    num8 = num8 << 2 | index1;
                else
                    num9 = num9 << 2 | index1;
                index3 += 4;
            }
            while (index3 <= 63);
            dest[destp] = (byte) (num2 & byte.MaxValue);
            ++destp;
            dest[destp] = (byte) (num2 / 256 & byte.MaxValue);
            ++destp;
            dest[destp] = (byte) (num3 & byte.MaxValue);
            ++destp;
            dest[destp] = (byte) (num3 / 256 & byte.MaxValue);
            ++destp;
            dest[destp] = (byte) (num9 & byte.MaxValue);
            ++destp;
            dest[destp] = (byte) (num9 / 256 & byte.MaxValue);
            ++destp;
            dest[destp] = (byte) (num8 & byte.MaxValue);
            ++destp;
            dest[destp] = (byte) (num8 / 256 & byte.MaxValue);
            ++destp;
        }

        private static int TruncateX4(int v) {
            if (v < 0)
                v = 0;
            if (v > byte.MaxValue)
                v = byte.MaxValue;
            return v + 7 + (v >> 7) - (v >> 4) & 240;
        }

        private static int TruncateX5(int v) {
            if (v <= 0)
                v = 0;
            if (v > byte.MaxValue)
                v = byte.MaxValue;
            return v + 4 - (v >> 5) & 248;
        }

        private static int ExtendX5(int v) {
            if (v <= 0)
                v = 0;
            if (v > byte.MaxValue)
                v = byte.MaxValue;
            v &= 248;
            return v + (v >> 5);
        }

        private static int NearestX5(int v) {
            if (v <= 0)
                v = 0;
            if (v > byte.MaxValue)
                v = byte.MaxValue;
            v = v + 4 - (v >> 5) & 248;
            return v + (v >> 5);
        }

        private static int TruncateX6(int v) {
            if (v <= 0)
                v = 0;
            if (v > byte.MaxValue)
                v = byte.MaxValue;
            return v + 2 - (v >> 6) & 252;
        }

        private static int ExtendX6(int v) {
            if (v <= 0)
                v = 0;
            if (v > byte.MaxValue)
                v = byte.MaxValue;
            v &= 252;
            return v + (v >> 6);
        }

        private static int NearestX6(int v) {
            if (v <= 0)
                v = 0;
            if (v > byte.MaxValue)
                v = byte.MaxValue;
            v = v + 2 - (v >> 6) & 252;
            return v + (v >> 6);
        }

        private void BestPairOneColor(int[] c, byte r, byte g, byte b, int Method) {
            if (Method == 2) {
                c[0] = BestOne[8 * r + 0];
                c[1] = BestOne[8 * g + 4];
                c[2] = BestOne[8 * b + 0];
                c[3] = BestOne[8 * r + 1];
                c[4] = BestOne[8 * g + 5];
                c[5] = BestOne[8 * b + 1];
            } else {
                c[0] = BestOne[8 * r + 2];
                c[1] = BestOne[8 * g + 6];
                c[2] = BestOne[8 * b + 2];
                c[3] = BestOne[8 * r + 3];
                c[4] = BestOne[8 * g + 7];
                c[5] = BestOne[8 * b + 3];
            }
        }

        private int BestColourPairA(byte[] src, int[] c, ref int method) {
            cFSHImage.UniqueColor[] uc = new cFSHImage.UniqueColor[16];
            int num1 = 0;
            int n = 0;
            int index1 = 0;
            do {
                if (method == 1 | method == 2 & src[index1] > 0) {
                    ++num1;
                    int num2 = cFSHImage.NearestX5(src[index1 + 1]);
                    int num3 = cFSHImage.NearestX6(src[index1 + 2]);
                    int num4 = cFSHImage.NearestX5(src[index1 + 3]);
                    int num5 = num2 << 16 | num3 << 8 | num4;
                    int num6 = n - 1;
                    int index2 = 0;
                    while (index2 <= num6 && num5 != uc[index2].c)
                        ++index2;
                    if (index2 == n) {
                        ref cFSHImage.UniqueColor local = ref uc[index2];
                        local.a = src[index1];
                        local.r = (byte) num2;
                        local.g = (byte) num3;
                        local.b = (byte) num4;
                        local.c = num5;
                        local.i = (byte) index1;
                        local.w = 1;
                        ++n;
                    } else {
                        // ISSUE: variable of a reference type
                        byte&local;
                        // ISSUE: explicit reference operation
                        int num7 = (byte) ((uint) ^(local = ref uc[index2].w) + 1U);
                        local = (byte) num7;
                    }
                }
                index1 += 4;
            }
            while (index1 <= 63);
            int num8;
            if (num1 == 0) {
                num8 = 0;
            } else {
                int best0_1;
                int best1_1;
                switch (n) {
                    case 1:
                        BestPairOneColor(c, uc[0].r, uc[0].g, uc[0].b, method);
                        num8 = 1;
                        goto label_20;
                    case 2:
                        best0_1 = uc[0].i;
                        best1_1 = uc[1].i;
                        break;
                    case 3:
                    case 4:
                        if (method == 1) {
                            int best0_2;
                            int best1_2;
                            if (cFSHImage.BestMatchesAllPairs(1, src, uc, n, ref best0_1, ref best1_1) > cFSHImage.BestMatchesAllPairs(2, src, uc, n, ref best0_2, ref best1_2)) {
                                best0_1 = best0_2;
                                best1_1 = best1_2;
                                method = 2;
                                break;
                            }
                            break;
                        }
                        cFSHImage.BestMatchesAllPairs(2, src, uc, n, ref best0_1, ref best1_1);
                        break;
                    default:
                        cFSHImage.BestMatchesAllPairs(method, src, uc, n, ref best0_1, ref best1_1);
                        break;
                }
                c[0] = src[best0_1 + 1];
                c[1] = src[best0_1 + 2];
                c[2] = src[best0_1 + 3];
                c[3] = src[best1_1 + 1];
                c[4] = src[best1_1 + 2];
                c[5] = src[best1_1 + 3];
                num8 = n;
            }
        label_20:
            return num8;
        }

        private static int BestMatchesAllPairs(int method, byte[] src, cFSHImage.UniqueColor[] uc, int n, ref int best0, ref int best1) {
            int[] c = new int[12];
            int num1;
            cFSHImage.C23FromC01 c23FromC01;
            if (method == 1) {
                num1 = 3;
                c23FromC01 = new cFSHImage.C23FromC01(cFSHImage.C2C3fromC0C1DXTC4);
            } else {
                num1 = 2;
                c23FromC01 = new cFSHImage.C23FromC01(cFSHImage.C2C3fromC0C1DXTC3);
            }
            int num2 = 16777216;
            int num3 = n - 2;
            for (int index1 = 0; index1 <= num3; ++index1) {
                c[0] = uc[index1].r;
                c[1] = uc[index1].g;
                c[2] = uc[index1].b;
                int num4 = index1 + 1;
                int num5 = n - 1;
                for (int index2 = num4; index2 <= num5; ++index2) {
                    c[3] = uc[index2].r;
                    c[4] = uc[index2].g;
                    c[5] = uc[index2].b;
                    c23FromC01(c);
                    int num6 = 0;
                    int num7 = n - 1;
                    for (int index3 = 0; index3 <= num7; ++index3) {
                        if (index3 != index1 & index3 != index2) {
                            int num8 = 16777216;
                            int i = uc[index3].i;
                            int num9 = num1;
                            for (int index4 = 0; index4 <= num9; ++index4) {
                                int num10 = 0;
                                int num11 = 0;
                                do {
                                    int num12 = src[i + num11 + 1] - c[3 * index4 + num11];
                                    num10 += num12 * num12;
                                    ++num11;
                                }
                                while (num11 <= 2);
                                if (num10 < num8) {
                                    num8 = num10;
                                    if (num8 == 0)
                                        break;
                                }
                            }
                            num6 += num8 * uc[index3].w;
                            if (num6 > num2)
                                break;
                        }
                    }
                    if (num6 < num2) {
                        num2 = num6;
                        best0 = uc[index1].i;
                        best1 = uc[index2].i;
                    }
                }
            }
            return num2;
        }

        private static void C2C3fromC0C1DXTC4(int[] c) {
            c[6] = (2 * c[0] + c[3]) / 3;
            c[7] = (2 * c[1] + c[4]) / 3;
            c[8] = (2 * c[2] + c[5]) / 3;
            c[9] = (c[0] + 2 * c[3]) / 3;
            c[10] = (c[1] + 2 * c[4]) / 3;
            c[11] = (c[2] + 2 * c[5]) / 3;
        }

        private static void C2C3fromC0C1DXTC3(int[] c) {
            c[6] = (c[0] + c[3]) / 2;
            c[7] = (c[1] + c[4]) / 2;
            c[8] = (c[2] + c[5]) / 2;
            c[9] = 0;
            c[10] = 0;
            c[11] = 0;
        }

        private static void PutPalette(ref byte[] data, ref int Offset, ref int[] Pal, int code) {
            cFSHImage.PutLong(ref data, Offset, code & byte.MaxValue);
            int length = Pal.Length;
            cFSHImage.PutShort(ref data, Offset + 4, length);
            cFSHImage.PutShort(ref data, Offset + 6, 1);
            cFSHImage.PutShort(ref data, Offset + 8, 256);
            Offset += 16;
            int num1;
            if (length == 256) {
                int index = 0;
                do {
                    Pal[index] = -16777216 | index << 16 | index << 8 | index;
                    ++index;
                }
                while (index <= byte.MaxValue);
            } else {
                int num2 = length - 1;
                for (int index = 0; index <= num2; ++index) {
                    num1 = index * byte.MaxValue / (length - 1);
                    Pal[index] = -16777216 | Information.RGB(num1, num1, num1);
                }
            }
            int num3;
            switch (code & byte.MaxValue) {
                case 34:
                    int num4 = length - 1;
                    for (int index = 0; index <= num4; ++index) {
                        int num5 = Pal[index] / 4;
                        data[Offset] = (byte) (num5 & 63);
                        ++Offset;
                        data[Offset] = (byte) (num5 / 256 & 63);
                        ++Offset;
                        data[Offset] = (byte) (num5 / 65536 & 63);
                        ++Offset;
                    }
                    break;
                case 36:
                    int num6 = length - 1;
                    for (int index = 0; index <= num6; ++index) {
                        data[Offset] = (byte) index;
                        ++Offset;
                        data[Offset] = (byte) index;
                        ++Offset;
                        data[Offset] = (byte) index;
                        ++Offset;
                    }
                    break;
                case 41:
                    int num7 = length - 1;
                    for (int index = 0; index <= num7; ++index) {
                        int num8 = Pal[index];
                        num3 = 2048 * ((num8 / 65536 & byte.MaxValue) / 8) | 32 * ((num8 / 256 & byte.MaxValue) / 4) | (num8 & byte.MaxValue) / 8;
                        data[Offset] = (byte) (num1 & byte.MaxValue);
                        ++Offset;
                        data[Offset] = (byte) (num1 / 256);
                        ++Offset;
                    }
                    break;
                case 42:
                    int num9 = length - 1;
                    for (int index = 0; index <= num9; ++index) {
                        int num10 = Pal[index];
                        Color color = Color.FromArgb(Pal[index]);
                        data[Offset] = color.B;
                        ++Offset;
                        data[Offset] = color.G;
                        ++Offset;
                        data[Offset] = color.R;
                        ++Offset;
                        data[Offset] = color.A;
                        ++Offset;
                    }
                    break;
                case 45:
                    int num11 = length - 1;
                    for (int index = 0; index <= num11; ++index) {
                        int num12 = Pal[index];
                        int num13 = 2048 * ((num12 / 65536 & byte.MaxValue) / 8) | 32 * ((num12 / 256 & byte.MaxValue) / 4) | (num12 & byte.MaxValue) / 8;
                        if ((Pal[index] & -16777216) != 0)
                            num3 = num13 | 32768;
                        data[Offset] = (byte) (num1 & byte.MaxValue);
                        ++Offset;
                        data[Offset] = (byte) (num1 / 256);
                        ++Offset;
                    }
                    break;
            }
        }

        public enum FSHBmpType {
            BMP_DXT1 = 96, // 0x00000060
            BMP_DXT3 = 97, // 0x00000061
            BMP_DXTAUTO = 98, // 0x00000062
            BMP_A4R4G4B4 = 109, // 0x0000006D
            BMP_R5G6B5 = 120, // 0x00000078
            BMP_PAL8 = 123, // 0x0000007B
            BMP_A8R8G8B8 = 125, // 0x0000007D
            BMP_A1R5G5B5 = 126, // 0x0000007E
            BMP_R8G8B8 = 127, // 0x0000007F
        }

        private enum NFSType {
            NFS_A6R6G6B6 = 34, // 0x00000022
            NFS_R8G8B8 = 36, // 0x00000024
            NFS_R5G6B5 = 41, // 0x00000029
            NFS_A8R8G8B8 = 42, // 0x0000002A
            NFS_A1R5G5B5 = 45, // 0x0000002D
        }

        public struct FSHBitMap {
            public cFSHImage.FSHBmpType FSHFormat;
            public int Width;
            public int Height;
            public int BPP;
            public int NColors;
            public int[] Palette;
            public byte[] PixBits;
            public int RowMod;
            public BITMAPFILEHEADER BMFH;
            public BITMAPINFOHEADER BMIH;
        }

        public enum FSHCmntType {
            Normal = 1,
            Enhanced = 2,
        }

        public struct FSHHeader {
            public string SHPI;
            public int FileSize;
            public int numBmps;
            public string DirID;
        }

        public struct FSHAttachHdr {
            public int code;
            public int Width;
            public int Height;
            public int Misc0;
            public int Misc1;
            public int Misc2;
            public int Misc3;
            public string[] data;
        }

        public struct FSHEntryHdr {
            public int code;
            public int Width;
            public int Height;
            public int Misc0;
            public int Misc1;
            public int Misc2;
            public int Misc3;
            public cFSHImage.FSHBitMap[] AlphaBM;
            public cFSHImage.FSHBitMap[] ColorBM;
            public int nBMPs;
            public cFSHImage.FSHAttachHdr[] Attach;
            public int NAttach;
        }

        public struct FSHDirEntry {
            public string EName;
            public int EOffset;
            public cFSHImage.FSHEntryHdr EHdr;
        }

        public struct FSHFile {
            public cFSHImage.FSHHeader Header;
            public cFSHImage.FSHDirEntry[] Directory;
            public string VendorAd;
        }

        public enum BMP_PRESET {
            BMP_COLOR,
            BMP_ALPHA,
        }

        private struct color {
            public byte B;
            public byte g;
            public byte r;
            public byte A;
        }

        private struct UniqueColor {
            public byte a;
            public byte r;
            public byte g;
            public byte b;
            public int c;
            public byte i;
            public byte w;
        }

        private enum BestComponents {
            X53LoEq,
            X53Hi,
            X54Hi,
            X54Lo,
            X63LoEq,
            X63Hi,
            X64Hi,
            X64Lo,
        }

        private delegate void C23FromC01(int[] c);
    }

    public struct BITMAPINFOHEADER {
        public int biSize;
        public int biWidth;
        public int biHeight;
        public short biPlanes;
        public short biBitCount;
        public int biCompression;
        public int biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public int biClrUsed;
        public int biClrImportant;
    }

    public struct BITMAPFILEHEADER {
        public short bfType;
        public int bfSize;
        public short bfReserved1;
        public short bfReserved2;
        public int bfOffBits;
    }
}

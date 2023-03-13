using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static csDBPF.Entries.DBPFEntryFSH;

namespace csDBPF.Entries {
    internal class DBPFEntryFSH_temp {
        private ArrayList bitmapItems;
        private FSHDirEntry[] directory;
        private FSHHeader fshHead;
        private bool isDirty;
        private bool isFSHComp;
        private byte[] rawData;
        private bool saveGlobPal;

        public DBPFEntryFSH_temp() {
            this.saveGlobPal = false;
            this.rawData = (byte[]) null;
            this.isFSHComp = false;
            this.isDirty = false;
            this.bitmapItems = new ArrayList();
            this.fshHead = new FSHHeader();
            this.directory = new FSHDirEntry[0];
        }

        public DBPFEntryFSH_temp(Stream stream) {
            this.saveGlobPal = false;
            this.rawData = (byte[]) null;
            this.isFSHComp = false;
            this.isDirty = false;
            this.bitmapItems = new ArrayList();
            this.Load(stream);
        }

        public byte[] Comp(byte[] data, bool incLen) {
            int length1 = 131072;
            int num1 = length1 - 1;
            int num2 = 50;
            int[,] numArray1 = new int[256, 256];
            int[] numArray2 = new int[length1];
            int num3 = 0;
            for (int index1 = 0; index1 < 256; ++index1) {
                for (int index2 = 0; index2 < 256; ++index2)
                    numArray1[index1, index2] = -1;
            }
            for (int index = 0; index < length1; ++index)
                numArray2[index] = -1;
            int length2 = data.Length;
            byte[] numArray3 = new byte[length2 + 1028];
            Array.Copy((Array) data, 0, (Array) numArray3, 0, length2);
            byte[] numArray4 = new byte[length2];
            numArray4[0] = (byte) 16;
            numArray4[1] = (byte) 251;
            numArray4[2] = (byte) (length2 >> 16);
            numArray4[3] = (byte) (length2 >> 8 & (int) byte.MaxValue);
            numArray4[4] = (byte) (length2 & (int) byte.MaxValue);
            int num4 = 5;
            int index3 = 0;
            int sourceIndex = 0;
            while (index3 < length2) {
                int num5 = numArray1[(int) numArray3[index3], (int) numArray3[index3 + 1]];
                int num6 = numArray2[index3 & num1] = num5;
                numArray1[(int) numArray3[index3], (int) numArray3[index3 + 1]] = index3;
                if (index3 < sourceIndex) {
                    ++index3;
                } else {
                    int num7 = 0;
                    for (int index4 = 0; num6 >= 0 && index3 - num6 < length1 && index4++ < num2; num6 = numArray2[num6 & num1]) {
                        int num8 = 2;
                        while ((int) numArray3[index3 + num8] == (int) numArray3[num6 + num8] && num8 < 1028)
                            ++num8;
                        if (num8 > num7) {
                            num7 = num8;
                            num3 = index3 - num6;
                        }
                    }
                    if (num7 > length2 - index3)
                        num7 = index3 - length2;
                    if (num7 <= 2)
                        num7 = 0;
                    if (num7 == 3 && num3 > 1024)
                        num7 = 0;
                    if (num7 == 4 && num3 > 16384)
                        num7 = 0;
                    if (num7 > 0) {
                        while (index3 - sourceIndex >= 4) {
                            int num9 = (index3 - sourceIndex) / 4 - 1;
                            if (num9 > 27)
                                num9 = 27;
                            byte[] numArray5 = numArray4;
                            int index5 = num4;
                            int destinationIndex = index5 + 1;
                            int num10 = (int) (byte) (224 + num9);
                            numArray5[index5] = (byte) num10;
                            int length3 = 4 * num9 + 4;
                            Array.Copy((Array) numArray3, sourceIndex, (Array) numArray4, destinationIndex, length3);
                            sourceIndex += length3;
                            num4 = destinationIndex + length3;
                        }
                        int num11 = index3 - sourceIndex;
                        if (num7 <= 10 && num3 <= 1024) {
                            byte[] numArray6 = numArray4;
                            int index6 = num4;
                            int num12 = index6 + 1;
                            int num13 = (int) (byte) ((num3 - 1 >> 8 << 5) + (num7 - 3 << 2) + num11);
                            numArray6[index6] = (byte) num13;
                            byte[] numArray7 = numArray4;
                            int index7 = num12;
                            num4 = index7 + 1;
                            int num14 = (int) (byte) (num3 - 1 & (int) byte.MaxValue);
                            numArray7[index7] = (byte) num14;
                            while (num11-- > 0)
                                numArray4[num4++] = numArray3[sourceIndex++];
                            sourceIndex += num7;
                        } else if (num7 <= 67 && num3 <= 16384) {
                            byte[] numArray8 = numArray4;
                            int index8 = num4;
                            int num15 = index8 + 1;
                            int num16 = (int) (byte) (128 + (num7 - 4));
                            numArray8[index8] = (byte) num16;
                            byte[] numArray9 = numArray4;
                            int index9 = num15;
                            int num17 = index9 + 1;
                            int num18 = (int) (byte) ((num11 << 6) + (num3 - 1 >> 8));
                            numArray9[index9] = (byte) num18;
                            byte[] numArray10 = numArray4;
                            int index10 = num17;
                            num4 = index10 + 1;
                            int num19 = (int) (byte) (num3 - 1 & (int) byte.MaxValue);
                            numArray10[index10] = (byte) num19;
                            while (num11-- > 0)
                                numArray4[num4++] = numArray3[sourceIndex++];
                            sourceIndex += num7;
                        } else if (num7 <= 1028 && num3 < length1) {
                            --num3;
                            byte[] numArray11 = numArray4;
                            int index11 = num4;
                            int num20 = index11 + 1;
                            int num21 = (int) (byte) (192 + (num3 >> 16 << 4) + (num7 - 5 >> 8 << 2) + num11);
                            numArray11[index11] = (byte) num21;
                            byte[] numArray12 = numArray4;
                            int index12 = num20;
                            int num22 = index12 + 1;
                            int num23 = (int) (byte) (num3 >> 8 & (int) byte.MaxValue);
                            numArray12[index12] = (byte) num23;
                            byte[] numArray13 = numArray4;
                            int index13 = num22;
                            int num24 = index13 + 1;
                            int num25 = (int) (byte) (num3 & (int) byte.MaxValue);
                            numArray13[index13] = (byte) num25;
                            byte[] numArray14 = numArray4;
                            int index14 = num24;
                            num4 = index14 + 1;
                            int num26 = (int) (byte) (num7 - 5 & (int) byte.MaxValue);
                            numArray14[index14] = (byte) num26;
                            while (num11-- > 0)
                                numArray4[num4++] = numArray3[sourceIndex++];
                            sourceIndex += num7;
                        }
                    }
                    ++index3;
                }
            }
            int num27 = length2;
            while (num27 - sourceIndex >= 4) {
                int num28 = (num27 - sourceIndex) / 4 - 1;
                if (num28 > 27)
                    num28 = 27;
                byte[] numArray15 = numArray4;
                int index15 = num4;
                int destinationIndex = index15 + 1;
                int num29 = (int) (byte) (224 + num28);
                numArray15[index15] = (byte) num29;
                int length4 = 4 * num28 + 4;
                Array.Copy((Array) numArray3, sourceIndex, (Array) numArray4, destinationIndex, length4);
                sourceIndex += length4;
                num4 = destinationIndex + length4;
            }
            int num30 = num27 - sourceIndex;
            byte[] numArray16 = numArray4;
            int index16 = num4;
            int length5 = index16 + 1;
            int num31 = (int) (byte) (252 + num30);
            numArray16[index16] = (byte) num31;
            while (num30-- > 0)
                numArray4[length5++] = numArray3[sourceIndex++];
            byte[] destinationArray1 = new byte[length5];
            Array.Copy((Array) numArray4, 0, (Array) destinationArray1, 0, length5);
            byte[] sourceArray = destinationArray1;
            if (incLen) {
                byte[] destinationArray2 = new byte[sourceArray.Length + 4];
                Array.Copy((Array) sourceArray, 0, (Array) destinationArray2, 4, sourceArray.Length);
                byte[] bytes = BitConverter.GetBytes(destinationArray2.Length);
                destinationArray2[0] = bytes[0];
                destinationArray2[1] = bytes[1];
                destinationArray2[2] = bytes[2];
                destinationArray2[3] = bytes[3];
                sourceArray = destinationArray2;
            }
            return sourceArray;
        }

        private Color[] CreatePalette(byte[] data, int pOfs) {
            FSHEntryHeader fshEntryHeader = new FSHEntryHeader();
            fshEntryHeader.code = this.GetInt(data, pOfs);
            fshEntryHeader.Width = this.GetShort(data, pOfs + 4);
            fshEntryHeader.height = this.GetShort(data, pOfs + 6);
            fshEntryHeader.misc = new short[4];
            Array.Copy((Array) data, pOfs + 8, (Array) fshEntryHeader.misc, 0, 4);
            int num = fshEntryHeader.code & (int) byte.MaxValue;
            int width = (int) fshEntryHeader.Width;
            int index1 = pOfs + 16;
            Color[] palette = new Color[width];
            for (int index2 = 0; index2 < width; ++index2)
                palette[index2] = Color.FromArgb(0);
            switch (num) {
                case 34:
                    int index3 = 0;
                    while (index3 < width) {
                        palette[index3] = Color.FromArgb(65536 * (int) data[index1] + 256 * (int) data[index1 + 1] + (int) data[index1 + 2] << 2);
                        ++index3;
                        index1 += 3;
                    }
                    return palette;
                case 36:
                    int index4 = 0;
                    while (index4 < width) {
                        palette[index4] = Color.FromArgb((int) byte.MaxValue, (int) data[index1], (int) data[index1 + 1], (int) data[index1 + 2]);
                        ++index4;
                        index1 += 3;
                    }
                    return palette;
                case 41:
                    ushort index5 = (ushort) index1;
                    int index6 = 0;
                    while (index6 < width) {
                        palette[index6] = Color.FromArgb(((int) data[(int) index5] & 31) + 256 * ((int) data[(int) index5 >> 5] & 63) + 65536 * ((int) data[(int) index5 >> 10] & 31) << 3);
                        ++index6;
                        ++index5;
                    }
                    return palette;
                case 45:
                    ushort index7 = (ushort) index1;
                    int index8 = 0;
                    while (index8 < width) {
                        palette[index8] = Color.FromArgb(((int) data[(int) index7] & 31) + 256 * ((int) data[(int) index7 >> 5] & 31) + 65536 * ((int) data[(int) index7 >> 10] & 31) << 3);
                        if (((int) data[(int) index7] & 32768) > 0) {
                            uint argb = (uint) (palette[index8].ToArgb() - 16777216);
                            palette[index8] = Color.FromArgb((int) argb);
                        }
                        ++index8;
                        ++index7;
                    }
                    return palette;
                default:
                    if (num == 42) {
                        int index9 = 0;
                        while (index9 < width) {
                            palette[index9] = Color.FromArgb((int) data[index1]);
                            ++index9;
                            index1 += 4;
                        }
                    }
                    return palette;
            }
        }

        public byte[] Decomp(byte[] data) {
            int index1 = 0;
            if (((int) data[0] & 254) * 256 + (int) data[1] != 4347) {
                if (((int) data[4] & 254) * 256 + (int) data[5] != 4347)
                    throw new NotSupportedException("The pack code is incorrect. This is either not a QFS file, or is of an unknown type.");
                index1 = 4;
            }
            int length1 = ((int) data[2 + index1] << 16) + ((int) data[3 + index1] << 8) + (int) data[4 + index1];
            int index2 = 5 + index1;
            int num1 = 0;
            if (((int) data[index1] & 1) > 0)
                index2 = 8;
            byte[] destinationArray;
            try {
                destinationArray = new byte[length1];
                while (index2 < data.Length && data[index2] < (byte) 252) {
                    byte num2 = data[index2];
                    int num3 = (int) data[index2 + 1];
                    int num4 = (int) data[index2 + 2];
                    if (((int) num2 & 128) == 0) {
                        int num5 = (int) num2 & 3;
                        int num6 = num1;
                        int num7 = index2 + 2;
                        int num8 = num1 + num5;
                        index2 += num5 + 2;
                        while (num5-- > 0)
                            destinationArray[num6++] = data[num7++];
                        int num9 = (((int) num2 & 28) >> 2) + 3;
                        int num10 = ((int) num2 >> 5 << 8) + num3 + 1;
                        int num11 = num8;
                        int num12 = num11 - num10;
                        num1 = num8 + num9;
                        while (num9-- > 0)
                            destinationArray[num11++] = destinationArray[num12++];
                    } else if (((int) num2 & 64) == 0) {
                        int num13 = num3 >> 6 & 3;
                        int num14 = num1;
                        int num15 = index2 + 3;
                        index2 += num13 + 3;
                        int num16 = num1 + num13;
                        while (num13-- > 0)
                            destinationArray[num14++] = data[num15++];
                        int num17 = ((int) num2 & 63) + 4;
                        int num18 = (num3 & 63) * 256 + num4 + 1;
                        int num19 = num16;
                        int num20 = num19 - num18;
                        num1 = num16 + num17;
                        while (num17-- > 0)
                            destinationArray[num19++] = destinationArray[num20++];
                    } else if (((int) num2 & 32) == 0) {
                        int num21 = (int) data[index2 + 3];
                        int num22 = (int) num2 & 3;
                        int num23 = num1;
                        int num24 = index2 + 4;
                        index2 += num22 + 4;
                        int num25 = num1 + num22;
                        while (num22-- > 0)
                            destinationArray[num23++] = data[num24++];
                        int num26 = ((int) num2 >> 2 & 3) * 256 + num21 + 5;
                        int num27 = (((int) num2 & 16) << 12) + 256 * num3 + num4 + 1;
                        int num28 = num25;
                        int num29 = num28 - num27;
                        num1 = num25 + num26;
                        while (num26-- > 0)
                            destinationArray[num28++] = destinationArray[num29++];
                    } else {
                        int num30 = ((int) num2 & 31) * 4 + 4;
                        int num31 = num1;
                        int num32 = index2 + 1;
                        index2 += num30 + 1;
                        num1 += num30;
                        while (num30-- > 0)
                            destinationArray[num31++] = data[num32++];
                    }
                }
                if (index2 < data.Length) {
                    if (num1 < length1) {
                        int destinationIndex = num1;
                        int sourceIndex = index2 + 1;
                        int length2 = (int) data[index2] & 3;
                        Array.Copy((Array) data, sourceIndex, (Array) destinationArray, destinationIndex, length2);
                    }
                }
            }
            catch (OutOfMemoryException ex) {
                destinationArray = (byte[]) null;
            }
            return destinationArray;
        }

        public FSHEntryHeader GetEntryHeader(int offset) {
            FSHEntryHeader entryHeader = new FSHEntryHeader();
            entryHeader.code = -1;
            if (this.rawData != null && this.rawData.Length > offset + 16) {
                entryHeader.code = this.GetInt(this.rawData, offset);
                entryHeader.Width = this.GetShort(this.rawData, offset + 4);
                entryHeader.height = this.GetShort(this.rawData, offset + 6);
                entryHeader.misc = new short[4];
                Array.Copy((Array) this.rawData, offset + 8, (Array) entryHeader.misc, 0, 4);
            }
            return entryHeader;
        }

        private int GetInt(byte[] data, int offset) => (int) data[offset] + ((int) data[offset + 1] << 8) + ((int) data[offset + 2] << 16) + ((int) data[offset + 3] << 24);

        private short GetShort(byte[] data, int offset) => (short) ((int) data[offset] + ((int) data[offset + 1] << 8));

        public void Load(Stream s) {
            this.fshHead = new FSHHeader();
            int size = this.fshHead.FileSize;
            ArrayList arrayList = new ArrayList(2);
            FSHEntryHeader fshEntryHeader1 = new FSHEntryHeader();
            int num1 = 0;
            byte[] buffer = new byte[2];
            s.Read(buffer, 0, 2);
            if (buffer[0] == (byte) 16 && buffer[1] == (byte) 251) {
                s.Position = 0L;
                byte[] numArray = new byte[(IntPtr) (uint) s.Length];
                s.Read(numArray, 0, (int) s.Length);
                this.rawData = this.Decomp(numArray);
                this.isFSHComp = true;
            } else {
                s.Position = 4L;
                s.Read(buffer, 0, 2);
                if (buffer[0] == (byte) 16 && buffer[1] == (byte) 251) {
                    s.Position = 0L;
                    byte[] numArray = new byte[(IntPtr) (uint) s.Length];
                    s.Read(numArray, 0, (int) s.Length);
                    this.rawData = this.Decomp(numArray);
                    this.isFSHComp = true;
                }
            }
            if (this.rawData == null) {
                s.Position = 0L;
                this.rawData = new byte[(IntPtr) (uint) s.Length];
                s.Read(this.rawData, 0, (int) s.Length);
            }

            //Header
            if (this.rawData.Length <= 4)
                throw new Exception("FSHImage: The file is truncated and invalid.");
            this.fshHead.SHPI = new byte[4];
            Array.Copy((Array) this.rawData, 0, (Array) this.fshHead.SHPI, 0, 4);
            if (Encoding.ASCII.GetString(this.fshHead.SHPI) != "SHPI")
                throw new Exception("FSHImage: An invalid header was read.");
            this.fshHead.FileSize = this.GetInt(this.rawData, 4);
            this.fshHead.BitmapCount = this.GetInt(this.rawData, 8);
            this.fshHead.dirID = new byte[4];
            Array.Copy((Array) this.rawData, 12, (Array) this.fshHead.dirID, 0, 4);
            int num2 = 16;
            int numBmps = this.fshHead.BitmapCount;

            //Directory
            FSHDirEntry[] fshDirEntryArray = new FSHDirEntry[numBmps];
            for (int index = 0; index < numBmps; ++index) {
                FSHDirEntry fshDirEntry = fshDirEntryArray[index] with {
                    name = new byte[4]
                };
                Array.Copy((Array) this.rawData, num2 + 8 * index, (Array) fshDirEntry.name, 0, 4);
                fshDirEntry.offset = this.GetInt(this.rawData, num2 + 8 * index + 4);
                fshDirEntryArray[index] = fshDirEntry;
            }
            this.directory = fshDirEntryArray;

            //Examine each bitmap
            for (int index1 = 0; index1 < numBmps; ++index1) {
                FSHDirEntry fshDirEntry = fshDirEntryArray[index1];
                int offset1 = fshDirEntry.offset;

                //not sure what this code block is doing?
                int num3 = this.fshHead.FileSize;
                for (int index2 = 0; index2 < numBmps; ++index2) {
                    if (fshDirEntryArray[index2].offset < num3 && fshDirEntryArray[index2].offset > offset1)
                        num3 = fshDirEntryArray[index2].offset;
                }
                if ((index1 != numBmps - 1 || num3 == this.fshHead.FileSize) && index1 < numBmps - 1) {
                    int offset2 = fshDirEntryArray[index1 + 1].offset;
                }

                //fill the entry header
                FSHEntryHeader fshEntryHeader2 = new FSHEntryHeader();
                fshEntryHeader2.code = this.GetInt(this.rawData, offset1);
                fshEntryHeader2.width = this.GetShort(this.rawData, offset1 + 4);
                fshEntryHeader2.height = this.GetShort(this.rawData, offset1 + 6);
                fshEntryHeader2.misc = new short[4];
                Array.Copy((Array) this.rawData, offset1 + 8, (Array) fshEntryHeader2.misc, 0, 4);

                //check if the bitmap code is valid
                int bitmapcode = fshEntryHeader2.code & (int) sbyte.MaxValue;
                int bitmapcodevalid;
                switch (bitmapcode) {
                    case 97:
                    case 109:
                    case 120:
                    case 123:
                    case 125:
                    case 126:
                    case (int) sbyte.MaxValue:
                        bitmapcodevalid = 1;
                        break;
                    default:
                        bitmapcodevalid = bitmapcode == 96 ? 1 : 0;
                        break;
                }
                bool isbitmapcodevalid = bitmapcodevalid != 0;

                //If the code is valid
                bool flag2 = (fshEntryHeader2.code & 128) > 0;
                if (isbitmapcodevalid) {

                    //why are we creating a new instance??? in a loop?
                    FSHEntryHeader fshEntryHeader3 = fshEntryHeader2;
                    int offset3 = offset1;
                    int num6 = 0;
                    int pOfs = 0;
                    while (fshEntryHeader3.code >> 8 > 0) {
                        ++num6;
                        offset3 += fshEntryHeader3.code >> 8;
                        if (offset3 != num3) {
                            fshEntryHeader3 = new FSHEntryHeader();
                            fshEntryHeader3.code = this.GetInt(this.rawData, offset3);
                            fshEntryHeader3.width = this.GetShort(this.rawData, offset3 + 4);
                            fshEntryHeader3.height = this.GetShort(this.rawData, offset3 + 6);
                            fshEntryHeader3.misc = new short[4];
                            Array.Copy((Array) this.rawData, offset3 + 8, (Array) fshEntryHeader3.misc, 0, 4);
                            int num7 = fshEntryHeader3.code & (int) byte.MaxValue;
                            if ((fshEntryHeader2.code & (int) sbyte.MaxValue) == 123 && (num7 == 34 || num7 == 36 || num7 == 45 || num7 == 42 || num7 == 41)) {
                                fshEntryHeader1 = fshEntryHeader3;
                                pOfs = offset3;
                            }
                        } else
                            break;
                    }
                    int num8 = 0;

                    //Not quite sure what flag2 is supposed to accomplish.
                    if (!flag2) {
                        if (((int) fshEntryHeader2.misc[3] & (int) ushort.MaxValue) == 0)
                            num8 = (int) fshEntryHeader2.misc[3] >> 12 & 15;
                        if ((int) fshEntryHeader2.width % (1 << num8) > 0 || (int) fshEntryHeader2.height % (1 << num8) > 0)
                            num8 = 0;
                        if (num8 > 0) {
                            int num9;
                            if (bitmapcode == 123 || bitmapcode == 97) {
                                num9 = 2;
                            } else {
                                switch (bitmapcode) {
                                    case 96:
                                        num9 = 1;
                                        break;
                                    case 125:
                                        num9 = 8;
                                        break;
                                    case (int) sbyte.MaxValue:
                                        num9 = 6;
                                        break;
                                    default:
                                        num9 = 4;
                                        break;
                                }
                            }
                            int num10;
                            int num11 = num10 = 0;
                            for (int index3 = 0; index3 <= num8; ++index3) {
                                int num12 = (int) fshEntryHeader2.width >> index3;
                                if ((fshEntryHeader2.code & 126) == 96)
                                    num12 += 4 - num12 & 3;
                                int num13 = (int) fshEntryHeader2.height >> index3;
                                if ((fshEntryHeader2.code & 126) == 96)
                                    num13 += 4 - num13 & 3;
                                num11 += num12 * num13 * num9 / 2;
                                num10 += num12 * num13 * num9 / 2;
                                if ((fshEntryHeader2.code & 126) != 96) {
                                    num11 += 16 - num11 & 15;
                                    if (index3 == num8)
                                        num10 += 16 - num10 & 15;
                                }
                            }
                            num1 = 0;
                            if (fshEntryHeader2.code >> 8 != num11 + 16 && fshEntryHeader2.code >> 8 != 0 || fshEntryHeader2.code >> 8 == 0 && num11 + offset1 + 16 != num3) {
                                num1 = 1;
                                if (fshEntryHeader2.code >> 8 != num10 + 16 && fshEntryHeader2.code >> 8 != 0 || fshEntryHeader2.code >> 8 == 0 && num10 + offset1 + 16 != num3)
                                    num8 = 0;
                            }
                        }
                    }


                    if (isbitmapcodevalid) {
                        int num14 = num8;
                        int num15 = offset1 + 16;
                        int num16 = num3 - num15;
                        for (; num14 >= 0; --num14) {
                            Color[] colorArray = new Color[0];
                            int[,] numArray1 = new int[(int) (IntPtr) (uint) fshEntryHeader2.height, (int) (IntPtr) (uint) fshEntryHeader2.width];
                            int[,] numArray2 = new int[(int) (IntPtr) (uint) fshEntryHeader2.height, (int) (IntPtr) (uint) fshEntryHeader2.width];
                            PixelFormat format = PixelFormat.Format32bppArgb;
                            FSHBmpType fshBmpType = FSHBmpType.ThirtyTwoBit;
                            switch (bitmapcode) {
                                case 96:
                                    format = PixelFormat.Format32bppArgb;
                                    fshBmpType = FSHBmpType.DXT1;
                                    for (int numArr2row = (int) fshEntryHeader2.height - 1; numArr2row >= 0; --numArr2row) {
                                        for (int numArr2col = 0; numArr2col < (int) fshEntryHeader2.width; ++numArr2col)
                                            numArray2[numArr2row, numArr2col] = -1;
                                    }
                                    int heightminus1 = (int) fshEntryHeader2.height - 1;
                                    int index7 = 0;
                                    byte[] target = new byte[12 * ((int) fshEntryHeader2.width / 4)];
                                    for (int pxlrow = (int) fshEntryHeader2.height / 4 - 1; pxlrow >= 0; --pxlrow) {
                                        int num17 = num15 + 2 * pxlrow * (int) fshEntryHeader2.width;
                                        for (int pxlcol = 7; pxlcol >= 4; --pxlcol) {
                                            for (int index10 = 0; index10 < (int) fshEntryHeader2.width / 4; ++index10) {
                                                int color0and1 = this.GetInt(this.rawData, num17 + 8 * index10);
                                                this.UnpackDXT((byte) (this.rawData[num17 + 8 * index10 + pxlcol] & 3U), (ushort) color0and1, (ushort) (color0and1 >> 16), target, 12 * index10);
                                                this.UnpackDXT((byte) (this.rawData[num17 + 8 * index10 + pxlcol] >> 2 & 3), (ushort) color0and1, (ushort) (color0and1 >> 16), target, 12 * index10 + 3);
                                                this.UnpackDXT((byte) (this.rawData[num17 + 8 * index10 + pxlcol] >> 4 & 3), (ushort) color0and1, (ushort) (color0and1 >> 16), target, 12 * index10 + 6);
                                                this.UnpackDXT((byte) (this.rawData[num17 + 8 * index10 + pxlcol] >> 6 & 3), (ushort) color0and1, (ushort) (color0and1 >> 16), target, 12 * index10 + 9);
                                                if (index7 >= fshEntryHeader2.width) {
                                                    index7 = 0;
                                                    --heightminus1;
                                                }
                                                int num18 = 0;
                                                numArray1[heightminus1, index7] = target[12 * index10 + num18] + (target[12 * index10 + 1 + num18] << 8) + (target[12 * index10 + 2 + num18] << 16) - 16777216;
                                                int index11 = index7 + 1;
                                                int num19 = num18 + 3;
                                                numArray1[heightminus1, index11] = target[12 * index10 + num19] + (target[12 * index10 + 1 + num19] << 8) + (target[12 * index10 + 2 + num19] << 16) - 16777216;
                                                int index12 = index11 + 1;
                                                int num20 = num19 + 3;
                                                numArray1[heightminus1, index12] = target[12 * index10 + num20] + (target[12 * index10 + 1 + num20] << 8) + ( target[12 * index10 + 2 + num20] << 16) - 16777216;
                                                int index13 = index12 + 1;
                                                int num21 = num20 + 3;
                                                numArray1[heightminus1, index13] = target[12 * index10 + num21] + (target[12 * index10 + 1 + num21] << 8) + (target[12 * index10 + 2 + num21] << 16) - 16777216;
                                                index7 = index13 + 1;
                                            }
                                        }
                                    }
                                    num15 += (int) fshEntryHeader2.width * (int) fshEntryHeader2.height / 2;
                                    break;
                                case 97:
                                    format = PixelFormat.Format32bppArgb;
                                    fshBmpType = FSHBmpType.DXT3;
                                    int index14 = (int) fshEntryHeader2.height - 1;
                                    int index15 = 0;
                                    byte[] numArray3 = new byte[(IntPtr) (uint) fshEntryHeader2.width];
                                    for (int index16 = (int) fshEntryHeader2.height / 4 - 1; index16 >= 0; --index16) {
                                        int num22 = num15 + 4 * index16 * (int) fshEntryHeader2.width;
                                        for (int index17 = 6; index17 >= 0; index17 -= 2) {
                                            for (int index18 = 0; index18 < (int) fshEntryHeader2.width / 4; ++index18) {
                                                numArray3[4 * index18] = (byte) ((uint) this.rawData[num22 + 16 * index18 + index17] & 15U);
                                                byte[] numArray4;
                                                IntPtr index19;
                                                (numArray4 = numArray3)[(int) (index19 = (IntPtr) (4 * index18))] = (byte) ((uint) numArray4[(int) index19] + (uint) (byte) ((uint) numArray3[4 * index18] << 4));
                                                numArray3[4 * index18 + 1] = (byte) ((uint) this.rawData[num22 + 16 * index18 + index17] >> 4);
                                                byte[] numArray5;
                                                IntPtr index20;
                                                (numArray5 = numArray3)[(int) (index20 = (IntPtr) (4 * index18 + 1))] = (byte) ((uint) numArray5[(int) index20] + (uint) (byte) ((uint) numArray3[4 * index18 + 1] << 4));
                                                numArray3[4 * index18 + 2] = (byte) ((uint) this.rawData[num22 + 16 * index18 + index17 + 1] & 15U);
                                                byte[] numArray6;
                                                IntPtr index21;
                                                (numArray6 = numArray3)[(int) (index21 = (IntPtr) (4 * index18 + 2))] = (byte) ((uint) numArray6[(int) index21] + (uint) (byte) ((uint) numArray3[4 * index18 + 2] << 4));
                                                numArray3[4 * index18 + 3] = (byte) ((uint) this.rawData[num22 + 16 * index18 + index17 + 1] >> 4);
                                                byte[] numArray7;
                                                IntPtr index22;
                                                (numArray7 = numArray3)[(int) (index22 = (IntPtr) (4 * index18 + 3))] = (byte) ((uint) numArray7[(int) index22] + (uint) (byte) ((uint) numArray3[4 * index18 + 3] << 4));
                                                if (index15 >= (int) fshEntryHeader2.width) {
                                                    index15 = 0;
                                                    --index14;
                                                }
                                                numArray2[index14, index15] = ((int) numArray3[4 * index18] << 24) + ((int) numArray3[4 * index18] << 16) + ((int) numArray3[4 * index18] << 8) + (int) numArray3[4 * index18];
                                                int index23 = index15 + 1;
                                                numArray2[index14, index23] = ((int) numArray3[4 * index18 + 1] << 24) + ((int) numArray3[4 * index18 + 1] << 16) + ((int) numArray3[4 * index18 + 1] << 8) + (int) numArray3[4 * index18 + 1];
                                                int index24 = index23 + 1;
                                                numArray2[index14, index24] = ((int) numArray3[4 * index18 + 2] << 24) + ((int) numArray3[4 * index18 + 2] << 16) + ((int) numArray3[4 * index18 + 2] << 8) + (int) numArray3[4 * index18 + 2];
                                                int index25 = index24 + 1;
                                                numArray2[index14, index25] = ((int) numArray3[4 * index18 + 3] << 24) + ((int) numArray3[4 * index18 + 3] << 16) + ((int) numArray3[4 * index18 + 3] << 8) + (int) numArray3[4 * index18 + 3];
                                                index15 = index25 + 1;
                                            }
                                        }
                                    }
                                    int num23 = 3 * (int) fshEntryHeader2.width;
                                    while ((num23 & 3) > 0)
                                        ++num23;
                                    int index26 = (int) fshEntryHeader2.height - 1;
                                    int index27 = 0;
                                    byte[] target2 = new byte[12 * ((int) fshEntryHeader2.width / 4)];
                                    for (int index28 = (int) fshEntryHeader2.height / 4 - 1; index28 >= 0; --index28) {
                                        int num24 = num15 + 4 * index28 * (int) fshEntryHeader2.width;
                                        for (int index29 = 15; index29 >= 12; --index29) {
                                            for (int index30 = 0; index30 < (int) fshEntryHeader2.width / 4; ++index30) {
                                                int c1 = this.GetInt(this.rawData, num24 + 16 * index30 + 8);
                                                this.UnpackDXT((byte) ((uint) this.rawData[num24 + 16 * index30 + index29] & 3U), (ushort) c1, (ushort) (c1 >> 16), target2, 12 * index30);
                                                this.UnpackDXT((byte) ((int) this.rawData[num24 + 16 * index30 + index29] >> 2 & 3), (ushort) c1, (ushort) (c1 >> 16), target2, 12 * index30 + 3);
                                                this.UnpackDXT((byte) ((int) this.rawData[num24 + 16 * index30 + index29] >> 4 & 3), (ushort) c1, (ushort) (c1 >> 16), target2, 12 * index30 + 6);
                                                this.UnpackDXT((byte) ((int) this.rawData[num24 + 16 * index30 + index29] >> 6 & 3), (ushort) c1, (ushort) (c1 >> 16), target2, 12 * index30 + 9);
                                                if (index27 >= (int) fshEntryHeader2.width) {
                                                    index27 = 0;
                                                    --index26;
                                                }
                                                int num25 = 0;
                                                int num26 = -16777216;
                                                numArray1[index26, index27] = (int) target2[12 * index30 + num25] + ((int) target2[12 * index30 + 1 + num25] << 8) + ((int) target2[12 * index30 + 2 + num25] << 16) + num26;
                                                int index31 = index27 + 1;
                                                int num27 = num25 + 3;
                                                numArray1[index26, index31] = (int) target2[12 * index30 + num27] + ((int) target2[12 * index30 + 1 + num27] << 8) + ((int) target2[12 * index30 + 2 + num27] << 16) + num26;
                                                int index32 = index31 + 1;
                                                int num28 = num27 + 3;
                                                numArray1[index26, index32] = (int) target2[12 * index30 + num28] + ((int) target2[12 * index30 + 1 + num28] << 8) + ((int) target2[12 * index30 + 2 + num28] << 16) + num26;
                                                int index33 = index32 + 1;
                                                int num29 = num28 + 3;
                                                numArray1[index26, index33] = (int) target2[12 * index30 + num29] + ((int) target2[12 * index30 + 1 + num29] << 8) + ((int) target2[12 * index30 + 2 + num29] << 16) + num26;
                                                index27 = index33 + 1;
                                            }
                                        }
                                    }
                                    num15 += (int) fshEntryHeader2.width * (int) fshEntryHeader2.height;
                                    break;
                                case 109:
                                    format = PixelFormat.Undefined;
                                    fshBmpType = FSHBmpType.SixteenBit4x4;
                                    break;
                                case 120:
                                    format = PixelFormat.Format32bppArgb;
                                    fshBmpType = FSHBmpType.SixteenBit;
                                    int num30 = 2 * (int) fshEntryHeader2.width;
                                    while ((num30 & 2) > 0)
                                        ++num30;
                                    int index34 = 0;
                                    for (int index35 = (int) fshEntryHeader2.height - 1; index34 < (int) fshEntryHeader2.height && index35 >= 0; --index35) {
                                        int num31 = num15 + 2 * index35 * (int) fshEntryHeader2.width;
                                        int num32 = 0;
                                        for (int index36 = 0; index36 < (int) fshEntryHeader2.width; ++index36) {
                                            try {
                                                short num33 = (short) (((int) this.rawData[num31 + 2 * num32] << 8) + (int) this.rawData[num31 + 2 * num32 + 1]);
                                                numArray1[index34, index36] = (int) num33;
                                                num32 += 2;
                                            }
                                            catch {
                                            }
                                        }
                                        ++index34;
                                    }
                                    num15 += 2 * (int) fshEntryHeader2.width * (int) fshEntryHeader2.height;
                                    break;
                                case 123:
                                    format = PixelFormat.Format8bppIndexed;
                                    fshBmpType = FSHBmpType.EightBit;
                                    if (pOfs > 0)
                                        colorArray = this.CreatePalette(this.rawData, pOfs);
                                    int width1 = (int) fshEntryHeader2.width;
                                    while ((width1 & 3) > 0)
                                        ++width1;
                                    int index37 = 0;
                                    for (int index38 = (int) fshEntryHeader2.height - 1; index37 < (int) fshEntryHeader2.height && index38 >= 0; --index38) {
                                        int num34 = 0;
                                        for (int index39 = 0; index39 < (int) fshEntryHeader2.width; ++index39) {
                                            numArray1[index37, index39] = (int) this.rawData[num15 + index37 * (int) fshEntryHeader2.width + num34];
                                            ++num34;
                                        }
                                        ++index37;
                                    }
                                    num15 += (int) fshEntryHeader2.width * (int) fshEntryHeader2.height;
                                    break;
                                case 125:
                                    format = PixelFormat.Format32bppArgb;
                                    fshBmpType = FSHBmpType.ThirtyTwoBit;
                                    int index40 = 0;
                                    for (int index41 = (int) fshEntryHeader2.height - 1; index40 < (int) fshEntryHeader2.height && index41 >= 0; --index41) {
                                        int num35 = num15 + 4 * index40 * (int) fshEntryHeader2.width;
                                        for (int index42 = 0; index42 < (int) fshEntryHeader2.width; ++index42)
                                            numArray2[index40, index42] = (int) this.rawData[num35 + 4 * index42 + 3] + ((int) this.rawData[num35 + 4 * index42 + 3] << 8) + ((int) this.rawData[num35 + 4 * index42 + 3] << 16) - 16777216;
                                        ++index40;
                                    }
                                    int num36 = 4 * (int) fshEntryHeader2.width;
                                    while ((num36 & 4) > 0)
                                        ++num36;
                                    int index43 = 0;
                                    for (int index44 = (int) fshEntryHeader2.height - 1; index43 < (int) fshEntryHeader2.height && index44 >= 0; --index44) {
                                        int num37 = num15 + 4 * index43 * (int) fshEntryHeader2.width;
                                        for (int index45 = 0; index45 < (int) fshEntryHeader2.width; ++index45)
                                            numArray1[index43, index45] = (int) this.rawData[num37 + 4 * index45] + ((int) this.rawData[num37 + 4 * index45 + 1] << 8) + ((int) this.rawData[num37 + 4 * index45 + 2] << 16) + ((int) this.rawData[num37 + 4 * index45 + 3] << 24);
                                        ++index43;
                                    }
                                    num15 += 4 * (int) fshEntryHeader2.width * (int) fshEntryHeader2.height;
                                    break;
                                case 126:
                                    format = PixelFormat.Format32bppArgb;
                                    fshBmpType = FSHBmpType.SixteenBitAlpha;
                                    int num38 = 2 * (int) fshEntryHeader2.width;
                                    while ((num38 & 2) > 0)
                                        ++num38;
                                    int index46 = 0;
                                    for (int index47 = (int) fshEntryHeader2.height - 1; index46 < (int) fshEntryHeader2.height && index47 >= 0; --index47) {
                                        int num39 = num15 + 2 * index47 * (int) fshEntryHeader2.width;
                                        int num40 = 0;
                                        for (int index48 = 0; index48 < (int) fshEntryHeader2.width; ++index48) {
                                            short num41 = (short) (((int) this.rawData[num39 + 2 * num40] << 8) + (int) this.rawData[num39 + 2 * num40 + 1]);
                                            numArray1[index46, index48] = (int) num41;
                                            num40 += 2;
                                        }
                                        ++index46;
                                    }
                                    num15 += 2 * (int) fshEntryHeader2.width * (int) fshEntryHeader2.height;
                                    break;
                                case (int) sbyte.MaxValue:
                                    format = PixelFormat.Format32bppArgb;
                                    fshBmpType = FSHBmpType.TwentyFourBit;
                                    int num42 = 3 * (int) fshEntryHeader2.width;
                                    while ((num42 & 3) > 0)
                                        ++num42;
                                    int index49 = 0;
                                    for (int index50 = (int) fshEntryHeader2.height - 1; index49 < (int) fshEntryHeader2.height && index50 >= 0; --index50) {
                                        int num43 = num15 + 3 * index49 * (int) fshEntryHeader2.width;
                                        for (int index51 = 0; index51 < (int) fshEntryHeader2.width; ++index51)
                                            numArray1[index49, index51] = (int) this.rawData[num43 + 3 * index51] + ((int) this.rawData[num43 + 3 * index51 + 1] << 8) + ((int) this.rawData[num43 + 3 * index51 + 2] << 16) - 16777216;
                                        ++index49;
                                    }
                                    num15 += 3 * (int) fshEntryHeader2.width * (int) fshEntryHeader2.height;
                                    break;
                            }
                            Bitmap bitmap1 = new Bitmap((int) fshEntryHeader2.width, (int) fshEntryHeader2.height, format);
                            if (colorArray.Length > 0) {
                                ColorPalette palette = bitmap1.Palette;
                                for (int index52 = 0; index52 < colorArray.Length; ++index52)
                                    palette.Entries[index52] = Color.FromArgb((int) byte.MaxValue, colorArray[index52]);
                                bitmap1.Palette = palette;
                            }
                            GraphicsUnit pageUnit = GraphicsUnit.Pixel;
                            RectangleF bounds = bitmap1.GetBounds(ref pageUnit);
                            Rectangle rect = new Rectangle((int) bounds.X, (int) bounds.Y, (int) bounds.Width, (int) bounds.Height);
                            BitmapData bitmapdata = bitmap1.LockBits(rect, ImageLockMode.ReadWrite, format);
                            switch (format) {
                                case PixelFormat.Format8bppIndexed:
                                    int width2 = (int) bounds.Width;
                                    if (width2 % 4 != 0)
                                        width2 = 4 * (width2 / 4 + 1);
                                    for (int y = 0; y < (int) fshEntryHeader2.height; ++y) {
                                        byte* numPtr = this.PixelAt(0, y, width2, (byte*) (void*) bitmapdata.Scan0, 1);
                                        for (int index53 = 0; index53 < (int) fshEntryHeader2.Width; ++index53) {
                                            *numPtr = (byte) numArray1[y, index53];
                                            ++numPtr;
                                        }
                                    }
                                    break;
                                case PixelFormat.Format32bppArgb:
                                    int width3 = (int) bounds.Width * 4;
                                    if (width3 % 4 != 0)
                                        width3 = 4 * (width3 / 4 + 1);
                                    for (int y = 0; y < (int) fshEntryHeader2.height; ++y) {
                                        byte* numPtr1 = this.PixelAt(0, y, width3, (byte*) (void*) bitmapdata.Scan0, 4);
                                        for (int index54 = 0; index54 < (int) fshEntryHeader2.Width; ++index54) {
                                            *numPtr1 = (byte) (numArray1[y, index54] & (int) byte.MaxValue);
                                            byte* numPtr2 = numPtr1 + 1;
                                            *numPtr2 = (byte) (numArray1[y, index54] >> 8 & (int) byte.MaxValue);
                                            byte* numPtr3 = numPtr2 + 1;
                                            *numPtr3 = (byte) (numArray1[y, index54] >> 16 & (int) byte.MaxValue);
                                            byte* numPtr4 = numPtr3 + 1;
                                            *numPtr4 = (byte) (numArray1[y, index54] >> 24 & (int) byte.MaxValue);
                                            numPtr1 = numPtr4 + 1;
                                        }
                                    }
                                    break;
                            }
                            bitmap1.UnlockBits(bitmapdata);
                            Bitmap bitmap2 = new Bitmap((int) fshEntryHeader2.width, (int) fshEntryHeader2.height, PixelFormat.Format32bppRgb);
                            for (int x = 0; x < (int) fshEntryHeader2.width; ++x) {
                                for (int y = 0; y < (int) fshEntryHeader2.height; ++y)
                                    bitmap2.SetPixel(x, y, Color.FromArgb(numArray2[y, x]));
                            }
                            BitmapItem bitmapItem = new BitmapItem();
                            bitmapItem.BmpType = fshBmpType;
                            bitmapItem.SetDirName(Encoding.ASCII.GetString(fshDirEntry.name));
                            bitmapItem.IsCompressed = flag2;
                            bitmapItem.Bitmap = bitmap1;
                            bitmapItem.Alpha = bitmap2;
                            bitmapItem.Palette = colorArray;
                            this.bitmapItems.Add((object) bitmapItem);
                            if (num8 > 0) {
                                fshEntryHeader2.width /= (short) 2;
                                fshEntryHeader2.height /= (short) 2;
                                if ((fshEntryHeader2.code & 126) == 96) {
                                    fshEntryHeader2.width += (short) (4 - (int) fshEntryHeader2.width & 3);
                                    fshEntryHeader2.height += (short) (4 - (int) fshEntryHeader2.height & 3);
                                } else {
                                    int num44 = num15 - offset1 & 15;
                                    if (num44 > 0 && num1 != 0)
                                        num15 += 16 - num44;
                                }
                            }
                        }
                        if (flag2) {
                            if (fshEntryHeader2.code >> 8 > 0)
                                num15 = offset1 + (fshEntryHeader2.code >> 8);
                            else
                                num15 = num3;
                        }
                        fshEntryHeader3 = fshEntryHeader2;
                        int offset4 = offset1;
                        try {
                            int num45 = num6;
                            while (num45 > 0) {
                                --num45;
                                offset4 += fshEntryHeader3.code >> 8;
                                fshEntryHeader3 = new FSHEntryHeader();
                                fshEntryHeader3.code = this.GetInt(this.rawData, offset4);
                                fshEntryHeader3.width = this.GetShort(this.rawData, offset4 + 4);
                                fshEntryHeader3.height = this.GetShort(this.rawData, offset4 + 6);
                                fshEntryHeader3.misc = new short[4];
                                for (int index55 = 0; index55 < 4; ++index55)
                                    fshEntryHeader3.misc[index55] = this.GetShort(this.rawData, offset4 + 8 + 2 * index1);
                                int num46 = fshEntryHeader3.code & (int) byte.MaxValue;
                                if ((fshEntryHeader2.code & (int) sbyte.MaxValue) == 123 && fshEntryHeader3.Equals((object) fshEntryHeader1)) {
                                    int num47;
                                    switch (num46) {
                                        case 41:
                                        case 45:
                                            num47 = 2;
                                            break;
                                        case 42:
                                            num47 = 4;
                                            break;
                                        default:
                                            num47 = 3;
                                            break;
                                    }
                                    num15 = offset4 + 16 + (int) fshEntryHeader3.width * num47;
                                } else if ((fshEntryHeader3.code & (int) byte.MaxValue) == 111)
                                    num15 = offset4 + 8 + (int) fshEntryHeader3.width;
                                else if ((fshEntryHeader3.code & (int) byte.MaxValue) == 105)
                                    num15 = offset4 + 16 + (int) fshEntryHeader3.width;
                                else if ((fshEntryHeader3.code & (int) byte.MaxValue) == 112) {
                                    num15 = offset4 + 16;
                                } else {
                                    int num48 = fshEntryHeader3.code >> 8;
                                    if (num48 == 0)
                                        num48 = num3 - offset4;
                                    if (num48 > 16384)
                                        throw new Exception("Data too large in FSH data attachment.");
                                    num15 = offset4 + num48;
                                }
                            }
                        }
                        catch {
                        }
                        if (num15 <= num3)
                            ;
                    } else {
                        switch (fshEntryHeader2.code & (int) byte.MaxValue) {
                            default:
                                continue;
                        }
                    }
                }
            }
        }

        private unsafe void PackDXT(ulong* px, byte* dest) {
            int nstep = 0;
            int num1 = 0;
            ulong[] numArray = new ulong[16];
            ulong col1 = 0;
            ulong col2 = 0;
            int num2 = 0;
            ulong num3;
            for (int index1 = 0; index1 < 16; ++index1) {
                num3 = px[index1] & 16317688UL;
                int index2 = 0;
                while (index2 < num2 && (long) numArray[index2] != (long) num3)
                    ++index2;
                if (index2 == num2)
                    numArray[num2++] = num3;
            }
            if (num2 == 1) {
                col1 = numArray[0];
                col2 = numArray[0];
                num1 = 1000;
                nstep = 3;
            } else {
                int num4 = 1073741824;
                for (int index3 = 0; index3 < num2 - 1; ++index3) {
                    for (int index4 = index3 + 1; index4 < num2; ++index4) {
                        ulong num5;
                        int num6 = this.ScoreDXT(px, 2, numArray[index3], numArray[index4], &num5);
                        if (num6 < num4) {
                            col1 = numArray[index3];
                            col2 = numArray[index4];
                            nstep = 2;
                            num4 = num6;
                        }
                        int num7 = this.ScoreDXT(px, 3, numArray[index3], numArray[index4], &num5);
                        if (num7 < num4) {
                            col1 = numArray[index3];
                            col2 = numArray[index4];
                            nstep = 3;
                            num4 = num7;
                        }
                    }
                }
            }
            byte* numPtr1 = (byte*) &num3;
            ulong num8;
            byte* numPtr2 = (byte*) &num8;
            ulong num9 = col1;
            ulong num10 = col2;
            ushort* numPtr3 = (ushort*) dest;
            *numPtr3 = (ushort) (((int) *numPtr1 >> 3) + ((int) numPtr1[1] >> 2 << 5) + ((int) numPtr1[2] >> 3 << 11));
            numPtr3[1] = (ushort) (((int) *numPtr2 >> 3) + ((int) numPtr2[1] >> 2 << 5) + ((int) numPtr2[2] >> 3 << 11));
            if ((int) *numPtr3 > (int) numPtr3[1] ^ nstep == 3) {
                ushort num11 = *numPtr3;
                *numPtr3 = numPtr3[1];
                numPtr3[1] = num11;
                col1 = num10;
                col2 = num9;
            }
            this.ScoreDXT(px, nstep, col1, col2, (ulong*) (dest + 4));
        }

        private unsafe byte* PixelAt(int x, int y, int width, byte* pBase, int strucSize) => pBase + ((IntPtr) y * width).ToInt64() + ((IntPtr) x * strucSize).ToInt64();

        public unsafe void Save(Stream stream) {
            try {
                MemoryStream memoryStream = new MemoryStream(1024);
                FSHHeader fshHeader = new FSHHeader();
                fshHeader.SHPI = Encoding.ASCII.GetBytes("SHPI");
                fshHeader.FileSize = 0;
                ref FSHHeader local = ref fshHeader;
                int count = this.bitmapItems.Count;
                int num1 = this.saveGlobPal ? 1 : 0;
                int num2;
                int length = num2 = count + num1;
                local.BitmapCount = num2;
                fshHeader.dirID = Encoding.ASCII.GetBytes("G264");
                memoryStream.Write(fshHeader.SHPI, 0, 4);
                memoryStream.Write(BitConverter.GetBytes(fshHeader.FileSize), 0, 4);
                memoryStream.Write(BitConverter.GetBytes(fshHeader.BitmapCount), 0, 4);
                memoryStream.Write(fshHeader.dirID, 0, 4);
                FSHDirEntry[] fshDirEntryArray = new FSHDirEntry[length];
                int index1 = 0;
                if (this.saveGlobPal) {
                    FSHDirEntry fshDirEntry = fshDirEntryArray[index1] with {
                        name = Encoding.ASCII.GetBytes("!PAL"),
                        offset = 16 + 8 * length
                    };
                    ++index1;
                    memoryStream.Write(fshDirEntry.name, 0, 4);
                    memoryStream.Write(BitConverter.GetBytes(fshDirEntry.offset), 0, 4);
                }
                int index2 = index1;
                for (int index3 = 0; index2 < length && index3 < this.bitmapItems.Count; ++index3) {
                    BitmapItem bitmapItem = (BitmapItem) this.bitmapItems[index3];
                    fshDirEntryArray[index2].name = bitmapItem.DirName;
                    fshDirEntryArray[index2].offset = 16 + 8 * length + index2;
                    memoryStream.Write(fshDirEntryArray[index2].name, 0, 4);
                    memoryStream.Write(BitConverter.GetBytes(fshDirEntryArray[index2].offset), 0, 4);
                    ++index2;
                }
                int num3 = 0;
                if (this.saveGlobPal)
                    ++num3;
                int index4 = num3;
                for (int index5 = 0; index4 < length && index5 < this.bitmapItems.Count; ++index5) {
                    BitmapItem bitmapItem = (BitmapItem) this.bitmapItems[index5];
                    fshDirEntryArray[index4].offset = (int) memoryStream.Position;
                    fshDirEntryArray[index4].name = bitmapItem.DirName;
                    FSHEntryHeader fshEntryHeader = new FSHEntryHeader();
                    fshEntryHeader.code = (int) bitmapItem.BmpType;
                    fshEntryHeader.height = (short) bitmapItem.Bitmap.Height;
                    fshEntryHeader.Width = (short) bitmapItem.Bitmap.Width;
                    fshEntryHeader.misc = new short[4];
                    memoryStream.Write(BitConverter.GetBytes(fshEntryHeader.code), 0, 4);
                    memoryStream.Write(BitConverter.GetBytes(fshEntryHeader.Width), 0, 2);
                    memoryStream.Write(BitConverter.GetBytes(fshEntryHeader.height), 0, 2);
                    for (int index6 = 0; index6 < 4; ++index6)
                        memoryStream.Write(BitConverter.GetBytes(fshEntryHeader.misc[index6]), 0, 2);
                    Bitmap bitmap = bitmapItem.Bitmap;
                    Bitmap alpha = bitmapItem.Alpha;
                    GraphicsUnit pageUnit = GraphicsUnit.Pixel;
                    RectangleF bounds1 = bitmap.GetBounds(ref pageUnit);
                    Rectangle rect1 = new Rectangle((int) bounds1.X, (int) bounds1.Y, (int) bounds1.Width, (int) bounds1.Height);
                    RectangleF bounds2 = alpha.GetBounds(ref pageUnit);
                    Rectangle rect2 = new Rectangle((int) bounds2.X, (int) bounds2.Y, (int) bounds2.Width, (int) bounds2.Height);
                    if (bitmapItem.BmpType == FSHBmpType.EightBit) {
                        BitmapData bitmapdata = bitmap.LockBits(rect1, ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
                        int width = (int) bounds1.Width;
                        if (width % 4 != 0)
                            width = 4 * (width / 4 + 1);
                        for (int y = (int) fshEntryHeader.height - 1; y >= 0; --y) {
                            byte* numPtr = this.PixelAt(0, y, width, (byte*) (void*) bitmapdata.Scan0, 1);
                            for (int index7 = 0; index7 < (int) fshEntryHeader.Width; ++index7) {
                                memoryStream.WriteByte(*numPtr);
                                ++numPtr;
                            }
                        }
                        bitmap.UnlockBits(bitmapdata);
                    } else if (bitmapItem.BmpType == FSHBmpType.SixteenBit) {
                        BitmapData bitmapdata = bitmap.LockBits(rect1, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                        int width = (int) bounds1.Width * 4;
                        if (width % 4 != 0)
                            width = 4 * (width / 4 + 1);
                        for (int y = 0; y < (int) fshEntryHeader.height; ++y) {
                            byte* numPtr = this.PixelAt(0, y, width, (byte*) (void*) bitmapdata.Scan0, 4);
                            for (int index8 = 0; index8 < (int) fshEntryHeader.Width; ++index8) {
                                byte[] bytes = BitConverter.GetBytes((ushort) (((int) numPtr[1] >> 3) + ((int) numPtr[2] >> 2 << 5) + ((int) numPtr[3] >> 3 << 11) & (int) ushort.MaxValue));
                                memoryStream.Write(bytes, 0, 2);
                                numPtr += 4;
                            }
                        }
                        bitmap.UnlockBits(bitmapdata);
                    } else if (bitmapItem.BmpType != FSHBmpType.SixteenBitAlpha) {
                        if (bitmapItem.BmpType == FSHBmpType.SixteenBit4x4) {
                            BitmapData bitmapdata1 = bitmap.LockBits(rect1, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                            BitmapData bitmapdata2 = alpha.LockBits(rect2, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                            int width = (int) bounds1.Width * 4;
                            if (width % 4 != 0)
                                width = 4 * (width / 4 + 1);
                            for (int y = 0; y < (int) fshEntryHeader.height; ++y) {
                                byte* numPtr1 = this.PixelAt(0, y, width, (byte*) (void*) bitmapdata1.Scan0, 4);
                                byte* numPtr2 = this.PixelAt(0, y, width, (byte*) (void*) bitmapdata2.Scan0, 4);
                                for (int index9 = 0; index9 < (int) fshEntryHeader.Width; ++index9) {
                                    byte[] bytes = BitConverter.GetBytes((ushort) ((int) numPtr2[1] >> 4 << 12 + ((int) numPtr1[1] >> 4) << 8 + ((int) numPtr1[2] >> 4) << 4 + ((int) numPtr1[3] >> 4) & (int) ushort.MaxValue));
                                    memoryStream.Write(bytes, 0, 2);
                                    numPtr1 += 4;
                                    numPtr2 += 4;
                                }
                            }
                            bitmap.UnlockBits(bitmapdata1);
                            alpha.UnlockBits(bitmapdata2);
                        } else if (bitmapItem.BmpType == FSHBmpType.TwentyFourBit) {
                            BitmapData bitmapdata = bitmap.LockBits(rect1, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                            int width = (int) bounds1.Width * 3;
                            if (width % 4 != 0)
                                width = 4 * (width / 4 + 1);
                            for (int y = 0; y < (int) fshEntryHeader.height; ++y) {
                                byte* numPtr = this.PixelAt(0, y, width, (byte*) (void*) bitmapdata.Scan0, 3);
                                for (int index10 = 0; index10 < (int) fshEntryHeader.Width; ++index10) {
                                    byte[] buffer = new byte[3]
                                    {
                                        *numPtr,
                                        numPtr[1],
                                        numPtr[2]
                                    };
                                    memoryStream.Write(buffer, 0, 3);
                                    numPtr += 3;
                                }
                            }
                            bitmap.UnlockBits(bitmapdata);
                        } else if (bitmapItem.BmpType == FSHBmpType.ThirtyTwoBit) {
                            BitmapData bitmapdata3 = bitmap.LockBits(rect1, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                            BitmapData bitmapdata4 = alpha.LockBits(rect2, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                            int width = (int) bounds1.Width * 4;
                            if (width % 4 != 0)
                                width = 4 * (width / 4 + 1);
                            for (int y = 0; y < (int) fshEntryHeader.height; ++y) {
                                byte* numPtr3 = this.PixelAt(0, y, width, (byte*) (void*) bitmapdata3.Scan0, 4);
                                byte* numPtr4 = this.PixelAt(0, y, width, (byte*) (void*) bitmapdata4.Scan0, 4);
                                for (int index11 = 0; index11 < (int) fshEntryHeader.Width; ++index11) {
                                    byte[] buffer = new byte[4]
                                    {
                                        *numPtr3,
                                        numPtr3[1],
                                        numPtr3[2],
                                        *numPtr4
                                    };
                                    memoryStream.Write(buffer, 0, 4);
                                    numPtr3 += 4;
                                    numPtr4 += 4;
                                }
                            }
                            bitmap.UnlockBits(bitmapdata3);
                            alpha.UnlockBits(bitmapdata4);
                        } else if (bitmapItem.BmpType == FSHBmpType.DXT3) {
                            int width = bitmapItem.Bitmap.Width;
                            int height = bitmapItem.Bitmap.Height;
                            BitmapData bitmapdata5 = bitmap.LockBits(rect1, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                            BitmapData bitmapdata6 = alpha.LockBits(rect2, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                            int num4 = (int) bounds1.Width * 4;
                            if (num4 % 4 != 0) {
                                int num5 = 4 * (num4 / 4 + 1);
                            }
                            if ((width & 3) <= 0 && (height & 3) <= 0) {
                                int num6 = 4 * width;
                                while ((num6 & 4) > 0)
                                    ++num6;
                                ulong[] numArray = new ulong[16];
                                byte[] buffer = new byte[width * height + 2048];
                                fixed (byte* numPtr5 = &buffer[0]) {
                                    byte* scan0_1 = (byte*) (void*) bitmapdata5.Scan0;
                                    for (int index12 = 0; index12 < height / 4; ++index12) {
                                        for (int index13 = 0; index13 < width / 4; ++index13) {
                                            for (int index14 = 0; index14 < 4; ++index14) {
                                                byte* numPtr6 = scan0_1 + ((IntPtr) (4 * index12 + index14) * num6).ToInt64() + (new IntPtr(16) * index13).ToInt64();
                                                for (int index15 = 0; index15 < 4; ++index15)
                                                    numArray[4 * index14 + index15] = (ulong) ((int) numPtr6[(new IntPtr(4) * index15).ToInt64()] + 256 * (int) numPtr6[4 * index15 + 1] + 65536 * (int) numPtr6[4 * index15 + 2]);
                                            }
                                            fixed (ulong* px = &numArray[0])
                                                this.PackDXT(px, numPtr5 + ((IntPtr) (4 * index12) * width).ToInt64() + (new IntPtr(16) * index13).ToInt64() + 8);
                                        }
                                    }
                                    byte* scan0_2 = (byte*) (void*) bitmapdata6.Scan0;
                                    for (int index16 = 0; index16 < height / 4; ++index16) {
                                        for (int index17 = 0; index17 < width / 4; ++index17) {
                                            for (int index18 = 0; index18 < 4; ++index18) {
                                                byte* numPtr7 = scan0_2 + ((IntPtr) (4 * index16 + index18) * num6).ToInt64() + (new IntPtr(16) * index17).ToInt64();
                                                byte* numPtr8 = numPtr5 + ((IntPtr) (4 * index16) * width).ToInt64() + (new IntPtr(16) * index17).ToInt64() + (new IntPtr(2) * index18).ToInt64();
                                                *numPtr8 = (byte) ((((int) *numPtr7 & 240) >> 4) + ((int) numPtr7[4] & 240));
                                                numPtr8[1] = (byte) ((((int) numPtr7[8] & 240) >> 4) + ((int) numPtr7[12] & 240));
                                            }
                                        }
                                    }
                                }
                                memoryStream.Write(buffer, 0, width * height);
                                bitmap.UnlockBits(bitmapdata5);
                                alpha.UnlockBits(bitmapdata6);
                            }
                        } else if (bitmapItem.BmpType == FSHBmpType.DXT1) {
                            int width = bitmapItem.Bitmap.Width;
                            int height = bitmapItem.Bitmap.Height;
                            BitmapData bitmapdata = bitmap.LockBits(rect1, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                            int num7 = (int) bounds1.Width * 4;
                            if (num7 % 4 != 0) {
                                int num8 = 4 * (num7 / 4 + 1);
                            }
                            if ((width & 3) <= 0 && (height & 3) <= 0) {
                                int num9 = 4 * width;
                                while ((num9 & 4) > 0)
                                    ++num9;
                                ulong[] numArray = new ulong[16];
                                byte[] buffer = new byte[width * height / 2 + 2048];
                                fixed (byte* numPtr9 = &buffer[0]) {
                                    byte* scan0 = (byte*) (void*) bitmapdata.Scan0;
                                    for (int index19 = 0; index19 < height / 4; ++index19) {
                                        for (int index20 = 0; index20 < width / 4; ++index20) {
                                            for (int index21 = 0; index21 < 4; ++index21) {
                                                byte* numPtr10 = scan0 + ((IntPtr) (4 * index19 + index21) * num9).ToInt64() + (new IntPtr(16) * index20).ToInt64();
                                                for (int index22 = 0; index22 < 4; ++index22)
                                                    numArray[4 * index21 + index22] = (ulong) ((int) numPtr10[(new IntPtr(4) * index22).ToInt64()] + 256 * (int) numPtr10[4 * index22 + 1] + 65536 * (int) numPtr10[4 * index22 + 2]);
                                            }
                                            fixed (ulong* px = &numArray[0])
                                                this.PackDXT(px, numPtr9 + ((IntPtr) (2 * index19) * width).ToInt64() + (new IntPtr(8) * index20).ToInt64());
                                        }
                                    }
                                }
                                memoryStream.Write(buffer, 0, width * height / 2);
                                bitmap.UnlockBits(bitmapdata);
                            }
                        }
                    }
                    ++index4;
                }
                int num10 = 0;
                memoryStream.Position = 16L;
                if (this.saveGlobPal) {
                    memoryStream.Position += 8L;
                    ++num10;
                }
                for (int index23 = num10; index23 < fshDirEntryArray.Length; ++index23) {
                    memoryStream.Position += 4L;
                    memoryStream.Write(BitConverter.GetBytes(fshDirEntryArray[index23].offset), 0, 4);
                }
                fshHeader.FileSize = (int) memoryStream.Length;
                memoryStream.Position = 4L;
                memoryStream.Write(BitConverter.GetBytes(fshHeader.FileSize), 0, 4);
                byte[] numArray1 = new byte[(IntPtr) (uint) memoryStream.Length];
                memoryStream.Position = 0L;
                memoryStream.Read(numArray1, 0, (int) memoryStream.Length);
                this.rawData = numArray1;
                this.directory = fshDirEntryArray;
                this.fshHead = fshHeader;
                if (this.IsCompressed)
                    numArray1 = this.Comp(numArray1, true);
                stream.Write(numArray1, 0, numArray1.Length);
            }
            catch (Exception ex) {
            }
        }

        private unsafe int ScoreDXT(ulong* px, int nstep, ulong col1, ulong col2, ulong* pack) {
            int[] numArray1 = new int[3];
            int[] numArray2 = new int[3];
            byte* numPtr1 = (byte*) &col1;
            byte* numPtr2 = (byte*) &col2;
            numArray2[0] = (int) *numPtr2 - (int) *numPtr1;
            numArray2[1] = (int) numPtr2[1] - (int) numPtr1[1];
            numArray2[2] = (int) numPtr2[2] - (int) numPtr1[2];
            int num1 = numArray2[0] * numArray2[0] + numArray2[1] * numArray2[1] + numArray2[2] * numArray2[2];
            int num2 = 0;
            *pack = 0UL;
            byte* numPtr3 = (byte*) (px + 15);
            int num3 = 15;
            while (num3 >= 0) {
                numArray1[0] = (int) *numPtr3 - (int) *numPtr1;
                numArray1[1] = (int) numPtr3[1] - (int) numPtr1[1];
                numArray1[2] = (int) numPtr3[2] - (int) numPtr1[2];
                int num4 = numArray1[0] * numArray1[0] + numArray1[1] * numArray1[1] + numArray1[2] * numArray1[2];
                int num5 = numArray1[0] * numArray2[0] + numArray1[1] * numArray2[1] + numArray1[2] * numArray2[2];
                int num6 = num1 == 0 ? 0 : (nstep * num5 + (num1 >> 1)) / num1;
                if (num6 < 0)
                    num6 = 0;
                if (num6 > nstep)
                    num6 = nstep;
                num2 += num4 - 2 * num6 * num5 / nstep + num6 * num6 * num1 / (nstep * nstep);
                *pack = *pack << 2;
                if (num6 == nstep) {
                    ulong* numPtr4 = pack;
                    long num7 = (long) *numPtr4 + 1L;
                    *numPtr4 = (ulong) num7;
                } else if (num6 != 0) {
                    ulong* numPtr5 = pack;
                    long num8 = (long) *numPtr5 + ((long) num6 + 1L);
                    *numPtr5 = (ulong) num8;
                }
                --num3;
                numPtr3 = (byte*) (px + num3);
            }
            return num2;
        }

        public void SetDirectoryName(string dirName) {
            Encoding.ASCII.GetBytes(dirName, 0, 4, this.fshHead.dirID, 0);
            this.isDirty = true;
        }

        private int UnpackDXT(byte mask, ushort c1, ushort c2, byte[] target, int idx) {
            ushort num1 = (ushort) (8 * ((int) c1 & 31));
            ushort num2 = (ushort) (4 * ((int) c1 >> 5 & 63));
            ushort num3 = (ushort) (8 * ((int) c1 >> 11));
            ushort num4 = (ushort) (8 * ((int) c2 & 31));
            ushort num5 = (ushort) (4 * ((int) c2 >> 5 & 63));
            ushort num6 = (ushort) (8 * ((int) c2 >> 11));
            switch (mask) {
                case 0:
                    target[idx] = (byte) num1;
                    target[idx + 1] = (byte) num2;
                    target[idx + 2] = (byte) num3;
                    break;
                case 1:
                    target[idx] = (byte) num4;
                    target[idx + 1] = (byte) num5;
                    target[idx + 2] = (byte) num6;
                    break;
                case 2:
                    if ((int) c1 <= (int) c2) {
                        target[idx] = (byte) (((int) num1 + (int) num4) / 2);
                        target[idx + 1] = (byte) (((int) num2 + (int) num5) / 2);
                        target[idx + 2] = (byte) (((int) num3 + (int) num6) / 2);
                        break;
                    }
                    target[idx] = (byte) ((2 * (int) num1 + (int) num4) / 3);
                    target[idx + 1] = (byte) ((2 * (int) num2 + (int) num5) / 3);
                    target[idx + 2] = (byte) ((2 * (int) num3 + (int) num6) / 3);
                    break;
                case 3:
                    if ((int) c1 <= (int) c2) {
                        target[idx] = (byte) 0;
                        target[idx + 1] = (byte) 0;
                        target[idx + 2] = (byte) 0;
                        break;
                    }
                    target[idx] = (byte) (((int) num1 + 2 * (int) num4) / 3);
                    target[idx + 1] = (byte) (((int) num2 + 2 * (int) num5) / 3);
                    target[idx + 2] = (byte) (((int) num3 + 2 * (int) num6) / 3);
                    break;
            }
            int num7;
            return num7 = ((int) target[idx] << 16) + ((int) target[idx + 1] << 8) + (int) target[idx + 2] | -16777216;
        }

        public void UpdateDirty() {
            this.fshHead.BitmapCount = this.bitmapItems.Count;
            this.isDirty = true;
        }

        public ArrayList Bitmaps => this.bitmapItems;

        public FSHDirEntry[] Directory => this.directory;

        public FSHHeader Header => this.fshHead;

        public bool IsCompressed {
            get => this.isFSHComp;
            set => this.isFSHComp = value;
        }

        public bool IsDirty => this.isDirty;

        public byte[] RawData => this.rawData;


        private struct FSHDirEntry {
            public byte[] name;
            public int offset;
        }

        private enum FSHBmpType : byte {
            DXT1 = 96, // 0x60
            DXT3 = 97, // 0x61
            SixteenBit4x4 = 109, // 0x6D
            SixteenBit = 120, // 0x78
            EightBit = 123, // 0x7B
            ThirtyTwoBit = 125, // 0x7D
            SixteenBitAlpha = 126, // 0x7E
            TwentyFourBit = 127, // 0x7F
        }

        private class BitmapItem {
            private Bitmap alpha;
            private Bitmap bitmap;
            private FSHBmpType bmpType;
            private string[] comments;
            private byte[] dirName = new byte[4];
            private bool isComp;
            private Color[] palette;

            public BitmapItem() {
                this.dirName = Encoding.ASCII.GetBytes("FiSH");
                this.bmpType = FSHBmpType.DXT1;
                this.isComp = false;
                this.comments = new string[0];
            }

            public void SetDirName(string name) {
                this.dirName = new byte[4];
                Encoding.ASCII.GetBytes(name, 0, 4, this.dirName, 0);
            }

            public Bitmap Alpha {
                get => this.alpha;
                set => this.alpha = value;
            }

            public Bitmap Bitmap {
                get => this.bitmap;
                set => this.bitmap = value;
            }

            public FSHBmpType BmpType {
                get => this.bmpType;
                set => this.bmpType = value;
            }

            public string[] Comments {
                get => this.comments;
                set => this.comments = value;
            }

            public byte[] DirName => this.dirName;

            public bool IsCompressed {
                get => this.isComp;
                set => this.isComp = value;
            }

            public Color[] Palette {
                get => this.palette;
                set => this.palette = value;
            }
        }

        private struct FSHEntryHeader {
            public int code;
            public short width;
            public short height;
            public short[] misc;
        }
    }

    
}
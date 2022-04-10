using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using csDBPF.Properties;
using System.Xml.Linq;

namespace csDBPF {
	[TestClass]
	public class DBPFUnitTests {

		// 01x Test Methods for DBPFUtil class
		[TestClass]
		public class _01x_DBPFUtil {
			[TestMethod]
			public void Test_011_DBPFUtil_ReverseBytes() {
				//Example: 1697917002 (0x 65 34 28 4A) returns 1244148837 (0x 4A 28 34 65)
				Assert.AreEqual((uint) 1244148837, DBPFUtil.ReverseBytes(1697917002));
				Assert.AreEqual((uint) 0x4a283465, DBPFUtil.ReverseBytes(0x6534284a));
				Assert.AreEqual((uint) 0, DBPFUtil.ReverseBytes(0));

				Assert.AreEqual(0x7FFFFFFFFFFF0000, DBPFUtil.ReverseBytes(0x0000FFFFFFFFFF7F));
			}

			[TestMethod]
			public void Test_012_DBPFUtil_UintToHexString() {
				Assert.AreEqual("6534284A", DBPFUtil.UIntToHexString(1697917002, 8));
				Assert.AreEqual("4A283465", DBPFUtil.UIntToHexString(1244148837, 8));
				Assert.AreEqual("6534284A", DBPFUtil.UIntToHexString(0x6534284A, 8));
				Assert.AreEqual("4A283465", DBPFUtil.UIntToHexString(0x4A283465, 8));
				Assert.AreEqual("4D2", DBPFUtil.UIntToHexString(1234, 3));
				Assert.AreEqual("000004D2", DBPFUtil.UIntToHexString(1234, 8));
			}

			[Ignore]
			[TestMethod]
			public void Test_013_DBPFUtil_StringFromByteArray() {
				byte[] dbpfB = new byte[] { 0x44, 0x42, 0x50, 0x46 };
				byte[] dbpfB1 = new byte[] { 68, 66, 80, 70 };
				Assert.AreEqual("DBPF", ByteArrayHelper.ToAString(dbpfB));
				Assert.AreEqual(ByteArrayHelper.ToAString(dbpfB), ByteArrayHelper.ToAString(dbpfB1));
				Assert.AreEqual("DBPF", ByteArrayHelper.ToAString(dbpfB, 0));
				Assert.AreEqual("DBPF", ByteArrayHelper.ToAString(dbpfB, 0, dbpfB.Length));
				Assert.AreEqual("BPF", ByteArrayHelper.ToAString(dbpfB, 1));
				Assert.AreEqual("DB", ByteArrayHelper.ToAString(dbpfB, 0, 2));
				Assert.AreEqual("P", ByteArrayHelper.ToAString(dbpfB, 2, 1));


				string lettersS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
				byte[] lettersB = new byte[] { 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6A, 0x6B, 0x6C, 0x6D, 0x6E, 0x6F, 0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7A, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5A };
				Assert.AreEqual(lettersS, ByteArrayHelper.ToAString(lettersB));

				string numbersS = "01213456789";
				byte[] numbersB = new byte[] { 0x30, 0x31, 0x32, 0x31, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 };
				Assert.AreEqual(numbersS, ByteArrayHelper.ToAString(numbersB));

				string specialS = "~`!@#$%^&*()_+-=[]\\{}|;':\",./<>?";
				byte[] specialB = new byte[] { 0x7E, 0x60, 0x21, 0x40, 0x23, 0x24, 0x25, 0x5E, 0x26, 0x2A, 0x28, 0x29, 0x5F, 0x2B, 0x2D, 0x3D, 0x5B, 0x5D, 0x5C, 0x7B, 0x7D, 0x7C, 0x3B, 0x27, 0x3A, 0x22, 0x2C, 0x2E, 0x2F, 0x3C, 0x3E, 0x3F };
				Assert.AreEqual(specialS, ByteArrayHelper.ToAString(specialB));

				Assert.ThrowsException<NullReferenceException>(() => ByteArrayHelper.ToAString(null));
			}

			[TestMethod]
			public void Test_014_DBPFUtil_StringToByteArray() {
				string s1 = "Test";
				byte[] b1 = { 0x54, 0x65, 0x73, 0x74 };
				string s2 = "Parks Aura";
				byte[] b2 = { 0x50, 0x61, 0x72, 0x6b, 0x73, 0x20, 0x41, 0x75, 0x72, 0x61 };
				CollectionAssert.AreEquivalent(b1, ByteArrayHelper.ToByteArray(s1));
				CollectionAssert.AreEquivalent(b2, ByteArrayHelper.ToByteArray(s2));
			}
		}

		// 02x Test methods for DBPFCompression class
		[TestClass]
		public class _02x_DBPFCompression {
			//Sample data from z_DataView - Parks Aura.dat
			public static byte[] notcompresseddata = new byte[] { 0x14, 0x00, 0x00, 0x10, 0x50, 0x00, 0x61, 0x00, 0x72, 0x00, 0x6B, 0x00, 0x73, 0x00, 0x20, 0x00, 0x41, 0x00, 0x75, 0x00, 0x72, 0x00, 0x61, 0x00, 0x20, 0x00, 0x28, 0x00, 0x62, 0x00, 0x79, 0x00, 0x20, 0x00, 0x43, 0x00, 0x6F, 0x00, 0x72, 0x00, 0x69, 0x00, 0x29, 0x00, 0x00 };
			public static byte[] compresseddata = new byte[] { 0x42, 0x01, 0x00, 0x00, 0x10, 0xFB, 0x00, 0x01, 0xBE, 0xE5, 0x45, 0x51, 0x5A, 0x42, 0x31, 0x23, 0x23, 0x23, 0x61, 0x28, 0x34, 0x05, 0x3F, 0x69, 0x0F, 0x69, 0x00, 0x67, 0x0B, 0x4A, 0x0F, 0x00, 0x00, 0x00, 0x01, 0x03, 0x10, 0x02, 0x03, 0x00, 0x03, 0x01, 0x03, 0x23, 0x05, 0x0C, 0x20, 0xE0, 0x0C, 0x80, 0x00, 0x00, 0x01, 0x07, 0x14, 0xE5, 0x44, 0x61, 0x74, 0x61, 0x56, 0x69, 0x65, 0x77, 0x3A, 0x20, 0x50, 0x61, 0x72, 0x6B, 0x73, 0x20, 0x41, 0x75, 0x72, 0x61, 0xE0, 0x47, 0x0B, 0x4A, 0x02, 0x20, 0x00, 0x03, 0x05, 0x29, 0x08, 0x9B, 0x00, 0x00, 0x05, 0x2C, 0xE1, 0x01, 0x08, 0x0B, 0x16, 0x09, 0x01, 0xE2, 0x0A, 0x40, 0x00, 0xE3, 0x10, 0x20, 0x15, 0x4D, 0xE4, 0x19, 0x33, 0x1C, 0x03, 0x05, 0x99, 0x70, 0x01, 0xE0, 0x3C, 0x53, 0xBC, 0x70, 0x11, 0x07, 0x0C, 0x01, 0x07, 0x0D, 0xE0, 0x79, 0x8C, 0xD9, 0x70, 0x11, 0x07, 0x46, 0x01, 0x07, 0x7F, 0xE0, 0xBA, 0xC5, 0xF0, 0x70, 0x00, 0x36, 0xE0, 0x00, 0xFF, 0xFF, 0xFF, 0x02, 0x07, 0x70, 0x81, 0xE0, 0xDD, 0xF1, 0xE2, 0x70, 0x01, 0x07, 0xB8, 0xE0, 0xBB, 0xE3, 0xC5, 0x70, 0x01, 0x07, 0xB9, 0xE0, 0x9A, 0xD4, 0xA8, 0x70, 0x05, 0x2F, 0xF2, 0xE0, 0xC6, 0x8A, 0x70, 0xF3, 0x00, 0x07, 0xE0, 0x58, 0xB7, 0x6A, 0x70, 0x01, 0x07, 0xFE, 0xE0, 0x36, 0xA8, 0x46, 0x70, 0x09, 0x66, 0xFF, 0x17, 0x89, 0x00, 0x70, 0xE5, 0x04, 0x68, 0x19, 0xAA, 0xE7, 0x15, 0x16, 0xE9, 0x01, 0x03, 0x06, 0x15, 0x0C, 0xEC, 0x01, 0x03, 0x64, 0x19, 0xBA, 0xEF, 0x00, 0x76, 0x15, 0xBA, 0xF2, 0x0D, 0xB6, 0x09, 0xE2, 0x99, 0x3B, 0x55, 0xBA, 0x99, 0x64, 0x83, 0xC8, 0x99, 0x7C, 0xA3, 0xC6, 0x01, 0x1F, 0x99, 0xE2, 0x99, 0x81, 0xB6, 0xB4, 0x99, 0x72, 0xBA, 0x94, 0x99, 0x4E, 0xB1, 0x65, 0x01, 0x6E, 0x99, 0x88, 0x80, 0x30, 0x99, 0xF3, 0xE8, 0x33, 0x72, 0x3E, 0x1C, 0x98, 0x3C, 0xAB, 0xB5, 0x9F, 0xDE, 0xD7, 0xC2, 0x88, 0xB1, 0xD1, 0x84, 0xFA, 0x44, 0x28, 0x16, 0xF1, 0x9F, 0x73, 0xB1, 0x5D, 0x99, 0x4E, 0x4F, 0x43, 0xD1, 0x97, 0x14, 0xA1, 0x35, 0x54, 0xF6, 0x15, 0x6E, 0xF4, 0xE0, 0xB4, 0x75, 0xCA, 0x39, 0xFC };
			public static byte[] decompresseddata = new byte[] { 0x45, 0x51, 0x5A, 0x42, 0x31, 0x23, 0x23, 0x23, 0x61, 0x28, 0x34, 0x05, 0x3F, 0x69, 0x0F, 0x69, 0x00, 0x67, 0x0B, 0x4A, 0x0F, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x23, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x0C, 0x80, 0x00, 0x00, 0x14, 0x00, 0x00, 0x00, 0x44, 0x61, 0x74, 0x61, 0x56, 0x69, 0x65, 0x77, 0x3A, 0x20, 0x50, 0x61, 0x72, 0x6B, 0x73, 0x20, 0x41, 0x75, 0x72, 0x61, 0xE0, 0x47, 0x0B, 0x4A, 0x00, 0x03, 0x80, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE1, 0x47, 0x0B, 0x4A, 0x00, 0x0B, 0x00, 0x00, 0x00, 0x01, 0xE2, 0x47, 0x0B, 0x4A, 0x00, 0x0B, 0x00, 0x00, 0x00, 0x00, 0xE3, 0x47, 0x0B, 0x4A, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE4, 0x47, 0x0B, 0x4A, 0x00, 0x03, 0x80, 0x00, 0x00, 0x1C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x99, 0x70, 0x01, 0x00, 0x00, 0x00, 0x3C, 0x53, 0xBC, 0x70, 0x0C, 0x00, 0x00, 0x00, 0x3C, 0x53, 0xBC, 0x70, 0x0D, 0x00, 0x00, 0x00, 0x79, 0x8C, 0xD9, 0x70, 0x46, 0x00, 0x00, 0x00, 0x79, 0x8C, 0xD9, 0x70, 0x7F, 0x00, 0x00, 0x00, 0xBA, 0xC5, 0xF0, 0x70, 0x80, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0x70, 0x81, 0x00, 0x00, 0x00, 0xDD, 0xF1, 0xE2, 0x70, 0xB8, 0x00, 0x00, 0x00, 0xBB, 0xE3, 0xC5, 0x70, 0xB9, 0x00, 0x00, 0x00, 0x9A, 0xD4, 0xA8, 0x70, 0xF2, 0x00, 0x00, 0x00, 0x79, 0xC6, 0x8A, 0x70, 0xF3, 0x00, 0x00, 0x00, 0x58, 0xB7, 0x6A, 0x70, 0xFE, 0x00, 0x00, 0x00, 0x36, 0xA8, 0x46, 0x70, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x99, 0x00, 0x70, 0xE5, 0x47, 0x0B, 0x4A, 0x00, 0x03, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0xE7, 0x47, 0x0B, 0x4A, 0x00, 0x0B, 0x00, 0x00, 0x00, 0x01, 0xE9, 0x47, 0x0B, 0x4A, 0x00, 0x03, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0xEC, 0x47, 0x0B, 0x4A, 0x00, 0x03, 0x00, 0x00, 0x00, 0x64, 0x00, 0x00, 0x00, 0xEF, 0x47, 0x0B, 0x4A, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xF2, 0x47, 0x0B, 0x4A, 0x00, 0x03, 0x80, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0x99, 0x99, 0x3B, 0x55, 0xBA, 0x99, 0x64, 0x83, 0xC8, 0x99, 0x7C, 0xA3, 0xC6, 0x99, 0xFF, 0xFF, 0xFF, 0x99, 0x81, 0xB6, 0xB4, 0x99, 0x72, 0xBA, 0x94, 0x99, 0x4E, 0xB1, 0x65, 0x99, 0x00, 0x99, 0x00, 0x99, 0xF3, 0x47, 0x0B, 0x4A, 0x00, 0x03, 0x80, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00, 0x33, 0x72, 0x3E, 0x1C, 0x98, 0x3C, 0xAB, 0xB5, 0x9F, 0xDE, 0xD7, 0xC2, 0x88, 0xB1, 0xD1, 0x84, 0xFA, 0x44, 0x28, 0x16, 0xF1, 0x9F, 0x73, 0xB1, 0x5D, 0x99, 0x4E, 0x4F, 0x43, 0xD1, 0x97, 0x14, 0xA1, 0x35, 0x54, 0xF6, 0xF4, 0x47, 0x0B, 0x4A, 0x00, 0x03, 0x00, 0x00, 0x00, 0xB4, 0x75, 0xCA, 0x39 };

			[TestMethod]
			public void Test_020_DBPFCompression_IsCompressed() {
				Assert.IsTrue(DBPFCompression.IsCompressed(compresseddata));
				Assert.IsFalse(DBPFCompression.IsCompressed(notcompresseddata));
				Assert.IsFalse(DBPFCompression.IsCompressed(decompresseddata));
			}

			[TestMethod]
			public void Test_021_DBPFCompression_GetDecompressedSize() {
				Assert.AreEqual((uint) 45, DBPFCompression.GetDecompressedSize(notcompresseddata));
				Assert.AreEqual((uint) 446, DBPFCompression.GetDecompressedSize(compresseddata));
				Assert.AreEqual((uint) 446, DBPFCompression.GetDecompressedSize(decompresseddata)); //BUG - figure out why this returns 318 when read from the index below
			}

			[TestMethod]
			public void Test_025_DBPFCompression_Decompress() {
				CollectionAssert.AreEquivalent(notcompresseddata, DBPFCompression.Decompress(notcompresseddata));
				CollectionAssert.AreEquivalent(decompresseddata, DBPFCompression.Decompress(compresseddata));
			}
		}

		// 03x Test Methods for ByteArrayHelper class
		[TestClass]
		public class _03x_ByteArrayHelper {
			//Remember here that we are not "reading" the bytes straight from the file in order; rather we are "getting" numbers from them so we are worried about endianness.
			readonly byte[] bytes = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x99, 0x70, 0x01, 0x00, 0x00, 0x00, 0x3C, 0x53, 0xBC, 0x70, 0x0C, 0x00, 0x00, 0x00, 0x3C, 0x53, 0xBC, 0x70 };
			readonly byte[] bytesAsBools = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x01, 0x01, 0x01, 0x01 };
			readonly bool[] bools = { false, false, false, false, false, false, true, true, true, false, false, false, true, true, true, true, true, false, false, false, true, true, true, true };
			readonly ushort[] uint16 = { 0x0000, 0x0000, 0000, 0x7099, 0x0001, 0x0000, 0x533C, 0x70BC, 0x000C, 0x0000, 0x533C, 0x70BC };
			readonly int[] sint32 = { 0x00000000, 0x70990000, 0x00000001, 0x70BC533C, 0x0000000C, 0x70BC533C };
			readonly float[] float32 = { 0f, 3.78809652e+29f, 1.401298e-45f, 4.66270448e+29f, 1.681558e-44f, 4.66270448e+29f };
			readonly uint[] uint32 = { 0x00000000, 0x70990000, 0x00000001, 0x70BC533C, 0x0000000C, 0x70BC533C };
			readonly long[] sint64 = { 0x7099000000000000, 0x70BC533C00000001, 0x70BC533C0000000C };
			//TODO - should add unit tests to test edge cases - min and max values

			[TestMethod]
			public void Test_030_ByteArrayHelper_ToTypeArray() {
				CollectionAssert.AreEqual(bools, ByteArrayHelper.ToBoolArray(bytesAsBools));
				CollectionAssert.AreEqual(bytes, ByteArrayHelper.ToUint8Array(bytes));
				CollectionAssert.AreEqual(uint16, ByteArrayHelper.ToUInt16Array(bytes));
				CollectionAssert.AreEqual(sint32, ByteArrayHelper.ToSInt32Array(bytes));
				CollectionAssert.AreEqual(float32, ByteArrayHelper.ToFloat32Array(bytes));
				CollectionAssert.AreEqual(uint32, ByteArrayHelper.ToUInt32Array(bytes));
				CollectionAssert.AreEqual(sint64, ByteArrayHelper.ToSInt64Array(bytes));
			}

			[TestMethod]
			public void Test_031_ByteArrayHelper_ToByteArray() {
				CollectionAssert.AreEqual(bytesAsBools, ByteArrayHelper.ToByteArray(bools));
				CollectionAssert.AreEqual(bytes, ByteArrayHelper.ToByteArray(uint16));
				CollectionAssert.AreEqual(bytes, ByteArrayHelper.ToByteArray(sint32));
				CollectionAssert.AreEqual(bytes, ByteArrayHelper.ToByteArray(float32));
				CollectionAssert.AreEqual(bytes, ByteArrayHelper.ToByteArray(uint32));
				CollectionAssert.AreEqual(bytes, ByteArrayHelper.ToByteArray(sint64));
			}
		}

		// 05x Test Methods for DBPFTGI Class
		[TestClass]
		public class _05x_DBPFTGI {
			[TestMethod]
			public void Test_050_DBPFTGI_CreateNew() {

			}

			[TestMethod]
			public void Test_052_DBPFTGI_Equals() {
				DBPFTGI tgi1 = new DBPFTGI(0, 0, 0);
				DBPFTGI tgi2 = new DBPFTGI(0, 0, 0);
				DBPFTGI tgi3 = new DBPFTGI(0xe86b1eef, 0xe86b1eef, 0x286b1f03);
				DBPFTGI tgi4 = new DBPFTGI(3899334383, 3899334383, 678108931);
				Assert.AreEqual(tgi1, tgi2);
				Assert.IsTrue(tgi1.Equals(tgi2));
				Assert.IsTrue(tgi2.Equals(tgi1));
				Assert.AreEqual(tgi3, tgi4);
				Assert.IsTrue(tgi3.Equals(tgi4));
				Assert.IsTrue(tgi4.Equals(tgi3));

				Assert.AreNotEqual(tgi2, tgi3);
				Assert.IsFalse(tgi1.Equals(tgi3));
				Assert.IsFalse(tgi3.Equals(tgi1));

				Assert.IsFalse(tgi1.Equals("string"));
				DBPFEntry e1 = new DBPFEntry(tgi1, 0, 0, 0);
				Assert.IsFalse(tgi1.Equals(e1));
			}

			[TestMethod]
			public void Test_054_DBPFTGI_MatchesKnownTGI() {
				DBPFTGI tgi_blank = new DBPFTGI(0, 0, 0);
				DBPFTGI tgi_exemplar = new DBPFTGI(0x6534284a, 0, 0);
				DBPFTGI tgi_exemplarRail = new DBPFTGI(0x6534284a, 0xe8347989, 0);
				DBPFTGI tgi_exemplarRail2 = new DBPFTGI(0x6534284a, 0xe8347989, 0x1ab4e56a);

				Assert.IsTrue(tgi_blank.MatchesKnownTGI(DBPFTGI.BLANKTGI));
				Assert.IsTrue(tgi_blank.MatchesKnownTGI(DBPFTGI.NULLTGI));

				Assert.IsTrue(tgi_exemplar.MatchesKnownTGI(DBPFTGI.EXEMPLAR));
				Assert.IsTrue(tgi_exemplarRail.MatchesKnownTGI(DBPFTGI.EXEMPLAR_RAIL));
				Assert.IsTrue(tgi_exemplarRail2.MatchesKnownTGI(DBPFTGI.EXEMPLAR_RAIL));
				Assert.IsTrue(tgi_exemplarRail2.MatchesKnownTGI(DBPFTGI.EXEMPLAR));
				Assert.IsFalse(tgi_exemplar.MatchesKnownTGI(DBPFTGI.COHORT));
				Assert.IsTrue(tgi_exemplar.MatchesKnownTGI(DBPFTGI.NULLTGI));
				Assert.IsFalse(tgi_exemplar.MatchesKnownTGI(DBPFTGI.PNG));
			}

			[TestMethod]
			public void Test_055_DBPFTGI_MatchesAnyKnownTGI() {
				DBPFTGI tgi_blank = new DBPFTGI(0, 0, 0);
				DBPFTGI tgi_exemplar = new DBPFTGI(0x6534284a, 0, 0);
				DBPFTGI tgi_exemplarRail = new DBPFTGI(0x6534284a, 0xe8347989, 0);
				DBPFTGI tgi_exemplarRail2 = new DBPFTGI(0x6534284a, 0xe8347989, 0x1ab4e56a);
				DBPFTGI tgi_PNG_Icon = new DBPFTGI(0x856ddbac, 0x6a386d26, 0x1ab4e56f);
				DBPFTGI tgi_PNG = new DBPFTGI(0x856ddbac, 0x6a386d27, 0x1ab4e56f);
				Assert.AreEqual("BLANKTGI", tgi_blank.MatchesAnyKnownTGI());
				Assert.AreEqual("EXEMPLAR", tgi_exemplar.MatchesAnyKnownTGI());
				Assert.AreEqual("EXEMPLAR_RAIL", tgi_exemplarRail.MatchesAnyKnownTGI());
				Assert.AreEqual("EXEMPLAR_RAIL", tgi_exemplarRail2.MatchesAnyKnownTGI());
				Assert.AreEqual("PNG_ICON", tgi_PNG_Icon.MatchesAnyKnownTGI());
				Assert.AreEqual("PNG", tgi_PNG.MatchesAnyKnownTGI());
			}

			[TestMethod]
			public void Test_056a_DBPFTGI_ModifyTGIusingDBPFTGI() {
				DBPFTGI exemplar = new DBPFTGI(0x6534284a, 0, 0);
				DBPFTGI exemplar2 = exemplar.ModifyTGI(DBPFTGI.EXEMPLAR_AVENUE);
				Assert.AreEqual("T:0x6534284A, G:0xCB730FAC, I:0x00000000", exemplar2.ToString());
				Assert.AreEqual(exemplar.ToString(), exemplar.ModifyTGI(DBPFTGI.NULLTGI).ToString());
				DBPFTGI exemplar4 = new DBPFTGI(0, 2, 3);
				Assert.AreEqual("T:0xCA63E2A3, G:0x4A5E8EF6, I:0x00000003", exemplar4.ModifyTGI(DBPFTGI.LUA).ToString());
			}

			[TestMethod]
			public void Test_056b_DBPFTGI_ModifyTGIusingUint() {
				DBPFTGI exemplar = new DBPFTGI(0x6534284a, 0, 1000001);
				Assert.AreEqual("T:0x6534284A, G:0x00000000, I:0x00000064", exemplar.ModifyTGI(null, null, 100).ToString());
				Assert.AreEqual("T:0x00000064, G:0x00000064, I:0x00000064", exemplar.ModifyTGI(100, 100, 100).ToString());
			}
		}


		// 06x Test Methods for DBPFProperty Class
		[TestClass]
		public class _06x_DBPFProperty {
			[TestMethod]
			public void Test_060_DBPFPropertyDataType_ReturnType() {
				Assert.AreEqual("SINT32", DBPFPropertyDataType.SINT32.name);
				Assert.AreEqual(DBPFPropertyDataType.BOOL, DBPFPropertyDataType.LookupDataType(0xB00));
				Assert.AreEqual(DBPFPropertyDataType.UINT32.name, DBPFPropertyDataType.LookupDataType(0x300).name);
				Assert.AreEqual(4, DBPFPropertyDataType.LookupDataType(0x300).length);
			}

			[Ignore]
			[TestMethod]
			public void Test_061_DBPFProperty() {
				throw new NotImplementedException();
			}

			[TestMethod]
			public void Test_062_DBPFPropertyString() {
				byte[] byteparks = { 0x50, 0x61, 0x72, 0x6B, 0x73 };
				string stringparks = "Parks";
				byte[] byteparksaura = { 0x50, 0x61, 0x72, 0x6b, 0x73, 0x20, 0x41, 0x75, 0x72, 0x61 };
				string stringparksaura = "Parks Aura";
				byte[] bytedataviewparksaura = { 0x44, 0x61, 0x74, 0x61, 0x56, 0x69, 0x65, 0x77, 0x3A, 0x20, 0x50, 0x61, 0x72, 0x6B, 0x73, 0x20, 0x41, 0x75, 0x72, 0x61 };
				string stringdataviewparksaura = "DataView: Parks Aura";

				//Test a property read from file
				DBPFProperty prop_file = DBPFProperty.DecodeExemplarProperty(_02x_DBPFCompression.decompresseddata, 37);
				Assert.AreEqual((uint) 0x20, prop_file.ID);
				Assert.AreEqual((uint) 20, prop_file.NumberOfReps);
				Assert.AreEqual(DBPFPropertyDataType.STRING, prop_file.DataType);
				CollectionAssert.AreEquivalent(bytedataviewparksaura, prop_file.ByteValues);
				Assert.AreEqual(stringdataviewparksaura, prop_file.DecodeValues());

				//Compare to known property
				DBPFProperty prop_created = new DBPFPropertyString(DBPFPropertyDataType.STRING);
				prop_created.ID = 0x20;
				prop_created.SetValues(stringdataviewparksaura);
				Assert.AreEqual(prop_created.ID, prop_file.ID);
				Assert.AreEqual(prop_created.NumberOfReps, prop_file.NumberOfReps);
				Assert.AreEqual(prop_created.DataType, prop_file.DataType);
				CollectionAssert.AreEquivalent(prop_created.ByteValues, prop_file.ByteValues);
				Assert.AreEqual(prop_created.DecodeValues(), prop_file.DecodeValues());

				//Check for no differences between values and valuesDecoded when each is changed
				prop_created.SetValues(stringparks);
				CollectionAssert.AreEquivalent(byteparks, prop_created.ByteValues);
				Assert.AreEqual((uint) byteparks.Length, prop_created.NumberOfReps);
				prop_created.ByteValues = byteparksaura;
				Assert.AreEqual(stringparksaura, prop_created.DecodeValues());
				Assert.AreEqual((uint) stringparksaura.Length, prop_created.NumberOfReps);
			}

			[TestMethod]
			public void Test_063_DBPFPropertyInteger() {
				//Single UInt32 value
				byte[] val = { 0x23, 0x00, 0x00, 0x00 };
				uint[] decoded = { 0x00000023 };
				DBPFProperty prop_file = DBPFProperty.DecodeExemplarProperty(_02x_DBPFCompression.decompresseddata);
				Assert.AreEqual((uint) 0x10, prop_file.ID);
				Assert.AreEqual((uint) 1, prop_file.NumberOfReps);
				Assert.AreEqual(DBPFPropertyDataType.UINT32, prop_file.DataType);
				CollectionAssert.AreEqual(val, prop_file.ByteValues);
				CollectionAssert.AreEqual(decoded, (System.Collections.ICollection) prop_file.DecodeValues());

				//8 repetitions of 0
				byte[] val2 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
				uint[] decoded2 = { 0, 0, 0, 0, 0, 0, 0, 0 };
				prop_file = DBPFProperty.DecodeExemplarProperty(_02x_DBPFCompression.decompresseddata, 70);
				Assert.AreEqual((uint) 0x4A0B47E0, prop_file.ID);
				Assert.AreEqual((uint) 8, prop_file.NumberOfReps);
				Assert.AreEqual(DBPFPropertyDataType.UINT32, prop_file.DataType);
				CollectionAssert.AreEqual(val2, prop_file.ByteValues);
				CollectionAssert.AreEqual(decoded2, (System.Collections.ICollection) prop_file.DecodeValues());

				//True boolean value
				byte[] val3 = { 1 };
				bool[] decoded3 = { true };
				prop_file = DBPFProperty.DecodeExemplarProperty(_02x_DBPFCompression.decompresseddata, 115);
				Assert.AreEqual((uint) 0x4A0B47E1, prop_file.ID);
				Assert.AreEqual((uint) 1, prop_file.NumberOfReps);
				Assert.AreEqual(DBPFPropertyDataType.BOOL, prop_file.DataType);
				CollectionAssert.AreEqual(val3, prop_file.ByteValues);
				CollectionAssert.AreEqual(decoded3, (System.Collections.ICollection) prop_file.DecodeValues());

				//False boolean value
				byte[] val4 = { 0 };
				bool[] decoded4 = { false };
				prop_file = DBPFProperty.DecodeExemplarProperty(_02x_DBPFCompression.decompresseddata, 125);
				Assert.AreEqual((uint) 0x4A0B47E2, prop_file.ID);
				Assert.AreEqual((uint) 1, prop_file.NumberOfReps);
				Assert.AreEqual(DBPFPropertyDataType.BOOL, prop_file.DataType);
				CollectionAssert.AreEqual(val4, prop_file.ByteValues);
				CollectionAssert.AreEqual(decoded4, (System.Collections.ICollection) prop_file.DecodeValues());

				//Single UInt32 value of 0
				byte[] val5 = { 0,0,0,0 };
				uint[] decoded5 = { 0 };
				prop_file = DBPFProperty.DecodeExemplarProperty(_02x_DBPFCompression.decompresseddata, 135);
				Assert.AreEqual((uint) 0x4A0B47E3, prop_file.ID);
				Assert.AreEqual((uint) 1, prop_file.NumberOfReps);
				Assert.AreEqual(DBPFPropertyDataType.UINT32, prop_file.DataType);
				CollectionAssert.AreEqual(val5, prop_file.ByteValues);
				CollectionAssert.AreEqual(decoded5, (System.Collections.ICollection) prop_file.DecodeValues());

				//28 UInt32s
				byte[] val6 = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x99, 0x70, 0x01, 0x00, 0x00, 0x00, 0x3C, 0x53, 0xBC, 0x70, 0x0C, 0x00, 0x00, 0x00, 0x3C, 0x53, 0xBC, 0x70, 0x0D, 0x00, 0x00, 0x00, 0x79, 0x8C, 0xD9, 0x70, 0x46, 0x00, 0x00, 0x00, 0x79, 0x8C, 0xD9, 0x70, 0x7F, 0x00, 0x00, 0x00, 0xBA, 0xC5, 0xF0, 0x70, 0x80, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0x70, 0x81, 0x00, 0x00, 0x00, 0xDD, 0xF1, 0xE2, 0x70, 0xB8, 0x00, 0x00, 0x00, 0xBB, 0xE3, 0xC5, 0x70, 0xB9, 0x00, 0x00, 0x00, 0x9A, 0xD4, 0xA8, 0x70, 0xF2, 0x00, 0x00, 0x00, 0x79, 0xC6, 0x8A, 0x70, 0xF3, 0x00, 0x00, 0x00, 0x58, 0xB7, 0x6A, 0x70, 0xFE, 0x00, 0x00, 0x00, 0x36, 0xA8, 0x46, 0x70, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x99, 0x00, 0x70 };
				uint[] decoded6 = { 0x00000000, 0x70990000, 0x00000001, 0x70BC533C, 0x0000000C, 0x70BC533C, 0x0000000D, 0x70D98C79, 0x00000046, 0x70D98C79, 0x0000007F, 0x70F0C5BA, 0x00000080, 0x70FFFFFF, 0x00000081, 0x70E2F1DD, 0x000000B8, 0x70C5E3BB, 0x000000B9, 0x70A8D49A, 0x000000F2, 0x708AC679, 0x000000F3, 0x706AB758, 0x000000FE, 0x7046A836, 0x000000FF, 0x70009900 };
				prop_file = DBPFProperty.DecodeExemplarProperty(_02x_DBPFCompression.decompresseddata, 148);
				Assert.AreEqual((uint) 0x4A0B47E4, prop_file.ID);
				Assert.AreEqual((uint) 28, prop_file.NumberOfReps);
				Assert.AreEqual(DBPFPropertyDataType.UINT32, prop_file.DataType);
				CollectionAssert.AreEqual(val6, prop_file.ByteValues);
				CollectionAssert.AreEqual(decoded6, (System.Collections.ICollection) prop_file.DecodeValues());

				//Set values
				prop_file.SetValues(val5);
				Assert.AreEqual((uint) 1, prop_file.NumberOfReps);
				CollectionAssert.AreEquivalent(val5, prop_file.ByteValues);
				CollectionAssert.AreEqual(decoded5, (System.Collections.ICollection) prop_file.DecodeValues());
				prop_file.SetValues(val6);
				Assert.AreEqual((uint) 28, prop_file.NumberOfReps);
				CollectionAssert.AreEquivalent(val6, prop_file.ByteValues);
				CollectionAssert.AreEqual(decoded6, (System.Collections.ICollection) prop_file.DecodeValues());
			}


			[TestMethod]
			public void Test_065_DBPFProperty_GetXMLProperty() {
				XElement el = DBPFProperty.GetXMLProperty(0x00000010);
				Assert.AreEqual("0x00000010", el.Attribute("ID").Value); 
				Assert.AreEqual("Exemplar Type", el.Attribute("Name").Value);

				el = DBPFProperty.GetXMLProperty(0x87cd6345);
				Assert.AreEqual("0x87cd6345", el.Attribute("ID").Value);
				Assert.AreEqual("R$$$ Proximity Effect", el.Attribute("Name").Value);
			}
		}


		// 1xx Test Methods for DBPFFile Class
		[TestClass]
		public class _1xx_DBPFFile {
			[Ignore]
			[TestMethod]
			public void Test_101_DBPFFile_ValidDBPF() {
				//DBPFFile dbpf = new DBPFFile("C:\\Users\\Administrator\\Documents\\SimCity 4\\Plugins\\mntoes\\Bournemouth Housing Pack\\Mntoes-Bournemouth Housing Pack.dat");
				DBPFFile dbpf = new DBPFFile("C:\\Users\\Administrator\\Documents\\SimCity 4\\Plugins\\z_DataView - Parks Aura.dat");
				Assert.AreEqual((uint) 0x44425046, dbpf.header.identifier); //1145196614 dec = 44425046 hex = DBPF ascii
				Assert.AreEqual(DBPFUtil.ReverseBytes(1), dbpf.header.majorVersion); //16777216 dec = 1000000 hex
				Assert.AreEqual((uint) 0, dbpf.header.minorVersion);
				Assert.AreEqual(DBPFUtil.ReverseBytes(7), dbpf.header.indexMajorVersion); //117440512 dec = 7000000 hex)
			}

			[TestMethod]
			public void Test_102_DBPFFile_NotValidDBPF() {
				//These should fail : not valid DBPF file
				Exception ex = Assert.ThrowsException<Exception>(() => new DBPFFile("C:\\Users\\Administrator\\Documents\\SimCity 4\\Plugins\\CAS_AutoHistorical_v0.0.2.dll"));
				Assert.IsTrue(ex.Message.Contains("File is not a DBPF file!"));

				//example: Assert.ThrowsException<System.ArgumentOutOfRangeException>(() => account.Debit(debitAmount));
			}

			[TestMethod]
			[Ignore]
			public void Test_110_ParseHeader() {

			}

			[TestMethod]
			[Ignore]
			public void Test_210_ParseIndex() {

			}
		}

		public void ParseExemplarSubfile(byte[] data) {
			//TODO - verify first 5 bytes are correct --------------------- figure out what to do with this
			ushort cohortTypeID = (ushort) ((data[8] << 2) & data[9]);
			ushort cohortGroupID = (ushort) ((data[10] << 2) & data[10]);
			ushort cohortInstanceID = (ushort) ((data[12] << 2) & data[13]);
			ushort propertyCount = (ushort) ((data[14] << 2) & data[15]);
		}
	}
}

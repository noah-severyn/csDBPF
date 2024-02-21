using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using csDBPF;
using csDBPF.Properties;
using System.Collections.Generic;
using csDBPF.Entries;
using System.Diagnostics;
using SixLabors.ImageSharp;
using System.IO;

namespace csDBPF_Test {
    [TestClass]
    public class DBPFUnitTests {
        internal class TestArrays {
            //Sample data from z_DataView - Parks Aura.dat --- in BINARY encoding ---
            public static byte[] notcompressedentry_b = new byte[] { 0x14, 0x00, 0x00, 0x10, 0x50, 0x00, 0x61, 0x00, 0x72, 0x00, 0x6B, 0x00, 0x73, 0x00, 0x20, 0x00, 0x41, 0x00, 0x75, 0x00, 0x72, 0x00, 0x61, 0x00, 0x20, 0x00, 0x28, 0x00, 0x62, 0x00, 0x79, 0x00, 0x20, 0x00, 0x43, 0x00, 0x6F, 0x00, 0x72, 0x00, 0x69, 0x00, 0x29, 0x00 };
            public static byte[] compressedentry_b = new byte[] { 0x42, 0x01, 0x00, 0x00, 0x10, 0xFB, 0x00, 0x01, 0xBE, 0xE5, 0x45, 0x51, 0x5A, 0x42, 0x31, 0x23, 0x23, 0x23, 0x61, 0x28, 0x34, 0x05, 0x3F, 0x69, 0x0F, 0x69, 0x00, 0x67, 0x0B, 0x4A, 0x0F, 0x00, 0x00, 0x00, 0x01, 0x03, 0x10, 0x02, 0x03, 0x00, 0x03, 0x01, 0x03, 0x23, 0x05, 0x0C, 0x20, 0xE0, 0x0C, 0x80, 0x00, 0x00, 0x01, 0x07, 0x14, 0xE5, 0x44, 0x61, 0x74, 0x61, 0x56, 0x69, 0x65, 0x77, 0x3A, 0x20, 0x50, 0x61, 0x72, 0x6B, 0x73, 0x20, 0x41, 0x75, 0x72, 0x61, 0xE0, 0x47, 0x0B, 0x4A, 0x02, 0x20, 0x00, 0x03, 0x05, 0x29, 0x08, 0x9B, 0x00, 0x00, 0x05, 0x2C, 0xE1, 0x01, 0x08, 0x0B, 0x16, 0x09, 0x01, 0xE2, 0x0A, 0x40, 0x00, 0xE3, 0x10, 0x20, 0x15, 0x4D, 0xE4, 0x19, 0x33, 0x1C, 0x03, 0x05, 0x99, 0x70, 0x01, 0xE0, 0x3C, 0x53, 0xBC, 0x70, 0x11, 0x07, 0x0C, 0x01, 0x07, 0x0D, 0xE0, 0x79, 0x8C, 0xD9, 0x70, 0x11, 0x07, 0x46, 0x01, 0x07, 0x7F, 0xE0, 0xBA, 0xC5, 0xF0, 0x70, 0x00, 0x36, 0xE0, 0x00, 0xFF, 0xFF, 0xFF, 0x02, 0x07, 0x70, 0x81, 0xE0, 0xDD, 0xF1, 0xE2, 0x70, 0x01, 0x07, 0xB8, 0xE0, 0xBB, 0xE3, 0xC5, 0x70, 0x01, 0x07, 0xB9, 0xE0, 0x9A, 0xD4, 0xA8, 0x70, 0x05, 0x2F, 0xF2, 0xE0, 0xC6, 0x8A, 0x70, 0xF3, 0x00, 0x07, 0xE0, 0x58, 0xB7, 0x6A, 0x70, 0x01, 0x07, 0xFE, 0xE0, 0x36, 0xA8, 0x46, 0x70, 0x09, 0x66, 0xFF, 0x17, 0x89, 0x00, 0x70, 0xE5, 0x04, 0x68, 0x19, 0xAA, 0xE7, 0x15, 0x16, 0xE9, 0x01, 0x03, 0x06, 0x15, 0x0C, 0xEC, 0x01, 0x03, 0x64, 0x19, 0xBA, 0xEF, 0x00, 0x76, 0x15, 0xBA, 0xF2, 0x0D, 0xB6, 0x09, 0xE2, 0x99, 0x3B, 0x55, 0xBA, 0x99, 0x64, 0x83, 0xC8, 0x99, 0x7C, 0xA3, 0xC6, 0x01, 0x1F, 0x99, 0xE2, 0x99, 0x81, 0xB6, 0xB4, 0x99, 0x72, 0xBA, 0x94, 0x99, 0x4E, 0xB1, 0x65, 0x01, 0x6E, 0x99, 0x88, 0x80, 0x30, 0x99, 0xF3, 0xE8, 0x33, 0x72, 0x3E, 0x1C, 0x98, 0x3C, 0xAB, 0xB5, 0x9F, 0xDE, 0xD7, 0xC2, 0x88, 0xB1, 0xD1, 0x84, 0xFA, 0x44, 0x28, 0x16, 0xF1, 0x9F, 0x73, 0xB1, 0x5D, 0x99, 0x4E, 0x4F, 0x43, 0xD1, 0x97, 0x14, 0xA1, 0x35, 0x54, 0xF6, 0x15, 0x6E, 0xF4, 0xE0, 0xB4, 0x75, 0xCA, 0x39, 0xFC };
            public static byte[] decompressedentry_b = new byte[] { 0x45, 0x51, 0x5A, 0x42, 0x31, 0x23, 0x23, 0x23, 0x61, 0x28, 0x34, 0x05, 0x3F, 0x69, 0x0F, 0x69, 0x00, 0x67, 0x0B, 0x4A, 0x0F, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x23, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x0C, 0x80, 0x00, 0x00, 0x14, 0x00, 0x00, 0x00, 0x44, 0x61, 0x74, 0x61, 0x56, 0x69, 0x65, 0x77, 0x3A, 0x20, 0x50, 0x61, 0x72, 0x6B, 0x73, 0x20, 0x41, 0x75, 0x72, 0x61, 0xE0, 0x47, 0x0B, 0x4A, 0x00, 0x03, 0x80, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE1, 0x47, 0x0B, 0x4A, 0x00, 0x0B, 0x00, 0x00, 0x00, 0x01, 0xE2, 0x47, 0x0B, 0x4A, 0x00, 0x0B, 0x00, 0x00, 0x00, 0x00, 0xE3, 0x47, 0x0B, 0x4A, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE4, 0x47, 0x0B, 0x4A, 0x00, 0x03, 0x80, 0x00, 0x00, 0x1C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x99, 0x70, 0x01, 0x00, 0x00, 0x00, 0x3C, 0x53, 0xBC, 0x70, 0x0C, 0x00, 0x00, 0x00, 0x3C, 0x53, 0xBC, 0x70, 0x0D, 0x00, 0x00, 0x00, 0x79, 0x8C, 0xD9, 0x70, 0x46, 0x00, 0x00, 0x00, 0x79, 0x8C, 0xD9, 0x70, 0x7F, 0x00, 0x00, 0x00, 0xBA, 0xC5, 0xF0, 0x70, 0x80, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0x70, 0x81, 0x00, 0x00, 0x00, 0xDD, 0xF1, 0xE2, 0x70, 0xB8, 0x00, 0x00, 0x00, 0xBB, 0xE3, 0xC5, 0x70, 0xB9, 0x00, 0x00, 0x00, 0x9A, 0xD4, 0xA8, 0x70, 0xF2, 0x00, 0x00, 0x00, 0x79, 0xC6, 0x8A, 0x70, 0xF3, 0x00, 0x00, 0x00, 0x58, 0xB7, 0x6A, 0x70, 0xFE, 0x00, 0x00, 0x00, 0x36, 0xA8, 0x46, 0x70, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x99, 0x00, 0x70, 0xE5, 0x47, 0x0B, 0x4A, 0x00, 0x03, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0xE7, 0x47, 0x0B, 0x4A, 0x00, 0x0B, 0x00, 0x00, 0x00, 0x01, 0xE9, 0x47, 0x0B, 0x4A, 0x00, 0x03, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0xEC, 0x47, 0x0B, 0x4A, 0x00, 0x03, 0x00, 0x00, 0x00, 0x64, 0x00, 0x00, 0x00, 0xEF, 0x47, 0x0B, 0x4A, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xF2, 0x47, 0x0B, 0x4A, 0x00, 0x03, 0x80, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0x99, 0x99, 0x3B, 0x55, 0xBA, 0x99, 0x64, 0x83, 0xC8, 0x99, 0x7C, 0xA3, 0xC6, 0x99, 0xFF, 0xFF, 0xFF, 0x99, 0x81, 0xB6, 0xB4, 0x99, 0x72, 0xBA, 0x94, 0x99, 0x4E, 0xB1, 0x65, 0x99, 0x00, 0x99, 0x00, 0x99, 0xF3, 0x47, 0x0B, 0x4A, 0x00, 0x03, 0x80, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00, 0x33, 0x72, 0x3E, 0x1C, 0x98, 0x3C, 0xAB, 0xB5, 0x9F, 0xDE, 0xD7, 0xC2, 0x88, 0xB1, 0xD1, 0x84, 0xFA, 0x44, 0x28, 0x16, 0xF1, 0x9F, 0x73, 0xB1, 0x5D, 0x99, 0x4E, 0x4F, 0x43, 0xD1, 0x97, 0x14, 0xA1, 0x35, 0x54, 0xF6, 0xF4, 0x47, 0x0B, 0x4A, 0x00, 0x03, 0x00, 0x00, 0x00, 0xB4, 0x75, 0xCA, 0x39 };

            //Sample data from b62-albertsons_60s v 1.1-0x6534284a-0xd3a3e650-0xd4ebfbfa.SC4Desc --- in TEXT encoding ---
            public static byte[] notcompressedentry_t = { 45, 0x51, 0x5A, 0x54, 0x31, 0x23, 0x23, 0x23, 0x0D, 0x0A, 0x50, 0x61, 0x72, 0x65, 0x6E, 0x74, 0x43, 0x6F, 0x68, 0x6F, 0x72, 0x74, 0x3D, 0x4B, 0x65, 0x79, 0x3A, 0x7B, 0x30, 0x78, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x2C, 0x30, 0x78, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x2C, 0x30, 0x78, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x7D, 0x0D, 0x0A, 0x50, 0x72, 0x6F, 0x70, 0x43, 0x6F, 0x75, 0x6E, 0x74, 0x3D, 0x30, 0x78, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x31, 0x38, 0x0D, 0x0A, 0x30, 0x78, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x31, 0x30, 0x3A, 0x7B, 0x22, 0x45, 0x78, 0x65, 0x6D, 0x70, 0x6C, 0x61, 0x72, 0x20, 0x54, 0x79, 0x70, 0x65, 0x22, 0x7D, 0x3D, 0x55, 0x69, 0x6E, 0x74, 0x33, 0x32, 0x3A, 0x30, 0x3A, 0x7B, 0x30, 0x78, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x32, 0x7D, 0x0D, 0x0A, 0x30, 0x78, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x32, 0x30, 0x3A, 0x7B, 0x22, 0x45, 0x78, 0x65, 0x6D, 0x70, 0x6C, 0x61, 0x72, 0x20, 0x4E, 0x61, 0x6D, 0x65, 0x22, 0x7D, 0x3D, 0x53, 0x74, 0x72, 0x69, 0x6E, 0x67, 0x3A, 0x31, 0x3A, 0x7B, 0x22, 0x42, 0x36, 0x32, 0x2D, 0x43, 0x53, 0x24, 0x5F, 0x41, 0x6C, 0x62, 0x65, 0x72, 0x74, 0x73, 0x6F, 0x6E, 0x73, 0x5F, 0x36, 0x30, 0x73, 0x5F, 0x47, 0x72, 0x6F, 0x63, 0x65, 0x72, 0x79, 0x20, 0x76, 0x20, 0x31, 0x2E, 0x31, 0x22, 0x7D, 0x0D, 0x0A, 0x30, 0x78, 0x30, 0x39, 0x39, 0x61, 0x66, 0x61, 0x63, 0x64, 0x3A, 0x7B, 0x22, 0x42, 0x75, 0x6C, 0x6C, 0x64, 0x6F, 0x7A, 0x65, 0x20, 0x43, 0x6F, 0x73, 0x74, 0x22, 0x7D, 0x3D, 0x53, 0x69, 0x6E, 0x74, 0x36, 0x34, 0x3A, 0x30, 0x3A, 0x7B, 0x30, 0x78, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x61, 0x39, 0x7D, 0x0D, 0x0A, 0x30, 0x78, 0x32, 0x37, 0x38, 0x31, 0x32, 0x38, 0x31, 0x30, 0x3A, 0x7B, 0x22, 0x4F, 0x63, 0x63, 0x75, 0x70, 0x61, 0x6E, 0x74, 0x20, 0x53, 0x69, 0x7A, 0x65, 0x22, 0x7D, 0x3D, 0x46, 0x6C, 0x6F, 0x61, 0x74, 0x33, 0x32, 0x3A, 0x33, 0x3A, 0x7B, 0x38, 0x31, 0x2E, 0x35, 0x38, 0x39, 0x37, 0x39, 0x37, 0x39, 0x37, 0x2C, 0x31, 0x33, 0x2E, 0x39, 0x34, 0x37, 0x32, 0x39, 0x39, 0x39, 0x36, 0x2C, 0x33, 0x39, 0x2E, 0x34, 0x34, 0x32, 0x35, 0x30, 0x31, 0x30, 0x37, 0x7D, 0x0D, 0x0A, 0x30, 0x78, 0x32, 0x37, 0x38, 0x31, 0x32, 0x38, 0x31, 0x31, 0x3A, 0x7B, 0x22, 0x55, 0x6E, 0x6B, 0x6E, 0x6F, 0x77, 0x6E, 0x22, 0x7D, 0x3D, 0x46, 0x6C, 0x6F, 0x61, 0x74, 0x33, 0x32, 0x3A, 0x31, 0x3A, 0x7B, 0x30, 0x2E, 0x35, 0x7D, 0x0D, 0x0A, 0x30, 0x78, 0x32, 0x37, 0x38, 0x31, 0x32, 0x38, 0x32, 0x31, 0x3A, 0x7B, 0x22, 0x52, 0x65, 0x73, 0x6F, 0x75, 0x72, 0x63, 0x65, 0x4B, 0x65, 0x79, 0x54, 0x79, 0x70, 0x65, 0x31, 0x22, 0x7D, 0x3D, 0x55, 0x69, 0x6E, 0x74, 0x33, 0x32, 0x3A, 0x33, 0x3A, 0x7B, 0x30, 0x78, 0x35, 0x61, 0x64, 0x30, 0x65, 0x38, 0x31, 0x37, 0x2C, 0x30, 0x78, 0x62, 0x32, 0x64, 0x36, 0x64, 0x65, 0x62, 0x65, 0x2C, 0x30, 0x78, 0x30, 0x30, 0x30, 0x33, 0x30, 0x30, 0x30, 0x30, 0x7D, 0x0D, 0x0A, 0x30, 0x78, 0x32, 0x37, 0x38, 0x31, 0x32, 0x38, 0x33, 0x32, 0x3A, 0x7B, 0x22, 0x57, 0x65, 0x61, 0x6C, 0x74, 0x68, 0x22, 0x7D, 0x3D, 0x55, 0x69, 0x6E, 0x74, 0x38, 0x3A, 0x30, 0x3A, 0x7B, 0x30, 0x78, 0x30, 0x31, 0x7D, 0x0D, 0x0A, 0x30, 0x78, 0x32, 0x37, 0x38, 0x31, 0x32, 0x38, 0x33, 0x33, 0x3A, 0x7B, 0x22, 0x50, 0x75, 0x72, 0x70, 0x6F, 0x73, 0x65, 0x22, 0x7D, 0x3D, 0x55, 0x69, 0x6E, 0x74, 0x38, 0x3A, 0x30, 0x3A, 0x7B, 0x30, 0x78, 0x30, 0x32, 0x7D, 0x0D, 0x0A, 0x30, 0x78, 0x32, 0x37, 0x38, 0x31, 0x32, 0x38, 0x33, 0x34, 0x3A, 0x7B, 0x22, 0x43, 0x61, 0x70, 0x61, 0x63, 0x69, 0x74, 0x79, 0x20, 0x53, 0x61, 0x74, 0x69, 0x73, 0x66, 0x69, 0x65, 0x64, 0x22, 0x7D, 0x3D, 0x55, 0x69, 0x6E, 0x74, 0x33, 0x32, 0x3A, 0x32, 0x3A, 0x7B, 0x30, 0x78, 0x30, 0x30, 0x30, 0x30, 0x33, 0x31, 0x31, 0x30, 0x2C, 0x30, 0x78, 0x30, 0x30, 0x30, 0x30, 0x30, 0x32, 0x65, 0x66, 0x7D, 0x0D, 0x0A, 0x30, 0x78, 0x32, 0x37, 0x38, 0x31, 0x32, 0x38, 0x35, 0x31, 0x3A, 0x7B, 0x22, 0x50, 0x6F, 0x6C, 0x6C, 0x75, 0x74, 0x69, 0x6F, 0x6E, 0x20, 0x61, 0x74, 0x20, 0x63, 0x65, 0x6E, 0x74, 0x72, 0x65, 0x22, 0x7D, 0x3D, 0x53, 0x69, 0x6E, 0x74, 0x33, 0x32, 0x3A, 0x34, 0x3A, 0x7B, 0x30, 0x78, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x37, 0x2C, 0x30, 0x78, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x33, 0x2C, 0x30, 0x78, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x31, 0x36, 0x2C, 0x30, 0x78, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x7D, 0x0D, 0x0A, 0x30, 0x78, 0x32, 0x37, 0x38, 0x31, 0x32, 0x38, 0x35, 0x34, 0x3A, 0x7B, 0x22, 0x50, 0x6F, 0x77, 0x65, 0x72, 0x20, 0x43, 0x6F, 0x6E, 0x73, 0x75, 0x6D, 0x65, 0x64, 0x22, 0x7D, 0x3D, 0x55, 0x69, 0x6E, 0x74, 0x33, 0x32, 0x3A, 0x30, 0x3A, 0x7B, 0x30, 0x78, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x31, 0x30, 0x7D, 0x0D, 0x0A, 0x30, 0x78, 0x32, 0x39, 0x32, 0x34, 0x34, 0x64, 0x62, 0x35, 0x3A, 0x7B, 0x22, 0x46, 0x6C, 0x61, 0x6D, 0x6D, 0x61, 0x62, 0x69, 0x6C, 0x69, 0x74, 0x79, 0x22, 0x7D, 0x3D, 0x55, 0x69, 0x6E, 0x74, 0x38, 0x3A, 0x30, 0x3A, 0x7B, 0x30, 0x78, 0x32, 0x64, 0x7D, 0x0D, 0x0A, 0x30, 0x78, 0x32, 0x61, 0x34, 0x39, 0x39, 0x66, 0x38, 0x35, 0x3A, 0x7B, 0x22, 0x51, 0x75, 0x65, 0x72, 0x79, 0x20, 0x65, 0x78, 0x65, 0x6D, 0x70, 0x6C, 0x61, 0x72, 0x20, 0x47, 0x55, 0x49, 0x44, 0x22, 0x7D, 0x3D, 0x55, 0x69, 0x6E, 0x74, 0x33, 0x32, 0x3A, 0x30, 0x3A, 0x7B, 0x30, 0x78, 0x63, 0x61, 0x35, 0x36, 0x37, 0x38, 0x33, 0x61, 0x7D, 0x0D, 0x0A, 0x30, 0x78, 0x32, 0x63, 0x38, 0x66, 0x38, 0x37, 0x34, 0x36, 0x3A, 0x7B, 0x22, 0x45, 0x78, 0x65, 0x6D, 0x70, 0x6C, 0x61, 0x72, 0x20, 0x43, 0x61, 0x74, 0x65, 0x67, 0x6F, 0x72, 0x79, 0x22, 0x7D, 0x3D, 0x55, 0x69, 0x6E, 0x74, 0x33, 0x32, 0x3A, 0x30, 0x3A, 0x7B, 0x30, 0x78, 0x38, 0x63, 0x38, 0x66, 0x62, 0x62, 0x63, 0x63, 0x7D, 0x0D, 0x0A, 0x30, 0x78, 0x34, 0x39, 0x39, 0x61, 0x66, 0x61, 0x33, 0x38, 0x3A, 0x7B, 0x22, 0x43, 0x6F, 0x6E, 0x73, 0x74, 0x72, 0x75, 0x63, 0x74, 0x69, 0x6F, 0x6E, 0x20, 0x54, 0x69, 0x6D, 0x65, 0x22, 0x7D, 0x3D, 0x55, 0x69, 0x6E, 0x74, 0x38, 0x3A, 0x30, 0x3A, 0x7B, 0x30, 0x78, 0x31, 0x30, 0x7D, 0x0D, 0x0A, 0x30, 0x78, 0x34, 0x39, 0x62, 0x65, 0x64, 0x61, 0x33, 0x31, 0x3A, 0x7B, 0x22, 0x4D, 0x61, 0x78, 0x46, 0x69, 0x72, 0x65, 0x53, 0x74, 0x61, 0x67, 0x65, 0x22, 0x7D, 0x3D, 0x55, 0x69, 0x6E, 0x74, 0x38, 0x3A, 0x30, 0x3A, 0x7B, 0x30, 0x78, 0x30, 0x34, 0x7D, 0x0D, 0x0A, 0x30, 0x78, 0x36, 0x38, 0x65, 0x65, 0x39, 0x37, 0x36, 0x34, 0x3A, 0x7B, 0x22, 0x50, 0x6F, 0x6C, 0x6C, 0x75, 0x74, 0x69, 0x6F, 0x6E, 0x20, 0x52, 0x61, 0x64, 0x69, 0x75, 0x73, 0x22, 0x7D, 0x3D, 0x46, 0x6C, 0x6F, 0x61, 0x74, 0x33, 0x32, 0x3A, 0x34, 0x3A, 0x7B, 0x35, 0x2C, 0x35, 0x2C, 0x30, 0x2C, 0x30, 0x7D, 0x0D, 0x0A, 0x30, 0x78, 0x38, 0x61, 0x31, 0x63, 0x33, 0x65, 0x37, 0x32, 0x3A, 0x7B, 0x22, 0x57, 0x6F, 0x72, 0x74, 0x68, 0x22, 0x7D, 0x3D, 0x53, 0x69, 0x6E, 0x74, 0x36, 0x34, 0x3A, 0x30, 0x3A, 0x7B, 0x30, 0x78, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x61, 0x39, 0x7D, 0x0D, 0x0A, 0x30, 0x78, 0x38, 0x63, 0x62, 0x33, 0x35, 0x31, 0x31, 0x66, 0x3A, 0x7B, 0x22, 0x4F, 0x63, 0x63, 0x75, 0x70, 0x61, 0x6E, 0x74, 0x20, 0x54, 0x79, 0x70, 0x65, 0x73, 0x22, 0x7D, 0x3D, 0x55, 0x69, 0x6E, 0x74, 0x33, 0x32, 0x3A, 0x31, 0x3A, 0x7B, 0x30, 0x78, 0x30, 0x30, 0x30, 0x30, 0x33, 0x31, 0x31, 0x30, 0x7D, 0x0D, 0x0A, 0x30, 0x78, 0x61, 0x61, 0x31, 0x64, 0x64, 0x33, 0x39, 0x36, 0x3A, 0x7B, 0x22, 0x4F, 0x63, 0x63, 0x75, 0x70, 0x61, 0x6E, 0x74, 0x47, 0x72, 0x6F, 0x75, 0x70, 0x73, 0x22, 0x7D, 0x3D, 0x55, 0x69, 0x6E, 0x74, 0x33, 0x32, 0x3A, 0x34, 0x3A, 0x7B, 0x30, 0x78, 0x30, 0x30, 0x30, 0x30, 0x31, 0x30, 0x30, 0x31, 0x2C, 0x30, 0x78, 0x30, 0x30, 0x30, 0x30, 0x32, 0x30, 0x30, 0x30, 0x2C, 0x30, 0x78, 0x30, 0x30, 0x30, 0x30, 0x32, 0x30, 0x30, 0x31, 0x2C, 0x30, 0x78, 0x30, 0x30, 0x30, 0x31, 0x33, 0x31, 0x31, 0x30, 0x7D, 0x0D, 0x0A, 0x30, 0x78, 0x61, 0x61, 0x31, 0x64, 0x64, 0x33, 0x39, 0x37, 0x3A, 0x7B, 0x22, 0x53, 0x46, 0x58, 0x3A, 0x51, 0x75, 0x65, 0x72, 0x79, 0x20, 0x53, 0x6F, 0x75, 0x6E, 0x64, 0x22, 0x7D, 0x3D, 0x55, 0x69, 0x6E, 0x74, 0x33, 0x32, 0x3A, 0x30, 0x3A, 0x7B, 0x30, 0x78, 0x32, 0x61, 0x38, 0x39, 0x31, 0x36, 0x61, 0x62, 0x7D, 0x0D, 0x0A, 0x30, 0x78, 0x61, 0x61, 0x38, 0x33, 0x35, 0x35, 0x38, 0x66, 0x3A, 0x7B, 0x22, 0x43, 0x72, 0x61, 0x6E, 0x65, 0x20, 0x48, 0x69, 0x6E, 0x74, 0x73, 0x22, 0x7D, 0x3D, 0x55, 0x69, 0x6E, 0x74, 0x38, 0x3A, 0x30, 0x3A, 0x7B, 0x30, 0x78, 0x30, 0x30, 0x7D, 0x0D, 0x0A, 0x30, 0x78, 0x63, 0x38, 0x65, 0x64, 0x32, 0x64, 0x38, 0x34, 0x3A, 0x7B, 0x22, 0x57, 0x61, 0x74, 0x65, 0x72, 0x20, 0x43, 0x6F, 0x6E, 0x73, 0x75, 0x6D, 0x65, 0x64, 0x22, 0x7D, 0x3D, 0x55, 0x69, 0x6E, 0x74, 0x33, 0x32, 0x3A, 0x30, 0x3A, 0x7B, 0x30, 0x78, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x39, 0x38, 0x7D, 0x0D, 0x0A, 0x30, 0x78, 0x65, 0x39, 0x31, 0x61, 0x30, 0x62, 0x35, 0x66, 0x3A, 0x7B, 0x22, 0x42, 0x75, 0x69, 0x6C, 0x64, 0x69, 0x6E, 0x67, 0x20, 0x76, 0x61, 0x6C, 0x75, 0x65, 0x22, 0x7D, 0x3D, 0x53, 0x69, 0x6E, 0x74, 0x36, 0x34, 0x3A, 0x30, 0x3A, 0x7B, 0x30, 0x78, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x31, 0x36, 0x37, 0x38, 0x7D, 0x0D, 0x0A };

            //Sample data from Jim CarProp Pack 1.2.dat (blank exemplars)
            public static byte[] entrynullproperty = { 0x45, 0x51, 0x5A, 0x42, 0x31, 0x23, 0x23, 0x23, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            public static byte[] entrynullproperty_extradata = { 0x45, 0x51, 0x5A, 0x42, 0x31, 0x23, 0x23, 0x23, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x57, 0xDF, 0x81, 0x70, 0x00, 0x49, 0x00 };
        }



        // 00x Misc Test Methods
        [TestClass]
        public class _00x_MiscTests {
            
        }



        // 01x Test Methods for DBPFUtil class
        [TestClass]
        public class _01x_DBPFUtil {
            [TestMethod]
            public void Test_010_DBPFUtil_IsFileDBPF() {
                Assert.IsTrue(DBPFUtil.IsValidDBPF("C:\\Users\\Administrator\\Documents\\SimCity 4\\Plugins\\z_GraphModd. V2.dat"));
                Assert.IsTrue(DBPFUtil.IsValidDBPF("C:\\Program Files (x86)\\Steam\\steamapps\\common\\SimCity 4 Deluxe\\Plugins\\CAS_AutoHistorical_v0.0.2.dat"));
                Assert.IsFalse(DBPFUtil.IsValidDBPF("C:\\Program Files (x86)\\Steam\\steamapps\\common\\SimCity 4 Deluxe\\Plugins\\CAS_AutoHistorical_v0.0.2.dll"));
                Assert.IsFalse(DBPFUtil.IsValidDBPF("C:\\Program Files (x86)\\Steam\\steamapps\\common\\SimCity 4 Deluxe\\Plugins\\Background3D0.png"));
                Assert.IsTrue(DBPFUtil.IsValidDBPF("C:\\Users\\Administrator\\Documents\\SimCity 4\\Plugins\\Network Addon Mod\\6 Miscellaneous\\Maxis Transit Lots\\Maxis Airports - Medium.dat"));

                Assert.IsTrue(DBPFUtil.IsValidDBPF("C:\\Users\\Administrator\\Documents\\SimCity 4\\Plugins\\z_GraphModd. V2.dat", true));
                Assert.IsTrue(DBPFUtil.IsValidDBPF("C:\\Program Files (x86)\\Steam\\steamapps\\common\\SimCity 4 Deluxe\\Plugins\\CAS_AutoHistorical_v0.0.2.dat", true));
                Assert.IsFalse(DBPFUtil.IsValidDBPF("C:\\Program Files (x86)\\Steam\\steamapps\\common\\SimCity 4 Deluxe\\Plugins\\CAS_AutoHistorical_v0.0.2.dll", true));
                Assert.IsFalse(DBPFUtil.IsValidDBPF("C:\\Program Files (x86)\\Steam\\steamapps\\common\\SimCity 4 Deluxe\\Plugins\\Background3D0.png", true));
                Assert.IsTrue(DBPFUtil.IsValidDBPF("C:\\Users\\Administrator\\Documents\\SimCity 4\\Plugins\\Network Addon Mod\\6 Miscellaneous\\Maxis Transit Lots\\Maxis Airports - Medium.dat", true));
            }

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
                Assert.AreEqual("6534284A", DBPFUtil.ToHexString(1697917002, 8, true, false));
                Assert.AreEqual("6534284a", DBPFUtil.ToHexString(1697917002, 8, false, false));
                Assert.AreEqual("0x6534284A", DBPFUtil.ToHexString(1697917002, 8, true, true));
                Assert.AreEqual("0x6534284a", DBPFUtil.ToHexString(1697917002, 8, false));
                Assert.AreEqual("0x4A283465", DBPFUtil.ToHexString(1244148837, 8, true));
                Assert.AreEqual("0x4a283465", DBPFUtil.ToHexString(1244148837, 8));
                Assert.AreEqual("6534284A", DBPFUtil.ToHexString(0x6534284A, 8, true, false));
                Assert.AreEqual("4A283465", DBPFUtil.ToHexString(0x4A283465, 8, true, false));
                Assert.AreEqual("0x4D2", DBPFUtil.ToHexString(1234, 3, true));
                Assert.AreEqual("0x4d2", DBPFUtil.ToHexString(1234, 3));
                Assert.AreEqual("0x000004D2", DBPFUtil.ToHexString(1234, 8, true));
            }

            //[Ignore]
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

                Assert.AreEqual(null, ByteArrayHelper.ToAString(null));
            }

            [TestMethod]
            public void Test_014_DBPFUtil_StringToByteArray() {
                string s1 = "Test";
                byte[] b1 = { 0x54, 0x65, 0x73, 0x74 };
                string s2 = "Parks Aura";
                byte[] b2 = { 0x50, 0x61, 0x72, 0x6b, 0x73, 0x20, 0x41, 0x75, 0x72, 0x61 };
                CollectionAssert.AreEqual(b1, ByteArrayHelper.ToBytes(s1, true));
                CollectionAssert.AreEqual(b2, ByteArrayHelper.ToBytes(s2, true));
            }

            [TestMethod]
            public void Test_015_DBPFUtil_DateFromUnix() {
                uint u1 = 0x5e115977; // 1/5/2020 3:35:19 AM 
                DateTime d1 = new DateTime(2020, 1, 5, 3, 35, 19);
                uint u2 = 0x60557b29; // 3/20/2021 4:33:45 AM
                DateTime d2 = new DateTime(2021, 3, 20, 4, 33, 45);

                Assert.AreEqual(d1, DBPFUtil.UnixToDate(u1));
                Assert.AreEqual(d2, DBPFUtil.UnixToDate(u2));
            }
        }



        // 02x Test methods for DBPFCompression class
        [TestClass]
        public class _02x_DBPFCompression {

            [TestMethod]
            public void Test_020_DBPFCompression_IsCompressed() {
                Assert.IsTrue(QFS.IsCompressed(TestArrays.compressedentry_b));
                Assert.IsFalse(QFS.IsCompressed(TestArrays.notcompressedentry_b));
                Assert.IsFalse(QFS.IsCompressed(TestArrays.decompressedentry_b));
            }

            [TestMethod]
            public void Test_021_DBPFCompression_GetDecompressedSize() {
                Assert.AreEqual((uint) 44, QFS.GetDecompressedSize(TestArrays.notcompressedentry_b));
                Assert.AreEqual((uint) 446, QFS.GetDecompressedSize(TestArrays.compressedentry_b));
                Assert.AreEqual((uint) 446, QFS.GetDecompressedSize(TestArrays.decompressedentry_b)); //BUG - figure out why this returns 318 when read from the index below
            }

            [TestMethod]
            public void Test_025_DBPFCompression_Decompress() {
                CollectionAssert.AreEqual(TestArrays.notcompressedentry_b, QFS.Decompress(TestArrays.notcompressedentry_b));
                CollectionAssert.AreEqual(TestArrays.decompressedentry_b, QFS.Decompress(TestArrays.compressedentry_b));
            }

            [TestMethod]
            public void Test_026_QFS_Compress() {
                DBPFFile dbpf = new DBPFFile("C:\\Users\\Administrator\\Documents\\SimCity 4\\Plugins\\Fixed Underfunded Notices (Med-High) - Copy.dat");
                dbpf.DecodeAllEntries();
                dbpf.EncodeAllEntries();
                dbpf.SaveAs("C:\\Users\\Administrator\\Documents\\SimCity 4\\Plugins\\Fixed Underfunded Notices (Med-High) - Copy2.dat");
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

            [TestMethod]
            public void Test_032_ByteArrayHelper_ReadAUint() {
                byte[] dbpf = { 0x44, 0x42, 0x50, 0x46 };
                Assert.AreEqual((uint) 0x44425046, ByteArrayHelper.ReadBytesIntoUint(dbpf));
                byte[] arr1 = { 0x00, 0x00, 0x10, 0x00 };
                Assert.AreEqual((uint) 0x00001000, ByteArrayHelper.ReadBytesIntoUint(arr1));
                byte[] arr2 = { 0x07, 0x00, 0x00, 0x30 };
                Assert.AreEqual((uint) 0x07000030, ByteArrayHelper.ReadBytesIntoUint(arr2));
            }
        }



        // 05x Test Methods for DBPFTGI Class
        [TestClass]
        public class _05x_DBPFTGI {

            [TestMethod]
            public void Test_050_DBPFTGI_CleanTGIFormat() {
                string s1 = "0x5ad0e817 0x5283112c 0x00030000";
                string s2 = "0x6534284a-0xbf3fbe81-0xe1278c85";
                string s3 = "0x5ad0e817_____0x7c051bc2_____0x00030000";
                string s4 = "0x6534284a, 0xbf3fbe81, 0x6208ab6f";
                string s5 = "0x5ad0 0x5 0x003";

                Assert.AreEqual("0x5ad0e817, 0x5283112c, 0x00030000", DBPFTGI.CleanTGIFormat(s1));
                Assert.AreEqual("0x6534284a, 0xbf3fbe81, 0xe1278c85", DBPFTGI.CleanTGIFormat(s2));
                Assert.AreEqual("0x5ad0e817, 0x7c051bc2, 0x00030000", DBPFTGI.CleanTGIFormat(s3));
                Assert.AreEqual("0x6534284a, 0xbf3fbe81, 0x6208ab6f", DBPFTGI.CleanTGIFormat(s4));
                Assert.AreEqual("0x00005ad0, 0x00000005, 0x00000003", DBPFTGI.CleanTGIFormat(s5));
            }

            [TestMethod]
            public void Test_051_DBPFTGI_ParseTGIString() {
                string s1 = "0x5ad0e817 0x5283112c 0x00030000";
                string s2 = "0x6534284a-0xbf3fbe81-0xe1278c85";
                string s3 = "0x5ad0e817_____0x7c051bc2_____0x00030000";
                string s4 = "0x6534284a, 0xbf3fbe81, 0x6208ab6f";
                string s5 = "0x5ad0 0x5 0x003";

                Assert.IsTrue(new TGI(0x5ad0e817, 0x5283112c, 0x00030000).Equals(DBPFTGI.ParseTGIString(s1)));
                Assert.IsTrue(new TGI(0x6534284a, 0xbf3fbe81, 0xe1278c85).Equals(DBPFTGI.ParseTGIString(s2)));
                Assert.IsTrue(new TGI(0x5ad0e817, 0x7c051bc2, 0x00030000).Equals(DBPFTGI.ParseTGIString(s3)));
                Assert.IsTrue(new TGI(0x6534284a, 0xbf3fbe81, 0x6208ab6f).Equals(DBPFTGI.ParseTGIString(s4)));
                Assert.IsTrue(new TGI(0x5ad0, 0x5, 0x3).Equals(DBPFTGI.ParseTGIString(s5)));

            }



            [TestMethod]
            public void Test_053_DBPFTGI_Equals() {
                TGI tgi1 = new TGI(0, 0, 0);
                TGI tgi2 = new TGI(0, 0, 0);
                TGI tgi3 = new TGI(0xe86b1eef, 0xe86b1eef, 0x286b1f03);
                TGI tgi4 = new TGI(3899334383, 3899334383, 678108931);
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
                Assert.IsFalse(tgi1.Equals(0));
            }

            [TestMethod]
            public void Test_054_DBPFTGI_MatchesKnownTGI() {
                //If called from a TGI object
                TGI tgi_blank = new TGI(0, 0, 0);
                TGI tgi_exemplar = new TGI(0x6534284a, 0, 0);
                TGI tgi_exemplarRail = new TGI(0x6534284a, 0xe8347989, 0);
                TGI tgi_exemplarRail2 = new TGI(0x6534284a, 0xe8347989, 0x1ab4e56a);

                Assert.IsTrue(tgi_blank.Matches(DBPFTGI.BLANKTGI));
                Assert.IsTrue(tgi_blank.Matches(DBPFTGI.NULLTGI));

                Assert.IsTrue(tgi_exemplar.Matches(DBPFTGI.EXEMPLAR));
                Assert.IsTrue(tgi_exemplarRail.Matches(DBPFTGI.EXEMPLAR_RAIL));
                Assert.IsTrue(tgi_exemplarRail2.Matches(DBPFTGI.EXEMPLAR_RAIL));
                Assert.IsTrue(tgi_exemplarRail2.Matches(DBPFTGI.EXEMPLAR));
                Assert.IsFalse(tgi_exemplar.Matches(DBPFTGI.COHORT));
                Assert.IsTrue(tgi_exemplar.Matches(DBPFTGI.NULLTGI));
                Assert.IsFalse(tgi_exemplar.Matches(DBPFTGI.PNG));


                //If called from an Entry object
                DBPFEntry entry_blank = new DBPFEntryEXMP(tgi_blank);
                DBPFEntry entry_exemplar = new DBPFEntryEXMP(tgi_exemplar);
                DBPFEntry entry_exemplarRail = new DBPFEntryEXMP(tgi_exemplarRail);
                DBPFEntry entry_exemplarRail2 = new DBPFEntryEXMP(tgi_exemplarRail2);

                Assert.IsTrue(entry_blank.MatchesEntryType(DBPFTGI.BLANKTGI));
                Assert.IsTrue(entry_blank.MatchesEntryType(DBPFTGI.NULLTGI));

                Assert.IsTrue(entry_exemplar.MatchesEntryType(DBPFTGI.EXEMPLAR));
                Assert.IsTrue(entry_exemplarRail.MatchesEntryType(DBPFTGI.EXEMPLAR_RAIL));
                Assert.IsTrue(entry_exemplarRail2.MatchesEntryType(DBPFTGI.EXEMPLAR_RAIL));
                Assert.IsTrue(entry_exemplarRail2.MatchesEntryType(DBPFTGI.EXEMPLAR));
                Assert.IsFalse(entry_exemplar.MatchesEntryType(DBPFTGI.COHORT));
                Assert.IsTrue(entry_exemplar.MatchesEntryType(DBPFTGI.NULLTGI));
                Assert.IsFalse(entry_exemplar.MatchesEntryType(DBPFTGI.PNG));
            }

            [TestMethod]
            public void Test_055_DBPFTGI_MatchesAnyKnown() {
                TGI tgi_blank = new TGI(0, 0, 0);
                TGI tgi_exemplar = new TGI(0x6534284a, 0, 0);
                TGI tgi_exemplarRail = new TGI(0x6534284a, 0xe8347989, 0);
                TGI tgi_exemplarRail2 = new TGI(0x6534284a, 0xe8347989, 0x1ab4e56a);
                TGI tgi_PNG_Icon = new TGI(0x856ddbac, 0x6a386d26, 0x1ab4e56f);
                TGI tgi_PNG = new TGI(0x856ddbac, 0x6a386d27, 0x1ab4e56f);

                TGI returned = tgi_blank.MatchesAnyKnown();
                Assert.AreEqual(DBPFTGI.BLANKTGI, returned);
                Assert.AreEqual("BLANK", returned.GetEntryType());

                returned = tgi_exemplar.MatchesAnyKnown();
                Assert.AreEqual(DBPFTGI.EXEMPLAR, returned);
                Assert.AreEqual("EXMP", returned.GetEntryType());
                Assert.AreEqual("EXEMPLAR", returned.GetEntryDetail());

                returned = tgi_exemplarRail.MatchesAnyKnown();
                Assert.AreEqual("EXMP", returned.GetEntryType());
                Assert.AreEqual("EXEMPLAR_RAIL", returned.GetEntryDetail());

                returned = tgi_exemplarRail2.MatchesAnyKnown();
                Assert.AreEqual("EXMP", returned.GetEntryType());
                Assert.AreEqual("EXEMPLAR_RAIL", returned.GetEntryDetail());

                returned = tgi_PNG_Icon.MatchesAnyKnown();
                Assert.AreEqual("PNG", returned.GetEntryType());
                Assert.AreEqual("PNG_ICON", returned.GetEntryDetail());

                returned = tgi_PNG.MatchesAnyKnown();
                Assert.AreEqual("PNG", returned.GetEntryType());
                Assert.AreEqual("PNG", returned.GetEntryDetail());
            }

            //[TestMethod]
            //public void Test_056a_DBPFTGI_SetTGIusingTGI() {
            //    TGI tgi1 = new TGI(0x6534284a, 0, 0);
            //    TGI tgi2 = tgi1;
            //    tgi2 =.SetTGI(TGI.EXEMPLAR_AVENUE);
            //    Assert.AreEqual((uint) 0x6534284A, tgi2.TypeID);
            //    Assert.AreEqual((uint) 0xCB730FAC, tgi2.GroupID);
            //    Assert.AreNotEqual<uint?>(0, tgi2.InstanceID);
            //    Assert.AreEqual("EXMP", tgi2.Category);
            //    Assert.AreEqual("EXEMPLAR_AVENUE", tgi2.Detail);
            //    tgi1.SetTGI(TGI.NULLTGI);
            //    Assert.AreEqual(tgi1.ToString(), tgi1.ToString());

            //    TGI tgi3 = new TGI(0, 2, 3);
            //    tgi3.SetTGI(DBPFTGI.LUA);
            //    Assert.AreEqual((uint) 0xCA63E2A3, tgi3.TypeID);
            //    Assert.AreEqual((uint) 0x4A5E8EF6, tgi3.GroupID);
            //    Assert.AreNotEqual<uint?>(0, tgi3.InstanceID);
            //    Assert.AreEqual("LUA", tgi3.Category);
            //    Assert.AreEqual("LUA", tgi3.Detail);
            //}

            //[TestMethod]
            //public void Test_056b_DBPFTGI_SetTGIusingUint() {
            //    TGI tgi1 = new TGI(0x6534284a, 0, 1000001);
            //    tgi1.SetTGI(null, null, 100);
            //    Assert.AreEqual("0x6534284a, 0x00000000, 0x00000064, EXMP, EXEMPLAR", tgi1.ToString());
            //    tgi1.SetTGI(100, 100, 100);
            //    Assert.AreEqual("0x00000064, 0x00000064, 0x00000064, NULL, NULLTGI", tgi1.ToString());

            //    TGI tgi2 = new TGI(DBPFTGI.LUA);
            //    tgi2.SetTGI(null, null, 100);
            //    Assert.AreEqual("0xca63e2a3, 0x4a5e8ef6, 0x00000064, LUA, LUA", tgi2.ToString());
            //}

            [TestMethod]
            public void Test_057_DBPFTGI_RandomizeGroupOrInstance() {
                TGI tgi1 = new TGI(DBPFTGI.EXEMPLAR);
                tgi1.RandomizeInstance();
                tgi1.RandomizeGroup();
                Assert.AreEqual(DBPFTGI.EXEMPLAR.GetEntryType(), tgi1.GetEntryType());

                TGI tgi2 = new TGI(DBPFTGI.EXEMPLAR_ROAD);
                tgi2.RandomizeInstance();
                Assert.AreEqual(DBPFTGI.EXEMPLAR_ROAD.GetEntryType(), tgi2.GetEntryType());
                tgi2.RandomizeGroup();
                Assert.AreEqual(DBPFTGI.EXEMPLAR_ROAD.GetEntryType(), tgi2.GetEntryType());
                Assert.AreNotEqual(DBPFTGI.EXEMPLAR_ROAD.GetEntryDetail(), tgi2.GetEntryDetail());
                Assert.AreEqual(DBPFTGI.EXEMPLAR.GetEntryType(), tgi2.GetEntryType());

                uint group = (uint) tgi2.GroupID;
                tgi2.RandomizeGroup();
                Assert.AreNotEqual(group, tgi2.GroupID);
            }

            [TestMethod]
            public void Test_058_DBPFTGI_SetRandomFromNewInstance() {
                TGI tgi1 = new TGI(DBPFTGI.EXEMPLAR);
                Assert.AreNotEqual<uint?>(0, tgi1.GroupID);
                Assert.AreNotEqual<uint?>(0, tgi1.InstanceID);

                TGI tgi2 = new TGI(DBPFTGI.INI);
                Assert.AreEqual((uint) 0, tgi2.TypeID);
                Assert.AreEqual(0x8a5971c5, tgi2.GroupID);
                Assert.AreNotEqual<uint?>(0, tgi2.InstanceID);
            }
        }



        /// <summary>
        /// 06x Test Methods for DBPFProperty Class
        /// </summary>
        [TestClass]
        public class _06x_DBPFProperty {
            [TestMethod]
            public void Test_060_DBPFPropertyDataType_Equals() {
                Assert.AreEqual(DBPFPropertyDataType.SINT32, DBPFPropertyDataType.SINT32);
                Assert.AreEqual(DBPFPropertyDataType.STRING, DBPFPropertyDataType.STRING);
                Assert.AreNotEqual(DBPFPropertyDataType.UINT8, DBPFPropertyDataType.SINT32);
                Assert.AreNotEqual(DBPFPropertyDataType.STRING, DBPFPropertyDataType.FLOAT32);
            }


            [TestMethod]
            public void Test_060a_DBPFPropertyDataType_ReturnType() {
                Assert.AreEqual("SINT32", DBPFPropertyDataType.SINT32.Name);
                Assert.AreEqual(DBPFPropertyDataType.BOOL, DBPFPropertyDataType.LookupDataType(0xB00));
                Assert.AreEqual(DBPFPropertyDataType.UINT32.Name, DBPFPropertyDataType.LookupDataType(0x300).Name);
                Assert.AreEqual(4, DBPFPropertyDataType.LookupDataType(0x300).Length);
            }

            [TestMethod]
            public void Test_060b_DBPFPropertyDataType_ReturnDataType() {
                string a = "";
                Assert.AreEqual(a.GetType(), DBPFPropertyDataType.STRING.PrimitiveDataType);
                uint b = 0x0;
                Assert.AreEqual(b.GetType(), DBPFPropertyDataType.UINT32.PrimitiveDataType);
                byte c = 0x08;
                Assert.AreEqual(c.GetType(), DBPFPropertyDataType.UINT8.PrimitiveDataType);
            }

            [TestMethod]
            public void Test_061a_DBPFPropertyString_Binary() {
                DBPFEntryEXMP entry = new DBPFEntryEXMP(DBPFTGI.EXEMPLAR, 0, 0, 0, TestArrays.decompressedentry_b);
                entry.Decode();

                byte[] byteparks = { 0x50, 0x61, 0x72, 0x6B, 0x73 };
                string stringparks = "Parks";
                byte[] byteparksaura = { 0x50, 0x61, 0x72, 0x6b, 0x73, 0x20, 0x41, 0x75, 0x72, 0x61 };
                string stringparksaura = "Parks Aura";
                byte[] bytedataviewparksaura = { 0x44, 0x61, 0x74, 0x61, 0x56, 0x69, 0x65, 0x77, 0x3A, 0x20, 0x50, 0x61, 0x72, 0x6B, 0x73, 0x20, 0x41, 0x75, 0x72, 0x61 };
                string stringdataviewparksaura = "DataView: Parks Aura";

                //Test a property read from file		
                DBPFProperty propb = entry.ListOfProperties.GetValueAtIndex(1);
                Assert.AreEqual((uint) 0x20, propb.ID);
                Assert.AreEqual(20, propb.NumberOfReps);
                Assert.AreEqual(DBPFPropertyDataType.STRING, propb.DataType);
                Assert.AreEqual(stringdataviewparksaura, propb.GetData());

                //Compare to property with known values
                DBPFPropertyString knownprop = new DBPFPropertyString(stringdataviewparksaura);
                knownprop.ID = 0x20;
                Assert.AreEqual(knownprop.ID, propb.ID);
                Assert.AreEqual(knownprop.NumberOfReps, propb.NumberOfReps);
                Assert.AreEqual(knownprop.DataType, propb.DataType);
                Assert.AreEqual(knownprop.GetData(), propb.GetData());

                //Check for no differences between values and valuesDecoded when each is changed
                propb.SetData(stringparksaura);
                Assert.AreEqual(stringparksaura, propb.GetData());
                Assert.AreEqual(stringparksaura.Length, propb.NumberOfReps);
                propb.SetData(stringparks);
                Assert.AreEqual(stringparks, propb.GetData());
                Assert.AreEqual(stringparks.Length, propb.NumberOfReps);
            }

            [TestMethod]
            public void Test_061b_DBPFPropertyLong_Binary() {
                DBPFEntryEXMP entry = new DBPFEntryEXMP(DBPFTGI.EXEMPLAR, 0, 0, 0, TestArrays.decompressedentry_b);
                entry.Decode();

                DBPFProperty propb;
                DBPFPropertyLong propknown;
                List<long> vals;

                //Single UInt32 value
                vals = new List<long> { 0x23 };
                propb = entry.ListOfProperties.GetValueAtIndex(0);
                propknown = new DBPFPropertyLong(DBPFPropertyDataType.UINT32, 0x23);
                Assert.AreEqual((uint) 0x10, propb.ID);
                Assert.AreEqual(0, propb.NumberOfReps);
                Assert.AreEqual(DBPFPropertyDataType.UINT32, propb.DataType);
                CollectionAssert.AreEqual(vals, (System.Collections.ICollection) propb.GetData());
                Assert.AreEqual(propknown.NumberOfReps, propb.NumberOfReps);
                Assert.AreEqual(propknown.DataType, propb.DataType);
                CollectionAssert.AreEqual(propknown.GetData(), (System.Collections.ICollection) propb.GetData());

                //7 repetitions of 0 (for 8 total values of 8)
                vals = new List<long> { 0, 0, 0, 0, 0, 0, 0, 0 };
                propb = entry.ListOfProperties.GetValueAtIndex(2);
                propknown = new DBPFPropertyLong(DBPFPropertyDataType.UINT32, vals);
                Assert.AreEqual((uint) 0x4A0B47E0, propb.ID);
                Assert.AreEqual(8, propb.NumberOfReps);
                Assert.AreEqual(DBPFPropertyDataType.UINT32, propb.DataType);
                CollectionAssert.AreEqual(vals, (System.Collections.ICollection) propb.GetData());
                Assert.AreEqual(propknown.NumberOfReps, propb.NumberOfReps);
                Assert.AreEqual(propknown.DataType, propb.DataType);
                CollectionAssert.AreEqual(propknown.GetData(), (System.Collections.ICollection) propb.GetData());

                //Single True boolean value
                vals = new List<long> { 1 };
                propb = entry.ListOfProperties.GetValueAtIndex(3);
                propknown = new DBPFPropertyLong(DBPFPropertyDataType.BOOL, vals);
                Assert.AreEqual((uint) 0x4A0B47E1, propb.ID);
                Assert.AreEqual(0, propb.NumberOfReps);
                Assert.AreEqual(DBPFPropertyDataType.BOOL, propb.DataType);
                CollectionAssert.AreEqual(vals, (System.Collections.ICollection) propb.GetData());
                Assert.AreEqual(propknown.NumberOfReps, propb.NumberOfReps);
                Assert.AreEqual(propknown.DataType, propb.DataType);
                CollectionAssert.AreEqual(propknown.GetData(), (System.Collections.ICollection) propb.GetData());

                //Single False boolean value
                vals = new List<long> { 0 };
                propb = entry.ListOfProperties.GetValueAtIndex(4);
                propknown = new DBPFPropertyLong(DBPFPropertyDataType.BOOL, vals);
                Assert.AreEqual((uint) 0x4A0B47E2, propb.ID);
                Assert.AreEqual(0, propb.NumberOfReps);
                Assert.AreEqual(DBPFPropertyDataType.BOOL, propb.DataType);
                CollectionAssert.AreEqual(vals, (System.Collections.ICollection) propb.GetData());
                Assert.AreEqual(propknown.NumberOfReps, propb.NumberOfReps);
                Assert.AreEqual(propknown.DataType, propb.DataType);
                CollectionAssert.AreEqual(propknown.GetData(), (System.Collections.ICollection) propb.GetData());

                //Single UInt32 value of 0
                vals = new List<long> { 0 };
                propb = entry.ListOfProperties.GetValueAtIndex(5);
                propknown = new DBPFPropertyLong(DBPFPropertyDataType.UINT32, vals);
                Assert.AreEqual((uint) 0x4A0B47E3, propb.ID);
                Assert.AreEqual(0, propb.NumberOfReps);
                Assert.AreEqual(DBPFPropertyDataType.UINT32, propb.DataType);
                CollectionAssert.AreEqual(vals, (System.Collections.ICollection) propb.GetData());
                Assert.AreEqual(propknown.NumberOfReps, propb.NumberOfReps);
                Assert.AreEqual(propknown.DataType, propb.DataType);
                CollectionAssert.AreEqual(propknown.GetData(), (System.Collections.ICollection) propb.GetData());

                //28 UInt32s
                vals = new List<long> { 0x00000000, 0x70990000, 0x00000001, 0x70BC533C, 0x0000000C, 0x70BC533C, 0x0000000D, 0x70D98C79, 0x00000046, 0x70D98C79, 0x0000007F, 0x70F0C5BA, 0x00000080, 0x70FFFFFF, 0x00000081, 0x70E2F1DD, 0x000000B8, 0x70C5E3BB, 0x000000B9, 0x70A8D49A, 0x000000F2, 0x708AC679, 0x000000F3, 0x706AB758, 0x000000FE, 0x7046A836, 0x000000FF, 0x70009900 };
                propb = entry.ListOfProperties.GetValueAtIndex(6);
                propknown = new DBPFPropertyLong(DBPFPropertyDataType.UINT32, vals);
                Assert.AreEqual((uint) 0x4A0B47E4, propb.ID);
                Assert.AreEqual(28, propb.NumberOfReps);
                Assert.AreEqual(DBPFPropertyDataType.UINT32, propb.DataType);
                CollectionAssert.AreEqual(vals, (System.Collections.ICollection) propb.GetData());
                Assert.AreEqual(propknown.NumberOfReps, propb.NumberOfReps);
                Assert.AreEqual(propknown.DataType, propb.DataType);
                CollectionAssert.AreEqual(propknown.GetData(), (System.Collections.ICollection) propb.GetData());
            }

            [Ignore]
            [TestMethod]
            public void Test_061c_DBPFPropertyFloat_Binary() {

            }

            [TestMethod]
            public void Test_061d_DBPFPropertyString_Text() {
                DBPFEntryEXMP entry = new DBPFEntryEXMP(DBPFTGI.EXEMPLAR, 0, 0, 0, TestArrays.notcompressedentry_t);
                entry.Decode();

                DBPFProperty propt;
                DBPFPropertyString propknown;

                //1x String
                string val = "B62-CS$_Albertsons_60s_Grocery v 1.1";
                propt = entry.ListOfProperties.GetValueAtIndex(1);
                Assert.AreEqual(DBPFPropertyDataType.STRING, propt.DataType);
                Assert.AreEqual(1, propt.NumberOfReps);
                Assert.AreEqual(val, propt.GetData());
                propknown = new DBPFPropertyString(val, DBPFEntry.EncodingType.Text);
                Assert.AreEqual(propknown.NumberOfReps, propt.NumberOfReps);
                Assert.AreEqual(propknown.DataType, propt.DataType);
                Assert.AreEqual(propknown.GetData(), propt.GetData());
            }

            [TestMethod]
            public void Test_061e_DBPFPropertyLong_Text() {
                DBPFEntryEXMP entry = new DBPFEntryEXMP(DBPFTGI.EXEMPLAR, 0, 0, 0, TestArrays.notcompressedentry_t);
                entry.Decode();

                DBPFProperty propt;
                DBPFPropertyLong propknown;
                List<long> vals;

                //TODO - text property parsing: remaining single: Sint64, , , Bool, , , UInt16,
                //TODO - text property parsing: remaining multi : , , , Bool, UInt8, SInt64, UInt16,


                //1x Uint32
                vals = new List<long> { 0x2 };
                propt = entry.ListOfProperties.GetValueAtIndex(0);
                propknown = new DBPFPropertyLong(DBPFPropertyDataType.UINT32, vals, DBPFEntry.EncodingType.Text);
                Assert.AreEqual((uint) 0x00000010, propt.ID);
                Assert.AreEqual(0, propt.NumberOfReps);
                Assert.AreEqual(DBPFPropertyDataType.UINT32, propt.DataType);
                CollectionAssert.AreEqual(vals, (System.Collections.ICollection) propt.GetData());
                Assert.AreEqual(propknown.NumberOfReps, propt.NumberOfReps);
                Assert.AreEqual(propknown.DataType, propt.DataType);
                CollectionAssert.AreEqual(propknown.GetData(), (System.Collections.ICollection) propt.GetData());

                //1x Sint64
                vals = new List<long> { 0x00000000000000A9 };
                propt = entry.ListOfProperties.GetValueAtIndex(2);
                propknown = new DBPFPropertyLong(DBPFPropertyDataType.SINT64, vals, DBPFEntry.EncodingType.Text);
                Assert.AreEqual((uint) 0x099AFACD, propt.ID);
                Assert.AreEqual(0, propt.NumberOfReps);
                Assert.AreEqual(DBPFPropertyDataType.SINT64, propt.DataType);
                CollectionAssert.AreEqual(vals, (System.Collections.ICollection) propt.GetData());
                Assert.AreEqual(propknown.NumberOfReps, propt.NumberOfReps);
                Assert.AreEqual(propknown.DataType, propt.DataType);
                CollectionAssert.AreEqual(propknown.GetData(), (System.Collections.ICollection) propt.GetData());

                //1x Uint8
                vals = new List<long> { 0x01 };
                propt = entry.ListOfProperties.GetValueAtIndex(6);
                propknown = new DBPFPropertyLong(DBPFPropertyDataType.UINT8, vals, DBPFEntry.EncodingType.Text);
                Assert.AreEqual((uint) 0x27812832, propt.ID);
                Assert.AreEqual(0, propt.NumberOfReps);
                Assert.AreEqual(DBPFPropertyDataType.UINT8, propt.DataType);
                CollectionAssert.AreEqual(vals, (System.Collections.ICollection) propt.GetData());
                Assert.AreEqual(propknown.NumberOfReps, propt.NumberOfReps);
                Assert.AreEqual(propknown.DataType, propt.DataType);
                CollectionAssert.AreEqual(propknown.GetData(), (System.Collections.ICollection) propt.GetData());

                //4x Sint32
                vals = new List<long> { 0x07, 0x03, 0x16, 0x00 };
                propt = entry.ListOfProperties.GetValueAtIndex(9);
                propknown = new DBPFPropertyLong(DBPFPropertyDataType.SINT32, vals, DBPFEntry.EncodingType.Text);
                Assert.AreEqual((uint) 0x27812851, propt.ID);
                Assert.AreEqual(4, propt.NumberOfReps);
                Assert.AreEqual(DBPFPropertyDataType.SINT32, propt.DataType);
                CollectionAssert.AreEqual(vals, (System.Collections.ICollection) propt.GetData());
                Assert.AreEqual(propknown.NumberOfReps, propt.NumberOfReps);
                Assert.AreEqual(propknown.DataType, propt.DataType);
                CollectionAssert.AreEqual(propknown.GetData(), (System.Collections.ICollection) propt.GetData());

                //4x Uint32
                vals = new List<long> { 0x1001, 0x2000, 0x2001, 0x13110 };
                propt = entry.ListOfProperties.GetValueAtIndex(19);
                propknown = new DBPFPropertyLong(DBPFPropertyDataType.UINT32, vals, DBPFEntry.EncodingType.Text);
                Assert.AreEqual(0xAA1DD396, propt.ID);
                Assert.AreEqual(4, propt.NumberOfReps);
                Assert.AreEqual(DBPFPropertyDataType.UINT32, propt.DataType);
                CollectionAssert.AreEqual(vals, (System.Collections.ICollection) propt.GetData());
                Assert.AreEqual(propknown.NumberOfReps, propt.NumberOfReps);
                Assert.AreEqual(propknown.DataType, propt.DataType);
                CollectionAssert.AreEqual(propknown.GetData(), (System.Collections.ICollection) propt.GetData());
            }

            [TestMethod]
            public void Test_061f_DBPFPropertyFloat_Text() {
                DBPFEntryEXMP entry = new DBPFEntryEXMP(DBPFTGI.EXEMPLAR, 0, 0, 0, TestArrays.notcompressedentry_t);
                entry.Decode();

                DBPFProperty propt;
                DBPFPropertyFloat propknown;
                List<float> vals;

                //3x Float32
                vals = new List<float> { 81.589798f, 13.947300f, 39.442501f };
                propt = entry.ListOfProperties.GetValueAtIndex(3);
                propknown = new DBPFPropertyFloat(vals, DBPFEntry.EncodingType.Text);
                Assert.AreEqual<uint>(0x27812810, propt.ID);
                Assert.AreEqual(3, propt.NumberOfReps);
                Assert.AreEqual(DBPFPropertyDataType.FLOAT32, propt.DataType);
                CollectionAssert.AreEqual(vals, (System.Collections.ICollection) propt.GetData());
                Assert.AreEqual(propknown.NumberOfReps, propt.NumberOfReps);
                Assert.AreEqual(propknown.DataType, propt.DataType);
                CollectionAssert.AreEqual(propknown.GetData(), (System.Collections.ICollection) propt.GetData());

                //1x Float32
                vals = new List<float> { 0.5f };
                propt = entry.ListOfProperties.GetValueAtIndex(4);
                propknown = new DBPFPropertyFloat(0.5f, DBPFEntry.EncodingType.Text);
                Assert.AreEqual<uint>(0x27812811, propt.ID);
                Assert.AreEqual(1, propt.NumberOfReps);
                Assert.AreEqual(DBPFPropertyDataType.FLOAT32, propt.DataType);
                CollectionAssert.AreEqual(vals, (System.Collections.ICollection) propt.GetData());
                Assert.AreEqual(propknown.NumberOfReps, propt.NumberOfReps);
                Assert.AreEqual(propknown.DataType, propt.DataType);
                CollectionAssert.AreEqual(propknown.GetData(), (System.Collections.ICollection) propt.GetData());
            }

            [TestMethod]
            public void Test_062_DBPFProperty_DecodeNoProperties() {
                DBPFEntryEXMP entry = new DBPFEntryEXMP(DBPFTGI.EXEMPLAR, 0, 0, 0, TestArrays.entrynullproperty);
                entry.Decode();
                Assert.AreEqual(0, entry.ListOfProperties.Count);

                entry = new DBPFEntryEXMP(DBPFTGI.EXEMPLAR, 0, 0, 0, TestArrays.entrynullproperty_extradata);
                Assert.AreEqual(0, entry.ListOfProperties.Count);

                //Use an IRL example
                DBPFFile jimspack = new DBPFFile("C:\\Users\\Administrator\\OneDrive\\SC4 Deps\\Jim CarProp Pack 1.2.dat");
                entry = (DBPFEntryEXMP) jimspack.GetEntry(0);
                entry.Decode();
                Assert.AreEqual(0, entry.ListOfProperties.Count);
                entry = (DBPFEntryEXMP) jimspack.GetEntry(178);
                entry.Decode();
                Assert.AreEqual(0, entry.ListOfProperties.Count);
            }
        }



        /// <summary>
        /// 07x Test Methods for Property XML Parsing
        /// </summary>
        [TestClass]
        public class _07x_XMLProperty {
            [TestMethod]
            public void Test_070_XMLProperties_AllProperties() {
                Assert.AreEqual(0x56f, XMLProperties.AllProperties.Count);
            }

            [TestMethod]
            public void Test_071_XMLProperties_GetPropertyByID() {
                //< PROPERTY Name = "Item Button ID" ID = "0x8a2602bb" Type = "Uint32" Default = "0x00000000" ShowAsHex = "Y" >
                XMLExemplarProperty exmp = XMLProperties.GetXMLProperty(0x8a2602bb);
                Assert.AreEqual(0x8a2602bb, exmp.ID);
                Assert.AreEqual("Item Button ID", exmp.Name);
                Assert.AreEqual(DBPFPropertyDataType.UINT32, exmp.Type);
                Assert.AreEqual(true, exmp.ShowAsHex);
                CollectionAssert.AreEqual(new List<string> { "0x00000000" }, exmp.DefaultValue);
                Assert.AreEqual(null, exmp.MaxValue);

                //<PROPERTY Name="Path Offset Range for Peds" ID="0x29dd40c1" Type="Float32" Count="2" Default="-1 3" ShowAsHex="Y">
                exmp = XMLProperties.GetXMLProperty(0x29dd40c1);
                Assert.AreEqual((uint) 0x29dd40c1, exmp.ID);
                Assert.AreEqual("Path Offset Range for Peds", exmp.Name);
                Assert.AreEqual(DBPFPropertyDataType.FLOAT32, exmp.Type);
                Assert.AreEqual(true, exmp.ShowAsHex);
                CollectionAssert.AreEqual(new List<string> { "-1", "3" }, exmp.DefaultValue);
                Assert.AreEqual((short) 2, exmp.Count);

                //<PROPERTY Name="WaveMinTimeInState" ID="0x6932dc06" Type="Float32" Count="4" Default="12 0.230 0.5 2" ShowAsHex="Y">
                exmp = XMLProperties.GetXMLProperty(0x6932dc06);
                Assert.AreEqual((uint) 0x6932dc06, exmp.ID);
                Assert.AreEqual("WaveMinTimeInState", exmp.Name);
                Assert.AreEqual(DBPFPropertyDataType.FLOAT32, exmp.Type);
                Assert.AreEqual(true, exmp.ShowAsHex);
                CollectionAssert.AreEqual(new List<string> { "12", "0.230", "0.5", "2" }, exmp.DefaultValue);
                Assert.AreEqual((short) 4, exmp.Count);
                Assert.AreEqual(null, exmp.Step);

                //<PROPERTY Name="Health Effectiveness vs. Distance Effect" ID="0x891b3ae6" Type="Float32" Count="-2" Default="0 100" MinValue="0" MaxValue="100" ShowAsHex="Y">
                exmp = XMLProperties.GetXMLProperty(0x891b3ae6);
                Assert.AreEqual(0x891b3ae6, exmp.ID);
                Assert.AreEqual("Health Effectiveness vs. Distance Effect", exmp.Name);
                Assert.AreEqual(DBPFPropertyDataType.FLOAT32, exmp.Type);
                Assert.AreEqual(true, exmp.ShowAsHex);
                CollectionAssert.AreEqual(new List<string> { "0", "100" }, exmp.DefaultValue);
                Assert.AreEqual((short) -2, exmp.Count);
                Assert.AreEqual("0", exmp.MinValue);
                Assert.AreEqual("100", exmp.MaxValue);
                Assert.AreEqual(null, exmp.MaxLength);
            }

            [TestMethod]
            public void Test_072_XMLProperties_GetPropertyByName() {
                //< PROPERTY Name = "Item Button ID" ID = "0x8a2602bb" Type = "Uint32" Default = "0x00000000" ShowAsHex = "Y" >
                XMLExemplarProperty exmp = XMLProperties.GetXMLProperty("Item Button ID");
                Assert.AreEqual(0x8a2602bb, exmp.ID);
                Assert.AreEqual("Item Button ID", exmp.Name);
                Assert.AreEqual(DBPFPropertyDataType.UINT32, exmp.Type);
                Assert.AreEqual(true, exmp.ShowAsHex);
                CollectionAssert.AreEqual(new List<string> { "0x00000000" }, exmp.DefaultValue);
                Assert.AreEqual(null, exmp.MaxValue);

                exmp = XMLProperties.GetXMLProperty("ItembuTTonID");
                Assert.AreEqual(0x8a2602bb, exmp.ID);
                Assert.AreEqual("Item Button ID", exmp.Name);

                //<PROPERTY Name="Path Offset Range for Peds" ID="0x29dd40c1" Type="Float32" Count="2" Default="-1 3" ShowAsHex="Y">
                exmp = XMLProperties.GetXMLProperty("Path Offset Range for Peds");
                Assert.AreEqual((uint) 0x29dd40c1, exmp.ID);
                Assert.AreEqual("Path Offset Range for Peds", exmp.Name);
                Assert.AreEqual(DBPFPropertyDataType.FLOAT32, exmp.Type);
                Assert.AreEqual(true, exmp.ShowAsHex);
                CollectionAssert.AreEqual(new List<string> { "-1", "3" }, exmp.DefaultValue);
                Assert.AreEqual((short) 2, exmp.Count);

                //<PROPERTY Name="WaveMinTimeInState" ID="0x6932dc06" Type="Float32" Count="4" Default="12 0.230 0.5 2" ShowAsHex="Y">
                exmp = XMLProperties.GetXMLProperty("WaveMinTimeInState");
                Assert.AreEqual((uint) 0x6932dc06, exmp.ID);
                Assert.AreEqual("WaveMinTimeInState", exmp.Name);
                Assert.AreEqual(DBPFPropertyDataType.FLOAT32, exmp.Type);
                Assert.AreEqual(true, exmp.ShowAsHex);
                CollectionAssert.AreEqual(new List<string> { "12", "0.230", "0.5", "2" }, exmp.DefaultValue);
                Assert.AreEqual((short) 4, exmp.Count);
                Assert.AreEqual(null, exmp.Step);

                //<PROPERTY Name="Health Effectiveness vs. Distance Effect" ID="0x891b3ae6" Type="Float32" Count="-2" Default="0 100" MinValue="0" MaxValue="100" ShowAsHex="Y">
                exmp = XMLProperties.GetXMLProperty("Health Effectiveness vs. Distance Effect");
                Assert.AreEqual(0x891b3ae6, exmp.ID);
                Assert.AreEqual("Health Effectiveness vs. Distance Effect", exmp.Name);
                Assert.AreEqual(DBPFPropertyDataType.FLOAT32, exmp.Type);
                Assert.AreEqual(true, exmp.ShowAsHex);
                CollectionAssert.AreEqual(new List<string> { "0", "100" }, exmp.DefaultValue);
                Assert.AreEqual((short) -2, exmp.Count);
                Assert.AreEqual("0", exmp.MinValue);
                Assert.AreEqual("100", exmp.MaxValue);
                Assert.AreEqual(null, exmp.MaxLength);
            }

            [TestMethod]
            public void Test_073_XMLProperties_GetPropertyID() {
                Assert.AreEqual((uint) 0x10, XMLProperties.GetPropertyID("Exemplar Type"));
                Assert.AreEqual((uint) 0x10, XMLProperties.GetPropertyID("ExemplarType"));
                Assert.AreEqual((uint) 0x10, XMLProperties.GetPropertyID("exemplar type"));
                Assert.AreEqual((uint) 0x10, XMLProperties.GetPropertyID("EXEMPLARTYPE"));
                Assert.AreEqual((uint) 0x879d12e7, XMLProperties.GetPropertyID("MaxSlopeAlongNetwork"));
                Assert.AreEqual((uint) 0, XMLProperties.GetPropertyID("property not found"));
            }
        }


        [TestClass]
        public class _08x_DBPFEntry {
            /// <summary>
            /// Test LTEXT entries.
            /// </summary>
            [TestClass]
            public class _080_LTEXT {
                [TestMethod]
                public void Test_081a_LTEXT_Decode() {
                    //Parse from bytes
                    DBPFEntryLTEXT entryb = new DBPFEntryLTEXT(DBPFTGI.LTEXT, 0, 0, 0, TestArrays.notcompressedentry_b);
                    entryb.Decode();
                    Assert.AreEqual("Parks Aura (by Cori)", entryb.Text);

                    //Parse from file
                    DBPFFile dbpf = new DBPFFile("C:\\Users\\Administrator\\Documents\\SimCity 4\\Plugins\\z_DataView - Parks Aura.dat");
                    DBPFEntryLTEXT ltext = (DBPFEntryLTEXT) dbpf.GetEntry(1);
                    ltext.Decode();
                    Assert.AreEqual("Parks Aura (by Cori)", ltext.Text);
                }

                [TestMethod]
                public void Test_081b_LTEXT_ModifyText() {
                    DBPFEntryLTEXT ltext = new DBPFEntryLTEXT();
                    Assert.AreEqual(null, ltext.Text);
                    ltext.Text = "Test String";
                    Assert.AreEqual("Test String", ltext.Text);
                }

                [TestMethod]
                public void Test_081c_LTEXT_Encode() {
                    DBPFEntryLTEXT entryknown = new DBPFEntryLTEXT(DBPFTGI.LTEXT) {
                        Text = "Parks Aura (by Cori)"
                    };
                    entryknown.Encode();
                    CollectionAssert.AreEqual(TestArrays.notcompressedentry_b, entryknown.ByteData);
                }
            }

            /// <summary>
            /// Test Exemplar (EXMP) entries.
            /// </summary>
            [TestClass]
            public class _081_EXMP {
                [TestMethod]
                public void Test_081_EXMP_IsTextEncoding() {
                    DBPFFile dbpf = new DBPFFile("C:\\Users\\Administrator\\Documents\\SimCity 4\\Plugins\\RJ - Block Road Barriers 1.0.dat");
                    List<DBPFEntry> entries = dbpf.GetEntries();

                    Assert.IsFalse(((DBPFEntryEXMP) entries[0]).IsTextEncoding()); //Compressed binary-encoding
                    Assert.IsFalse(((DBPFEntryEXMP) entries[1]).IsTextEncoding()); //Uncompressed binary-encoding

                    DBPFFile dbpf2 = new DBPFFile("C:\\Users\\Administrator\\Desktop\\Jasoncw - Beacon Apartments (DN)\\R$$6_1x2_Beacon Apartments L _7097cb41.SC4Lot");
                    entries = dbpf2.GetEntries();
                    Assert.IsTrue(((DBPFEntryEXMP) entries[0]).IsTextEncoding()); //Uncompressed text-encoding
                }



                [Ignore]
                [TestMethod]
                public void Test_082a_EXMP_Decode() {

                }

                [TestMethod]
                public void Test_082b_EXMP_Encode() {
                    //Binary Encoding
                    DBPFFile dbpf = new DBPFFile("C:\\Users\\Administrator\\Documents\\SimCity 4\\Plugins\\z_DataView - Parks Aura.dat");
                    DBPFEntry entry = dbpf.GetEntry(0);
                    entry.Decode();
                    entry.Encode();
                    Assert.AreEqual((uint) 446, entry.UncompressedSize);
                    CollectionAssert.AreEqual(TestArrays.decompressedentry_b, entry.ByteData);
                    //Assert.IsTrue(entry.ShouldBeCompressed);

                    //Text Encoding
                    dbpf = new DBPFFile("C:\\Users\\Administrator\\OneDrive\\FINAL\\nos.17\\B62-Albertsons 60's Retro v2.0\\b62-albertsons_60s v 1.1-0x6534284a-0xd3a3e650-0xd4ebfbfa.SC4Desc");
                    entry = dbpf.GetEntry(0);
                    entry.Decode();
                    entry.Encode();
                    string expected = "EQZT1###..ParentCohort=Key:{0x00000000,0x00000000,0x00000000}..PropCount=0x00000018..0x00000010:{\"Exemplar Type\"}=Uint32:0:{0x00000002}..0x00000020:{\"Exemplar Name\"}=String:1:{\"B62-CS$_Albertsons_60s_Grocery v 1.1\"}..0x099afacd:{\"Bulldoze Cost\"}=Sint64:0:{0x00000000000000a9}..0x27812810:{\"Occupant Size\"}=Float32:3:{81.58979797,13.94729996,39.44250107}..0x27812811:{\"Unknown\"}=Float32:1:{0.5}..0x27812821:{\"ResourceKeyType1\"}=Uint32:3:{0x5ad0e817,0xb2d6debe,0x00030000}..0x27812832:{\"Wealth\"}=Uint8:0:{0x01}..0x27812833:{\"Purpose\"}=Uint8:0:{0x02}..0x27812834:{\"Capacity Satisfied\"}=Uint32:2:{0x00003110,0x000002ef}..0x27812851:{\"Pollution at centre\"}=Sint32:4:{0x00000007,0x00000003,0x00000016,0x00000000}..0x27812854:{\"Power Consumed\"}=Uint32:0:{0x00000010}..0x29244db5:{\"Flammability\"}=Uint8:0:{0x2d}..0x2a499f85:{\"Query exemplar GUID\"}=Uint32:0:{0xca56783a}..0x2c8f8746:{\"Exemplar Category\"}=Uint32:0:{0x8c8fbbcc}..0x499afa38:{\"Construction Time\"}=Uint8:0:{0x10}..0x49beda31:{\"MaxFireStage\"}=Uint8:0:{0x04}..0x68ee9764:{\"Pollution Radius\"}=Float32:4:{5,5,0,0}..0x8a1c3e72:{\"Worth\"}=Sint64:0:{0x00000000000000a9}..0x8cb3511f:{\"Occupant Types\"}=Uint32:1:{0x00003110}..0xaa1dd396:{\"OccupantGroups\"}=Uint32:4:{0x00001001,0x00002000,0x00002001,0x00013110}..0xaa1dd397:{\"SFX:Query Sound\"}=Uint32:0:{0x2a8916ab}..0xaa83558f:{\"Crane Hints\"}=Uint8:0:{0x00}..0xc8ed2d84:{\"Water Consumed\"}=Uint32:0:{0x00000098}..0xe91a0b5f:{\"Building value\"}=Sint64:0:{0x0000000000001678}..r";
                    string actual = ByteArrayHelper.ToAString(entry.ByteData);
                    Assert.AreEqual(expected[..85], actual[..85]); //Entry header
                    Assert.AreEqual(expected[85..137], actual[85..137]); //Property 1: Exemplar Type
                    Assert.AreEqual(expected[137..217], actual[137..217]); //Property 2: Exemplar Name
                    Assert.AreEqual(expected[217..277], actual[217..277]); //Property 3: Bulldoze Cost
                    Assert.AreEqual(expected[277..355], actual[277..355]); //Property 4: Occupant Size
                    Assert.AreEqual(expected[355..395], actual[355..395]); //Property 5: Unknown
                    Assert.AreEqual(expected[395..472], actual[395..472]); //Property 6: ResourceKeyType1
                    Assert.AreEqual(expected[472..510], actual[472..510]); //Property 7: Wealth
                    Assert.AreEqual(expected[510..549], actual[510..549]); //Property 8: Purpose
                    Assert.AreEqual(expected[549..617], actual[549..617]); //Property 9: Capacity Satisfied
                    Assert.AreEqual(expected[617..708], actual[617..708]); //Property 10: Pollution at center
                    Assert.AreEqual(expected[708..761], actual[708..761]); //Property 11: Power Consumed
                    Assert.AreEqual(expected[761..805], actual[761..805]); //Property 12: Flammability
                    Assert.AreEqual(expected[805..863], actual[805..863]); //Property 13: Query exemplar GUID
                    Assert.AreEqual(expected[863..919], actual[863..919]); //Property 14: Exemplar Category
                    Assert.AreEqual(expected[919..968], actual[919..968]); //Property 15: Construction Time
                    Assert.AreEqual(expected[968..1012], actual[968..1012]); //Property 16: MaxFireStage
                    Assert.AreEqual(expected[1012..1065], actual[1012..1065]); //Property 17: Pollution Radius
                    Assert.AreEqual(expected[1065..1117], actual[1065..1117]); //Property 18: Worth
                    Assert.AreEqual(expected[1117..1170], actual[1117..1170]); //Property 19: Occupant Types
                    Assert.AreEqual(expected[1170..1256], actual[1170..1256]); //Property 20: OccupantGroups
                    Assert.AreEqual(expected[1256..1310], actual[1256..1310]); //Property 21: SFX:Query Sound
                    Assert.AreEqual(expected[1310..1353], actual[1310..1353]); //Property 22: Crane Hints
                    Assert.AreEqual(expected[1353..1406], actual[1353..1406]); //Property 23: Water Consumed
                    Assert.AreEqual(expected[1406..1467], actual[1406..1467]); //Property 24: Building Value

                    Assert.AreEqual((uint) 0x5BB, entry.CompressedSize);
                    //Assert.IsFalse(entry.ShouldBeCompressed);
                }
            }

            /// <summary>
            /// Test Directory (DIR) entries.
            /// </summary>
            [TestClass]
            public class _082_DIR {
                [TestMethod]
                public void Test_083a_DIR_Decode() {
                    //sample from: z_DataView - Parks Aura.dat
                    byte[] dirbytes = new byte[] { 0x4A, 0x28, 0x34, 0x65, 0x3F, 0x69, 0x0F, 0x69, 0x19, 0x68, 0x0B, 0x4A, 0xBE, 0x01, 0x00, 0x00 };

                    //Parse from bytes
                    DBPFEntryDIR entryd = new DBPFEntryDIR(0, 0, 0, dirbytes);
                    entryd.Decode();
                    Assert.AreEqual(1, entryd.CompressedItems.Count);
                    Assert.AreEqual((uint) 0x6534284A, entryd.CompressedItems[0].TID);
                    Assert.AreEqual((uint) 0x690F693F, entryd.CompressedItems[0].GID);
                    Assert.AreEqual((uint) 0x4A0B6819, entryd.CompressedItems[0].IID);
                    Assert.AreEqual((uint) 446, entryd.CompressedItems[0].Size);

                    //Parse from file
                    DBPFFile dbpf = new DBPFFile("C:\\Users\\Administrator\\Documents\\SimCity 4\\Plugins\\z_DataView - Parks Aura.dat");
                    DBPFEntryDIR dir = (DBPFEntryDIR) dbpf.GetEntry(12);
                    dir.Decode();
                    Assert.AreEqual(1, dir.CompressedItems.Count);
                    Assert.AreEqual((uint) 0x6534284A, dir.CompressedItems[0].TID);
                    Assert.AreEqual((uint) 0x690F693F, dir.CompressedItems[0].GID);
                    Assert.AreEqual((uint) 0x4A0B6819, dir.CompressedItems[0].IID);
                    Assert.AreEqual((uint) 446, dir.CompressedItems[0].Size);
                }

                [TestMethod]
                public void Test_083b_DIR_Build() {
                    //File with single DIR entry
                    DBPFFile dbpf = new DBPFFile("C:\\Users\\Administrator\\Documents\\SimCity 4\\Plugins\\z_DataView - Parks Aura.dat");
                    DBPFEntryDIR dir = (DBPFEntryDIR) dbpf.GetEntry(12);
                    dir.Decode();
                    dbpf.RebuildDirectory();
                    Assert.AreEqual(1, dir.CompressedItems.Count);
                    Assert.AreEqual((uint) 0x6534284A, dir.CompressedItems[0].TID);
                    Assert.AreEqual((uint) 0x690F693F, dir.CompressedItems[0].GID);
                    Assert.AreEqual((uint) 0x4A0B6819, dir.CompressedItems[0].IID);
                    Assert.AreEqual((uint) 446, dir.CompressedItems[0].Size);

                    //File with multiple DIR entries
                    DBPFFile dbpf2 = new DBPFFile("C:\\Users\\Administrator\\Documents\\SimCity 4\\Plugins\\z_DataviewModd_RH.dat");
                    DBPFEntryDIR dir2 = (DBPFEntryDIR) dbpf2.GetEntry(13);
                    dir2.Decode();
                    dbpf2.RebuildDirectory();
                    Assert.AreEqual(7, dir2.CompressedItems.Count);
                    Assert.AreEqual((uint) 0x6534284A, dir2.CompressedItems[0].TID);
                    Assert.AreEqual((uint) 0x690F693F, dir2.CompressedItems[0].GID);
                    Assert.AreEqual((uint) 0x4A0B684C, dir2.CompressedItems[0].IID);
                    Assert.AreEqual((uint) 396, dir2.CompressedItems[0].Size);
                    Assert.AreEqual((uint) 0x6534284A, dir2.CompressedItems[6].TID);
                    Assert.AreEqual((uint) 0x690F693F, dir2.CompressedItems[6].GID);
                    Assert.AreEqual((uint) 0x4A0B68F2, dir2.CompressedItems[6].IID);
                    Assert.AreEqual((uint) 464, dir2.CompressedItems[6].Size);

                }

                [TestMethod]
                public void Test_083c_DIR_Encode() {
                    //File with single DIR entry
                    byte[] target = { 0x4A, 0x28, 0x34, 0x65, 0x3F, 0x69, 0x0F, 0x69, 0x19, 0x68, 0x0B, 0x4A, 0xBE, 0x01, 0x00, 0x00 };
                    DBPFFile dbpf = new DBPFFile("C:\\Users\\Administrator\\Documents\\SimCity 4\\Plugins\\z_DataView - Parks Aura.dat");
                    DBPFEntryDIR dir = (DBPFEntryDIR) dbpf.GetEntry(12);
                    dir.Decode();
                    CollectionAssert.AreEqual(target, dir.ByteData);
                    dir.Encode();
                    CollectionAssert.AreEqual(target, dir.ByteData);

                    //File with multiple DIR entries
                    byte[] target2 = { 0x4A, 0x28, 0x34, 0x65, 0x3F, 0x69, 0x0F, 0x69, 0x4C, 0x68, 0x0B, 0x4A, 0x8C, 0x01, 0x00, 0x00, 0x4A, 0x28, 0x34, 0x65, 0x3F, 0x69, 0x0F, 0x69, 0x4E, 0x68, 0x0B, 0x4A, 0x83, 0x01, 0x00, 0x00, 0x4A, 0x28, 0x34, 0x65, 0x3F, 0x69, 0x0F, 0x69, 0x4F, 0x68, 0x0B, 0x4A, 0x83, 0x01, 0x00, 0x00, 0x4A, 0x28, 0x34, 0x65, 0x3F, 0x69, 0x0F, 0x69, 0x51, 0x68, 0x0B, 0x4A, 0x8F, 0x01, 0x00, 0x00, 0x4A, 0x28, 0x34, 0x65, 0x3F, 0x69, 0x0F, 0x69, 0x53, 0x68, 0x0B, 0x4A, 0x8D, 0x01, 0x00, 0x00, 0x4A, 0x28, 0x34, 0x65, 0x3F, 0x69, 0x0F, 0x69, 0x55, 0x68, 0x0B, 0x4A, 0x88, 0x01, 0x00, 0x00, 0x4A, 0x28, 0x34, 0x65, 0x3F, 0x69, 0x0F, 0x69, 0xF2, 0x68, 0x0B, 0x4A, 0xD0, 0x01, 0x00, 0x00 };
                    DBPFFile dbpf2 = new DBPFFile("C:\\Users\\Administrator\\Documents\\SimCity 4\\Plugins\\z_DataviewModd_RH.dat");
                    DBPFEntryDIR dir2 = (DBPFEntryDIR) dbpf2.GetEntry(13);
                    dir2.Decode();
                    CollectionAssert.AreEqual(target2, dir2.ByteData);
                    dir2.Encode();
                    CollectionAssert.AreEqual(target2, dir2.ByteData);
                }
            }

            /// <summary>
            /// Test PNG entries.
            /// </summary>
            [TestClass]
            public class _083_PNG {
                [TestMethod]
                public void Test_084a_PNG_Decode() {
                    DBPFFile dbpf = new DBPFFile("C:\\source\\repos\\csDBPF\\csDBPF\\csDBPF_Test\\Test Files\\Jigsaw 2010 tilesets.dat");
                    Assert.AreEqual(8, dbpf.CountEntries());

                    DBPFEntryPNG pngentry1 = (DBPFEntryPNG) dbpf.GetEntry(4);
                    pngentry1.Decode();
                    Image png1 = Image.Load("C:\\source\\repos\\csDBPF\\csDBPF\\csDBPF_Test\\Test Files\\Jigsaw 2010 tilesets 1.png");
                    Assert.AreEqual(png1.PixelType.BitsPerPixel, pngentry1.PNGImage.PixelType.BitsPerPixel);
                    Assert.AreEqual(png1.Size.Height, pngentry1.PNGImage.Size.Height);
                    Assert.AreEqual(png1.Size.Width, pngentry1.PNGImage.Size.Width);

                    string expectedURI = png1.ToBase64String(png1.Metadata.DecodedImageFormat);
                    string actualURI = pngentry1.PNGImage.ToBase64String(pngentry1.PNGImage.Metadata.DecodedImageFormat);
                    Assert.AreEqual(expectedURI, actualURI);
                }

                [TestMethod]
                public void Test_084b_PNG_SaveImage() {
                    DBPFFile dbpf = new DBPFFile("C:\\source\\repos\\csDBPF\\csDBPF\\csDBPF_Test\\Test Files\\Jigsaw 2010 tilesets.dat");
                    DBPFEntryPNG pngentry = (DBPFEntryPNG) dbpf.GetEntry(4);
                    pngentry.Decode();

                    string path = "C:\\source\\repos\\csDBPF\\csDBPF\\csDBPF_Test\\Test Files\\testsave.png";
                    pngentry.PNGImage.SaveAsPng(path);
                    Assert.AreEqual(true,File.Exists(path));
                    File.Delete(path);
                }

                [TestMethod]
                public void Test_084c_PNG_SetImage() {
                    DBPFFile dbpf = new DBPFFile("C:\\source\\repos\\csDBPF\\csDBPF\\csDBPF_Test\\Test Files\\Jigsaw 2010 tilesets.dat");
                    DBPFEntryPNG pngentry = (DBPFEntryPNG) dbpf.GetEntry(4);
                    pngentry.Decode();

                    string path2 = "C:\\source\\repos\\csDBPF\\csDBPF\\csDBPF_Test\\Test Files\\Jigsaw 2010 tilesets 2.png";
                    string path3 = "C:\\source\\repos\\csDBPF\\csDBPF\\csDBPF_Test\\Test Files\\Jigsaw 2010 tilesets 3.png";
                    
                    Image target = Image.Load(path2);
                    Assert.AreNotEqual(target.ToBase64String(target.Metadata.DecodedImageFormat), pngentry.PNGImage.ToBase64String(pngentry.PNGImage.Metadata.DecodedImageFormat));
                    pngentry.SetImage(target);
                    Assert.AreEqual(target.ToBase64String(target.Metadata.DecodedImageFormat), pngentry.PNGImage.ToBase64String(pngentry.PNGImage.Metadata.DecodedImageFormat));

                    target = Image.Load(path3);
                    Assert.AreNotEqual(target.ToBase64String(target.Metadata.DecodedImageFormat), pngentry.PNGImage.ToBase64String(pngentry.PNGImage.Metadata.DecodedImageFormat));
                    pngentry.SetImage(path3);
                    Assert.AreEqual(target.ToBase64String(target.Metadata.DecodedImageFormat), pngentry.PNGImage.ToBase64String(pngentry.PNGImage.Metadata.DecodedImageFormat));
                }

                [TestMethod]
                public void Test_084d_PNG_Encode() {
                    DBPFFile dbpf = new DBPFFile("C:\\source\\repos\\csDBPF\\csDBPF\\csDBPF_Test\\Test Files\\Jigsaw 2010 tilesets.dat");
                    DBPFEntryPNG pngentry = (DBPFEntryPNG) dbpf.GetEntry(4);
                    pngentry.Decode();

                    Image targetimg = Image.Load("C:\\source\\repos\\csDBPF\\csDBPF\\csDBPF_Test\\Test Files\\Jigsaw 2010 tilesets 1.png");
                    DBPFEntryPNG target = new DBPFEntryPNG(DBPFTGI.PNG);
                    target.SetImage(targetimg);
                    target.Encode();
                    pngentry.Encode();
                    CollectionAssert.AreEqual(target.ByteData, pngentry.ByteData);
                }
            }

            /// <summary>
            /// Test FSH entries.
            /// </summary>
            [TestClass]
            public class _084_FSH {
                [TestMethod]
                public void Test_084a_FSH_Decode() {
                    //DXT1
                    DBPFFile file1 = new DBPFFile("C:\\source\\repos\\csDBPF\\csDBPF\\csDBPF_Test\\Test Files\\b62- blue cart corral v.1.0-0x5ad0e817_0xf00f7c4_0x2580000.SC4Model");
                    DBPFEntryFSH fshentry1 = (DBPFEntryFSH) file1.GetEntry(1);
                    fshentry1.Decode();

                    //DXT3
                    DBPFFile file2 = new DBPFFile("C:\\source\\repos\\csDBPF\\csDBPF\\csDBPF_Test\\Test Files\\ALN_Dirt_Path_Textures.dat");
                    DBPFEntryFSH fshentry2 = (DBPFEntryFSH) file2.GetEntry(4);
                    fshentry2.Decode();
                }


            }
        }



        /// <summary>
        /// 1xx Test Methods for DBPFFile Class
        /// </summary>
        [TestClass]
        public class _1xx_DBPFFile {

            [TestMethod]
            public void Test_100_DBPFFile_IssueLog() {
                DBPFFile testdbpf = new DBPFFile("C:\\source\\repos\\csDBPF\\csDBPF\\csDBPF_Test\\Test Files\\Logging Test File.dat");
                testdbpf.DecodeAllEntries();
                string issue = testdbpf.GetIssueLog();
                string[] issues = issue.Split("\r\n");

                Assert.AreEqual("Logging Test File.dat,0x6534284B,0xA8FBD372,0x7097CB41,NULL,NULLTGI,Unknown TGI identifier.", issues[0]);
                Assert.AreEqual("Logging Test File.dat,0x6534284A,0x7B647F18,0xD097CA4F,EXMP,EXEMPLAR,Property 0x8CB3511F is duplicated.", issues[1]);
                Assert.AreEqual("Logging Test File.dat,0x6534284A,0x7B647F18,0xD097CA50,EXMP,EXEMPLAR,Missing property Exemplar Name.", issues[2]);
                Assert.AreEqual("Logging Test File.dat,0x6534284A,0x7B647F18,0xD097CA51,EXMP,EXEMPLAR,Missing property Exemplar Type.", issues[3]);
                Assert.AreEqual("Logging Test File.dat,0x6534284A,0x7B647F18,0xD097CA52,EXMP,EXEMPLAR,Entry contains 0 properties.", issues[4]);
                Assert.AreEqual("Logging Test File.dat,0x6534284A,0x7B647F18,0xD097CA53,EXMP,EXEMPLAR,Property 0x27812811 contains a potential macOS TE bug.", issues[5]);
            }

            [TestMethod]
            public void Test_101a_DBPFFile_IsDBPF() {
                DBPFFile dbpf = new DBPFFile("C:\\Users\\Administrator\\Documents\\SimCity 4\\Plugins\\z_DataView - Parks Aura.dat");
                Assert.AreEqual("DBPF", dbpf.Header.Identifier);
                Assert.AreEqual((uint) 1, dbpf.Header.MajorVersion);
                Assert.AreEqual((uint) 0, dbpf.Header.MinorVersion);
                Assert.AreEqual((uint) 7, dbpf.Header.IndexMajorVersion);
            }

            [TestMethod]
            public void Test_101b_DBPFFile_IsNotDBPF() {
                DBPFFile notdbpf = new DBPFFile("C:\\Program Files (x86)\\Steam\\steamapps\\common\\SimCity 4 Deluxe\\Plugins\\CAS_AutoHistorical_v0.0.2.dll");
                Assert.AreEqual("CAS_AutoHistorical_v0.0.2.dll: 0 subfiles", notdbpf.ToString());
                Assert.AreEqual(0, notdbpf.GetEntries().Count);
            }

            [TestMethod]
            public void Test_102_DBPFFile_CreateBlank() {

            }

            [TestMethod]
            public void Test_110_DBPFFile_GetEntry() {
                DBPFFile dbpf = new DBPFFile("C:\\Users\\Administrator\\Documents\\SimCity 4\\Plugins\\z_DataView - Parks Aura.dat");
                DBPFEntry entry, entry0, entry1;

                //Get by index position
                entry0 = dbpf.GetEntry(0);
                Assert.IsTrue(entry0.TGI.Matches(DBPFTGI.EXEMPLAR));
                CollectionAssert.AreEqual(dbpf.GetEntry(0).ByteData, entry0.ByteData);
                entry1 = dbpf.GetEntry(3);

                Assert.IsTrue(entry1.TGI.Matches(DBPFTGI.LTEXT));
                CollectionAssert.AreEqual(dbpf.GetEntry(3).ByteData, entry1.ByteData);

                //Get by Instance ID
                entry = dbpf.GetEntry((uint) 0x4A0B6819);
                Assert.IsTrue(entry.TGI.Matches(DBPFTGI.EXEMPLAR));
                Assert.AreEqual(entry.TGI, entry.TGI);
                CollectionAssert.AreEqual(dbpf.GetEntry(0).ByteData, entry.ByteData);
                CollectionAssert.AreEqual(entry.ByteData, entry.ByteData);

                entry = dbpf.GetEntry(0xF65435A1);
                Assert.IsTrue(entry.TGI.Matches(DBPFTGI.LTEXT));
                CollectionAssert.AreEqual(dbpf.GetEntry(3).ByteData, entry.ByteData);
                CollectionAssert.AreEqual(entry1.ByteData, entry.ByteData);

                //Get by TGI
                entry = dbpf.GetEntry(DBPFTGI.DIRECTORY);
                Assert.IsTrue(entry.TGI.Matches(DBPFTGI.DIRECTORY));
                CollectionAssert.AreEqual(dbpf.GetEntry(12).ByteData, entry.ByteData);

                entry = dbpf.GetEntry(DBPFTGI.LTEXT);
                Assert.IsTrue(entry.TGI.Matches(DBPFTGI.LTEXT));
                CollectionAssert.AreEqual(dbpf.GetEntry(1).ByteData, entry.ByteData);

                entry = dbpf.GetEntry(new TGI(0x2026960B, 0x6A231EAA, 0xF65435A1));
                Assert.IsTrue(entry.TGI.Matches(DBPFTGI.LTEXT));
                CollectionAssert.AreEqual(entry1.ByteData, entry.ByteData);
            }

            [TestMethod]
            public void Test_111_ParseAllEntries() {
                DBPFEntryEXMP entryKnown = new DBPFEntryEXMP(DBPFTGI.EXEMPLAR, 0, 0, 0, TestArrays.decompressedentry_b);

                DBPFFile dbpf = new DBPFFile("C:\\Users\\Administrator\\Documents\\SimCity 4\\Plugins\\z_DataView - Parks Aura.dat");
                DBPFEntryEXMP entry0 = (DBPFEntryEXMP) dbpf.GetEntry(0);
                entry0.Decode();
                for (int idx = 0; idx < entryKnown.ListOfProperties.Count; idx++) {
                    DBPFProperty outk = entryKnown.ListOfProperties.GetValueAtIndex(idx);
                    DBPFProperty outr = entry0.ListOfProperties.GetValueAtIndex(idx);
                    Assert.AreEqual(outk.ID, outr.ID);
                    Assert.AreEqual(outk.NumberOfReps, outr.NumberOfReps);
                    Assert.AreEqual(outk.DataType, outr.DataType);
                    CollectionAssert.AreEqual((System.Collections.ICollection) outk.GetData(), (System.Collections.ICollection) outr.GetData());
                }

                DBPFEntryLTEXT entry1 = (DBPFEntryLTEXT) dbpf.GetEntry(1);
                entry1.Decode();
                Assert.AreEqual("Parks Aura (by Cori)", entry1.Text);

                DBPFEntryLTEXT entry11 = (DBPFEntryLTEXT) dbpf.GetEntry(dbpf.CountEntries() - 2);
                entry11.Decode();
                Assert.AreEqual("+100  to +165", entry11.Text);
            }

            [Ignore]
            [TestMethod]
            public void Test_112_ParseBuildingExemplar() {
                DBPFFile dbpf = new DBPFFile("C:\\Users\\Administrator\\OneDrive\\SC4 MODPACC\\B62\\B62-Albertsons 60's Retro v2.0\\b62-albertsons_60s v 1.1-0x6534284a-0xd3a3e650-0xd4ebfbfa.SC4Desc");
                List<DBPFEntry> entries = dbpf.GetEntries();
                DBPFEntry entry = entries[0];
                entry.Decode();

                //uint[] val = (uint[]) properties[0].DecodeValues();
                //Assert.AreEqual(DBPFProperty.ExemplarTypes.Building, val[0]);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

namespace csDBPF.Entries {
    /// <summary>
    /// An implementation of <see cref="DBPFEntry"/> for PNG entries. Object data is stored in <see cref="PNGImage"/>.
    /// </summary>
    /// <see href="https://www.wiki.sc4devotion.com/index.php?title=PNG"/>
    public class DBPFEntryPNG : DBPFEntry {
        private Image _image;
        /// <summary>
        /// PNG image in this entry.
        /// </summary>
        public Image PNGImage {
            get { return _image; }
        }


        /// <summary>
		/// Create a new instance. Use when creating a new exemplar.
		/// </summary>
		/// <param name="tgi">TGI set to assign</param>
        public DBPFEntryPNG(TGI tgi) : base(tgi) {
        }

        /// <summary>
		/// Create a new instance. Use when reading an existing image from a file.
		/// </summary>
		/// <param name="tgi">TGI object representing the entry</param>
		/// <param name="offset">Offset (location) of the entry within the DBPF file</param>
		/// <param name="size">Compressed size of data for the entry, in bytes. Uncompressed size is also temporarily set to this to this until the data is set</param>
		/// <param name="index">Entry position in the file, 0-n</param>
		/// <param name="bytes">Byte data for this entry</param>
        public DBPFEntryPNG(TGI tgi, uint offset, uint size, uint index, byte[] bytes) : base(tgi, offset, size, index, bytes) {

        }


        /// <summary>
		/// Sets the <see cref="PNGImage"/> property from this entry's byte sequence.
		/// </summary>
		/// <remarks>
		/// Use when reading from a file.
		/// </remarks>
        public override void DecodeEntry() {
            _image = Image.Load(ByteData);
        }

        /// <summary>
        /// Build <see cref="DBPFEntry.ByteData"/> from the current state of <see cref="PNGImage"/>.
        /// </summary>
        public override void ToBytes() {
            using MemoryStream ms = new MemoryStream();

            _image.Save(ms, new PngEncoder());
            _image.Dispose();
            ByteData = ms.ToArray();
        }


        /// <summary>
        /// Saves this image to a file.
        /// </summary>
        /// <param name="path">File path to save to</param>
        public void SaveImage(string path) {
            _image.SaveAsPng(path);
        }

        /// <summary>
        /// Set this entry's image to an image loaded from a file.
        /// </summary>
        /// <param name="path">File path to load from</param>
        public void SetImage(string path) {
            _image = Image.Load(new FileStream(path, FileMode.Open));
        }
        /// <summary>
        /// Set this entry's image to an existing Image.
        /// </summary>
        /// <param name="img">Image to use</param>
        public void SetImage(Image img) {
            _image = img;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

//See: https://github.com/memo33/jDBPFX/blob/master/src/jdbpfx/DBPFTGI.java
namespace csDBPF {

    /// <summary>
    /// A struct representing three unsigned integers as a Type, Group, Instance triplet.
    /// </summary>
    public struct TGI : IComparable<TGI> {
        /// <summary>
        /// Returns the <see href="https://www.wiki.sc4devotion.com/index.php?title=Type_ID">Type ID</see> (TID).
        /// </summary>
        public uint TypeID  { get; private set; }

        /// <summary>
        /// Returns the <see href="https://www.wiki.sc4devotion.com/index.php?title=Group_ID">Group ID</see> (GID).
        /// </summary>
        public uint GroupID { get; private set; }

        /// <summary>
        /// Returns the <see href="https://www.wiki.sc4devotion.com/index.php?title=Instance_ID">Instance ID</see> (IID).
        /// </summary>
        public uint InstanceID { get; private set; }

        /// <summary>
        /// Internal constructor used to instantiate the static TGI templates, and is not exposed publicly as it allows null arguments. Null arguments are set to zero.
        /// </summary>
        internal TGI(uint? t, uint? g, uint? i) {
            if (t.HasValue) {
                TypeID = (uint) t;
            } else {
                TypeID = 0;
            }
            if (g.HasValue) {
                GroupID = (uint) g;
            } else {
                GroupID = 0;
            }
            if (i.HasValue) {
                InstanceID = (uint) i;
            } else {
                InstanceID = 0;
            }
        }
        /// <summary>
        /// Create a new TGI from the specified uints. A random value will be assigned for the Group or Instance if either is zero.
        /// </summary>
        /// <param name="t">Type ID</param>
        /// <param name="g">Group ID</param>
        /// <param name="i">Instance ID</param>
        public TGI(uint t, uint g, uint i) {
            TypeID = t;
            if (g == 0) {
                GroupID = DBPFUtil.GenerateRandomUint();
            } else {
                GroupID = g;
            }
            if (i == 0) {
                InstanceID = DBPFUtil.GenerateRandomUint();
            } else {
                InstanceID = i;
            }
        }
        /// <summary>
        /// Create a new TGI based on a TGI template. Assigns a random Group or Instance as appropriate based on the template.
        /// </summary>
        /// <param name="template">TGI template. Should be one of the static types in <see cref="DBPFTGI"/> class.</param>
        public TGI(TGI template) {
            TypeID = template.TypeID;
            if (template.GroupID == 0) {
                GroupID = DBPFUtil.GenerateRandomUint();
            } else {
                GroupID = template.GroupID;
            }
            if (template.InstanceID == 0) {
                InstanceID = DBPFUtil.GenerateRandomUint();
            } else {
                InstanceID = template.InstanceID;
            }
        }
        /// <summary>
        /// Create a new TGI based on an existing string representation.
        /// </summary>
        /// <remarks>The input string must satisfy the conditions of <see cref="DBPFUtil.FormatTgiString(string)"/> to be valid.</remarks>
        /// <param name="tgi">A string representing the TGI (Type, Group, Instance) in a valid format.</param>
        public TGI(string tgi) {
            string cleaned = DBPFUtil.FormatTgiString(tgi);
            TypeID = uint.Parse(cleaned.Substring(2, 8), NumberStyles.HexNumber);
            GroupID = uint.Parse(cleaned.Substring(14, 8), NumberStyles.HexNumber);
            InstanceID = uint.Parse(cleaned.Substring(26, 8), NumberStyles.HexNumber);
        }


        /// <inheritdoc/>
        public readonly int CompareTo(TGI other) {
            //Check if the T difers first, then if the G differs, then if the I differs
            var typediff = TypeID - other.TypeID;
            if (typediff != 0)
                return (int) typediff;

            var groupdiff = this.GroupID - other.GroupID;
            if (groupdiff != 0) 
                return (int) groupdiff;

            return (int) (InstanceID - other.InstanceID);
        }


        /// <summary>
        /// Evaluate whether two TGIs are equal. TGIs are equal if their Types, Groups, and Instances are identical.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns><see langword="true"/> if the TGIs are identical; <see langword="false"/> otherwise</returns>
        public override readonly bool Equals(object obj) {
            if (obj is not TGI tgi) return false;
            return TypeID == tgi.TypeID && GroupID == tgi.GroupID && InstanceID == tgi.InstanceID;
        }
        /// <inheritdoc/>
        public static bool operator ==(TGI left, TGI right) { return left.Equals(right); }
        /// <inheritdoc/>
        public static bool operator !=(TGI left, TGI right) { return !(left == right); }
        /// <inheritdoc/>
        public override readonly int GetHashCode() {
            return TypeID.GetHashCode() ^ GroupID.GetHashCode() ^ InstanceID.GetHashCode();
        }


        /// <summary>
        /// Determines whether the current TGI matches the specified <paramref name="otherTGI"/> based on its TypeID, GroupID, and InstanceID.
        /// </summary>
        /// <remarks>If  A match occurs if each non-zero field in <paramref name="otherTGI"/> equals the corresponding field in the current TGI. If <paramref name="otherTGI"/> is one of the static templates in <see cref="DBPFTGI"/>, then fields with a value of 0 are treated as wildcards and will always match.</remarks>
        /// <param name="otherTGI">The <see cref="TGI"/> instance to compare against. A value of 0 for any of its fields (TypeID, GroupID, or
        /// InstanceID) acts as a wildcard, meaning that field will always match.</param>
        /// <returns><see langword="true"/> if the current TGI matches the specified TGI; otherwise, <see langword="false"/>.</returns>
        public readonly bool Matches(TGI otherTGI) {
            bool evalT, evalG, evalI;
            if (otherTGI.TypeID == 0) {
                evalT = true;
            } else {
                evalT = TypeID == otherTGI.TypeID;
            }
            if (otherTGI.GroupID == 0) {
                evalG = true;
            } else {
                evalG = GroupID == otherTGI.GroupID;
            }
            if (otherTGI.InstanceID == 0) {
                evalI = true;
            } else {
                evalI = InstanceID == otherTGI.InstanceID;
            }
            return evalT && evalG && evalI;
        }

        /// <summary>
        /// Determines whether the current instance matches any known TGI template.
        /// </summary>
        /// <remarks>This method iterates through all known TGI templates and checks for a match using the <see cref="Matches"/> method. If a match is found, the corresponding TGI entry is returned. If no matches are found, a blank TGI entry is returned.</remarks>
        /// <returns>The first matching TGI entry if a match is found; otherwise, <see cref="DBPFTGI.BLANKTGI"/> is returned.</returns>
        public readonly TGI MatchesAnyKnown() {
            foreach (TGI tgi in DBPFTGI.KnownEntries.Keys) {
                if (Matches(tgi)) {
                    return tgi;
                }
            }
            return DBPFTGI.BLANKTGI;
        }



        /// <summary>
        /// Returns the general file type this TGI represents (EXMP, FSH, PNG, etc.).
        /// </summary>
        /// <returns>A string representing the entry type if a match is found; otherwise, an empty string.</returns>
        public readonly string GetEntryType() {
            foreach (TGI tgi in DBPFTGI.KnownEntries.Keys) {
                if (Matches(tgi)) {
                    return DBPFTGI.KnownEntries.GetValueOrDefault(tgi).EntryType;
                }
            }
            return string.Empty;
        }
        /// <summary>
        /// Returns the specific file type detail associated with this TGI, as it relates to an in-game implementation (EXEMPLAR_ROAD, FSH_BASE_OVERLAY, PNG_ICON, etc.).
        /// </summary>
        /// <returns>A string representing the file type detail if a match is found; otherwise, an empty string.</returns>
        public readonly string GetEntryDetail() {
            foreach (TGI tgi in DBPFTGI.KnownEntries.Keys) {
                if (Matches(tgi)) {
                    return DBPFTGI.KnownEntries.GetValueOrDefault(tgi).EntryDetail;
                }
            }
            return string.Empty;
        }



        /// <summary>
        /// Returns a comma separated list of the TypeID, GroupID, and InstanceID in hexadecimal format.
        /// </summary>
        /// <returns>A string in the format <c>0x########, 0x########, 0x########</c></returns>
        public override readonly string ToString() {
            string t = TypeID == 0 ? "#" : DBPFUtil.ToHexString(TypeID, prefix: true);
            string g = GroupID == 0 ? "#" : DBPFUtil.ToHexString(GroupID, prefix: true);
            string i = InstanceID == 0 ? "#" : DBPFUtil.ToHexString(InstanceID, prefix: true);
            return $"{t}, {g}, {i}";
        }



        /// <summary>
        /// Assign a random Group ID to this TGI.
        /// </summary>
        public void RandomizeGroup() {
            GroupID = DBPFUtil.GenerateRandomUint();
        }
        /// <summary>
        /// Assign a random Instance ID to this TGI.
        /// </summary>
        public void RandomizeInstance() {
            InstanceID = DBPFUtil.GenerateRandomUint();
        }
    }



    /// <summary>
    /// Provides a collection of predefined TGI constants ("templates") representing the known SC4 entry types. 
    /// </summary>
    /// <remarks>The TGI templates may be used in combination with the methods provided by the <see cref="TGI"/> struct to instantiate a new TGI of a desired type, or to identify the nearest known entry type of an arbitrary TGI.</remarks>
    public static class DBPFTGI {
		//In general Dictionary items are kept in the order they are added> However, since we're not doing a lot of adding or *any* deleting/sorting, its not as big of a deal meaning we don't need to use a special type like SortedDictionary
		internal static readonly Dictionary<TGI, TGIDetails> KnownEntries = [];
        internal struct TGIDetails(string cat, string det) {
            /// <summary>
            /// The general file type this TGI represents.
            /// </summary>
            public string EntryType = cat;
            /// <summary>
            /// The detailed description of the file type this TGI represents.
            /// </summary>
            public string EntryDetail = det;
        }
        #region KnownTGIs
        /// <summary>Directory file (0xe86b1eef, 0xe86b1eef, 0x286b1f03)</summary>
        public static readonly TGI DIRECTORY;
        /// <summary>LD file (0x6be74c6#, 0x6be74c60, #) </summary>
        public static readonly TGI LD;
		/// <summary>Exemplar file: road network (0x6534284a, 0x2821ed93, #)</summary>
		public static readonly TGI EXEMPLAR_ROAD;
		/// <summary>Exemplar file: street network (0x6534284a, 0xa92a02ea, #)</summary>
		public static readonly TGI EXEMPLAR_STREET;
		/// <summary>Exemplar file: one-way road network (0x6534284a, 0xcbe084cb, #)</summary>
		public static readonly TGI EXEMPLAR_ONEWAYROAD;
		/// <summary>Exemplar file: avenue network (0x6534284a, 0xcb730fac, #)</summary>
		public static readonly TGI EXEMPLAR_AVENUE;
		/// <summary>Exemplar file: elevated highway network (0x6534284a, 0xa8434037, #)</summary>
		public static readonly TGI EXEMPLAR_HIGHWAY;
		/// <summary>Exemplar file: ground highway network (0x6534284a, 0xebe084d1, #)</summary>
		public static readonly TGI EXEMPLAR_GROUNDHIGHWAY;
		/// <summary>Exemplar file: dirt road/ANT/RHW network (0x6534284a, 0x6be08658, #)</summary>
		public static readonly TGI EXEMPLAR_DIRTROAD;
		/// <summary>Exemplar file: rail network (0x6534284a, 0xe8347989, #)</summary>
		public static readonly TGI EXEMPLAR_RAIL;
		/// <summary>Exemplar file: light rail network (0x6534284a, 0x2b79dffb, #)</summary>
		public static readonly TGI EXEMPLAR_LIGHTRAIL;
		/// <summary>Exemplar file: monorail network (0x6534284a, 0xebe084c2, #)</summary>
		public static readonly TGI EXEMPLAR_MONORAIL;
		/// <summary>Exemplar file: power poles network (0x6534284a, 0x088e1962, #)</summary>
		public static readonly TGI EXEMPLAR_POWERPOLE;
		/// <summary>Exemplar file: Type 21 (0x6534284a, 0x89AC5643, #)</summary>
		public static readonly TGI EXEMPLAR_T21;
		/// <summary>Exemplar file: LotInfo, LotConfig (0x6534284a, #, #)</summary>
		public static readonly TGI EXEMPLAR;
		/// <summary>Cohort file (0x05342861, #, #)</summary>
		public static readonly TGI COHORT;
		/// <summary>PNG file: Menu building icons, bridges, overlays (0x856ddbac, 0x6a386d26, #)</summary>
		public static readonly TGI PNG_ICON;
		/// <summary>PNG file (image, icon) (0x856ddbac, #, #)</summary>
		public static readonly TGI PNG;
		/// <summary>FSH file: Transit Textures/Buildings/Bridges/Misc (0x7ab50e44, 0x1abe787d, #)</summary>
		[Obsolete("Use FISH_MISC instead.")]
		public static readonly TGI FSH_TRANSIT;
		/// <summary>FSH file: Transit Textures/Buildings/Bridges/Misc (0x7ab50e44, 0x1abe787d, #)</summary>
		public static readonly TGI FSH_MISC;
		/// <summary>FSH file: Base and Overlay Lot Textures (0x7ab50e44, 0x0986135e, #)</summary>
		public static readonly TGI FSH_BASE_OVERLAY;
		/// <summary>FSH file: Transit Network Shadows (Masks) (0x7ab50e44, 0x2BC2759a, #)</summary>
		public static readonly TGI FSH_SHADOW;
		/// <summary>FSH file: Animation Sprites (Props) (0x7ab50e44, 0x2a2458f9, #)</summary>
		public static readonly TGI FSH_ANIM_PROPS;
		/// <summary>FSH file: Animation Sprites (Non Props) (0x7ab50e44, 0x49a593e7, #)</summary>
		public static readonly TGI FSH_ANIM_NONPROPS;
		/// <summary>FSH file: Terrain And Foundations (0x7ab50e44, 0x891b0e1a, #)</summary>
		public static readonly TGI FSH_TERRAIN_FOUNDATION;
		/// <summary>FSH file: User Interface Images (0x7ab50e44, 0x46a006b#, #)</summary>
		public static readonly TGI FSH_UI;
		/// <summary>FSH file: Textures (0x7ab50e44, #, #)</summary>
		public static readonly TGI FSH;
		/// <summary>S3D file: Maxis Models (0x5ad0e817, 0xbadb57f1, #)</summary>
		public static readonly TGI S3D_MAXIS;
		/// <summary>S3D file: Models (0x5ad0e817, #, #)</summary>
		public static readonly TGI S3D;
		/// <summary>SC4PATH (2D) (0x296678f7, 0x69668828, #)</summary>
		public static readonly TGI SC4PATH_2D;
		/// <summary>SC4PATH (3D) (0x296678f7, 0xa966883f, #)</summary>
		public static readonly TGI SC4PATH_3D;
		/// <summary>SC4PATH file (0x296678f7, #, #)</summary>
		public static readonly TGI SC4PATH;
		/// <summary>LUA file: Missions, Advisors, Tutorials and Packaging files (0xca63e2a3, 0x4a5e8ef6, #)</summary>
		public static readonly TGI LUA;
		/// <summary>LUA file: Generators, Attractors, Repulsors and System LUA (0xca63e2a3, 0x4a5e8f3f, #)</summary>
		public static readonly TGI LUA_GEN;
		/// <summary>RUL file: Network rules (0x0a5bcf4b, 0xaa5bcf57, #)</summary>
		public static readonly TGI RUL;
		/// <summary>WAV file (0x2026960b, 0xaa4d1933, #)</summary>
		public static readonly TGI WAV;
		/// <summary>LTEXT file (0x2026960b, #, #)</summary>
		public static readonly TGI LTEXT;
		/// <summary>Effect Directory file (0xea5118b0, #, #)</summary>
		public static readonly TGI EFFDIR;
		/// <summary>Font Table INI (#, 0x4a87bfe8, 0x2a87bffc)</summary>
		public static readonly TGI INI_FONT;
		/// <summary>Network INI: Remapping, Bridge Exemplars (#, 0x8a5971c5, 0x8a5993b9)</summary>
		public static readonly TGI INI_NETWORK;
		/// <summary>INI file (#, 0x8a5971c5, #)</summary>
		public static readonly TGI INI;
        /// <summary>UI file (0x000000, 0x96a006b0, #)</summary>
        public static readonly TGI UI;
        /// <summary>XML file (0x88777602, #, #)</summary>
        public static readonly TGI XML;
        /// <summary>BLANKTGI (0, 0, 0)</summary>
        public static readonly TGI BLANKTGI;
        #endregion KnownTGIs

        /// <summary>
        /// This static constructor will be called as soon as the class is loaded into memory, and not necessarily when an object is created.
        /// Known types need to be ordered "bottom-up", that is, specialized entries need to be inserted first, more general ones later.
        /// A null value indicates that any number is valid to identify the type.
        /// </summary>
        static DBPFTGI() {
            
            DIRECTORY = new TGI(0xe86b1eef, 0xe86b1eef, 0x286b1f03);
            LD = new TGI(0x6be74c60, 0x6be74c60, null);
            S3D_MAXIS = new TGI(0x5ad0e817, 0xbadb57f1, null);
            S3D = new TGI(0x5ad0e817, null, null);
            COHORT = new TGI(0x05342861, null, null);

            EXEMPLAR_ROAD = new TGI(0x6534284a, 0x2821ed93, null); //EXEMPLAR (Road)
            EXEMPLAR_STREET = new TGI(0x6534284a, 0xa92a02ea, null); //EXEMPLAR (Street)
            EXEMPLAR_ONEWAYROAD = new TGI(0x6534284a, 0xcbe084cb, null); //EXEMPLAR (One-Way Road)
            EXEMPLAR_AVENUE = new TGI(0x6534284a, 0xcb730fac, null); //EXEMPLAR (Avenue)
            EXEMPLAR_HIGHWAY = new TGI(0x6534284a, 0xa8434037, null); //EXEMPLAR (Highway)
            EXEMPLAR_GROUNDHIGHWAY = new TGI(0x6534284a, 0xebe084d1, null); //EXEMPLAR (Ground Highway)
            EXEMPLAR_DIRTROAD = new TGI(0x6534284a, 0x6be08658, null); //EXEMPLAR (Dirtroad)
            EXEMPLAR_RAIL = new TGI(0x6534284a, 0xe8347989, null); //EXEMPLAR (Rail)
            EXEMPLAR_LIGHTRAIL = new TGI(0x6534284a, 0x2b79dffb, null); //EXEMPLAR (Lightrail)
            EXEMPLAR_MONORAIL = new TGI(0x6534284a, 0xebe084c2, null); //EXEMPLAR (Monorail)
            EXEMPLAR_POWERPOLE = new TGI(0x6534284a, 0x088e1962, null); //EXEMPLAR (Power Pole)
            EXEMPLAR_T21 = new TGI(0x6534284a, 0x89ac5643, null); //EXEMPLAR (T21)
            EXEMPLAR = new TGI(0x6534284a, null, null);

            FSH_MISC = new TGI(0x7ab50e44, 0x1abe787d, null); //FSH (Misc)
            FSH_BASE_OVERLAY = new TGI(0x7ab50e44, 0x0986135e, null); //FSH (Base/Overlay Texture)
            FSH_SHADOW = new TGI(0x7ab50e44, 0x2bc2759a, null); //FSH (Shadow Mask)
            FSH_ANIM_PROPS = new TGI(0x7ab50e44, 0x2a2458f9, null); //FSH (Animation Sprites (Props))
            FSH_ANIM_NONPROPS = new TGI(0x7ab50e44, 0x49a593e7, null); //FSH (Animation Sprites (Non Props))
            FSH_TERRAIN_FOUNDATION = new TGI(0x7ab50e44, 0x891b0e1a, null); //FSH (Terrain/Foundation)
            FSH_UI = new TGI(0x7ab50e44, 0x46a006b0, null); //FSH (UI Image)
            FSH = new TGI(0x7ab50e44, null, null);

            SC4PATH_2D = new TGI(0x296678f7, 0x69668828, null); //SC4PATH (2D)
            SC4PATH_3D = new TGI(0x296678f7, 0xa966883f, null); //SC4PATH (3D)
            SC4PATH = new TGI(0x296678f7, null, null);

            PNG_ICON = new TGI(0x856ddbac, 0x6a386d26, null); //PNG (Icon)
            PNG = new TGI(0x856ddbac, null, null);
            LUA = new TGI(0xca63e2a3, 0x4a5e8ef6, null);
            LUA_GEN = new TGI(0xca63e2a3, 0x4a5e8f3f, null); //LUA (Generators)
            WAV = new TGI(0x2026960b, 0xaa4d1933, null);
            LTEXT = new TGI(0x2026960b, null, null);
            INI_FONT = new TGI(0, 0x4a87bfe8, 0x2a87bffc); //INI (Font Table)
            INI_NETWORK = new TGI(0, 0x8a5971c5, 0x8a5993b9); //INI (Networks)
            INI = new TGI(0, 0x8a5971c5, null);
            UI = new TGI(0, 0x96a006b0, null);
            RUL = new TGI(0x0a5bcf4b, 0xaa5bcf57, null);
            XML = new TGI(0x88777602, null, null);
            EFFDIR = new TGI(0xea5118b0, null, null);
            BLANKTGI = new TGI(null, null, null); // BLANKTGI matches with everything - this MUST be last in the list

            KnownEntries.Add(DIRECTORY, new TGIDetails("DIR", "DIR"));
            KnownEntries.Add(LD, new TGIDetails("LD", "LD"));
            KnownEntries.Add(S3D_MAXIS, new TGIDetails("S3D", "S3D"));
            KnownEntries.Add(S3D, new TGIDetails("S3D", "S3D"));
            KnownEntries.Add(COHORT, new TGIDetails("EXMP", "COHORT"));
            KnownEntries.Add(EXEMPLAR_ROAD, new TGIDetails("EXMP", "EXEMPLAR_ROAD"));
            KnownEntries.Add(EXEMPLAR_STREET, new TGIDetails("EXMP", "EXEMPLAR_STREET"));
            KnownEntries.Add(EXEMPLAR_ONEWAYROAD, new TGIDetails("EXMP", "EXEMPLAR_ONEWAYROAD"));
            KnownEntries.Add(EXEMPLAR_AVENUE, new TGIDetails("EXMP", "EXEMPLAR_AVENUE"));
            KnownEntries.Add(EXEMPLAR_HIGHWAY, new TGIDetails("EXMP", "EXEMPLAR_HIGHWAY"));
            KnownEntries.Add(EXEMPLAR_GROUNDHIGHWAY, new TGIDetails("EXMP", "EXEMPLAR_GROUNDHIGHWAY"));
            KnownEntries.Add(EXEMPLAR_DIRTROAD, new TGIDetails("EXMP", "EXEMPLAR_DIRTROAD"));
            KnownEntries.Add(EXEMPLAR_RAIL, new TGIDetails("EXMP", "EXEMPLAR_RAIL"));
            KnownEntries.Add(EXEMPLAR_LIGHTRAIL, new TGIDetails("EXMP", "EXEMPLAR_LIGHTRAIL"));
            KnownEntries.Add(EXEMPLAR_MONORAIL, new TGIDetails("EXMP", "EXEMPLAR_MONORAIL"));
            KnownEntries.Add(EXEMPLAR_POWERPOLE, new TGIDetails("EXMP", "EXEMPLAR_POWERPOLE"));
            KnownEntries.Add(EXEMPLAR_T21, new TGIDetails("EXMP", "EXEMPLAR_T21"));
            KnownEntries.Add(EXEMPLAR, new TGIDetails("EXMP", "EXEMPLAR"));
            KnownEntries.Add(FSH_MISC, new TGIDetails("FSH", "FSH_MISC"));
            KnownEntries.Add(FSH_BASE_OVERLAY, new TGIDetails("FSH", "FSH_BASE_OVERLAY"));
            KnownEntries.Add(FSH_SHADOW, new TGIDetails("FSH", "FSH_SHADOW"));
            KnownEntries.Add(FSH_ANIM_PROPS, new TGIDetails("FSH", "FSH_ANIM_PROPS"));
            KnownEntries.Add(FSH_ANIM_NONPROPS, new TGIDetails("FSH", "FSH_ANIM_NONPROPS"));
            KnownEntries.Add(FSH_TERRAIN_FOUNDATION, new TGIDetails("FSH", "FSH_TERRAIN_FOUNDATION"));
            KnownEntries.Add(FSH_UI, new TGIDetails("FSH", "FSH_UI"));
            KnownEntries.Add(FSH, new TGIDetails("FSH", "FSH"));
            KnownEntries.Add(SC4PATH_2D, new TGIDetails("PATH", "SC4PATH_2D"));
            KnownEntries.Add(SC4PATH_3D, new TGIDetails("PATH", "SC4PATH_3D"));
            KnownEntries.Add(SC4PATH, new TGIDetails("PATH", "SC4PATH"));
            KnownEntries.Add(PNG_ICON, new TGIDetails("PNG", "PNG_ICON"));
            KnownEntries.Add(PNG, new TGIDetails("PNG", "PNG"));
            KnownEntries.Add(LUA, new TGIDetails("LUA", "LUA"));
            KnownEntries.Add(LUA_GEN, new TGIDetails("LUA", "LUA_GEN"));
            KnownEntries.Add(WAV, new TGIDetails("WAV", "WAV"));
            KnownEntries.Add(LTEXT, new TGIDetails("LTEXT", "LTEXT"));
            KnownEntries.Add(INI_FONT, new TGIDetails("INI", "INI_FONT"));
            KnownEntries.Add(INI_NETWORK, new TGIDetails("INI", "INI_NETWORK"));
            KnownEntries.Add(INI, new TGIDetails("INI", "INI"));
            KnownEntries.Add(UI, new TGIDetails("UI", "UI"));
            KnownEntries.Add(XML, new TGIDetails("RUL", "RUL"));
            KnownEntries.Add(RUL, new TGIDetails("XML", "XML"));
            KnownEntries.Add(EFFDIR, new TGIDetails("EFF", "EFFDIR"));
            KnownEntries.Add(BLANKTGI, new TGIDetails("BLANK", "BLANKTGI"));
        }
	}
}
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

//See: https://github.com/memo33/jDBPFX/blob/master/src/jdbpfx/DBPFTGI.java
namespace csDBPF {
	/// <summary>
	/// A DBPFTGI encapsulates a Type, Group, Instance identifier.
	/// </summary>
	/// <remarks>
	/// Common known entry types are listed in <see cref="KnownEntries"/>.
	/// </remarks>
	public class DBPFTGI {
		#region KnownTGIs
		//In general Dictionary items are kept in the order they are added, and since we're not doing a lot of adding/deleting/otherwise sorting, its not as big of a deal and we don't need to use a special type like SortedDictionary
		private static readonly List<DBPFTGI> KnownEntries = new List<DBPFTGI>();
		/// <summary>BLANKTGI (0, 0, 0)</summary>
		public static readonly DBPFTGI BLANKTGI;
		/// <summary>Directory file (0xe86b1eef, 0xe86b1eef, 0x286b1f03)</summary>
		public static readonly DBPFTGI DIRECTORY;
		/// <summary>LD file (0x6be74c6#, 0x6be74c6#, #) </summary>
		public static readonly DBPFTGI LD; /** */
		/// <summary>Exemplar file: road network (0x6534284a, 0x2821ed93, #)</summary>
		public static readonly DBPFTGI EXEMPLAR_ROAD;
		/// <summary>Exemplar file: street network (0x6534284a, 0xa92a02ea, #)</summary>
		public static readonly DBPFTGI EXEMPLAR_STREET;
		/// <summary>Exemplar file: one-way road network (0x6534284a, 0xcbe084cb, #)</summary>
		public static readonly DBPFTGI EXEMPLAR_ONEWAYROAD;
		/// <summary>Exemplar file: avenue network (0x6534284a, 0xcb730fac, #)</summary>
		public static readonly DBPFTGI EXEMPLAR_AVENUE;
		/// <summary>Exemplar file: elevated highway network (0x6534284a, 0xa8434037, #)</summary>
		public static readonly DBPFTGI EXEMPLAR_HIGHWAY;
		/// <summary>Exemplar file: ground highway network (0x6534284a, 0xebe084d1, #)</summary>
		public static readonly DBPFTGI EXEMPLAR_GROUNDHIGHWAY;
		/// <summary>Exemplar file: dirt road/ANT/RHW network (0x6534284a, 0x6be08658, #)</summary>
		public static readonly DBPFTGI EXEMPLAR_DIRTROAD;
		/// <summary>Exemplar file: rail network (0x6534284a, 0xe8347989, #)</summary>
		public static readonly DBPFTGI EXEMPLAR_RAIL;
		/// <summary>Exemplar file: light rail network (0x6534284a, 0x2b79dffb, #)</summary>
		public static readonly DBPFTGI EXEMPLAR_LIGHTRAIL;
		/// <summary>Exemplar file: monorail network (0x6534284a, 0xebe084c2, #)</summary>
		public static readonly DBPFTGI EXEMPLAR_MONORAIL;
		/// <summary>Exemplar file: power poles network (0x6534284a, 0x088e1962, #)</summary>
		public static readonly DBPFTGI EXEMPLAR_POWERPOLE;
		/// <summary>Exemplar file: Type 21 (0x6534284a, 0x89AC5643, #)</summary>
		public static readonly DBPFTGI EXEMPLAR_T21;
		/// <summary>Exemplar file: LotInfo, LotConfig (0x6534284a, #, #)</summary>
		public static readonly DBPFTGI EXEMPLAR;
		/// <summary>Cohort file (0x05342861, #, #)</summary>
		public static readonly DBPFTGI COHORT;
		/// <summary>PNG file: Menu building icons, bridges, overlays (0x856ddbac, 0x6a386d26, #)</summary>
		public static readonly DBPFTGI PNG_ICON;
		/// <summary>PNG file (image, icon) (0x856ddbac, #, #)</summary>
		public static readonly DBPFTGI PNG;
		/// <summary>FSH file: Transit Textures/Buildings/Bridges/Misc (0x7ab50e44, 0x1abe787d, #)</summary>
		[Obsolete("Use FISH_MISC instead.")]
		public static readonly DBPFTGI FSH_TRANSIT;
		/// <summary>FSH file: Transit Textures/Buildings/Bridges/Misc (0x7ab50e44, 0x1abe787d, #)</summary>
		public static readonly DBPFTGI FSH_MISC;
		/// <summary>FSH file: Base and Overlay Lot Textures (0x7ab50e44, 0x0986135e, #)</summary>
		public static readonly DBPFTGI FSH_BASE_OVERLAY;
		/// <summary>FSH file: Transit Network Shadows (Masks) (0x7ab50e44, 0x2BC2759a, #)</summary>
		public static readonly DBPFTGI FSH_SHADOW;
		/// <summary>FSH file: Animation Sprites (Props) (0x7ab50e44, 0x2a2458f9, #)</summary>
		public static readonly DBPFTGI FSH_ANIM_PROPS;
		/// <summary>FSH file: Animation Sprites (Non Props) (0x7ab50e44, 0x49a593e7, #)</summary>
		public static readonly DBPFTGI FSH_ANIM_NONPROPS;
		/// <summary>FSH file: Terrain And Foundations (0x7ab50e44, 0x891b0e1a, #)</summary>
		public static readonly DBPFTGI FSH_TERRAIN_FOUNDATION;
		/// <summary>FSH file: User Interface Images (0x7ab50e44, 0x46a006b#, #)</summary>
		public static readonly DBPFTGI FSH_UI;
		/// <summary>FSH file: Textures (0x7ab50e44, #, #)</summary>
		public static readonly DBPFTGI FSH;
		/// <summary>S3D file: Maxis Models (0x5ad0e817, 0xbadb57f1, #)</summary>
		public static readonly DBPFTGI S3D_MAXIS;
		/// <summary>S3D file: Models (0x5ad0e817, #, #)</summary>
		public static readonly DBPFTGI S3D;
		/// <summary>SC4PATH (2D) (0x296678f7, 0x69668828, #)</summary>
		public static readonly DBPFTGI SC4PATH_2D;
		/// <summary>SC4PATH (3D) (0x296678f7, 0xa966883f, #)</summary>
		public static readonly DBPFTGI SC4PATH_3D;
		/// <summary>SC4PATH file (0x296678f7, #, #)</summary>
		public static readonly DBPFTGI SC4PATH;
		/// <summary>LUA file: Missions, Advisors, Tutorials and Packaging files (0xca63e2a3, 0x4a5e8ef6, #)</summary>
		public static readonly DBPFTGI LUA;
		/// <summary>LUA file: Generators, Attractors, Repulsors and System LUA (0xca63e2a3, 0x4a5e8f3f, #)</summary>
		public static readonly DBPFTGI LUA_GEN;
		/// <summary>RUL file: Network rules (0x0a5bcf4b, 0xaa5bcf57, #)</summary>
		public static readonly DBPFTGI RUL;
		/// <summary>WAV file (0x2026960b, 0xaa4d1933, #)</summary>
		public static readonly DBPFTGI WAV;
		/// <summary>LTEXT file (0x2026960b, #, #)</summary>
		public static readonly DBPFTGI LTEXT;
		/// <summary>Effect Directory file (0xea5118b#, #, #)</summary>
		public static readonly DBPFTGI EFFDIR;
		/// <summary>Font Table INI (#, 0x4a87bfe8, 0x2a87bffc)</summary>
		public static readonly DBPFTGI INI_FONT;
		/// <summary>Network INI: Remapping, Bridge Exemplars (#, 0x8a5971c5, 0x8a5993b9)</summary>
		public static readonly DBPFTGI INI_NETWORK;
		/// <summary>INI file (#, 0x8a5971c5, #)</summary>
		public static readonly DBPFTGI INI;
		/// <summary>XML file (0x88777602, #, #)</summary>
		public static readonly DBPFTGI XML;
		/// <summary>NULLTGI (#, #, #)</summary>
		public static readonly DBPFTGI NULLTGI;
		#endregion KnownTGIs



		private uint? _typeID;
		/// <summary>
		/// Type ID (TID). See <see ref="https://www.wiki.sc4devotion.com/index.php?title=Type_ID"/>
		/// </summary>
		public uint? TypeID {
			get { return _typeID; }
			private set { _typeID = value; }
		}

		private uint? _groupID;
		/// <summary>
		/// Group ID (GID). See <see ref="https://www.wiki.sc4devotion.com/index.php?title=Group_ID"/>
		/// </summary>
		public uint? GroupID {
			get { return _groupID; }
			private set { _groupID = value; }
		}

		private uint? _instanceID;
		/// <summary>
		/// Instance ID (IID). See <see ref="https://www.wiki.sc4devotion.com/index.php?title=Instance_ID"/>
		/// </summary>
		public uint? InstanceID {
			get { return _instanceID; }
			private set { _instanceID = value; }
		}

		private string _category;
		/// <summary>
		/// The general file type this TGI represents.
		/// </summary>
		public string Category {
			get { return _category; }
			private set { _category = value; }
		}

		private string _detail;
		/// <summary>
		/// The detailed description of the file type this TGI represents.
		/// </summary>
		public string Detail {
			get { return _detail; }
			private set { _detail = value; }
		}


		
		/// <summary>
		/// Create a new DBPFTGI from the specified Type Group and Instance.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="group"></param>
		/// <param name="instance"></param>
		public DBPFTGI(uint type, uint group, uint instance) {
			//Important: Never allow creation of null TID, GID, or IID because they interfere with the lookups of KnownType.
			SetTGI(type, group, instance);
		}

		/// <summary>
		/// Create a new DBPFTGI based on a known entry type.
		/// </summary>
		/// <remarks>
		/// If knownEntry TID is null, the new TID is 0, if knownEntry GID or IID is null a random ID is assigned.
		/// </remarks>
		/// <param name="knownEntry">Known entry type</param>
		public DBPFTGI(DBPFTGI knownEntry) {
			_typeID = knownEntry.TypeID != null ? knownEntry.TypeID : 0;
			if (knownEntry.GroupID is null) {
				SetRandomGroup();
			} else {
				_groupID = knownEntry.GroupID;
			}
			if (knownEntry.InstanceID is null) {
				SetRandomInstance();
			} else {
				_instanceID = knownEntry.InstanceID;
			}
			_category = knownEntry.Category;
			_detail = knownEntry.Detail;
		}



		/// <summary>
		/// Check if this DBPFTGI matches a specific known DBPFTGI entry type. Unlike equals, this method is not reflexive.
		/// </summary>
		/// <remarks>
		/// If any component of the provided DBPFTGI of knownType is null it will be skipped. This is opposed to Equals which explicitly checks every component.
		/// Only the provided DBPFTGI of knownType is checked for null components.
		/// </remarks>
		/// <param name="knownType">A DBPFTGI to check against</param>
		/// <returns>TRUE if check passes; FALSE otherwise</returns>
		public bool MatchesKnownTGI(DBPFTGI knownType) {
			bool isTIDok, isGIDok, isIIDok;

			if (!knownType._typeID.HasValue) {
				isTIDok = true;
			} else {
				isTIDok = _typeID == knownType.TypeID;
			}

			if (!knownType.GroupID.HasValue) {
				isGIDok = true;
			} else {
				isGIDok = _groupID == knownType.GroupID;
			}

			if (!knownType.InstanceID.HasValue) {
				isIIDok = true;
			} else {
				isIIDok = _instanceID == knownType.InstanceID;
			}

			return isTIDok && isGIDok && isIIDok;
		}


		/// <summary>
		/// Checks if this DBPFTGI matches any of the known DBPFTGI entry types and returns the matched DBPFTGI if a match is found.
		/// </summary>
		/// <returns>A DBPFTGI of the matched known type if found; null otherwise.</returns>
		public DBPFTGI MatchesAnyKnownTGI() {
			foreach (DBPFTGI TGI in KnownEntries) {
				if (Equals(TGI)) {
					return TGI;
				}
			}
			return null;
		}


		/// <summary>
		/// Tests for equality of DBPFTGI objects by comparing T, G, I components of each. This method is reflexive.
		/// </summary>
		/// <remarks>If any component of the passed DBPFTGI is null that component is ignored in the evaluation.</remarks>
		/// <param name="obj">Any object to compare</param>
		/// <returns>TRUE if check passes; FALSE otherwise</returns>
		public override bool Equals(object obj) {
			bool evalT, evalG, evalI;
			if (obj is DBPFTGI checkTGI) {
				if (checkTGI.TypeID is not null) {
					evalT = _typeID == checkTGI.TypeID;
				} else {
					evalT = true;
				}
				if (checkTGI.GroupID is not null) {
					evalG = _groupID == checkTGI.GroupID;
				} else {
					evalG = true;
				}
				if (checkTGI.InstanceID is not null) {
					evalI = _instanceID == checkTGI.InstanceID;
				} else {
					evalI = true;
				}
				return evalT && evalG && evalI;
			} else {
				return false;
			}
		}



        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object</returns>
        public override string ToString() {
			return $"0x{DBPFUtil.ToHexString(_typeID)}, 0x{DBPFUtil.ToHexString(_groupID)}, 0x{DBPFUtil.ToHexString(_instanceID)}, {_category}, {_detail}";
		}
        /// <summary>
        /// Returns a string that represents only the Type, Group, Instance of this object.
        /// </summary>
        /// <param name="uppercase">Specify output as uppercase. Default is lowercase.</param>
        /// <returns>A string that represents the current object</returns>
        public string ToStringShort(bool uppercase = false) {
			return $"0x{DBPFUtil.ToHexString(_typeID, 8 , uppercase)}, 0x{DBPFUtil.ToHexString(_groupID, 8, uppercase)}, 0x{DBPFUtil.ToHexString(_instanceID, 8, uppercase)}";
		}



		/// <summary>
		/// Set the Type, Group, or Instance of this TGI instance. Providing a null parameter indicates that identifier will not be set.
		/// </summary>
		/// <param name="type">Type identifier to set</param>
		/// <param name="group">Group identifier to set</param>
		/// <param name="instance">Instance identifier to set</param>
		/// <remarks>If the TGI set matches a known combination then Category and Detail are set too.</remarks>
		public void SetTGI(uint? type = null, uint? group = null, uint? instance = null) {
			if (type != null) {
				_typeID = type;
			}
			if (group != null) {
				_groupID = group;
			}
			if (instance != null) {
				_instanceID = instance;
			}

			UpdateCategoryAndDetail();
		}



		/// <summary>
		/// Set the Type, Group, and Instance of this TGI instance.
		/// </summary>
		/// <param name="tgi">TGI set to set this instace to</param>
		/// <remarks>If the TGI set matches a known combination then Category and Detail are set too. A random G or I if that parameter in the provided TGI is null.</remarks>
		public void SetTGI(DBPFTGI tgi) {
			if (tgi.TypeID != null) {
				_typeID = tgi.TypeID;
			}
			if (tgi.GroupID != null) {
				_groupID = tgi.GroupID;
			} else {
				SetRandomGroup();
			}
			if (tgi.InstanceID != null) {
				_instanceID = tgi.InstanceID;
			} else {
				SetRandomInstance();
			}

			UpdateCategoryAndDetail();
		}



		/// <summary>
		/// Assign a random Group identifier.
		/// </summary>
		public void SetRandomGroup() {
			//https://stackoverflow.com/a/18332307/10802255
			Random rand = new Random();
			_groupID = (uint) (rand.Next(1 << 30)) << 2 | (uint) (rand.Next(1 << 2));

			UpdateCategoryAndDetail();
		}

		/// <summary>
		/// Assign a random Instance identifier.
		/// </summary>
		public void SetRandomInstance() {
			Random rand = new Random();
			_instanceID = (uint) (rand.Next(1 << 30)) << 2 | (uint) (rand.Next(1 << 2));

			UpdateCategoryAndDetail();
		}



		/// <summary>
		/// Update the category and detail properties of this instance based on the current TGI.
		/// </summary>
		private void UpdateCategoryAndDetail() {
			DBPFTGI match = MatchesAnyKnownTGI();
			if (match is null) {
				_category = null;
				_detail = null;
			} else {
				_category = match.Category;
				_detail = match.Detail;
			}
		}



		/// <summary>
		/// This static constructor will be called as soon as the class is loaded into memory, and not necessarily when an object is created.
		/// Known types need to be ordered "bottom-up", that is, specialized entries need to be inserted first, more general ones later.
		/// </summary>
		static DBPFTGI() {
			BLANKTGI = new DBPFTGI(0, 0, 0, null, null);
			DIRECTORY = new DBPFTGI(0xe86b1eef, 0xe86b1eef, 0x286b1f03, "DIR", "DIR");
			LD = new DBPFTGI(0x6be74c60, 0x6be74c60, null, "LD", "LD");
			S3D_MAXIS = new DBPFTGI(0x5ad0e817, 0xbadb57f1, null, "S3D", "S3D");
			S3D = new DBPFTGI(0x5ad0e817, null, null, "S3D", "S3D");
			COHORT = new DBPFTGI(0x05342861, null, null, "EXMP", "COHORT");

			EXEMPLAR_ROAD = new DBPFTGI(0x6534284a, 0x2821ed93, null, "EXMP", "EXEMPLAR_ROAD"); //EXEMPLAR (Road)
			EXEMPLAR_STREET = new DBPFTGI(0x6534284a, 0xa92a02ea, null, "EXMP", "EXEMPLAR_STREET"); //EXEMPLAR (Street)
			EXEMPLAR_ONEWAYROAD = new DBPFTGI(0x6534284a, 0xcbe084cb, null, "EXMP", "EXEMPLAR_ONEWAYROAD"); //EXEMPLAR (One-Way Road)
			EXEMPLAR_AVENUE = new DBPFTGI(0x6534284a, 0xcb730fac, null, "EXMP", "EXEMPLAR_AVENUE"); //EXEMPLAR (Avenue)
			EXEMPLAR_HIGHWAY = new DBPFTGI(0x6534284a, 0xa8434037, null, "EXMP", "EXEMPLAR_HIGHWAY"); //EXEMPLAR (Highway)
			EXEMPLAR_GROUNDHIGHWAY = new DBPFTGI(0x6534284a, 0xebe084d1, null, "EXMP", "EXEMPLAR_GROUNDHIGHWAY"); //EXEMPLAR (Ground Highway)
			EXEMPLAR_DIRTROAD = new DBPFTGI(0x6534284a, 0x6be08658, null, "EXMP", "EXEMPLAR_DIRTROAD"); //EXEMPLAR (Dirtroad)
			EXEMPLAR_RAIL = new DBPFTGI(0x6534284a, 0xe8347989, null, "EXMP", "EXEMPLAR_RAIL"); //EXEMPLAR (Rail)
			EXEMPLAR_LIGHTRAIL = new DBPFTGI(0x6534284a, 0x2b79dffb, null, "EXMP", "EXEMPLAR_LIGHTRAIL"); //EXEMPLAR (Lightrail)
			EXEMPLAR_MONORAIL = new DBPFTGI(0x6534284a, 0xebe084c2, null, "EXMP", "EXEMPLAR_MONORAIL"); //EXEMPLAR (Monorail)
			EXEMPLAR_POWERPOLE = new DBPFTGI(0x6534284a, 0x088e1962, null, "EXMP", "EXEMPLAR_POWERPOLE"); //EXEMPLAR (Power Pole)
			EXEMPLAR_T21 = new DBPFTGI(0x6534284a, 0x89ac5643, null, "EXMP", "EXEMPLAR_T21"); //EXEMPLAR (T21)
			EXEMPLAR = new DBPFTGI(0x6534284a, null, null, "EXMP", "EXEMPLAR");

			FSH_MISC = new DBPFTGI(0x7ab50e44, 0x1abe787d, null, "FSH", "FSH_MISC"); //FSH (Misc)
			FSH_TRANSIT = new DBPFTGI(0x7ab50e44, 0x1abe787d, null, "FSH", "FSH_MISC"); //FSH (Misc)
			FSH_BASE_OVERLAY = new DBPFTGI(0x7ab50e44, 0x0986135e, null, "FSH", "FSH_BASE_OVERLAY"); //FSH (Base/Overlay Texture)
			FSH_SHADOW = new DBPFTGI(0x7ab50e44, 0x2bC2759a, null, "FSH", "FSH_SHADOW"); //FSH (Shadow Mask)
			FSH_ANIM_PROPS = new DBPFTGI(0x7ab50e44, 0x2a2458f9, null, "FSH", "FSH_ANIM_PROPS"); //FSH (Animation Sprites (Props))
			FSH_ANIM_NONPROPS = new DBPFTGI(0x7ab50e44, 0x49a593e7, null, "FSH", "FSH_ANIM_NONPROPS"); //FSH (Animation Sprites (Non Props))
			FSH_TERRAIN_FOUNDATION = new DBPFTGI(0x7ab50e44, 0x891b0e1a, null, "FSH", "FSH_TERRAIN_FOUNDATION"); //FSH (Terrain/Foundation)
			FSH_UI = new DBPFTGI(0x7ab50e44, 0x46a006b0, null, "FSH", "FSH_UI"); //FSH (UI Image)
			FSH = new DBPFTGI(0x7ab50e44, null, null, "FSH", "FSH");

			SC4PATH_2D = new DBPFTGI(0x296678f7, 0x69668828, null, "PATH", "SC4PATH_2D"); //SC4PATH (2D)
			SC4PATH_3D = new DBPFTGI(0x296678f7, 0xa966883f, null, "PATH", "SC4PATH_3D"); //SC4PATH (3D)
			SC4PATH = new DBPFTGI(0x296678f7, null, null, "PATH", "SC4PATH");

			PNG_ICON = new DBPFTGI(0x856ddbac, 0x6a386d26, null, "PNG", "PNG_ICON"); //PNG (Icon)
			PNG = new DBPFTGI(0x856ddbac, null, null, "PNG", "PNG");
			LUA = new DBPFTGI(0xca63e2a3, 0x4a5e8ef6, null, "LUA", "LUA");
			LUA_GEN = new DBPFTGI(0xca63e2a3, 0x4a5e8f3f, null, "LUA", "LUA_GEN"); //LUA (Generators)
			WAV = new DBPFTGI(0x2026960b, 0xaa4d1933, null, "WAV", "WAV");
			LTEXT = new DBPFTGI(0x2026960b, null, null, "LTEXT", "LTEXT");
			INI_FONT = new DBPFTGI(0, 0x4a87bfe8, 0x2a87bffc, "INI", "INI_FONT"); //INI (Font Table)
			INI_NETWORK = new DBPFTGI(0, 0x8a5971c5, 0x8a5993b9, "INI", "INI_NETWORK"); //INI (Networks)
			INI = new DBPFTGI(0, 0x8a5971c5, null, "INI", "INI");
			RUL = new DBPFTGI(0x0a5bcf4b, 0xaa5bcf57, null, "RUL", "RUL");
			XML = new DBPFTGI(0x88777602, null, null, "XML", "XML");
			EFFDIR = new DBPFTGI(0xea5118b0, null, null, "EFF", "EFFDIR");
			NULLTGI = new DBPFTGI(null, null, null, "NULL", "NULLTGI"); // NULLTGI matches with everything

			KnownEntries.Add(BLANKTGI);
			KnownEntries.Add(DIRECTORY);
			KnownEntries.Add(LD);
			KnownEntries.Add(S3D_MAXIS);
			KnownEntries.Add(S3D);
			KnownEntries.Add(COHORT);
			KnownEntries.Add(EXEMPLAR_ROAD);
			KnownEntries.Add(EXEMPLAR_STREET);
			KnownEntries.Add(EXEMPLAR_ONEWAYROAD);
			KnownEntries.Add(EXEMPLAR_AVENUE);
			KnownEntries.Add(EXEMPLAR_HIGHWAY);
			KnownEntries.Add(EXEMPLAR_GROUNDHIGHWAY);
			KnownEntries.Add(EXEMPLAR_DIRTROAD);
			KnownEntries.Add(EXEMPLAR_RAIL);
			KnownEntries.Add(EXEMPLAR_LIGHTRAIL);
			KnownEntries.Add(EXEMPLAR_MONORAIL);
			KnownEntries.Add(EXEMPLAR_POWERPOLE);
			KnownEntries.Add(EXEMPLAR_T21);
			KnownEntries.Add(EXEMPLAR);
			KnownEntries.Add(FSH_MISC);
			KnownEntries.Add(FSH_TRANSIT);
			KnownEntries.Add(FSH_BASE_OVERLAY);
			KnownEntries.Add(FSH_SHADOW);
			KnownEntries.Add(FSH_ANIM_PROPS);
			KnownEntries.Add(FSH_ANIM_NONPROPS);
			KnownEntries.Add(FSH_TERRAIN_FOUNDATION);
			KnownEntries.Add(FSH_UI);
			KnownEntries.Add(FSH);
			KnownEntries.Add(SC4PATH_2D);
			KnownEntries.Add(SC4PATH_3D);
			KnownEntries.Add(SC4PATH);
			KnownEntries.Add(PNG_ICON);
			KnownEntries.Add(PNG);
			KnownEntries.Add(LUA);
			KnownEntries.Add(LUA_GEN);
			KnownEntries.Add(WAV);
			KnownEntries.Add(LTEXT);
			KnownEntries.Add(INI_FONT);
			KnownEntries.Add(INI_NETWORK);
			KnownEntries.Add(INI);
			KnownEntries.Add(XML);
			KnownEntries.Add(RUL);
			KnownEntries.Add(EFFDIR);
			KnownEntries.Add(NULLTGI); 
		}

		/// <summary>
		/// This constructor only to be used internally to this class to declare known TGI types in the static constructor.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="group"></param>
		/// <param name="instance"></param>
		/// <param name="label"></param>
		/// <param name="detailLabel"></param>
		private DBPFTGI(uint? type, uint? group, uint? instance, string label, string detailLabel) {
			_typeID = type;
			_groupID = group;
			_instanceID = instance;
			_category = label;
			_detail = detailLabel;
		}
	}
}

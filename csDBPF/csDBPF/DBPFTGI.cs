using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

//See: https://github.com/memo33/jDBPFX/blob/master/src/jdbpfx/DBPFTGI.java
namespace csDBPF {
	public class DBPFTGI {
		//In general Dictionary items are kept in the order they are added, and since we're not doing a lot of adding/deleting/otherwise sorting, its not as big of a deal and we don't need to use a special type like SortedDictionary
		//QUESTION - Fix knownEntries to use sorted dictionary type? https://stackoverflow.com/questions/1453190/does-the-enumerator-of-a-dictionarytkey-tvalue-return-key-value-pairs-in-the
		//TODO - also make this dictionary immutable
		private static readonly Dictionary<DBPFTGI, string> knownEntries = new Dictionary<DBPFTGI, string>();
		public static readonly DBPFTGI BLANKTGI; /** BLANKTGI (0, 0, 0) */
		public static readonly DBPFTGI DIRECTORY; /** Directory file (0xe86b1eef, 0xe86b1eef, 0x286b1f03) */
		public static readonly DBPFTGI LD; /** LD file (0x6be74c60, 0x6be74c60, 0) */
		public static readonly DBPFTGI EXEMPLAR_ROAD; /** Exemplar file: road network (0x6534284a, 0x2821ed93, 0) */
		public static readonly DBPFTGI EXEMPLAR_STREET; /** Exemplar file: street network (0x6534284a, 0xa92a02ea, 0) */
		public static readonly DBPFTGI EXEMPLAR_ONEWAYROAD; /** Exemplar file: one-way road network (0x6534284a, 0xcbe084cb, 0) */
		public static readonly DBPFTGI EXEMPLAR_AVENUE; /** Exemplar file: avenue network (0x6534284a, 0xcb730fac, 0) */
		public static readonly DBPFTGI EXEMPLAR_HIGHWAY; /** Exemplar file: elevated highway network (0x6534284a, 0xa8434037, 0) */
		public static readonly DBPFTGI EXEMPLAR_GROUNDHIGHWAY; /** Exemplar file: ground highway network (0x6534284a, 0xebe084d1, 0) */
		public static readonly DBPFTGI EXEMPLAR_DIRTROAD; /** Exemplar file: dirt road/ANT/RHW network (0x6534284a, 0x6be08658, 0) */
		public static readonly DBPFTGI EXEMPLAR_RAIL; /** Exemplar file: rail network (0x6534284a, 0xe8347989, 0) */
		public static readonly DBPFTGI EXEMPLAR_LIGHTRAIL; /** Exemplar file: light rail network (0x6534284a, 0x2b79dffb, 0) */
		public static readonly DBPFTGI EXEMPLAR_MONORAIL; /** Exemplar file: monorail network (0x6534284a, 0xebe084c2, 0) */
		public static readonly DBPFTGI EXEMPLAR_POWERPOLE; /** Exemplar file: power poles network (0x6534284a, 0x088e1962, 0) */
		public static readonly DBPFTGI EXEMPLAR_T21; /** Exemplar file: Type 21 (0x6534284a, 0x89AC5643, 0) */
		public static readonly DBPFTGI EXEMPLAR; /** Exemplar file: LotInfo, LotConfig (0x6534284a, 0, 0) */
		public static readonly DBPFTGI COHORT; /** Cohort file (0x05342861, 0, 0) */
		public static readonly DBPFTGI PNG_ICON; /** PNG file: Menu building icons, bridges, overlays (0x856ddbac, 0x6a386d26, 0) */
		public static readonly DBPFTGI PNG; /** PNG file (image, icon) (0x856ddbac, 0, 0) */
		[Obsolete("Use FISH_MISC instead.")]
		public static readonly DBPFTGI FSH_TRANSIT; /** FSH file: Transit Textures/Buildings/Bridges/Misc (0x7ab50e44, 0x1abe787d, 0) */
		public static readonly DBPFTGI FSH_MISC; /** FSH file: Transit Textures/Buildings/Bridges/Misc (0x7ab50e44, 0x1abe787d, 0) */
		public static readonly DBPFTGI FSH_BASE_OVERLAY; /** FSH file: Base and Overlay Lot Textures (0x7ab50e44, 0x0986135e, 0) */
		public static readonly DBPFTGI FSH_SHADOW; /** FSH file: Transit Network Shadows (Masks) (0x7ab50e44, 0x2BC2759a, 0) */
		public static readonly DBPFTGI FSH_ANIM_PROPS; /** FSH file: Animation Sprites (Props) (0x7ab50e44, 0x2a2458f9, 0) */
		public static readonly DBPFTGI FSH_ANIM_NONPROPS; /** FSH file: Animation Sprites (Non Props) (0x7ab50e44, 0x49a593e7, 0) */
		public static readonly DBPFTGI FSH_TERRAIN_FOUNDATION; /** FSH file: Terrain And Foundations (0x7ab50e44, 0x891b0e1a, 0) */
		public static readonly DBPFTGI FSH_UI; /** FSH file: User Interface Images (0x7ab50e44, 0x46a006b0, 0) */
		public static readonly DBPFTGI FSH; /** FSH file: Textures (0x7ab50e44, 0, 0) */
		public static readonly DBPFTGI S3D_MAXIS; /** S3D file: Maxis Models (0x5ad0e817, 0xbadb57f1, 0) */
		public static readonly DBPFTGI S3D; /** S3D file: Models (0x5ad0e817, 0, 0) */
		public static readonly DBPFTGI SC4PATH_2D; /** SC4PATH (2D) (0x296678f7, 0x69668828, 0) */
		public static readonly DBPFTGI SC4PATH_3D; /** SC4PATH (3D) (0x296678f7, 0xa966883f, 0) */
		public static readonly DBPFTGI SC4PATH; /** SC4PATH file (0x296678f7, 0, 0) */
		public static readonly DBPFTGI LUA; /** LUA file: Missions, Advisors, Tutorials and Packaging files (0xca63e2a3, 0x4a5e8ef6, 0) */
		public static readonly DBPFTGI LUA_GEN; /** LUA file: Generators, Attractors, Repulsors and System LUA (0xca63e2a3, 0x4a5e8f3f, 0) */
		public static readonly DBPFTGI RUL; /** RUL file: Network rules (0x0a5bcf4b, 0xaa5bcf57, 0) */
		public static readonly DBPFTGI WAV; /** WAV file (0x2026960b, 0xaa4d1933, 0) */
		public static readonly DBPFTGI LTEXT; /** LTEXT or WAV file (0x2026960b, 0, 0) */
		public static readonly DBPFTGI EFFDIR; /** Effect Directory file (0xea5118b0, 0, 0) */
		public static readonly DBPFTGI INI_FONT; /** Font Table INI (0, 0x4a87bfe8, 0x2a87bffc) */
		public static readonly DBPFTGI INI_NETWORK; /** Network INI: Remapping, Bridge Exemplars (0, 0x8a5971c5, 0x8a5993b9) */
		public static readonly DBPFTGI INI; /** INI file (0, 0x8a5971c5, 0) */
		public static readonly DBPFTGI NULLTGI; /** NULLTGI (0, 0, 0) */



		private readonly uint? _type;
		public uint? type {
			get { return _type; }
			//set { myVar = value; }
		}
		private readonly uint? _group;
		public uint? group {
			get { return _group; }
			//set { myVar = value; }
		}
		private readonly uint? _instance;
		public uint? instance {
			get { return _instance; }
			//set { myVar = value; }
		}
		private readonly string _label;
		public string label {
			get { return _label; }
		}

		private string _labelshort;
		public string shortLabel {
			get { return _labelshort; }
		}

		//IMPORTANT - Never allow creation of null TID, GID, or IID because they interfere with the lookups of knownType
		public DBPFTGI(uint type, uint group, uint instance) {
			_type = type;
			_group = group;
			_instance = instance;
			//_label = MatchesAnyKnownTGI();
			//TODO - call matches here to set label field
		}

		/// <summary>
		/// Create a new DBPFTGI based on a known entry type.
		/// </summary>
		/// <remarks>
		/// If any component of the known entry is null the new component is set to 0.
		/// </remarks>
		/// <param name="existingType">Known entry type</param>
		public DBPFTGI(DBPFTGI knownEntry) {
			_type = knownEntry.type != null ? knownEntry.type : 0;
			_group = knownEntry.group != null ? knownEntry.group : 0;
			_instance = knownEntry.instance != null ? knownEntry.instance : 0;
			//_label = MatchesAnyKnownTGI();
		}


		/// <summary>
		/// Check if this DBPFTGI matches a specific known DBPFTGI entry type. Unlike equals, this method is not reflexive.
		/// </summary>
		/// <remarks>
		/// If any component of the provided DBPFTGI of knownType is null it will be skipped. This is opposed to Equals which explicitly checks every component.
		/// Only the provided DBPFTGI of knownType is checked for null components.
		/// </remarks>
		/// <param name="tgi">A DBPFTGI to check against</param>
		/// <returns>TRUE if check passes; FALSE otherwise</returns>
		public bool MatchesKnownTGI(DBPFTGI knownType) {
			bool isTIDok, isGIDok, isIIDok;

			if (!knownType.type.HasValue) {
				isTIDok = true;
			} else {
				isTIDok = this.type == knownType.type;
			}

			if (!knownType.group.HasValue) {
				isGIDok = true;
			} else {
				isGIDok = this.group == knownType.group;
			}

			if (!knownType.instance.HasValue) {
				isIIDok = true;
			} else {
				isIIDok = this.instance == knownType.instance;
			}

			return isTIDok && isGIDok && isIIDok;
		}


		/// <summary>
		/// Checks if this DBPFTGI matches any of the known DBPFTGI entry types.
		/// </summary>
		/// <returns>The label of the known entry type if found; null otherwise.</returns>
		public string MatchesAnyKnownTGI() {
			foreach (KeyValuePair<DBPFTGI, string> entry in knownEntries) {
				if (this.Equals(entry.Key)) {
					return entry.Value;
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
			if (obj is DBPFTGI) {
				DBPFTGI checkTGI = (DBPFTGI) obj;
				if (!(checkTGI.type is null)) {
					evalT = this.type == checkTGI.type;
				} else {
					evalT = true;
				}
				if (!(checkTGI.group is null)) {
					evalG = this.group == checkTGI.group;
				} else {
					evalG = true;
				}
				if (!(checkTGI.instance is null)) {
					evalI = this.instance == checkTGI.instance;
				} else {
					evalI = true;
				}
				return evalT && evalG && evalI;
				//return this.type == checkTGI.type && this.group == checkTGI.group && this.instance == checkTGI.instance;
			} else {
				return false;
			}

		}

		public override string ToString() {
			return $"T:{_type} 0x{DBPFUtil.UIntToHexString(_type, 8)}, G:{_group} 0x{DBPFUtil.UIntToHexString(_group, 8)}, I:{_instance} 0x{DBPFUtil.UIntToHexString(_instance, 8)}";
		}

		/// <summary>
		/// Returns a new DBPFTGI with the fields of this and the provided TGI.
		/// </summary>
		/// <remarks>
		/// Each component is replaced by the corresponding component of the modifier. If any modifier component is null, the original component is used instead.
		/// </remarks>
		/// <param name="modifier">Provided DBPFTGI to modify this DBPFTGI</param>
		/// <returns>A new DBPFTGI object with modified TGI components.</returns>
		public DBPFTGI ModifyTGI(DBPFTGI modifier) {
			//if modifier.type != null then use modifier.type else use this.type
			return new DBPFTGI(
				modifier.type != null ? (uint) modifier.type : (uint) this.type,
				modifier.group != null ? (uint) modifier.group : (uint) this.group,
				modifier.instance != null ? (uint) modifier.instance : (uint) this.instance
			);
		}

		/// <summary>
		/// Returns a new DBPFTGI with the specified TGI components.
		/// </summary>
		/// <remarks>
		/// If any provided value is null it will be skipped and the existing component is used instead.
		/// </remarks>
		/// <param name="t">Specified type value</param>
		/// <param name="g">Specified group value</param>
		/// <param name="i">Specified instance value</param>
		/// <returns>A new DBPFTGI object with specified TGI components.</returns>
		public DBPFTGI ModifyTGI(uint? t, uint? g, uint? i) {
			//if t != null then use t else use this.type
			return new DBPFTGI(
				t != null ? (uint) t : (uint) this.type,
				g != null ? (uint) g : (uint) this.group,
				i != null ? (uint) i : (uint) this.instance
			);
		}


		/// <summary>
		/// This static constructor will be called as soon as the class is loaded into memory, and not necessarily when an object is created.
		/// Known types need to be ordered "bottom-up", that is, specialized entries need to be inserted first, more general ones later.
		/// </summary>
		static DBPFTGI() {
			BLANKTGI = new DBPFTGI(0, 0, 0, "-");
			DIRECTORY = new DBPFTGI(0xe86b1eef, 0xe86b1eef, 0x286b1f03, "DIR");
			LD = new DBPFTGI(0x6be74c60, 0x6be74c60, null, "LD");
			S3D_MAXIS = new DBPFTGI(0x5ad0e817, 0xbadb57f1, null, "S3D");
			S3D = new DBPFTGI(0x5ad0e817, null, null, "S3D");
			COHORT = new DBPFTGI(0x05342861, null, null, "COHORT");

			EXEMPLAR_ROAD = new DBPFTGI(0x6534284a, 0x2821ed93, null, "EXEMPLAR (Road)");
			EXEMPLAR_STREET = new DBPFTGI(0x6534284a, 0xa92a02ea, null, "EXEMPLAR (Street)");
			EXEMPLAR_ONEWAYROAD = new DBPFTGI(0x6534284a, 0xcbe084cb, null, "EXEMPLAR (One-Way Road)");
			EXEMPLAR_AVENUE = new DBPFTGI(0x6534284a, 0xcb730fac, null, "EXEMPLAR (Avenue)");
			EXEMPLAR_HIGHWAY = new DBPFTGI(0x6534284a, 0xa8434037, null, "EXEMPLAR (Highway)");
			EXEMPLAR_GROUNDHIGHWAY = new DBPFTGI(0x6534284a, 0xebe084d1, null, "EXEMPLAR (Ground Highway)");
			EXEMPLAR_DIRTROAD = new DBPFTGI(0x6534284a, 0x6be08658, null, "EXEMPLAR (Dirtroad)");
			EXEMPLAR_RAIL = new DBPFTGI(0x6534284a, 0xe8347989, null, "EXEMPLAR (Rail)");
			EXEMPLAR_LIGHTRAIL = new DBPFTGI(0x6534284a, 0x2b79dffb, null, "EXEMPLAR (Lightrail)");
			EXEMPLAR_MONORAIL = new DBPFTGI(0x6534284a, 0xebe084c2, null, "EXEMPLAR (Monorail)");
			EXEMPLAR_POWERPOLE = new DBPFTGI(0x6534284a, 0x088e1962, null, "EXEMPLAR (Power Pole)");
			EXEMPLAR_T21 = new DBPFTGI(0x6534284a, 0x89ac5643, null, "EXEMPLAR (T21)");
			EXEMPLAR = new DBPFTGI(0x6534284a, null, null, "EXEMPLAR");

			FSH_MISC = new DBPFTGI(0x7ab50e44, 0x1abe787d, null, "FSH (Misc)");
			FSH_TRANSIT = new DBPFTGI(0x7ab50e44, 0x1abe787d, null, "FSH (Misc)");
			FSH_BASE_OVERLAY = new DBPFTGI(0x7ab50e44, 0x0986135e, null, "FSH (Base/Overlay Texture)");
			FSH_SHADOW = new DBPFTGI(0x7ab50e44, 0x2bC2759a, null, "FSH (Shadow DBPFTGI)");
			FSH_ANIM_PROPS = new DBPFTGI(0x7ab50e44, 0x2a2458f9, null, "FSH (Animation Sprites (Props)");
			FSH_ANIM_NONPROPS = new DBPFTGI(0x7ab50e44, 0x49a593e7, null, "FSH (Animation Sprites (Non Props)");
			FSH_TERRAIN_FOUNDATION = new DBPFTGI(0x7ab50e44, 0x891b0e1a, null, "FSH (Terrain/Foundation)");
			FSH_UI = new DBPFTGI(0x7ab50e44, 0x46a006b0, null, "FSH (UI Image)");
			FSH = new DBPFTGI(0x7ab50e44, null, null, "FSH");

			SC4PATH_2D = new DBPFTGI(0x296678f7, 0x69668828, null, "SC4PATH (2D)");
			SC4PATH_3D = new DBPFTGI(0x296678f7, 0xa966883f, null, "SC4PATH (3D)");
			SC4PATH = new DBPFTGI(0x296678f7, null, null, "SC4PATH");

			PNG_ICON = new DBPFTGI(0x856ddbac, 0x6a386d26, null, "PNG (Icon)");
			PNG = new DBPFTGI(0x856ddbac, null, null, "PNG");
			LUA = new DBPFTGI(0xca63e2a3, 0x4a5e8ef6, null, "LUA");
			LUA_GEN = new DBPFTGI(0xca63e2a3, 0x4a5e8f3f, null, "LUA (Generators)");
			WAV = new DBPFTGI(0x2026960b, 0xaa4d1933, null, "WAV");
			LTEXT = new DBPFTGI(0x2026960b, null, null, "LTEXT");
			INI_FONT = new DBPFTGI(0, 0x4a87bfe8, 0x2a87bffc, "INI (Font Table)");
			INI_NETWORK = new DBPFTGI(0, 0x8a5971c5, 0x8a5993b9, "INI (Networks)");
			INI = new DBPFTGI(0, 0x8a5971c5, null, "INI");
			RUL = new DBPFTGI(0x0a5bcf4b, 0xaa5bcf57, null, "RUL");
			EFFDIR = new DBPFTGI(0xea5118b0, null, null, "EFFDIR");
			NULLTGI = new DBPFTGI(null, null, null, "UNKNOWN");

			knownEntries.Add(BLANKTGI, "BLANKTGI");
			knownEntries.Add(DIRECTORY, "DIRECTORY");
			knownEntries.Add(LD, "LD");
			knownEntries.Add(S3D_MAXIS, "S3D_MAXIS");
			knownEntries.Add(S3D, "S3D");
			knownEntries.Add(COHORT, "COHORT");
			knownEntries.Add(EXEMPLAR_ROAD, "EXEMPLAR_ROAD");
			knownEntries.Add(EXEMPLAR_STREET, "EXEMPLAR_STREET");
			knownEntries.Add(EXEMPLAR_ONEWAYROAD, "EXEMPLAR_ONEWAYROAD");
			knownEntries.Add(EXEMPLAR_AVENUE, "EXEMPLAR_AVENUE");
			knownEntries.Add(EXEMPLAR_HIGHWAY, "EXEMPLAR_HIGHWAY");
			knownEntries.Add(EXEMPLAR_GROUNDHIGHWAY, "EXEMPLAR_GROUNDHIGHWAY");
			knownEntries.Add(EXEMPLAR_DIRTROAD, "EXEMPLAR_DIRTROAD");
			knownEntries.Add(EXEMPLAR_RAIL, "EXEMPLAR_RAIL");
			knownEntries.Add(EXEMPLAR_LIGHTRAIL, "EXEMPLAR_LIGHTRAIL");
			knownEntries.Add(EXEMPLAR_MONORAIL, "EXEMPLAR_MONORAIL");
			knownEntries.Add(EXEMPLAR_POWERPOLE, "EXEMPLAR_POWERPOLE");
			knownEntries.Add(EXEMPLAR_T21, "EXEMPLAR_T21");
			knownEntries.Add(EXEMPLAR, "EXEMPLAR");
			knownEntries.Add(FSH_MISC, "FSH_MISC");
			knownEntries.Add(FSH_TRANSIT, "FSH_TRANSIT");
			knownEntries.Add(FSH_BASE_OVERLAY, "FSH_BASE_OVERLAY");
			knownEntries.Add(FSH_SHADOW, "FSH_SHADOW");
			knownEntries.Add(FSH_ANIM_PROPS, "FSH_ANIM_PROPS");
			knownEntries.Add(FSH_ANIM_NONPROPS, "FSH_ANIM_NONPROPS");
			knownEntries.Add(FSH_TERRAIN_FOUNDATION, "FSH_TERRAIN_FOUNDATION");
			knownEntries.Add(FSH_UI, "FSH_UI");
			knownEntries.Add(FSH, "FSH");
			knownEntries.Add(SC4PATH_2D, "SC4PATH_2D");
			knownEntries.Add(SC4PATH_3D, "SC4PATH_3D");
			knownEntries.Add(SC4PATH, "SC4PATH");
			knownEntries.Add(PNG_ICON, "PNG_ICON");
			knownEntries.Add(PNG, "PNG");
			knownEntries.Add(LUA, "LUA");
			knownEntries.Add(LUA_GEN, "LUA_GEN");
			knownEntries.Add(WAV, "WAV");
			knownEntries.Add(LTEXT, "LTEXT");
			knownEntries.Add(INI_FONT, "INI_FONT");
			knownEntries.Add(INI_NETWORK, "INI_NETWORK");
			knownEntries.Add(INI, "INI");
			knownEntries.Add(RUL, "RUL");
			knownEntries.Add(EFFDIR, "EFFDIR");
			knownEntries.Add(NULLTGI, "NULLTGI"); // NULLTGI matches with everything
		}

		/// <summary>
		/// This constructor only to be used internally to this class to declare known TGI types in the static constructor.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="group"></param>
		/// <param name="instance"></param>
		/// <param name="label"></param>
		private DBPFTGI(uint? type, uint? group, uint? instance, string label) {
			_type = type;
			_group = group;
			_instance = instance;
			_label = label;
		}

	}

}

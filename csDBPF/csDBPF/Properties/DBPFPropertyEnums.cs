using System;
using System.Collections.Generic;
using System.Text;


namespace csDBPF.Properties {

	public abstract partial class DBPFProperty {
		/// <summary>
		/// The Exemplar Type is one of the properties of an exemplar, and is used for grouping exemplars into similar categories. See <see ref="https://www.wiki.sc4devotion.com/index.php?title=Exemplar#Exemplar_Types"/>.
		/// </summary>
		public enum ExemplarTypes {
			T00,
			Tuning,
			Building,
			RCI,
			Developer,
			Simulator,
			Road,
			Bridge,
			MiscNetwork,
			Unknown,
			Rail,
			Highway,
			PowerLine,
			Terrain,
			Ordinance,
			FloraFauna,
			LotConfiguration,
			Foundation,
			Lighting,
			LotRetianingWall,
			Vehicle,
			Pedestrian,
			Aircraft,
			Prop,
			Construction,
			AutomataTuning,
			NetworkLot,
			Disaster,
			DataView,
			Crime,
			Audio,
			GodMode,
			MayorMode,
			TrendBar,
			GraphControl
		}

		/// <summary>
		/// The first rep (0) of a LotConfigPropertyLotObject (LCPLO) describes its type. See <see ref="https://www.wiki.sc4devotion.com/index.php?title=LotConfigPropertyLotObject#Specification"/>.
		/// </summary>
		public enum LotConfigPropertyLotObjectTypes {
			Building,
			Prop,
			Texture,
			[Obsolete("Not implemented in SC4.")]
			Fence,
			Flora,
			WaterConstraint,
			LandConstraint,
			NetworkNode
		}


		/// <summary>
		/// Defines the RCI type for a lot. See <see ref="https://www.wiki.sc4devotion.com/index.php?title=Exemplar_properties#Lot_Configuration"/>.
		/// </summary>
		public enum LotConfigPropertyPurposeTypes {
			None = 0,
			Residential = 1,
			CommercialService = 2,
			CommercialOffice = 3,
			Agriculture = 5,
			IndustryDirty = 6,
			IndustryMfg = 7,
			IndustryHighTech = 8
		}

		/// <summary>
		/// Defines the wealth for a lot. See <seecref="https://www.wiki.sc4devotion.com/index.php?title=Exemplar_properties#Lot_Configuration"/>.
		/// </summary>
		public enum LotConfigPropertyWealthTypes {
			None,
			Low,
			Medium,
			High
		}

		/// <summary>
		/// Defines the zone type for a lot (what type of zones the lot will grow on). See <see ref="https://www.wiki.sc4devotion.com/index.php?title=Exemplar_properties#Lot_Configuration"/>.
		/// </summary>
		public enum LotConfigPropertyZoneTypes {
			None,
			LowDensityR,
			MediumDensityR,
			HighDensityR,
			LowDensityC,
			MediumDensityC,
			HighDensityC,
			Agriculture,
			MediumDensityI,
			HighDensityI,
			Military,
			Airport,
			Seaport,
			SpacePort,
			Landmark,
			CivicPlopped
		}

		/// <summary>
		/// Defines the road orientation for a lot. See <see ref="https://www.wiki.sc4devotion.com/index.php?title=Exemplar_properties#Lot_Configuration"/>.
		/// </summary>
		public enum LotConfigPropertyRoadRequirement {
			Normal = 0x08,
			LeftCorner = 0x09,
			RightCorner = 0x0C
		}
	}
}

using System;


namespace csDBPF.Properties {

	public abstract partial class DBPFProperty {
        /// <summary>
        /// Defines the <see href="https://www.wiki.sc4devotion.com/index.php?title=Exemplar#Exemplar_Types">type of exemplar</see> and what game systems or components the exemplar contains info for.
        /// </summary>
        public enum ExemplarTypes {
			/// <summary>
			/// The use of T00 exemplars is relatively unknown. 
			/// </summary>
			T00,
			/// <summary>
			/// Tuning Exemplars. These control various game properties such as transit network slopes. 
			/// </summary>
			Tuning,
			/// <summary>
			/// Building Exemplars. These contain properties related to buildings. 
			/// </summary>
			Building,
			/// <summary>
			/// RCI Exemplars. 
			/// </summary>
			RCI,
			/// <summary>
			/// Developer Exemplars. 
			/// </summary>
			Developer,
			/// <summary>
			/// Simulator Exemplars. These are among the most critical to game functionality, and include among them the Demand Simulator and the Traffic Simulator. 
			/// </summary>
			Simulator,
			/// <summary>
			/// Road Exemplars. These are generally used for tunnel entrance models. 
			/// </summary>
			Road,
			/// <summary>
			/// Bridge Exemplars. These are used to specify bridge properties. 
			/// </summary>
			Bridge,
			/// <summary>
			/// Misc Network Exemplars.
			/// </summary>
			MiscNetwork,
			/// <summary>
			/// Unknown.
			/// </summary>
			Unknown,
			/// <summary>
			/// Rail Exemplars.
			/// </summary>
			Rail,
			/// <summary>
			/// Highway Exemplars, used to reference transit models. They are among the most important exemplars used in the transit-modding world. Virtually all NAM Puzzle Pieces, as well as draggable model-based functionality, as exemplified by the High Speed Rail mod, use Type 0B exemplars. 
			/// </summary>
			Highway,
			/// <summary>
			/// Power line Exemplars.
			/// </summary>
			PowerLine,
			/// <summary>
			/// Terrain Exemplars.
			/// </summary>
			Terrain,
			/// <summary>
			/// Ordinance Exemplars.
			/// </summary>
			Ordinance,
			/// <summary>
			/// Flora/Fauna Exemplars.
			/// </summary>
			FloraFauna,
			/// <summary>
			/// LotConfiguration and Building Exemplars, and are used to specify the location of props, textures and buildings on Lots, along with other properties. Type 10 Exemplars for plopped buildings are stored in a city's Savefile upon save.
			/// </summary>
			LotConfiguration,
			/// <summary>
			/// Foundation Exemplars.
			/// </summary>
			Foundation,
			/// <summary>
			/// Lighting Exemplars.
			/// </summary>
			Lighting,
			/// <summary>
			/// LotRetainingWall Exemplars.
			/// </summary>
			LotRetianingWall,
			/// <summary>
			/// Vehicle Exemplars. Used to reference automata.
			/// </summary>
			Vehicle,
			/// <summary>
			/// Pedestrian Exemplars. Used to reference pedestrian automata.
			/// </summary>
			Pedestrian,
			/// <summary>
			/// Aircraft Exemplars. Used to reference aircraft automata
			/// </summary>
			Aircraft,
			/// <summary>
			/// Prop Exemplars.
			/// </summary>
			Prop,
			/// <summary>
			/// Construction Exemplars.
			/// </summary>
			Construction,
			/// <summary>
			/// Automata Tuning Exemplars.
			/// </summary>
			AutomataTuning,
			/// <summary>
			/// Network Lots, often referred to as T21 Exemplars. Used to add props rendered in BAT (isometric) to networks, such as traffic lights and lamp posts. These exemplars differ from Lot Exemplars (T10) in that they don't allow for base or overlay textures and can't be Transit Enabled.
			/// </summary>
			NetworkLot,
			/// <summary>
			/// Disaster Exemplars
			/// </summary>
			Disaster,
			/// <summary>
			/// DataView Exemplars.
			/// </summary>
			DataView,
			/// <summary>
			/// Crime Exemplars.
			/// </summary>
			Crime,
			/// <summary>
			/// Audio Exemplars.
			/// </summary>
			Audio,
			/// <summary>
			/// God Mode Exemplars.
			/// </summary>
			GodMode,
			/// <summary>
			/// Mayor Mode Exemplars.
			/// </summary>
			MayorMode,
			/// <summary>
			/// Trend Bar Exemplars.
			/// </summary>
			TrendBar,
			/// <summary>
			///  	Graph Control Exemplars.
			/// </summary>
			GraphControl
		}


        /// <summary>
        /// The first rep (0) of a LotConfigPropertyLotObject describes its <see href="https://www.wiki.sc4devotion.com/index.php?title=LotConfigPropertyLotObject#Specification">object type</see>.
        /// </summary>
        public enum LotConfigPropertyLotObjectTypes {
			/// <summary>
			/// Defines position and IID reference of building exemplar.
			/// </summary>
			Building,
			/// <summary>
			/// Defines position and IID reference of a prop exemplar.
			/// </summary>
			Prop,
			/// <summary>
			/// Defines position and IID reference of a base or overlay texture.
			/// </summary>
			Texture,
			/// <summary>
			/// Not implemented currently.
			/// </summary>
			[Obsolete("Not implemented in SC4.")]
			Fence,
			/// <summary>
			/// Defines position, and IID reference of a flora/growable tree exemplar.
			/// </summary>
			Flora,
			/// <summary>
			/// Defines a water constraint tile.
			/// </summary>
			WaterConstraint,
			/// <summary>
			/// Defines a land constraint tile.
			/// </summary>
			LandConstraint,
			/// <summary>
			/// Defines transit connections and automata paths.
			/// </summary>
			NetworkNode
		}


        /// <summary>
        /// Defines the <see href="https://www.wiki.sc4devotion.com/index.php?title=Exemplar_properties#Lot_Configuration">RCI type</see> for a lot.
        /// </summary>
        public enum LotConfigPropertyPurposeTypes {
			/// <summary>
			/// None.
			/// </summary>
			None = 0,
			/// <summary>
			/// Residential (R).
			/// </summary>
			Residential = 1,
			/// <summary>
			/// Commercial Service (CS).
			/// </summary>
			CommercialService = 2,
			/// <summary>
			/// Commercial Office (CO)
			/// </summary>
			CommercialOffice = 3,
			/// <summary>
			/// Agriculture (AG)
			/// </summary>
			Agriculture = 5,
			/// <summary>
			/// Industrial Dirty (ID)
			/// </summary>
			IndustryDirty = 6,
			/// <summary>
			/// Industrial Manufacturing (IM)
			/// </summary>
			IndustryMfg = 7,
			/// <summary>
			/// Industial High Tech (IHT)
			/// </summary>
			IndustryHighTech = 8
		}


        /// <summary>
        /// Defines the <see href="https://www.wiki.sc4devotion.com/index.php?title=Exemplar_properties#Lot_Configuration">wealth</see> for a lot.
        /// </summary>
        public enum LotConfigPropertyWealthTypes {
			/// <summary>
			/// No wealth (civic, landmark, etc.).
			/// </summary>
			None,
			/// <summary>
			/// Low Wealth ($)
			/// </summary>
			Low,
			/// <summary>
			/// Medium Wealth ($$)
			/// </summary>
			Medium,
			/// <summary>
			/// High Wealth ($$$)
			/// </summary>
			High
		}


        /// <summary>
        /// Defines the <see href="https://www.wiki.sc4devotion.com/index.php?title=Exemplar_properties#Lot_Configuration">zone type</see> for a lot - which type of zones the lot will grow on or was plopped as.
        /// </summary>
        public enum LotConfigPropertyZoneTypes {
			/// <summary>
			/// None
			/// </summary>
			None,
			/// <summary>
			/// Low density residential zones.
			/// </summary>
			LowDensityR,
			/// <summary>
			/// Medium density residential zones.
			/// </summary>
			MediumDensityR,
			/// <summary>
			/// High density residential zones.
			/// </summary>
			HighDensityR,
			/// <summary>
			/// Low density commercial zones.
			/// </summary>
			LowDensityC,
			/// <summary>
			/// Medium density commercial zones.
			/// </summary>
			MediumDensityC,
			/// <summary>
			/// High density commercial zones.
			/// </summary>
			HighDensityC,
			/// <summary>
			/// Agricultrual zones.
			/// </summary>
			Agriculture,
			/// <summary>
			/// Medium density industrial zones.
			/// </summary>
			MediumDensityI,
			/// <summary>
			/// High density industrial zones.
			/// </summary>
			HighDensityI,
			/// <summary>
			/// Military plopped.
			/// </summary>
			Military,
			/// <summary>
			/// Airport plopped.
			/// </summary>
			Airport,
			/// <summary>
			/// Seaport plopped.
			/// </summary>
			Seaport,
			/// <summary>
			/// Spaceport plopped.
			/// </summary>
			SpacePort,
			/// <summary>
			/// Landmark plopped.
			/// </summary>
			Landmark,
			/// <summary>
			/// Civic plopped.
			/// </summary>
			CivicPlopped
		}


        /// <summary>
        /// Defines the <see href="https://www.wiki.sc4devotion.com/index.php?title=Exemplar_properties#Lot_Configuration">road orientation</see> for a lot.
        /// </summary>
        public enum LotConfigPropertyRoadRequirement {
			/// <summary>
			/// Front road requirement.
			/// </summary>
			Normal = 0x08,
			/// <summary>
			/// Front and left road requirement.
			/// </summary>
			LeftCorner = 0x09,
			/// <summary>
			/// Front and right road requirement.
			/// </summary>
			RightCorner = 0x0C
		}
	}
}

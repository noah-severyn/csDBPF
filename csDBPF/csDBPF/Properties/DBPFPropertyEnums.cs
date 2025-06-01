using System;


namespace csDBPF {

	public abstract partial class DBPFProperty {
        /// <summary>
        /// Defines the <see href="https://www.wiki.sc4devotion.com/index.php?title=Exemplar#Exemplar_Types">type of exemplar</see> and what game systems or components the exemplar contains info for.
        /// </summary>
        public enum ExemplarType {
			/// <summary>
			/// Error reading type or type is not present.
			/// </summary>
			Error = -1,
			/// <summary>
			/// The use of T00 exemplars is relatively unknown. 
			/// </summary>
			T00 = 0x00,
            /// <summary>
            /// Tuning Exemplars. These control various game properties such as transit network slopes. 
            /// </summary>
            Tuning = 0x01,
            /// <summary>
            /// Building Exemplars. These contain properties related to buildings. 
            /// </summary>
            Building = 0x02,
            /// <summary>
            /// RCI Exemplars. 
            /// </summary>
            RCI = 0x03,
            /// <summary>
            /// Developer Exemplars. 
            /// </summary>
            Developer = 0x04,
            /// <summary>
            /// Simulator Exemplars. These are among the most critical to game functionality, and include among them the Demand Simulator and the Traffic Simulator. 
            /// </summary>
            Simulator = 0x05,
            /// <summary>
            /// Road Exemplars. These are generally used for tunnel entrance models. 
            /// </summary>
            Road = 0x06,
            /// <summary>
            /// Bridge Exemplars. These are used to specify bridge properties. 
            /// </summary>
            Bridge = 0x07,
            /// <summary>
            /// Misc Network Exemplars.
            /// </summary>
            MiscNetwork = 0x08,
            /// <summary>
            /// Unknown type.
            /// </summary>
            UnknownType = 0x09,
            /// <summary>
            /// Rail Exemplars.
            /// </summary>
            Rail = 0x0A,
            /// <summary>
            /// Highway Exemplars, used to reference transit models. They are among the most important exemplars used in the transit-modding world. Virtually all NAM Puzzle Pieces, as well as draggable model-based functionality, as exemplified by the High Speed Rail mod, use Type 0B exemplars. 
            /// </summary>
            Highway = 0x0B,
            /// <summary>
            /// Power line Exemplars.
            /// </summary>
            PowerLine = 0x0C,
            /// <summary>
            /// Terrain Exemplars.
            /// </summary>
            Terrain = 0x0D,
            /// <summary>
            /// Ordinance Exemplars.
            /// </summary>
            Ordinance = 0x0E,
            /// <summary>
            /// Flora/Fauna Exemplars.
            /// </summary>
            FloraFauna = 0x0F,
            /// <summary>
            /// LotConfiguration and Building Exemplars, and are used to specify the location of props, textures and buildings on Lots, along with other properties. Type 10 Exemplars for plopped buildings are stored in a city's Savefile upon save.
            /// </summary>
            LotConfiguration = 0x10,
            /// <summary>
            /// Foundation Exemplars.
            /// </summary>
            Foundation = 0x11,
            /// <summary>
            /// Lighting Exemplars.
            /// </summary>
            Lighting = 0x13,
            /// <summary>
            /// LotRetainingWall Exemplars.
            /// </summary>
            LotRetianingWall = 0x15,
            /// <summary>
            /// Vehicle Exemplars. Used to reference automata.
            /// </summary>
            Vehicle = 0x16,
            /// <summary>
            /// Pedestrian Exemplars. Used to reference pedestrian automata.
            /// </summary>
            Pedestrian = 0x17,
            /// <summary>
            /// Aircraft Exemplars. Used to reference aircraft automata
            /// </summary>
            Aircraft = 0x18,
            /// <summary>
            /// Prop Exemplars.
            /// </summary>
            Prop = 0x1E,
            /// <summary>
            /// Construction Exemplars.
            /// </summary>
            Construction = 0x1F,
            /// <summary>
            /// Automata Tuning Exemplars.
            /// </summary>
            AutomataTuning = 0x20,
            /// <summary>
            /// Network Lots, often referred to as T21 Exemplars. Used to add props rendered in BAT (isometric) to networks, such as traffic lights and lamp posts. These exemplars differ from Lot Exemplars (T10) in that they don't allow for base or overlay textures and can't be Transit Enabled.
            /// </summary>
            NetworkLot = 0x21,
            /// <summary>
            /// Disaster Exemplars
            /// </summary>
            Disaster = 0x22,
            /// <summary>
            /// DataView Exemplars.
            /// </summary>
            DataView = 0x23,
            /// <summary>
            /// Crime Exemplars.
            /// </summary>
            Crime = 0x24,
            /// <summary>
            /// Audio Exemplars.
            /// </summary>
            Audio = 0x25,
            /// <summary>
            /// God Mode Exemplars.
            /// </summary>
            GodMode = 0x27,
            /// <summary>
            /// Mayor Mode Exemplars.
            /// </summary>
            MayorMode = 0x28,
            /// <summary>
            /// Trend Bar Exemplars.
            /// </summary>
            TrendBar = 0x2A,
            /// <summary>
            ///  	Graph Control Exemplars.
            /// </summary>
            GraphControl = 0x2B
        }


        /// <summary>
        /// The first rep (0) of a LotConfigPropertyLotObject describes its <see href="https://www.wiki.sc4devotion.com/index.php?title=LotConfigPropertyLotObject#Specification">object type</see>.
        /// </summary>
        public enum LotConfigPropertyLotObjectType {
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
        public enum LotConfigPropertyPurposeType {
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
        public enum LotConfigPropertyWealthType {
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
        public enum LotConfigPropertyZoneType {
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Linq;

namespace PSSSupporter {
    public class vessel {
        public static int maxMapX = 30;
        public static int maxMapY = 30;
        //public Dictionary<string, object> properties = new Dictionary<string, object>();
        List<character> _characters = new List<character>();
        List<room> _rooms = new List<room>();
        List<item> _items = new List<item>();
        List<lift> _lifts = new List<lift>();
        public List<room>[,] _shipMap = new List<room>[50,50];
        public Dictionary<string,List<room>> _roomsByCategories = new Dictionary<string,List<room>>();

        public shipDesign _shipDesign = null;
        public int ShipId, ShipDesignId, Hp, StandardCharacterDraws, UniqueCharacterDraws
            , UserId, AllianceId, OriginalRaceId, UpgradeShipDesignId, Shield;
        public float SaturationValue, BrightnessValue, HueValue;
        public string ShipStatus, ShipName;
        public DateTime UpdateDate, StatusStartDate, UpgradeStartDate, ImmunityDate;
        public static vessel Dictionary2Vessel(PSSEngine engine,Dictionary<string,object> dict) {
            vessel vessel = new vessel();
            initVesselFromDict(engine,vessel,(Dictionary<string,object>)dict["Ship"]);
            return vessel;
        }
        public vessel() {
        }
        public vessel(PSSEngine engine,Dictionary<string,object> dict) {
            initVesselFromDict(engine,this,dict);
        }
        public Dictionary<int,room> _roomIndex = new Dictionary<int,room>();
        public Dictionary<int,item> _itemIndex = new Dictionary<int,item>();
        public Dictionary<int,character> _characterIndex = new Dictionary<int,character>();
        private static void initVesselFromDict(PSSEngine engine,vessel vessel,Dictionary<string,object> dict) {
            Helpers.DictionaryToAttributes(dict,vessel);
            if (dict.Keys.Contains("Rooms")) {
                vessel._rooms = room.Dictionary2Rooms(engine,(Dictionary<string,object>)dict["Rooms"]);
                foreach (room room in vessel._rooms) {
                    vessel._roomIndex.Add(room.RoomId,room);
                    if (room._roomDesign != null) {
                        for (int x = 0; x < room._roomDesign.Columns; x++) {
                            for (int y = 0; y < room._roomDesign.Rows; y++) {
                                if (vessel._shipMap[room.Column + x,room.Row + y] == null) {
                                    vessel._shipMap[room.Column + x,room.Row + y] = new List<room>();
                                } else {
                                    //WHOOPS
                                }
                                vessel._shipMap[room.Column + x,room.Row + y].Add(room);
                            }
                        }
                        if (!vessel._roomsByCategories.Keys.Contains(room._roomDesign.RoomType)) {
                            vessel._roomsByCategories.Add(room._roomDesign.RoomType,new List<room>());
                        }
                        vessel._roomsByCategories[room._roomDesign.RoomType].Add(room);
                    } else {
                        //WHOOPS!!!
                    }
                }
                foreach (room aRoom in vessel._rooms) {
                    if (aRoom._roomDesign.RoomType == "Wall") {
                        int armorBonus = aRoom._roomDesign.Capacity;
                        if (aRoom.Column > 0 && vessel._shipMap[aRoom.Column - 1,aRoom.Row] != null) {
                            foreach (room room in vessel._shipMap[aRoom.Column - 1,aRoom.Row]) {
                                room.TotalDefense += armorBonus;
                            }
                        }
                        if (aRoom.Column < maxMapX - 1 && vessel._shipMap[aRoom.Column + 1,aRoom.Row] != null) {
                            foreach (room room in vessel._shipMap[aRoom.Column + 1,aRoom.Row]) {
                                room.TotalDefense += armorBonus;
                            }
                        }
                        if (aRoom.Row > 0 && vessel._shipMap[aRoom.Column,aRoom.Row - 1] != null) {
                            foreach (room room in vessel._shipMap[aRoom.Column,aRoom.Row - 1]) {
                                room.TotalDefense += armorBonus;
                            }
                        }
                        if (aRoom.Row < maxMapY - 1 && vessel._shipMap[aRoom.Column,aRoom.Row + 1] != null) {
                            foreach (room room in vessel._shipMap[aRoom.Column,aRoom.Row + 1]) {
                                room.TotalDefense += armorBonus;
                            }
                        }
                    }
                }
            }
            if (dict.Keys.Contains("Items")) {
                vessel._items = item.Dictionary2Items(engine,(Dictionary<string,object>)dict["Items"]);
                foreach (item item in vessel._items) {
                    vessel._itemIndex.Add(item.ItemId,item);
                }
            }
            if (dict.Keys.Contains("Characters")) {
                vessel._characters = character.Dictionary2characters(engine,(Dictionary<string,object>)dict["Characters"]);
                foreach (character character in vessel._characters) {
                    vessel._characterIndex.Add(character.CharacterId,character);
                    if (vessel._roomIndex.Keys.Contains(character.RoomId)) {
                        vessel._roomIndex[character.RoomId]._characters.Add(character);
                    }
                }
            }
            if (dict.Keys.Contains("Lift")) {
                vessel._lifts = lift.Dictionary2Lifts((Dictionary<string,object>)dict["Lift"]);
                //[.........]
            }
            vessel._shipDesign = engine.shipDesigns[vessel.ShipDesignId];
        }
        /*public object this[string index] {
            get {
                return properties[index];
            }
        }
        public int getInt(string index) {
            int result;
            int.TryParse((string)properties[index], out result);
            return result;
        }*/
        public bool isTargetAffordable(autoCombatProcessStatus status,combatRequestReply combat) {
            if (status.farming && !status.diveLookingForNiceTarget) {
                if (status.diveProcess && combat.LoseTrophyResult > 50) {
                    return true;
                } else if (status.diveProcess) {
                    Console.Write("Ltl");
                    return false;
                }

                if (!status.diveProcess && combat.WinTrophyResult > 5) {
                    Console.Write("Htw");
                    return false;
                }
                //if (_shipDesign.ShipLevel > 9) {
                //    return true;
                //}
            }
            if (status.diveProcess) {
                return true;
            }
            if (_shipDesign.ShipLevel <= 8) {
                Console.Write("Low" + _shipDesign.ShipLevel);
                return true;
            }
            user user = combat.DefendingUser;
            //Turtle ships...
            if ((!_roomsByCategories.Keys.Contains("Laser") || _roomsByCategories["Laser"].Count() == 0) && _roomsByCategories["Bedroom"].Count() < 3) {
                Console.Write("Ptur");
                return false; //dirty... could check middle room defense or something more elaborated.
            }
            double crewCombatPowerRatioCalculation = CrewCombatPowerRatioCalculation();
            bool dangerousCrew = crewCombatPowerRatioCalculation >= 50;
            bool armoredTeleport = false, defendedTeleport = false, lowArmoredTeleport = true;
            if (_roomsByCategories.Keys.Contains("Teleport") && _roomsByCategories["Teleport"].Count > 0) {
                room teleport = _roomsByCategories["Teleport"][0];
                armoredTeleport = (teleport._roomDesign.Level > 6) && teleport.TotalDefense > 28;
                lowArmoredTeleport = teleport.TotalDefense < 28;
                foreach (character character in teleport._characters) {
                    if (character._characterDesign.SpecialAbilityType == "Poison Gas" || character._characterDesign.SpecialAbilityType == "Critical Strike") {
                        if ((double)character._characterDesign.SpecialAbilityFinalArgument
                            * (double)1 + ((double)character.AbilityImprovement / (double)100) >= 10) {
                            defendedTeleport = true;
                        }
                    }
                }
            }
            Console.Write(_shipDesign.ShipLevel + (lowArmoredTeleport? "Lat": "") + (defendedTeleport ? "Dt" : "")
                + (armoredTeleport ? "Hat" : "") + (dangerousCrew ? "Dc" : "") + crewCombatPowerRatioCalculation);
            if (_shipDesign.ShipLevel <= 8) {
                return true;
            }
            if (_shipDesign.ShipLevel <= 9 && (lowArmoredTeleport || !defendedTeleport)) {
                return true;
            }
            if (_shipDesign.ShipLevel >= 9 && (armoredTeleport && defendedTeleport || dangerousCrew)) {
                return false;
            }
            if (_shipDesign.ShipLevel >= 9 && !defendedTeleport && !dangerousCrew) {
                return true;
            }
            return user.Trophy > 1 && DateTime.Now - user.LastLoginDate > TimeSpan.FromDays(20);
        }

        public double CrewCombatPowerRatioCalculation() {
            Dictionary<string,double> rarityToDouble = new Dictionary<string,double>();
            rarityToDouble.Add("Common",1); rarityToDouble.Add("Elite",2); rarityToDouble.Add("Unique",3);
            rarityToDouble.Add("Epic",4); rarityToDouble.Add("Hero",5); rarityToDouble.Add("Legendary",6);
            rarityToDouble.Add("Special",3.5);
            string[] betterThanTierCharacters = new string[5] { "Zhuge Liang","Zombieee","Zombie","Meowy Cat","Roach" };
            //('Common' => 1, 'Elite' => 2, 'Unique' => 3, 'Epic' => 4, 'Hero' => 5, 'Legendary' => 6, 'Special' => 3.5);

            double crewCombatPowerRatio = 0;
            foreach (character character in _characterIndex.Values) {
                double rarity = rarityToDouble[character._characterDesign.Rarity];
                if (betterThanTierCharacters.Contains(character.CharacterName)) {
                    rarity += 1;
                }
                rarity += (double)character.AbilityImprovement / (double)100;
                crewCombatPowerRatio += rarity * (double)character.Level / (double)40;
            }

            return crewCombatPowerRatio;
        }

        public double levelMultiplicatedByCrewCombatPowerRatioCalculation {
            get {
                return (double)CrewCombatPowerRatioCalculation() * (double)_shipDesign.ShipLevel;
            }
        }
        public string ToCSV() {
            char[] CSVInit = { (char)239,(char)187,(char)191 };
            string CSV = new string(CSVInit);
            CSV += DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "," + Helpers.CSVEncoding(ShipName) + "\n\n";
            List<character> charactersDone = new List<character>();
            foreach (string category in _roomsByCategories.Keys) {
                CSV += "\n\n\"" + category + "\",\n";
                foreach (room room in _roomsByCategories[category]) {
                    bool addedContent = true;
                    int counter = -1;
                    while (addedContent) {
                        addedContent = false;
                        if (counter == -1) {
                            addedContent = true;
                            CSV += "\"" + Helpers.CSVEncoding(room._roomDesign.RoomName) + "[" + room.Column + ", " + room.Row + "]\",,";
                            foreach (character character in room._characters) {
                                CSV += "\"" + character.CharacterName + "[" + character.Level + "]\",";
                            }
                            addedContent = true;
                        } else {
                            if (room._Actions.Count > counter) {
                                CSV += room._Actions[counter].ToString();
                                addedContent = true;
                            }
                            CSV += ",,";
                            foreach (character character in room._characters) {
                                if (character._Actions.Count > counter) {
                                    CSV += "\"" + character._Actions[counter].ToString() + "\"";
                                    addedContent = true;
                                }
                                CSV += ",";
                            }
                        }
                        CSV += "\n";
                        counter++;
                    }
                }
            }
            return CSV;
        }
    }
    public class shipDesign {
        public int ShipDesignId, ShipLevel, Rows, Columns, Hp, MineralCost, RepairTime, ExteriorSpriteId
            , InteriorSpriteId, LogoSpriteId, UpgradeTime, RoomFrameSpriteId, UpgradeOffsetRows, UpgradeOffsetColumns
            , LiftSpriteId, DoorFrameLeftSpriteId, DoorFrameRightSpriteId, StarbuxCost, EngineX, EngineY
            , MineralCapacity, GasCapacity, FlagX, FlagY, MiniShipSpriteId, ItemCapacity, RequiredResearchDesignId
            , ThrustParticleSpriteId, ThrustLineAnimationId, RequiredShipDesignId, RaceId, ExteriorFileId
            , InteriorFileId, LogoFileId, RoomFrameFileId, LiftFileId, DoorFrameLeftFileId, DoorFrameRightFileId
            , EquipmentCapacity;
        public string ShipDesignName, ShipDescription, Mask, ShipType;
        public Boolean AllowInteracial;
        public float ThrustScale;
        public List<character> _characters = new List<character>();
        public override string ToString() {
            return ShipDesignName + "(" + ShipLevel + ")";
        }
        public static List<shipDesign> Dictionary2RoomDesigns(Dictionary<string,object> dict) {
            List<shipDesign> result = new List<shipDesign>();
            Helpers.CorrectDictionaryListFormat(dict,"ShipDesign");
            foreach (Dictionary<string,object> shipDesignsDict in (List<object>)dict["l_ShipDesign"]) {
                result.Add(Dictionary2shipDesign(shipDesignsDict));
            }
            return result;
        }
        public static Dictionary<int,shipDesign> Dictionary2shipDesignsDictionary(Dictionary<string,object> dict) {
            Dictionary<int,shipDesign> result = new Dictionary<int,shipDesign>();
            Helpers.CorrectDictionaryListFormat(dict,"ShipDesign");
            foreach (Dictionary<string,object> roomDesignsDict in (List<object>)dict["l_ShipDesign"]) {
                shipDesign shipDesign = Dictionary2shipDesign(roomDesignsDict);
                result.Add(shipDesign.ShipDesignId,shipDesign);
            }
            return result;
        }
        public static shipDesign Dictionary2shipDesign(Dictionary<string,object> dict) {
            shipDesign shipDesign = new shipDesign();
            Helpers.DictionaryToAttributes(dict,shipDesign);
            return shipDesign;
        }
    }
    public class character {
        public List<conditionAction> _Actions = new List<conditionAction>();
        public int CharacterId, ShipId, CharacterDesignId, Xp, Level, SkillPoints, OwnerShipId, HpImprovement,
            PilotImprovement, RepairImprovement, WeaponImprovement, ShieldImprovement,
            EngineImprovement, AttackImprovement, AbilityImprovement, StaminaImprovement,
            Fatigue, TrainingDesignId, RoomId, Stamina;
        public string CharacterName, OwnerUsername, ItemIds;
        public float BattleCharacterHp;
        public DateTime DeploymentDate = DateTime.MinValue, AvailableDate = DateTime.MinValue, TrainingEndDate = DateTime.MinValue;
        public characterDesign _characterDesign;
        public override string ToString() {
            return CharacterName + "(" + Level + ")"; // - [" + CharacterId + "] " + AvailableDate.ToString();
        }
        public static List<character> Dictionary2characters(PSSEngine engine,Dictionary<string,object> dict) {
            List<character> result = new List<character>();
            Helpers.CorrectDictionaryListFormat(dict,"Character");
            foreach (Dictionary<string,object> characterDict in (List<object>)dict["l_Character"]) {
                result.Add(Dictionary2character(engine,characterDict));
            }
            return result;
        }
        public static character Dictionary2character(PSSEngine engine,Dictionary<string,object> dict) {
            character character = new character();
            Helpers.DictionaryToAttributes(dict,character);
            if (dict.Keys.Contains("CharacterActions")) {
                character._Actions = conditionAction.Dictionary2conditionActions(engine,(Dictionary<string,object>)dict["CharacterActions"],"CharacterAction");
            }
            character._characterDesign = engine.characterDesigns[character.CharacterDesignId];
            return character;
        }
    }
    public class characterDesign {
        public int CharacterDesignId, CharacterHeadPartId, CharacterBodyPartId, Hp, Level, GasCost
            , MineralCost, MinShipLevel, FinalHp, CharacterLegPartId, MaxCharacterLevel
            , ProfileSpriteId, TrainingCapacity, TapSoundFileId, ActionSoundFileId;
        public string CharacterDesignName, GenderType, RaceType, Rarity, ProgressionType
            , CharacterDesignDescription, SpecialAbilityType, EquipmentMask;
        public Boolean Rotate, FlipOnEnemyShip;
        public float FireResistance, Pilot, Attack
            , Repair, Weapon, Shield, Engine, Research, WalkingSpeed, FinalPilot, FinalAttack
            , FinalRepair, FinalWeapon, FinalShield, FinalEngine, FinalResearch, XpRequirementScale
            , RunSpeed, SpecialAbilityArgument, SpecialAbilityFinalArgument;
        public override string ToString() {
            return "[" + CharacterDesignId + ", " + Rarity + "] " + CharacterDesignName;
        }
        public static List<characterDesign> Dictionary2characterDesigns(Dictionary<string,object> dict) {
            List<characterDesign> result = new List<characterDesign>();
            Helpers.CorrectDictionaryListFormat(dict,"RoomDesign");
            foreach (Dictionary<string,object> characterDesignsDict in (List<object>)dict["l_RoomDesign"]) {
                result.Add(Dictionary2characterDesign(characterDesignsDict));
            }
            return result;
        }
        public static Dictionary<int,characterDesign> Dictionary2characterDesignsDictionary(Dictionary<string,object> dict) {
            Dictionary<int,characterDesign> result = new Dictionary<int,characterDesign>();
            Helpers.CorrectDictionaryListFormat(dict,"CharacterDesign");
            foreach (Dictionary<string,object> characterDesignsDict in (List<object>)dict["l_CharacterDesign"]) {
                characterDesign characterDesign = Dictionary2characterDesign(characterDesignsDict);
                result.Add(characterDesign.CharacterDesignId,characterDesign);
            }
            return result;
        }
        public static characterDesign Dictionary2characterDesign(Dictionary<string,object> dict) {
            characterDesign characterDesign = new characterDesign();
            Helpers.DictionaryToAttributes(dict,characterDesign);
            return characterDesign;
        }
    }
    public class conditionAction {
        public conditionType ConditionType;
        public actionType ActionType;
        public int ConditionTypeId, ActionTypeId, CharacterActionId, RoomActionId, CharacterId, RoomId, CharacterActionIndex, RoomActionIndex;
        public override string ToString() {
            return ConditionType.ToString() + " --> " + ActionType.ToString();
        }
        public string actionToString {
            get { return ActionType.ToString(); }
        }
        public string conditionToString {
            get { return ConditionType.ToString(); }
        }
        public static List<conditionAction> Dictionary2conditionActions(PSSEngine engine,Dictionary<string,object> dict,string listName) {
            List<conditionAction> result = new List<conditionAction>();
            Helpers.CorrectDictionaryListFormat(dict,listName);
            foreach (Dictionary<string,object> actionDict in (List<object>)dict["l_" + listName]) {
                result.Add(Dictionary2conditionAction(engine,actionDict));
            }
            return result;
        }
        public static conditionAction Dictionary2conditionAction(PSSEngine engine,Dictionary<string,object> dict) {
            conditionAction conditionOrder = new conditionAction();
            Helpers.DictionaryToAttributes(dict,conditionOrder);
            conditionOrder.ConditionType = engine.conditionTypes[conditionOrder.ConditionTypeId];
            conditionOrder.ActionType = engine.actionTypes[conditionOrder.ActionTypeId];
            return conditionOrder;
        }
    }
    public class actionType {
        public int ActionTypeId, ActionTypeParameterValue, ImageSpriteId, RequiredResearchDesignId;
        public string ActionTypeName, RoomType, ActionTypeCategory, ActionTypeKey, ActionTypeDescription, ActionTypeParameterRelativity, ConditionTypeCategory;
        public override string ToString() {
            return ActionTypeName; //ConditionTypeId.Description() + " --> " + ActionTypeId.Description();
        }
        public static List<actionType> Dictionary2actionTypes(Dictionary<string,object> dict) {
            List<actionType> result = new List<actionType>();
            Helpers.CorrectDictionaryListFormat(dict,"ActionType");
            foreach (Dictionary<string,object> actionDict in (List<object>)dict["l_ActionType"]) {
                result.Add(Dictionary2actionType(actionDict));
            }
            return result;
        }
        public static actionType Dictionary2actionType(Dictionary<string,object> dict) {
            actionType actionType = new actionType();
            Helpers.DictionaryToAttributes(dict,actionType);
            return actionType;
        }
        public static Dictionary<int,actionType> Dictionary2actionTypesDictionary(Dictionary<string,object> dict) {
            Dictionary<int,actionType> result = new Dictionary<int,actionType>();
            Helpers.CorrectDictionaryListFormat(dict,"ActionType");
            foreach (Dictionary<string,object> actionDict in (List<object>)dict["l_ActionType"]) {
                actionType actionType = Dictionary2actionType(actionDict);
                result.Add(actionType.ActionTypeId,actionType);
            }
            return result;
        }
    }
    public class conditionType {
        public int ConditionTypeId, ConditionTypeParameterValue, ImageSpriteId, RequiredResearchDesignId;
        public string ConditionTypeName, ConditionTypeDescription, ConditionTypeTarget, ConditionTypeCategory, ConditionTypeComparison, ConditionTypeKey, RoomType;
        public override string ToString() {
            return ConditionTypeName; //ConditionTypeId.Description() + " --> " + ActionTypeId.Description();
        }
        public static List<conditionType> Dictionary2conditionTypes(Dictionary<string,object> dict) {
            List<conditionType> result = new List<conditionType>();
            Helpers.CorrectDictionaryListFormat(dict,"ConditionType");
            foreach (Dictionary<string,object> actionDict in (List<object>)dict["l_ConditionType"]) {
                result.Add(Dictionary2conditionType(actionDict));
            }
            return result;
        }
        public static conditionType Dictionary2conditionType(Dictionary<string,object> dict) {
            conditionType conditionType = new conditionType();
            Helpers.DictionaryToAttributes(dict,conditionType);
            return conditionType;
        }
        public static Dictionary<int,conditionType> Dictionary2conditionTypesDictionary(Dictionary<string,object> dict) {
            Dictionary<int,conditionType> result = new Dictionary<int,conditionType>();
            Helpers.CorrectDictionaryListFormat(dict,"ConditionType");
            foreach (Dictionary<string,object> actionDict in (List<object>)dict["l_ConditionType"]) {
                conditionType conditionType = Dictionary2conditionType(actionDict);
                result.Add(conditionType.ConditionTypeId,conditionType);
            }
            return result;
        }
    }
    public class room {
        public List<conditionAction> _Actions = new List<conditionAction>();
        public int RoomId, RoomDesignId, ShipId, Row, Column, Manufactured, RandomSeed, CapacityUsed,
            AssignedPower, SystemPower, Progress, PowerGenerated, UpgradeRoomDesignId, TotalDefense,
            TargetCraftId;
        public string RoomStatus, ManufactureItemDesignIds, ItemIds;
        public DateTime ManufactureStartDate = DateTime.MinValue, ConstructionStartDate = DateTime.MinValue;
        public int queuedSpace, freeCapacity; // this is for proccess usage
        public List<character> _characters = new List<character>();
        public roomDesign _roomDesign = null;
        public static List<room> Dictionary2Rooms(PSSEngine engine,Dictionary<string,object> dict) {
            List<room> result = new List<room>();
            Helpers.CorrectDictionaryListFormat(dict,"Room");
            foreach (Dictionary<string,object> roomDict in (List<object>)dict["l_Room"]) {
                result.Add(Dictionary2Room(engine,roomDict));
            }
            return result;
        }
        public static room Dictionary2Room(PSSEngine engine,Dictionary<string,object> dict) {
            room room = new room();
            Helpers.DictionaryToAttributes(dict,room);
            if (dict.Keys.Contains("RoomActions")) {
                room._Actions = conditionAction.Dictionary2conditionActions(engine,(Dictionary<string,object>)dict["RoomActions"],"RoomAction");
            }
            room._roomDesign = engine.RoomDesigns[room.RoomDesignId];
            if (room._roomDesign != null) { room.TotalDefense = room._roomDesign.DefaultDefenceBonus; }
            return room;
        }
        public override string ToString() {
            string output;
            if (_roomDesign != null) {
                output = _roomDesign.RoomName;
            } else {
                output = RoomDesignId.ToString();
            }
            output += "[" + Column + ", " + Row + "] (" + RoomStatus + ")";
            return output;
        }
    }
    public class roomDesign {
        public int RoomDesignId, MineralCost, MaxSystemPower, ConstructionTime, Capacity, ReloadTime
            , ImageSpriteId, LogoSpriteId, MaxPowerGenerated, RandomImprovements, ImprovementAmounts
            , ManufactureCapacity, GasCost, Level, ConstructionSpriteId, MinShipLevel, ItemRank
            , Rows, Columns, RootRoomDesignId, RefillUnitCost, DefaultDefenceBonus
            , UpgradeFromRoomDesignId, MissileDesignId, RaceId;
        public string RoomName, RoomShortName, CategoryType, RoomType, ManufactureType, RoomDescription;
        public Boolean Rotate, FlipOnEnemyShip;
        public float ManufactureRate;
        public override string ToString() {
            return "[" + RoomDesignId + "] " + RoomName;
        }
        public static List<roomDesign> Dictionary2RoomDesigns(Dictionary<string,object> dict) {
            List<roomDesign> result = new List<roomDesign>();
            Helpers.CorrectDictionaryListFormat(dict,"RoomDesign");
            foreach (Dictionary<string,object> roomDesignsDict in (List<object>)dict["l_RoomDesign"]) {
                result.Add(Dictionary2RoomDesign(roomDesignsDict));
            }
            return result;
        }
        public static Dictionary<int,roomDesign> Dictionary2RoomDesignsDictionary(Dictionary<string,object> dict) {
            Dictionary<int,roomDesign> result = new Dictionary<int,roomDesign>();
            Helpers.CorrectDictionaryListFormat(dict,"RoomDesign");
            foreach (Dictionary<string,object> roomDesignsDict in (List<object>)dict["l_RoomDesign"]) {
                roomDesign roomDesign = Dictionary2RoomDesign(roomDesignsDict);
                result.Add(roomDesign.RoomDesignId,roomDesign);
            }
            return result;
        }
        public static roomDesign Dictionary2RoomDesign(Dictionary<string,object> dict) {
            roomDesign roomDesign = new roomDesign();
            Helpers.DictionaryToAttributes(dict,roomDesign);
            return roomDesign;
        }
    }
    public class item {
        public int ItemId, ShipId, ItemDesignId, Quantity, Seed;
        public itemDesign _itemDesign = null;
        public static List<item> Dictionary2Items(PSSEngine engine,Dictionary<string,object> dict) {
            List<item> result = new List<item>();
            Helpers.CorrectDictionaryListFormat(dict,"Item");
            foreach (Dictionary<string,object> itemDict in (List<object>)dict["l_Item"]) {
                result.Add(Dictionary2Item(engine,itemDict));
            }
            return result;
        }
        public static Dictionary<int,item> Dictionary2ItemsIndex(PSSEngine engine,Dictionary<string,object> dict) {
            Dictionary<int,item> result = new Dictionary<int,item>();
            Helpers.CorrectDictionaryListFormat(dict,"Item");
            foreach (Dictionary<string,object> itemDict in (List<object>)dict["l_Item"]) {
                item item = Dictionary2Item(engine,itemDict);
                result.Add(item.ItemDesignId,item);
            }
            return result;
        }
        public static item Dictionary2Item(PSSEngine engine,Dictionary<string,object> dict) {
            item item = new item();
            Helpers.DictionaryToAttributes(dict,item);
            item._itemDesign = engine.itemDesigns[item.ItemDesignId];
            return item;
        }
        public override string ToString() {
            return _itemDesign.ToString() + ": " + Quantity + " units";
        }
    }
    public class itemDesign {
        public int ItemDesignId, ImageSpriteId, LogoSpriteId, ItemSpace, GasCost, MineralCost, Rank, MinRoomLevel, BuildTime
            , RootItemDesignId, MarketPrice, DropChance, RaceId
            , RequiredResearchDesignId, ParentItemDesignId, CraftDesignId, MissileDesignId, CharacterPartId
            , ModuleArgument, ActiveAnimationId, AnimationId, BorderSpriteId;
        public string ItemDesignName, ItemDesignKey, ItemDesignDescription, ItemType, ItemSubType, EnhancementType
            , Rarity, Ingredients, ModuleType;
        public float EnhancementValue;
        public override string ToString() {
            return "[" + ItemDesignId + "] " + ItemDesignName;
        }
        public static List<itemDesign> Dictionary2itemDesigns(Dictionary<string,object> dict) {
            List<itemDesign> result = new List<itemDesign>();
            Helpers.CorrectDictionaryListFormat(dict,"ItemDesign");
            foreach (Dictionary<string,object> itemDesignsDict in (List<object>)dict["l_ItemDesign"]) {
                result.Add(Dictionary2itemDesign(itemDesignsDict));
            }
            return result;
        }
        public static Dictionary<int,itemDesign> Dictionary2itemDesignDictionary(Dictionary<string,object> dict) {
            Dictionary<int,itemDesign> result = new Dictionary<int,itemDesign>();
            Helpers.CorrectDictionaryListFormat(dict,"ItemDesign");
            foreach (Dictionary<string,object> roomDesignsDict in (List<object>)dict["l_ItemDesign"]) {
                itemDesign itemDesign = Dictionary2itemDesign(roomDesignsDict);
                result.Add(itemDesign.ItemDesignId,itemDesign);
            }
            return result;
        }
        public static itemDesign Dictionary2itemDesign(Dictionary<string,object> dict) {
            itemDesign itemDesign = new itemDesign();
            Helpers.DictionaryToAttributes(dict,itemDesign);
            return itemDesign;
        }
    }
    public class lift {
        public static List<lift> Dictionary2Lifts(Dictionary<string,object> dict) {
            List<lift> result = new List<lift>();
            Helpers.CorrectDictionaryListFormat(dict,"List");
            foreach (Dictionary<string,object> liftDict in (List<object>)dict["l_Lift"]) {
                result.Add(Dictionary2Lift(liftDict));
            }
            return result;
        }
        public static lift Dictionary2Lift(Dictionary<string,object> dict) {
            lift lift = new lift();
            Helpers.DictionaryToAttributes(dict,lift);
            return lift;
        }
    }
    public class combatRequestReply {
        public vessel AttackingShip, DefendingShip;
        public user AttackingUser, DefendingUser;
        public int BattleId, AttackingShipId, DefendingShipId, RandomSeed, WinTrophyResult, WinMineralsResult
            , WinGasResult, LoseTrophyResult, LoseMineralsResult, LoseGasResult, BattleEndFrame, ClientEndFrame
            , AttackingUserId, DefendingUserId, AttackingUserTrophy, DefendingUserTrophy, AllianceWarId
            , MissionEventId, MissionDesignId;
        public string AttackingShipXml, DefendingShipXml, OutcomeType, ClientOutcomeType, AttackingUserXml
            , DefendingUserXml, ServerOutcomeType, AttackingShipName, DefendingShipName, Commands;
        public DateTime BattleDate = DateTime.MinValue, BattleEndDate = DateTime.MinValue;
        public static List<combatRequestReply> Dictionary2combatRequestReplys(PSSEngine engine,Dictionary<string,object> dict) {
            List<combatRequestReply> result = new List<combatRequestReply>();
            Helpers.CorrectDictionaryListFormat(dict,"Battle");
            foreach (Dictionary<string,object> combatRequestReplyDict in (List<object>)dict["l_Battle"]) {
                result.Add(Dictionary2combatRequestReply(engine,combatRequestReplyDict));
            }
            return result;
        }
        public static combatRequestReply Dictionary2combatRequestReply(PSSEngine engine,Dictionary<string,object> dict) {
            combatRequestReply combat = new combatRequestReply();
            Helpers.DictionaryToAttributes(dict,combat);
            Dictionary<string,object> data, dataB;
            if (!string.IsNullOrEmpty((string)dict["a_AttackingShipXml"])) {
                data = ((Dictionary<string,object>)xml2Dictionary.Parse((string)dict["a_AttackingShipXml"]));
                combat.AttackingShip = vessel.Dictionary2Vessel(engine,data);
            }
            if (!string.IsNullOrEmpty((string)dict["a_DefendingShipXml"])) {
                dataB = ((Dictionary<string,object>)xml2Dictionary.Parse((string)dict["a_DefendingShipXml"]));
                combat.DefendingShip = vessel.Dictionary2Vessel(engine,dataB);
            }
            if (!string.IsNullOrEmpty((string)dict["a_AttackingUserXml"])) {
                data = ((Dictionary<string,object>)xml2Dictionary.Parse((string)dict["a_AttackingUserXml"]));
                data = Helpers.advanceDict(data,"User");
                combat.AttackingUser = user.Dictionary2user(engine,data);
            }
            if (!string.IsNullOrEmpty((string)dict["a_DefendingUserXml"])) {
                dataB = ((Dictionary<string,object>)xml2Dictionary.Parse((string)dict["a_DefendingUserXml"]));
                dataB = Helpers.advanceDict(dataB,"User");
                combat.DefendingUser = user.Dictionary2user(engine,dataB);
            }
            //if (dict.Keys.Contains("CharacterActions")) {
            //    character._CharacterActions = conditionAction.Dictionary2conditionActions(engine,(Dictionary<string,object>)dict["CharacterActions"]);
            //}
            return combat;
        }
    }
    public class user {
        //public vessel ship;
        public int Id, Credits, Trophy, TutorialStatus, IconSpriteId, TipStatus, CrewDonated, CrewReceived
            , DailyRewardStatus, HeroBonusChance, GameCenterFriendCount, Status
            , ShipDesignId, AllianceSpriteId, Ranking, AllianceId;
        public string Name, GameCenterId, FacebookToken, Email, UserType, GenderType, RaceType, ProfileImageUrl
            , GameCenterName, CompletedMissionDesigns, LanguageKey, AllianceMembership, UnlockedShipDesignIds
            , UnlockedCharacterDesignIds, CompletedMissionEventIds, AllianceName;
        public DateTime LastAlertDate = DateTime.MinValue, LastCatalogPurchaseDate = DateTime.MinValue
            , LastPurchaseDate = DateTime.MinValue, LastHeartBeatDate = DateTime.MinValue
            , LastRewardActionDate = DateTime.MinValue, FacebookTokenExpiryDate = DateTime.MinValue
            , CreationDate = DateTime.MinValue, LastLoginDate = DateTime.MinValue;
        public static user Dictionary2user(PSSEngine engine,Dictionary<string,object> dict) {
            user user = new user();
            Helpers.DictionaryToAttributes(dict,user);
            //if (dict.Keys.Contains("CharacterActions")) {
            //    character._CharacterActions = conditionAction.Dictionary2conditionActions(engine,(Dictionary<string,object>)dict["CharacterActions"]);
            //}
            return user;
        }
    }
}
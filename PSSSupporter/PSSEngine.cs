using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Xml.Linq;
using System.Threading;
using System.Text.RegularExpressions;

namespace PSSSupporter {
    public class PSSEngine {
        /*
        https://api2.pixelstarships.com/UserService/AuthorizePushToken?deviceKey=12B66CB0-E280-4348-9F66-5EF307727A9B&pushToken=8cda3977b07e2b65359cd395697e18d2eb9f79dd7e31aa386798919b8b225255&accessToken=1cbde31c-cd88-4390-92f4-8e7bff52c8a1
        /UserService/DeviceLogin5?deviceKey=&advertisingKey=&isJailBroken=&checksum=&deviceType=
[12:04 PM] zensi: the problem is this one /UserService/AuthorizePushToken?deviceKey=&pushToken=&accessToken=
        */
        public String accessToken = "";
        public string userAgent = "PixelStarships/1 CFNetwork/808.1.4 Darwin/16.1.0";
        Random rnd = new Random();

        int[] levelGasRequirement = new int[40] {0,
                1, 17, 33, 65, 130, //1-5
                325, 650, 1300, 3200, 6500, //6-10
                9700, 13000, 19500, 26000, 35700, //11-15
                43800, 52000, 61700, 71500, 84500, //16-20
                104000, 117000, 130000, 156000, 175000, //21-25
                201000, 227000, 0279000, 279000, 312000, //26-30
                312000, 351000, 383000, 468000, 507000, //31-35
                552000, 604000, 650000, 715000 }; //36-39
        int[] levelGasLegendaryRequirement = new int[40] {0,
                130000, 162000, 195000, 227000, 260000, //1-5 +32, +33 + 32 +33 +32
                292000, 325000, 357000, 390000, 422000, //6-10 +32 +33 +32 +33 +32
                455000, 487000, 520000, 552000, 585000, //11-15 +33 +32 +33 +32 +33
                617000, 650000, 682000, 715000, 747000, //16-20 +32 +33 +32 +33 +32
                780000, 812000, 845000, 877000, 910000, //21-25 +33 +32 +33 +32 +33
                942000, 975000, 1007000, 1040000, 1072000, //26-30 +32 +33 +32 +33 +32
                1105000, 1137000, 1170000, 1202000, 1235000, //31-35 +33 +32 +33 +32 +33
                1267000, 1300000, 1332000, 1365000 }; //36-39 +32 +33 +32 +33 +32
        int[] levelXPLegendaryRequirement = new int[40] {0,
                0, 0, 0, 0, 0, //1-5
                0, 0, 0, 0, 0, //6-10
                0, 0, 0, 0, 0, //11-15
                0, 0, 0, 0, 0, //16-20
                0, 0, 0, 200520, 220500, //21-25
                241650, 263970, 287460, 312120, 337950, //26-30
                337950, 0, 0, 453870, 486000, //31-35
                519480, 554310, 590490, 628020 }; //36-39
        /* Legendary
            34: 453870 37: 554310
         
        

       
        */
        int[] levelXPRequirement = new int[40] {0,
                90, 360, 810, 1440, 2250, //1-5
                3270, 4500, 5940, 7590, 9450, //6-10
                11580, 13980, 16650, 19590, 22800, //11-15
                26340, 30210, 34410, 38940, 43800, //16-20
                49020, 54600, 60540, 66840, 73500, //21-25
                80550, 87990, 95820, 104040, 112650, //26-30
                121680, 131130, 141000, 151290, 162000, //31-35
                173160, 184770, 196830, 209340 }; //36-39

        //-9.0062 + 42.138 * x  + 69.08* x^2 + 2.1183 * x^3 -0.010025 * x^4
        /*
            1 90 52
            2 360 362
            3 810 822
            4 1400 1446
            5 2200 2248
            6 3200 3242
            7 4500 4439
            8 5900 5853
            9 7500 7495
            10 9450 9379
            11 11500 11514
            12 13900 13913
            13 16600
            14 19500
            15 22800 22795
            16 26300 26352
            17 30200 30221 <-- 30210
            18 34400 34413
            19 38940 38937 OK
            20 43800 43800
            21 49000 49010
            22 54600 54575
            23 60500 60502
            24 66800 66798
            25 73500 73469
            26 80500 80521
            27 87990 87961
            28 95820 95794
            29 104000 104023
            30 112639 112656
            31 121000
            32 131130 131144
            33 141000 141007
            34 151290 151288
            35 162000 161989
            36 173160 173112
            37 184000 184661 ok
            38 196000 196636
            39 209000 209039
         */


        //app data
        //public 
        //User items
        public user user;
        public vessel vessel;
        public Dictionary<int,character> Characters = new Dictionary<int,character>();
        public Dictionary<int,room> Rooms = new Dictionary<int,room>();
        public Dictionary<int,roomDesign> RoomDesigns = new Dictionary<int,roomDesign>();
        public Dictionary<int,conditionType> conditionTypes = new Dictionary<int,conditionType>();
        public Dictionary<int,actionType> actionTypes = new Dictionary<int,actionType>();
        public Dictionary<int,shipDesign> shipDesigns = new Dictionary<int,shipDesign>();
        public Dictionary<int,itemDesign> itemDesigns = new Dictionary<int,itemDesign>();
        public Dictionary<int,characterDesign> characterDesigns = new Dictionary<int,characterDesign>();

        public PSSEngine(string accessToken) {
            this.accessToken = accessToken;
            httpCommunications.userAgent = userAgent;
            Dictionary<string,object> result = httpCommunications.getParsedContent(@"http://api.pixelstarships.com/RoomService/ListRoomDesigns2?languageKey=en");
            result = Helpers.advanceDict(result,"RoomService","ListRoomDesigns","RoomDesigns");
            RoomDesigns = roomDesign.Dictionary2RoomDesignsDictionary((Dictionary<string,object>)result);
            result = httpCommunications.getParsedContent(@"http://api.pixelstarships.com/RoomService/ListConditionTypes2?languageKey=en");
            result = Helpers.advanceDict(result,"RoomService","ListConditionTypes","ConditionTypes");
            conditionTypes = conditionType.Dictionary2conditionTypesDictionary((Dictionary<string,object>)result);
            result = httpCommunications.getParsedContent(@"http://api.pixelstarships.com/RoomService/ListActionTypes2?languageKey=en");
            result = Helpers.advanceDict(result,"RoomService","ListActionTypes","ActionTypes");
            actionTypes = actionType.Dictionary2actionTypesDictionary((Dictionary<string,object>)result);
            result = httpCommunications.getParsedContent(@"http://api.pixelstarships.com/ShipService/ListAllShipDesigns2?languageKey=en");
            result = Helpers.advanceDict(result,"ShipService","ListShipDesigns","ShipDesigns");
            shipDesigns = shipDesign.Dictionary2shipDesignsDictionary((Dictionary<string,object>)result);
            result = httpCommunications.getParsedContent(@"http://api.pixelstarships.com/ItemService/ListItemDesigns2?languageKey=en");
            result = Helpers.advanceDict(result,"ItemService","ListItemDesigns","ItemDesigns");
            itemDesigns = itemDesign.Dictionary2itemDesignDictionary((Dictionary<string,object>)result);
            result = httpCommunications.getParsedContent(@"http://api.pixelstarships.com/CharacterService/ListAllCharacterDesigns2?languageKey=en");
            result = Helpers.advanceDict(result,"CharacterService","ListAllCharacterDesigns","CharacterDesigns");
            characterDesigns = characterDesign.Dictionary2characterDesignsDictionary((Dictionary<string,object>)result);
            ///////////////////////
            /*
             * 
             /UserService/GetCurrentUser?accessToken=
             
             http://api.pixelstarships.com/FileService/ListSprites
             http://api2.pixelstarships.com/CharacterService/ListAllCharacterDesigns2?languageKey=en
             http://api2.pixelstarships.com/FileService/ListFiles
             http://datxcu1rnppcg.cloudfront.net/%d.png
             */
        }
        public void updatePlayerData() {
            vessel = getOwnShip();
        }
        DateTime pastBuxTime = DateTime.Now;
        public void getStarbuxIfAble() {
            DateTime test = DateTime.Now;
            if (test >= (pastBuxTime + TimeSpan.FromSeconds(60))) {
                Console.WriteLine(test - pastBuxTime);
                string content = httpCommunications.getPostContent(@"http://api2.pixelstarships.com/UserService/AddStarbux?quantity=1&accessToken=" + accessToken);
                pastBuxTime = DateTime.Now;
            }
        }
        public vessel getOwnShip() {
            Dictionary<string,object> result = httpCommunications.getParsedContent(@"http://api2.pixelstarships.com/ShipService/GetShip?shipId=3366&accessToken=" + accessToken);
            result = Helpers.advanceDict(result,"ShipService","GetShip");
            return vessel.Dictionary2Vessel(this,(Dictionary<string,object>)result);
        }
        public void updateShipAndUserData() {
            Dictionary<string,object> result = httpCommunications.getParsedContent(@"http://api2.pixelstarships.com/ShipService/InspectShip?userId=695&accessToken=" + accessToken);
            result = Helpers.advanceDict(result,"ShipService","InspectShip");
            user = user.Dictionary2user(this,(Dictionary<string,object>)result["User"]);
            //vessel = vessel.Dictionary2Vessel(this,result);
            result = httpCommunications.getParsedContent(@"http://api2.pixelstarships.com/ShipService/GetShipByUserId?userId=695&accessToken=" + accessToken);
            result = Helpers.advanceDict(result,"ShipService","GetShipByUserId");
            vessel = vessel.Dictionary2Vessel(this,result);
            updateItemsData();
            //vessel = vessel.Dictionary2Vessel(this,(Dictionary<string,object>)result["Ship"]);
            ////////////////
        }
        public void updateItemsData() {
            Dictionary<string,object> result = httpCommunications.getParsedContent(@"http://api2.pixelstarships.com/ItemService/ListItemsOfAShip?accessToken=" + accessToken);
            result = Helpers.advanceDict(result,"ItemService","ListItemsOfAShip","Items");
            vessel._itemIndex = item.Dictionary2ItemsIndex(this,result);
        }
        public int getOwnUserRanking() {
            Dictionary<string,object> result = httpCommunications.getParsedContent(@"http://api2.pixelstarships.com/LadderService/FindUserRanking?accessToken=" + accessToken);
            result = Helpers.advanceDict(result,"LadderService","FindUserRanking");
            int ranking;
            int.TryParse((string)result["a_Ranking"],out ranking);
            return ranking;
            ////////////////
        }
        ///
        public Dictionary<int,character> getOwnCharacters() {
            Dictionary<string,object> result = httpCommunications.getParsedContent(@"http://api2.pixelstarships.com/CharacterService/ListAllCharactersOfUser?accessToken=" + accessToken);
            result = Helpers.advanceDict(result,"CharacterService","ListAllCharactersOfUser","Characters");
            List<character> list = character.Dictionary2characters(this,(Dictionary<string,object>)result);
            Dictionary<int,character> dict = new Dictionary<int,character>();
            foreach (character character in list) {
                dict.Add(character.CharacterId,character);
            }
            return dict;
        }
        public combatRequestReply getCombat() {
            string url = @"http://api2.pixelstarships.com/BattleService/CreateBattle?accessToken=" + accessToken;
            combatRequestReply combat = downloadAndProcessCombat(url,false);
            return combat;
        }
        public combatRequestReply getRevenge(int userId) {
            string url = @"http://api2.pixelstarships.com/BattleService/RevengeBattle?opponentUserId=" + userId + "&accessToken=" + accessToken;
            combatRequestReply combat = downloadAndProcessCombat(url,true);
            return combat;
        }

        //http://api2.pixelstarships.com/LadderService/FindUserRanking?accessToken=0782d9e0-a2bf-4a66-8665-9b68aeadf235

        private void delayUntilOnline() {
            //engine.getStarbuxIfAble();
            vessel vessel = this.getOwnShip();
            int delay = 5000;
            bool firstTime = true;
            while (vessel.ShipStatus != "Online" && vessel.ShipStatus != "Attacking") {
                if (firstTime) {
                    firstTime = false;
                    Console.WriteLine("DC at " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                }
                Thread.Sleep(delay);
                vessel = this.getOwnShip();
                /*if (delay < 10000) {
                    delay += 1000;
                } else if (delay < 600000) {
                    delay += 60000;
                }*/
            }
            delay = 60000;
        }

        private combatRequestReply downloadAndProcessCombat(string url,bool isRevenge) {
            string content = httpCommunications.getPostContent(url);
            if (content.Contains("has just cleaned up this area")) {
                return null;
            }
            var doc = XDocument.Parse(content);
            Dictionary<string,object> result = (Dictionary<string,object>)xml2Dictionary.Parse(doc.Root);
            if (isRevenge) {
                result = Helpers.advanceDict(result,"BattleService","RevengeBattle","Battle");
            } else {
                result = Helpers.advanceDict(result,"BattleService","CreateBattle","Battle");
            }
            combatRequestReply combat = combatRequestReply.Dictionary2combatRequestReply(this,(Dictionary<string,object>)result);
            string fileName = combat.DefendingUser.Id + " - " + combat.DefendingUser.Name + " - "
                + combat.DefendingUser.UserType + " - " + combat.DefendingUser.Email + " - (crewRatio " + combat.DefendingShip.CrewCombatPowerRatioCalculation() + ") - " 
                + DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss");
            string regexSearch = new string(System.IO.Path.GetInvalidFileNameChars()) + new string(System.IO.Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]",Regex.Escape(regexSearch)));
            fileName = r.Replace(fileName,"");
            File.WriteAllText("c:\\PSS\\ships\\" + " - " + fileName + ".ship.xml",(string)result["a_DefendingShipXml"]);
            File.WriteAllText("c:\\PSS\\users\\" + " - " + fileName + ".user.xml",(string)result["a_DefendingUserXml"]);
            File.WriteAllText("c:\\PSS\\ships\\" + " - " + fileName + ".user.csv",combat.DefendingShip.ToCSV());
            if (DateTime.Now - combat.DefendingUser.LastLoginDate > TimeSpan.FromDays(15)) {
                File.AppendAllText("c:\\PSS\\innactivePlayerList",", " + combat.DefendingUser.Id);
            } else {

            }

            return combat;
        }

        public void acceptCombat(combatRequestReply combat) {
            try {
                string url = @"http://api2.pixelstarships.com/BattleService/AcceptBattle?battleId=" + combat.BattleId + "&accessToken=" + accessToken;
                string content = httpCommunications.getPostContent(url);
                //var doc = XDocument.Parse(content);
                //Dictionary<string,object> result = (Dictionary<string,object>)xml2Dictionary.Parse(doc.Root);
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        public void finalizeCombat(combatRequestReply combat) {
            try {
                room weakestRoom = null;
                float relativeRoomPower = 0, otherRelativeRoomPower;
                foreach (room room in combat.DefendingShip._roomIndex.Values) {
                    if (room._roomDesign.MaxSystemPower > 0) {
                        if (weakestRoom == null) {
                            weakestRoom = room;
                            relativeRoomPower = room.TotalDefense * room._roomDesign.MaxSystemPower;
                        } else {
                            otherRelativeRoomPower = room.TotalDefense * room._roomDesign.MaxSystemPower;
                            if (otherRelativeRoomPower < relativeRoomPower) {
                                weakestRoom = room;
                                relativeRoomPower = room.TotalDefense * room._roomDesign.MaxSystemPower;
                            } else if (otherRelativeRoomPower == relativeRoomPower) {
                                if (room._roomDesign.MaxSystemPower < weakestRoom._roomDesign.MaxSystemPower) {
                                    weakestRoom = room;
                                }
                            }
                        }
                    }
                }
                string orders = "";
                int commandIndex = 1;
                int frame = rnd.Next(10,15);
                if (combat.AttackingShip._roomsByCategories.Keys.Contains("Laser")) {
                    foreach (room room in combat.AttackingShip._roomsByCategories["Laser"]) {
                        orders += createAttackCommandString(weakestRoom,orders,commandIndex,frame,room);
                        commandIndex++;
                        frame += rnd.Next(10,15);
                    }
                }
                if (combat.AttackingShip._roomsByCategories.Keys.Contains("Missile")) {
                    foreach (room room in combat.AttackingShip._roomsByCategories["Missile"]) {
                        orders += createAttackCommandString(weakestRoom,orders,commandIndex,frame,room);
                        commandIndex++;
                        frame += rnd.Next(10,15);
                    }
                }
                if (combat.AttackingShip._roomsByCategories.Keys.Contains("Hangar")) {
                    foreach (room room in combat.AttackingShip._roomsByCategories["Hangar"]) {
                        orders += createAttackCommandString(weakestRoom,orders,commandIndex,frame,room);
                        commandIndex++;
                        frame += rnd.Next(10,15);
                    }
                }
                //<UserCommands><Commands><Command Index="1" CommandType="SetTarget" Frame="70" RoomId="52072" CharacterId="0" TargetRoomId="7433462" /><Command Index="2" CommandType="SetTarget" Frame="102" RoomId="52127" CharacterId="0" TargetRoomId="7433462" /><Command Index="3" CommandType="SetTarget" Frame="130" RoomId="52884" CharacterId="0" TargetRoomId="7433462" /><Command Index="4" CommandType="SetTarget" Frame="158" RoomId="64914" CharacterId="0" TargetRoomId="7433462" /><Command Index="5" CommandType="SetTarget" Frame="198" RoomId="3645094" CharacterId="0" TargetRoomId="7433462" /><Command Index="6" CommandType="SetTarget" Frame="245" RoomId="7506371" CharacterId="0" TargetRoomId="7433462" /><Command Index="7" CommandType="SetTarget" Frame="343" RoomId="7841566" CharacterId="0" TargetRoomId="7433462" /></Commands></UserCommands>
                orders = "<UserCommands><Commands>" + orders + "</Commands></UserCommands>";
                string url = @"http://api2.pixelstarships.com/BattleService/FinaliseBattle5?battleId=" + combat.BattleId + "&clientOutcomeType=1&clientEndFrame=" + rnd.Next(800,1600) + "&attackingShipHp=2100&checksum=" + rnd.Next(12,25) + "&accessToken=" + accessToken;
                string content = httpCommunications.getPostContent(url,orders);//"<UserCommands><Commands></Commands></UserCommands>");
                //var doc = XDocument.Parse(content);
                //Dictionary<string,object> result = (Dictionary<string,object>)xml2Dictionary.Parse(doc.Root);
                Console.Write("x ");
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
        public void finalizeCombatToLose(int combatId) {
            try {
                string url = @"http://api2.pixelstarships.com/BattleService/FinaliseBattle5?battleId=" + combatId + "&clientOutcomeType=2&clientEndFrame=" + rnd.Next(800,1600) + "&attackingShipHp=2100&checksum=" + rnd.Next(12,25) + "&accessToken=" + accessToken;
                string content = httpCommunications.getPostContent(url,"<UserCommands><Commands><Command Index=\"1\" CommandType=\"SetPower\" Parameter=\"0\" Frame=\"31\" RoomId=\"52072\" /><Command Index=\"2\" CommandType=\"SetPower\" Parameter=\"-1\" Frame=\"33\" RoomId=\"52072\" /><Command Index=\"3\" CommandType=\"SetPower\" Parameter=\"-1\" Frame=\"35\" RoomId=\"52072\" /><Command Index=\"4\" CommandType=\"SetPower\" Parameter=\"0\" Frame=\"68\" RoomId=\"52884\" /><Command Index=\"5\" CommandType=\"SetPower\" Parameter=\"-1\" Frame=\"70\" RoomId=\"52884\" /><Command Index=\"6\" CommandType=\"SetPower\" Parameter=\"-1\" Frame=\"97\" RoomId=\"64914\" /><Command Index=\"7\" CommandType=\"SetPower\" Parameter=\"-1\" Frame=\"100\" RoomId=\"64914\" /><Command Index=\"8\" CommandType=\"SetPower\" Parameter=\"-1\" Frame=\"127\" RoomId=\"3645094\" /><Command Index=\"9\" CommandType=\"SetPower\" Parameter=\"-1\" Frame=\"128\" RoomId=\"3645094\" /><Command Index=\"10\" CommandType=\"SetPower\" Parameter=\"-2\" Frame=\"151\" RoomId=\"7841566\" /><Command Index=\"11\" CommandType=\"SetPower\" Parameter=\"-1\" Frame=\"152\" RoomId=\"7841566\" /><Command Index=\"12\" CommandType=\"SetPower\" Parameter=\"0\" Frame=\"175\" RoomId=\"52954\" /><Command Index=\"13\" CommandType=\"SetPower\" Parameter=\"0\" Frame=\"176\" RoomId=\"52954\" /><Command Index=\"14\" CommandType=\"SetPower\" Parameter=\"-1\" Frame=\"179\" RoomId=\"52954\" /><Command Index=\"15\" CommandType=\"SetPower\" Parameter=\"-1\" Frame=\"201\" RoomId=\"54471\" /><Command Index=\"16\" CommandType=\"SetPower\" Parameter=\"-1\" Frame=\"202\" RoomId=\"54471\" /><Command Index=\"17\" CommandType=\"SetPower\" Parameter=\"-1\" Frame=\"204\" RoomId=\"54471\" />"
                    + "<Command Index =\"17\" CommandType=\"SetPower\" Parameter=\"-1\" Frame=\"204\" RoomId=\"52127\" />"
                    + "<Command Index =\"17\" CommandType=\"SetPower\" Parameter=\"-1\" Frame=\"204\" RoomId=\"7506371\" />"
                    + "<Command Index=\"18\" CommandType=\"SetPower\" Parameter=\"0\" Frame=\"270\" RoomId=\"52137\" /><Command Index=\"19\" CommandType=\"SetPower\" Parameter=\"-1\" Frame=\"272\" RoomId=\"52137\" /><Command Index=\"20\" CommandType=\"SetPower\" Parameter=\"1\" Frame=\"294\" RoomId=\"52063\" /><Command Index=\"21\" CommandType=\"SetPower\" Parameter=\"-1\" Frame=\"296\" RoomId=\"52063\" /><Command Index=\"22\" CommandType=\"SetPower\" Parameter=\"-2\" Frame=\"335\" RoomId=\"8975028\" /><Command Index=\"23\" CommandType=\"SetPower\" Parameter=\"-1\" Frame=\"337\" RoomId=\"8975028\" /><Command Index=\"24\" CommandType=\"SetPower\" Parameter=\"-1\" Frame=\"340\" RoomId=\"8975028\" /></Commands></UserCommands>");
                Console.Write("xL ");
                //var doc = XDocument.Parse(content);
                //Dictionary<string,object> result = (Dictionary<string,object>)xml2Dictionary.Parse(doc.Root);
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
        public string TodayAndNow() {
            return DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        }

        public void reloadAmmoWithScarlets(autoCombatProcessStatus status) {
            // /RoomService/BuyMissile?roomId=%d&itemDesignId=%d&quantity=%d&manufactureStartDate=&accessToken=
            // /RoomService/BuyMissile2?roomId=%i&itemDesignString=&manufactureStartDate=&accessToken=
            // /RoomService/Refill?roomId=%d&accessToken=
            string url, content;
            foreach (room laser in vessel._roomsByCategories["Laser"]) {
                if (laser.CapacityUsed < laser._roomDesign.Capacity) {
                    url = @"http://api2.pixelstarships.com/RoomService/Refill?roomId=" + laser.RoomId + "&accessToken=" + accessToken;
                    content = httpCommunications.getPostContent(url);
                }
            }

            int totalMissileCapacity = 0;
            int scarletSpace = vessel._itemIndex[25]._itemDesign.ItemSpace;
            int scarlets = vessel._itemIndex[25].Quantity;
            int realMissileSpaceUsed = 0;
            foreach (item item in vessel._itemIndex.Values) {
                if (item._itemDesign.ItemType == "Missile") {
                    realMissileSpaceUsed += item.Quantity * item._itemDesign.ItemSpace;
                }
            }
            int queuedSpace = 0;

            if (vessel._roomsByCategories.Keys.Contains("Missile")) {
                //Dictionary<room,double> missileRooms = new Dictionary<room,double>();
                //Dictionary<room,string> missileQueues = new Dictionary<room,string>();
                List<room> missiles = new List<room>();
                foreach (room missile in vessel._roomsByCategories["Missile"]) {
                    totalMissileCapacity += missile._roomDesign.Capacity;
                    string[] queue = missile.ManufactureItemDesignIds.Split(',');
                    int queuedCapacity = 0;
                    foreach (string item in queue) {
                        if (!string.IsNullOrEmpty(item)) {
                            int result;
                            int.TryParse(item,out result);
                            queuedCapacity += itemDesigns[result].ItemSpace;
                        }
                    }
                    missiles.Add(missile);
                    queuedSpace += queuedCapacity;
                    missile.queuedSpace = queuedCapacity;
                    missile.freeCapacity = missile._roomDesign.ManufactureCapacity - queuedCapacity;
                }
                //List<room> missilesPerCount = .OrderBy(o => o.DefendingShip.levelMultiplicatedByCrewCombatPowerRatioCalculation).ToList();
                //Dictionary<room, int> missilesPerCount = missileRoomQueuedSpace.OrderBy(key => key.Key).ToList();

                int scarletsToAdd = (totalMissileCapacity /*/ 2*/ - realMissileSpaceUsed - queuedSpace) / scarletSpace;

                if (scarletsToAdd > 0) {
                    while (scarletsToAdd > 0 && missiles.Count > 0) {
                        List<room> oMis = missiles.OrderBy(o => o.queuedSpace).ToList();
                        room missile = oMis[0];
                        if (missile.freeCapacity / scarletSpace > 0) {
                            missile.freeCapacity -= scarletSpace;
                            missile.queuedSpace += scarletSpace;
                            missile.ManufactureItemDesignIds = updateQueue(missile,missile.ManufactureItemDesignIds,"25");
                            scarletsToAdd--;
                        }
                        if (missile.freeCapacity / scarletSpace == 0) {
                            missiles.Remove(missile);
                            //break;
                        }

                        /*room lowestQueueRoom = missiles[0];
                        foreach (room room in missiles) {
                            if (room.CapacityUsed < lowestQueueRoom.CapacityUsed) {
                                lowestQueueRoom = room;
                            }
                        }
                        int space = lowestQueueRoom.freeCapacity / scarletSpace;
                        if (space < scarletsToAdd) {
                            scarletsToAdd -= space;
                            missiles.Remove(lowestQueueRoom);
                        }*/
                    }
                    foreach (room missile in vessel._roomsByCategories["Missile"]) {
                        /*for (int i = 0; i < doables; i++) {
                            finalQueue = updateQueue(missile,finalQueue,"25");
                        }
                        freeSpace -= doables * scarletSpace;
                        scarletsToAdd -= doables;
                        finalQueue = reorderQueue(finalQueue);
                        */
                        updateMissileAmmoQueue(missile,missile.ConstructionStartDate != DateTime.MinValue,reorderQueue(missile.ManufactureItemDesignIds));
                    }
                }

                ////// TODO: Add compensation algorithm, so if the capacity is filled with current queues, part of missiles are moved from longer queues to shorter ones.


                if (realMissileSpaceUsed < 25
                   //|| vessel._itemIndex[40].Quantity < 20
                   || vessel._itemIndex[25].Quantity < 20) {
                    Console.Write("r" + vessel._itemIndex[25].Quantity + " ");
                    Thread.Sleep(20000);
                    updateShipAndUserData();
                }
            }
        }

        public void reloadAmmoMixindPenetratorsAndFirehawks(autoCombatProcessStatus status) {
            // /RoomService/BuyMissile?roomId=%d&itemDesignId=%d&quantity=%d&manufactureStartDate=&accessToken=
            // /RoomService/BuyMissile2?roomId=%i&itemDesignString=&manufactureStartDate=&accessToken=
            // /RoomService/Refill?roomId=%d&accessToken=
            string url, content;
            foreach (room laser in vessel._roomsByCategories["Laser"]) {
                if (laser.CapacityUsed < laser._roomDesign.Capacity) {
                    url = @"http://api2.pixelstarships.com/RoomService/Refill?roomId=" + laser.RoomId + "&accessToken=" + accessToken;
                    content = httpCommunications.getPostContent(url);
                }
            }

            int totalMissileCapacity = 0;
            int penetratorSpace = vessel._itemIndex[40]._itemDesign.ItemSpace;
            int penetrators = vessel._itemIndex[40].Quantity;
            int scarletSpace = vessel._itemIndex[25]._itemDesign.ItemSpace;
            int scarlets = vessel._itemIndex[25].Quantity;
            int realMissileCount = penetrators + scarlets;
            int otherMissiles = 0, otherMissilesSpace = 0;

            Dictionary<room,double> missileRooms = new Dictionary<room,double>();
            Dictionary<room,int> missileRoomQueuedCapacity = new Dictionary<room,int>();
            //Dictionary<room,string> missileQueues = new Dictionary<room,string>();
            foreach (room missile in vessel._roomsByCategories["Missile"]) {
                totalMissileCapacity += missile._roomDesign.Capacity;
                string[] queue = missile.ManufactureItemDesignIds.Split(',');
                int queuedCapacity = 0;
                foreach (string item in queue) {
                    if (!string.IsNullOrEmpty(item)) {
                        if (item == "40") {
                            penetrators++;
                            queuedCapacity += penetratorSpace;
                        } else if (item == "25") {
                            scarlets++;
                            queuedCapacity += scarletSpace;
                        } else {
                            otherMissiles++;
                            int result;
                            int.TryParse(item,out result);
                            otherMissilesSpace += itemDesigns[result].ItemSpace;
                        }
                    }
                }
                missileRoomQueuedCapacity[missile] = queuedCapacity;
            }
            int penetratorsToAdd = 0;// (totalMissileCapacity / 2 - penetrators * penetratorSpace) / penetratorSpace;
            int scarletsToAdd = (totalMissileCapacity /*/ 2*/ - scarlets * scarletSpace) / scarletSpace;
            int pairsToAdd = 0;
            if (penetratorsToAdd >= scarletsToAdd) {
                pairsToAdd = scarletsToAdd;
                penetratorsToAdd -= scarletsToAdd;
                scarletsToAdd = 0;
            } else {
                pairsToAdd = penetratorsToAdd;
                scarletsToAdd -= penetratorsToAdd;
                penetratorsToAdd = 0;
            }
            if (pairsToAdd > 0 || penetratorsToAdd > 0 || scarletsToAdd > 0) {
                foreach (room missile in vessel._roomsByCategories["Missile"]) {
                    int queue = missileRoomQueuedCapacity[missile];
                    string finalQueue = missile.ManufactureItemDesignIds;
                    int freeSpace = missile._roomDesign.ManufactureCapacity - queue;
                    int doables = freeSpace / (penetratorSpace + scarletSpace);
                    if (pairsToAdd < doables) {
                        doables = pairsToAdd;
                    }
                    for (int i = 0; i < doables; i++) {
                        finalQueue = updateQueue(missile,finalQueue,"40");
                        finalQueue = updateQueue(missile,finalQueue,"25");
                    }
                    freeSpace -= doables * (penetratorSpace + scarletSpace);
                    pairsToAdd -= doables;
                    doables = freeSpace / penetratorSpace;
                    if (penetratorsToAdd < doables) {
                        doables = penetratorsToAdd;
                    }
                    for (int i = 0; i < doables; i++) {
                        finalQueue = updateQueue(missile,finalQueue,"40");
                    }
                    freeSpace -= doables * penetratorSpace;
                    penetratorsToAdd -= doables;
                    doables = freeSpace / scarletSpace;
                    if (scarletsToAdd < doables) {
                        doables = scarletsToAdd;
                    }
                    for (int i = 0; i < doables; i++) {
                        finalQueue = updateQueue(missile,finalQueue,"25");
                    }
                    freeSpace -= doables * scarletSpace;
                    scarletsToAdd -= doables;
                    finalQueue = reorderQueue(finalQueue);

                    updateMissileAmmoQueue(missile,queue > 0,finalQueue);
                }
            }

            //int penetratorsToAddPerRoom = totalMissileCapacity / 2 - penetrators * penetratorSpace;
            //int scarletsToAddPerRoom = totalMissileCapacity / 2 - scarlets * scarletSpace;
            //"http://api2.pixelstarships.com/RoomService/BuyMissile2?roomId=52954&itemDesignString=40,25&manufactureStartDate=2016-11-26T15-14-10&accessToken=699c389b-f517-41bb-99c3-b70d33e9a831"
            /*while ((penetrators + 1) * penetratorSpace < totalMissileCapacity / 2) {
                foreach (room missile in missileRooms) {
                    url = @"http://api2.pixelstarships.com/RoomService/BuyMissile2?roomId=" + missile.RoomId + "&itemDesignString=40&manufactureStartDate=" + TodayAndNow() + "&accessToken=" + accessToken;
                    content = httpCommunications.getPostContent(url);
                    penetrators++;
                }
            }
                    url = @"http://api2.pixelstarships.com/RoomService/BuyMissile2?roomId=" + missile.RoomId + "&itemDesignString=25&manufactureStartDate=" + TodayAndNow() + "&accessToken=" + accessToken;
                    content = httpCommunications.getPostContent(url);
                    scarlets++;
            }*/
            //Penetrators
            //MML: http://api2.pixelstarships.com/RoomService/BuyMissile2?roomId=64911&itemDesignString=40&manufactureStartDate=2016-11-24T20:36:55&accessToken=
            //Mis1: http://api2.pixelstarships.com/RoomService/BuyMissile2?roomId=54471&itemDesignString=&manufactureStartDate=2016-11-24T20:38:36&accessToken=f5436670-cd06-45e4-aae2-641ef377067e
            //      http://api2.pixelstarships.com/RoomService/BuyMissile2?roomId=52954&itemDesignString=40&manufactureStartDate=2016-11-24T22-48-18&accessToken=699c389b-f517-41bb-99c3-b70d33e9a831
            //Mis2: http://api2.pixelstarships.com/RoomService/BuyMissile2?roomId=52954&itemDesignString=&manufactureStartDate=2016-11-24T20:38:41&accessToken=f5436670-cd06-45e4-aae2-641ef377067e
            //Scarlet: http://api2.pixelstarships.com/RoomService/BuyMissile2?roomId=54471&itemDesignString=&manufactureStartDate=2016-11-24T20:43:51&accessToken=f5436670-cd06-45e4-aae2-641ef377067e
            if (realMissileCount < 25
                //|| vessel._itemIndex[40].Quantity < 20
                || vessel._itemIndex[25].Quantity < 20) {
                Thread.Sleep(600000);
            }
        }

        private static string reorderQueue(string finalQueue) {
            string[] split = finalQueue.Split(',');
            Array.Sort(split,StringComparer.InvariantCulture);
            finalQueue = string.Join(",",split);
            return finalQueue;
        }

        private string updateQueue(room missile,string finalQueue,string missileDesignId) {
            if (string.IsNullOrEmpty(finalQueue)) {
                finalQueue = missileDesignId;
                //updateMissileAmmoQueue(missile,true,finalQueue);
            } else {
                finalQueue += "," + missileDesignId;
                //updateMissileAmmoQueue(missile,false,finalQueue);
            }

            return finalQueue;
        }
        private void updateMissileAmmoQueue(room missile,bool reuseDate,string finalQueue) {
            string url;
            string date = reuseDate ? missile.ManufactureStartDate.ToString("yyyy-MM-ddTHH:mm:ss") : TodayAndNow();
            url = @"http://api2.pixelstarships.com/RoomService/BuyMissile2?roomId=" + missile.RoomId
                + "&itemDesignString=" + finalQueue + "&manufactureStartDate="
                + date + "&accessToken=" + accessToken;
            string content = httpCommunications.getPostContent(url);
        }

        public void LimitMaxPositionOrTrophies(int delayLenght,autoCombatProcessStatus status) {
            int currentRank = getOwnUserRanking();
            //isLastXDaysOfMonth(3)
            if (status.diveProcess) {
                if (isLastXDaysOfMonth(1) || DateTime.Now.Day == 1 || user.Trophy - status.diveExpectedTrophyLoses < (status.diveProccessStartAt - status.diveTrophiesToGoDown)) {
                    status.diveProcess = false;
                    status.diveLookingForNiceTarget = true;
                    Console.Write("dE(" + status.diveExpectedTrophyLoses + ") ");
                    //RemoveReactors(status);
                }
            } else {
                if (isLastXDaysOfMonth(3) || DateTime.Now.Day == 1) { //last momments of tournament
                    if (currentRank < status.diveLastDayWhenRankingBelow) {
                        Thread.Sleep(delayLenght); //Sleeps instead of diving, to keep a ladder position.
                    }
                } else {
                    switch (status.diveActivationType) {
                        case diveActivationType.Position:
                            if (currentRank < status.diveWhenRankingBelow) {
                                status.diveProccessStartAt = user.Trophy;
                                status.diveProcess = true;
                                Console.Write("dS ");
                                //PlaceReactorsBackAgain(status);
                            }
                            break;
                        case diveActivationType.Trophies:
                            if (user.Trophy > status.diveWhenTrophiesOver) {
                                status.diveProccessStartAt = user.Trophy;
                                status.diveExpectedTrophyLoses = 0;
                                status.diveProcess = true;
                                Console.Write("dS ");
                                //PlaceReactorsBackAgain(status);
                            }
                            break;
                    }
                }
            }
        }

        private void PlaceReactorsBackAgain(autoCombatProcessStatus status) {
            status.diveReactorsCount = vessel._roomsByCategories["Reactor"].Count();
            int counter = 0;
            foreach (room reactor in vessel._roomsByCategories["Reactor"]) {
                status.diveReactorsId[counter] = reactor.RoomId;
                status.diveLastPositionsOfReactorsX[counter] = reactor.Column;
                status.diveLastPositionsOfReactorsY[counter] = reactor.Row;
                counter++;
                string url = @"http://api2.pixelstarships.com/RoomService/RemoveRoom?shipId=" + vessel.ShipId + "&roomId=" + reactor.RoomId + "&accessToken=" + accessToken;
                string content = httpCommunications.getPostContent(url);
            }
        }

        private void RemoveReactors(autoCombatProcessStatus status) {
            for (int i = 0; i < status.diveReactorsCount; i++) {
                string url = @"http://api2.pixelstarships.com/RoomService/MoveRoom?roomId="
                    + status.diveReactorsId[i] + "&row=" + status.diveLastPositionsOfReactorsY[i]
                    + "&column=" + status.diveLastPositionsOfReactorsX[i] + "&accessToken=" + accessToken;
                string content = httpCommunications.getPostContent(url);
            }
        }

        public bool isLastXDaysOfMonth(int days) {
            DateTime now = DateTime.Now;
            return now.Day >= DateTime.DaysInMonth(now.Year,now.Month) - days;
        }
        public int daysUntilEndOfMonth() {
            DateTime now = DateTime.Now;
            return DateTime.DaysInMonth(now.Year,now.Month) - now.Day;
        }
        public int getNextValidTarget(autoCombatProcessStatus status) {
            int enemyShipId;
            int startingCounterValue = status.counter;
            status.counter++;
            ManageTargetPoolAndItsResults(status);
            int.TryParse(status.targetsPool[status.counter],out enemyShipId);
            while (enemyShipId == 0 || status.ShipsIdsAndHPLeft.Keys.Contains(enemyShipId)
                && status.ShipsIdsAndHPLeft[enemyShipId] >= status.hardShipsHPAllowance) {
                status.counter++;
                ManageTargetPoolAndItsResults(status);
                int.TryParse(status.targetsPool[status.counter],out enemyShipId);
            }
            return enemyShipId;
        }

        public void StartAutocombatIterations() {
            /*
             Create, once curl is done, wait 1-2 seconds. Accept. Wait 1-3 seconds. Then finalize.
            */
            string targetsString = File.ReadAllText("innactivePlayerList");
            autoCombatProcessStatus status = new autoCombatProcessStatus();
            status.targetsPool = targetsString.Split(',');//,StringSplitOptions.RemoveEmptyEntries);
            Characters = getOwnCharacters();
            updateShipAndUserData();
            //vessel = getOwnShip();
            //int ownRanking = getOwnUserRanking();
            Helpers.PopulateArray<int>(status.idCharsNotMovable,ref status.idCharsNotMovableCount,5265635,5265636,5306674); //yus
            Helpers.PopulateArray<int>(status.idCharsNotMovable,ref status.idCharsNotMovableCount,5306708); //air
            Helpers.PopulateArray<int>(status.idCharsNotMovable,ref status.idCharsNotMovableCount,5309994); //gre
            Helpers.PopulateArray<int>(status.idCharsNotMovable,ref status.idCharsNotMovableCount,5344996,5723950,5741874,5985709,6160551,6281605); // Llo
            Helpers.PopulateArray<int>(status.idCharsNotMovable,ref status.idCharsNotMovableCount,5476320,5491330,5706989); //squ
            Helpers.PopulateArray<int>(status.idCharsNotMovable,ref status.idCharsNotMovableCount,5265643,4109741,5508023); //Rocky
            Helpers.PopulateArray<int>(status.idCharsNotMovable,ref status.idCharsNotMovableCount,5306762,5509612,5309608,6342405); //Xin y Tomas y DMH
            Helpers.PopulateArray<int>(status.idCharsNotMovable,ref status.idCharsNotMovableCount,6161656,6175578); //Brendas
            Helpers.PopulateArray<int>(status.idCharsNotMovable,ref status.idCharsNotMovableCount,13176,15966,2986934,2986935,2986936,2986937,4962989,2986931); //captain, etc.
            Helpers.PopulateArray<int>(status.idCharsNotMovable,ref status.idCharsNotMovableCount,5304394);
            Helpers.PopulateArray<int>(status.idCharsNotMovable,ref status.idCharsNotMovableCount,6160274,6187862); //Pending to swap
            status.previousVessel = vessel;
            //DateTime prev = DateTime.Now;
            delayUntilOnline();
            //Console.WriteLine(DateTime.Now - prev);
            int enemyShipId = -1;
            combatRequestReply combat = null;
            status.diveProcess = false;
            ShowStatus();
            //status.ShipsIdsAndHPLeft = Helpers.DeserializeFromFile("ShipsIdsAndHPLeft.data");
            // y ejemplo de carga, por probar... Helpers.SerializeToFile(status.ShipsIdsAndHPLeft, "ShipsIdsAndHPLeft.data");
            List<combatRequestReply> CombatsToFinalizeLosing = new List<combatRequestReply>(), CombatsToFinalize = new List<combatRequestReply>();
            while (status.continueLooking) {
                status.previousVessel = vessel;
                //updateShipAndUserData();
                reloadAmmoWithScarlets(status);
                if (status.firstTime) {
                    status.firstTime = false;
                } else {
                    if (!status.diveLookingForNiceTarget) {
                        LimitMaxPositionOrTrophies(3600000,status);
                    }
                    if (status.diveProcess) {
                        while (vessel.Hp < 1) {
                            Thread.Sleep(60000);
                            updateShipAndUserData();
                        }
                    } else {
                        int lostHp = vessel._shipDesign.Hp - vessel.Hp;
                        status.ShipsIdsAndHPLeft[enemyShipId] = vessel.Hp;
                        if (lostHp > 0) {
                            //List<combatRequestReply> combats = getLastCombats();
                            while (lostHp > 1) {
                                Console.Write("rHP(" + vessel.Hp + ") ");
                                Thread.Sleep(10000);
                                updateShipAndUserData();
                                lostHp = vessel._shipDesign.Hp - vessel.Hp;
                            }
                        }
                    }
                    /*if (lostHp > 0) {
                        Thread.Sleep(vessel._shipDesign.RepairTime * lostHp * 1000);
                    }*/
                }
                //calculated lost HP... if (enemyShipId > 0)...
                delayUntilOnline(); //Remember to set the characters to their starting/official position
                //delayUntilHPFull();
                ////enemyShipId = getNextValidTarget(status);
                //int.TryParse(status.targetsPool[status.counter],out enemyShipId);
                ////combat = getRevenge(enemyShipId);
                combat = getCombat();
                Thread.Sleep(rnd.Next(500,1200));
                bool isNiceTarget;
                if (combat != null) {
                    isNiceTarget = combat.DefendingShip.isTargetAffordable(status,combat);
                } else {
                    Console.Write("o ");
                    isNiceTarget = false;
                    Thread.Sleep(rnd.Next(10000,15000));
                }
                if (isNiceTarget) {
                    status.diveLookingForNiceTarget = false;
                    Console.Write("+ ");
                    acceptCombat(combat);
                    if (status.firstInitializedCombat == DateTime.MinValue) {
                        status.firstInitializedCombat = DateTime.Now;
                    }
                    Thread.Sleep(rnd.Next(1000,3000));
                    if (!status.diveProcess) {
                        if (CombatsToFinalizeLosing.Count() > 0) {
                            FinalizedCombatsAsLosses(CombatsToFinalizeLosing);
                            finalizeCombat(combat);
                            updateShipAndUserData();
                            ShowStatus();
                            processCharactersLevelsUp(status); ////// TODO: mover a después de los ciclos...
                        } else {
                            if (status.stackCommonCombats) {
                                CombatsToFinalize.Add(combat);
                                if (status.firstInitializedCombat != DateTime.MinValue) {
                                    if (CombatsToFinalize.Count() > 9
                                            || status.firstInitializedCombat.AddSeconds(150) < DateTime.Now) {
                                        FinalizeCombatsInNicenessOrder(CombatsToFinalize);
                                        status.firstInitializedCombat = DateTime.MinValue;
                                        updateShipAndUserData();
                                        ShowStatus();
                                        processCharactersLevelsUp(status); ////// TODO: mover a después de los ciclos...
                                    }
                                } else { //No debería parar aquí?

                                }
                            } else {
                                finalizeCombat(combat);
                                updateShipAndUserData();
                                ShowStatus();
                                processCharactersLevelsUp(status); ////// TODO: mover a después de los ciclos...
                            }
                        }
                    } else {
                        if (CombatsToFinalize.Count() > 0) { //This needs a improvement... we can use the expected trophy wins to stop earlier the round of normal combats.
                            FinalizeCombatsInNicenessOrder(CombatsToFinalize);
                            status.firstInitializedCombat = DateTime.MinValue;
                            updateShipAndUserData();
                            ShowStatus();
                            processCharactersLevelsUp(status); ////// TODO: mover a después de los ciclos...
                        }
                        CombatsToFinalizeLosing.Add(combat);
                        status.diveExpectedTrophyLoses += combat.LoseTrophyResult;
                    }
                    status.combatsInCycle++;
                } else if (combat != null && combat.DefendingUser.Trophy == 1) {
                    if (combat != null) {
                        Console.Write("- ");
                    }
                    status.noTrophiesTargets++; status.avoidedCombats++;
                } else {
                    if (combat != null) {
                        Console.Write("- ");
                    }
                    status.avoidedCombats++;
                    if (!status.diveProcess && status.firstInitializedCombat != DateTime.MinValue
                        && (status.firstInitializedCombat.AddSeconds(150) < DateTime.Now)) {
                        FinalizeCombatsInNicenessOrder(CombatsToFinalize);
                        status.firstInitializedCombat = DateTime.MinValue;
                        updateShipAndUserData();
                        ShowStatus();
                        processCharactersLevelsUp(status); ////// TODO: mover a después de los ciclos...
                    }
                }
                //status.counter++;
                ManageTargetPoolAndItsResults(status);
            }
        }

        private static string createAttackCommandString(room weakestRoom,string orders,int commandIndex,int frame,room room) {
            orders += "<Command Index=\"" + commandIndex + "\" CommandType=\"SetTarget\" Frame=\"" + frame + "\" RoomId=\"" + room.RoomId + "\" CharacterId=\"0\" TargetRoomId=\"" + weakestRoom.RoomId + "\" />";
            return orders;
        }

        private List<combatRequestReply> getLastCombats() {
            List<combatRequestReply> combats;
            string url = @"http://api2.pixelstarships.com/BattleService/ListBattles?take=100&skip=0&accessToken=" + accessToken;
            string content = httpCommunications.getContent(url);
            var doc = XDocument.Parse(content);
            Dictionary<string,object> result = (Dictionary<string,object>)xml2Dictionary.Parse(doc.Root);
            result = Helpers.advanceDict(result,"BattleService","ListBattles","Battles");
            combats = combatRequestReply.Dictionary2combatRequestReplys(this,result);
            return combats;
        }

        private void ShowStatus() {
            Console.WriteLine("- " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " - " + vessel.Hp + " hp - m" + (vessel._itemIndex[3].Quantity) + ", g" + (vessel._itemIndex[2].Quantity));
            //Console.WriteLine("- " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " - " + enemyShipId + " - " + vessel.Hp + " hp - m" + (vessel._itemIndex[3].Quantity) + ", g" + (vessel._itemIndex[2].Quantity));
        }

        private void FinalizeCombatsInNicenessOrder(List<combatRequestReply> CombatsToFinalize) {
            List<combatRequestReply> combats = CombatsToFinalize.OrderBy(o => o.DefendingShip.levelMultiplicatedByCrewCombatPowerRatioCalculation).ToList();
            foreach (combatRequestReply aCombat in combats) {
                finalizeCombat(aCombat);
                Thread.Sleep(rnd.Next(500,1200));
            }
            CombatsToFinalize.Clear();
        }

        private void FinalizedCombatsAsLosses(List<combatRequestReply> CombatsToFinalizeLosing) {
            foreach (combatRequestReply aCombat in CombatsToFinalizeLosing) {
                finalizeCombatToLose(aCombat.BattleId);
                Thread.Sleep(rnd.Next(500,1200));
            }
            CombatsToFinalizeLosing.Clear();
        }

        /*
                private static bool isTargetAffordable(autoCombatProcessStatus status,combatRequestReply combat) {
                    if (combat == null) {
                        return false;
                    }
                    if (status.diveProcess) {
                        return true;
                    }
                    if (combat.DefendingShip._shipDesign.ShipLevel <= 8) {
                        return true;
                    }
                    Dictionary<string,double> rarityToDouble = new Dictionary<string,double>();
                    rarityToDouble.Add("Common",1); rarityToDouble.Add("Elite",2); rarityToDouble.Add("Unique",3);
                    rarityToDouble.Add("Epic",4); rarityToDouble.Add("Hero",5); rarityToDouble.Add("Legendary",6);
                    rarityToDouble.Add("Special",3.5);
                    string[] betterThanTierCharacters = new string[5] { "Zhuge Liang","Zombieee","Zombie","Meowy Cat","Roach" };
                    //('Common' => 1, 'Elite' => 2, 'Unique' => 3, 'Epic' => 4, 'Hero' => 5, 'Legendary' => 6, 'Special' => 3.5);
                    bool dangerousCrew = false;
                    double crewCombatPowerRatio = 0;
                    foreach (character character in combat.DefendingShip._characterIndex.Values) {
                        double rarity = rarityToDouble[character._characterDesign.Rarity];
                        if (betterThanTierCharacters.Contains(character.CharacterName)) {
                            rarity += 1;
                        }
                        rarity += (double)character.AbilityImprovement / (double)100;
                        crewCombatPowerRatio += rarity * (double)character.Level / (double)40;
                    }
                    bool dangerousTeleport = false, weakTeleport = false;
                    if (combat.DefendingShip._roomsByCategories["Teleport"].Count > 0) {
                        room teleport = combat.DefendingShip._roomsByCategories["Teleport"][0];
                        dangerousTeleport = (teleport._roomDesign.Level > 6) && teleport.TotalDefense > 28;
                        foreach (character character in teleport._characters) {
                            if (character._characterDesign.SpecialAbilityType == "Poison Gas" || character._characterDesign.SpecialAbilityType == "Critical Strike") {
                                if ((double)character._characterDesign.SpecialAbilityFinalArgument
                                    * (double)1 + ((double)character.AbilityImprovement / (double)100) >= 10) {
                                    dangerousTeleport = true;
                                }
                            }
                            weakTeleport = teleport.TotalDefense < 28;
                        }
                    }
                    if (combat.DefendingShip._shipDesign.ShipLevel <= 8) {
                        return true;
                    }
                    if (combat.DefendingShip._shipDesign.ShipLevel <= 9 && weakTeleport) {
                        return true;
                    }
                    return combat.DefendingUser.Trophy > 1 && DateTime.Now - combat.DefendingUser.LastLoginDate > TimeSpan.FromDays(20);
                }
        */
        private void ManageTargetPoolAndItsResults(autoCombatProcessStatus status) {
            if (status.counter >= status.targetsPool.Count()) {
                status.counter = 0;
                status.continueLooking = status.combatsInCycle != 0 && status.avoidedCombats != 0 && status.inImmunity != 0;
                if ((status.avoidedCombats == status.combatsInCycle || status.combatsInCycle < 15) && status.hardShipsHPAllowance > 1) {
                    status.hardShipsHPAllowance--;
                }
                if (status.combatsInCycle == 0 && status.avoidedCombats == 0 && status.inImmunity > 0) {
                    Console.WriteLine("Ttl " + status.targetsPool.Count() + " - cmb " + status.combatsInCycle + " - avd " + status.avoidedCombats + " - imm " + status.inImmunity + " - nTr " + status.noTrophiesTargets);
                    Thread.Sleep(rnd.Next(30000,300000));
                }
                status.combatsInCycle = 0;
                status.avoidedCombats = 0;
                status.inImmunity = 0;
                status.noTrophiesTargets = 0;
            }
        }

        public void CopyCharacterActions(int source,int target) {

        }
        public void processCharactersLevelsUp(autoCombatProcessStatus status) {
            List<character> charsOnVesselToLevelUp = new List<character>();
            List<character> charsOutOfVesselToLevelUp = new List<character>();
            int pendingCharactersToLevelUp = 0, pendingLegendariesToLevelUp = 0, charactersBelow40 = 0, pendingsCharactersOver35ToLevelUp = 0;
            foreach (character character in vessel._characterIndex.Values) {
                if (character.OwnerShipId == vessel.ShipId) {
                    if (character.Level == 40) {
                        ReplaceCharacterIfRequired(status,character);
                    } else {
                        charactersBelow40++;
                        if (status.previousVessel._characterIndex.Keys.Contains(character.CharacterId)) {

                            character prevCharacter = status.previousVessel._characterIndex[character.CharacterId];
                            if (character._characterDesign.Rarity == "Legendary" && character.Level == prevCharacter.Level && character.Xp == prevCharacter.Xp
                            //if (character._characterDesign.Rarity == "Legendary" && character.Xp == levelXPLegendaryRequirement[character.Level]
                                || character._characterDesign.Rarity != "Legendary" && character.Xp == levelXPRequirement[character.Level]) { //////////////// ==
                                if (character._characterDesign.Rarity != "Legendary" && vessel._itemIndex[2].Quantity > levelGasRequirement[character.Level]
                                    || vessel._itemIndex[2].Quantity > levelGasLegendaryRequirement[character.Level]) {
                                    string url = @"http://api2.pixelstarships.com/CharacterService/UpgradeCharacter?characterId=" + character.CharacterId + "&accessToken=" + accessToken;
                                    Dictionary<string,object> result = httpCommunications.getParsedPostContent(url);

                                    string response = httpCommunications.getContent(url,"POST",null);
                                    if (response.Contains("Character does not have enough experience to level up.")) {
                                        Console.WriteLine(" x " + character.ToString() + " <-- not enough XP");
                                        prevCharacter.Level = 0;
                                        updateItemsData();
                                    } else if (response.Contains("Not enough gas")) {
                                        Console.WriteLine(" x " + character.ToString() + " <-- not enough gas");
                                        updateItemsData();
                                    } else {
                                        var doc = XDocument.Parse(response);
                                        result = (Dictionary<string,object>)xml2Dictionary.Parse(doc.Root);
                                        result = Helpers.advanceDict(result,"CharacterService","UpgradeCharacter","Character");
                                        Helpers.DictionaryToAttributes(result,character);
                                        if (result.Keys.Contains("CharacterActions")) {
                                            character._Actions = conditionAction.Dictionary2conditionActions(this,(Dictionary<string,object>)result["CharacterActions"],"CharacterAction");
                                        }
                                        Console.WriteLine(" + " + character.ToString());
                                        updateItemsData();
                                    }
                                } else {
                                    if (character._characterDesign.Rarity != "Legendary") {
                                        pendingCharactersToLevelUp++;
                                        if (character.Level > 35) {
                                            pendingsCharactersOver35ToLevelUp++;
                                        }
                                    } else {
                                        pendingLegendariesToLevelUp++;
                                    }
                                    
                                }
                            }
                        }
                        // //////////////////
                        // /CharacterService/UpgradeCharacter?characterId=2562627&accessToken=3c234bd7-0222-421c-9a3a-40420dd3b02f
                    }
                }
            }
            if (charactersBelow40 == 0) {
                if (!status.stackCommonCombats) {
                    Console.Write("sMode ");
                }
                status.stackCommonCombats = true;
            } else {
                //if (pendingCharactersToLevelUp + pendingLegendariesToLevelUp > 4) { //|| pendingLegendariesToLevelUp >= 2
                    if (pendingCharactersToLevelUp + pendingLegendariesToLevelUp >= 4 || pendingLegendariesToLevelUp >= 1 || pendingsCharactersOver35ToLevelUp > 0) { //
                        if (!status.stackCommonCombats) {
                        Console.Write("sMode ");
                    }
                    status.stackCommonCombats = true;
                } else {
                    if (status.stackCommonCombats) {
                        Console.Write("cMode ");
                    }
                    status.stackCommonCombats = false;
                }
            }
            //pendingCharactersToLevelUp = 0, pendingLegendariesToLevelUp = 0; charactersBelow40
            //Prestige? /CharacterService/MergeCharacters?characterId1=%d&characterId2=%d&accessToken=
        }

        private void ReplaceCharacterIfRequired(autoCombatProcessStatus status,character character) {
            if (!status.idCharsNotMovable.Contains(character.CharacterId)) {
                foreach (character savedCharacter in Characters.Values) {
                    if (!status.idCharsNotMovable.Contains(savedCharacter.CharacterId) && savedCharacter.RoomId == 0) {
                        if (savedCharacter.Level != 40 && savedCharacter._characterDesign.Rarity != "Special"
                            && savedCharacter.OwnerShipId == vessel.ShipId) {
                            string url, result;
                            if (savedCharacter._Actions.Count == 0) {
                                // /CharacterService/CopyCharacterActions?characterId=%i&toCharacterId=%i&accessToken=
                                //url = @"http://api2.pixelstarships.com/CharacterService/CopyCharacterActions?characterId="
                                //    + character.CharacterId + "&to=" + savedCharacter.CharacterId + "&accessToken=" + accessToken;
                                //result = httpCommunications.getPostContent(url);
                            }
                            Console.WriteLine(character.ToString() + " out, " + savedCharacter.ToString() + " in");
                            int roomId = character.RoomId;
                            url = @"http://api2.pixelstarships.com/CharacterService/MoveCharacterToRoom?characterId="
                                   + character.CharacterId + "&roomId=0&accessToken=" + accessToken;
                            result = httpCommunications.getPostContent(url);
                            url = @"http://api2.pixelstarships.com/CharacterService/MoveCharacterToRoom?characterId="
                                   + savedCharacter.CharacterId + "&roomId=" + roomId + "&accessToken=" + accessToken;
                            result = httpCommunications.getPostContent(url);
                            break;
                            // http://api2.pixelstarships.com/CharacterService/MoveCharacterToRoom?characterId=5509612&roomId=0&accessToken=7637d744-a280-48ca-8b56-c622fd8a40d1
                            // /CharacterService/MoveCharacterToRoom?characterId=%d&roomId=%d&accessToken=
                        }
                    }
                }
            }
        }
    }
    public class autoCombatProcessStatus {
        public int counter = 0;
        public bool farming = true;
        public bool continueLooking = true, firstTime = true;
        public int combatsInCycle = 0;
        public int avoidedCombats = 0;
        public int noTrophiesTargets = 0;
        public int inImmunity = 0;
        public int hardShipsHPAllowance = 22;
        public string[] targetsPool;
        public int[] idCharsNotMovable = new int[137];
        public int idCharsNotMovableCount = 0;
        public vessel previousVessel = null;
        public Dictionary<int,int> ShipsIdsAndHPLeft = new Dictionary<int,int>();
        public bool diveProcess, diveLookingForNiceTarget = false;
        public diveActivationType diveActivationType = diveActivationType.Trophies;
        public int diveProccessStartAt = 0, diveTrophiesToGoDown = 500, diveWhenRankingBelow = 32, diveWhenTrophiesOver = 3599;
        public int[] diveLastPositionsOfReactorsX = new int[4];
        public int[] diveLastPositionsOfReactorsY = new int[4];
        public int[] diveReactorsId = new int[4];
        public int diveReactorsCount;
        public int diveLastDayWhenRankingBelow = 25, diveExpectedTrophyLoses = 0;
        public List<int> CombatsToFinalize = new List<int>();
        public DateTime firstInitializedCombat = DateTime.MinValue;
        public bool stackCommonCombats = false;
    }
    public enum diveActivationType { Position, Trophies };

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Net;
using System.IO;
//using System.Xml;
using System.Xml.Linq;
using System.Threading;
using System.Text.RegularExpressions;

namespace PSSSupporter
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            //WebProxy proxyObject = new WebProxy("http://127.0.0.1:8888/");
            //GlobalProxySelection.Select = proxyObject;
            //WebRequest.DefaultWebProxy = proxyObject;

            InitializeComponent();
            //Console.WriteLine("Hola!");
            String accessToken = "5c808561-6220-4d5a-8dd2-97cf7463d173";//"eebac5c3-5a73-491b-a54f-dbe1fe8abc5d";//"409b9565-a9e1-4dc9-9451-cc314732de48";// "e592b1e3-25ba-4810-9eeb-e86c944b144d";

            Dictionary<string,object> result;
            string url, content, fileName;
            XDocument doc;
            combatRequestReply combat = null, myOwnShip;

            PSSEngine engine = new PSSEngine(accessToken);

            
            content = File.ReadAllText("blacksteed2.txt");
            doc = XDocument.Parse(content);
            result = (Dictionary<string,object>)xml2Dictionary.Parse(doc.Root);
            result = Helpers.advanceDict(result, "BattleService", "GetBattle", "Battle");
            Dictionary<string,object> data = ((Dictionary<string,object>)xml2Dictionary.Parse((string)result["a_AttackingShipXml"]));
            //result = ReflectionHelpers.advanceDict(result, "BattleService", "CreateBattle", "Battle");
            vessel attackingVessel = vessel.Dictionary2Vessel(engine, data);
            data = ((Dictionary<string,object>)xml2Dictionary.Parse((string)result["a_DefendingShipXml"]));
            vessel deffendingVessel = vessel.Dictionary2Vessel(engine,data);
            string CSV = attackingVessel.ToCSV();
            File.WriteAllText("" + attackingVessel.ShipName + ".csv",CSV);
            File.WriteAllText("c:\\PSS\\" + attackingVessel.ShipName + ".csv",CSV);
            CSV = deffendingVessel.ToCSV();
            File.WriteAllText("" + deffendingVessel.ShipName + ".csv",CSV);
            File.WriteAllText("c:\\PSS\\" + deffendingVessel.ShipName + ".csv",CSV);
            character to, ca;
            foreach (character char1 in deffendingVessel._characterIndex.Values) {
                if (char1.CharacterName == "Tomas") {
                    to = char1;
                }
            }
            foreach (character char1 in attackingVessel._characterIndex.Values) {
                if (char1.CharacterName == "Meowy Cat") {
                    ca = char1;
                }
            }
            return;

            while (true) {
                try {
                    engine.StartAutocombatIterations();
                } catch (Exception e) {
                    Console.WriteLine();
                    Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " -- " + e.ToString());
                    Thread.Sleep(15000);
                }
            }
            
            return;

            vessel ownVessel = engine.getOwnShip();
            //string CSV = ownVessel.ToCSV();
            //File.WriteAllText("c:\\PSS\\informe.csv",CSV);
            //String url = @"http://api2.pixelstarships.com/CharacterService/UpgradeCharacter?characterId=5491330&accessToken=" + accessToken;
            //string content = httpCommunications.getPostContent(url);


            // subir nivel http://api2.pixelstarships.com/CharacterService/UpgradeCharacter?characterId=5491330&accessToken=d7221924-8b52-47a7-89af-4b678564555e

            //url = @"http://api2.pixelstarships.com/BattleService/CreateBattle?accessToken=" + accessToken;
            //content = httpCommunications.getPostContent(url);

            ////var parser = new xml2Dictionary();
            ////Dictionary<string, object> result = (Dictionary < string, object>)xml2Dictionary.Parse("<a a1=\"1\">va</a>");
            ////result = (Dictionary<string, object>)xml2Dictionary.Parse("<a a1=\"1\">va<a1></a1></a>");
            //var doc = XDocument.Parse(content);
            //Dictionary<string, object> result = (Dictionary<string, object>)xml2Dictionary.Parse(doc.Root);
            ////result = ((Dictionary<string, object>)((Dictionary<string, object>)result["BattleService"])["CreateBattle"])["Battle"];
            //result = ReflectionHelpers.advanceDict(result, "BattleService", "CreateBattle", "Battle");
            //combatRequestReply combat = combatRequestReply.Dictionary2combatRequestReply(engine, (Dictionary<string,object>)result);
            //string fileName = combat.DefendingUser.Name + " - " + combat.DefendingUser.Email + " - " + DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss") + ".xml";
            //File.WriteAllText(fileName,content);

            ////result = ((Dictionary<string, object>)result["BattleService"]);
            ////result = ((Dictionary<string, object>)result["CreateBattle"]);
            ////result = ((Dictionary<string, object>)result["Battle"]);
            //Dictionary<string, object> data = ((Dictionary<string, object>)xml2Dictionary.Parse((string)result["a_AttackingShipXml"]));
            ////data = (Dictionary<string, object>)data["Ship"];
            //result["AttackingShip"] = data;
            //data = ((Dictionary<string, object>)xml2Dictionary.Parse((string)result["a_DefendingShipXml"]));
            ////data = (Dictionary<string, object>)data["Ship"];
            //result["DefendingShip"] = data;
            ////result["DefendingShip"] = ((Dictionary<string, object>)xml2Dictionary.Parse((string)result["a_DefendingShipXml"]))["DefendingShip"];
            ////Parseo...
            ////http://stackoverflow.com/questions/4484460/parse-xml-data-into-an-array-in-c-sharp
            //vessel attacking = vessel.Dictionary2Vessel(engine, (Dictionary<string, object>)result["AttackingShip"]);
            Random rnd = new Random();
            //return;
            url = @"http://api2.pixelstarships.com/BattleService/RevengeBattle?opponentUserId=695&accessToken=" + accessToken;
            content = httpCommunications.getPostContent(url);
            if (!content.Contains("Can't find any available ship.")) {
                doc = XDocument.Parse(content);
                result = (Dictionary<string,object>)xml2Dictionary.Parse(doc.Root);
                result = Helpers.advanceDict(result,"BattleService","RevengeBattle","Battle");
                myOwnShip = combatRequestReply.Dictionary2combatRequestReply(engine,(Dictionary<string,object>)result);
            }
            for (int i = 11509; i < 1000000; i++) {
                try {
                    if (i % 23 == 0) {
                        //engine.getStarbuxIfAble();
                        vessel vessel = engine.getOwnShip();
                        int delay = 5000;
                        while (vessel.ShipStatus != "Online" && vessel.ShipStatus != "Attacking") {
                            Thread.Sleep(delay);
                            vessel = engine.getOwnShip();
                            /*if (delay < 10000) {
                                delay += 1000;
                            } else if (delay < 600000) {
                                delay += 60000;
                            }*/
                        }
                        delay = 60000;
                    }
                    url = @"http://api2.pixelstarships.com/BattleService/RevengeBattle?opponentUserId=" + i + "&accessToken=" + accessToken;
                    content = httpCommunications.getPostContent(url);
                    if (!content.Contains("Can't find any available ship.")) {
                        doc = XDocument.Parse(content);
                        result = (Dictionary<string,object>)xml2Dictionary.Parse(doc.Root);
                        result = Helpers.advanceDict(result,"BattleService","RevengeBattle","Battle");
                        combat = combatRequestReply.Dictionary2combatRequestReply(engine,(Dictionary<string,object>)result);
                        fileName = combat.DefendingUser.Id + " - " + combat.DefendingUser.Name + " - " + combat.DefendingUser.UserType + " - " + combat.DefendingUser.Email + " - " + DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss");
                        //fileName = fileName.Replace("*","_asterisk_").Replace(":","_colon_").Replace("/", "_slash_")
                        //    .Replace("\\", "_backslash_").Replace("|", "_verticalBar_").Replace("<","_lessThan_")
                        //    .Replace(">","_greatedThan_");
                        //string illegal = "\"M\"\\a/ry/ h**ad:>> a\\/:*?\"| li*tt|le|| la\"mb.?";
                        string regexSearch = new string(System.IO.Path.GetInvalidFileNameChars()) + new string(System.IO.Path.GetInvalidPathChars());
                        Regex r = new Regex(string.Format("[{0}]",Regex.Escape(regexSearch)));
                        fileName = r.Replace(fileName,"");
                        File.WriteAllText("c:\\PSS\\ships\\" + fileName + ".ship.xml",(string)result["a_DefendingShipXml"]);
                        File.WriteAllText("c:\\PSS\\users\\" + fileName + ".user.xml",(string)result["a_DefendingUserXml"]);
                        if (DateTime.Now - combat.DefendingUser.LastLoginDate > TimeSpan.FromDays(15)) {
                            File.AppendAllText("c:\\PSS\\innactivePlayerList",", " + combat.DefendingUser.Id);
                        } else {

                        }
                    }
                    //Thread.Sleep(rnd.Next(3000,5000));
                } catch (Exception oEx) {
                    Console.WriteLine(i + " - " + oEx.ToString());
                    i--;
                    Thread.Sleep(15000);
                }
            }
        }
    }
}

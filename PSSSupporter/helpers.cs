using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Globalization;
using System.IO;

namespace PSSSupporter {
    public static class Helpers { //http://stackoverflow.com/questions/3624894/enum-to-string
        public static string GetCustomDescription(object objEnum) {
            var fi = objEnum.GetType().GetField(objEnum.ToString());
            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute),false);
            return (attributes.Length > 0) ? attributes[0].Description : objEnum.ToString();
        }
        public static string Description(this Enum value) {
            return GetCustomDescription(value);
        }
        /*
            public enum Tile
            {
                [Description("E")]
                Empty,

                [Description("W")]
                White,

                [Description("B")]
                Black
            }

            Console.Write(Tile.Description());
         */
        public static void DictionaryToAttributes(Dictionary<string,object> dict,object target) {
            foreach (var property in dict.Keys) {
                if (property.StartsWith("a_")) {
                    string propName = property.Substring(2);
                    int intValue;
                    float floatValue;
                    DateTime dateTimeValue;
                    string value = (string)dict[property];
                    System.Reflection.FieldInfo field = target.GetType().GetField(propName);
                    object finalValue = null;
                    if (field.FieldType.Equals(typeof(DateTime)) && string.IsNullOrEmpty(value)) {
                        finalValue = DateTime.MinValue;
                    } else {
                        finalValue = Convert.ChangeType(value,field.FieldType);
                    }
                    field.SetValue(target,finalValue);
                    //if (value == "true") {
                    //    field.SetValue(target, true);
                    //} else if (value == "false") {
                    //    field.SetValue(target, false);
                    //} else if (propName != "Mask" && !propName.EndsWith("Ids") && !propName.Contains(",")
                    //        && int.TryParse(value, out intValue)) {
                    //    field.SetValue(target, intValue);
                    //} else if(propName != "Mask" && !propName.EndsWith("Ids") && !propName.Contains(",")
                    //        && float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out floatValue)) {
                    //    field.SetValue(target, floatValue);
                    //} else if (DateTime.TryParse(value, out dateTimeValue)) {
                    //    field.SetValue(target, dateTimeValue);
                    //} else {
                    //    if (field.FieldType.FullName == "System.String") {
                    //        field.SetValue(target, value);
                    //    } else if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                    //        field.SetValue(target, null);
                    //    } else {
                    //        //???????
                    //    }
                    //}
                }
            }
        }
        public static Dictionary<string,object> advanceDict(Dictionary<string,object> dict,params string[] listToAdvance) {
            Dictionary<string,object> currentStep = dict;
            foreach (string toAdvance in listToAdvance) {
                dict = ((Dictionary<string,object>)dict[toAdvance]);
            }
            return dict;
        }
        public static void CorrectDictionaryListFormat(Dictionary<string,object> dict,params string[] listsToCorrect) {
            foreach (string listName in listsToCorrect) {
                if (!dict.Keys.Contains("l_" + listName)) {
                    if (dict.Keys.Contains(listName)) {
                        List<object> list = new List<object>();
                        list.Add(dict[listName]);
                        dict.Add("l_" + listName,list);
                    } else {
                        dict.Add("l_" + listName,new List<object>());
                    }
                }
            }
        }
        public static void PopulateArray<T>(this T[] arr,ref int arrayCount,params T[] values) {
            int offset = arrayCount;
            for (int i = 0; i < values.Length; i++) {
                arr[i + offset] = values[i];
                arrayCount++;
            }
        }
        public static string CSVEncoding(string toEncode) {
            return toEncode.Replace("\"","\"\"");
        }
        public static void Serialize(Dictionary<int,int> dictionary,Stream stream) {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(dictionary.Count);
            foreach (var kvp in dictionary) {
                writer.Write(kvp.Key);
                writer.Write(kvp.Value);
            }
            writer.Flush();
        }

        public static Dictionary<int,int> Deserialize(Stream stream) {
            BinaryReader reader = new BinaryReader(stream);
            int count = reader.ReadInt32();
            var dictionary = new Dictionary<int,int>(count);
            for (int n = 0; n < count; n++) {
                var key = reader.ReadInt32();
                var value = reader.ReadInt32();
                dictionary.Add(key,value);
            }
            return dictionary;
        }
        public static void SerializeToFile(Dictionary<int,int> dictionary,string file) {
            var fileStream = File.Create("C:\\Path\\To\\File");
            Serialize(dictionary,fileStream);
            //myOtherObject.InputStream.Seek(0,SeekOrigin.Begin);
            //myOtherObject.InputStream.CopyTo(fileStream);
            fileStream.Close();
        }
        public static Dictionary<int,int> DeserializeFromFile(string file) {
            if (!File.Exists(file)) {
                return new Dictionary<int,int>();
            }
            var fileStream = File.OpenRead(file);
            Dictionary<int,int> result = Deserialize(fileStream);
            fileStream.Close();
            return result;
        }
    }
}

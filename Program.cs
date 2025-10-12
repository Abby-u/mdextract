using AssetStudio;
using Fmod5Sharp;
using Fmod5Sharp.CodecRebuilders;
using OdinSerializer;
using System;
using System.Collections;
using System.Collections.Generic;
//using System.Collections.Specialized;
using System.IO;
//using System.Net.NetworkInformation;
//using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
//using System.Xml.Linq;
//using UnityEngine;
//using static AssetStudio.BundleFile;

[Serializable]

public class OriginNoteConfigData
{
    public string id { get; set; }
    public string scene { get; set; }
    public string ibms_id { get; set; }
    public string uid { get; set; }
    public string mirror_uid { get; set; }
    public string des { get; set; }
    public string prefab_name { get; set; }
    public string speed { get; set; }
    public string type { get; set; }
    public string pathway { get; set; }
    public string score { get; set; }
    public string fever { get; set; }
    public string effect { get; set; }
    public string boss_action { get; set; }
    public string damage { get; set; }
    public string key_audio { get; set; }
    public string left_perfect_range { get; set; }
    public string left_great_range { get; set; }
    public string right_perfect_range { get; set; }
    public string right_great_range { get; set; }
}

public class NoteConfigData
{
    public string id;
    public string ibms_id;
    public string uid;
    public string mirror_uid;
    public string scene;
    public string des;
    public string prefab_name;
    public uint type;
    public string effect;
    public string key_audio;
    public string boss_action;
    public decimal left_perfect_range;
    public decimal left_great_range;
    public decimal right_perfect_range;
    public decimal right_great_range;
    public int damage;
    public int pathway;
    public int speed;
    public int score;
    public int fever;
}

public class MusicConfigData
{
    public int id;
    public decimal time;
    public string note_uid;
    public decimal length;
    public bool blood;
    public int pathway;
}

public class MusicData
{
    public short objId;
    public decimal tick;
    public MusicConfigData? configData;
    public NoteConfigData? noteData;
    public bool isLongPressing;
    public int doubleIdx;
    public bool isDouble;
    public bool isLongPressEnd;
    public decimal longPressPTick;
    public int endIndex;
    public decimal dt;
    public int longPressNum;
    public decimal showTick;
}

namespace mdextract
{
    internal class Program
    {
        private static List<OriginNoteConfigData> notedata;

        private static string path;

        private static string target;

        public static List<string> theoutput = new List<string>();

        static string normalizePath(string path)
        {
            string trimmed = path.Trim().Trim('"').Replace('/', '\\').Replace('#', ' ');
            trimmed = System.IO.Path.GetFullPath(trimmed);
            if (!trimmed.EndsWith("\\")) 
            {
                trimmed += "\\";
            }
            return trimmed;
        }

        static int ToInt(string s)
        {
            return int.TryParse(s, out var val) ? val : 0;
        }

        static decimal ToDec(string s)
        {
            return decimal.TryParse(s, out var val) ? val : 0m;
        }

        static uint ToUInt(string s)
        {
            return uint.TryParse(s, out var val) ? val : 0u;
        }
      
        static OriginNoteConfigData? FindNoteData(string s, List<OriginNoteConfigData> thething)
        {
            foreach (var note in thething)
            {
                if (note != null && note.uid != null && note.uid == s)
                {
                    return note;
                }
            }
            return null;
        }
        static string catchArgs(string[] args,string key)
        {
            for (int i=0;i<args.Length;i++)
            {
                if (args[i]==key && i+1<args.Length)
                {
                    return args[i+1];
                }
            }
            return null;
        }

        static void createChartJson(AssetStudio.Object theArray, string targetDir)
        {
            byte[] serialized = null;
            var daName = new MonoBehaviour(theArray.reader).m_Name;

            if (theArray.ToType()["serializationData"] is IDictionary dict)
            {
                if (dict["SerializedBytes"] is System.Collections.IList list)
                {
                    serialized = new byte[list.Count];
                    for (int i = 0; i < list.Count; i++)
                    {
                        serialized[i] = Convert.ToByte(list[i]);
                    }
                }
            }
            var decoded = SerializationUtility.DeserializeValue<List<MusicData>>(serialized, DataFormat.Binary);
            var daNotes = new List<object>();

            foreach (var music in decoded)
            {
                if (string.IsNullOrEmpty(music.configData.note_uid)) { continue; }

                var thisNote = FindNoteData(music.configData.note_uid, notedata);
                if (thisNote == null) { continue; }

                var daNote = new
                {
                    id = thisNote.id,
                    ibms_id = thisNote.ibms_id,
                    uid = thisNote.uid,
                    mirror_uid = thisNote.mirror_uid,
                    scene = thisNote.scene,
                    des = thisNote.des,
                    prefab_name = thisNote.prefab_name,
                    type = ToUInt(thisNote.type),
                    effect = thisNote.effect,
                    key_audio = thisNote.key_audio,
                    boss_action = thisNote.boss_action,
                    left_perfect_range = ToDec(thisNote.left_perfect_range),
                    left_great_range = ToDec(thisNote.left_great_range),
                    right_perfect_range = ToDec(thisNote.right_perfect_range),
                    right_great_range = ToDec(thisNote.right_great_range),
                    damage = ToInt(thisNote.damage),
                    pathway = ToInt(thisNote.pathway),
                    speed = ToInt(thisNote.speed),
                    score = ToInt(thisNote.score),
                    fever = ToInt(thisNote.fever)
                };

                var entry = new
                {
                    music.objId,
                    music.tick,
                    music.isLongPressing,
                    music.doubleIdx,
                    music.isDouble,
                    music.isLongPressEnd,
                    music.longPressPTick,
                    music.endIndex,
                    music.dt,
                    music.longPressNum,
                    music.showTick,
                    config = music.configData,
                    note = daNote
                };
                daNotes.Add(entry);
            }
            var whole = new
            {
                mapName = theArray.ToType()["m_Name"].ToString(),
                music = theArray.ToType()["music"].ToString(),
                scene = theArray.ToType()["scene"].ToString(),
                difficulty = ToInt(theArray.ToType()["difficulty"].ToString()),
                md5 = theArray.ToType()["md5"].ToString(),
                bpm = ToDec(theArray.ToType()["bpm"].ToString()),
                notes = daNotes.ToArray()
            };
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            string json = JsonSerializer.Serialize(whole, options);
            System.IO.File.WriteAllText(targetDir+daName+".json", json);
            theoutput.Insert(0,targetDir + daName + ".json");
            //Console.WriteLine($"{daName}.json");
        }
        static void Main(string[] args)
        {
            Console.Out.Flush();
            string exedir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string noteDir = exedir +"\\"+ "notedata.json";
            noteDir = noteDir.Replace("\n", "\\n");
            //Console.WriteLine(noteDir);
            if (!File.Exists(noteDir))
            {
                Console.WriteLine("notedata.json not found (required)");
                return;
            }
            string notedataRaw = System.IO.File.ReadAllText(noteDir);
            notedata = JsonSerializer.Deserialize<List<OriginNoteConfigData>>(notedataRaw);

            path = catchArgs(args, "-pf");
            target = catchArgs(args, "-tf");
            while (string.IsNullOrEmpty(path))
            {
                Console.WriteLine("Path to .bundle file:");
                path = Console.ReadLine();
            }
            if (string.IsNullOrEmpty(target))
            {
                Console.WriteLine("Path to output Json file:");
                target = Console.ReadLine();
                if (string.IsNullOrEmpty(target))
                {
                    target = ".";
                }
            }

            path = path.Trim().Trim('"').Replace('/', '\\').Replace('#',' ');
            path = System.IO.Path.GetFullPath(path);
            target = normalizePath(target);


            //Console.WriteLine(target);
            //Console.WriteLine(noteDir);

            var root = new System.Collections.Generic.List<AssetStudio.Object>();

            try
            {
                var am = new AssetsManager();
                am.LoadFiles(path);
                root = am.assetsFileList[0].Objects;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.Out.Flush();
            }

            //Console.WriteLine(noteDir);


            foreach (var thing in root)
            {
                if (thing.GetType() == typeof(AssetStudio.MonoBehaviour))
                {
                    createChartJson(thing,target);
                }
                else if (thing.GetType() == typeof(AssetStudio.AudioClip))
                {
                    AudioClip ac = new AudioClip(thing.reader);
                    var res = ac.m_AudioData.GetData();
                    var fsb = FsbLoader.LoadFsbFromByteArray(res).Samples[0];
                    var ogg = FmodVorbisRebuilder.RebuildOggFile(fsb);
                    System.IO.File.WriteAllBytes(target+ac.m_Name+".ogg", ogg);
                    //Console.WriteLine($"Exported {target+ac.m_Name}.ogg");
                    theoutput.Insert(theoutput.Count, target + ac.m_Name + ".ogg");
                }
            }
            //Console.WriteLine(target);
            string outputjson = JsonSerializer.Serialize(theoutput);
            Console.WriteLine(outputjson);
            return;
        }
    }
}

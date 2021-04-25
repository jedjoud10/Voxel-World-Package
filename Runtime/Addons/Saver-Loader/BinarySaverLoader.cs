using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Threading.Tasks;
public static class BinaryLoaderSaver
{
    //Load
    public static object Load(string path)
    {
        Debug.Log("Load object " + path);
        BinaryFormatter formatter = new BinaryFormatter();

        if (!File.Exists(path)) return new object();
        FileStream stream = new FileStream(path, FileMode.Open);

        object data = formatter.Deserialize(stream);
        stream.Close();
        return data;
    }
    //Save
    public static void Save(string path, object data)
    {
        Debug.Log("Save object " + path);
        BinaryFormatter formatter = new BinaryFormatter();

        FileStream stream = new FileStream(path, FileMode.Create);
        formatter.Serialize(stream, data);

        stream.Close();
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Xenogon_DataBase
{
    static Dictionary<string, XenogonBase> xenogonDB;

    public static void Init()
    {
        xenogonDB = new Dictionary<string, XenogonBase>();

        var xenogonArray = Resources.LoadAll<XenogonBase>("");
        foreach (var xenogon in xenogonArray)
        {
            if (xenogonDB.ContainsKey(xenogon.Name))
            {
                Debug.LogError($"There are two xenogon with the name {xenogon.Name}");
                continue;
            }

            xenogonDB[xenogon.Name] = xenogon;
        }
    }

    public static XenogonBase GetXenogonByName(string name)
    {
        if (!xenogonDB.ContainsKey(name))
        {
            Debug.LogError($"Xenogon with name {name} not found in the database");
            return null;
        }

        return xenogonDB[name];
    }
}

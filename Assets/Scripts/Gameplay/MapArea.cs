using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<Xenogon> wildXenogon;

    public Xenogon GetRandomWildXenogon()
    {
        var _wildXenogon = wildXenogon[Random.Range(0, wildXenogon.Count)];

        _wildXenogon.Init();
        return _wildXenogon;
    }
}

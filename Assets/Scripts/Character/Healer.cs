using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour
{
  public IEnumerator Heal(Transform player, NPCDialog dialog)
    {
        // yield return DialogManager.Instance.ShowDialog(dialog);

        Debug.Log("Temporarily disabled dialogue from healer!");

        XenogonParty playerParty = player.GetComponent<XenogonParty>();
        playerParty.XenogonList.ForEach(p => p.Heal());
        playerParty.PartyUpdated();

        yield break;        //This is temporary. Usually, this means it doesn't have to be a coroutine!
    }
}

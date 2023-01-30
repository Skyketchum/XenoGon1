using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour
{
  public IEnumerator Heal(Transform player, NPCDialog dialog)
    {
        yield return DialogManager.Instance.ShowDialog(dialog);

        var playerParty = player.GetComponent<XenogonParty>();
        playerParty.XenogonList.ForEach(p => p.Heal());
        playerParty.PartyUpdated();


    }
}

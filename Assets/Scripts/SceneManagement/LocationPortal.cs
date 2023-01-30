using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
//teleports player without switching scenes
public class LocationPortal : MonoBehaviour, IPlayerTriggerable
{
 
    [SerializeField] DestinationIdentifier destinationPortal;
    [SerializeField] Transform spawnPoint;

    PlayerController player;


    public void OnPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        this.player = player;
        Debug.Log("transition");
        StartCoroutine(Teleport());
    }

    Fader fader;

    private void Start()
    {
        fader = FindObjectOfType<Fader>();
    }

    IEnumerator Teleport()
    {

        

        GameController.Instance.PauseGame(true);
        yield return fader.FadeIn(0.5f);

       

        Debug.Log("Logging from portal after scene switch");

        var destPortal = FindObjectsOfType<LocationPortal>().First(x => x != this && x.destinationPortal == this.destinationPortal);
        //new way to set position by exposing character class property
        player.Character.SetPositionAndSnapToTile(destPortal.SpawnPoint.position);

        yield return fader.Banner(1f);

        yield return fader.BannerOff(1f);


        yield return fader.FadeOut(0.5f);

        GameController.Instance.PauseGame(false);

        //original way to set position 
        //  player.transform.position = destPortal.SpawnPoint.position;

   
    }

    public Transform SpawnPoint => spawnPoint;
}

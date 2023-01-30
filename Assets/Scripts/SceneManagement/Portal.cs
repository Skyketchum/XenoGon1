using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] int sceneToLoad = -1;
    [SerializeField] DestinationIdentifier destinationPortal;
    [SerializeField] Transform spawnPoint;

    PlayerController player;


    public void OnPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        this.player = player;
        Debug.Log("transition");
        StartCoroutine(SwitchScene());
    }

    Fader fader;

    private void Start()
    {
        fader = FindObjectOfType<Fader>();
    }

    IEnumerator SwitchScene()
    {

        DontDestroyOnLoad(gameObject);

        GameController.Instance.PauseGame(true);
        yield return fader.FadeIn(0.5f);

       yield return SceneManager.LoadSceneAsync(sceneToLoad);

        Debug.Log("Logging from portal after scene switch");

       var destPortal = FindObjectsOfType<Portal>().First(x => x != this && x.destinationPortal == this.destinationPortal);
        //new way to set position by exposing character class property
        player.Character.SetPositionAndSnapToTile(destPortal.SpawnPoint.position);

        yield return fader.Banner(1f);

        yield return fader.BannerOff(1f);


        yield return fader.FadeOut(0.5f);

        GameController.Instance.PauseGame(false);

        //original way to set position 
      //  player.transform.position = destPortal.SpawnPoint.position;

        Destroy(gameObject);
    }

    public Transform SpawnPoint => spawnPoint;
}

public enum DestinationIdentifier
{ A, B, C, D, E, F }

      

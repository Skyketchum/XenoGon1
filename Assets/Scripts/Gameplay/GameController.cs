using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { FreeRoam, Dialog, Battle, DoubleBattle, Menu, PartyScreen, Bag, PartnerBag, EnemyPartnerBag, Cutscene, Defeat, Paused}


public class GameController : MonoBehaviour
{
    //[SerializeField] PlayerPartnerController playerPartnerController;
    //[SerializeField] TrainerPartnerController trainerPartnerController;
    [SerializeField] PlayerController playerController;
    [SerializeField] TrainerController trainerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    [SerializeField] PartyScreen partyScreen;
   // [SerializeField] DualPartyScreen dualPartyScreen;
    [SerializeField] InventoryUI inventoryUI;
    //[SerializeField] InventoryUI partnerInventory;
    //[SerializeField] InventoryUI cutsceneInventory;
    [SerializeField] SavingSystem savingSystem;


    public GameState state;

    GameState stateBeforePause;


    public SceneDetails CurrentScene { get; private set; }

    public SceneDetails PrevScene { get; private set; }

    MenuController menuController;

    public static GameController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        menuController = GetComponent<MenuController>();
        
        // Locks cursor and disables it so it doesnt show in game; Commented out for programming phase
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        Xenogon_DataBase.Init();
        Conditions_DataBase.Init();
    }

    // Start is called before the first frame update
    private void Start()
    {
        battleSystem.OnBattleOver += EndBattle;

        partyScreen.Init();

        DialogManager.Instance.OnShowDialog += () =>
        {
            state = GameState.Dialog;
        };

        DialogManager.Instance.OnCloseDialog += () =>
        {
            if (state == GameState.Dialog)
            {
                state = GameState.FreeRoam;
            }
        };

        menuController.onBack += () =>
        {
            state = GameState.FreeRoam;
        };

        menuController.onMenuSelected += OnMenuSelected;
    }

    public void CheckForSteps()
    {
        playerController.numOfSteps--;

        if (playerController.numOfSteps == 0)
        {
            Debug.Log("Xenogon Encounter");
            //numOfSteps = 10;
            playerController.numOfSteps = UnityEngine.Random.Range(playerController.stepsMin, playerController.stepsMax);
            StartBattle();
        }
    }

    public void PauseGame(bool pause)
    {
        if (pause)
        {
            stateBeforePause = state;
            state = GameState.Paused;
        }
        else
        {
            state = stateBeforePause;
        }
    }

    public void StartBattle() // on encountered xenogon, call start battle
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<XenogonParty>();
        var wildXenogon = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildXenogon();


        var wildXenogonCopy = new Xenogon(wildXenogon.Base, wildXenogon.Level);
      
        battleSystem.StartBattle(playerParty, wildXenogonCopy);
    }

    TrainerController trainer;

     public void StartTrainerBattle(TrainerController trainer) // on encountered trainer, call start battle
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        this.trainer = trainer;

        var playerParty = playerController.GetComponent<XenogonParty>();
        //var wildXenogon = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildXenogon();

        var trainerParty = trainer.GetComponent<XenogonParty>();
        // var captainParty = captainController.GetComponent<XenogonCaptainParty>();

        battleSystem.StartTrainerBattle(playerParty, trainerParty);
    }

    /* void StartFullDoubleBattle(TrainerController trainer, TrainerController trainerPartner, PartnerController partner, Player Controller player) // on encountered trainer, call start battle
    {
        state = GameState.DoubleBattle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<XenogonParty>();
        //var wildXenogon = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildXenogon();

        var trainerParty = trainer.GetComponent<XenogonParty>();
         var trainerPartnerParty = trainerpartner.GetComponent<XenogonParty>();
         var partnerParty = partner.GetComponent<XenogonCaptainParty>();

        battleSystem.StartFullDoubleBattle(playerParty, trainerParty, trainerPartnerParty, partnerParty);
    } */

    /* void StartEnemyDoubleBattle(TrainerController trainer, TrainerPartnerController trainerPartner, Player Controller player) // on encountered trainer, call start battle
{
    state = GameState.EnemyDoubleBattle;
    battleSystem.gameObject.SetActive(true);
    worldCamera.gameObject.SetActive(false);

    var playerParty = playerController.GetComponent<XenogonParty>();
    var trainerParty = trainer.GetComponent<XenogonParty>();
     var trainerPartnerParty = trainerpartner.GetComponent<XenogonParty>();


    battleSystem.StartEnemyDoubleBattle(playerParty, trainerParty, trainerPartnerParty);
} */

    /* void StartPartnerDoubleBattle(TrainerController trainer, TrainerPartnerController trainerPartner, Player Controller player) // on encountered trainer, call start battle
{
    state = GameState.EnemyDoubleBattle;
    battleSystem.gameObject.SetActive(true);
    worldCamera.gameObject.SetActive(false);

    var playerParty = playerController.GetComponent<XenogonParty>();
     var partnerParty = partner.GetComponent<XenogonParty>();
    var trainerParty = trainer.GetComponent<XenogonParty>();
     var trainerPartnerParty = trainerpartner.GetComponent<XenogonParty>();


    battleSystem.StartPartnerDoubleBattle(playerParty, trainerParty, trainerPartnerParty);
} */

    /* void StartSingleDoubleBattle(TrainerController trainer, Player Controller player) // on encountered trainer, call start battle
    {
          state = GameState.SingleDoubleBattle;
         battleSystem.gameObject.SetActive(true);
         worldCamera.gameObject.SetActive(false);

           var playerParty = playerController.GetComponent<XenogonParty>();

           var trainerParty = trainer.GetComponent<XenogonParty>();


             battleSystem.StartSingleDoubleBattle(playerParty,trainerPartnerParty);
                                 } */


      /* void StartWildDoubleBattle(PartnerController partner, Player Controller player) // on encountered trainer, call start battle
             {
        state = GameState.WildDoubleBattle;
      battleSystem.gameObject.SetActive(true);
          worldCamera.gameObject.SetActive(false);

      var playerParty = playerController.GetComponent<XenogonParty>();

         var partnerParty = partner.GetComponent<XenogonParty>();


     battleSystem.StartWildDoubleBattle(playerParty,partnerParty);
                         } */

    public void OnEnterTrainersView(TrainerController trainer)
    {
        state = GameState.Cutscene;
        StartCoroutine(trainer.TriggerTrainerBattle(playerController));
    }



    //var partnerParty = partner.GetComponent<XenogonCaptainParty>();
    // var trainerParty = trainerController.GetComponent<XenogonParty>();
    // var captainParty = captainController.GetComponent<XenogonCaptainParty>();


    void EndBattle(bool won)
    {
        if(trainer != null && won == true)
        {
            trainer.BattleLost();
            trainer = null;
        }
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }
    
    // Update is called once per frame
  private void Update()
    {
        if (state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();

            if (Input.GetKeyDown(KeyCode.Return))
            {
                menuController.OpenMenu();
                state = GameState.Menu;
            }
        }
        else if (state == GameState.Dialog)
        {
            DialogManager.Instance.HandleUpdate();
        }
        else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
        else if (state == GameState.Menu)
        {
            menuController.HandleUpdate();
        }
        else if (state == GameState.PartyScreen)
        {
            Action onSelected = () =>
            {
                //TODO: Go to summary screen
            };

            Action onBack = () =>
            {
                partyScreen.gameObject.SetActive(false);
                state = GameState.FreeRoam;
            };
            partyScreen.HandleUpdate(onSelected, onBack);
        }
        else if (state == GameState.Bag)
        {
            Action onBack = () =>
            {
                inventoryUI.gameObject.SetActive(false);
                state = GameState.FreeRoam;
            };

            inventoryUI.HandleUpdate(onBack);
        }
    }

    public void SetCurrentScene(SceneDetails currScene)
    {
        PrevScene = CurrentScene;
        CurrentScene = currScene;
    }

    void OnMenuSelected(int selectedItem)
    {
        if (selectedItem == 0)
        {
            //Xenogon
            partyScreen.gameObject.SetActive(true);
            partyScreen.openPanel = true;
            state = GameState.PartyScreen;
        }
        else if (selectedItem == 1)
        {
            //Bag
            inventoryUI.gameObject.SetActive(true);
            state = GameState.Bag;
        }
        else if (selectedItem == 2)
        {
            //Save
            SavingSystem.i.Save("saveSlot1");
            state = GameState.FreeRoam;
        }
        else if (selectedItem == 3)
        {
            //Load
            SavingSystem.i.Load("saveSlot1");
            state = GameState.FreeRoam;
        }
        else if (selectedItem == 4)
        {
            //Quit
            Debug.Log("Quitting Game...");
            Application.Quit();
            state = GameState.FreeRoam;
        }
    }

    public GameState State => state;
}

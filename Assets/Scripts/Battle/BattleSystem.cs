using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, AboutToUse, MoveToForget, BattleOver, Bag} //setting up enumeration for player battle states
public enum BattleAction { Move, SwitchXenogon, UseItem, Run}
public class BattleSystem : MonoBehaviour
{
    //line 746 Open Bag function
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    // [SerializeField] BattleUnit enemyPartnerUnit;
    // [SerializeField] BattleUnit playerPartnerUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    // [SerializeField] Image playerPartnerImage;
    // [SerializeField] Image enemyPartnerImage;
    [SerializeField] Image trainerImage;

    [SerializeField] GameObject baitSprite;
    [SerializeField] GameObject baitStartPosition;
    [SerializeField] GameObject baitEndPosition;

    [SerializeField] GameObject xenoCrystal;

    [SerializeField] MoveSelectionUI moveSelectionUI;
    [SerializeField] InventoryUI battleInventoryUI;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;
    int currentMovePage;
    int currentSelection;
    int xenogonMoveCount;
    bool aboutToUseChoice = true;

    // XenogonParty enemyPartnerParty;
   // XenogonParty playerPartnerParty;
    XenogonParty playerParty;
    XenogonParty trainerParty;

    Xenogon wildXenogon;

    
    //bool isDoubleBattle = false;
    //bool isDoubleTrainerBattle = false;

    bool isTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;

    int escapeAttempts;
    MoveBase moveToLearn;

    public void StartBattle(XenogonParty playerParty, Xenogon wildXenogon)
    {
        this.playerParty = playerParty;
        this.wildXenogon = wildXenogon;
        player = playerParty.GetComponent<PlayerController>();
        isTrainerBattle = false;

        StartCoroutine(SetupBattle());
    }

    public void StartTrainerBattle(XenogonParty playerParty, XenogonParty trainerParty)
    {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;
        isTrainerBattle = true;

        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();

        StartCoroutine(SetupBattle());
    }

    /*
     * 
     *  
     *  
     *  
     *  
     *  public void StartFullDoubleBattle(XengonParty trainerParty, XenogonParty trainerPartnerParty, XenogonParty partnerParty, playerParty)
    {
        this.playerParty = playerParty;
        this.partnerParty = partnerParty;
        this.trainerParty = trainerParty;
        this.trainerPartnerParty = trainerPartnerParty;

        isTrainerBattle = true;

        player = playerParty.GetComponent<PlayerController>();
        partner = partnerParty.GetComponent<PartnerController>();
        enemyPartner = trainerParty.GetComponent<EnemyPartnerController>();

        StartCoroutine(SetupBattle());

        }

         public void StartTrainerBattle(XenogonParty playerParty, XenogonParty trainerParty)
    {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;
        isTrainerBattle = true;

        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();

        StartCoroutine(SetupBattle());






    }  */

    public IEnumerator SetupBattle() //changed to coroutine for the dialog box word animations
    {
        playerUnit.Clear();
        enemyUnit.Clear();

        if (!isTrainerBattle)
        {

            playerUnit.Setup(playerParty.GetHealthyXenogon());
            enemyUnit.Setup(wildXenogon);

            dialogBox.SetMoveNames(playerUnit.Xenogon.Moves); //set moves for the player's current xenogon

            yield return dialogBox.TypeDialog($"A wild {enemyUnit.Xenogon.Base.Name} appeared!");


        }
        else
        {
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);
            // enemyPartnerUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);
            //captainImage.gameObject.SetActive(true);

            playerImage.sprite = player.Sprite;

            trainerImage.sprite = trainer.Sprite;

            //  captainImage.sprite = captain.Sprite;

            yield return dialogBox.TypeDialog($"{trainer.Name} wants to battle!");


            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);

            var enemyXenogon = trainerParty.GetHealthyXenogon();
            enemyUnit.Setup(enemyXenogon);

            yield return dialogBox.TypeDialog($"{trainer.Name} sent out, {enemyXenogon.Base.Name}!");


            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerXenogon = playerParty.GetHealthyXenogon();
            playerUnit.Setup(playerXenogon);
            yield return dialogBox.TypeDialog($"Gooo {playerXenogon.Base.Name}!");
            dialogBox.SetMoveNames(playerUnit.Xenogon.Moves);
        }

        escapeAttempts = 0;
        partyScreen.Init();

        currentAction = 0;
        currentMove = 0;

        ActionSelection(); //call and set the first battle state
    }


    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.XenogonList.ForEach(p => p.OnBattleOver());
        playerUnit.Hud.ClearData();
        enemyUnit.Hud.ClearData();
        dialogBox.EnableActionSelector(false); //--> debugging
        OnBattleOver(won);
    }

    void ActionSelection() //Battle State: PlayerAction = enable action selector and display new dialog in the dialog box
    {
        StartCoroutine(dialogBox.TypeDialog("Choose an action"));
        state = BattleState.ActionSelection;
        dialogBox.EnableActionSelector(true);
    }

    void MoveSelection() //Battle State: PlayerMove = player selects from available xenogon moves
    {
        currentMovePage = 0;
        xenogonMoveCount = playerUnit.Xenogon.Moves.Count;
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator RunTurns(BattleAction playerAction) // New function for controlling how the turns run along with proper turn orders
    {
        state = BattleState.RunningTurn;

        if (playerAction == BattleAction.Move)
        {
            playerUnit.Xenogon.CurrentMove = playerUnit.Xenogon.Moves[currentMove];
            enemyUnit.Xenogon.CurrentMove = enemyUnit.Xenogon.GetRandomMove();

            int playerMovePriority = playerUnit.Xenogon.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Xenogon.CurrentMove.Base.Priority;

            // Check who goes first; if there is a move with high priority being used, calculate that into the turn order
            bool playerGoesFirst = true;
            if (enemyMovePriority > playerMovePriority)
                playerGoesFirst = false;
            else if (enemyMovePriority == playerMovePriority)
                playerGoesFirst = playerUnit.Xenogon.Dex >= enemyUnit.Xenogon.Dex;

            var fisrtUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            var secondXenogon = secondUnit.Xenogon;

            // First turn
            yield return RunMove(fisrtUnit, secondUnit, fisrtUnit.Xenogon.CurrentMove);
            yield return RunAfterTurn(fisrtUnit, secondUnit);
            if (state == BattleState.BattleOver) yield break;

            if (secondXenogon.HP > 0) // If the Xenogon going second fainted, dont run next turn
            {
                // Second turn
                yield return RunMove(secondUnit, fisrtUnit, secondUnit.Xenogon.CurrentMove);
                yield return RunAfterTurn(secondUnit, fisrtUnit);
                if (state == BattleState.BattleOver) yield break;
            }
        }
        else
        {
            if (playerAction == BattleAction.SwitchXenogon)
            {
                var selectedMember = partyScreen.SelectedMember;
                state = BattleState.Busy;
                yield return SwitchXenogon(selectedMember);
            }
            else if (playerAction == BattleAction.UseItem)
            {
                //this is handled from the item screen, so do nothing and skip to enemy move
                dialogBox.EnableActionSelector(false);
              
                
                // yield return ThrowBait();
            }
            else if (playerAction == BattleAction.Run)
            {
                yield return TryToEscape();
            }

            // Enemy turn
            var enemyMove = enemyUnit.Xenogon.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit, playerUnit);
            if (state == BattleState.BattleOver) yield break;
        }

        if (state != BattleState.BattleOver) //Return back to action selection afterwards
        {
            playerUnit.Xenogon.MakeMoveList();
            enemyUnit.Xenogon.MakeMoveList();
            dialogBox.SetMoveNames(playerUnit.Xenogon.Moves);
            ActionSelection();
        }
    }
    IEnumerator AboutToUse(Xenogon newXenogon)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"{trainer.Name} is about to use {newXenogon.Base.Name}. Do you want to use a different Xenogon?");

        state = BattleState.AboutToUse;
        dialogBox.EnableChoiceBox(true);
    }

    IEnumerator ChooseMoveToForget(Xenogon xenogon, MoveBase newMove)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"Choose a move you want to forget.");
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(xenogon.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;

        state = BattleState.MoveToForget;
    }


    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Moves move) // handle move functionality
    {

        bool canRunMove = sourceUnit.Xenogon.OnBeforeMove();

        int moveCost = move.MPCOST;

        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Xenogon);
            yield return sourceUnit.Hud.WaitForHPUpdate();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Xenogon); //if returns true

        if (sourceUnit.Xenogon.Status == null)
            moveCost = move.MPCOST;
        else if (sourceUnit.Xenogon.Status.Id == ConditionID.crs)
            moveCost = move.MPCOST * 2;

        sourceUnit.Xenogon.DecreaseMP(moveCost);
        yield return dialogBox.TypeDialog($"{sourceUnit.Xenogon.Base.name} used {move.Base.name}.");

        if (CheckIfMoveHits(move, sourceUnit.Xenogon, targetUnit.Xenogon))
        {
            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Xenogon, targetUnit.Xenogon, move.Base.Target);
            }
            else
            {
                WasStaggered(sourceUnit.Xenogon, targetUnit.Xenogon);
                WasDrenched(targetUnit.Xenogon);
                var damageDetails = targetUnit.Xenogon.TakeDamage(move, sourceUnit.Xenogon);
                yield return targetUnit.Hud.WaitForHPUpdate();
                yield return ShowDamageDetails(damageDetails);
                yield return new WaitForSeconds(1f);
                sourceUnit.Xenogon.OnAfterAttack();
            }

            if (move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Xenogon.HP > 0)
            {
                foreach (var secondary in move.Base.Secondaries)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondary.Chance)
                        yield return RunMoveEffects(secondary, sourceUnit.Xenogon, targetUnit.Xenogon, secondary.Target);
                }
            }

            if (targetUnit.Xenogon.HP <= 0)
            {
                yield return HandleXenogonFainted(targetUnit);
            }
        }
        else
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Xenogon.Base.name}'s attack missed.");
        }


    }


    IEnumerator RunMoveEffects(MoveEffects effects, Xenogon source, Xenogon enemy, MoveTarget moveTarget)
    {
        // Stat Boosting
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
                source.ApplyBoosts(effects.Boosts);
            else
                enemy.ApplyBoosts(effects.Boosts);
        }

        // Status Condition
        if (effects.Status != ConditionID.none)
        {
            enemy.SetStatus(effects.Status);
        }

        // Volatile Status Condition
        if (effects.VolatileStatus != ConditionID.none)
        {
            enemy.SetVolatileStatus(effects.VolatileStatus);
        }


        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(enemy);

    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit, BattleUnit targetUnit)
    {
        if (state == BattleState.BattleOver) yield break;
        yield return new WaitUntil(() => state == BattleState.RunningTurn); // Waiting for the state to return back to RunningTurn

        // Statuses like burn or psn will hurt the xenogon after the turn
        sourceUnit.Xenogon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Xenogon);
         yield return sourceUnit.Hud.WaitForHPUpdate();

        if (WasSapped(sourceUnit.Xenogon))
        {
            targetUnit.Xenogon.DecreaseHP(-(sourceUnit.Xenogon.MaxHP / 16)); //adding hp to the opposing xenogon that isnt sapped
             yield return targetUnit.Hud.WaitForHPUpdate();
            yield return dialogBox.TypeDialog($"{targetUnit.Xenogon.Base.name} healed from sapped.");
            yield return new WaitForSeconds(1f);
        }

        if (sourceUnit.Xenogon.HP <= 0)
        {
            yield return HandleXenogonFainted(sourceUnit);
            yield return new WaitUntil(() => state == BattleState.RunningTurn);
        }
    }

    bool CheckIfMoveHits(Moves move, Xenogon source, Xenogon target)
    {
        int moveAccuracy = 95;

        if (source.Status == null)
            moveAccuracy = 95;
        else if (source.Status.Id == ConditionID.dsy)
            moveAccuracy = 75;

        if (move.Base.AlwaysHits)
        {
            return true;
        }
        else if (Math.Abs(source.Dex - target.Dex) >= 50)
        {
            if (source.Dex > target.Dex)
                moveAccuracy += 5;
            else
                moveAccuracy -= 5;

            //return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
        }

        /*
        Debug.Log(source.Dex); //<-- Attacker Dex
        Debug.Log(target.Dex); //<-- Target Dex
        Debug.Log(moveAccuracy); //<-- Showing Accuracy
        */
        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    bool WasSapped(Xenogon sourceUnit)
    {
        if (sourceUnit.Status == null)
        {
            return false;
        }
        else if (sourceUnit.Status.Id == ConditionID.sap)
        {
            return true;
        }
        else
            return false;
    }

    void WasStaggered(Xenogon sourceUnit, Xenogon targetUnit)
    {
        if (sourceUnit.Status == null)
        {
            targetUnit.cannotCrit = false;
        }
        else if (sourceUnit.Status.Id == ConditionID.stg)
        {
            targetUnit.cannotCrit = true;
        }

        if (targetUnit.Status == null)
        {
            targetUnit.Staggered = false;
        }
        else if (targetUnit.Status.Id == ConditionID.stg)
        {
            targetUnit.Staggered = true;
        }
    }

    void WasDrenched(Xenogon targetUnit)
    {
        if (targetUnit.VolatileStatus == null)
        {
            targetUnit.Drenched = false;
        }
        else if (targetUnit.VolatileStatus.Id == ConditionID.drh)
        {
            targetUnit.Drenched = true;
        }
    }

    IEnumerator ShowStatusChanges(Xenogon xenogon)
    {
        while (xenogon.StatusChanges.Count > 0)
        {
            var message = xenogon.StatusChanges.Dequeue();

            yield return dialogBox.TypeDialog(message);
        }
    }

    IEnumerator HandleXenogonFainted(BattleUnit faintedUnit)
    {
        yield return dialogBox.TypeDialog($"{faintedUnit.Xenogon.Base.name} fainted.");
        yield return new WaitForSeconds(2f);

        if (!faintedUnit.IsPlayerUnit)
        {
            // Exp Gain
            int expYield = faintedUnit.Xenogon.Base.ExpYield;
            int enemyLevel = faintedUnit.Xenogon.Level;
            float trainerBonus = (isTrainerBattle) ? 1.5f : 1f;

            int expGain = Mathf.FloorToInt((expYield * enemyLevel * trainerBonus) / 7);
            playerUnit.Xenogon.Exp += expGain;
            yield return dialogBox.TypeDialog($"{playerUnit.Xenogon.Base.name} gained {expGain} exp.");
            yield return playerUnit.Hud.UpdateExp();

            // Check level up
            while (playerUnit.Xenogon.CheckForLevelUp())
            {
                playerUnit.Hud.SetLevel();
                yield return dialogBox.TypeDialog($"{playerUnit.Xenogon.Base.name} grew to level {playerUnit.Xenogon.Level}.");

                //Try to learn a new Move
                var newMove = playerUnit.Xenogon.GetLearnableMoveAtCurrLevel();
                if (newMove != null)
                {
                    if (playerUnit.Xenogon.Moves.Count < XenogonBase.MaxNumOfMoves)
                    {
                        playerUnit.Xenogon.LearnMove(newMove.Base);
                        yield return dialogBox.TypeDialog($"{playerUnit.Xenogon.Base.name} learned {newMove.Base.name}.");
                        dialogBox.SetMoveNames(playerUnit.Xenogon.Moves);
                    }
                    else
                    {
                        yield return dialogBox.TypeDialog($"{playerUnit.Xenogon.Base.name} is trying to learn {newMove.Base.name} but already knows {XenogonBase.MaxNumOfMoves} moves.");
                        yield return ChooseMoveToForget(playerUnit.Xenogon, newMove.Base);
                        yield return new WaitUntil(() => state != BattleState.MoveToForget);
                        yield return new WaitForSeconds(2f);
                    }
                }


                yield return playerUnit.Hud.UpdateExp(true);
            }

            yield return new WaitForSeconds(1f);
        }

        CheckForBattleOver(faintedUnit);
    }

    void CheckForBattleOver(BattleUnit faintedUnit) //When the attacked xenogon faints, check to see if the battle is over
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextXenogon = playerParty.GetHealthyXenogon();
            if (nextXenogon != null)
                OpenPartyScreen();
            else
                BattleOver(false);
        }
        else
        {
            if (!isTrainerBattle)
            {
                BattleOver(true);
            }
            else
            {
                var nextXenogon = trainerParty.GetHealthyXenogon();
                if (nextXenogon != null)
                    StartCoroutine(AboutToUse(nextXenogon));
                else
                    BattleOver(true);
            }
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails) //shows the damage details; i.e: Critical, Effectiveness Bonuses
    {
        if (damageDetails.Critical > 1f)
            yield return dialogBox.TypeDialog("A critical hit!");

        if (damageDetails.TypeEffectiveness > 1f)
            yield return dialogBox.TypeDialog("Super Effective Hit!");
        else if (damageDetails.TypeEffectiveness < 1f)
            yield return dialogBox.TypeDialog("Not much damage..");
    }

    public void HandleUpdate()
    {
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelecton();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
        else if (state == BattleState.Bag)
        {
            Action onBack = () =>
            {
                battleInventoryUI.gameObject.SetActive(false);
                state = BattleState.ActionSelection;
            };

            Action<ItemBase> onItemUsed = (ItemBase usedItem) =>
            {
                StartCoroutine(OnItemUsed(usedItem));
            };

            battleInventoryUI.HandleUpdate(onBack, onItemUsed);
        }
        else if (state == BattleState.AboutToUse)
        {
            HandleAboutToUse();
        }
        else if (state == BattleState.MoveToForget)
        {
            Action<int> onMoveSelected = (moveIndex) =>
            {
                moveSelectionUI.gameObject.SetActive(false);
                if (moveIndex == XenogonBase.MaxNumOfMoves)
                {
                    //Don't learn the new move
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Xenogon.Base.Name} did not learn {moveToLearn.name}."));
                }
                else
                {
                    //Forget the selected move and learn the new move
                    var selectedMove = playerUnit.Xenogon.Moves[moveIndex].Base;
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Xenogon.Base.Name} forgot {selectedMove.name} and learned {moveToLearn.name}."));
                    playerUnit.Xenogon.Moves[moveIndex] = new Moves(moveToLearn);
                }

                moveToLearn = null;
                state = BattleState.RunningTurn;
            };

            moveSelectionUI.HandleMoveSelection(onMoveSelected);
        }
    }

    void HandleActionSelecton() //Which action to select (Fight or Run)
    {
        if ((Input.GetKeyDown(KeyCode.RightArrow)) || (Input.GetKeyDown(KeyCode.D)))
            ++currentAction;
        else if ((Input.GetKeyDown(KeyCode.LeftArrow)) || (Input.GetKeyDown(KeyCode.A)))
            --currentAction;
        else if ((Input.GetKeyDown(KeyCode.DownArrow)) || (Input.GetKeyDown(KeyCode.S)))
            currentAction += 2;
        else if ((Input.GetKeyDown(KeyCode.UpArrow)) || (Input.GetKeyDown(KeyCode.W)))
            currentAction -= 2;

        currentAction = Mathf.Clamp(currentAction, 0, 3);

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0) //Fight
            {
                MoveSelection();
            }
            else if (currentAction == 1)  //Bag
            {
                print("Open Bag");
                // StartCoroutine(RunTurns(BattleAction.UseItem));

                OpenBag();

            }
            else if (currentAction == 2) // Xenogon
            {
                OpenPartyScreen();
            }
            else if (currentAction == 3) //Run
            {
                //StartCoroutine(RunTurns(BattleAction.Run)); <-- actual way to run from battles
                BattleOver(true); // <-- this is for debugging so the run option automatically runs from every battle no matter what
            }
        }
    }

    void HandleMoveSelection() //Which of the 9 moves to select / switching pages
    {
        if ((Input.GetKeyDown(KeyCode.RightArrow)) || (Input.GetKeyDown(KeyCode.D)))
            ++currentSelection;
        else if ((Input.GetKeyDown(KeyCode.LeftArrow)) || (Input.GetKeyDown(KeyCode.A)))
            --currentSelection;
        else if ((Input.GetKeyDown(KeyCode.DownArrow)) || (Input.GetKeyDown(KeyCode.S)))
            currentSelection += 2;
        else if ((Input.GetKeyDown(KeyCode.UpArrow)) || (Input.GetKeyDown(KeyCode.W)))
            currentSelection -= 2;

        currentSelection = Mathf.Clamp(currentSelection, 0, 3);

        switch (currentMovePage)
        {
            case 0:
                break;
            case 1:
                if (xenogonMoveCount < 4)
                    currentSelection = 3;
                break;
            case 2:
                if (xenogonMoveCount < 7)
                    currentSelection = 3;
                break;
        }

        if (currentSelection == 3)
        {
            dialogBox.UpdateMoveSelection(true, currentMovePage, playerUnit.Xenogon.Moves[currentMove]);
        }
        else
        {
            if (currentSelection + (currentMovePage * 3) > xenogonMoveCount - 1)
                currentMove = 0;
            else
                currentMove = currentSelection + (currentMovePage * 3);
            dialogBox.UpdateMoveSelection(false, currentMovePage, playerUnit.Xenogon.Moves[currentMove], currentMove, playerUnit.Xenogon);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentSelection == 3)
            {
                currentMovePage += 1;
                if (currentMovePage > 2)
                    currentMovePage = 0;
                dialogBox.SetActiveMovePage(enabled, currentMovePage);
            }
            else
            {
                StartCoroutine(HasMPForMove());
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }

    IEnumerator HasMPForMove()
    {
        state = BattleState.Busy;
        int moveCost = playerUnit.Xenogon.Moves[currentMove].MPCOST;

        if (playerUnit.Xenogon.Status == null)
            moveCost = playerUnit.Xenogon.Moves[currentMove].MPCOST;
        else if (playerUnit.Xenogon.Status.Id == ConditionID.crs)
            moveCost = playerUnit.Xenogon.Moves[currentMove].MPCOST * 2;

        if (playerUnit.Xenogon.MP < moveCost) //adding logic for stopping moves if not enough MP
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            yield return dialogBox.TypeDialog($"{playerUnit.Xenogon.Base.name} does not have enough MP .");
            yield return new WaitForSeconds(1f);
            dialogBox.EnableMoveSelector(true);
            dialogBox.EnableDialogText(false);
            state = BattleState.MoveSelection;
        }
        else
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        }
    }

    void OpenBag()
    {
        state = BattleState.Bag;
        battleInventoryUI.gameObject.SetActive(true);
    }

    void OpenPartyScreen()
    {
        partyScreen.CalledFrom = state;
        state = BattleState.PartyScreen;
        enemyUnit.Hud.HideStatus();
        playerUnit.Hud.HideStatus();
        partyScreen.gameObject.SetActive(true);
    }

    void HandlePartySelection()
    {
        Action onSelected = () =>
        {
            var selectedMember = partyScreen.SelectedMember;
            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("You cannot call on a knocked out Xenogon!!!");
                return;
            }

            if (selectedMember == playerUnit.Xenogon)
            {
                partyScreen.SetMessageText("You cannot switch with the same Xenogon.");
                return;
            }

            if (playerUnit.Xenogon.VolatileStatus == null)
            {
                //need to check if null first because if it is then the next line will break from checking the ID of a null element
            }
            else if (playerUnit.Xenogon.VolatileStatus.Id == ConditionID.drh) //if drenched, cure on swap
            {
                playerUnit.Xenogon.CureVolatileStatus();
            }

            if (partyScreen.CalledFrom == BattleState.ActionSelection)
            {
                StartCoroutine(RunTurns(BattleAction.SwitchXenogon));
            }
            else
            {
                state = BattleState.Busy;
                bool isTrainerAboutToUse = partyScreen.CalledFrom == BattleState.AboutToUse;
                StartCoroutine(SwitchXenogon(selectedMember, isTrainerAboutToUse));
            }

            partyScreen.CalledFrom = null;
        };

        Action onBack = () =>
        {
            if (playerUnit.Xenogon.HP <= 0)
            {
                partyScreen.SetMessageText("You have to choose a Xenogon to continue");
                return;
            }
            partyScreen.gameObject.SetActive(false);
            if (partyScreen.CalledFrom == BattleState.AboutToUse)
            {
                StartCoroutine(SendNextTrainerXenogon());
            }
            else
                ActionSelection();
            enemyUnit.Hud.ShowStatus();
            playerUnit.Hud.ShowStatus();

            partyScreen.CalledFrom = null;
        };

        partyScreen.HandleUpdate(onSelected, onBack);
    }
    void HandleAboutToUse()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            aboutToUseChoice = !aboutToUseChoice;

        dialogBox.UpdateChoiceBox(aboutToUseChoice);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableChoiceBox(false);
            if (aboutToUseChoice == true)
            {
                // Yes Option
                OpenPartyScreen();
            }
            else
            {
                //no option

                StartCoroutine(SendNextTrainerXenogon());
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableChoiceBox(false);
            StartCoroutine(SendNextTrainerXenogon());

        }
    }

    IEnumerator SwitchXenogon(Xenogon newXenogon, bool isTrainerAboutToUse=false)
    {
        partyScreen.gameObject.SetActive(false);
        //enemyUnit.Hud.ShowStatus();
        //playerUnit.Hud.ShowStatus();

        if (playerUnit.Xenogon.HP > 0)
        {
            yield return dialogBox.TypeDialog($"Come back {playerUnit.Xenogon.Base.Name}!!");

            // playerUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newXenogon);
        currentAction = 0;
        currentMove = 0;

        dialogBox.SetMoveNames(newXenogon.Moves); //set moves for the player's current xenogon

        yield return dialogBox.TypeDialog($"Go {newXenogon.Base.Name}!");

        if (isTrainerAboutToUse)
            StartCoroutine(SendNextTrainerXenogon());
        else
            state = BattleState.RunningTurn;
    }

    IEnumerator SendNextTrainerXenogon()
    {
        state = BattleState.Busy;

        var nextXenogon = trainerParty.GetHealthyXenogon();

        enemyUnit.Setup(nextXenogon);

        yield return dialogBox.TypeDialog($"{trainer.Name} sent out {nextXenogon.Base.Name}!");

        state = BattleState.RunningTurn;
    }

    IEnumerator OnItemUsed(ItemBase usedItem)
    {
        state = BattleState.Busy;
        battleInventoryUI.gameObject.SetActive(false);

        if (usedItem is BaitItem)
        {
          yield return  ThrowBait((BaitItem)usedItem); //conversion to bait item
        }

        StartCoroutine(RunTurns(BattleAction.UseItem));
    }

    IEnumerator ThrowBait(BaitItem baitItem) //adding parameter for bait selection
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't steal the trainer's Xenogon!");
            state = BattleState.RunningTurn;
            yield break;
        }

        yield return dialogBox.TypeDialog($"{player.Name} used a {baitItem.Name.ToUpper()}!");

        var baitStart = baitStartPosition.transform.position;
        var baitEnd = baitEndPosition.transform.position;
        var baitObject = Instantiate(baitSprite, baitStart, Quaternion.identity);
        baitObject.SetActive(true);
        var bait = baitObject.GetComponent<SpriteRenderer>();
        bait.sprite = baitItem.Icon;
        var crystal = xenoCrystal.GetComponent<SpriteRenderer>();

        //Animations
        yield return bait.transform.DOJump(baitEnd, 2f, 1, 1f).WaitForCompletion();
        yield return enemyUnit.PlayTakeBaitAnimation(baitObject);

        yield return dialogBox.TypeDialog("...", 40f);
        
        if (TryToCatchXenogon(enemyUnit.Xenogon, baitItem))
        {
            var originalPos = xenoCrystal.transform.position;
            xenoCrystal.SetActive(true);
            yield return enemyUnit.PlayCaptureAnimation();
            yield return crystal.transform.DOMoveY(xenoCrystal.transform.position.y - 0.95f, 0.5f).WaitForCompletion();
            yield return dialogBox.TypeDialog($"{enemyUnit.Xenogon.Base.Name} was caught!");

            yield return new WaitForSeconds(1f);
            playerParty.AddXenogon(enemyUnit.Xenogon);
            yield return new WaitForSeconds(2f);

            xenoCrystal.SetActive(false);
            crystal.transform.position = originalPos;

            BattleOver(true);
        }
        else
        {
            yield return dialogBox.TypeDialog($"{enemyUnit.Xenogon.Base.Name} didn't take the bait.");
            yield return new WaitForSeconds(1f);

            state = BattleState.RunningTurn;
        }

        Destroy(baitObject);
    }

    bool TryToCatchXenogon(Xenogon xenogon, BaitItem baitItem)
    {
        float a = (3 * xenogon.MaxHP - 2 * xenogon.HP) * xenogon.Base.CatchRate * baitItem.CatchRateModifier * Conditions_DataBase.GetStatusBonus(xenogon.Status) / (3 * xenogon.MaxHP);

        //Debug.Log("Catch Rate: " + a);

        if (a >= 255)
            return true;

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int catchChance = 0;
        while (catchChance < 4)
        {
            if (UnityEngine.Random.Range(0, 65535) >= b)
                break;

            ++catchChance;
        }

        if (catchChance >= 4)
            return true;


        return false;
    }

    IEnumerator TryToEscape()
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't run from trainer battles!");
            state = BattleState.RunningTurn;
            yield break;
        }

        ++escapeAttempts;

        int playerSpeed = playerUnit.Xenogon.Dex;
        int enemySpeed = enemyUnit.Xenogon.Dex;

        if (enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog($"Ran away safely!");
            BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f % 256;

            if (UnityEngine.Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog($"Ran away safely!");
                BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog($"Couldn't escape!");
                state = BattleState.RunningTurn;
            }
        }
    }
}

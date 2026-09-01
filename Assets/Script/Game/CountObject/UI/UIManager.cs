using Assets.Script.Game.CountObject;
using Assets.Script.Game.CountObject.UI;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class UIManager : MonoBehaviour {

    [Header("Inject")]
    PlayerContract playerContract;

    [SerializeField] PlayerSelectionManager playerSelectionManager;
    //[SerializeField] CountInputUI countInputUI;
    [SerializeField] GameState gameState;

    [Inject]
    void Construct(PlayerContract playerContract ) {
        this.playerContract = playerContract;
    }


    void Start() {
        playerSelectionManager.gameObject.SetActive(true);
        //countInputUI.gameObject.SetActive(false);

        SetUpObserver();
    }

    void SetUpObserver() {
        playerSelectionManager.OnPlay += BeginGame;
    }


    void BeginGame(List<PlayerInfo> playerinfos) {
        playerSelectionManager.gameObject.SetActive(false);
        //countInputUI.gameObject.SetActive(false);

        playerContract.AddRange(playerinfos);

        gameState.StartGame();
    }



    



}

using Assets.Script.Game.CountObject.Effect;
using Assets.Script.Game.CountObject.UI;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting.YamlDotNet.Core;
using UnityEngine;
using VContainer;
using Cysharp.Threading.Tasks;

namespace Assets.Script.Game.CountObject {
    public class GameState : MonoBehaviour {

        enum State {
            ObjectMoving,
            WaitingUserInput,
            ShowResult
        }

        [Header("Inject")]
        PlayerContract playerContract;
        AddressableLoader addressableLoader;
        ObjectPooling objectPooling;
        GridScript gridScript;
        ClockUI clockUI;

        [SerializeField] Transform gridTransform;
        [SerializeField] GameObject inputUI;
        [SerializeField] GameObject panelInputUi;



        [SerializeField] TextMeshProUGUI questionText;

        int currentTurn = 1;
        int maxTurn = 5;

        [SerializeField] Dictionary<int, float> timeStayDictionary = new Dictionary<int, float>{ { 1, 5f}, {2,6f}, { 3, 5.5f } };
        [SerializeField] Dictionary<int, float> timeInAndOutDictionary = new Dictionary<int, float>{{1, 3f}, {2, 2.8f}, {3, 2.5f} };
        [SerializeField] Dictionary<int, int> timeReadingInputDictionary = new Dictionary<int, int>{{1, 10_000} , { 2, 9_500}, { 3, 9_000} }; //value is in ms
        [SerializeField] List<Vector3> definedPosition;

        Vector3 startPosition;
        Vector3 endPosition;

        [SerializeField] int TimeBetweenTurn;

        List<CountInputUI> list = new();
        IReadOnlyList<CountItemSO> items;

        List<CountItemSO> itemThisTurn = new(); //SO type of this , TODO: rename
        Dictionary<int, List<GameObject>> gameObjectThisTurn = new(); //list of gameobject use for relase to object pool, TODO: rename


        [Inject]
        void Construct( PlayerContract playerContract,
            AddressableLoader addressableLoader,
            ObjectPooling objectPolling,
            GridScript grid,
            ClockUI clockUI ) {
            this.playerContract = playerContract;
            this.addressableLoader = addressableLoader;
            this.objectPooling = objectPolling;
            this.gridScript = grid;
            this.clockUI = clockUI;
        }

        async Task Start() {
            items = await addressableLoader.LoadSOAsync();
        }

        public async Task StartGame() {

            while ( currentTurn < maxTurn ) {
                //StartCoroutine(WaitingForSecond(TimeBetweenTurn));

                Debug.Log($"--- START TURN {currentTurn} ---");

                await UniTask.Delay(TimeSpan.FromMilliseconds(TimeBetweenTurn));

                Debug.Log("done waiting");

                RandomNumberObject();

                Debug.Log("get random object");

                await MoveGrid();
                Debug.Log("move grid done");

                var item = GetRandomObjectThisTurn();

                await ShowQuestion(item.name);

                await ShowingResult(item.id);

                currentTurn++;
                Debug.Log($"current increase {currentTurn}");
            }

        }




        //async Task StartTurn() {

        //}



        //TODO: use wave to increase difficulty.
        void RandomNumberObject() {
            int n = UnityEngine.Random.Range(5, 25); //25 is max item grid can hold, 5 is min for now

            while ( n > 0 ) {
                CountItemSO countItemSO =  GetRandomItem();
                if ( !itemThisTurn.Contains(countItemSO) ) {
                    itemThisTurn.Add(countItemSO);
                    int amount = UnityEngine.Random.Range(1, n  + 1);
                    n -= amount;

                    var list = objectPooling.Get(countItemSO, amount);


                    gameObjectThisTurn.Add(countItemSO.id, list);
                    SpawnObject(amount, list, countItemSO);
                }
            }
        }

        CountItemSO GetRandomItem() {
            int index = UnityEngine.Random.Range(0, items.Count);
            return items[index];
        }

        void SpawnObject( int amount, List<GameObject> gameObjects, CountItemSO countItemSO ) {
            gridScript.AddRandomOjbectToGrid(amount, gameObjects, countItemSO);

        }



        //Move Grid
        async Task MoveGrid() {
            startPosition = GetRandomPosition();
            gridTransform.transform.position = startPosition;

            var tcs = new TaskCompletionSource<bool>();

            Sequence sequence = DOTween.Sequence();

            sequence.Append(
                gridTransform.DOMove(
                    Vector3.zero,
                    timeInAndOutDictionary[currentTurn]
                )
            );

            sequence.AppendInterval(timeStayDictionary[currentTurn]);

            sequence.Append(
                gridTransform.DOMove(
                    -startPosition,
                    timeInAndOutDictionary[currentTurn]
                )
            );

            sequence.OnComplete(() => tcs.SetResult(true));


            await tcs.Task;
        }

        //Choose random pos, dir for grid to move
        Vector3 GetRandomPosition() {
            int index = UnityEngine.Random.Range(0,definedPosition.Count);
            return definedPosition[index];
        }

        //Time for gird move should decrese as turn increase

        CountItemSO GetRandomObjectThisTurn() {
            int index = UnityEngine.Random.Range(0, itemThisTurn.Count);
            return itemThisTurn[index];
        }

        async Task ShowQuestion( string name ) {

            Debug.Log("Showing question right now");

            panelInputUi.gameObject.SetActive(true);

            questionText.text = $"How Many {name} is there";

            SetUpScreen();

            clockUI.StartTimer((float)timeReadingInputDictionary[currentTurn] / 1000); //convert ms to s

            await Task.Delay(timeReadingInputDictionary[currentTurn]);

            //Close screen reading input.
            panelInputUi.gameObject.SetActive(false);
            GetInputResult();
        }

        //read how many user to divide screen space for reading input
        void SetUpScreen() {
            int count = playerContract.GetPlayerCount();

            while ( count != 0 ) {
                count--;
                var temp = Instantiate(inputUI,panelInputUi.transform);
                list.Add(temp.GetComponent<CountInputUI>());
            }
        }

        //read user input for result
        public List<int> GetInputResult() {
            List<int> res = new();

            foreach ( var item in list ) {
                res.Add(item.count);
            }

            return res;
        }

        //show result will move grid to the screen, and apply effect for showing result

        async Task ShowingResult( int id ) {

            await MoveGridToCenter();

            await gridScript.ApplyEffect(id);

            await MoveGridOutCenter();
        }

        //async Task CorrectEffect( int id ) {


        //    Debug.Log("apply effect pls");

        //    var tasks = new List<Task>();


        //}

        Task MoveGridToCenter() {
            var tcs = new TaskCompletionSource<bool>();

            gridTransform.position = startPosition;
            gridTransform.DOMove(Vector3.zero, 3)
                         .OnComplete(() => {
                             Debug.Log("Move to center done");
                             tcs.SetResult(true);
                         });

            return tcs.Task;
        }

        Task MoveGridOutCenter() {
            var tcs = new TaskCompletionSource<bool>();

            gridTransform.DOMove(-startPosition, 3)
                         .OnComplete(() => {
                             Debug.Log("Move our center done");
                             tcs.SetResult(true);
                         });

            return tcs.Task;
        }

        IEnumerator WaitingForSecond( float second ) {
            yield return new WaitForSeconds(second);
        }

    }
}
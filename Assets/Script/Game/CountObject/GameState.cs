using Assets.Script.Game.CountObject.UI;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
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

        [Serializable]
        class TurnProperty {
            [SerializeField, Min(0)] public int turn;
            [SerializeField] public int minAnimalType;
            [SerializeField] public int maxAnimalType;
            [SerializeField] public int minAnimalPerType;
            [SerializeField] public int maxAnimalPerType;
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

        [SerializeField]
        Dictionary<int, float> timeStayDictionary = new Dictionary<int, float>{
                                                                            { 1, 5.0f },
                                                                            { 2, 4.5f },
                                                                            { 3, 4.0f },
                                                                            { 4, 3.5f },
                                                                            { 5, 3.0f }
                                                                            };
        [SerializeField]
        Dictionary<int, float> timeInAndOutDictionary = new Dictionary<int, float>{
                                                                                    { 1, 3.0f },
                                                                                    { 2, 2.7f },
                                                                                    { 3, 2.4f },
                                                                                    { 4, 2.1f },
                                                                                    { 5, 1.8f }
                                                                                    };
        [SerializeField]
        Dictionary<int, int> timeReadingInputDictionary = new Dictionary<int, int>{
                                                                                    { 1, 10_000 },
                                                                                    { 2, 9_000 },
                                                                                    { 3, 8_000 },
                                                                                    { 4, 7_000 },
                                                                                    { 5, 6_000 }
                                                                                    }; //value is in ms
        [SerializeField] List<TurnProperty> turnProperty;
        [SerializeField] List<Vector3> definedPosition;

        Vector3 startPosition;
        Vector3 endPosition;

        bool isSetUpScreenForInput;

        [SerializeField] int TimeBetweenTurn;

        List<CountInputUI> inputUiList = new();
        IReadOnlyList<AnimalSO> animalSO;

        List<AnimalSO> animalThisTurn = new(); //SO type of this , TODO: rename
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

        async void Start() {
            animalSO = await addressableLoader.LoadSOAsync();


        }


        //TODO: refactor this to synchronous code, async is no need
        public async Task StartGame() {

            try {
                while ( currentTurn < maxTurn ) {
                    Debug.Log($"--- START TURN {currentTurn} ---");

                    await UniTask.Delay(TimeSpan.FromMilliseconds(TimeBetweenTurn));
                    //StartCoroutine(WaitingForSecond(TimeBetweenTurn));

                    Debug.Log("done waiting");

                    RandomNumberObject();

                    Debug.Log("get random object");

                    await MoveGrid();
                    Debug.Log("move grid done");

                    var item = GetRandomObjectThisTurn();

                    await ShowQuestion(item.name);

                    await ShowingResult(item.id);

                    EndTurn();

                    currentTurn++;
                    Debug.Log($"current increase {currentTurn}");
                }
            }

            catch ( Exception e ) {
                Debug.LogException(e);
            }

        }






        //async Task StartTurn() {

        //}



        //TODO: use wave to increase difficulty.
        void RandomNumberObject() {
            var turn = turnProperty[currentTurn - 1];

            List<AnimalSO> availAnimalSO = new(animalSO);
            int n = 0;

            int animalType = UnityEngine.Random.Range(turn.minAnimalType, turn.maxAnimalType);

            while ( animalType > 0 ) {
                if ( n >= 25 ) break;

                AnimalSO animalSO =  GetRandomAnimal(availAnimalSO);

                if ( !animalThisTurn.Contains(animalSO) ) {
                    availAnimalSO.Remove(animalSO);
                    animalThisTurn.Add(animalSO);

                    int amount = UnityEngine.Random.Range(turn.minAnimalPerType, turn.maxAnimalPerType);
                    amount = Mathf.Min(amount, 25 - n);
                    n += amount;

                    var list = objectPooling.Get(animalSO, amount);
                    gameObjectThisTurn.Add(animalSO.id, list);
                    SpawnObject(amount, list, animalSO);
                }

                animalType--;
            }
        }

        AnimalSO GetRandomAnimal( List<AnimalSO> list ) {
            int index = UnityEngine.Random.Range(0, list.Count);
            return list[index];
        }

        void SpawnObject( int amount, List<GameObject> gameObjects, AnimalSO countItemSO ) {
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

        AnimalSO GetRandomObjectThisTurn() {
            int index = UnityEngine.Random.Range(0, animalThisTurn.Count);
            return animalThisTurn[index];
        }

        async Task ShowQuestion( string name ) {

            Debug.Log("Showing question right now");

            SetUpScreen();
            panelInputUi.gameObject.SetActive(true);
            questionText.text = $"How Many {name} is there";

            clockUI.StartTimer((float)timeReadingInputDictionary[currentTurn] / 1000); //convert ms to s
            await Task.Delay(timeReadingInputDictionary[currentTurn]);

            //Close screen reading input.
            panelInputUi.gameObject.SetActive(false);
            GetInputResult();
        }

        //read how many user to divide screen space for reading input
        void SetUpScreen() {
            if ( isSetUpScreenForInput ) return;
            isSetUpScreenForInput = true;

            int count = playerContract.GetPlayerCount();

            while ( count != 0 ) {
                count--;
                var temp = Instantiate(inputUI,panelInputUi.transform);
                inputUiList.Add(temp.GetComponent<CountInputUI>());
            }
        }

        //read user input for result
        public List<int> GetInputResult() {
            List<int> res = new();

            foreach ( var item in inputUiList ) {
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

        void EndTurn() {
            ReleaseGameObject();
            ClearlistThisTurn();
            ResetCounterUI();
            ResetGrid();
        }

        void ReleaseGameObject() {
            foreach ( var kv in gameObjectThisTurn ) {
                foreach ( var item in animalThisTurn ) {
                    if ( gameObjectThisTurn.TryGetValue(item.id, out var list) ) {
                        objectPooling.Release(list, item);
                    }
                }
            }
        }

        void ClearlistThisTurn() {
            animalThisTurn.Clear();
            gameObjectThisTurn.Clear();
        }

        void ResetCounterUI() {
            foreach ( var script in inputUiList ) {
                script.count = 0;
                script.UpdateText();
            }
        }

        void ResetGrid() {
            gridScript.ResetGrid();
        }

        IEnumerator WaitingForSecond( float second ) {
            yield return new WaitForSeconds(second);
        }

    }
}
using Assets.Script.Game.CountObject;
using Assets.Script.Game.CountObject.Effect;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class GridScript : MonoBehaviour {

    const int width = 5;
    const int height = 5;
    [SerializeField] float cellSize = 2;
    [SerializeField] Vector3 offSet;

    //[Header("Inject")]

    [SerializeField] TextMeshProUGUI textResultUI;

    Dictionary<Vector2Int, CellData> grid = new();

    List<Vector2Int> emptyCell = new();


    void Start() {
        InitializeGrid();
    }


    void InitializeGrid() {
        grid.Clear();
        emptyCell.Clear();

        for ( int i = 0; i < width; i++ ) {
            for ( int j = 0; j < height; j++ ) {
                Vector2Int pos = new Vector2Int(i,j);
                grid.Add(pos, null);
                emptyCell.Add(pos);
            }
        }
    }



    //void TestSpawn() {
    //    for ( int i = 0; i < width; i++ ) {
    //        for ( int j = 0; j < height; j++ ) {
    //            var pos = GridToWorld(new Vector2Int(i,j));
    //            Instantiate(prefab, pos, Quaternion.identity, this.gameObject.transform);
    //        }
    //    }
    //}


    public Vector3 GridToWorld( Vector2Int cell ) {
        return gameObject.transform.position - offSet + new Vector3(
            cell.x * cellSize,
            cell.y * cellSize,
            0f
        );
    }

    public Vector2Int WorldToGrid( Vector3 worldPosition ) {
        Vector3 local = worldPosition - gameObject.transform.position;

        int x = Mathf.FloorToInt(local.x / cellSize);
        int y = Mathf.FloorToInt(local.y / cellSize);

        return new Vector2Int(x, y);
    }



    int turn;



    //Vector2Int GetRandomPosition() {
    //    int x = UnityEngine.Random.Range(0,5);
    //    int y = UnityEngine.Random.Range(0,5);
    //    Vector2Int res = new Vector2Int(x, y);

    //    if ( grid.ContainsKey(res) ) GetRandomPosition();

    //    return res;
    //}


    //BUG: some time there are some animal in the same cell
    public void AddRandomOjbectToGrid( int amount, List<GameObject> gameObjects, AnimalSO countItemSO ) {
        for ( int i = 0; i < amount; i++ ) {
            int randomIndex = UnityEngine.Random.Range(0, emptyCell.Count - i);

            Vector2Int selected = emptyCell[randomIndex];
            SnapObjectToGrid(selected, gameObjects[i], countItemSO);

            int lastIndex = emptyCell.Count - i - 1;

            (emptyCell[randomIndex], emptyCell[lastIndex]) = (emptyCell[lastIndex], emptyCell[randomIndex]);

        }

        emptyCell.RemoveRange(emptyCell.Count - amount, amount);
    }

    public void SnapObjectToGrid( Vector2Int pos, GameObject gameObject, AnimalSO countItemSO ) {
        gameObject.transform.position = GridToWorld(pos);

        grid[pos] = new CellData() {
            isOccupied = true,
            id = countItemSO.id,
            countItemSO = countItemSO,
            correctEffect = gameObject.GetComponent<CorrectEffect>()
        };
    }


    public async Task ApplyEffect( int id ) {
        textResultUI.gameObject.SetActive(true);
        textResultUI.text = "0";

        var tasks = new List<Task>();


        for ( int i = 4; i >= 0; i-- ) {
            for ( int j = 4; j >= 0; j-- ) {
                Vector2Int pos = new Vector2Int(j,i);

                if ( grid.TryGetValue(pos, out CellData data) ) {
                    if ( data == null ) continue;
                    if ( id != data.id ) continue;

                    tasks.Add(data.correctEffect.PlayCorrectEffect());
                    textResultUI.text = tasks.Count.ToString();
                    await Task.Delay(150);

                }

            }
        }

        await Task.Delay(500);

        textResultUI.gameObject.SetActive(false);
        Debug.Log("done apply effect");

        await Task.WhenAll(tasks);
    }

    public void ResetGrid() {
        InitializeGrid();
    }


}

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Script.Game.Pool {
    /// <summary>Displays the read-only groups in PoolGameState and hides pocketed ball icons.</summary>
    public sealed class BallGroupUIManager : MonoBehaviour {
        [SerializeField] PoolGameState gameState;
        [SerializeField] GameObject hudRoot;
        [SerializeField] Transform player1Row;
        [SerializeField] Transform player2Row;
        [SerializeField] TextMeshProUGUI player1Title;
        [SerializeField] TextMeshProUGUI player2Title;
        [SerializeField] GameObject player1EightBallIcon;
        [SerializeField] GameObject player2EightBallIcon;

        readonly Dictionary<int, GameObject> ballIcons = new();
        readonly HashSet<int> pocketedBallNumbers = new();

        void Awake() {
            gameState ??= FindFirstObjectByType<PoolGameState>();
            hudRoot ??= GameObject.Find("Ball Group HUD Prototype");
            if ( hudRoot != null ) {
                player1Row ??= hudRoot.transform.Find("Player 1 Solids");
                player2Row ??= hudRoot.transform.Find("Player 2 Stripes");
            }

            player1Title ??= player1Row != null ? player1Row.Find("Title")?.GetComponent<TextMeshProUGUI>() : null;
            player2Title ??= player2Row != null ? player2Row.Find("Title")?.GetComponent<TextMeshProUGUI>() : null;
            player1EightBallIcon ??= CreateEightBallIcon(player1Row, "Ball 8 P1");
            player2EightBallIcon ??= CreateEightBallIcon(player2Row, "Ball 8 P2");
            CacheBallIcons(player1Row);
            CacheBallIcons(player2Row);

            if ( hudRoot != null )
                hudRoot.SetActive(gameState != null && gameState.GroupsDecided);
        }

        void OnEnable() {
            if ( gameState != null ) {
                gameState.BallGroupsDecided += ShowGroups;
                gameState.PlayerBallGroupChanged += ChangePlayerGroup;
            }

            PoolPocket.BallPocketed += RemovePocketedBall;
        }

        void OnDisable() {
            if ( gameState != null ) {
                gameState.BallGroupsDecided -= ShowGroups;
                gameState.PlayerBallGroupChanged -= ChangePlayerGroup;
            }

            PoolPocket.BallPocketed -= RemovePocketedBall;
        }

        void ShowGroups( BallType player1Group, BallType player2Group ) {
            if ( hudRoot != null )
                hudRoot.SetActive(true);
            Refresh(player1Group, player2Group);
        }

        void ChangePlayerGroup( Turn player, BallType ballType ) {
            if ( gameState != null && gameState.GroupsDecided )
                Refresh(gameState.Player1BallType, gameState.Player2BallType);
        }

        void RemovePocketedBall( PoolBall ball ) {
            if ( ball == null )
                return;

            pocketedBallNumbers.Add(ball.Number);
            if ( ballIcons.TryGetValue(ball.Number, out GameObject icon) )
                icon.SetActive(false);
        }

        void Refresh( BallType player1Group, BallType player2Group ) {
            if ( hudRoot == null || !hudRoot.activeSelf )
                return;

            foreach ( GameObject icon in ballIcons.Values )
                icon.SetActive(false);
            if ( player1EightBallIcon != null ) player1EightBallIcon.SetActive(false);
            if ( player2EightBallIcon != null ) player2EightBallIcon.SetActive(false);

            SetRow(player1Row, player1Title, player1Group, player1EightBallIcon, "PLAYER 1");
            SetRow(player2Row, player2Title, player2Group, player2EightBallIcon, "PLAYER 2");
        }

        void SetRow( Transform row, TextMeshProUGUI title, BallType group, GameObject eightBallIcon, string playerName ) {
            if ( title != null )
                title.text = playerName + " - " + GroupLabel(group);

            if ( group == BallType.Ball8 ) {
                if ( eightBallIcon != null )
                    eightBallIcon.SetActive(!pocketedBallNumbers.Contains(8));
                return;
            }

            int firstNumber = group == BallType.Solid ? 1 : 9;
            for ( int number = firstNumber; number < firstNumber + 7; number++ ) {
                if ( !ballIcons.TryGetValue(number, out GameObject icon) )
                    continue;

                icon.transform.SetParent(row, false);
                icon.SetActive(!pocketedBallNumbers.Contains(number));
            }
        }

        void CacheBallIcons( Transform row ) {
            if ( row == null )
                return;

            foreach ( Transform child in row ) {
                if ( child.name.StartsWith("Ball ") && int.TryParse(child.name.Substring(5), out int number) )
                    ballIcons[number] = child.gameObject;
            }
        }

        static GameObject CreateEightBallIcon( Transform row, string iconName ) {
            if ( row == null )
                return null;

            Transform existing = row.Find(iconName);
            if ( existing != null )
                return existing.gameObject;

            Transform template = row.Find("Ball 1") ?? row.Find("Ball 9");
            if ( template == null )
                return null;

            GameObject icon = Instantiate(template.gameObject, row);
            icon.name = iconName;
            Image image = icon.GetComponent<Image>();
            if ( image != null ) image.color = new Color(0.04f, 0.04f, 0.04f, 1f);
            TextMeshProUGUI label = icon.GetComponentInChildren<TextMeshProUGUI>();
            if ( label != null ) {
                label.text = "8";
                label.color = Color.white;
            }

            icon.SetActive(false);
            return icon;
        }

        static string GroupLabel( BallType group ) {
            return group == BallType.Solid ? "SOLIDS"
                : group == BallType.Strip ? "STRIPES"
                : "8 BALL";
        }
    }
}

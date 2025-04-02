using System.Collections;
using UnityEngine;

namespace Assets
{
    public enum EGameMode
    {
        NONE = 0,
        SINGLE = 1,
        COMPETITIVE = 2,
        COOPERATIVE = 3,
    }

    public class GameSettings : MonoBehaviour
    {

        public static EGameMode gameMode = EGameMode.NONE;
        public static int characterId = -1;
        public static int locationId = -1;
        public static string playername = "";
        public static int gameTime;
        public static int collectableCount = 5;
        public static string lobbyId = "";
    }
}
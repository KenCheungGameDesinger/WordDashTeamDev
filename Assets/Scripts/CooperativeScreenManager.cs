using Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
public class CooperativeScreenManager : MonoBehaviour
{
    static public CooperativeScreenManager Singleton { get; internal set; }

    public Transform playersTransform;
    public GridLayoutGroup teamsGrid;
    public List<TeamManager> teams = new List<TeamManager>();
    public List<PlayerManager> players = new List<PlayerManager>();
    public Dictionary<ulong, PlayerManager> playerList = new Dictionary<ulong, PlayerManager>();
    public GameObject teamPrefab;
    public GameObject playerPrefab;
    public TMP_Dropdown teamCountDropdown;

    void Awake()
    {
        Singleton = this;
    }

    private void OnDestroy()
    {
        Singleton = null;
    }


    // Use this for initialization
    void Start()
    {
    }

    private void OnEnable()
    {
        ResetUI();
        Init(2);
        if(NetworkManager.Singleton.IsClient)
        {
            StartCoroutine("UpdateClientLobby");
        }
    }
    
    void ResetUI()
    {
        foreach(var item in playerList)
        {
            GameObject.Destroy(item.Value.gameObject);
        }
        playerList.Clear();

        teamCountDropdown.value = 0;
    }
    
    IEnumerator UpdateClientLobby()
    {
        while(true)
        {
            ResetUI();
            int max = -1;
            List<int> tmpList = new List<int>();
            for(int i = 0; i < MyNetwork.Singleton.playerTeams.Count; i++)
            {
                if(!tmpList.Contains(MyNetwork.Singleton.playerTeams[i]))
                {
                    tmpList.Add(MyNetwork.Singleton.playerTeams[i]);
                }
                if(max < MyNetwork.Singleton.playerTeams[i])
                    max = MyNetwork.Singleton.playerTeams[i];
            }
            Init(max);
            teamCountDropdown.captionText.text = max.ToString();
            for(int i = 0; i < MyNetwork.Singleton.playerNames.Count; i++)
            {
                var playerManager = GameObject.Instantiate(playerPrefab, teams[MyNetwork.Singleton.playerTeams[i] - 1].playersGrid.transform).GetComponent<PlayerManager>();
                playerManager.SetName(MyNetwork.Singleton.playerNames[i].ToString());
            }
            yield return new WaitForSeconds(3f);
        }
    }

    public void AddPlayer(ulong clientId, PlayerData playerData)
    {
        var playerManager = GameObject.Instantiate(playerPrefab, playersTransform).GetComponent<PlayerManager>();
        playerManager.SetName(playerData.playerName);
        playerManager.playerData = playerData;
        playerList.Add(clientId, playerManager);

        PlaceAuto(playerManager);
    }

    public void RemovePlayer(ulong clientId)
    {
        GameObject.Destroy(playerList[clientId].gameObject);
        playerList.Remove(clientId);
    }


    public void Init(int teamCount)
    {
        foreach (var item in playerList)
        {
            item.Value.transform.SetParent(playersTransform);
        }

        Common.DestroyChildren(teamsGrid);
        teams.Clear();

        teamsGrid.spacing = new Vector2(teamCount==2? 100: (teamCount==3? 70: 20), 0);
        
        for (int i = 1; i <= teamCount; i++)
        {
            var teamManager = GameObject.Instantiate(teamPrefab, teamsGrid.transform).GetComponent<TeamManager>();
            teamManager.gameObject.SetActive(true);
            teamManager.Init(i);
            teams.Add(teamManager);
        }

        foreach (var item in playerList)
        {
            PlaceAuto(item.Value);
        }
    }

    public void PlaceAuto(PlayerManager playerManager)
    {
        TeamManager selItem = null;
        foreach (var item in teams)
        {
            if (!selItem || selItem.playersGrid.transform.childCount > item.playersGrid.transform.childCount)
                selItem = item;
        }
        if(selItem)
        {
            playerManager.transform.SetParent(selItem.playersGrid.transform);
            playerManager.playerData.teamID = selItem.teamNumber;
            MyNetwork.Singleton.ChangePlayerTeam(playerManager.playerData.playerName, selItem.teamNumber);
        }
    }

    public void OnChangeTeamCount(int index)
    {
        Init(index + 2);
    }

}

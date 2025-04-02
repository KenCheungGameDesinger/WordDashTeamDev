using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Assets;

public class TeamManager : MonoBehaviour
{
    public VerticalLayoutGroup playersGrid;
    public int teamNumber;
    public TextMeshProUGUI titleLabel;

    // Use this for initialization
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Init(int pTeamNumber)
    {
        teamNumber = pTeamNumber;
        switch (teamNumber) {
            case 1:
                titleLabel.SetText("Team One");
                titleLabel.color = new Color32(255,25,65,255);
                break;
            case 2:
                titleLabel.SetText("Team Two");
                titleLabel.color = new Color32(40, 70, 16, 255);
                break;
            case 3:
                titleLabel.SetText("Team Three");
                titleLabel.color = new Color32(99, 19, 77, 255);
                break;
            case 4:
                titleLabel.SetText("Team Four");
                titleLabel.color = new Color32(0, 74, 154, 255);
                break;
        }
    }

    public void AddPlayer(PlayerManager player)
    {
        player.GetComponent<Drag>().parentTransform = playersGrid.transform;
        player.transform.SetParent(playersGrid.transform);
        player.transform.localScale = Vector3.one;
        player.teamManager = this;
        player.playerData.teamID = teamNumber;
        MyNetwork.Singleton.ChangePlayerTeam(player.playerData.playerName, player.playerData.teamID);
    }
}

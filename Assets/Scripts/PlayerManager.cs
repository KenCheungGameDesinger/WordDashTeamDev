using System.Collections;
using TMPro;
using UnityEngine;
using Assets;

public class PlayerManager : MonoBehaviour
{
    public TextMeshProUGUI nameLabel;
    public TeamManager teamManager;
    public PlayerData playerData;

    public void SetName(string pName)
    {
        nameLabel.SetText(pName);
    }

}

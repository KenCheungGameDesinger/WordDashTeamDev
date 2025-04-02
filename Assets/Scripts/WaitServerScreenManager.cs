using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Assets
{
    public class WaitServerScreenManager : MonoBehaviour
    {
        public Transform loadingCursorTransform;
        public TextMeshProUGUI loadingLabel;
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            var oldZ = loadingCursorTransform.localEulerAngles.z;
            loadingCursorTransform.localEulerAngles = new Vector3(0, 0, oldZ - 200f * Time.deltaTime);
            //int cnt = (int)(Time.realtimeSinceStartup * 6) % 12;
            //loadingCursorTransform.localEulerAngles = new Vector3(0, 0, -30f * cnt);

            int cnt = (int)(Time.realtimeSinceStartup*2) % 4;
            var str = NetworkManager.Singleton.IsConnectedClient ? "Waiting Host" : "Connecting";
            for (int i = 1; i <= cnt; i++)
                str += ".";
            loadingLabel.text = str;
        }
    }
}
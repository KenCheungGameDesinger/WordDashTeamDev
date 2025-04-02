using System.Collections;
using UnityEngine;

namespace Assets
{
    public class HomeManager : MonoBehaviour
    {
        public int homeId = 0;

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
                other.GetComponent<PlayerController>()?.OnHomeEnter(this);
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.tag == "Player")
                other.GetComponent<PlayerController>()?.OnHomeExit();
        }
    }
}
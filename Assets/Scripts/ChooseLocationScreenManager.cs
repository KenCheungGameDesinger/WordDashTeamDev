using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    public class ChooseLocationScreenManager : MonoBehaviour
    {
        static public ChooseLocationScreenManager instance = null;
        public GameObject characterItemPrefab;
        public List<LocationItem> items = new List<LocationItem>();
        public Transform locationsTransform;

        private LocationItem selectedItem;
        

        // Use this for initialization
        void Start()
        {
            instance = this;

            var item = locationsTransform.GetComponentInChildren<LocationItem>();
            if (item)
            {
                OnSelectItem(item);
            }
        }

        private void OnDestroy()
        {
            instance = null;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnClickReadyButton()
        { 
        
        }

        public void OnSelectItem(LocationItem item)
        {
            if (selectedItem)
                selectedItem.Deselect();
            item.Select();
            selectedItem = item;
            GameSettings.locationId = item.id;
        }


    }
}
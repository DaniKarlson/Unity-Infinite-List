using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.InfiniteList {
    public class ListItemExample : InfiniteListItem {
        
            [SerializeField] private TextMeshProUGUI t_text;

            /// <summary>
            /// Just override the Set method and do whatever you want with your object
            /// </summary>
            public override void Set(object item) {
                MyItem myItem = (MyItem) item;
                t_text.text = myItem.value.ToString();
            }
    }
}
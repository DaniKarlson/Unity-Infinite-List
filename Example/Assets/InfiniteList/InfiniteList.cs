using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfiniteList : MonoBehaviour {
    
    //Inspector
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GameObject prefab;
    [SerializeField] private RectTransform mask, content;

    //Constants
    private float spacing = 15f, heightPerItem, maskHeight;
    private Vector2 prefabSize, contentStartSize;

    //Other
    private InfiniteListItem[] listItems;
    private Dictionary<int, object> items; //Hold all items we add to the infinite list
    private int listItemsIndex, numItems, topIndexCurrent, numMaxItems;

    void Start() {
        StartCoroutine(Init());
    }

    /// <summary>
    /// Used to empty / clear the list
    /// </summary>
    public void Reset() {
        items = new Dictionary<int, object>();
        numItems = 0;
        topIndexCurrent = 0;
        scrollRect.verticalNormalizedPosition = 1f;
        scrollbar.value = 1f;
        content.sizeDelta = contentStartSize;
        listItemsIndex = 0;

        //Reset all prefabs in list (if they exist)
        if (listItems != null) {
            foreach (var item in listItems) { item.gameObject.SetActive(false); }
        }
    }

    /// <summary>
    /// Initialize the Infinite List. Should only be called once.
    /// The reason it's a coroutine, is because we can't get the size of a stretched rect transform unless we wait one frame.
    /// Might be a way to get around this? But seems like a Unity limitation to me but idfk
    /// </summary>
    IEnumerator Init() {
        yield return null; //Wait one frame, otherwise we can't get the size of a scaling / fitting rect transform
        
        //Reset values
        Reset();

        //Find sizes
        prefabSize = prefab.GetComponent<RectTransform>().sizeDelta;
        heightPerItem = prefabSize.y + spacing;
        maskHeight = mask.rect.height;
        content.sizeDelta = new Vector2(0f, -heightPerItem);
        
        numMaxItems = (int) Mathf.Ceil(maskHeight / heightPerItem) + 1;
        listItems = new InfiniteListItem[numMaxItems];

        //Allocate list items
        for (int i = 0; i < numMaxItems; i++) {
            AddPrefab(i);
        }
        contentStartSize = content.sizeDelta;
        
        //Hide prefab
        prefab.SetActive(false);

        foreach (var o in queuedAddItems) {
            AddItem(o);
        }
    }

    void AddPrefab(int index) {
        InfiniteListItem i = Instantiate(prefab, content).GetComponent<InfiniteListItem>();
        listItems[index] = i;
        i.gameObject.SetActive(false);
        i.rect.anchoredPosition = new Vector2(i.rect.anchoredPosition.x, index * -heightPerItem);
    }

    private List<object> queuedAddItems = new List<object>();
    /// <summary>
    /// Add a new item to the list
    /// </summary>
    /// <param name="o"></param>
    public void AddItem(object o) {
        //If we add items before we are initialized, we just queue them up and add them a frame later once we initialize
        if (items == null) { queuedAddItems.Add(o); return; }
        //Add height to content container size, and refresh
        content.sizeDelta = new Vector2(content.sizeDelta.x, content.sizeDelta.y + heightPerItem);
        
        items.Add(numItems++, o);
        if (numItems > numMaxItems) return;
        SetItem((numItems - 1) % numMaxItems, (numItems - 1) % numMaxItems);
    }

    void RemoveItem() {
        throw new NotImplementedException("Removing item from infinite list not implemented...");
    }

    /// <summary>
    /// Must be called whenever we move the infinite list (scrolling or using the scroll-bar).
    /// Basically just checks if any new items are visible, and if so, display them.
    /// </summary>
    public void RefreshScroll() {
        //1. Find new top item index
        if (content.anchoredPosition.y < 0) content.anchoredPosition = new Vector2(content.anchoredPosition.x, 0f);
        int topIndex = Mathf.FloorToInt(content.anchoredPosition.y / heightPerItem);
        if (topIndex < 0 || topIndex == topIndexCurrent) return;
        
        //2. If same as old, return, otherwise find how many new items to display
        int delta = topIndex - topIndexCurrent;
        topIndexCurrent = topIndex;

        //3. Move through the listItems, and set their new anchored positions
        for (int i = 0; i < Mathf.Abs(delta); i++) {
            int itemIndex;
            int prefabIndex;
            
            //Find indexes
            if (Mathf.Sign(delta) > 0) { prefabIndex = listItemsIndex++ % numMaxItems; itemIndex = ((Mathf.RoundToInt(Mathf.Sign(delta) * i)) + numMaxItems - (Mathf.Abs(delta)) + topIndex); }
            else { prefabIndex = (listItemsIndex-- - 1) % numMaxItems; itemIndex = ((Mathf.RoundToInt(Mathf.Sign(delta) * i)) + topIndex) + (Mathf.Abs(delta) - 1); }

            //Actually set items at indexes
            SetItem(prefabIndex, itemIndex);
        }
    }

    void SetItem(int prefabIndex, int itemIndex) {
        float height = -(itemIndex * heightPerItem);
        listItems[prefabIndex].rect.anchoredPosition = new Vector2(listItems[prefabIndex].rect.anchoredPosition.x, height);
        listItems[prefabIndex].gameObject.SetActive(true);
        listItems[prefabIndex].Set(items[itemIndex]);
    }

}

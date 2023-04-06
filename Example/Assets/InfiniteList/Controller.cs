using UnityEngine;

public class Controller : MonoBehaviour {

    public InfiniteList list;

    private int index = 0;
    void Start() {
        for (index = 0; index < 50; index++) {
            list.AddItem(new MyItem(index));
        }
    }

    public void Add() {
        list.AddItem(new MyItem(++index));
    }

    public void Reset() {
        list.Reset();
    }

}

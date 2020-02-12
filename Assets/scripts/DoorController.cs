using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public AudioSource AudioSource;
    public AudioClip DoorSfx;

    int _index;    
    GameObject _highlight;
    GameController _gameController;
    FadeController _fadeController;

    public void SetIndex(int index) { _index = index; }

    void Start()
    {
        _highlight = transform.Find("Highlight").gameObject;
        _gameController = GameObject.Find("GameController").GetComponent<GameController>();
        
        _fadeController = transform.parent.parent.Find("black_screen").GetComponent<FadeController>();

        StartCoroutine(_fadeController.FadeIn(0.35f));
    }

    void OnMouseEnter()
    {
        _highlight.SetActive(true);
    }
    void OnMouseExit()
    {
        _highlight.SetActive(false);
    }
    void OnMouseDown()
    {
        StartCoroutine(ChangeDoor());
    }

    IEnumerator ChangeDoor()
    {
        AudioSource.PlayOneShot(DoorSfx);
        yield return StartCoroutine(_fadeController.FadeOut(0.35f));
        _gameController.EnterDoor(_index);
    }
}

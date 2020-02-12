using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Button_Clickable : Clickable
{
    public GameObject Highlight;
    public AudioClip Click_Sound;

    AudioSource _audioSource;

    private void Start()
    {
        _audioSource = GameObject.Find("AudioSource").GetComponent<AudioSource>();
    }

    void OnMouseEnter()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        Highlight.SetActive(true);
    }

    new void OnMouseDown()
    {
        _audioSource.PlayOneShot(Click_Sound);
        base.OnMouseDown();
    }

    void OnMouseExit()
    {
        GetComponent<SpriteRenderer>().enabled = true;
        Highlight.SetActive(false);
    }
}

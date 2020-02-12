using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Events;

public class GameController : MonoBehaviour
{
    public Text Title;
    public Text Description;
    //public List<Button> Doors;
    public List<GameObject> RoomPrefabs;
    public GameObject PlayAgain;

    [Header("Audio")]
    public AudioSource AudioSource;
    public AudioClip Door;

    List<Room> _rooms;
    Room activeRoom;
    //FadeController _fadeController;

    void Start()
    {
        //_fadeController = GetComponent<FadeController>();

        GenerateLab();
        SetRoom(_rooms.FirstOrDefault());
    }

    public void EnterDoor(int doorIndex)
    {
        var room = activeRoom.Doors.FirstOrDefault(_ => _.UIButton == doorIndex).Room;
        SetRoom(room);
    }

    public void PlayAgainAction()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("1_Menu");
    }

    void SetRoom(Room room)
    {
        var livingRooms = GameObject.FindGameObjectsWithTag("Room").ToList();
        livingRooms.ForEach(_ => Destroy(_));

        Title.text = room.Title;
        Description.text = room.Description;

        //var names = _rooms.Select(_ => _.Title).ToList();
        //var roomPrefab = RoomPrefabs[_rooms.IndexOf(room)];
        var roomPrefab = RoomPrefabs.FirstOrDefault(_ => _.name == room.Name);
        var createdRoom = Instantiate(roomPrefab);

        if (room.Type != RoomType.Exit)
        {
            var doors = new List<GameObject>();
            foreach (Transform door in createdRoom.transform.Find("Door").transform)
            {
                var doorController = door.gameObject.AddComponent<DoorController>();
                doorController.AudioSource = AudioSource;
                doorController.DoorSfx = Door;
                doorController.SetIndex(doors.Count);

                doors.Add(door.gameObject);
            }
            doors.ForEach(_ => _.gameObject.SetActive(false));

            room.Doors.ForEach(_ =>
            {
                doors[_.UIButton].gameObject.SetActive(true);
                //Doors[_.UIButton].transform.Find("Text").GetComponent<Text>().text = _.DoorType;
                //Doors[_.UIButton].transform.GetComponent<Image>().sprite = _.Sprite;
            });
        }
        else
        {
            var fadeController = GameObject.Find("Last_Room(Clone)/black_screen").GetComponent<FadeController>();
            StartCoroutine(fadeController.FadeIn(0.35f));
            StartCoroutine(ActivatePlayAgainButton());
        }

        activeRoom = room;
    }
    void GenerateLab()
    {
        GetComponent<LabGenerator>().RoomInitializer();
        _rooms = GetComponent<LabGenerator>().Rooms;
    }
    IEnumerator ActivatePlayAgainButton()
    {
        yield return new WaitForSeconds(2);

        var playAgain = GameObject.Find("Last_Room(Clone)").transform.GetChild(5).gameObject;
        var credits = GameObject.Find("Last_Room(Clone)").transform.GetChild(6).gameObject;
        var playAgainButton = playAgain.transform.GetChild(0);
        var creditsButton = credits.transform.GetChild(0);

        playAgain.SetActive(true);
        playAgainButton.GetComponent<Button_Clickable>().Click_Event.AddListener(PlayAgainAction);

        credits.SetActive(true);
        creditsButton.GetComponent<Button_Clickable>().Click_Event.AddListener(ShowCredits);
    }

    public void ShowCredits()
    {
        StartCoroutine(Credits());
    }
    IEnumerator Credits()
    {
        var playAgain_ = GameObject.Find("Last_Room(Clone)").transform.GetChild(5).gameObject;
        var credits_ = GameObject.Find("Last_Room(Clone)").transform.GetChild(6).gameObject;

        var canvas = GameObject.Find("Canvas").transform;
        var credits = canvas.Find("Credits");
        var blackScreen = GameObject.FindGameObjectWithTag("Room").transform.Find("black_screen");
        var fadeController = blackScreen.GetComponent<FadeController>();

        var collider = blackScreen.gameObject.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(1, 1);
        collider.offset = new Vector2(0, 0);

        var bsClickable = blackScreen.gameObject.AddComponent<Clickable>();
        var unityEvent = new UnityEvent();
        unityEvent.AddListener(HideCredits);
        bsClickable.Click_Event = unityEvent;

        yield return StartCoroutine(fadeController.FadeOut(0.5f, 0.75f));

        credits.gameObject.SetActive(true);
        Title.gameObject.SetActive(false);
        Description.gameObject.SetActive(false);
        playAgain_.gameObject.SetActive(false);
        credits_.gameObject.SetActive(false);
    }

    public void HideCredits()
    {
        StartCoroutine(Credits_());
    }
    IEnumerator Credits_()
    {
        var playAgain_ = GameObject.Find("Last_Room(Clone)").transform.GetChild(5).gameObject;
        var credits_ = GameObject.Find("Last_Room(Clone)").transform.GetChild(6).gameObject;

        var canvas = GameObject.Find("Canvas").transform;
        var credits = canvas.Find("Credits");
        var blackScreen = GameObject.FindGameObjectWithTag("Room").transform.Find("black_screen");
        var fadeController = blackScreen.GetComponent<FadeController>();

        var clickable = blackScreen.GetComponent<Clickable>();
        var collider = blackScreen.GetComponent<BoxCollider2D>();
        Destroy(clickable);
        Destroy(collider);
                
        yield return StartCoroutine(fadeController.FadeIn(0.5f, 0.75f));

        credits.gameObject.SetActive(false);
        Title.gameObject.SetActive(true);
        Description.gameObject.SetActive(true);
        playAgain_.gameObject.SetActive(true);
        credits_.gameObject.SetActive(true);
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LabGenerator : MonoBehaviour
{
    public List<Room> Rooms { get { return _rooms; } }
    private List<Room> _rooms;

    public List<Sprite> DoorMaterial;

    Room _entrance { get { return _rooms.FirstOrDefault(_ => _.Type == RoomType.Entrance); } }
    List<Room> _firstFloor { get { return _rooms.Where(_ => _.Type == RoomType.FirstFloor).ToList(); } }
    List<Room> _secondFloor { get { return _rooms.Where(_ => _.Type == RoomType.SecondFloor).ToList(); } }
    List<Room> _thirdFloor { get { return _rooms.Where(_ => _.Type == RoomType.ThirdFloor).ToList(); } }
    Room _exit { get { return _rooms.FirstOrDefault(_ => _.Type == RoomType.Exit); } }

    Room _entrance_main { get { return _entrance; } }
    Room _firstFloor_main { get { return _firstFloor.FirstOrDefault(_ => _.IsMainRoom); } }
    Room _secondFloor_main { get { return _secondFloor.FirstOrDefault(_ => _.IsMainRoom); } }
    Room _thirdFloor_main { get { return _thirdFloor.FirstOrDefault(_ => _.IsMainRoom); } }
    Room _exit_main { get { return _exit; } }

    public LabGenerator() { _rooms = new List<Room>(); }

    public void RoomInitializer()
    {
        CreateFloors();
        GenerateDoors();
        GetTextAndTitles();
        CreateBeginAndEnd();

        var path = exportDoorSchema();
    }

    #region CreateFloors
    void CreateFloors()
    {
        var ent_amount = 1;
        var ff_amount = 3;
        var sf_amount = 4;
        var tf_amount = 5;
        var ex_amount = 1;

        InsertFloorRooms(ent_amount, RoomType.Entrance);
        InsertFloorRooms(ff_amount, RoomType.FirstFloor);
        InsertFloorRooms(sf_amount, RoomType.SecondFloor);
        InsertFloorRooms(tf_amount, RoomType.ThirdFloor);
        InsertFloorRooms(ex_amount, RoomType.Exit);
    }
    void InsertFloorRooms(int roomsAmount, RoomType roomType)
    {
        for (var i = 0; i < roomsAmount; i++)
        {
            var roomIndex = _rooms.Count;
            _rooms.Add(new Room(roomIndex, roomType));
        }
    }
    #endregion

    #region CreateDoors
    void GenerateDoors()
    {
        PrimaryDoors();
        Passages();
        SecondaryDoors();
    }

    #region Primary
    void PrimaryDoors()
    {
        //entrance > firstFloor
        var ff_targetRoom = GetRandomDoor(_firstFloor);
        CreateDoor(_entrance, ff_targetRoom);
        _entrance.IsMainRoom = true;
        ff_targetRoom.IsMainRoom = true;

        //firstFloor > secondFloor
        var ff_originRoom = GetRandomDoor(_firstFloor);
        var sf_targetRoom = GetRandomDoor(_secondFloor);
        CreateDoor(ff_originRoom, sf_targetRoom);
        sf_targetRoom.IsMainRoom = true;

        //secondFloor > thirdFloor
        var sf_originRoom = GetRandomDoor(_secondFloor);
        var tf_targetRoom = GetRandomDoor(_thirdFloor);
        CreateDoor(sf_originRoom, tf_targetRoom);
        tf_targetRoom.IsMainRoom = true;

        //thirFloor > exit
        var tf_originRoom = GetRandomDoor(_thirdFloor);
        CreateDoor(tf_originRoom, _exit);
        _exit.IsMainRoom = true;
    }
    #endregion

    #region Passages
    void Passages()
    {
        SetPassage(_firstFloor);
        SetPassage(_secondFloor);
        SetPassage(_thirdFloor);
    }
    void SetPassage(List<Room> floor)
    {
        var origin = floor.FirstOrDefault(_ => _.IsMainRoom);

        var targetType = (RoomType)(System.Convert.ToInt32(origin.Type) + 1);
        var target = floor.FirstOrDefault(room =>
            room.Doors.FirstOrDefault(door =>
                door.Room.Type == targetType
            ) != null
        );

        if (origin != target) CreateDoor(origin, target);
    }
    #endregion

    #region SecondaryDoors
    void SecondaryDoors()
    {
        SetEntrancesAndExits(_firstFloor);
        SetEntrancesAndExits(_secondFloor);
        SetEntrancesAndExits(_thirdFloor);

        SetExceedDoors(6, _firstFloor);
        SetExceedDoors(9, _secondFloor);
        SetExceedDoors(12, _thirdFloor);
    }
    void SetEntrancesAndExits(List<Room> floor)
    {
        foreach (var room in floor)
        {
            var otherRooms = floor.Where(_ => _ != room).ToList();

            //Verify if door exists
            if (room.Doors.Count == 0)
            {
                var targetRoom = GetRandomDoor(otherRooms);
                CreateDoor(room, targetRoom);
            }

            //Verify if room has entrance. Another room has door for this room?
            if (_firstFloor.FirstOrDefault(_ => _.Doors.FirstOrDefault(x => _ == x.Room) != null) == null)
            {
                var originRoom = GetRandomDoor(otherRooms);
                CreateDoor(originRoom, room);
            }
        }
    }
    void SetExceedDoors(int roomDoorsDesiredAmount, List<Room> floor)
    {
        var roomDoorsAmount = floor.Sum(_ => _.Doors.Count);

        for (int i = 0; i < roomDoorsDesiredAmount - roomDoorsAmount; i++)
        {
            Room origin = null;
            Room target = origin;

            while (origin == target)
            {
                origin = GetRandomDoor(floor);
                target = GetRandomDoor(floor);
            }

            //Script for repair duplicate exceed doors --- already applied
            //if (origin.Doors.FirstOrDefault(_ => _.Room == target) != null) { i--; continue; }

            if (origin.Doors.FirstOrDefault(_ => _.Room == target) != null || !CreateDoor(origin, target)) i--;
        }
    }
    #endregion

    #region auxiliar
    bool CreateDoor(Room origin, Room target)
    {
        var UIButtonsUsed = origin.Doors.Select(_ => _.UIButton);

        if (UIButtonsUsed.Count() >= 3)
            return false;

        var UIButton = -1;
        while (true)
        {
            UIButton = Random.Range(0, 3);
            if (!UIButtonsUsed.Contains(UIButton)) break;
        }

        origin.Doors.Add(
            new Door(target, UIButton, DoorMaterial)
        );

        return true;
    }
    Room GetRandomDoor(List<Room> rooms)
    {
        var randomNumber = Random.Range(0, rooms.Count());
        return rooms[randomNumber];
    }
    string exportDoorSchema()
    {
        var dictionary = new Dictionary<int, string>();

        dictionary.Add(0, string.Join(", ", _entrance.Doors.Select(_ => $"idx:{_.Room.Index.ToString()}-Door:{_.UIButton}")));

        dictionary.Add(1, string.Join(", ", _firstFloor[0].Doors.Select(_ => $"idx:{_.Room.Index.ToString()}-Door:{_.UIButton}")));
        dictionary.Add(2, string.Join(", ", _firstFloor[1].Doors.Select(_ => $"idx:{_.Room.Index.ToString()}-Door:{_.UIButton}")));
        dictionary.Add(3, string.Join(", ", _firstFloor[2].Doors.Select(_ => $"idx:{_.Room.Index.ToString()}-Door:{_.UIButton}")));

        dictionary.Add(4, string.Join(", ", _secondFloor[0].Doors.Select(_ => $"idx:{_.Room.Index.ToString()}-Door:{_.UIButton}")));
        dictionary.Add(5, string.Join(", ", _secondFloor[1].Doors.Select(_ => $"idx:{_.Room.Index.ToString()}-Door:{_.UIButton}")));
        dictionary.Add(6, string.Join(", ", _secondFloor[2].Doors.Select(_ => $"idx:{_.Room.Index.ToString()}-Door:{_.UIButton}")));
        dictionary.Add(7, string.Join(", ", _secondFloor[3].Doors.Select(_ => $"idx:{_.Room.Index.ToString()}-Door:{_.UIButton}")));

        dictionary.Add(8, string.Join(", ", _thirdFloor[0].Doors.Select(_ => $"idx:{_.Room.Index.ToString()}-Door:{_.UIButton}")));
        dictionary.Add(9, string.Join(", ", _thirdFloor[1].Doors.Select(_ => $"idx:{_.Room.Index.ToString()}-Door:{_.UIButton}")));
        dictionary.Add(10, string.Join(", ", _thirdFloor[2].Doors.Select(_ => $"idx:{_.Room.Index.ToString()}-Door:{_.UIButton}")));
        dictionary.Add(11, string.Join(", ", _thirdFloor[3].Doors.Select(_ => $"idx:{_.Room.Index.ToString()}-Door:{_.UIButton}")));
        dictionary.Add(12, string.Join(", ", _thirdFloor[4].Doors.Select(_ => $"idx:{_.Room.Index.ToString()}-Door:{_.UIButton}")));

        dictionary.Add(13, string.Join(", ", _exit.Doors.Select(_ => $"idx:{_.Room.Index.ToString()}-Door:{_.UIButton}")));

        var strings = dictionary.Select(_ => $"{_.Key} => {_.Value}");
        return string.Join(";", strings);
    }
    #endregion
    #endregion

    #region TextAndTitles
    void GetTextAndTitles()
    {
        var titlesAndDescriptions = new List<RoomDescriptions>();

        titlesAndDescriptions.Add(new RoomDescriptions(RoomType.Entrance, "Portão da Frente", "House", "Você se encontra no portão de uma casa.\nVeio por causa do grande tesouro e em certo momento, você se lembra detalhadamente da voz de seu \"melhor\" amigo:\n\"O tesouro está dentro da Sala Secreta. Mas cuidado!\nAs portas te levam à lugar nenhum e não há porta que vá te trazer de volta.\""));

        titlesAndDescriptions.Add(new RoomDescriptions(RoomType.FirstFloor, "Corredor", "Hallway", "Você encontra um corredor escuro e sem fim!\nO que será que tem no final dele?"));
        titlesAndDescriptions.Add(new RoomDescriptions(RoomType.FirstFloor, "Hall de Entrada", "Entrance", "Como qualquer hall de entrada você logo vê uma poltroninha e uma planta dentro de um vaso no canto esquerdo."));
        titlesAndDescriptions.Add(new RoomDescriptions(RoomType.FirstFloor, "Quintal de Plantas Carnívoras", "Garden", "Um lugar bonito, exceto pelas plantas. Todas elas parecem que vão te engolir caso você dê as costas para elas. Inclusive, parece que uma planta está mais perto."));
        titlesAndDescriptions.Add(new RoomDescriptions(RoomType.SecondFloor, "Cozinha", "Kitchen", "Um cheirinho de bacon recem frito. O que será que cozinharam?\nVocê encontra restos de comidas espalhados pelo chão e colheres sujas de molhos exóticos."));
        titlesAndDescriptions.Add(new RoomDescriptions(RoomType.SecondFloor, "Porão de Bruxa", "Witch_Room", "Alguma bruxa passou por aqui?\n O QUE É ISTO??? Ratos e morcegos?! O que será que fazem aqui?"));
        titlesAndDescriptions.Add(new RoomDescriptions(RoomType.SecondFloor, "Escritório", "Office_Room", "Chás de todos os sabores e comidas vencidas. Muitas delas. Muitas mesmo."));
        titlesAndDescriptions.Add(new RoomDescriptions(RoomType.SecondFloor, "Lavanderia", "Laundry_Room", "Meias sujas e cheiro de cheetos. As roupas parecem ter sido usadas por pessoas bem ecléticas."));
        titlesAndDescriptions.Add(new RoomDescriptions(RoomType.ThirdFloor, "Quarto Obscuro", "Obscure_Room", "Uma mesa de centro com um arranjo de flores bem bonito, uma televisão fora do ar e... doces!!!"));
        titlesAndDescriptions.Add(new RoomDescriptions(RoomType.ThirdFloor, "Banheiro", "Bathroom", "Você vê marcas de musgos na parede e algo amarelo e marrom no chão que você acha melhor não identificar."));
        titlesAndDescriptions.Add(new RoomDescriptions(RoomType.ThirdFloor, "Quarto Principal", "Girl_Room", "Um quarto de menina todo rosa. ECA!!!\nCheio de unicórnios de arco-íris. Ou seriam arco-íris de unicórnios?"));
        titlesAndDescriptions.Add(new RoomDescriptions(RoomType.ThirdFloor, "Quarto de Visitas", "Visitors_Room", "Fotos rasgadas e quadros quebrados. Alguém que entrou estava com muita raiva."));
        titlesAndDescriptions.Add(new RoomDescriptions(RoomType.ThirdFloor, "Sala de TV", "TV_Room", "Oras... uma sala cheia de tvs para se arrumar. O que mais você esperava?!"));

        titlesAndDescriptions.Add(new RoomDescriptions(RoomType.Exit, "Sala Secreta", "Last_Room", "Finalmente encontrou a Sala Secreta!!!\nVocê vai até o canto e acha o tesouro que tanto procurava: dinheiro e jóias raras!!!\nPorém, você se lembra do que seu amigo disse: \"As portas te levam à lugar nenhum e não há porta vai te trazer de volta.\"\n\nE então a frase ecoa na sua mente: \"...e não há porta que vá te trazer de volta...\"."));

        var types = _rooms.Select(_ => _.Type).Distinct();

        foreach (var room in _rooms)
        {
            var type = room.Type;
            var possibleTitlesAndDescriptions = titlesAndDescriptions.Where(_ => _.RoomType == type).ToList();

            var rdn = Random.Range(0, possibleTitlesAndDescriptions.Count);
            var obj = possibleTitlesAndDescriptions[rdn];

            room.Title = obj.Title;
            room.Description = obj.Description;
            room.Name = obj.EntityName;

            titlesAndDescriptions.Remove(obj);
        }
    }
    #endregion

    #region BeginAndEnd
    void CreateBeginAndEnd()
    {
        _rooms[0].Doors.FirstOrDefault().UIButton = 1;


    }
    #endregion
}

public class RoomDescriptions
{
    public RoomType RoomType { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string EntityName { get; set; }

    public RoomDescriptions(RoomType type, string title, string entityName, string description)
    {
        RoomType = type;
        Title = title;
        Description = description;
        EntityName = entityName;
    }
}
public class Room
{
    public int Index;
    public string Title;
    public string Description;
    public string Name;
    public RoomType Type;
    public List<Door> Doors;
    public bool IsMainRoom;

    public Room() { }
    public Room(int idx, RoomType type)
    {
        Doors = new List<Door>();
        Index = idx;
        Type = type;
    }
}
public class Door
{
    public Room Room;
    public int UIButton;
    public string DoorType;
    public Sprite Sprite { get; set; }

    public Door() { }
    public Door(Room room, int uiButton, List<Sprite> materials)
    {
        Room = room;
        UIButton = uiButton;

        var rdn = Random.Range(0, _doorTypes.Count);
        DoorType = _doorTypes[rdn];
        Sprite = materials[rdn];
    }

    private List<string> _doorTypes = new List<string>() { "Porta de Madeira", "Porta de Metal", "Porta de Vidro", "Porta Sanfonada", "Porta Bang-Bang" };
}
public enum RoomType { Entrance, FirstFloor, SecondFloor, ThirdFloor, Exit }
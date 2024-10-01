using System.Runtime.Serialization;
using SnakeGame;

namespace SnakeState;

public class SnakeState
{
    [DataMember]
    public int snake;

    [DataMember]
    public string name;

    [DataMember]
    public List<Vector2D> body;

    [DataMember]
    public Vector2D dir;

    [DataMember]
    public int score;

    [DataMember]
    public bool died;

    [DataMember]
    public bool alive;

    [DataMember]
    public bool dc;

    [DataMember]
    public bool join;
}


using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public enum Type { Low, High }   // Low = jump over, High = slide under
    public Type type;
}

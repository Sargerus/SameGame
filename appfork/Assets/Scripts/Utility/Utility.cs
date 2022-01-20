using UnityEngine;

public delegate void Notify();

public enum FallDirection
{
    Bottom = 0
}

public enum ShiftDirection
{
    Left = -1,
    Right = 1,
}

public static class Utility
{
    public static Vector3 Multiply(this Vector3 vector1, Vector3 vector2) => new Vector3(vector1.x * vector2.x, vector1.y * vector2.y, vector1.z * vector2.z);
}

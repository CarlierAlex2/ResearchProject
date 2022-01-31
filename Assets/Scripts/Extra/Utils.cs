using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary> 
/// Utility code. Based on the implementation of Code Monkey for grid generation (see Grid.cs).
/// Link: https://unitycodemonkey.com/video.php?v=waEsGu--9P8
/// </summary>
public class Utils
{
    public static TextMesh CreateWorldText(string text, Transform parent=null, Vector3 localPosition = default(Vector3), int fontSize=40, Color color=default(Color), TextAnchor textAnchor=TextAnchor.MiddleCenter, TextAlignment textAlignment=TextAlignment.Center, int sortingOrder = 0)
    {
        if (color == null)
            color = Color.white;
        return CreateWorldText(parent, text, localPosition, fontSize, color, textAnchor, textAlignment, sortingOrder);
    }

    public static TextMesh CreateWorldText(Transform parent, string text, Vector3 localPosition, 
                                            int fontSize, Color color, TextAnchor textAnchor, TextAlignment textAlignment, int sortingOrder)
    {
        GameObject obj = new GameObject("text", typeof(TextMesh));
        obj.layer = 8;
        Transform transform = obj.transform;
        transform.SetParent(parent, false);
        transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        transform.localPosition = localPosition;
        transform.Rotate(90f, 0f, 0f);

        TextMesh textMesh = obj.GetComponent<TextMesh>();
        textMesh.anchor = textAnchor;
        textMesh.alignment = textAlignment;
        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.color = color;
        textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
        return textMesh;
    }

    //has issues with perspective
    public static Vector3 GetMouseWorldPosition(){
        Vector3 vec = GetWorldPositionFromScreen(Input.mousePosition, Camera.main);
        vec.z = 0f;
        return vec;
    }

    public static Vector3 GetMouseWorldPositionWithZ(){
        return GetMouseWorldPositionWithZ(Camera.main);
    }

    public static Vector3 GetMouseWorldPositionWithZ(Camera cam){
        return GetWorldPositionFromScreen(Input.mousePosition, cam);
    }

    public static Vector3 GetWorldPositionFromScreen(Vector3 screenPosition, Camera cam){
        return cam.ScreenToWorldPoint(screenPosition);
    }
}

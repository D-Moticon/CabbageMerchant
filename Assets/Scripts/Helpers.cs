using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class Helpers
{
    public static Vector2 RotateVector2(this Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }

    public static float Vector2ToAngle(Vector2 vector2)
    {
        float result;

        if (vector2.x < 0)
        {
            result = 360 - (Mathf.Atan2(vector2.y, vector2.x) * Mathf.Rad2Deg * -1);
        }
        else
        {
            result = Mathf.Atan2(vector2.y, vector2.x) * Mathf.Rad2Deg;
        }


        if (result > 360)
        {
            result -= 360;
        }

        return result;
    }

    public static Quaternion Vector2ToRotation(Vector2 vector2)
    {
        float angle = 0;

        if (vector2.x < 0)
        {
            angle = 360 - (Mathf.Atan2(vector2.y, vector2.x) * Mathf.Rad2Deg * -1);
        }
        else
        {
            angle = Mathf.Atan2(vector2.y, vector2.x) * Mathf.Rad2Deg;
        }

        return Quaternion.Euler(0, 0, angle);
    }

    public static Vector2 AngleDegToVector2(float angle)
    {
        float angleRad = Mathf.Deg2Rad * angle;
        float x = Mathf.Cos(angleRad);
        float y = Mathf.Sin(angleRad);
        Vector2 vec = new Vector2(x, y);
        return vec;
    }

    public static Quaternion AngleDegToRotation(float angle)
    {
        return Quaternion.Euler(0, 0, angle);
    }

    public static Vector2 RotationToVector2(Quaternion rotation)
    {
        Vector2 dir = Helpers.AngleDegToVector2(rotation.eulerAngles.z);
        return dir;
    }

    public static float RoundToDecimal(float value, int decimalPlaces)
    {
        float multiplier = Mathf.Pow(10, decimalPlaces);
        return Mathf.Round(value * multiplier) / multiplier;
    }
    
    public static string FormatWithSuffix(double value)
    {
        // Preserve negative sign if needed
        bool isNegative = value < 0;
        double absValue =  System.Math.Abs(value);

        string suffix;
        double displayValue;

        if (absValue >= 1e9)
        {
            suffix = "B";
            displayValue = absValue / 1e9;
        }
        else if (absValue >= 1e6)
        {
            suffix = "M";
            displayValue = absValue / 1e6;
        }
        else if (absValue >= 1e3)
        {
            suffix = "k";
            displayValue = absValue / 1e3;
        }
        else
        {
            suffix = "";
            displayValue = System.Math.Round(absValue);
        }

        // Format to up to 3 decimals, trimming trailing zeros automatically
        string formatted = displayValue.ToString("0.###");

        // Reapply negative sign if required
        if (isNegative)
            formatted = "-" + formatted;

        return formatted + suffix;
    }
    
    public static string ToPercentageString(float value)
    {
        int percentage = Mathf.RoundToInt(value * 100f);
        return percentage.ToString() + "%";
    }
    
    public static float RemapClamped(this float aValue, float aIn1, float aIn2, float aOut1, float aOut2)
    {
        float t = (aValue - aIn1) / (aIn2 - aIn1);
        if (t > 1f)
            return aOut2;
        if (t < 0f)
            return aOut1;
        return aOut1 + (aOut2 - aOut1) * t;
    }

    public static List<T> GetUniqueRandomEntries<T>(List<T> inputList, int count)
    {
        if (inputList == null)
        {
            Debug.LogWarning("Input list is null.");
            return new List<T>();
        }

        if (count <= 0 || inputList.Count == 0)
        {
            return new List<T>();
        }

        if (count >= inputList.Count)
        {
            // Shuffle and return all elements
            return inputList.OrderBy(x => Random.value).ToList();
        }

        // Shuffle and take only the desired amount
        return inputList.OrderBy(x => Random.value).Take(count).ToList();
    }

    public static T DeepClone<T>(T src) where T : class
    {
        var json = JsonUtility.ToJson(src);
        // this overload uses the concrete runtime type of src
        return JsonUtility.FromJson(json, src.GetType()) as T;
    }
    
    public static void Shuffle<T>(this IList<T> list)
    {
        System.Random rng = new System.Random();

        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static void Shuffle<T>(T[] array)
    {
        System.Random rng = new System.Random();
        int n = array.Length;
        while (n > 1)
        {
            int k = rng.Next(n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }

}

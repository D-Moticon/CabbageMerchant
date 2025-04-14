using UnityEngine;

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
}

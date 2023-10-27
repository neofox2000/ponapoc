using UnityEngine;

public static class ColorTypeConverter
{
    public static string ToRGBHex(Color c)
    {
        return string.Format("#{0:X2}{1:X2}{2:X2}", ToByte(c.r), ToByte(c.g), ToByte(c.b));
    }
    public static string ToRGBAHex(Color c)
    {
        return string.Concat(
            ToRGBHex(c),
            string.Format("{0:X2}", ToByte(c.a)));
    }

    private static byte ToByte(float f)
    {
        f = Mathf.Clamp01(f);
        return (byte)(f * 255);
    }
}
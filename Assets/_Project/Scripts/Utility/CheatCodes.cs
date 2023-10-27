public class CheatCodes
{
    public static string[] codes = new string[]{
        "stats",
        "skills",
        "ammo",
        "money",
        "alicorn"
    };

    public static int getCodeID(string code)
    {
        int ret = -1;
        for (int i = 0; i < codes.Length; i++)
            if (code == codes[i])
            {
                ret = i;
                break;
            }

        return ret;
    }
}

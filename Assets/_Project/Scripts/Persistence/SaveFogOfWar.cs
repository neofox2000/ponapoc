using System;
using System.Collections;

public class SaveFogOfWar
{
    public const int width = 500;
    public const int height = 500;

    //public FogOfWar.FOWState[] fowState;
    public byte[] fowState;

    public SaveFogOfWar()
    {
        //fowState = new FogOfWar.FOWState[width * height];
        fowState = new byte[width * height];
    }
    public SaveFogOfWar(FogOfWar.FOWState[,] fowState)
    {
        this.fowState = pack(fowState);
    }
    public byte[] pack(FogOfWar.FOWState[,] unpackedState)
    {
        byte[] packedArray = new byte[width * height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                packedArray[x + y * width] = (byte)unpackedState[x, y];

        return packedArray;
    }
    public FogOfWar.FOWState[,] unpack()
    {
        FogOfWar.FOWState[,] ret = new FogOfWar.FOWState[width, height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                ret[x, y] = (FogOfWar.FOWState)fowState[x + y * width];

        return ret;
    }
}


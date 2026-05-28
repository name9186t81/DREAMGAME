public static class MiscUtils
{
    public static bool IsSelect(this PlayerInputReader.InputType type)
    {
        byte num = (byte)type;
        return num > 1 && num < 6;
    }
}
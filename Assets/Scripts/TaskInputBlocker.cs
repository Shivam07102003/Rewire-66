public static class TaskInputBlocker
{
    public static bool IsBlocked { get; private set; }

    public static void Block()
    {
        IsBlocked = true;
    }

    public static void Unblock()
    {
        IsBlocked = false;
    }
}
namespace AMO.Core.Infrastructure
{
    /// <summary>
    /// Available message receive thread options
    /// </summary>
    public enum MessageThread
    {
        AsInvoker,
        Synchronized,
        NewThread
    }
}

public abstract class AbstractConnectionToWebSocketClient {

    public delegate void MessageReceivedAsText(string message);
    public delegate void MessageReceivedAsByte(byte[] message);
    public delegate void ServerConnectChangedEvent(bool connectionSwitch);
    public abstract void PushMessageToServer(string messageAsText);
    public abstract void PushMessageToServer(byte messageAsByte);

    public abstract void AddListenerByteListener(MessageReceivedAsText listener);
    public abstract void AddListenerTextListener(MessageReceivedAsByte listener);
    public abstract void AddConnectEvent(ServerConnectChangedEvent listener);

}



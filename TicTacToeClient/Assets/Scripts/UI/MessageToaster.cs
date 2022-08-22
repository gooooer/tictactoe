using System;
using System.Collections.Generic;
using TMPro;

// This class's purpose is to display short-lived messages to inform user about application state changes.
// Each message is enqueued to the internal queue.Every message is displayed for 3 seconds.
// One message at a time and in the order in which they were enqueued.
public class MessageToaster
{
    private TMP_Text _label;
    private Queue<string> _queuedMessages;
    private DateTime _lastMessageShowedTimestamp;

    public MessageToaster(TMP_Text label)
	{
        _label = label;
        _queuedMessages = new Queue<string>();
        _lastMessageShowedTimestamp = DateTime.MinValue;
    }

    public void ShowText(string text)
    {
        _queuedMessages.Enqueue(text);
    }

    public void Update()
    {
        TimeSpan ts = DateTime.Now - _lastMessageShowedTimestamp;
        if (ts.Seconds > 3)
        {
            if (_queuedMessages.Count == 0)
            {
                _label.text = "";
            }
            else
            {
                _label.text = _queuedMessages.Dequeue();
                _lastMessageShowedTimestamp = DateTime.Now;
            }
        }
    }
}

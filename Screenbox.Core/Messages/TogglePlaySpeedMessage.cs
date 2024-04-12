namespace Screenbox.Core.Messages;

public class TogglePlaySpeedMessage
{
    public bool SpeedUp { get; }

    public TogglePlaySpeedMessage(bool speedUp)
    {
        SpeedUp = speedUp;
    }
}
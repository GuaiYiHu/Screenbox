namespace Screenbox.Core.Messages;

public class SlideMessage
{
    public bool Slide { get; }

    public SlideMessage(bool slide)
    {
        Slide = slide;
    }
}
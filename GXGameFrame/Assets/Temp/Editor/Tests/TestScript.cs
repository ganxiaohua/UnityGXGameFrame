using GameFrame;
using NUnit.Framework;

public class TestScript
{
    [Test]
    public void Bytes()
    {
        ByteSequenceSegmentPipe pipe = new ByteSequenceSegmentPipe();
        pipe.Write(new byte[] {1,2,3,4,5,6,7,8,9,10}, 0, 10);
        var x = pipe.CreateSequence();
    }
}
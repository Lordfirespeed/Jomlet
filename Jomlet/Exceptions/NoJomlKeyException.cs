namespace Jomlet.Exceptions;

public class NoJomlKeyException : JomlExceptionWithLine
{
    public NoJomlKeyException(int lineNumber) : base(lineNumber) { }

    public override string Message => $"Expected a JOML key on line {LineNumber}, but found an equals sign ('=').";
}
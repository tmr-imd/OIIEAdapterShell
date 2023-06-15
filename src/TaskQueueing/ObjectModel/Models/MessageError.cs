namespace TaskQueueing.ObjectModel.Models;

public enum ErrorSeverity
{
    Warning = 5,
    Error = 10
}

public record class MessageError(ErrorSeverity Severity, string Message = "", int LineNumber = 0, int LinePosition = 0);

using CommonBOD;
using Oagis;
using TaskQueueing.ObjectModel.Enums;
using TaskQueueing.ObjectModel.Models;

namespace AdapterServer.Extensions;

public static class MessageErrorExtensions
{
    public static MessageError ToMessageError(this ValidationResult self)
    {
        var severity = self.Severity switch
        {
            System.Xml.Schema.XmlSeverityType.Error => ErrorSeverity.Error,
            System.Xml.Schema.XmlSeverityType.Warning => ErrorSeverity.Warning,
            _ => ErrorSeverity.Error
        };
        var error = new MessageError(severity, self.Message, self.LineNumber, self.LinePosition);
        return error;
    }

    public static MessageType ToOagisMessage(this MessageError self)
    {
        return new MessageType
        {
            Description = new DescriptionType[]
            {
                new DescriptionType()
                {
                    languageID = "en-US",
                    Value = $"{self.Message} at Line: {self.LineNumber} Position: {self.LinePosition}"
                }
            }
        };
    }

    public static MessageError ToMessageError(this MessageType self, ErrorSeverity severity = ErrorSeverity.Error)
    {
        return new MessageError(severity, self.Description.First().Value);
    }
}
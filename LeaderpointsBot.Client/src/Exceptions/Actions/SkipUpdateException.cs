namespace LeaderpointsBot.Client.Exceptions.Actions;

public class SkipUpdateException : ClientException
{
    public SkipUpdateException() : base("Process asked to skip data update. Sending only points data instead.") { }
}

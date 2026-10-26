namespace SEMPT3X_SMTP_CHECKER.Models;

public sealed record ErrorDefinition(string Code, string Name, string Cause, string Solution, string Category);

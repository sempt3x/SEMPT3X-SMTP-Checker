namespace SBH_SMTP_Checker.Models;

public sealed record ErrorDefinition(string Code, string Name, string Cause, string Solution, string Category);

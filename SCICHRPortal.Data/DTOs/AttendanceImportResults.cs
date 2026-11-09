namespace SCICHRPortal.Data.DTOs;

public sealed record BiometricsImportResult(int TotalFetched, int ProcessedCount, int SkippedCount,
    int DuplicateCount, int Inserted, int Failed, IReadOnlyList<string> Errors, long ImportTimeMs);

public sealed record TimeLogImportResult(int Imported, int Skipped, IReadOnlyList<string> Errors, bool ProjectUnavailable = false);

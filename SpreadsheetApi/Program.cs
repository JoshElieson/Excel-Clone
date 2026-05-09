using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using SpreadsheetUtilities;
using SS;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddSingleton<SessionStore>();

var corsOrigins = new List<string> { "http://localhost:5173", "http://127.0.0.1:5173" };
var extraOrigins = builder.Configuration["CORS_ORIGINS"];
if (!string.IsNullOrWhiteSpace(extraOrigins))
{
    foreach (var o in extraOrigins.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        corsOrigins.Add(o);
}

builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(corsOrigins.ToArray())
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

app.UseCors();

app.MapPost("/api/sessions", (SessionStore store) =>
{
    var id = store.Create();
    return Results.Ok(new SessionResponse(id));
});

app.MapGet("/api/sessions/{id:guid}/snapshot", (Guid id, SessionStore store) =>
{
    if (!store.TryGet(id, out var sheet))
        return Results.NotFound();
    return Results.Ok(SnapshotBuilder.Build(sheet));
});

app.MapPost("/api/sessions/{id:guid}/reset", (Guid id, SessionStore store) =>
{
    if (!store.TryReset(id))
        return Results.NotFound();
    if (!store.TryGet(id, out var sheet))
        return Results.NotFound();
    return Results.Ok(SnapshotBuilder.Build(sheet));
});

app.MapPost("/api/sessions/{id:guid}/cells/{cellName}/commit", (Guid id, string cellName, CommitBody body, SessionStore store) =>
{
    if (!store.TryGet(id, out var sheet))
        return Results.NotFound();

    var content = body.Content ?? "";

    if (content.Length > 0 && content[0] == '=')
        content = content.ToUpperInvariant();

    try
    {
        sheet.SetContentsOfCell(cellName, content);
    }
    catch (CircularException)
    {
        return Results.BadRequest(new ErrorBody("Circular dependency detected."));
    }
    catch (InvalidNameException)
    {
        return Results.BadRequest(new ErrorBody("Invalid cell name."));
    }
    catch (FormulaFormatException ex)
    {
        return Results.BadRequest(new ErrorBody(ex.Message));
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new ErrorBody(ex.Message));
    }

    return Results.Ok(SnapshotBuilder.Build(sheet));
});

app.MapPost("/api/sessions/{id:guid}/load", async (Guid id, HttpRequest request, SessionStore store) =>
{
    if (!store.ContainsSession(id))
        return Results.NotFound();

    using var reader = new StreamReader(request.Body, Encoding.UTF8);
    var xml = await reader.ReadToEndAsync();
    if (string.IsNullOrWhiteSpace(xml))
        return Results.BadRequest(new ErrorBody("Empty file."));

    var tmp = Path.Combine(Path.GetTempPath(), $"ss-load-{id}-{Guid.NewGuid():N}.xml");
    try
    {
        await File.WriteAllTextAsync(tmp, xml, Encoding.UTF8);
        var sheet = new Spreadsheet(tmp, SessionStore.IsValidName, s => s.ToUpper(), "six");
        store.Replace(id, sheet);
    }
    catch (SpreadsheetReadWriteException ex)
    {
        return Results.BadRequest(new ErrorBody(ex.Message));
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new ErrorBody(ex.Message));
    }
    finally
    {
        try { File.Delete(tmp); } catch { /* ignore */ }
    }

    if (!store.TryGet(id, out var loaded))
        return Results.NotFound();
    return Results.Ok(SnapshotBuilder.Build(loaded));
});

app.MapGet("/api/sessions/{id:guid}/save", (Guid id, SessionStore store) =>
{
    if (!store.TryGet(id, out var sheet))
        return Results.NotFound();

    var xml = sheet.GetXML();
    return Results.Bytes(Encoding.UTF8.GetBytes(xml), "application/xml", "spreadsheet.sprd");
});

app.Run();

internal sealed class SessionStore
{
    private readonly ConcurrentDictionary<Guid, Spreadsheet> _sessions = new();

    private static readonly Regex NameRegex = new(@"^[a-zA-Z]+[0-9]+$", RegexOptions.Compiled);

    public static bool IsValidName(string name) => NameRegex.IsMatch(name);

    public Guid Create()
    {
        var id = Guid.NewGuid();
        _sessions[id] = new Spreadsheet();
        return id;
    }

    public bool ContainsSession(Guid id) => _sessions.ContainsKey(id);

    public bool TryGet(Guid id, out Spreadsheet sheet) => _sessions.TryGetValue(id, out sheet!);

    public bool TryReset(Guid id)
    {
        if (!_sessions.ContainsKey(id))
            return false;
        _sessions[id] = new Spreadsheet();
        return true;
    }

    public void Replace(Guid id, Spreadsheet sheet) => _sessions[id] = sheet;
}

internal static class SnapshotBuilder
{
    public static SnapshotResponse Build(Spreadsheet sheet)
    {
        var cells = new Dictionary<string, CellPayload>();
        foreach (var name in sheet.GetNamesOfAllNonemptyCells())
        {
            cells[name] = new CellPayload(
                FormatContent(sheet.GetCellContents(name)),
                FormatValue(sheet.GetCellValue(name)));
        }

        return new SnapshotResponse(sheet.Changed, cells);
    }

    private static string FormatContent(object contents) => contents switch
    {
        double d => d.ToString(CultureInfo.InvariantCulture),
        string s => s,
        Formula f => "=" + f,
        _ => contents?.ToString() ?? ""
    };

    private static string FormatValue(object valueObj) => valueObj switch
    {
        double d => d.ToString(CultureInfo.InvariantCulture),
        string s => s,
        FormulaError fe => fe.Reason,
        _ => valueObj?.ToString() ?? ""
    };
}

internal record SessionResponse(Guid SessionId);

internal record CommitBody(string? Content);

internal record ErrorBody(string Message);

internal record CellPayload(string Contents, string Value);

internal record SnapshotResponse(bool Changed, Dictionary<string, CellPayload> Cells);

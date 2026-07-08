[HttpPost("import")]
[RequestSizeLimit(50_000_000)] // 50 Mo
public async Task<IActionResult> Import(
    IFormFile file,
    [FromQuery] bool replaceExisting = false)
{
    if (file is null || file.Length == 0)
        return BadRequest("Fichier vide.");

    using var stream = file.OpenReadStream();

    var count = await _mediator.Send(new ImportSegmentationDistributiveCommand
    {
        CsvStream = stream,
        ReplaceExisting = replaceExisting
    });

    return Ok(new { imported = count });
}

[HttpPost]
public async Task<ActionResult<bool>> Save(
    [FromBody] SaveDimensionnementPortefeuilleEtpData request,
    CancellationToken cancellationToken)
{
    var command = new SaveDimensionnementPortefeuilleEtpCommand
    {
        Data = request
    };

    var result = await _mediator.Send(
        command,
        cancellationToken);

    return Ok(result);
}

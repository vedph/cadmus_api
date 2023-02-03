namespace Cadmus.Api.Models.Preview;

/// <summary>
/// Rendition result model.
/// </summary>
public class RenditionModel
{
    /// <summary>
    /// Gets the result string.
    /// </summary>
    public string Result { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RenditionModel"/> class.
    /// </summary>
    /// <param name="result">The result.</param>
    public RenditionModel(string result)
    {
        Result = result;
    }
}

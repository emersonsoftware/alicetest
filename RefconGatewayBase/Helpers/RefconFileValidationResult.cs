using System.Collections.Generic;

namespace RefconGatewayBase.Helpers;

/// <summary>
/// Response object for ValidateRefconfile Activity Trigger
/// </summary>
public class RefconFileValidationResult
{
    /// <summary>
    /// True if the REFCON file passed validation, False if failed
    /// </summary>
    public bool Result { get; set; }

    /// <summary>
    /// Reasons for failure
    /// </summary>
    public List<string> Messages { get; set; }
}
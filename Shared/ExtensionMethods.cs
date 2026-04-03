using Shared.Interfaces.ReturnResults;

namespace Shared;

public static class ExtensionMethods
{
    public static void VerifyOperation(this IOperationResult result) {
        if(!result.HasSuccessStatus) {
            throw new OperationFailedException(result.Message);
        }
    }
}
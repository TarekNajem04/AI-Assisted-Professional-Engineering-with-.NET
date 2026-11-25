// book/samples/Chapter-03/EngineeringCommunicationToolkit/src/AiToolkit.Application/Ports/IAiClientFactory.cs
//
// S03/S05: one application-owned seam for the two generation modes.
// Free-form output supports specification-anchored dialogue (S04);
// schema-constrained output supports the review workflow (S05).
// Consumers depend on this interface, never on an SDK client type.

namespace AiToolkit.Application.Ports;

public interface IAiClientFactory
{
    IChatCompletionClient CreateFreeForm();
    IChatCompletionClient CreateStructured();
}

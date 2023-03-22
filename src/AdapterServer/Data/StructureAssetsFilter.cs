namespace AdapterServer.Data;

public record class StructureAssetsFilter
(
    string FilterCode,
    string FilterType,
    string FilterLocation,
    string FilterOwner,
    string FilterCondition,
    string FilterInspector
);
